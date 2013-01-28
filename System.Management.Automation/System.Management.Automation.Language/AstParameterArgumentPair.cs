namespace System.Management.Automation.Language
{
    using System;

    internal abstract class AstParameterArgumentPair
    {
        private bool _argumentSpecified;
        private Type _argumentType;
        private CommandParameterAst _parameter;
        private AstParameterArgumentType _parameterArgumentType;
        private string _parameterName;
        private bool _parameterSpecified;
        private string _parameterText;

        protected AstParameterArgumentPair()
        {
        }

        public bool ArgumentSpecified
        {
            get
            {
                return this._argumentSpecified;
            }
            protected set
            {
                this._argumentSpecified = value;
            }
        }

        public Type ArgumentType
        {
            get
            {
                return this._argumentType;
            }
            protected set
            {
                this._argumentType = value;
            }
        }

        public CommandParameterAst Parameter
        {
            get
            {
                return this._parameter;
            }
            protected set
            {
                this._parameter = value;
            }
        }

        public AstParameterArgumentType ParameterArgumentType
        {
            get
            {
                return this._parameterArgumentType;
            }
            protected set
            {
                this._parameterArgumentType = value;
            }
        }

        public string ParameterName
        {
            get
            {
                return this._parameterName;
            }
            protected set
            {
                this._parameterName = value;
            }
        }

        public bool ParameterSpecified
        {
            get
            {
                return this._parameterSpecified;
            }
            protected set
            {
                this._parameterSpecified = value;
            }
        }

        public string ParameterText
        {
            get
            {
                return this._parameterText;
            }
            protected set
            {
                this._parameterText = value;
            }
        }
    }
}

