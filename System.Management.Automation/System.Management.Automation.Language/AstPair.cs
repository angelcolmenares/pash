namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;

    internal sealed class AstPair : AstParameterArgumentPair
    {
        private CommandElementAst _argument;
        private bool _argumentIsCommandParameterAst;
        private bool _parameterContainsArgument;

        internal AstPair(CommandParameterAst parameterAst)
        {
            if ((parameterAst == null) || (parameterAst.Argument == null))
            {
                throw PSTraceSource.NewArgumentException("parameterAst");
            }
            base.Parameter = parameterAst;
            base.ParameterArgumentType = AstParameterArgumentType.AstPair;
            base.ParameterSpecified = true;
            base.ArgumentSpecified = true;
            base.ParameterName = parameterAst.ParameterName;
            base.ParameterText = "-" + base.ParameterName + ":";
            base.ArgumentType = parameterAst.Argument.StaticType;
            this._parameterContainsArgument = true;
            this._argument = parameterAst.Argument;
        }

        internal AstPair(CommandParameterAst parameterAst, CommandParameterAst argumentAst)
        {
            if ((parameterAst != null) && (parameterAst.Argument != null))
            {
                throw PSTraceSource.NewArgumentException("parameterAst");
            }
            if ((parameterAst == null) || (argumentAst == null))
            {
                throw PSTraceSource.NewArgumentNullException("argumentAst");
            }
            base.Parameter = parameterAst;
            base.ParameterArgumentType = AstParameterArgumentType.AstPair;
            base.ParameterSpecified = true;
            base.ArgumentSpecified = true;
            base.ParameterName = parameterAst.ParameterName;
            base.ParameterText = parameterAst.ParameterName;
            base.ArgumentType = typeof(string);
            this._parameterContainsArgument = false;
            this._argument = argumentAst;
            this._argumentIsCommandParameterAst = true;
        }

        internal AstPair(CommandParameterAst parameterAst, ExpressionAst argumentAst)
        {
            if ((parameterAst != null) && (parameterAst.Argument != null))
            {
                throw PSTraceSource.NewArgumentException("parameterAst");
            }
            if ((parameterAst == null) && (argumentAst == null))
            {
                throw PSTraceSource.NewArgumentNullException("argumentAst");
            }
            base.Parameter = parameterAst;
            base.ParameterArgumentType = AstParameterArgumentType.AstPair;
            base.ParameterSpecified = parameterAst != null;
            base.ArgumentSpecified = argumentAst != null;
            base.ParameterName = (parameterAst != null) ? parameterAst.ParameterName : null;
            base.ParameterText = (parameterAst != null) ? parameterAst.ParameterName : null;
            base.ArgumentType = (argumentAst != null) ? argumentAst.StaticType : null;
            this._parameterContainsArgument = false;
            this._argument = argumentAst;
        }

        public CommandElementAst Argument
        {
            get
            {
                return this._argument;
            }
        }

        public bool ArgumentIsCommandParameterAst
        {
            get
            {
                return this._argumentIsCommandParameterAst;
            }
        }

        public bool ParameterContainsArgument
        {
            get
            {
                return this._parameterContainsArgument;
            }
        }
    }
}

