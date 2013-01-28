namespace System.Management.Automation.Language
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IAssignableValue
    {
        Expression GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps);
        Expression SetValue(Compiler compiler, Expression rhs);
    }
}

