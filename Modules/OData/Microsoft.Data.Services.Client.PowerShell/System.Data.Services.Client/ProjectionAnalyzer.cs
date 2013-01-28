namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ProjectionAnalyzer
    {
        private static void Analyze(LambdaExpression e, PathBox pb, DataServiceContext context)
        {
            bool flag = ClientTypeUtil.TypeOrElementTypeIsEntity(e.Body.Type);
            ParameterExpression pe = e.Parameters.Last<ParameterExpression>();
            bool flag2 = ClientTypeUtil.TypeOrElementTypeIsEntity(pe.Type);
            if (flag2)
            {
                pb.PushParamExpression(pe);
            }
            if (!flag)
            {
                NonEntityProjectionAnalyzer.Analyze(e.Body, pb, context);
            }
            else
            {
                switch (e.Body.NodeType)
                {
                    case ExpressionType.Constant:
                        throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CannotCreateConstantEntity);

                    case ExpressionType.MemberInit:
                        EntityProjectionAnalyzer.Analyze((MemberInitExpression) e.Body, pb, context);
                        goto Label_0099;

                    case ExpressionType.New:
                        throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CannotConstructKnownEntityTypes);
                }
                NonEntityProjectionAnalyzer.Analyze(e.Body, pb, context);
            }
        Label_0099:
            if (flag2)
            {
                pb.PopParamExpression();
            }
        }

        private static void Analyze(MemberInitExpression mie, PathBox pb, DataServiceContext context)
        {
            if (ClientTypeUtil.TypeOrElementTypeIsEntity(mie.Type))
            {
                EntityProjectionAnalyzer.Analyze(mie, pb, context);
            }
            else
            {
                NonEntityProjectionAnalyzer.Analyze(mie, pb, context);
            }
        }

        internal static bool Analyze(LambdaExpression le, ResourceExpression re, bool matchMembers, DataServiceContext context)
        {
            if (le.Body.NodeType == ExpressionType.Constant)
            {
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(le.Body.Type))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CannotCreateConstantEntity);
                }
                re.Projection = new ProjectionQueryOptionExpression(le.Body.Type, le, new List<string>());
                return true;
            }
            if ((le.Body.NodeType == ExpressionType.MemberInit) || (le.Body.NodeType == ExpressionType.New))
            {
                AnalyzeResourceExpression(le, re, context);
                return true;
            }
            if (matchMembers && (SkipConverts(le.Body).NodeType == ExpressionType.MemberAccess))
            {
                AnalyzeResourceExpression(le, re, context);
                return true;
            }
            return false;
        }

        private static void AnalyzeResourceExpression(LambdaExpression lambda, ResourceExpression resource, DataServiceContext context)
        {
            PathBox pb = new PathBox();
            Analyze(lambda, pb, context);
            resource.Projection = new ProjectionQueryOptionExpression(lambda.Body.Type, lambda, pb.ProjectionPaths.ToList<string>());
            resource.ExpandPaths = pb.ExpandPaths.Union<string>(resource.ExpandPaths, StringComparer.Ordinal).ToList<string>();
            resource.RaiseUriVersion(pb.UriVersion);
        }

        internal static void CheckChainedSequence(MethodCallExpression call, Type type)
        {
            if (ReflectionUtil.IsSequenceSelectMethod(call.Method))
            {
                MethodCallExpression expression = ResourceBinder.StripTo<MethodCallExpression>(call.Arguments[0]);
                if ((expression != null) && ReflectionUtil.IsSequenceSelectMethod(expression.Method))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(type, call.ToString()));
                }
            }
        }

        internal static bool IsCollectionProducingExpression(Expression e)
        {
            if (TypeSystem.FindIEnumerable(e.Type) != null)
            {
                Type elementType = TypeSystem.GetElementType(e.Type);
                Type dataServiceCollectionOfT = WebUtil.GetDataServiceCollectionOfT(new Type[] { elementType });
                if (typeof(List<>).MakeGenericType(new Type[] { elementType }).IsAssignableFrom(e.Type) || ((dataServiceCollectionOfT != null) && dataServiceCollectionOfT.IsAssignableFrom(e.Type)))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsDisallowedExpressionForMethodCall(Expression e, DataServiceProtocolVersion maxProtocolVersion)
        {
            MemberExpression expression = e as MemberExpression;
            if ((expression != null) && ClientTypeUtil.TypeIsEntity(expression.Expression.Type, maxProtocolVersion))
            {
                return false;
            }
            return IsCollectionProducingExpression(e);
        }

        internal static bool IsMethodCallAllowedEntitySequence(MethodCallExpression call)
        {
            if (!ReflectionUtil.IsSequenceMethod(call.Method, SequenceMethod.ToList))
            {
                return ReflectionUtil.IsSequenceMethod(call.Method, SequenceMethod.Select);
            }
            return true;
        }

        private static Expression SkipConverts(Expression expression)
        {
            Expression operand = expression;
            while ((operand.NodeType == ExpressionType.Convert) || (operand.NodeType == ExpressionType.ConvertChecked))
            {
                operand = ((UnaryExpression) operand).Operand;
            }
            return operand;
        }

        private class EntityProjectionAnalyzer : ALinqExpressionVisitor
        {
            private readonly PathBox box;
            private readonly DataServiceContext context;
            private bool leafExpressionIsMemberAccess;
            private readonly Type type;

            private EntityProjectionAnalyzer(PathBox pb, Type type, DataServiceContext context)
            {
                this.box = pb;
                this.type = type;
                this.context = context;
            }

            internal static void Analyze(MemberInitExpression mie, PathBox pb, DataServiceContext context)
            {
                ProjectionAnalyzer.EntityProjectionAnalyzer analyzer = new ProjectionAnalyzer.EntityProjectionAnalyzer(pb, mie.Type, context);
                MemberAssignmentAnalysis previous = null;
                foreach (MemberBinding binding in mie.Bindings)
                {
                    MemberAssignment assignment = binding as MemberAssignment;
                    analyzer.Visit(assignment.Expression);
                    if (assignment != null)
                    {
                        MemberAssignmentAnalysis analysis2 = MemberAssignmentAnalysis.Analyze(pb.ParamExpressionInScope, assignment.Expression);
                        if (analysis2.IncompatibleAssignmentsException != null)
                        {
                            throw analysis2.IncompatibleAssignmentsException;
                        }
                        Type memberType = ClientTypeUtil.GetMemberType(assignment.Member);
                        Expression[] expressionsBeyondTargetEntity = analysis2.GetExpressionsBeyondTargetEntity();
                        if (expressionsBeyondTargetEntity.Length == 0)
                        {
                            throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(memberType, assignment.Expression));
                        }
                        MemberExpression expression = expressionsBeyondTargetEntity[expressionsBeyondTargetEntity.Length - 1] as MemberExpression;
                        analysis2.CheckCompatibleAssignments(mie.Type, ref previous);
                        if (expression != null)
                        {
                            if (expression.Member.Name != assignment.Member.Name)
                            {
                                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_PropertyNamesMustMatchInProjections(expression.Member.Name, assignment.Member.Name));
                            }
                            bool flag = ClientTypeUtil.TypeOrElementTypeIsEntity(memberType);
                            if (ClientTypeUtil.TypeOrElementTypeIsEntity(expression.Type) && !flag)
                            {
                                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(memberType, assignment.Expression));
                            }
                        }
                    }
                }
            }

            internal override Expression VisitBinary(BinaryExpression b)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, b.ToString()));
            }

            internal override Expression VisitConditional(ConditionalExpression c)
            {
                ResourceBinder.PatternRules.MatchNullCheckResult result = ResourceBinder.PatternRules.MatchNullCheck(this.box.ParamExpressionInScope, c);
                if (!result.Match)
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, c.ToString()));
                }
                this.Visit(result.AssignExpression);
                return c;
            }

            internal override Expression VisitConstant(ConstantExpression c)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, c.ToString()));
            }

            internal override Expression VisitInvocation(InvocationExpression iv)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, iv.ToString()));
            }

            internal override Expression VisitLambda(LambdaExpression lambda)
            {
                ProjectionAnalyzer.Analyze(lambda, this.box, this.context);
                return lambda;
            }

            internal override Expression VisitListInit(ListInitExpression init)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, init.ToString()));
            }

            internal override Expression VisitMemberAccess(MemberExpression m)
            {
                PropertyInfo info;
                Expression expression;
                Type type;
                this.leafExpressionIsMemberAccess = true;
                if (!ClientTypeUtil.TypeOrElementTypeIsEntity(m.Expression.Type) || ProjectionAnalyzer.IsCollectionProducingExpression(m.Expression))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, m.ToString()));
                }
                if (!ResourceBinder.PatternRules.MatchNonPrivateReadableProperty(m, out info, out expression))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, m.ToString()));
                }
                Expression expression2 = base.VisitMemberAccess(m);
                ResourceBinder.StripTo<Expression>(m.Expression, out type);
                this.box.AppendPropertyToPath(info, type, this.context);
                this.leafExpressionIsMemberAccess = false;
                return expression2;
            }

            internal override Expression VisitMemberInit(MemberInitExpression init)
            {
                if (!ClientTypeUtil.TypeOrElementTypeIsEntity(init.Type))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, init.ToString()));
                }
                ProjectionAnalyzer.Analyze(init, this.box, this.context);
                return init;
            }

            internal override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (((m.Object != null) && (ProjectionAnalyzer.IsDisallowedExpressionForMethodCall(m.Object, this.context.MaxProtocolVersion) || !ClientTypeUtil.TypeOrElementTypeIsEntity(m.Object.Type))) || (m.Arguments.Any<Expression>(a => ProjectionAnalyzer.IsDisallowedExpressionForMethodCall(a, this.context.MaxProtocolVersion)) || ((m.Object == null) && !ClientTypeUtil.TypeOrElementTypeIsEntity(m.Arguments[0].Type))))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, m.ToString()));
                }
                if (!ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(m))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, m.ToString()));
                }
                ProjectionAnalyzer.CheckChainedSequence(m, this.type);
                return base.VisitMethodCall(m);
            }

            internal override NewExpression VisitNew(NewExpression nex)
            {
                if (ResourceBinder.PatternRules.MatchNewDataServiceCollectionOfT(nex))
                {
                    if (ClientTypeUtil.TypeOrElementTypeIsEntity(nex.Type))
                    {
                        foreach (Expression expression in nex.Arguments)
                        {
                            if (expression.NodeType != ExpressionType.Constant)
                            {
                                base.Visit(expression);
                            }
                        }
                        return nex;
                    }
                }
                else if (ResourceBinder.PatternRules.MatchNewCollectionOfT(nex) && !ClientTypeUtil.TypeOrElementTypeIsEntity(nex.Type))
                {
                    foreach (Expression expression2 in nex.Arguments)
                    {
                        if (expression2.NodeType != ExpressionType.Constant)
                        {
                            base.Visit(expression2);
                        }
                    }
                    return nex;
                }
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, nex.ToString()));
            }

            internal override Expression VisitNewArray(NewArrayExpression na)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, na.ToString()));
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                if (p != this.box.ParamExpressionInScope)
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CanOnlyProjectTheLeaf);
                }
                this.box.StartNewPath();
                return p;
            }

            internal override Expression VisitTypeIs(TypeBinaryExpression b)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, b.ToString()));
            }

            internal override Expression VisitUnary(UnaryExpression u)
            {
                if (ResourceBinder.PatternRules.MatchConvertToAssignable(u) || ((u.NodeType == ExpressionType.TypeAs) && this.leafExpressionIsMemberAccess))
                {
                    return base.VisitUnary(u);
                }
                if ((u.NodeType == ExpressionType.Convert) || (u.NodeType == ExpressionType.ConvertChecked))
                {
                    Type type = Nullable.GetUnderlyingType(u.Operand.Type) ?? u.Operand.Type;
                    Type type2 = Nullable.GetUnderlyingType(u.Type) ?? u.Type;
                    if (PrimitiveType.IsKnownType(type) && PrimitiveType.IsKnownType(type2))
                    {
                        return base.Visit(u.Operand);
                    }
                }
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjectionToEntity(this.type, u.ToString()));
            }
        }

        private class NonEntityProjectionAnalyzer : DataServiceALinqExpressionVisitor
        {
            private PathBox box;
            private readonly DataServiceContext context;
            private bool leafExpressionIsMemberAccess;
            private Type type;

            private NonEntityProjectionAnalyzer(PathBox pb, Type type, DataServiceContext context)
            {
                this.box = pb;
                this.type = type;
                this.context = context;
            }

            internal static void Analyze(Expression e, PathBox pb, DataServiceContext context)
            {
                ProjectionAnalyzer.NonEntityProjectionAnalyzer analyzer = new ProjectionAnalyzer.NonEntityProjectionAnalyzer(pb, e.Type, context);
                MemberInitExpression expression = e as MemberInitExpression;
                if (expression != null)
                {
                    foreach (MemberBinding binding in expression.Bindings)
                    {
                        MemberAssignment assignment = binding as MemberAssignment;
                        if (assignment != null)
                        {
                            analyzer.Visit(assignment.Expression);
                        }
                    }
                }
                else
                {
                    analyzer.Visit(e);
                }
            }

            internal override Expression VisitBinary(BinaryExpression b)
            {
                if ((ClientTypeUtil.TypeOrElementTypeIsEntity(b.Left.Type) || ClientTypeUtil.TypeOrElementTypeIsEntity(b.Right.Type)) || (ProjectionAnalyzer.IsCollectionProducingExpression(b.Left) || ProjectionAnalyzer.IsCollectionProducingExpression(b.Right)))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, b.ToString()));
                }
                return base.VisitBinary(b);
            }

            internal override Expression VisitConditional(ConditionalExpression c)
            {
                ResourceBinder.PatternRules.MatchNullCheckResult result = ResourceBinder.PatternRules.MatchNullCheck(this.box.ParamExpressionInScope, c);
                if (result.Match)
                {
                    this.Visit(result.AssignExpression);
                    return c;
                }
                if (((ClientTypeUtil.TypeOrElementTypeIsEntity(c.Test.Type) || ClientTypeUtil.TypeOrElementTypeIsEntity(c.IfTrue.Type)) || (ClientTypeUtil.TypeOrElementTypeIsEntity(c.IfFalse.Type) || ProjectionAnalyzer.IsCollectionProducingExpression(c.Test))) || (ProjectionAnalyzer.IsCollectionProducingExpression(c.IfTrue) || ProjectionAnalyzer.IsCollectionProducingExpression(c.IfFalse)))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, c.ToString()));
                }
                return base.VisitConditional(c);
            }

            internal override Expression VisitConstant(ConstantExpression c)
            {
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(c.Type))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, c.ToString()));
                }
                return base.VisitConstant(c);
            }

            internal override Expression VisitInvocation(InvocationExpression iv)
            {
                if ((ClientTypeUtil.TypeOrElementTypeIsEntity(iv.Expression.Type) || ProjectionAnalyzer.IsCollectionProducingExpression(iv.Expression)) || iv.Arguments.Any<Expression>(delegate (Expression a) {
                    if (!ClientTypeUtil.TypeOrElementTypeIsEntity(a.Type))
                    {
                        return ProjectionAnalyzer.IsCollectionProducingExpression(a);
                    }
                    return true;
                }))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, iv.ToString()));
                }
                return base.VisitInvocation(iv);
            }

            internal override Expression VisitLambda(LambdaExpression lambda)
            {
                ProjectionAnalyzer.Analyze(lambda, this.box, this.context);
                return lambda;
            }

            internal override Expression VisitMemberAccess(MemberExpression m)
            {
                PropertyInfo info;
                Expression expression;
                Type type = m.Expression.Type;
                this.leafExpressionIsMemberAccess = true;
                if (PrimitiveType.IsKnownNullableType(type))
                {
                    this.leafExpressionIsMemberAccess = false;
                    return base.VisitMemberAccess(m);
                }
                if (ProjectionAnalyzer.IsCollectionProducingExpression(m.Expression))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, m.ToString()));
                }
                if (!ResourceBinder.PatternRules.MatchNonPrivateReadableProperty(m, out info, out expression))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, m.ToString()));
                }
                Expression expression2 = base.VisitMemberAccess(m);
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(type))
                {
                    Type type2;
                    ResourceBinder.StripTo<Expression>(m.Expression, out type2);
                    this.box.AppendPropertyToPath(info, type2, this.context);
                    this.leafExpressionIsMemberAccess = false;
                }
                return expression2;
            }

            internal override Expression VisitMemberInit(MemberInitExpression init)
            {
                ProjectionAnalyzer.Analyze(init, this.box, this.context);
                return init;
            }

            internal override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (((m.Object != null) && ProjectionAnalyzer.IsDisallowedExpressionForMethodCall(m.Object, this.context.MaxProtocolVersion)) || m.Arguments.Any<Expression>(a => ProjectionAnalyzer.IsDisallowedExpressionForMethodCall(a, this.context.MaxProtocolVersion)))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, m.ToString()));
                }
                ProjectionAnalyzer.CheckChainedSequence(m, this.type);
                if (!ProjectionAnalyzer.IsMethodCallAllowedEntitySequence(m) && (((m.Object != null) ? ClientTypeUtil.TypeOrElementTypeIsEntity(m.Object.Type) : false) || m.Arguments.Any<Expression>(a => ClientTypeUtil.TypeOrElementTypeIsEntity(a.Type))))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, m.ToString()));
                }
                return base.VisitMethodCall(m);
            }

            internal override NewExpression VisitNew(NewExpression nex)
            {
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(nex.Type) && !ResourceBinder.PatternRules.MatchNewDataServiceCollectionOfT(nex))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, nex.ToString()));
                }
                return base.VisitNew(nex);
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(p.Type))
                {
                    if (p != this.box.ParamExpressionInScope)
                    {
                        throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, p.ToString()));
                    }
                    this.box.StartNewPath();
                }
                return p;
            }

            internal override Expression VisitTypeIs(TypeBinaryExpression b)
            {
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(b.Expression.Type) || ProjectionAnalyzer.IsCollectionProducingExpression(b.Expression))
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, b.ToString()));
                }
                return base.VisitTypeIs(b);
            }

            internal override Expression VisitUnary(UnaryExpression u)
            {
                if (!ResourceBinder.PatternRules.MatchConvertToAssignable(u))
                {
                    if ((u.NodeType == ExpressionType.TypeAs) && this.leafExpressionIsMemberAccess)
                    {
                        return base.VisitUnary(u);
                    }
                    if (ClientTypeUtil.TypeOrElementTypeIsEntity(u.Operand.Type))
                    {
                        throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ExpressionNotSupportedInProjection(this.type, u.ToString()));
                    }
                }
                return base.VisitUnary(u);
            }
        }
    }
}

