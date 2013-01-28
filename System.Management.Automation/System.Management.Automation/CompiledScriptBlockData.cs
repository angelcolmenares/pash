namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation.Interpreter;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;

    internal class CompiledScriptBlockData
    {
        private Attribute[] _attributes;
        private bool _compiledOptimized;
        private bool _compiledUnoptimized;
        private MergedCommandParameterMetadata _parameterMetadata;
        private RuntimeDefinedParameterDictionary _runtimeDefinedParameterDictionary;
        private readonly object _syncObject;
        private bool _usesCmdletBinding;

        internal CompiledScriptBlockData(IParameterMetadataProvider ast)
        {
            this.Ast = ast;
            this._syncObject = new object();
        }

        internal bool Compile(bool optimized)
        {
            if (this._attributes == null)
            {
                this.InitializeMetadata();
            }
            if (optimized && (this.NameToIndexMap == null))
            {
                this.CompileOptimized();
            }
            optimized = optimized && !VariableAnalysis.AnyVariablesCouldBeAllScope(this.NameToIndexMap);
            if (!optimized && !this._compiledUnoptimized)
            {
                this.CompileUnoptimized();
                return optimized;
            }
            if (optimized && !this._compiledOptimized)
            {
                this.CompileOptimized();
            }
            return optimized;
        }

        private void CompileOptimized()
        {
            lock (this._syncObject)
            {
                if (!this._compiledOptimized)
                {
                    new Compiler().Compile(this, true);
                    if (this.DynamicParamBlockTree != null)
                    {
                        this.DynamicParamBlock = this.CompileTree(this.DynamicParamBlockTree);
                    }
                    if (this.BeginBlockTree != null)
                    {
                        this.BeginBlock = this.CompileTree(this.BeginBlockTree);
                    }
                    if (this.ProcessBlockTree != null)
                    {
                        this.ProcessBlock = this.CompileTree(this.ProcessBlockTree);
                    }
                    if (this.EndBlockTree != null)
                    {
                        this.EndBlock = this.CompileTree(this.EndBlockTree);
                    }
                    this._compiledOptimized = true;
                }
            }
        }

        private Action<FunctionContext> CompileTree(Expression<Action<FunctionContext>> lambda)
        {
            if (this.CompileInterpretDecision == CompileInterpretChoice.AlwaysCompile)
            {
                return lambda.Compile();
            }
            int compilationThreshold = (this.CompileInterpretDecision == CompileInterpretChoice.NeverCompile) ? 0x7fffffff : -1;
            return (Action<FunctionContext>) new LightCompiler(compilationThreshold).CompileTop(lambda).CreateDelegate();
        }

        private void CompileUnoptimized()
        {
            lock (this._syncObject)
            {
                if (!this._compiledUnoptimized)
                {
                    new Compiler().Compile(this, false);
                    if (this.UnoptimizedDynamicParamBlockTree != null)
                    {
                        this.UnoptimizedDynamicParamBlock = this.CompileTree(this.UnoptimizedDynamicParamBlockTree);
                    }
                    if (this.UnoptimizedBeginBlockTree != null)
                    {
                        this.UnoptimizedBeginBlock = this.CompileTree(this.UnoptimizedBeginBlockTree);
                    }
                    if (this.UnoptimizedProcessBlockTree != null)
                    {
                        this.UnoptimizedProcessBlock = this.CompileTree(this.UnoptimizedProcessBlockTree);
                    }
                    if (this.UnoptimizedEndBlockTree != null)
                    {
                        this.UnoptimizedEndBlock = this.CompileTree(this.UnoptimizedEndBlockTree);
                    }
                    this._compiledUnoptimized = true;
                }
            }
        }

        internal List<Attribute> GetAttributes()
        {
            if (this._attributes == null)
            {
                this.InitializeMetadata();
            }
            return this._attributes.ToList<Attribute>();
        }

        public MergedCommandParameterMetadata GetParameterMetadata(ScriptBlock scriptBlock)
        {
            if (this._parameterMetadata == null)
            {
                lock (this._syncObject)
                {
                    if (this._parameterMetadata == null)
                    {
                        CommandMetadata metadata = new CommandMetadata(scriptBlock, "", LocalPipeline.GetExecutionContextFromTLS());
                        this._parameterMetadata = metadata.StaticCommandParameterMetadata;
                    }
                }
            }
            return this._parameterMetadata;
        }

        private void InitializeMetadata()
        {
            lock (this._syncObject)
            {
                if (this._attributes == null)
                {
                    System.Management.Automation.CmdletBindingAttribute attribute = null;
                    Attribute[] attributeArray = this.Ast.GetScriptBlockAttributes().ToArray<Attribute>();
                    foreach (Attribute attribute2 in attributeArray)
                    {
                        if (attribute2 is System.Management.Automation.CmdletBindingAttribute)
                        {
                            attribute = attribute ?? ((System.Management.Automation.CmdletBindingAttribute) attribute2);
                        }
                        else if (attribute2 is DebuggerHiddenAttribute)
                        {
                            this.DebuggerHidden = true;
                        }
                        else if ((attribute2 is DebuggerStepThroughAttribute) || (attribute2 is DebuggerNonUserCodeAttribute))
                        {
                            this.DebuggerStepThrough = true;
                        }
                    }
                    this._usesCmdletBinding = attribute != null;
                    bool automaticPositions = (attribute != null) ? attribute.PositionalBinding : true;
                    RuntimeDefinedParameterDictionary parameterMetadata = this.Ast.GetParameterMetadata(automaticPositions, ref this._usesCmdletBinding);
                    this._attributes = attributeArray;
                    this._runtimeDefinedParameterDictionary = parameterMetadata;
                }
            }
        }

        internal IParameterMetadataProvider Ast { get; private set; }

        internal Action<FunctionContext> BeginBlock { get; private set; }

        internal Expression<Action<FunctionContext>> BeginBlockTree { get; set; }

        internal System.Management.Automation.CmdletBindingAttribute CmdletBindingAttribute
        {
            get
            {
                if (this._runtimeDefinedParameterDictionary == null)
                {
                    this.InitializeMetadata();
                }
                if (!this._usesCmdletBinding)
                {
                    return null;
                }
                return (System.Management.Automation.CmdletBindingAttribute) (from attr in this._attributes
                    where attr is System.Management.Automation.CmdletBindingAttribute
                    select attr).FirstOrDefault<Attribute>();
            }
        }

        internal CompileInterpretChoice CompileInterpretDecision { get; set; }

        internal bool DebuggerHidden { get; set; }

        internal bool DebuggerStepThrough { get; set; }

        internal Action<FunctionContext> DynamicParamBlock { get; private set; }

        internal Expression<Action<FunctionContext>> DynamicParamBlockTree { get; set; }

        internal Action<FunctionContext> EndBlock { get; private set; }

        internal Expression<Action<FunctionContext>> EndBlockTree { get; set; }

        internal Type LocalsMutableTupleType { get; set; }

        internal Dictionary<string, int> NameToIndexMap { get; set; }

        internal Action<FunctionContext> ProcessBlock { get; private set; }

        internal Expression<Action<FunctionContext>> ProcessBlockTree { get; set; }

        internal RuntimeDefinedParameterDictionary RuntimeDefinedParameters
        {
            get
            {
                if (this._runtimeDefinedParameterDictionary == null)
                {
                    this.InitializeMetadata();
                }
                return this._runtimeDefinedParameterDictionary;
            }
        }

        internal IScriptExtent[] SequencePoints { get; set; }

        internal Action<FunctionContext> UnoptimizedBeginBlock { get; private set; }

        internal Expression<Action<FunctionContext>> UnoptimizedBeginBlockTree { get; set; }

        internal Action<FunctionContext> UnoptimizedDynamicParamBlock { get; private set; }

        internal Expression<Action<FunctionContext>> UnoptimizedDynamicParamBlockTree { get; set; }

        internal Action<FunctionContext> UnoptimizedEndBlock { get; private set; }

        internal Expression<Action<FunctionContext>> UnoptimizedEndBlockTree { get; set; }

        internal Type UnoptimizedLocalsMutableTupleType { get; set; }

        internal Action<FunctionContext> UnoptimizedProcessBlock { get; private set; }

        internal Expression<Action<FunctionContext>> UnoptimizedProcessBlockTree { get; set; }

        internal bool UsesCmdletBinding
        {
            get
            {
                if (this._attributes != null)
                {
                    return this._usesCmdletBinding;
                }
                return this.Ast.UsesCmdletBinding();
            }
        }
    }
}

