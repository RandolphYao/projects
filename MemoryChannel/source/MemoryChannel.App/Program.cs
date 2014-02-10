//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MemoryChannel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            Logger logger = new Logger();
            MemoryChannel channel = new MemoryChannel();
            Delay delay = new Delay(3, 100);

            Receiver receiver = new Receiver(channel, logger, 16);
            receiver.DataReceived += ReceiverOnDataReceived;
            Sender sender = new Sender(channel, logger, 16, 1, delay);

            Task receiverTask = receiver.RunAsync();
            Task senderTask = sender.RunAsync(CancellationToken.None);

            try
            {
                Task.WaitAll(receiverTask, senderTask);
            }
            catch (AggregateException e)
            {
                Console.WriteLine(e.ToString());
            }

            channel.Dispose();

            logger.WriteLine("Done.");
        }

        private static void ReceiverOnDataReceived(object sender, DataEventArgs dataEventArgs)
        {
            Console.WriteLine("Receiving: " + Encoding.ASCII.GetString(dataEventArgs.Buffer));
        }
    }
}