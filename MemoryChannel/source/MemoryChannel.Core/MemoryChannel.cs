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

            lock (this.excessBuffers)
            {
                if (this.pendingReceiveBuffer != null)
                {
                    throw new InvalidOperationException("A receive operation is already in progress");
                }

                this.pendingReceiveBuffer = buffer;
                this.pendingReceiveTaskSource = new TaskCompletionSource<int>();

                if (this.excessBuffers.Count > 0)
                {
                    this.SendDataToReceiver();
                }

                return this.pendingReceiveTaskSource.Task;
            }
        }

        public void Send(byte[] buffer)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Channel was disposed");
            }

            lock (this.excessBuffers)
            {
                this.excessBuffers = this.excessBuffers.Concat(buffer).ToList();

                // check if there are any pending receive 
                // it coould be this.pendingReceiveTaskSource is not null but this.pendingReceiveBuffer is null
                // because if "send" send data to the receiver and then send again. now no pending ReceiveBuffer exist 
                if (this.pendingReceiveTaskSource != null && this.pendingReceiveBuffer != null)
                {
                    this.SendDataToReceiver();
                }
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
                    if (this.pendingReceiveTaskSource != null && this.pendingReceiveTaskSource.Task.IsCompleted != true)
                    {
                        this.pendingReceiveTaskSource.SetResult(0);
                    }
                }

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
            // This means the continuation task will be executed immediately after SetResult() call
            // if we place this.pendingReceiveBuffer = null; after SetResult(), the behavior is weird. 
            this.pendingReceiveBuffer = null;
            this.pendingReceiveTaskSource.SetResult(lengh);
        }
    }
}