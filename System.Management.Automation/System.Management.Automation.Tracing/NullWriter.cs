namespace System.Management.Automation.Tracing
{
    using System;

    public sealed class NullWriter : BaseChannelWriter
    {
        private static readonly BaseChannelWriter nullWriter = new NullWriter();

        private NullWriter()
        {
        }

        public static BaseChannelWriter Instance
        {
            get
            {
                return nullWriter;
            }
        }
    }
}

