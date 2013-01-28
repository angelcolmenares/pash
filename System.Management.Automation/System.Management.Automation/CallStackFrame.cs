namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    public sealed class CallStackFrame
    {
        private readonly System.Management.Automation.Language.FunctionContext _functionContext;

        internal CallStackFrame(System.Management.Automation.Language.FunctionContext functionContext, System.Management.Automation.InvocationInfo invocationInfo)
        {
            this.InvocationInfo = invocationInfo;
            this._functionContext = functionContext;
            this.Position = functionContext.CurrentPosition;
        }

        public Dictionary<string, PSVariable> GetFrameVariables()
        {
            Func<MutableTuple, bool> predicate = null;
            Dictionary<string, PSVariable> result = new Dictionary<string, PSVariable>(StringComparer.OrdinalIgnoreCase);
            for (SessionStateScope scope = this._functionContext._executionContext.EngineSessionState.CurrentScope; scope != null; scope = scope.Parent)
            {
                if (scope.LocalsTuple == this._functionContext._localsTuple)
                {
                    break;
                }
                if (scope.DottedScopes != null)
                {
                    if (predicate == null)
                    {
                        predicate = s => s == this._functionContext._localsTuple;
                    }
                    if (scope.DottedScopes.Where<MutableTuple>(predicate).Any<MutableTuple>())
                    {
                        MutableTuple[] tupleArray = scope.DottedScopes.ToArray();
                        int index = 0;
                        while (index < tupleArray.Length)
                        {
                            if (tupleArray[index] == this._functionContext._localsTuple)
                            {
                                break;
                            }
                            index++;
                        }
                        while (index < tupleArray.Length)
                        {
                            tupleArray[index].GetVariableTable(result, true);
                            index++;
                        }
                        break;
                    }
                }
            }
            this._functionContext._localsTuple.GetVariableTable(result, true);
            return result;
        }

        public string GetScriptLocation()
        {
            if (string.IsNullOrEmpty(this.ScriptName))
            {
                return DebuggerStrings.NoFile;
            }
            return StringUtil.Format(DebuggerStrings.LocationFormat, Path.GetFileName(this.ScriptName), this.ScriptLineNumber);
        }

        public override string ToString()
        {
            object[] o = new object[] { this.FunctionName, this.ScriptName ?? DebuggerStrings.NoFile, this.ScriptLineNumber };
            return StringUtil.Format(DebuggerStrings.StackTraceFormat, o);
        }

        internal System.Management.Automation.Language.FunctionContext FunctionContext
        {
            get
            {
                return this._functionContext;
            }
        }

        public string FunctionName
        {
            get
            {
                return this._functionContext._functionName;
            }
        }

        public System.Management.Automation.InvocationInfo InvocationInfo { get; private set; }

        public IScriptExtent Position { get; private set; }

        public int ScriptLineNumber
        {
            get
            {
                return this.Position.StartLineNumber;
            }
        }

        public string ScriptName
        {
            get
            {
                return this.Position.File;
            }
        }
    }
}

