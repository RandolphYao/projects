//-----------------------------------------------------------------------
// <copyright file="AwaitableCriticalSection.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace AwaitableCriticalSection
{
    using System;
    using System.Threading.Tasks;

    public class AwaitableCriticalSection
    {
        private readonly string name;

        public AwaitableCriticalSection(string name)
        {
            this.name = name;
        }

        public Task<Token> AcquireAsync()
        {
            throw new NotImplementedException();
        }

        public void Release(Token token)
        {
            throw new NotImplementedException();
        }

        /*
         * My lock has one bonus feature: it returns a Token that can be used to prevent someone 
         * who does not own the lock from releasing it. Typically, “It is the programmer’s responsibility 
         * to ensure that threads do not release the semaphore too many times.” 
         * The Token is a design fix to prevent this problem from ever occurring in any reasonable situation 
         * — at the expense of one additional object allocation.
         */
        public class Token
        {
            protected Token()
            {
            }
        }
    }
}
