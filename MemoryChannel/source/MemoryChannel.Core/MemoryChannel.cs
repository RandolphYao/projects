namespace MemoryChannel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MemoryChannel
    {
        private List<byte> dataRepo;

        private byte[] pendingReceiveBuffer;
        private TaskCompletionSource<int> pendingReceiveTaskSource;

        public MemoryChannel()
        {
            this.dataRepo = new List<byte>(); 
        }

        public Task<int> ReceiveAsync(byte[] buffer)
        {
            this.pendingReceiveBuffer = buffer;
            this.pendingReceiveTaskSource = new TaskCompletionSource<int>();
            return this.pendingReceiveTaskSource.Task;
        }

        public void Send(byte[] buffer)
        {
            this.dataRepo = this.dataRepo.Concat(buffer).ToList();

            if (this.pendingReceiveTaskSource != null)
            {
                int lengh = this.dataRepo.Count > this.pendingReceiveBuffer.Length ? this.pendingReceiveBuffer.Length : this.dataRepo.Count;               
                Array.Copy(this.dataRepo.ToArray(), 0, this.pendingReceiveBuffer, 0, lengh);
                this.dataRepo.RemoveRange(0, lengh);
                this.pendingReceiveTaskSource.SetResult(lengh);                
            }
        }
    }
}