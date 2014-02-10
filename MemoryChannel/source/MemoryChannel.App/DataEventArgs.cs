namespace MemoryChannel
{
    using System;

    internal sealed class DataEventArgs : EventArgs
    {
        public DataEventArgs(byte[] buffer, int bytesRead)
        {
            this.Buffer = buffer;
            this.BytesRead = bytesRead;
        }

        public int BytesRead { get; private set; }

        public byte[] Buffer { get; private set; }
    }
}