namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public abstract class Ast
    {
        internal static PSTypeName[] EmptyPSTypeNameArray = new PSTypeName[0];

        protected Ast(IScriptExtent extent)
        {
            if (extent == null)
            {
                throw PSTraceSource.NewArgumentNullException("extent");
            }
            this.Extent = extent;
        }

        internal abstract object Accept(ICustomAstVisitor visitor);
        internal void ClearParent()
        {
            this.Parent = null;
        }

        public Ast Find(Func<Ast, bool> predicate, bool searchNestedScriptBlocks)
        {
            if (predicate == null)
            {
                throw PSTraceSource.NewArgumentNullException("predicate");
            }
            return AstSearcher.FindFirst(this, predicate, searchNestedScriptBlocks);
        }

        public IEnumerable<Ast> FindAll(Func<Ast, bool> predicate, bool searchNestedScriptBlocks)
        {
            if (predicate == null)
            {
                throw PSTraceSource.NewArgumentNullException("predicate");
            }
            return AstSearcher.FindAll(this, predicate, searchNestedScriptBlocks);
        }

        internal abstract IEnumerable<PSTypeName> GetInferredType(CompletionContext context);
        internal abstract AstVisitAction InternalVisit(AstVisitor visitor);
        internal bool IsInWorkflow()
        {
            Ast parent = this;
            bool flag = false;
            while ((parent != null) && !flag)
            {
                ScriptBlockAst ast2 = parent as ScriptBlockAst;
                if (ast2 != null)
                {
                    FunctionDefinitionAst ast3 = ast2.Parent as FunctionDefinitionAst;
                    if (ast3 != null)
                    {
                        flag = true;
                        if (ast3.IsWorkflow)
                        {
                            return true;
                        }
                    }
                }
                CommandAst ast4 = parent as CommandAst;
                if (((ast4 != null) && string.Equals(TokenKind.InlineScript.Text(), ast4.GetCommandName(), StringComparison.OrdinalIgnoreCase)) && (this != ast4))
                {
                    return false;
                }
                parent = parent.Parent;
            }
            return false;
        }

        internal void SetParent(Ast child)
        {
            if (child.Parent != null)
            {
                throw new InvalidOperationException(ParserStrings.AstIsReused);
            }
            child.Parent = this;
        }

        internal void SetParents<T1, T2>(IEnumerable<Tuple<T1, T2>> children) where T1: Ast where T2: Ast
        {
            foreach (Tuple<T1, T2> tuple in children)
            {
                this.SetParent(tuple.Item1);
                this.SetParent(tuple.Item2);
            }
        }

        internal void SetParents(IEnumerable<Ast> children)
        {
            foreach (Ast ast in children)
            {
                this.SetParent(ast);
            }
        }

        public override string ToString()
        {
            return this.Extent.Text;
        }

        public void Visit(AstVisitor astVisitor)
        {
            if (astVisitor == null)
            {
                throw PSTraceSource.NewArgumentNullException("astVisitor");
            }
            this.InternalVisit(astVisitor);
        }

        public object Visit(ICustomAstVisitor astVisitor)
        {
            if (astVisitor == null)
            {
                throw PSTraceSource.NewArgumentNullException("astVisitor");
            }
            return this.Accept(astVisitor);
        }

        public IScriptExtent Extent { get; private set; }

        public Ast Parent { get; private set; }
    }
}

