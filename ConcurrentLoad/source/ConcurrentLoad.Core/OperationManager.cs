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

        public Task RunAsync(int totalCalls)
        {
            AsyncSemaphore asyncSemaphore = new AsyncSemaphore(this.maxPendingCalls);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < totalCalls; i++)
            {
                tasks.Add(this.RunAsyncInner(asyncSemaphore));
            }

            return Task.WhenAll(tasks);
        }

        private async Task RunAsyncInner(AsyncSemaphore asyncSemaphore)
        {
            await asyncSemaphore.WaitAsync();
            try
            {
                await this.doAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                asyncSemaphore.Release();
            }          
        }
    }
}