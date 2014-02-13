namespace HelloWorldService
{
    public class HelloWorldService : IHelloWorld
    {
        public string SayHello(Name person)
        {
            return string.Format("Hello {0} {1}", person.FirstName, person.LastName);
        }
    }
}
