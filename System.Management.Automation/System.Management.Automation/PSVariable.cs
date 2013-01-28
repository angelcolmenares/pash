namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;

    public class PSVariable : IHasSessionStateEntryVisibility
    {
        private static readonly CallSite<Func<CallSite, object, object>> _copyMutableValueSite = CallSite<Func<CallSite, object, object>>.Create(PSVariableAssignmentBinder.Get());
        private PSModuleInfo _module;
        private SessionStateInternal _sessionState;
        private object _value;
        private SessionStateEntryVisibility _visibility;
        private bool _wasRemoved;
        private PSVariableAttributeCollection attributes;
        private string description;
        private string name;
        private ScopedItemOptions options;

        public PSVariable(string name) : this(name, null, ScopedItemOptions.None, (Collection<Attribute>) null)
        {
        }

        internal PSVariable(string name, bool dummy)
        {
            this.name = string.Empty;
            this.description = string.Empty;
            this.name = name;
        }

        public PSVariable(string name, object value) : this(name, value, ScopedItemOptions.None, (Collection<Attribute>) null)
        {
        }

        public PSVariable(string name, object value, ScopedItemOptions options) : this(name, value, options, (Collection<Attribute>) null)
        {
        }

        public PSVariable(string name, object value, ScopedItemOptions options, Collection<Attribute> attributes)
        {
            this.name = string.Empty;
            this.description = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            this.name = name;
            this.attributes = new PSVariableAttributeCollection(this);
            this.SetValueRawImpl(value, true);
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    this.attributes.Add(attribute);
                }
            }
            this.options = options;
            if (this.IsAllScope)
            {
                VariableAnalysis.NoteAllScopeVariable(name);
            }
        }

        internal PSVariable(string name, object value, ScopedItemOptions options, string description) : this(name, value, options, (Collection<Attribute>) null)
        {
            this.description = description;
        }

        internal PSVariable(string name, object value, ScopedItemOptions options, Collection<Attribute> attributes, string description) : this(name, value, options, attributes)
        {
            this.description = description;
        }

        internal void AddParameterAttributesNoChecks(Collection<Attribute> attributes)
        {
            foreach (Attribute attribute in attributes)
            {
                this.attributes.AddAttributeNoCheck(attribute);
            }
        }

        internal static object CopyMutableValues(object o)
        {
            return _copyMutableValueSite.Target(_copyMutableValueSite, o);
        }

        internal void DebuggerCheckVariableRead()
        {
            ExecutionContext context = (this._sessionState != null) ? this._sessionState.ExecutionContext : LocalPipeline.GetExecutionContextFromTLS();
            if ((context != null) && (context._debuggingMode > 0))
            {
                context.Debugger.CheckVariableRead(this.Name);
            }
        }

        internal void DebuggerCheckVariableWrite()
        {
            ExecutionContext context = (this._sessionState != null) ? this._sessionState.ExecutionContext : LocalPipeline.GetExecutionContextFromTLS();
            if ((context != null) && (context._debuggingMode > 0))
            {
                context.Debugger.CheckVariableWrite(this.Name);
            }
        }

        public virtual bool IsValidValue(object value)
        {
            return IsValidValue(this.attributes, value);
        }

        internal static bool IsValidValue(IEnumerable<Attribute> attributes, object value)
        {
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    if (!IsValidValue(value, attribute))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool IsValidValue(object value, Attribute attribute)
        {
            bool flag = true;
            ValidateArgumentsAttribute attribute2 = attribute as ValidateArgumentsAttribute;
            if (attribute2 != null)
            {
                try
                {
                    ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                    EngineIntrinsics engineIntrinsics = null;
                    if (executionContextFromTLS != null)
                    {
                        engineIntrinsics = executionContextFromTLS.EngineIntrinsics;
                    }
                    attribute2.InternalValidate(value, engineIntrinsics);
                }
                catch (ValidationMetadataException)
                {
                    flag = false;
                }
            }
            return flag;
        }

        internal void SetModule(PSModuleInfo module)
        {
            this._module = module;
        }

        internal void SetOptions(ScopedItemOptions newOptions, bool force)
        {
            if (this.IsConstant || (!force && this.IsReadOnly))
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableNotWritable", SessionStateStrings.VariableNotWritable);
                throw exception;
            }
            if ((newOptions & ScopedItemOptions.Constant) != ScopedItemOptions.None)
            {
                SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableCannotBeMadeConstant", SessionStateStrings.VariableCannotBeMadeConstant);
                throw exception2;
            }
            if (this.IsAllScope && ((newOptions & ScopedItemOptions.AllScope) == ScopedItemOptions.None))
            {
                SessionStateUnauthorizedAccessException exception3 = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableAllScopeOptionCannotBeRemoved", SessionStateStrings.VariableAllScopeOptionCannotBeRemoved);
                throw exception3;
            }
            this.options = newOptions;
        }

        private void SetValue(object value)
        {
            if ((this.options & (ScopedItemOptions.Constant | ScopedItemOptions.ReadOnly)) != ScopedItemOptions.None)
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(this.name, SessionStateCategory.Variable, "VariableNotWritable", SessionStateStrings.VariableNotWritable);
                throw exception;
            }
            object obj2 = value;
            if ((this.attributes != null) && (this.attributes.Count > 0))
            {
                obj2 = TransformValue(this.attributes, value);
                if (!this.IsValidValue(obj2))
                {
                    ValidationMetadataException exception2 = new ValidationMetadataException("ValidateSetFailure", null, Metadata.InvalidValueFailure, new object[] { this.name, (obj2 != null) ? obj2.ToString() : "" });
                    throw exception2;
                }
            }
            if (obj2 != null)
            {
                obj2 = CopyMutableValues(obj2);
            }
            this._value = obj2;
            this.DebuggerCheckVariableWrite();
        }

        internal virtual void SetValueRaw(object newValue, bool preserveValueTypeSemantics)
        {
            this.SetValueRawImpl(newValue, preserveValueTypeSemantics);
        }

        private void SetValueRawImpl(object newValue, bool preserveValueTypeSemantics)
        {
            if (preserveValueTypeSemantics)
            {
                newValue = CopyMutableValues(newValue);
            }
            this._value = newValue;
        }

        internal static object TransformValue(IEnumerable<Attribute> attributes, object value)
        {
            object inputData = value;
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            EngineIntrinsics engineIntrinsics = null;
            if (executionContextFromTLS != null)
            {
                engineIntrinsics = executionContextFromTLS.EngineIntrinsics;
            }
            foreach (Attribute attribute in attributes)
            {
                ArgumentTransformationAttribute attribute2 = attribute as ArgumentTransformationAttribute;
                if (attribute2 != null)
                {
                    inputData = attribute2.Transform(engineIntrinsics, inputData);
                }
            }
            return inputData;
        }

        internal void WrapValue()
        {
            if (!this.IsConstant && (this._value != null))
            {
                this._value = PSObject.AsPSObject(this._value);
            }
        }

        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new PSVariableAttributeCollection(this);
                }
                return this.attributes;
            }
        }

        public virtual string Description
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

        internal bool IsAllScope
        {
            get
            {
                return ((this.options & ScopedItemOptions.AllScope) != ScopedItemOptions.None);
            }
        }

        internal bool IsConstant
        {
            get
            {
                return ((this.options & ScopedItemOptions.Constant) != ScopedItemOptions.None);
            }
        }

        internal bool IsPrivate
        {
            get
            {
                return ((this.options & ScopedItemOptions.Private) != ScopedItemOptions.None);
            }
        }

        internal bool IsReadOnly
        {
            get
            {
                return ((this.options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None);
            }
        }

        public PSModuleInfo Module
        {
            get
            {
                return this._module;
            }
        }

        public string ModuleName
        {
            get
            {
                if (this._module != null)
                {
                    return this._module.Name;
                }
                return string.Empty;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public virtual ScopedItemOptions Options
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

        internal SessionStateInternal SessionState
        {
            get
            {
                return this._sessionState;
            }
            set
            {
                this._sessionState = value;
            }
        }

        public virtual object Value
        {
            get
            {
                this.DebuggerCheckVariableRead();
                return this._value;
            }
            set
            {
                this.SetValue(value);
            }
        }

        public SessionStateEntryVisibility Visibility
        {
            get
            {
                return this._visibility;
            }
            set
            {
                this._visibility = value;
            }
        }

        internal bool WasRemoved
        {
            get
            {
                return this._wasRemoved;
            }
            set
            {
                this._wasRemoved = value;
                if (value)
                {
                    this.options = ScopedItemOptions.None;
                    this._value = null;
                    this._wasRemoved = true;
                    this.attributes = null;
                }
            }
        }
    }
}

