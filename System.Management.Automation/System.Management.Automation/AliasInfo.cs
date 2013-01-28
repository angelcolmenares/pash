namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class AliasInfo : CommandInfo
    {
        private string _definition;
        private string description;
        private ScopedItemOptions options;
        private string unresolvedCommandName;

        internal AliasInfo(string name, string definition, ExecutionContext context) : base(name, CommandTypes.Alias)
        {
            this._definition = string.Empty;
            this.description = string.Empty;
            this._definition = definition;
            base.Context = context;
            if (context != null)
            {
                base.SetModule(context.SessionState.Internal.Module);
            }
        }

        internal AliasInfo(string name, string definition, ExecutionContext context, ScopedItemOptions options) : base(name, CommandTypes.Alias)
        {
            this._definition = string.Empty;
            this.description = string.Empty;
            this._definition = definition;
            base.Context = context;
            this.options = options;
            if (context != null)
            {
                base.SetModule(context.SessionState.Internal.Module);
            }
        }

        internal void SetDefinition(string definition, bool force)
        {
            if (((this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None) || (!force && ((this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None)))
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Alias, "AliasNotWritable", SessionStateStrings.AliasNotWritable);
                throw exception;
            }
            this._definition = definition;
        }

        internal void SetOptions(ScopedItemOptions newOptions, bool force)
        {
            if ((this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Alias, "AliasIsConstant", SessionStateStrings.AliasIsConstant);
                throw exception;
            }
            if (!force && ((this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None))
            {
                SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Alias, "AliasIsReadOnly", SessionStateStrings.AliasIsReadOnly);
                throw exception2;
            }
            if ((newOptions & ScopedItemOptions.Constant) != ScopedItemOptions.None)
            {
                SessionStateUnauthorizedAccessException exception3 = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Alias, "AliasCannotBeMadeConstant", SessionStateStrings.AliasCannotBeMadeConstant);
                throw exception3;
            }
            if (((newOptions & ScopedItemOptions.AllScope) == ScopedItemOptions.None) && ((this.options & ScopedItemOptions.AllScope) != ScopedItemOptions.None))
            {
                SessionStateUnauthorizedAccessException exception4 = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Alias, "AliasAllScopeOptionCannotBeRemoved", SessionStateStrings.AliasAllScopeOptionCannotBeRemoved);
                throw exception4;
            }
            this.options = newOptions;
        }

        public override string Definition
        {
            get
            {
                return this._definition;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Alias;
            }
        }

        public ScopedItemOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.SetOptions(value, false);
            }
        }

        public override ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                CommandInfo resolvedCommand = this.ResolvedCommand;
                if (resolvedCommand != null)
                {
                    return resolvedCommand.OutputType;
                }
                return null;
            }
        }

        public CommandInfo ReferencedCommand
        {
            get
            {
                CommandInfo current = null;
                if ((this._definition != null) && (base.Context != null))
                {
                    CommandSearcher searcher = new CommandSearcher(this._definition, SearchResolutionOptions.None, CommandTypes.All, base.Context);
                    if (searcher.MoveNext())
                    {
                        IEnumerator<CommandInfo> enumerator = searcher;
                        current = enumerator.Current;
                    }
                }
                return current;
            }
        }

        public CommandInfo ResolvedCommand
        {
            get
            {
                CommandInfo referencedCommand = null;
                if (this._definition != null)
                {
                    List<string> collection = new List<string> {
                        base.Name
                    };
                    string definition = this._definition;
                    referencedCommand = this.ReferencedCommand;
                    while ((referencedCommand != null) && (referencedCommand.CommandType == CommandTypes.Alias))
                    {
                        referencedCommand = ((AliasInfo) referencedCommand).ReferencedCommand;
                        if (referencedCommand is AliasInfo)
                        {
                            if (SessionStateUtilities.CollectionContainsValue(collection, referencedCommand.Name, StringComparer.OrdinalIgnoreCase))
                            {
                                referencedCommand = null;
                                break;
                            }
                            collection.Add(referencedCommand.Name);
                            definition = referencedCommand.Definition;
                        }
                    }
                    if (referencedCommand == null)
                    {
                        this.unresolvedCommandName = definition;
                    }
                }
                return referencedCommand;
            }
        }

        internal string UnresolvedCommandName
        {
            get
            {
                return this.unresolvedCommandName;
            }
        }
    }
}

