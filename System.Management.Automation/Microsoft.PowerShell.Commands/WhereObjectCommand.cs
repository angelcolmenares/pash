namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;
    using System.Text;

    [Cmdlet("Where", "Object", DefaultParameterSetName="EqualSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113423", RemotingCapability=RemotingCapability.None)]
    public sealed class WhereObjectCommand : PSCmdlet
    {
        private TokenKind _binaryOperator = TokenKind.Ieq;
        private bool _forceBooleanEvaluation = true;
        private PSObject _inputObject = AutomationNull.Value;
        private string _property;
        private object _value = true;
        private bool _valueNotSpecified = true;
        private Func<object, object, object> operationDelegate;
        private ScriptBlock script;
        private readonly CallSite<Func<CallSite, object, bool>> toBoolSite = CallSite<Func<CallSite, object, bool>>.Create(PSConvertBinder.Get(typeof(bool)));

        protected override void BeginProcessing()
        {
            Func<object, object, object> func9 = null;
            Func<object, object, object> func10 = null;
            Func<object, object, object> func11 = null;
            Func<object, object, object> func12 = null;
            Func<object, object, object> func13 = null;
            Func<object, object, object> func14 = null;
            Func<object, object, object> func15 = null;
            Func<object, object, object> func16 = null;
            Func<object, object, object> func17 = null;
            Func<object, object, object> func18 = null;
            if (this.script == null)
            {
                switch (this._binaryOperator)
                {
                    case TokenKind.Ieq:
                        if (this._forceBooleanEvaluation)
                        {
                            CallSite<Func<CallSite, object, object, object>> site = CallSite<Func<CallSite, object, object, object>>.Create(PSBinaryOperationBinder.Get(ExpressionType.Equal, true, false));
                            this.operationDelegate = (x, y) => site.Target(site, y, x);
                            return;
                        }
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.Equal, true);
                        return;

                    case TokenKind.Ine:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.NotEqual, true);
                        return;

                    case TokenKind.Ige:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThanOrEqual, true);
                        return;

                    case TokenKind.Igt:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThan, true);
                        return;

                    case TokenKind.Ilt:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.LessThan, true);
                        return;

                    case TokenKind.Ile:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.LessThanOrEqual, true);
                        return;

                    case TokenKind.Ilike:
                        if (func9 == null)
                        {
                            func9 = (lval, rval) => ParserOps.LikeOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, false, true);
                        }
                        this.operationDelegate = func9;
                        return;

                    case TokenKind.Inotlike:
                        if (func11 == null)
                        {
                            func11 = (lval, rval) => ParserOps.LikeOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, true, true);
                        }
                        this.operationDelegate = func11;
                        return;

                    case TokenKind.Imatch:
                        this.CheckLanguageMode();
                        if (func13 == null)
                        {
                            func13 = (lval, rval) => ParserOps.MatchOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, false, true);
                        }
                        this.operationDelegate = func13;
                        return;

                    case TokenKind.Inotmatch:
                        this.CheckLanguageMode();
                        if (func15 == null)
                        {
                            func15 = (lval, rval) => ParserOps.MatchOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, true, true);
                        }
                        this.operationDelegate = func15;
                        return;

                    case TokenKind.Ireplace:
                    case TokenKind.Iin:
                    case TokenKind.Isplit:
                    case TokenKind.Creplace:
                    case TokenKind.Csplit:
                        return;

                    case TokenKind.Icontains:
                    case TokenKind.Inotcontains:
                    case TokenKind.Inotin:
                    case TokenKind.In:
                    {
                        Func<object, object, object> func = null;
                        Func<object, object, object> func2 = null;
                        Func<object, object, object> func3 = null;
                        Func<object, object, object> func4 = null;
                        Tuple<CallSite<Func<CallSite, object, IEnumerator>>, CallSite<Func<CallSite, object, object, object>>> sites = GetContainsCallSites(true);
                        switch (this._binaryOperator)
                        {
                            case TokenKind.Icontains:
                                if (func == null)
                                {
                                    func = (lval, rval) => ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, lval, rval);
                                }
                                this.operationDelegate = func;
                                return;

                            case TokenKind.Inotcontains:
                                if (func2 == null)
                                {
                                    func2 = (lval, rval) => !ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, lval, rval);
                                }
                                this.operationDelegate = func2;
                                return;

                            case TokenKind.Iin:
                                return;

                            case TokenKind.Inotin:
                                if (func4 == null)
                                {
                                    func4 = (lval, rval) => !ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, rval, lval);
                                }
                                this.operationDelegate = func4;
                                return;

                            case TokenKind.In:
                                if (func3 == null)
                                {
                                    func3 = (lval, rval) => ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, rval, lval);
                                }
                                this.operationDelegate = func3;
                                return;
                        }
                        return;
                    }
                    case TokenKind.Ceq:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.Equal, false);
                        return;

                    case TokenKind.Cne:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.NotEqual, false);
                        return;

                    case TokenKind.Cge:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThanOrEqual, false);
                        return;

                    case TokenKind.Cgt:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.GreaterThan, false);
                        return;

                    case TokenKind.Clt:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.LessThan, false);
                        return;

                    case TokenKind.Cle:
                        this.operationDelegate = GetCallSiteDelegate(ExpressionType.LessThanOrEqual, false);
                        return;

                    case TokenKind.Clike:
                        if (func10 == null)
                        {
                            func10 = (lval, rval) => ParserOps.LikeOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, false, false);
                        }
                        this.operationDelegate = func10;
                        return;

                    case TokenKind.Cnotlike:
                        if (func12 == null)
                        {
                            func12 = (lval, rval) => ParserOps.LikeOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, true, false);
                        }
                        this.operationDelegate = func12;
                        return;

                    case TokenKind.Cmatch:
                        this.CheckLanguageMode();
                        if (func14 == null)
                        {
                            func14 = (lval, rval) => ParserOps.MatchOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, false, false);
                        }
                        this.operationDelegate = func14;
                        return;

                    case TokenKind.Cnotmatch:
                        this.CheckLanguageMode();
                        if (func16 == null)
                        {
                            func16 = (lval, rval) => ParserOps.MatchOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval, true, false);
                        }
                        this.operationDelegate = func16;
                        return;

                    case TokenKind.Ccontains:
                    case TokenKind.Cnotcontains:
                    case TokenKind.Cin:
                    case TokenKind.Cnotin:
                    {
                        Func<object, object, object> func5 = null;
                        Func<object, object, object> func6 = null;
                        Func<object, object, object> func7 = null;
                        Func<object, object, object> func8 = null;
                        Tuple<CallSite<Func<CallSite, object, IEnumerator>>, CallSite<Func<CallSite, object, object, object>>> sites = GetContainsCallSites(false);
                        switch (this._binaryOperator)
                        {
                            case TokenKind.Ccontains:
                                if (func5 == null)
                                {
                                    func5 = (lval, rval) => ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, lval, rval);
                                }
                                this.operationDelegate = func5;
                                return;

                            case TokenKind.Cnotcontains:
                                if (func6 == null)
                                {
                                    func6 = (lval, rval) => !ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, lval, rval);
                                }
                                this.operationDelegate = func6;
                                return;

                            case TokenKind.Cin:
                                if (func7 == null)
                                {
                                    func7 = (lval, rval) => ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, rval, lval);
                                }
                                this.operationDelegate = func7;
                                return;

                            case TokenKind.Cnotin:
                                if (func8 == null)
                                {
                                    func8 = (lval, rval) => !ParserOps.ContainsOperatorCompiled(this.Context, sites.Item1, sites.Item2, rval, lval);
                                }
                                this.operationDelegate = func8;
                                return;
                        }
                        return;
                    }
                    case TokenKind.Is:
                        if (func17 == null)
                        {
                            func17 = (lval, rval) => ParserOps.IsOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval);
                        }
                        this.operationDelegate = func17;
                        return;

                    case TokenKind.IsNot:
                        if (func18 == null)
                        {
                            func18 = (lval, rval) => ParserOps.IsNotOperator(base.Context, PositionUtilities.EmptyExtent, lval, rval);
                        }
                        this.operationDelegate = func18;
                        return;
                }
            }
        }

        private void CheckLanguageMode()
        {
            if (base.Context.LanguageMode.Equals(PSLanguageMode.RestrictedLanguage))
            {
                PSInvalidOperationException exception = new PSInvalidOperationException(string.Format(CultureInfo.InvariantCulture, InternalCommandStrings.OperationNotAllowedInRestrictedLanguageMode, new object[] { this._binaryOperator }));
                base.ThrowTerminatingError(new ErrorRecord(exception, "OperationNotAllowedInRestrictedLanguageMode", ErrorCategory.InvalidOperation, null));
            }
        }

        private static Func<object, object, object> GetCallSiteDelegate(ExpressionType expressionType, bool ignoreCase)
        {
            CallSite<Func<CallSite, object, object, object>> site = CallSite<Func<CallSite, object, object, object>>.Create(PSBinaryOperationBinder.Get(expressionType, ignoreCase, false));
            return (x, y) => site.Target(site, x, y);
        }

        private static Tuple<CallSite<Func<CallSite, object, IEnumerator>>, CallSite<Func<CallSite, object, object, object>>> GetContainsCallSites(bool ignoreCase)
        {
            CallSite<Func<CallSite, object, IEnumerator>> site = CallSite<Func<CallSite, object, IEnumerator>>.Create(PSEnumerableBinder.Get());
            CallSite<Func<CallSite, object, object, object>> site2 = CallSite<Func<CallSite, object, object, object>>.Create(PSBinaryOperationBinder.Get(ExpressionType.Equal, ignoreCase, true));
            return Tuple.Create<CallSite<Func<CallSite, object, IEnumerator>>, CallSite<Func<CallSite, object, object, object>>>(site, site2);
        }

        private ReadOnlyPSMemberInfoCollection<PSMemberInfo> GetMatchMembers()
        {
            if (WildcardPattern.ContainsWildcardCharacters(this._property))
            {
                return this._inputObject.Members.Match(this._property, PSMemberTypes.All);
            }
            PSMemberInfoInternalCollection<PSMemberInfo> members = new PSMemberInfoInternalCollection<PSMemberInfo>();
            PSMemberInfo member = this._inputObject.Members[this._property];
            if (member != null)
            {
                members.Add(member);
            }
            return new ReadOnlyPSMemberInfoCollection<PSMemberInfo>(members);
        }

        private object GetValue(ref bool error)
        {
            if (LanguagePrimitives.IsNull(this.InputObject))
            {
                if (base.Context.IsStrictVersion(2))
                {
                    base.WriteError(ForEachObjectCommand.GenerateNameParameterError("InputObject", InternalCommandStrings.InputObjectIsNull, "InputObjectIsNull", this._inputObject, new object[] { this._property }));
                    error = true;
                }
                return null;
            }
            IDictionary dictionary = PSObject.Base(this._inputObject) as IDictionary;
            try
            {
                if ((dictionary != null) && dictionary.Contains(this._property))
                {
                    return dictionary[this._property];
                }
            }
            catch (InvalidOperationException)
            {
            }
            ReadOnlyPSMemberInfoCollection<PSMemberInfo> matchMembers = this.GetMatchMembers();
            if (matchMembers.Count > 1)
            {
                StringBuilder builder = new StringBuilder();
                foreach (PSMemberInfo info in matchMembers)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, " {0}", new object[] { info.Name });
                }
                base.WriteError(ForEachObjectCommand.GenerateNameParameterError("Property", InternalCommandStrings.AmbiguousPropertyOrMethodName, "AmbiguousPropertyName", this._inputObject, new object[] { this._property, builder }));
                error = true;
            }
            else if (matchMembers.Count == 0)
            {
                if (base.Context.IsStrictVersion(2))
                {
                    base.WriteError(ForEachObjectCommand.GenerateNameParameterError("Property", InternalCommandStrings.PropertyNotFound, "PropertyNotFound", this._inputObject, new object[] { this._property }));
                    error = true;
                }
            }
            else
            {
                try
                {
                    return matchMembers[0].Value;
                }
                catch (TerminateException)
                {
                    throw;
                }
                catch (MethodException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    return null;
                }
            }
            return null;
        }

        protected override void ProcessRecord()
        {
            if (this._inputObject != AutomationNull.Value)
            {
                if (this.script != null)
                {
                    object[] input = new object[] { this._inputObject };
                    object obj2 = this.script.DoInvokeReturnAsIs(false, ScriptBlock.ErrorHandlingBehavior.WriteToCurrentErrorPipe, this.InputObject, input, AutomationNull.Value, new object[0]);
                    if (this.toBoolSite.Target(this.toBoolSite, obj2))
                    {
                        base.WriteObject(this.InputObject);
                    }
                }
                else
                {
                    if (this._valueNotSpecified && ((this._binaryOperator != TokenKind.Ieq) || !this._forceBooleanEvaluation))
                    {
                        base.ThrowTerminatingError(ForEachObjectCommand.GenerateNameParameterError("Value", InternalCommandStrings.ValueNotSpecifiedForWhereObject, "ValueNotSpecifiedForWhereObject", null, new object[0]));
                    }
                    if ((!this._valueNotSpecified && (this._binaryOperator == TokenKind.Ieq)) && this._forceBooleanEvaluation)
                    {
                        base.ThrowTerminatingError(ForEachObjectCommand.GenerateNameParameterError("Operator", InternalCommandStrings.OperatorNotSpecified, "OperatorNotSpecified", null, new object[0]));
                    }
                    bool error = false;
                    object obj3 = this.GetValue(ref error);
                    if (!error)
                    {
                        try
                        {
                            if ((this._binaryOperator == TokenKind.Is) || (this._binaryOperator == TokenKind.IsNot))
                            {
                                string str = this._value as string;
                                if (((str != null) && str.StartsWith("[", StringComparison.CurrentCulture)) && str.EndsWith("]", StringComparison.CurrentCulture))
                                {
                                    this._value = str.Substring(1, str.Length - 2);
                                }
                            }
                            object obj4 = this.operationDelegate(obj3, this._value);
                            if (this.toBoolSite.Target(this.toBoolSite, obj4))
                            {
                                base.WriteObject(this.InputObject);
                            }
                        }
                        catch (PipelineStoppedException)
                        {
                            throw;
                        }
                        catch (ArgumentException exception)
                        {
                            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("BinaryOperator", "ParserStrings", "BadOperatorArgument", new object[] { this._binaryOperator, exception.Message }), "BadOperatorArgument", ErrorCategory.InvalidArgument, this._inputObject);
                            base.WriteError(errorRecord);
                        }
                        catch (Exception exception2)
                        {
                            CommandProcessorBase.CheckForSevereException(exception2);
                            ErrorRecord record2 = new ErrorRecord(PSTraceSource.NewInvalidOperationException("ParserStrings", "OperatorFailed", new object[] { this._binaryOperator, exception2.Message }), "OperatorFailed", ErrorCategory.InvalidOperation, this._inputObject);
                            base.WriteError(record2);
                        }
                    }
                }
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveContainsSet")]
        public SwitchParameter CContains
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ccontains);
            }
            set
            {
                this._binaryOperator = TokenKind.Ccontains;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveEqualSet")]
        public SwitchParameter CEQ
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ceq);
            }
            set
            {
                this._binaryOperator = TokenKind.Ceq;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveGreaterOrEqualSet")]
        public SwitchParameter CGE
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cge);
            }
            set
            {
                this._binaryOperator = TokenKind.Cge;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveGreaterThanSet")]
        public SwitchParameter CGT
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cgt);
            }
            set
            {
                this._binaryOperator = TokenKind.Cgt;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveInSet")]
        public SwitchParameter CIn
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cin);
            }
            set
            {
                this._binaryOperator = TokenKind.Cin;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveLessOrEqualSet")]
        public SwitchParameter CLE
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cle);
            }
            set
            {
                this._binaryOperator = TokenKind.Cle;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveLikeSet")]
        public SwitchParameter CLike
        {
            get
            {
                return (this._binaryOperator == TokenKind.Clike);
            }
            set
            {
                this._binaryOperator = TokenKind.Clike;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveLessThanSet")]
        public SwitchParameter CLT
        {
            get
            {
                return (this._binaryOperator == TokenKind.Clt);
            }
            set
            {
                this._binaryOperator = TokenKind.Clt;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveMatchSet")]
        public SwitchParameter CMatch
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cmatch);
            }
            set
            {
                this._binaryOperator = TokenKind.Cmatch;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveNotEqualSet")]
        public SwitchParameter CNE
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cne);
            }
            set
            {
                this._binaryOperator = TokenKind.Cne;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveNotContainsSet")]
        public SwitchParameter CNotContains
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cnotcontains);
            }
            set
            {
                this._binaryOperator = TokenKind.Cnotcontains;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveNotInSet")]
        public SwitchParameter CNotIn
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cnotin);
            }
            set
            {
                this._binaryOperator = TokenKind.Cnotin;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveNotLikeSet")]
        public SwitchParameter CNotLike
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cnotlike);
            }
            set
            {
                this._binaryOperator = TokenKind.Cnotlike;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="CaseSensitiveNotMatchSet")]
        public SwitchParameter CNotMatch
        {
            get
            {
                return (this._binaryOperator == TokenKind.Cnotmatch);
            }
            set
            {
                this._binaryOperator = TokenKind.Cnotmatch;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="ContainsSet"), Alias(new string[] { "IContains" })]
        public SwitchParameter Contains
        {
            get
            {
                return (this._binaryOperator == TokenKind.Icontains);
            }
            set
            {
                this._binaryOperator = TokenKind.Icontains;
            }
        }

        [Alias(new string[] { "IEQ" }), Parameter(ParameterSetName="EqualSet")]
        public SwitchParameter EQ
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ieq);
            }
            set
            {
                this._binaryOperator = TokenKind.Ieq;
                this._forceBooleanEvaluation = false;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="ScriptBlockSet")]
        public ScriptBlock FilterScript
        {
            get
            {
                return this.script;
            }
            set
            {
                this.script = value;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="GreaterOrEqualSet"), Alias(new string[] { "IGE" })]
        public SwitchParameter GE
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ige);
            }
            set
            {
                this._binaryOperator = TokenKind.Ige;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="GreaterThanSet"), Alias(new string[] { "IGT" })]
        public SwitchParameter GT
        {
            get
            {
                return (this._binaryOperator == TokenKind.Igt);
            }
            set
            {
                this._binaryOperator = TokenKind.Igt;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="InSet"), Alias(new string[] { "IIn" })]
        public SwitchParameter In
        {
            get
            {
                return (this._binaryOperator == TokenKind.In);
            }
            set
            {
                this._binaryOperator = TokenKind.In;
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this._inputObject;
            }
            set
            {
                this._inputObject = value;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="IsSet")]
        public SwitchParameter Is
        {
            get
            {
                return (this._binaryOperator == TokenKind.Is);
            }
            set
            {
                this._binaryOperator = TokenKind.Is;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="IsNotSet")]
        public SwitchParameter IsNot
        {
            get
            {
                return (this._binaryOperator == TokenKind.IsNot);
            }
            set
            {
                this._binaryOperator = TokenKind.IsNot;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="LessOrEqualSet"), Alias(new string[] { "ILE" })]
        public SwitchParameter LE
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ile);
            }
            set
            {
                this._binaryOperator = TokenKind.Ile;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="LikeSet"), Alias(new string[] { "ILike" })]
        public SwitchParameter Like
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ilike);
            }
            set
            {
                this._binaryOperator = TokenKind.Ilike;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="LessThanSet"), Alias(new string[] { "ILT" })]
        public SwitchParameter LT
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ilt);
            }
            set
            {
                this._binaryOperator = this._binaryOperator = TokenKind.Ilt;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="MatchSet"), Alias(new string[] { "IMatch" })]
        public SwitchParameter Match
        {
            get
            {
                return (this._binaryOperator == TokenKind.Imatch);
            }
            set
            {
                this._binaryOperator = TokenKind.Imatch;
            }
        }

        [Alias(new string[] { "INE" }), Parameter(Mandatory=true, ParameterSetName="NotEqualSet")]
        public SwitchParameter NE
        {
            get
            {
                return (this._binaryOperator == TokenKind.Ine);
            }
            set
            {
                this._binaryOperator = TokenKind.Ine;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="NotContainsSet"), Alias(new string[] { "INotContains" })]
        public SwitchParameter NotContains
        {
            get
            {
                return (this._binaryOperator == TokenKind.Inotcontains);
            }
            set
            {
                this._binaryOperator = TokenKind.Inotcontains;
            }
        }

        [Alias(new string[] { "INotIn" }), Parameter(Mandatory=true, ParameterSetName="NotInSet")]
        public SwitchParameter NotIn
        {
            get
            {
                return (this._binaryOperator == TokenKind.Inotin);
            }
            set
            {
                this._binaryOperator = TokenKind.Inotin;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="NotLikeSet"), Alias(new string[] { "INotLike" })]
        public SwitchParameter NotLike
        {
            get
            {
                return false;
            }
            set
            {
                this._binaryOperator = TokenKind.Inotlike;
            }
        }

        [Alias(new string[] { "INotMatch" }), Parameter(Mandatory=true, ParameterSetName="NotMatchSet")]
        public SwitchParameter NotMatch
        {
            get
            {
                return (this._binaryOperator == TokenKind.Inotmatch);
            }
            set
            {
                this._binaryOperator = TokenKind.Inotmatch;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, Position=0, ParameterSetName="EqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveEqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="NotEqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveNotEqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="GreaterThanSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveGreaterThanSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="LessThanSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveLessThanSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="GreaterOrEqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveGreaterOrEqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="LessOrEqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveLessOrEqualSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="LikeSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveLikeSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="NotLikeSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveNotLikeSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="MatchSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveMatchSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="NotMatchSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveNotMatchSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="ContainsSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveContainsSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="NotContainsSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveNotContainsSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="InSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveInSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="NotInSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="CaseSensitiveNotInSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="IsSet"), Parameter(Mandatory=true, Position=0, ParameterSetName="IsNotSet")]
        public string Property
        {
            get
            {
                return this._property;
            }
            set
            {
                this._property = value;
            }
        }

        [Parameter(Position=1, ParameterSetName="CaseSensitiveNotLikeSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveNotInSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveLessThanSet"), Parameter(Position=1, ParameterSetName="LikeSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveLikeSet"), Parameter(Position=1, ParameterSetName="MatchSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveGreaterOrEqualSet"), Parameter(Position=1, ParameterSetName="LessThanSet"), Parameter(Position=1, ParameterSetName="IsSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveNotContainsSet"), Parameter(Position=1, ParameterSetName="ContainsSet"), Parameter(Position=1, ParameterSetName="InSet"), Parameter(Position=1, ParameterSetName="IsNotSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveInSet"), Parameter(Position=1, ParameterSetName="NotInSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveLessOrEqualSet"), Parameter(Position=1, ParameterSetName="NotMatchSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveNotMatchSet"), Parameter(Position=1, ParameterSetName="LessOrEqualSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveContainsSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveMatchSet"), Parameter(Position=1, ParameterSetName="GreaterOrEqualSet"), Parameter(Position=1, ParameterSetName="NotContainsSet"), Parameter(Position=1, ParameterSetName="NotLikeSet"), Parameter(Position=1, ParameterSetName="EqualSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveEqualSet"), Parameter(Position=1, ParameterSetName="NotEqualSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveNotEqualSet"), Parameter(Position=1, ParameterSetName="GreaterThanSet"), Parameter(Position=1, ParameterSetName="CaseSensitiveGreaterThanSet")]
        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                this._valueNotSpecified = false;
            }
        }
    }
}

