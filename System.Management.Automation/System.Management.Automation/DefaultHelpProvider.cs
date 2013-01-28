namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal class DefaultHelpProvider : HelpFileHelpProvider
    {
        internal DefaultHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        {
        }

        internal override IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest)
        {
            HelpRequest request = helpRequest.Clone();
            request.Target = "default";
            return base.ExactMatchHelp(request);
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.DefaultHelp;
            }
        }

        internal override string Name
        {
            get
            {
                return "Default Help Provider";
            }
        }
    }
}

