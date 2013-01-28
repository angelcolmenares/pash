namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    public class PSSnapInSpecification
    {
        internal PSSnapInSpecification(string psSnapinName)
        {
            PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(psSnapinName);
            this.Name = psSnapinName;
            this.Version = null;
        }

        public string Name { get; internal set; }

        public System.Version Version { get; internal set; }
    }
}

