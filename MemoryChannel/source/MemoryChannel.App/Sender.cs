// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sender.cs" company="">
// </copyright>
// <summary>
//   The sender.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace MemoryChannel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The sender.
    /// </summary>
    internal sealed class Sender
    {
        /// <summary>
        /// The channel.
        /// </summary>
        private readonly MemoryChannel channel;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        /// The buffer size.
        /// </summary>
        private readonly int bufferSize;

        /// <summary>
        /// The delay.
        /// </summary>
        private readonly Delay delay;

        /// <summary>
        /// The fill.
        /// </summary>
        private readonly byte fill;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sender"/> class.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="bufferSize">
        /// The buffer size.
        /// </param>
        /// <param name="fill">
        /// The fill.
        /// </param>
        /// <param name="delay">
        /// The delay.
        /// </param>
        public Sender(MemoryChannel channel, Logger logger, int bufferSize, byte fill, Delay delay)
        {
            this.channel = channel;
            this.logger = logger;
            this.bufferSize = bufferSize;
            this.fill = fill;
            this.delay = delay;
        }

        /// <summary>
        /// The run async.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<long> RunAsync(CancellationToken token)
        {
            this.logger.WriteLine("Sender B={0}/F=0x{1:x} starting...", this.bufferSize, this.fill);
            long totalBytes = 0;
            byte[] buffer = new byte[this.bufferSize];
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = this.fill;
            }

            await
                Task.Factory.StartNew(() => totalBytes = this.RunInner(buffer, token), TaskCreationOptions.LongRunning);

            this.logger.WriteLine(
                "Sender B={0}/F=0x{1:x} completed. Sent {2} bytes.", 
                this.bufferSize, 
                this.fill, 
                totalBytes);
            return totalBytes;
        }

        /// <summary>
        /// The run inner.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        private long RunInner(byte[] buffer, CancellationToken token)
        {
            long totalBytes = 0;
            while (!token.IsCancellationRequested)
            {
                this.channel.Send(buffer);
                totalBytes += buffer.Length;
                this.delay.Next();
            }

            return totalBytes;
        }
    }
}