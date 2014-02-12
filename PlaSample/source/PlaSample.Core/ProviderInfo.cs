﻿namespace PlaSample
{
    using System;

    public class ProviderInfo
    {
        public ProviderInfo(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; private set; }

        public uint? Level { get; set; }

        public ulong? KeywordsAny { get; set; }

        public ulong? KeywordsAll { get; set; }
    }
}