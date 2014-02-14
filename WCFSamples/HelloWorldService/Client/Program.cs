namespace Client
{
    using System;

    using Client.HelloWorldServiceReference;

    using HelloWorldService;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Enter First Name:");
            string firstname = Console.ReadLine();

            Console.WriteLine("Enter Last Name:");
            string lastName = Console.ReadLine();

            HelloWorldClient client = new HelloWorldClient("NetTcpBinding_IHelloWorld");
            Name myName = new Name() { FirstName = firstname, LastName = lastName };
            Console.WriteLine("Sending Service Request ...");
            Console.WriteLine(client.SayHelloAsync(myName).Result);     
            client.Close();
            Console.ReadLine();
        }
    }
}