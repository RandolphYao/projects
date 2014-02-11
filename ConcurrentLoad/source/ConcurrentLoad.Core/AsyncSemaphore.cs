namespace ConcurrentLoad
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AsyncSemaphore
    {
        private static readonly Task CompletedTask = Task.FromResult(true);

        private readonly Queue<TaskCompletionSource<bool>> waiters;

        private int currentCount;

        public AsyncSemaphore(int maxCount)
        {
            if (maxCount < 0)
            {
                throw new ArgumentOutOfRangeException("maxCount");
            }

            this.currentCount = maxCount;
            this.waiters = new Queue<TaskCompletionSource<bool>>();
        }

        public Task WaitAsync()
        {
            lock (this.waiters)
            {
                // for the initial maxCount tasks, grant execution by setting task result
                if (this.currentCount > 0)
                {
                    --this.currentCount;
                    return CompletedTask;
                }
                else
                {
                    TaskCompletionSource<bool> waiter = new TaskCompletionSource<bool>();
                    this.waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (this.waiters)
            {
                if (this.waiters.Count > 0)
                {
                    toRelease = this.waiters.Dequeue();
                }
                else
                {
                    ++this.currentCount;
                }
            }

            if (toRelease != null)
            {
                toRelease.SetResult(true);
            }
        }
    }
}