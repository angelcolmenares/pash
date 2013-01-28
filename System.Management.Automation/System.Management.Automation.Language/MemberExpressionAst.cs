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

    public class MemberExpressionAst : ExpressionAst, ISupportsAssignment
    {
        public MemberExpressionAst(IScriptExtent extent, ExpressionAst expression, CommandElementAst member, bool @static) : base(extent)
        {
            if ((expression == null) || (member == null))
            {
                throw PSTraceSource.NewArgumentNullException((expression == null) ? "expression" : "member");
            }
            this.Expression = expression;
            base.SetParent(expression);
            this.Member = member;
            base.SetParent(member);
            this.Static = @static;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitMemberExpression(this);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            StringConstantExpressionAst member = this.Member as StringConstantExpressionAst;
            if (member != null)
            {
                PSTypeName[] iteratorVariable1;
                if (this.Static)
                {
                    TypeExpressionAst expression = this.Expression as TypeExpressionAst;
                    if (expression == null)
                    {
                        goto Label_064A;
                    }
                    Type type = expression.TypeName.GetReflectionType();
                    if (type == null)
                    {
                        goto Label_064A;
                    }
                    iteratorVariable1 = new PSTypeName[] { new PSTypeName(type) };
                }
                else
                {
                    iteratorVariable1 = this.Expression.GetInferredType(context).ToArray<PSTypeName>();
                    if (iteratorVariable1.Length == 0)
                    {
                        goto Label_064A;
                    }
                }
                List<string> iteratorVariable2 = new List<string> {
                    member.Value
                };
                foreach (PSTypeName iteratorVariable3 in iteratorVariable1)
                {
                    IEnumerable<object> iteratorVariable4 = CompletionCompleters.GetMembersByInferredType(iteratorVariable3, this.Static, context);
                    for (int i = 0; i < iteratorVariable2.Count; i++)
                    {
                        string iteratorVariable6 = iteratorVariable2[i];
                        foreach (object iteratorVariable7 in iteratorVariable4)
                        {
                            PropertyInfo iteratorVariable8 = iteratorVariable7 as PropertyInfo;
                            if (iteratorVariable8 != null)
                            {
                                if (iteratorVariable8.Name.Equals(iteratorVariable6, StringComparison.OrdinalIgnoreCase) && !(this is InvokeMemberExpressionAst))
                                {
                                    yield return new PSTypeName(iteratorVariable8.PropertyType);
                                    break;
                                }
                            }
                            else
                            {
                                FieldInfo iteratorVariable9 = iteratorVariable7 as FieldInfo;
                                if (iteratorVariable9 != null)
                                {
                                    if (iteratorVariable9.Name.Equals(iteratorVariable6, StringComparison.OrdinalIgnoreCase) && !(this is InvokeMemberExpressionAst))
                                    {
                                        yield return new PSTypeName(iteratorVariable9.FieldType);
                                        break;
                                    }
                                    continue;
                                }
                                DotNetAdapter.MethodCacheEntry iteratorVariable10 = iteratorVariable7 as DotNetAdapter.MethodCacheEntry;
                                if (iteratorVariable10 != null)
                                {
                                    if (iteratorVariable10[0].method.Name.Equals(iteratorVariable6, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!(this is InvokeMemberExpressionAst))
                                        {
                                            yield return new PSTypeName(typeof(PSMethod));
                                            break;
                                        }
                                        foreach (MethodInformation iteratorVariable11 in iteratorVariable10.methodInformationStructures)
                                        {
                                            MethodInfo method = iteratorVariable11.method as MethodInfo;
                                            if ((method != null) && !method.ReturnType.ContainsGenericParameters)
                                            {
                                                yield return new PSTypeName(method.ReturnType);
                                            }
                                        }
                                        break;
                                    }
                                    continue;
                                }
                                PSMemberInfo iteratorVariable13 = iteratorVariable7 as PSMemberInfo;
                                if ((iteratorVariable13 != null) && iteratorVariable13.Name.Equals(iteratorVariable6, StringComparison.OrdinalIgnoreCase))
                                {
                                    PSNoteProperty iteratorVariable14 = iteratorVariable7 as PSNoteProperty;
                                    if (iteratorVariable14 != null)
                                    {
                                        yield return new PSTypeName(iteratorVariable14.Value.GetType());
                                        break;
                                    }
                                    PSAliasProperty iteratorVariable15 = iteratorVariable7 as PSAliasProperty;
                                    if (iteratorVariable15 != null)
                                    {
                                        iteratorVariable2.Add(iteratorVariable15.ReferencedMemberName);
                                    }
                                    else
                                    {
                                        PSCodeProperty iteratorVariable16 = iteratorVariable7 as PSCodeProperty;
                                        if (iteratorVariable16 != null)
                                        {
                                            if (iteratorVariable16.GetterCodeReference != null)
                                            {
                                                yield return new PSTypeName(iteratorVariable16.GetterCodeReference.ReturnType);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            ScriptBlock getterScript = null;
                                            PSScriptProperty iteratorVariable18 = iteratorVariable7 as PSScriptProperty;
                                            if (iteratorVariable18 != null)
                                            {
                                                getterScript = iteratorVariable18.GetterScript;
                                            }
                                            PSScriptMethod iteratorVariable19 = iteratorVariable7 as PSScriptMethod;
                                            if (iteratorVariable19 != null)
                                            {
                                                getterScript = iteratorVariable19.Script;
                                            }
                                            if (getterScript != null)
                                            {
                                                foreach (PSTypeName iteratorVariable20 in getterScript.OutputType)
                                                {
                                                    yield return iteratorVariable20;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        Label_064A:
            yield break;
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitMemberExpression(this);
            switch (action)
            {
                case AstVisitAction.SkipChildren:
                    return AstVisitAction.Continue;

                case AstVisitAction.Continue:
                    action = this.Expression.InternalVisit(visitor);
                    break;
            }
            if (action == AstVisitAction.Continue)
            {
                action = this.Member.InternalVisit(visitor);
            }
            return action;
        }

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            return new MemberAssignableValue { MemberExpression = this };
        }

        public ExpressionAst Expression { get; private set; }

        public CommandElementAst Member { get; private set; }

        public bool Static { get; private set; }

        
    }
}

