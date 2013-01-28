namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class IndexExpressionAst : ExpressionAst, ISupportsAssignment
    {
        public IndexExpressionAst(IScriptExtent extent, ExpressionAst target, ExpressionAst index) : base(extent)
        {
            if ((target == null) || (index == null))
            {
                throw PSTraceSource.NewArgumentNullException((target == null) ? "target" : "index");
            }
            this.Target = target;
            base.SetParent(target);
            this.Index = index;
            base.SetParent(index);
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitIndexExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            IEnumerable<PSTypeName> inferredType = this.Target.GetInferredType(context);
            foreach (PSTypeName iteratorVariable1 in inferredType)
            {
                Type type = iteratorVariable1.Type;
                if (type != null)
                {
                    Func<MethodInfo, bool> predicate = null;
                    if (type.IsArray)
                    {
                        yield return new PSTypeName(type.GetElementType());
                        continue;
                    }
                    foreach (Type iteratorVariable3 in type.GetInterfaces())
                    {
                        if (iteratorVariable3.IsGenericType && (iteratorVariable3.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                        {
                            Type iteratorVariable4 = iteratorVariable3.GetGenericArguments()[1];
                            if (!iteratorVariable4.ContainsGenericParameters)
                            {
                                yield return new PSTypeName(iteratorVariable4);
                            }
                        }
                    }
                    DefaultMemberAttribute defaultMember = type.GetCustomAttributes<DefaultMemberAttribute>(true).FirstOrDefault<DefaultMemberAttribute>();
                    if (defaultMember != null)
                    {
                        if (predicate == null)
                        {
                            predicate = m => m.Name.Equals("get_" + defaultMember.MemberName);
                        }
                        IEnumerable<MethodInfo> iteratorVariable5 = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where<MethodInfo>(predicate);
                        foreach (MethodInfo iteratorVariable6 in iteratorVariable5)
                        {
                            yield return new PSTypeName(iteratorVariable6.ReturnType);
                        }
                    }
                }
                yield return iteratorVariable1;
            }
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitIndexExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Target.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = this.Index.InternalVisit(visitor);
            }
            return action;
        }

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            return new IndexAssignableValue { IndexExpressionAst = this };
        }

        public ExpressionAst Index { get; private set; }

        public ExpressionAst Target { get; private set; }

        
    }
}

