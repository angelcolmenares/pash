namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;

    public interface IAstToWorkflowConverter
    {
        WorkflowInfo CompileWorkflow(string name, string definition, InitialSessionState initialSessionState);
        List<WorkflowInfo> CompileWorkflows(ScriptBlockAst ast, PSModuleInfo definingModule);
        List<ParseError> ValidateAst(FunctionDefinitionAst ast);
    }
}

