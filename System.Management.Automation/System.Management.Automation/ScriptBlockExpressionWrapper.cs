namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;

    internal class ScriptBlockExpressionWrapper
    {
        private readonly IParameterMetadataProvider _ast;
        private ScriptBlock _scriptBlock;

        internal ScriptBlockExpressionWrapper(IParameterMetadataProvider ast)
        {
            this._ast = ast;
        }

        internal ScriptBlock GetScriptBlock(ExecutionContext context, bool isFilter)
        {
            ScriptBlock block = (this._scriptBlock ?? (this._scriptBlock = new ScriptBlock(this._ast, isFilter))).Clone(false);
            block.SessionStateInternal = context.EngineSessionState;
            return block;
        }
    }
}

