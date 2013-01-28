namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal abstract class HelpProviderWithFullCache : HelpProviderWithCache
    {
        internal HelpProviderWithFullCache(HelpSystem helpSystem) : base(helpSystem)
        {
        }

        internal sealed override void DoExactMatchHelp(HelpRequest helpRequest)
        {
        }

        internal sealed override IEnumerable<HelpInfo> DoSearchHelp(HelpRequest helpRequest)
        {
            return null;
        }

        internal sealed override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            if (!base.CacheFullyLoaded || base.AreSnapInsSupported())
            {
                this.LoadCache();
            }
            base.CacheFullyLoaded = true;
            return base.ExactMatchHelp(helpRequest);
        }

        internal virtual void LoadCache()
        {
        }

        internal sealed override IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent)
        {
            if (!base.CacheFullyLoaded || base.AreSnapInsSupported())
            {
                this.LoadCache();
            }
            base.CacheFullyLoaded = true;
            return base.SearchHelp(helpRequest, searchOnlyContent);
        }
    }
}

