namespace ConsoleHost
{
    using System;
    using System.ServiceModel;

    using EvalService;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(EvalService));
            host.AddServiceEndpoint(typeof(IEvalService), new BasicHttpBinding(), "http://localhost:8080/evals/basic");
            host.AddServiceEndpoint(typeof(IEvalService), new WSHttpBinding(), "http://localhost:8080/evals/ws");
            host.AddServiceEndpoint(typeof(IEvalService), new NetTcpContextBinding(), "net.tcp://localhost:8081/evals/");

            try
            {
                host.Open();
                PrintServiceInfo(host);
                Console.ReadLine();
                host.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                host.Abort();
                throw;
            }
        }

        static void PrintServiceInfo(ServiceHost host)
        {
            Console.WriteLine("{0} is up and running", host.Description.ServiceType);
            foreach (var se in host.Description.Endpoints)
            {
                Console.WriteLine(se.Address);
            }
        }
    }
}