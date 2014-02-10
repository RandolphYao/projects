namespace MemoryChannel
{
    using System.Threading;

    internal sealed class Delay
    {
        private readonly int iterations;

        private readonly int milliseconds;

        private int current;

        public Delay(int iterations, int milliseconds)
        {
            this.iterations = iterations;
            this.milliseconds = milliseconds;
        }

        public void Next()
        {
            if (++this.current == this.iterations)
            {
                this.current = 0;
                Thread.Sleep(this.milliseconds);
            }
        }
    }
}