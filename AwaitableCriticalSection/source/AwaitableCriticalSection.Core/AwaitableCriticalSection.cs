//-----------------------------------------------------------------------
// <copyright file="AwaitableCriticalSection.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AwaitableCriticalSection
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AwaitableCriticalSection
    {
        private readonly Queue<AcquireRequest> acquireRequests;

        private AcquireRequest comingRequest;

        private AcquireRequest processingRequest;

        public AwaitableCriticalSection()
        {
            this.acquireRequests = new Queue<AcquireRequest>();
        }

        public Task<Token> AcquireAsync()
        {
            this.comingRequest = new AcquireRequest();
            Task<Token> resultTask = this.comingRequest.AcquireTask;
            lock (this.acquireRequests)
            {
                if (this.processingRequest == null)
                {
                    this.processingRequest = this.comingRequest;
                    this.comingRequest.GrantRequestAndRunContinuation();
                }
                else
                {
                    this.acquireRequests.Enqueue(this.comingRequest);
                }
            }

            return resultTask;
        }

        public void Release(Token token)
        {
            if (this.processingRequest != token)
            {
                throw new InvalidOperationException("Invalid owner token");
            }

            lock (this.acquireRequests)
            {
                // need to clean it before grant the next request!! 
                this.processingRequest = null; 

                if (this.acquireRequests.Count > 0)
                {
                    this.processingRequest = this.acquireRequests.Dequeue();
                }
            }

            if (this.processingRequest != null)
            {
                // to prevent deadlocks, you should generally avoid running arbitrary user code such as event handlers or in this case, continuations, under a lock
                this.processingRequest.GrantRequestAndRunContinuation();
            }
        }

        /*
         * My lock has one bonus feature: it returns a Token that can be used to prevent someone 
         * who does not own the lock from releasing it. Typically, “It is the programmer’s responsibility 
         * to ensure that threads do not release the semaphore too many times.” 
         * The Token is a design fix to prevent this problem from ever occurring in any reasonable situation 
         * — at the expense of one additional object allocation.
         */
        public abstract class Token
        {
            protected Token()
            {
            }
        }

        private sealed class AcquireRequest : Token
        {
            private readonly TaskCompletionSource<Token> tcs;

            public AcquireRequest()
            {
                this.tcs = new TaskCompletionSource<Token>();
            }

            public Task<Token> AcquireTask
            {
                get
                {
                    return this.tcs.Task;
                }
            }

            public void GrantRequestAndRunContinuation()
            {
                this.tcs.SetResult(this);
            }
        }
    }
}