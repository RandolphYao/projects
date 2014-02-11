//-----------------------------------------------------------------------
// <copyright file="OperationManager.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ConcurrentLoad
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class OperationManager
    {
        private readonly Func<Task> doAsync;

        private readonly int maxPendingCalls;

        public OperationManager(int maxPendingCalls, Func<Task> doAsync)
        {
            this.doAsync = doAsync;
            this.maxPendingCalls = maxPendingCalls;
        }

        public async Task RunAsync(int totalCalls)
        {
            AsyncSemaphore asyncSemaphore = new AsyncSemaphore(this.maxPendingCalls);
 
            List<Exception> exceptions = new List<Exception>();
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < totalCalls; i++)
            {
                // need this to ensure the code below await only execute after entering critical section
                await asyncSemaphore.WaitAsync();
                Task newTask = this.RunAsyncInner(asyncSemaphore); 
                tasks.Add(newTask);
                this.DetectExceptionsFromPendingTasks(tasks, exceptions);
                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions).Flatten();
                }
            }

            await Task.WhenAll(tasks);
        }

        private void DetectExceptionsFromPendingTasks(List<Task> tasks, List<Exception> exceptions)
        {
            foreach (var task in tasks)
            {
                if (task.IsCompleted)
                {
                    if (task.Exception != null)
                    {
                        exceptions.Add(task.Exception);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private async Task RunAsyncInner(AsyncSemaphore asyncSemaphore)
        {
            try
            {
                await this.doAsync();
            }
            finally
            {
                asyncSemaphore.Release();
            }          
        }
    }
}