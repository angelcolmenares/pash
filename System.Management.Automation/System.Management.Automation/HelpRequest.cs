namespace System.Management.Automation
{
    using System;

    internal class HelpRequest
    {
        private string[] _component;
        private string[] _functionality;
        private System.Management.Automation.HelpCategory _helpCategory;
        private int _maxResults = -1;
        private System.Management.Automation.CommandOrigin _origin;
        private string _provider;
        private System.Management.Automation.ProviderContext _providerContext;
        private string[] _role;
        private string _target;

        internal HelpRequest(string target, System.Management.Automation.HelpCategory helpCategory)
        {
            this._target = target;
            this._helpCategory = helpCategory;
            this._origin = System.Management.Automation.CommandOrigin.Runspace;
        }

        internal HelpRequest Clone()
        {
            return new HelpRequest(this.Target, this.HelpCategory) { Provider = this.Provider, MaxResults = this.MaxResults, Component = this.Component, Role = this.Role, Functionality = this.Functionality, ProviderContext = this.ProviderContext, CommandOrigin = this.CommandOrigin };
        }

        internal void Validate()
        {
            if (((string.IsNullOrEmpty(this._target) && (this._helpCategory == System.Management.Automation.HelpCategory.None)) && (string.IsNullOrEmpty(this._provider) && (this._component == null))) && ((this._role == null) && (this._functionality == null)))
            {
                this._target = "default";
                this._helpCategory = System.Management.Automation.HelpCategory.DefaultHelp;
            }
            else
            {
                if (string.IsNullOrEmpty(this._target))
                {
                    if (!string.IsNullOrEmpty(this._provider) && ((this._helpCategory == System.Management.Automation.HelpCategory.None) || (this._helpCategory == System.Management.Automation.HelpCategory.Provider)))
                    {
                        this._target = this._provider;
                    }
                    else
                    {
                        this._target = "*";
                    }
                }
                if ((((this._component != null) || (this._role != null)) || (this._functionality != null)) && (this._helpCategory == System.Management.Automation.HelpCategory.None))
                {
                    this._helpCategory = System.Management.Automation.HelpCategory.Workflow | System.Management.Automation.HelpCategory.ExternalScript | System.Management.Automation.HelpCategory.Filter | System.Management.Automation.HelpCategory.Function | System.Management.Automation.HelpCategory.ScriptCommand | System.Management.Automation.HelpCategory.Cmdlet | System.Management.Automation.HelpCategory.Alias;
                }
                else
                {
                    if ((this._helpCategory & System.Management.Automation.HelpCategory.Cmdlet) > System.Management.Automation.HelpCategory.None)
                    {
                        this._helpCategory |= System.Management.Automation.HelpCategory.Alias;
                    }
                    if (this._helpCategory == System.Management.Automation.HelpCategory.None)
                    {
                        this._helpCategory = System.Management.Automation.HelpCategory.All;
                    }
                    this._helpCategory &= ~System.Management.Automation.HelpCategory.DefaultHelp;
                }
            }
        }

        internal System.Management.Automation.CommandOrigin CommandOrigin
        {
            get
            {
                return this._origin;
            }
            set
            {
                this._origin = value;
            }
        }

        internal string[] Component
        {
            get
            {
                return this._component;
            }
            set
            {
                this._component = value;
            }
        }

        internal string[] Functionality
        {
            get
            {
                return this._functionality;
            }
            set
            {
                this._functionality = value;
            }
        }

        internal System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return this._helpCategory;
            }
            set
            {
                this._helpCategory = value;
            }
        }

        internal int MaxResults
        {
            get
            {
                return this._maxResults;
            }
            set
            {
                this._maxResults = value;
            }
        }

        internal string Provider
        {
            get
            {
                return this._provider;
            }
            set
            {
                this._provider = value;
            }
        }

        internal System.Management.Automation.ProviderContext ProviderContext
        {
            get
            {
                return this._providerContext;
            }
            set
            {
                this._providerContext = value;
            }
        }

        internal string[] Role
        {
            get
            {
                return this._role;
            }
            set
            {
                this._role = value;
            }
        }

        internal string Target
        {
            get
            {
                return this._target;
            }
            set
            {
                this._target = value;
            }
        }
    }
}

