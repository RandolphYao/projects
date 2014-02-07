//-----------------------------------------------------------------------
// <copyright file="Class1.cs" company="MyCompany">
// Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MemoryChannel
{
    using System;
    using System.Threading.Tasks;

    public class Class1
    {
        private readonly string name;

        public Class1(string name)
        {
            this.name = name;
        }

        public Task<string> DoAsync()
        {
            return Task.FromResult(this.name);
        }
    }
}
