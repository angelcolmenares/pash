namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public class WorkflowInfo : FunctionInfo
    {
        private string _definition;
        private ReadOnlyCollection<WorkflowInfo> _workflowsCalled;
        private static ReadOnlyCollection<WorkflowInfo> EmptyCalledWorkflows = new ReadOnlyCollection<WorkflowInfo>(new WorkflowInfo[0]);

        internal WorkflowInfo(WorkflowInfo other) : base(other)
        {
            this._definition = "";
            base.SetCommandType(CommandTypes.Workflow);
            this.CopyFields(other);
        }

        internal WorkflowInfo(string name, WorkflowInfo other) : base(name, other)
        {
            this._definition = "";
            base.SetCommandType(CommandTypes.Workflow);
            this.CopyFields(other);
        }

        internal WorkflowInfo(string name, ScriptBlock workflow, ExecutionContext context) : this(name, workflow, context, null)
        {
        }

        internal WorkflowInfo(string name, ScriptBlock workflow, ExecutionContext context, string helpFile) : base(name, workflow, context, helpFile)
        {
            this._definition = "";
            base.SetCommandType(CommandTypes.Workflow);
        }

        internal WorkflowInfo(string name, ScriptBlock workflow, ScopedItemOptions options, ExecutionContext context) : this(name, workflow, options, context, null)
        {
        }

        internal WorkflowInfo(string name, ScriptBlock workflow, ScopedItemOptions options, ExecutionContext context, string helpFile) : base(name, workflow, options, context, helpFile)
        {
            this._definition = "";
            base.SetCommandType(CommandTypes.Workflow);
        }

        public WorkflowInfo(string name, string definition, ScriptBlock workflow, string xamlDefinition, WorkflowInfo[] workflowsCalled) : this(name, workflow, null)
        {
            if (string.IsNullOrEmpty(xamlDefinition))
            {
                throw PSTraceSource.NewArgumentNullException("xamlDefinition");
            }
            this._definition = definition;
            this.XamlDefinition = xamlDefinition;
            if (workflowsCalled != null)
            {
                this._workflowsCalled = new ReadOnlyCollection<WorkflowInfo>(workflowsCalled);
            }
        }

        public WorkflowInfo(string name, string definition, ScriptBlock workflow, string xamlDefinition, WorkflowInfo[] workflowsCalled, PSModuleInfo module) : this(name, definition, workflow, xamlDefinition, workflowsCalled)
        {
            base.SetModule(module);
        }

        private void CopyFields(WorkflowInfo other)
        {
            this.XamlDefinition = other.XamlDefinition;
            this.NestedXamlDefinition = other.NestedXamlDefinition;
            this._workflowsCalled = other.WorkflowsCalled;
            this._definition = other.Definition;
        }

        internal override CommandInfo CreateGetCommandCopy(object[] arguments)
        {
            return new WorkflowInfo(this) { IsGetCommandCopy = true, Arguments = arguments };
        }

        protected internal override void Update(FunctionInfo function, bool force, ScopedItemOptions options, string helpFile)
        {
            WorkflowInfo other = function as WorkflowInfo;
            if (other == null)
            {
                throw PSTraceSource.NewArgumentException("function");
            }
            base.Update(function, force, options, helpFile);
            this.CopyFields(other);
        }

        public override string Definition
        {
            get
            {
                return this._definition;
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Workflow;
            }
        }

        public string NestedXamlDefinition { get; set; }

        public ReadOnlyCollection<WorkflowInfo> WorkflowsCalled
        {
            get
            {
                return (this._workflowsCalled ?? EmptyCalledWorkflows);
            }
        }

        public string XamlDefinition { get; internal set; }
    }
}

