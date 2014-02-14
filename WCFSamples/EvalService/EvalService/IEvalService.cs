namespace EvalService
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;

   [ServiceContract]
    public interface IEvalService
    {
        [OperationContract]
        void SubmitEval(Eval eval);

        [OperationContract]
        List<Eval> GetEvals();
    }

    [DataContract]
    public class Eval
    {
        [DataMember]
        public string Submit { get; set; }

        [DataMember]
        public DateTime Timesent { get; set; }

        [DataMember]
        public string Commments { get; set; }
    }
}