//-----------------------------------------------------------------------
// <copyright file="AwaitableCriticalSectionTest.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AwaitableCriticalSection.Test.Unit
{
    using System.Threading.Tasks;
    using Xunit;

    public class AwaitableCriticalSectionTest
    {
        public AwaitableCriticalSectionTest()
        {
        }

        [Fact]
        public void Acquire_completes_sync_then_release_succeeds()
        {
            AwaitableCriticalSection acs = new AwaitableCriticalSection();
            var token = AssertTaskCompleted(acs.AcquireAsync());
            acs.Release(token);
        }

        [Fact]
        public void First_acquire_completes_sync_next_acquire_is_pending_until_first_release()
        {
            AwaitableCriticalSection l = new AwaitableCriticalSection();
            AwaitableCriticalSection.Token token = AssertTaskCompleted(l.AcquireAsync());

            Task<AwaitableCriticalSection.Token> nextAcquireTask = AssertTaskPending(l.AcquireAsync());
            l.Release(token);

            AssertTaskCompleted(nextAcquireTask);
        }

        private static Task<TResult> AssertTaskPending<TResult>(Task<TResult> task)
        {
            Assert.False(task.IsCompleted, "Task should not be completed.");
            Assert.False(task.IsFaulted, "Task should not be faulted: " + task.Exception);
            return task;
        }

        private static void AssertTaskCompleted<TResult>(TResult expected, Task<TResult> task)
        {
            // IsCompleted means that the Task has finished executing, either successfully, exceptionally, or due to cancellation.  
            // In other words, IsCompleted is equivalent to the Task.Status being equal to RanToCompletion, Faulted, or Canceled
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(expected, task.Result);
        }

        private static TResult AssertTaskCompleted<TResult>(Task<TResult> task)
        {
            // IsCompleted means that the Task has finished executing, either successfully, exceptionally, or due to cancellation.  
            // In other words, IsCompleted is equivalent to the Task.Status being equal to RanToCompletion, Faulted, or Canceled
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            return task.Result; 
        }
    }
}
