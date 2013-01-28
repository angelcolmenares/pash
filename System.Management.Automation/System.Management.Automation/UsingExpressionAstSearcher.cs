namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Language;

    internal class UsingExpressionAstSearcher : AstSearcher
    {
        private UsingExpressionAstSearcher(Func<Ast, bool> callback, bool stopOnFirst, bool searchNestedScriptBlocks) : base(callback, stopOnFirst, searchNestedScriptBlocks)
        {
        }

        internal static IEnumerable<Ast> FindAllUsingExpressionExceptForWorkflow(Ast ast)
        {
            UsingExpressionAstSearcher visitor = new UsingExpressionAstSearcher(astParam => astParam is UsingExpressionAst, false, true);
            ast.InternalVisit(visitor);
            return visitor.Results;
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst ast)
        {
            if (ast.IsWorkflow)
            {
                return AstVisitAction.SkipChildren;
            }
            return base.CheckScriptBlock(ast);
        }
    }
}

