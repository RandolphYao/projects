//-----------------------------------------------------------------------
// <copyright file="MemoryChannelTest.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MemoryChannel.Test.Unit
{
    using System.Threading.Tasks;
    using Xunit;

    public class MemoryChannelTest
    {
        public MemoryChannelTest()
        {
        }

        [Fact]
        public void Pending_ReceiveCompleteAfterSendWithSameDataSize()
        {
            MemoryChannel c = new MemoryChannel();
            byte[] receiveBuffer = new byte[3];
            Task<int> receiveTask = c.ReceiveAsync(receiveBuffer);

            Assert.False(receiveTask.IsCompleted);
            Assert.False(receiveTask.IsFaulted);

            var sendBuffer = new byte[] { 1, 2, 3 };
            c.Send(sendBuffer);

            // IsCompleted means that the Task has finished executing, either successfully, exceptionally, or due to cancellation.  
            // In other words, IsCompleted is equivalent to the Task.Status being equal to RanToCompletion, Faulted, or Canceled
            Assert.Equal(TaskStatus.RanToCompletion, receiveTask.Status);
            Assert.Equal(sendBuffer.Length, receiveBuffer.Length);
            Assert.Equal(sendBuffer, receiveBuffer);                
        }
    }
}
