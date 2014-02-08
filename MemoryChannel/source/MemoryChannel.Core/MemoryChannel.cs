namespace MemoryChannel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MemoryChannel : IDisposable
    {
        // store data send to the channel but did not read by receiver yet
        private List<byte> excessBuffers;

        private byte[] pendingReceiveBuffer;

        private TaskCompletionSource<int> pendingReceiveTaskSource;

        private bool disposed = false;

        public MemoryChannel()
        {
            this.excessBuffers = new List<byte>();
        }

        public Task<int> ReceiveAsync(byte[] buffer)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Channel was disposed");
            }

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
            if (this.disposed)
            {
                throw new ObjectDisposedException("Channel was disposed");
            }

            this.excessBuffers = this.excessBuffers.Concat(buffer).ToList();

            if (this.pendingReceiveTaskSource != null)
            {
                this.SendDataToReceiver();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.  
                    if (this.pendingReceiveTaskSource != null)
                    {
                        this.pendingReceiveTaskSource.SetResult(0);
                    }
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.

                // Note disposing has been done.
                this.disposed = true;
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