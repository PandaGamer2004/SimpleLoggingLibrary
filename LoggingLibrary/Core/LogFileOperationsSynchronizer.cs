using System.Collections.Concurrent;

namespace LoggingLibrary;



//Almost all side effects could be implemented via closure
//Should be singleton
//Could be optimized buy determination which operation type and removing unnecessary flushes
public class LogFileOperationsSynchronizer : IFileOperationsSynchronizer
{
    private readonly Func<FileStream> fileStreamFactory;

    private record IoOperationScheduleGroup(
        Func<FileStream, CancellationToken, TaskCompletionSource, Task> AsyncIoOperation,
        TaskCompletionSource NotificationSource
    );

    private int synchronizationFlag = 0;

    //Used to avoid 
    private ConcurrentQueue<IoOperationScheduleGroup> asyncFileOperationsQueue = new();

    public LogFileOperationsSynchronizer(Func<FileStream> fileStreamFactory)
    {
        this.fileStreamFactory = fileStreamFactory;
    }

    public Task ScheduleAsyncFileOperation(Func<FileStream, CancellationToken, Task> asyncOperation,
        CancellationToken ct)
    {
        if (Interlocked.CompareExchange(ref synchronizationFlag, 1, 0) == 1)
        {
            return ScheduleDeferredOperation(asyncOperation);
        }

        return RunDeferredOperations(asyncOperation, ct);
    }

    private async Task RunDeferredOperations(Func<FileStream, CancellationToken, Task> asyncOperation,
        CancellationToken ct)
    {
        //If we get here means that all previous operations already scheduled
        //Making snapshot of queue size on time when operations scheduled
        Func<Func<FileStream, CancellationToken, Task>, Task> AsyncMutationFactory(FileStream fs) =>
            async operation =>
            {
                await operation(fs, ct);
                await fs.FlushAsync(ct);
            };

        int queueSize = this.asyncFileOperationsQueue.Count;
        var runAsyncIOOperation = AsyncMutationFactory(fileStreamFactory());
        while (queueSize-- != 0)
        {
            await runAsyncIOOperation(DeferredAsyncOperationIteration);
        }

        await runAsyncIOOperation(asyncOperation);
    }

    private Task ScheduleDeferredOperation(Func<FileStream, CancellationToken, Task> asyncOperation)
    {
        var defferedIoOperationCompetionSource = new TaskCompletionSource();
        var asyncOperationDecorator
            = (FileStream stream, CancellationToken ct, TaskCompletionSource defferedIOOperationTaskSource) =>
            {
                Task asyncIOOperationTask = asyncOperation(stream, ct);
                RegisterContinuations(asyncIOOperationTask, defferedIOOperationTaskSource, ct);
                return asyncIOOperationTask;
            };

        var group = new IoOperationScheduleGroup(asyncOperationDecorator, defferedIoOperationCompetionSource);
        this.asyncFileOperationsQueue.Enqueue(group);
        return defferedIoOperationCompetionSource.Task;
    }

    private Task DeferredAsyncOperationIteration(FileStream sharedFileStream, CancellationToken ct)
    {
        bool isSucceedWrite = this.asyncFileOperationsQueue.TryDequeue(out var deferredAsyncOperation);
        if (!isSucceedWrite)
        {
            throw new DeferredSynchronizationException(
                "For unknown reason read from deferred IO operations wasn't suc ");
        }

        var (asyncIoOperation, taskCompletionSource) = deferredAsyncOperation!;
        Task resultTask = asyncIoOperation(sharedFileStream, ct, taskCompletionSource);
        return resultTask;
    }

    private void RegisterContinuations(Task asyncIoOperationTask,
        TaskCompletionSource deferredIoOperationCompletionSource,
        CancellationToken ct)
    {
        asyncIoOperationTask.ContinueWith(t => { deferredIoOperationCompletionSource.SetException(t.Exception!); },
            TaskContinuationOptions.OnlyOnFaulted);
        asyncIoOperationTask.ContinueWith(_ => { deferredIoOperationCompletionSource.SetResult(); },
            TaskContinuationOptions.OnlyOnRanToCompletion);
        asyncIoOperationTask.ContinueWith(_ => { deferredIoOperationCompletionSource.SetCanceled(ct); },
            TaskContinuationOptions.OnlyOnCanceled);
    }
}