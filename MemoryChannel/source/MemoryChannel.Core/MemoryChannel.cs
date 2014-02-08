namespace MemoryChannel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MemoryChannel
    {
        // store data send to the channel but did not read by receiver yet
        private List<byte> excessBuffers;
        private byte[] pendingReceiveBuffer;
        private TaskCompletionSource<int> pendingReceiveTaskSource;

        public MemoryChannel()
        {
            this.excessBuffers = new List<byte>(); 
        }

        public Task<int> ReceiveAsync(byte[] buffer)
        {
            if (this.pendingReceiveBuffer != null)
            {
                throw new InvalidOperationException();
            }

            this.pendingReceiveBuffer = buffer;
            this.pendingReceiveTaskSource = new TaskCompletionSource<int>();

            if (this.excessBuffers.Count > 0)
            {
               this.SendDataToReceiver();
            }

            return this.pendingReceiveTaskSource.Task;
        }

        public void Send(byte[] buffer)
        {
            this.excessBuffers = this.excessBuffers.Concat(buffer).ToList();

            if (this.pendingReceiveTaskSource != null)
            {
                this.SendDataToReceiver();
            }
        }

        private void SendDataToReceiver()
        {
            int lengh = this.excessBuffers.Count > this.pendingReceiveBuffer.Length
                            ? this.pendingReceiveBuffer.Length
                            : this.excessBuffers.Count;
            Array.Copy(this.excessBuffers.ToArray(), 0, this.pendingReceiveBuffer, 0, lengh);
            this.excessBuffers.RemoveRange(0, lengh);

            // Note that SetResult will transition the task to a completed state as well run any synchronous continuations. 
            // This can be undesirable in some situations 
            this.pendingReceiveTaskSource.SetResult(lengh);
            this.pendingReceiveBuffer = null;
        }
    }
}