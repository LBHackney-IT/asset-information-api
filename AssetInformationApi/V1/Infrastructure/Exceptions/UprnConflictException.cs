using System;

namespace AssetInformationApi.V1.Infrastructure.Exceptions
{
    public class UprnConflictException : Exception
    {
        public int? IncomingUprn { get; private set; }

        public UprnConflictException(int? incoming)
            : base(string.Format("The supplied UPRN ({0}) must be unique.",
                                 (incoming is null) ? "{null}" : incoming.ToString()))
        {
            IncomingUprn = incoming;

        }
    }
}
