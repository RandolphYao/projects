namespace HelloWorldService
{
    using System.Runtime.Serialization;
    using System.ServiceModel;

    // Indicates that an interface or a class defines a service contract in a Windows Communication Foundation (WCF) application.
    [ServiceContract]
    public interface IHelloWorld
    {
        // operation contract indicates that a method defines an operation that is part of a service contract in a Windows Communication Foundation (WCF) application.
        [OperationContract]
        string SayHello(Name person);
    }

    [DataContract]
    public class Name
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }
    }
}