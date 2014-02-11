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

        private ReceiveRequest pendingReceiveRequest;

        private bool disposed;

        public MemoryChannel()
        {
            // List is an array
            this.excessBuffers = new List<byte>();
            this.disposed = false;
        }

        public Task<int> ReceiveAsync(byte[] buffer)
        {
            this.ThrowIfDisposed();
            Task<int> resultTask; 

            lock (this.excessBuffers)
            {
                if (this.pendingReceiveRequest != null)
                {
                    throw new InvalidOperationException("A receive operation is already in progress");
                }

                this.pendingReceiveRequest = new ReceiveRequest(buffer);
                resultTask = this.pendingReceiveRequest.ReceiveTask;

				// to prevent deadlocks, you should generally avoid running arbitrary user code such as event handlers or in this case, continuations (invoked by SetResult), under a lock
                this.SendDataToReceiver();
            }

            return resultTask;
        }

        public void Send(byte[] buffer)
        {
            this.ThrowIfDisposed();

            lock (this.excessBuffers)
            {
                this.excessBuffers = this.excessBuffers.Concat(buffer).ToList();

                if (this.pendingReceiveRequest != null)
                {
					// to prevent deadlocks, you should generally avoid running arbitrary user code such as event handlers or in this case, continuations (invoked by SetResult), under a lock
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
                    if (this.pendingReceiveRequest != null && this.pendingReceiveRequest.ReceiveTask.IsCompleted != true)
                    {
                        this.pendingReceiveRequest.CompleteRequest();
                    }
                }

                this.disposed = true;
            }
        }

        private void SendDataToReceiver()
        {
            bool dataSend = false;
            while (this.excessBuffers.Count > 0 && this.pendingReceiveRequest.RemainingReceiveBufferSize > 0)
            {
                int length = this.pendingReceiveRequest.AddReceiveData(this.excessBuffers.ToArray());
                dataSend = true;
                this.excessBuffers.RemoveRange(0, length);
            }

            if (dataSend)
            {
                ReceiveRequest requestResult = this.pendingReceiveRequest;

                /* due to the synchronous continuation execution after SetResult to the pendingReceive task
                 * the SetResult must be after null assignment to this.pendingReceiveRequest
                 * otherwise, the conitnuation task might execute before null assignment to this.pendingReceiveRequest
                 */
                this.pendingReceiveRequest = null;
                requestResult.CompleteRequest();
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Channel was disposed");
            }
        }

        private class ReceiveRequest
        {
            private readonly TaskCompletionSource<int> pendingReceiveTaskSource;

            private readonly byte[] receiveBufferToBeFilled;

            private int totalBytesReceived;

            public ReceiveRequest(byte[] receiveBufferToBeFilled)
            {
                this.receiveBufferToBeFilled = receiveBufferToBeFilled;
                this.pendingReceiveTaskSource = new TaskCompletionSource<int>();
                this.RemainingReceiveBufferSize = receiveBufferToBeFilled.Length;
            }

            public int RemainingReceiveBufferSize { get; private set; }

            public Task<int> ReceiveTask
            {
                get
                {
                    return this.pendingReceiveTaskSource.Task;
                }
            }

            public int AddReceiveData(byte[] receiveData)
            {
                int bytesReceived = Math.Min(this.RemainingReceiveBufferSize, receiveData.Length);
                this.RemainingReceiveBufferSize -= bytesReceived;
                Array.Copy(receiveData, 0, this.receiveBufferToBeFilled, this.totalBytesReceived, bytesReceived);
                this.totalBytesReceived += bytesReceived;
                return bytesReceived;
            }

            public void CompleteRequest()
            {
                this.pendingReceiveTaskSource.SetResult(this.totalBytesReceived);
            }
        }
    }
}