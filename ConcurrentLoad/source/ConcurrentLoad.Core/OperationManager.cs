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
            int remainingCalls = totalCalls;

            while (remainingCalls > 0)
            {
                int calls = Math.Min(this.maxPendingCalls, remainingCalls);
                var tasks = new List<Task>();
                for (int i = 0; i < calls; i++)
                {
                    tasks.Add(this.doAsync());
                    remainingCalls--;
                }

                await Task.WhenAll(tasks); 
            }
        }
    }
}