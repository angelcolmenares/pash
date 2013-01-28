namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.InteropServices;

    internal class FindAllVariablesVisitor : AstVisitor
    {
        private bool _disableOptimizations;
        private int _runtimeUsingIndex;
        private readonly Dictionary<string, VariableAnalysisDetails> _variables;
        private static readonly HashSet<string> hashOfPessimizingCmdlets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly string[] pessimizingCmdlets = new string[] { "New-Variable", "Remove-Variable", "Set-Variable", "Set-PSBreakpoint", @"Microsoft.PowerShell.Utility\New-Variable", @"Microsoft.PowerShell.Utility\Remove-Variable", @"Microsoft.PowerShell.Utility\Set-Variable", @"Microsoft.PowerShell.Utility\Set-PSBreakpoint", "nv", "rv", "sbp", "sv", "set" };

        static FindAllVariablesVisitor()
        {
            foreach (string str in pessimizingCmdlets)
            {
                hashOfPessimizingCmdlets.Add(str);
            }
        }

        private FindAllVariablesVisitor(bool disableOptimizations, bool scriptCmdlet)
        {
            int num;
            this._variables = new Dictionary<string, VariableAnalysisDetails>(StringComparer.OrdinalIgnoreCase);
            this._disableOptimizations = disableOptimizations;
            string[] automaticVariables = SpecialVariables.AutomaticVariables;
            for (num = 0; num < automaticVariables.Length; num++)
            {
                this.NoteVariable(automaticVariables[num], num, SpecialVariables.AutomaticVariableTypes[num], true, false);
            }
            if (scriptCmdlet)
            {
                string[] preferenceVariables = SpecialVariables.PreferenceVariables;
                for (num = 0; num < preferenceVariables.Length; num++)
                {
                    this.NoteVariable(preferenceVariables[num], num + 9, SpecialVariables.PreferenceVariableTypes[num], false, true);
                }
            }
            this.NoteVariable("?", -1, typeof(bool), true, false);
        }

        private void NoteVariable(string variableName, int index, Type type, bool automatic = false, bool preferenceVariable = false)
        {
            if (!this._variables.ContainsKey(variableName))
            {
                VariableAnalysisDetails details = new VariableAnalysisDetails {
                    BitIndex = this._variables.Count,
                    LocalTupleIndex = index,
                    Name = variableName,
                    Type = type,
                    Automatic = automatic,
                    PreferenceVariable = preferenceVariable
                };
                this._variables.Add(variableName, details);
            }
        }

        internal static Dictionary<string, VariableAnalysisDetails> Visit(ExpressionAst exprAst)
        {
            FindAllVariablesVisitor visitor = new FindAllVariablesVisitor(true, false);
            exprAst.InternalVisit(visitor);
            return visitor._variables;
        }

        internal static Dictionary<string, VariableAnalysisDetails> Visit(TrapStatementAst trap)
        {
            FindAllVariablesVisitor visitor = new FindAllVariablesVisitor(true, false);
            trap.Body.InternalVisit(visitor);
            return visitor._variables;
        }

        internal static Dictionary<string, VariableAnalysisDetails> Visit(IParameterMetadataProvider ast, bool disableOptimizations, bool scriptCmdlet, out int localsAllocated, out bool forceNoOptimizing)
        {
            FindAllVariablesVisitor visitor = new FindAllVariablesVisitor(disableOptimizations, scriptCmdlet);
            ast.Body.InternalVisit(visitor);
            forceNoOptimizing = visitor._disableOptimizations;
            if (ast.Parameters != null)
            {
                visitor.VisitParameters(ast.Parameters);
            }
            localsAllocated = (from details in visitor._variables
                where details.Value.LocalTupleIndex != -1
                select details).Count<KeyValuePair<string, VariableAnalysisDetails>>();
            return visitor._variables;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            StringConstantExpressionAst ast = commandAst.CommandElements[0] as StringConstantExpressionAst;
            if ((ast != null) && hashOfPessimizingCmdlets.Contains(ast.Value))
            {
                this._disableOptimizations = true;
            }
            if (commandAst.InvocationOperator == TokenKind.Dot)
            {
                this._disableOptimizations = true;
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst)
        {
            if (dataStatementAst.Variable != null)
            {
                this.NoteVariable(dataStatementAst.Variable, -1, null, false, false);
            }
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            this.NoteVariable("foreach", -1, typeof(IEnumerator), false, false);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            return AstVisitAction.SkipChildren;
        }

        private void VisitParameters(ReadOnlyCollection<ParameterAst> parameters)
        {
            foreach (ParameterAst ast in parameters)
            {
                VariablePath variablePath = ast.Name.VariablePath;
                if (variablePath.IsAnyLocal())
                {
                    VariableAnalysisDetails details;
                    string unaliasedVariableName = VariableAnalysis.GetUnaliasedVariableName(variablePath);
                    if (this._variables.TryGetValue(unaliasedVariableName, out details))
                    {
                        object obj2;
                        details.Type = ast.StaticType;
                        if (!Compiler.TryGetDefaultParameterValue(ast.StaticType, out obj2))
                        {
                            details.LocalTupleIndex = -2;
                        }
                    }
                    else
                    {
                        this.NoteVariable(unaliasedVariableName, -1, ast.StaticType, false, false);
                    }
                }
            }
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            this.NoteVariable("switch", -1, typeof(IEnumerator), false, false);
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitTrap(TrapStatementAst trapStatementAst)
        {
            return AstVisitAction.SkipChildren;
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            if (usingExpressionAst.RuntimeUsingIndex == -1)
            {
                usingExpressionAst.RuntimeUsingIndex = this._runtimeUsingIndex;
            }
            this._runtimeUsingIndex++;
            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            VariablePath variablePath = variableExpressionAst.VariablePath;
            if (variablePath.IsAnyLocal())
            {
                if (variablePath.IsPrivate)
                {
                    this._disableOptimizations = true;
                }
                this.NoteVariable(VariableAnalysis.GetUnaliasedVariableName(variablePath), -1, null, false, false);
            }
            return AstVisitAction.Continue;
        }
    }
}

