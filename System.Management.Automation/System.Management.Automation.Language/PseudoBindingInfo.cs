namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal sealed class PseudoBindingInfo
    {
        private readonly Collection<AstParameterArgumentPair> _allParsedArguments;
        private readonly Collection<CommandParameterAst> _ambiguousParameters;
        private readonly Dictionary<string, AstParameterArgumentPair> _boundArguments;
        private readonly Dictionary<string, MergedCompiledCommandParameter> _boundParameters;
        private readonly Collection<string> _boundPositionalParameter;
        private readonly System.Management.Automation.CommandInfo _commandInfo;
        private readonly int _defaultParameterSetFlag;
        private readonly Collection<AstParameterArgumentPair> _duplicateParameters;
        private readonly PseudoBindingInfoType _infoType;
        private readonly Collection<CommandParameterAst> _parametersNotFound;
        private readonly List<MergedCompiledCommandParameter> _unboundParameters;
        private readonly int _validParameterSetsFlags;

        internal PseudoBindingInfo(System.Management.Automation.CommandInfo commandInfo, int defaultParameterSetFlag, Collection<AstParameterArgumentPair> allParsedArguments, List<MergedCompiledCommandParameter> unboundParameters)
        {
            this._commandInfo = commandInfo;
            this._infoType = PseudoBindingInfoType.PseudoBindingFail;
            this._defaultParameterSetFlag = defaultParameterSetFlag;
            this._allParsedArguments = allParsedArguments;
            this._unboundParameters = unboundParameters;
        }

        internal PseudoBindingInfo(System.Management.Automation.CommandInfo commandInfo, int validParameterSetsFlags, int defaultParameterSetFalg, Dictionary<string, MergedCompiledCommandParameter> boundParameters, List<MergedCompiledCommandParameter> unboundParameters, Dictionary<string, AstParameterArgumentPair> boundArguments, Collection<string> boundPositionalParameter, Collection<AstParameterArgumentPair> allParsedArguments, Collection<CommandParameterAst> parametersNotFound, Collection<CommandParameterAst> ambiguousParameters, Collection<AstParameterArgumentPair> duplicateParameters)
        {
            this._commandInfo = commandInfo;
            this._infoType = PseudoBindingInfoType.PseudoBindingSucceed;
            this._validParameterSetsFlags = validParameterSetsFlags;
            this._defaultParameterSetFlag = defaultParameterSetFalg;
            this._boundParameters = boundParameters;
            this._unboundParameters = unboundParameters;
            this._boundArguments = boundArguments;
            this._boundPositionalParameter = boundPositionalParameter;
            this._allParsedArguments = allParsedArguments;
            this._parametersNotFound = parametersNotFound;
            this._ambiguousParameters = ambiguousParameters;
            this._duplicateParameters = duplicateParameters;
        }

        internal Collection<AstParameterArgumentPair> AllParsedArguments
        {
            get
            {
                return this._allParsedArguments;
            }
        }

        internal Collection<CommandParameterAst> AmbiguousParameters
        {
            get
            {
                return this._ambiguousParameters;
            }
        }

        internal Dictionary<string, AstParameterArgumentPair> BoundArguments
        {
            get
            {
                return this._boundArguments;
            }
        }

        internal Dictionary<string, MergedCompiledCommandParameter> BoundParameters
        {
            get
            {
                return this._boundParameters;
            }
        }

        internal Collection<string> BoundPositionalParameter
        {
            get
            {
                return this._boundPositionalParameter;
            }
        }

        internal System.Management.Automation.CommandInfo CommandInfo
        {
            get
            {
                return this._commandInfo;
            }
        }

        internal string CommandName
        {
            get
            {
                return this._commandInfo.Name;
            }
        }

        internal int DefaultParameterSetFlag
        {
            get
            {
                return this._defaultParameterSetFlag;
            }
        }

        internal Collection<AstParameterArgumentPair> DuplicateParameters
        {
            get
            {
                return this._duplicateParameters;
            }
        }

        internal PseudoBindingInfoType InfoType
        {
            get
            {
                return this._infoType;
            }
        }

        internal Collection<CommandParameterAst> ParametersNotFound
        {
            get
            {
                return this._parametersNotFound;
            }
        }

        internal List<MergedCompiledCommandParameter> UnboundParameters
        {
            get
            {
                return this._unboundParameters;
            }
        }

        internal int ValidParameterSetsFlags
        {
            get
            {
                return this._validParameterSetsFlags;
            }
        }
    }
}

