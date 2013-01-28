namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Language;

    internal static class FunctionOps
    {
        internal static void DefineFunction(ExecutionContext context, FunctionDefinitionAst functionDefinitionAst, ScriptBlockExpressionWrapper scriptBlockExpressionWrapper)
        {
            try
            {
                ScriptBlock scriptBlock = scriptBlockExpressionWrapper.GetScriptBlock(context, functionDefinitionAst.IsFilter);
                context.EngineSessionState.SetFunctionRaw(functionDefinitionAst.Name, scriptBlock, context.EngineSessionState.CurrentScope.ScopeOrigin);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                RuntimeException exception2 = exception as RuntimeException;
                if (exception2 == null)
                {
                    throw ExceptionHandlingOps.ConvertToRuntimeException(exception, functionDefinitionAst.Extent);
                }
                InterpreterError.UpdateExceptionErrorRecordPosition(exception2, functionDefinitionAst.Extent);
                throw;
            }
        }

        internal static void DefineWorkflows(ExecutionContext context, ScriptBlockAst scriptBlockAst)
        {
            try
            {
                foreach (WorkflowInfo info in Utils.GetAstToWorkflowConverterAndEnsureWorkflowModuleLoaded(context).CompileWorkflows(scriptBlockAst, null))
                {
                    context.EngineSessionState.SetWorkflowRaw(info, context.EngineSessionState.CurrentScope.ScopeOrigin);
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                RuntimeException exception2 = exception as RuntimeException;
                if (exception2 == null)
                {
                    throw ExceptionHandlingOps.ConvertToRuntimeException(exception, scriptBlockAst.Extent);
                }
                InterpreterError.UpdateExceptionErrorRecordPosition(exception2, scriptBlockAst.Extent);
                throw;
            }
        }
    }
}

