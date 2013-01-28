namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;

    internal sealed class AstArrayPair : AstParameterArgumentPair
    {
        private ExpressionAst[] _argument;

        internal AstArrayPair(string parameterName, ICollection<ExpressionAst> arguments)
        {
            if (parameterName == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterName");
            }
            if ((arguments == null) || (arguments.Count == 0))
            {
                throw PSTraceSource.NewArgumentNullException("arguments");
            }
            base.Parameter = null;
            base.ParameterArgumentType = AstParameterArgumentType.AstArray;
            base.ParameterSpecified = true;
            base.ArgumentSpecified = true;
            base.ParameterName = parameterName;
            base.ParameterText = parameterName;
            base.ArgumentType = typeof(Array);
            this._argument = arguments.ToArray<ExpressionAst>();
        }

        public ExpressionAst[] Argument
        {
            get
            {
                return this._argument;
            }
        }
    }
}

