namespace System.Management.Automation.Internal
{
    using System;

    internal abstract class ICabinetExtractorLoader
    {
        protected ICabinetExtractorLoader()
        {
        }

        internal virtual ICabinetExtractor GetCabinetExtractor()
        {
            return null;
        }
    }
}

