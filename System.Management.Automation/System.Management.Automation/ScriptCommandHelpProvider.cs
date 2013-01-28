namespace System.Management.Automation
{
    using System;

    internal class ScriptCommandHelpProvider : CommandHelpProvider
    {
        internal ScriptCommandHelpProvider(HelpSystem helpSystem) : base(helpSystem)
        {
        }

        internal override CommandSearcher GetCommandSearcherForExactMatch(string commandName, ExecutionContext context)
        {
            return new CommandSearcher(commandName, SearchResolutionOptions.None, CommandTypes.ExternalScript | CommandTypes.Filter | CommandTypes.Function, context);
        }

        internal override CommandSearcher GetCommandSearcherForSearch(string pattern, ExecutionContext context)
        {
            return new CommandSearcher(pattern, SearchResolutionOptions.CommandNameIsPattern | SearchResolutionOptions.ResolveFunctionPatterns, CommandTypes.ExternalScript | CommandTypes.Filter | CommandTypes.Function, context);
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return (System.Management.Automation.HelpCategory.Workflow | System.Management.Automation.HelpCategory.ExternalScript | System.Management.Automation.HelpCategory.Filter | System.Management.Automation.HelpCategory.Function | System.Management.Automation.HelpCategory.ScriptCommand);
            }
        }
    }
}

