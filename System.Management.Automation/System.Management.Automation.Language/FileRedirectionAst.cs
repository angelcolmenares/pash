namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class FileRedirectionAst : RedirectionAst
    {
        public FileRedirectionAst(IScriptExtent extent, RedirectionStream stream, ExpressionAst file, bool append) : base(extent, stream)
        {
            if (file == null)
            {
                throw PSTraceSource.NewArgumentNullException("file");
            }
            this.Location = file;
            base.SetParent(file);
            this.Append = append;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitFileRedirection(this);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitFileRedirection(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Location.InternalVisit(visitor);
                    break;
            }
            return action;
        }

        public bool Append { get; private set; }

        public ExpressionAst Location { get; private set; }
    }
}

