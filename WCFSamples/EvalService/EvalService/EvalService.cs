namespace EvalService
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EvalService : IEvalService
    {
        private readonly List<Eval> evals;

        public EvalService()
        {
            this.evals = new List<Eval>();
        }

        public void SubmitEval(Eval eval)
        {
            this.evals.Add(eval);
        }

        public List<Eval> GetEvals()
        {
            return this.evals;
        }
    }
}