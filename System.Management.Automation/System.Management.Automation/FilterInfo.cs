namespace System.Management.Automation
{
    using System;

    public class FilterInfo : FunctionInfo
    {
        internal FilterInfo(FilterInfo other) : base(other)
        {
        }

        internal FilterInfo(string name, FilterInfo other) : base(name, other)
        {
        }

        internal FilterInfo(string name, ScriptBlock filter, ExecutionContext context) : this(name, filter, context, null)
        {
        }

        internal FilterInfo(string name, ScriptBlock filter, ExecutionContext context, string helpFile) : base(name, filter, context, helpFile)
        {
            base.SetCommandType(CommandTypes.Filter);
        }

        internal FilterInfo(string name, ScriptBlock filter, ScopedItemOptions options, ExecutionContext context) : this(name, filter, options, context, null)
        {
        }

        internal FilterInfo(string name, ScriptBlock filter, ScopedItemOptions options, ExecutionContext context, string helpFile) : base(name, filter, options, context, helpFile)
        {
            base.SetCommandType(CommandTypes.Filter);
        }

        internal override CommandInfo CreateGetCommandCopy(object[] arguments)
        {
            return new FilterInfo(this) { IsGetCommandCopy = true, Arguments = arguments };
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Filter;
            }
        }
    }
}

