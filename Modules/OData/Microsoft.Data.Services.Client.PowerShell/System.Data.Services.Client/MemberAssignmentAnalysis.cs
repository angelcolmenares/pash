namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class MemberAssignmentAnalysis : ALinqExpressionVisitor
    {
        internal static readonly Expression[] EmptyExpressionArray = new Expression[0];
        private readonly Expression entity;
        private Exception incompatibleAssignmentsException;
        private bool multiplePathsFound;
        private List<Expression> pathFromEntity;

        private MemberAssignmentAnalysis(Expression entity)
        {
            this.entity = entity;
            this.pathFromEntity = new List<Expression>();
        }

        internal static MemberAssignmentAnalysis Analyze(Expression entityInScope, Expression assignmentExpression)
        {
            MemberAssignmentAnalysis analysis = new MemberAssignmentAnalysis(entityInScope);
            analysis.Visit(assignmentExpression);
            return analysis;
        }

        private bool CheckCompatibleAssigmentExpression(Expression expressionToAssign, Type initType, ref MemberAssignmentAnalysis previousNested)
        {
            MemberAssignmentAnalysis analysis = Analyze(this.entity, expressionToAssign);
            if (analysis.MultiplePathsFound)
            {
                this.multiplePathsFound = true;
                return false;
            }
            Exception exception = analysis.CheckCompatibleAssignments(initType, ref previousNested);
            if (exception != null)
            {
                this.incompatibleAssignmentsException = exception;
                return false;
            }
            if (this.pathFromEntity.Count == 0)
            {
                this.pathFromEntity.AddRange(analysis.GetExpressionsToTargetEntity());
            }
            return true;
        }

        internal Exception CheckCompatibleAssignments(Type targetType, ref MemberAssignmentAnalysis previous)
        {
            if (previous == null)
            {
                previous = this;
                return null;
            }
            Expression[] expressionsToTargetEntity = previous.GetExpressionsToTargetEntity();
            Expression[] candidate = this.GetExpressionsToTargetEntity();
            return CheckCompatibleAssignments(targetType, expressionsToTargetEntity, candidate);
        }

        private static Exception CheckCompatibleAssignments(Type targetType, Expression[] previous, Expression[] candidate)
        {
            if (previous.Length != candidate.Length)
            {
                throw CheckCompatibleAssignmentsFail(targetType, previous, candidate);
            }
            for (int i = 0; i < previous.Length; i++)
            {
                Expression expression = previous[i];
                Expression expression2 = candidate[i];
                if (expression.NodeType != expression2.NodeType)
                {
                    throw CheckCompatibleAssignmentsFail(targetType, previous, candidate);
                }
                if (expression != expression2)
                {
                    if (expression.NodeType != ExpressionType.MemberAccess)
                    {
                        return CheckCompatibleAssignmentsFail(targetType, previous, candidate);
                    }
                    if (((MemberExpression) expression).Member.Name != ((MemberExpression) expression2).Member.Name)
                    {
                        return CheckCompatibleAssignmentsFail(targetType, previous, candidate);
                    }
                }
            }
            return null;
        }

        private static Exception CheckCompatibleAssignmentsFail(Type targetType, Expression[] previous, Expression[] candidate)
        {
            return new NotSupportedException(System.Data.Services.Client.Strings.ALinq_ProjectionMemberAssignmentMismatch(targetType.FullName, previous.LastOrDefault<Expression>(), candidate.LastOrDefault<Expression>()));
        }

        internal Expression[] GetExpressionsBeyondTargetEntity()
        {
            if (this.pathFromEntity.Count <= 1)
            {
                return EmptyExpressionArray;
            }
            return new Expression[] { this.pathFromEntity[this.pathFromEntity.Count - 1] };
        }

        internal Expression[] GetExpressionsToTargetEntity()
        {
            return this.GetExpressionsToTargetEntity(true);
        }

        internal Expression[] GetExpressionsToTargetEntity(bool ignoreLastExpression)
        {
            int num = ignoreLastExpression ? 1 : 0;
            if (this.pathFromEntity.Count <= num)
            {
                return EmptyExpressionArray;
            }
            Expression[] expressionArray = new Expression[this.pathFromEntity.Count - num];
            for (int i = 0; i < expressionArray.Length; i++)
            {
                expressionArray[i] = this.pathFromEntity[i];
            }
            return expressionArray;
        }

        internal override Expression Visit(Expression expression)
        {
            if (!this.multiplePathsFound && (this.incompatibleAssignmentsException == null))
            {
                return base.Visit(expression);
            }
            return expression;
        }

        internal override Expression VisitConditional(ConditionalExpression c)
        {
            ResourceBinder.PatternRules.MatchNullCheckResult result = ResourceBinder.PatternRules.MatchNullCheck(this.entity, c);
            if (result.Match)
            {
                this.Visit(result.AssignExpression);
                return c;
            }
            return base.VisitConditional(c);
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            Type type;
            Expression expression = base.VisitMemberAccess(m);
            Expression item = ResourceBinder.StripTo<Expression>(m.Expression, out type);
            if (this.pathFromEntity.Contains(item))
            {
                this.pathFromEntity.Add(m);
            }
            return expression;
        }

        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            Expression expression = init;
            MemberAssignmentAnalysis previousNested = null;
            foreach (MemberBinding binding in init.Bindings)
            {
                MemberAssignment assignment = binding as MemberAssignment;
                if ((assignment != null) && !this.CheckCompatibleAssigmentExpression(assignment.Expression, init.Type, ref previousNested))
                {
                    return expression;
                }
            }
            return expression;
        }

        internal override Expression VisitMethodCall(MethodCallExpression call)
        {
            if (ReflectionUtil.IsSequenceMethod(call.Method, SequenceMethod.Select))
            {
                this.Visit(call.Arguments[0]);
                return call;
            }
            return base.VisitMethodCall(call);
        }

        internal override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Members == null)
            {
                return base.VisitNew(nex);
            }
            MemberAssignmentAnalysis previousNested = null;
            foreach (Expression expression in nex.Arguments)
            {
                if (!this.CheckCompatibleAssigmentExpression(expression, nex.Type, ref previousNested))
                {
                    return nex;
                }
            }
            return nex;
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if (p == this.entity)
            {
                if (this.pathFromEntity.Count != 0)
                {
                    this.multiplePathsFound = true;
                    return p;
                }
                this.pathFromEntity.Add(p);
            }
            return p;
        }

        internal Exception IncompatibleAssignmentsException
        {
            get
            {
                return this.incompatibleAssignmentsException;
            }
        }

        internal bool MultiplePathsFound
        {
            get
            {
                return this.multiplePathsFound;
            }
        }
    }
}

