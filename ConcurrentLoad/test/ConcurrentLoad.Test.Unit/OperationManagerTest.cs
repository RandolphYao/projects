//-----------------------------------------------------------------------
// <copyright file="OperationManagerTest.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ConcurrentLoad.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class OperationManagerTest
    {
        public OperationManagerTest()
        {
        }

        [Fact]
        public void Max_of_one_allows_only_one_call_at_a_time()
        {
            Queue<TaskCompletionSource<bool>> pendingTasks = new Queue<TaskCompletionSource<bool>>();
            Func<Task> doAsync = delegate
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                pendingTasks.Enqueue(tcs);
                Assert.Equal(1, pendingTasks.Count);
                return tcs.Task;
            };

            OperationManager manager = new OperationManager(1, doAsync);
            Task task = manager.RunAsync(1);

            Assert.False(task.IsCompleted);
            Assert.Equal(1, pendingTasks.Count);

            TaskCompletionSource<bool> current = pendingTasks.Dequeue();
            current.SetResult(false);

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(0, pendingTasks.Count);
        }

        [Fact]
        public void Max_of_one_with_call_count_3_allows_only_one_call_at_a_time_for_3_iterations()
        {
            Queue<TaskCompletionSource<bool>> pending = new Queue<TaskCompletionSource<bool>>();
            Func<Task> doAsync = delegate
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                pending.Enqueue(tcs);
                Assert.Equal(1, pending.Count);
                return tcs.Task;
            };

            OperationManager manager = new OperationManager(1, doAsync);
            Task task = manager.RunAsync(3);

            Assert.False(task.IsCompleted);
            Assert.Equal(1, pending.Count);

            TaskCompletionSource<bool> current = pending.Dequeue();

            // complete one iteration of the task controlled by tcs
            current.SetResult(false);

            // the overall operation task is still going on, currently having one task
            Assert.False(task.IsCompleted);
            Assert.Equal(1, pending.Count);

            current = pending.Dequeue();
            current.SetResult(false);

            Assert.False(task.IsCompleted);
            Assert.Equal(1, pending.Count);

            current = pending.Dequeue();
            current.SetResult(false);

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(0, pending.Count);
        }

        [Fact]
        public void Max_of_2_with_call_count_3_allows_2_calls_at_a_time_for_3_total()
        {
            Queue<TaskCompletionSource<bool>> pending = new Queue<TaskCompletionSource<bool>>();
            Func<Task> doAsync = delegate
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                pending.Enqueue(tcs);
                Assert.True(pending.Count < 3, "Pending calls should be less than 3");
                return tcs.Task;
            };

            OperationManager manager = new OperationManager(2, doAsync);
            Task task = manager.RunAsync(3);

            Assert.False(task.IsCompleted);
            Assert.Equal(2, pending.Count);

            TaskCompletionSource<bool> current = pending.Dequeue();
            current.SetResult(false);

            Assert.False(task.IsCompleted);
            Assert.Equal(2, pending.Count);

            current = pending.Dequeue();
            current.SetResult(false);

            Assert.False(task.IsCompleted);
            Assert.Equal(1, pending.Count);

            current = pending.Dequeue();
            current.SetResult(false);

            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Assert.Equal(0, pending.Count);
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
