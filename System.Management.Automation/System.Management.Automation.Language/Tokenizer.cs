namespace System.Management.Automation.Language
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Numerics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    [DebuggerDisplay("Mode = {Mode}; Script = {_script}")]
    internal class Tokenizer
    {
        private InternalScriptExtent _beginSignatureExtent;
        private int _currentIndex;
        private static readonly Dictionary<string, TokenKind> _keywordTable = new Dictionary<string, TokenKind>(StringComparer.OrdinalIgnoreCase);
        private static readonly string[] _keywordText = new string[] { 
            "elseif", "if", "else", "switch", "foreach", "from", "in", "for", "while", "until", "do", "try", "catch", "finally", "trap", "data", 
            "return", "continue", "break", "exit", "throw", "begin", "process", "end", "dynamicparam", "function", "filter", "param", "class", "define", "var", "using", 
            "workflow", "parallel", "sequence", "inlinescript"
         };
        private static readonly TokenKind[] _keywordTokenKind = new TokenKind[] { 
            TokenKind.ElseIf, TokenKind.If, TokenKind.Else, TokenKind.Switch, TokenKind.Foreach, TokenKind.From, TokenKind.In, TokenKind.For, TokenKind.While, TokenKind.Until, TokenKind.Do, TokenKind.Try, TokenKind.Catch, TokenKind.Finally, TokenKind.Trap, TokenKind.Data, 
            TokenKind.Return, TokenKind.Continue, TokenKind.Break, TokenKind.Exit, TokenKind.Throw, TokenKind.Begin, TokenKind.Process, TokenKind.End, TokenKind.Dynamicparam, TokenKind.Function, TokenKind.Filter, TokenKind.Param, TokenKind.Class, TokenKind.Define, TokenKind.Var, TokenKind.Using, 
            TokenKind.Workflow, TokenKind.Parallel, TokenKind.Sequence, TokenKind.InlineScript
         };
        private int _nestedTokensAdjustment;
        private static readonly Dictionary<string, TokenKind> _operatorTable = new Dictionary<string, TokenKind>(StringComparer.OrdinalIgnoreCase);
        internal static readonly string[] _operatorText = new string[] { 
            "bnot", "not", "eq", "ieq", "ceq", "ne", "ine", "cne", "ge", "ige", "cge", "gt", "igt", "cgt", "lt", "ilt", 
            "clt", "le", "ile", "cle", "like", "ilike", "clike", "notlike", "inotlike", "cnotlike", "match", "imatch", "cmatch", "notmatch", "inotmatch", "cnotmatch", 
            "replace", "ireplace", "creplace", "contains", "icontains", "ccontains", "notcontains", "inotcontains", "cnotcontains", "in", "iin", "cin", "notin", "inotin", "cnotin", "split", 
            "isplit", "csplit", "isnot", "is", "as", "f", "and", "band", "or", "bor", "xor", "bxor", "join", "shl", "shr"
         };
        private static readonly TokenKind[] _operatorTokenKind = new TokenKind[] { 
            TokenKind.Bnot, TokenKind.Not, TokenKind.Ieq, TokenKind.Ieq, TokenKind.Ceq, TokenKind.Ine, TokenKind.Ine, TokenKind.Cne, TokenKind.Ige, TokenKind.Ige, TokenKind.Cge, TokenKind.Igt, TokenKind.Igt, TokenKind.Cgt, TokenKind.Ilt, TokenKind.Ilt, 
            TokenKind.Clt, TokenKind.Ile, TokenKind.Ile, TokenKind.Cle, TokenKind.Ilike, TokenKind.Ilike, TokenKind.Clike, TokenKind.Inotlike, TokenKind.Inotlike, TokenKind.Cnotlike, TokenKind.Imatch, TokenKind.Imatch, TokenKind.Cmatch, TokenKind.Inotmatch, TokenKind.Inotmatch, TokenKind.Cnotmatch, 
            TokenKind.Ireplace, TokenKind.Ireplace, TokenKind.Creplace, TokenKind.Icontains, TokenKind.Icontains, TokenKind.Ccontains, TokenKind.Inotcontains, TokenKind.Inotcontains, TokenKind.Cnotcontains, TokenKind.Iin, TokenKind.Iin, TokenKind.Cin, TokenKind.Inotin, TokenKind.Inotin, TokenKind.Cnotin, TokenKind.Isplit, 
            TokenKind.Isplit, TokenKind.Csplit, TokenKind.IsNot, TokenKind.Is, TokenKind.As, TokenKind.Format, TokenKind.And, TokenKind.Band, TokenKind.Or, TokenKind.Bor, TokenKind.Xor, TokenKind.Bxor, TokenKind.Join, TokenKind.Shl, TokenKind.Shr
         };
        private readonly Parser _parser;
        private PositionHelper _positionHelper;
        private string _script;
        private static readonly int _simpleBeginSigHash;
        private BitArray _skippedCharOffsets;
        private int _tokenStart;
        private const string assemblyToken = "assembly";
        private const string modulesToken = "modules";
        private const string PSSnapinToken = "pssnapin";
        private const string shellIDToken = "shellid";
        private const string versionToken = "version";

        static Tokenizer()
        {
            for (int i = 0; i < _keywordText.Length; i++)
            {
                _keywordTable.Add(_keywordText[i], _keywordTokenKind[i]);
            }
            for (int j = 0; j < _operatorText.Length; j++)
            {
                _operatorTable.Add(_operatorText[j], _operatorTokenKind[j]);
            }
            _simpleBeginSigHash = "sig#beginsignatureblock".Aggregate<char, int>(0, (current, t) => current + t);
        }

        internal Tokenizer(Parser parser)
        {
            this._parser = parser;
        }

        private bool AtEof()
        {
            return (this._currentIndex > this._script.Length);
        }

        private static char Backtick(char c)
        {
            char ch = c;
            if (ch <= 'b')
            {
                switch (ch)
                {
                    case 'a':
                        return '\a';

                    case 'b':
                        return '\b';

                    case '0':
                        return '\0';
                }
                return c;
            }
            switch (ch)
            {
                case 'r':
                    return '\r';

                case 's':
                case 'u':
                    return c;

                case 't':
                    return '\t';

                case 'v':
                    return '\v';

                case 'n':
                    return '\n';

                case 'f':
                    return '\f';
            }
            return c;
        }

        internal void CheckAstIsBeforeSignature(Ast ast)
        {
            if ((this._beginSignatureExtent != null) && (this._beginSignatureExtent.StartOffset < ast.Extent.StartOffset))
            {
                this.ReportError(ast.Extent, () => ParserStrings.TokenAfterEndOfValidScriptText, new object[0]);
            }
        }

        private Token CheckOperatorInCommandMode(char c, TokenKind tokenKind)
        {
            if (this.InCommandMode() && !this.PeekChar().ForceStartNewToken())
            {
                return this.ScanGenericToken(c);
            }
            return this.NewToken(tokenKind);
        }

        private Token CheckOperatorInCommandMode(char c1, char c2, TokenKind tokenKind)
        {
            if (this.InCommandMode() && !this.PeekChar().ForceStartNewToken())
            {
                StringBuilder sb = new StringBuilder(4);
                sb.Append(c1);
                sb.Append(c2);
                return this.ScanGenericToken(sb);
            }
            return this.NewToken(tokenKind);
        }

        internal InternalScriptExtent CurrentExtent()
        {
            int startOffset = this._tokenStart + this._nestedTokensAdjustment;
            int endOffset = this._currentIndex + this._nestedTokensAdjustment;
            if (this._skippedCharOffsets != null)
            {
                int num3 = this._nestedTokensAdjustment;
                while ((num3 < startOffset) && (num3 < this._skippedCharOffsets.Count))
                {
                    if (this._skippedCharOffsets[num3])
                    {
                        startOffset++;
                        endOffset++;
                    }
                    num3++;
                }
                while ((num3 < endOffset) && (num3 < this._skippedCharOffsets.Count))
                {
                    if (this._skippedCharOffsets[num3])
                    {
                        endOffset++;
                    }
                    num3++;
                }
            }
            return new InternalScriptExtent(this._positionHelper, startOffset, endOffset);
        }

        internal void FinishNestedScan(TokenizerState ts)
        {
            this._currentIndex = ts.CurrentIndex;
            this._nestedTokensAdjustment = ts.NestedTokensAdjustment;
            this._script = ts.Script;
            this._tokenStart = ts.TokenStart;
            this._skippedCharOffsets = ts.SkippedCharOffsets;
            this.TokenList = ts.TokenList;
        }

        internal string GetAssemblyNameSpec()
        {
            StringBuilder sb = new StringBuilder();
            this.ScanAssemblyNameSpecToken(sb);
            while (this.PeekChar() == ',')
            {
                this._tokenStart = this._currentIndex;
                sb.Append(", ");
                this.SkipChar();
                this.NewToken(TokenKind.Comma);
                this.ScanAssemblyNameSpecToken(sb);
                if (this.PeekChar() == '=')
                {
                    this._tokenStart = this._currentIndex;
                    sb.Append("=");
                    this.SkipChar();
                    this.NewToken(TokenKind.Equals);
                    this.ScanAssemblyNameSpecToken(sb);
                }
            }
            return sb.ToString();
        }

        private char GetChar()
        {
            int num = this._currentIndex++;
            if (num == this._script.Length)
            {
                return '\0';
            }
            return this._script[num];
        }

        internal Token GetInvokeMemberOpenParen()
        {
            if (this.PeekChar() == '(')
            {
                this._tokenStart = this._currentIndex;
                this.SkipChar();
                return this.NewToken(TokenKind.LParen);
            }
            return null;
        }

        internal Token GetLBracket()
        {
            int start = this._currentIndex;
            bool flag = false;
        Label_0009:
            this._tokenStart = this._currentIndex;
            char c = this.GetChar();
            switch (c)
            {
                case '#':
                    flag = true;
                    this.ScanLineComment();
                    goto Label_0009;

                case '.':
                case ':':
                    if (flag)
                    {
                        this.Resync(start);
                    }
                    else
                    {
                        this.UngetChar();
                    }
                    break;

                case '\t':
                case '\v':
                case '\f':
                case ' ':
                case '\x0085':
                case '\x00a0':
                    flag = true;
                    this.SkipWhiteSpace();
                    goto Label_0009;

                case '<':
                    if (this.PeekChar() != '#')
                    {
                        this.UngetChar();
                        break;
                    }
                    flag = false;
                    this.SkipChar();
                    this.ScanBlockComment();
                    goto Label_0009;

                case '[':
                    return this.NewToken(TokenKind.LBracket);

                default:
                    if (c.IsWhitespace())
                    {
                        flag = true;
                        this.SkipWhiteSpace();
                        goto Label_0009;
                    }
                    this.UngetChar();
                    break;
            }
            return null;
        }

        internal Token GetMemberAccessOperator(bool allowLBracket)
        {
            char c = this.PeekChar();
            while (c == '<')
            {
                this._tokenStart = this._currentIndex;
                this.SkipChar();
                if (this.PeekChar() == '#')
                {
                    this.SkipChar();
                    this.ScanBlockComment();
                    c = this.PeekChar();
                }
                else
                {
                    this.UngetChar();
                    return null;
                }
            }
            switch (c)
            {
                case '.':
                    this._tokenStart = this._currentIndex;
                    this.SkipChar();
                    c = this.PeekChar();
                    if (c != '.')
                    {
                        if (!this.InCommandMode() || ((!c.IsWhitespace() && (c != '\0')) && ((c != '\r') && (c != '\n'))))
                        {
                            return this.NewToken(TokenKind.Dot);
                        }
                        this.UngetChar();
                        return null;
                    }
                    this.UngetChar();
                    return null;

                case ':':
                    this._tokenStart = this._currentIndex;
                    this.SkipChar();
                    if (this.PeekChar() == ':')
                    {
                        this.SkipChar();
                        c = this.PeekChar();
                        if (!this.InCommandMode() || ((!c.IsWhitespace() && (c != '\0')) && ((c != '\r') && (c != '\n'))))
                        {
                            return this.NewToken(TokenKind.ColonColon);
                        }
                        this.UngetChar();
                        this.UngetChar();
                        return null;
                    }
                    this.UngetChar();
                    return null;
            }
            if ((c == '[') && allowLBracket)
            {
                this._tokenStart = this._currentIndex;
                this.SkipChar();
                return this.NewToken(TokenKind.LBracket);
            }
            return null;
        }

        internal int GetRestorePoint()
        {
            this._tokenStart = this._currentIndex;
            return this.CurrentExtent().StartOffset;
        }

        internal IScriptExtent GetScriptExtent()
        {
            return this.NewScriptExtent(0, this._script.Length);
        }

        internal ScriptRequirements GetScriptRequirements()
        {
            if (this.RequiresTokens == null)
            {
                return null;
            }
            Token[] tokenArray = this.RequiresTokens.ToArray();
            this.RequiresTokens = null;
            string requiredShellId = null;
            Version requiredVersion = null;
            List<ModuleSpecification> requiredModules = null;
            List<PSSnapInSpecification> list = null;
            List<string> requiredAssemblies = null;
            foreach (Token token in tokenArray)
            {
                InternalScriptExtent scriptExtent = new InternalScriptExtent(this._positionHelper, token.Extent.StartOffset + 1, token.Extent.EndOffset);
                TokenizerState ts = this.StartNestedScan(new UnscannedSubExprToken(scriptExtent, TokenFlags.None, scriptExtent.Text, null));
                CommandAst ast = this._parser.CommandRule() as CommandAst;
                this._parser._ungotToken = null;
                this.FinishNestedScan(ts);
                string snapinName = null;
                Version snapinVersion = null;
                if (ast != null)
                {
                    if (!string.Equals(ast.GetCommandName(), "requires", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ReportError(ast.Extent, () => DiscoveryExceptions.ScriptRequiresInvalidFormat, new object[0]);
                    }
                    bool snapinSpecified = false;
                    for (int i = 1; i < ast.CommandElements.Count; i++)
                    {
                        CommandParameterAst ast2 = ast.CommandElements[i] as CommandParameterAst;
                        if ((ast2 != null) && "pssnapin".StartsWith(ast2.ParameterName, StringComparison.OrdinalIgnoreCase))
                        {
                            snapinSpecified = true;
                            if (list == null)
                            {
                                list = new List<PSSnapInSpecification>();
                            }
                            break;
                        }
                    }
                    for (int j = 1; j < ast.CommandElements.Count; j++)
                    {
                        CommandParameterAst parameter = ast.CommandElements[j] as CommandParameterAst;
                        if (parameter != null)
                        {
                            this.HandleRequiresParameter(parameter, ast.CommandElements, snapinSpecified, ref j, ref snapinName, ref snapinVersion, ref requiredShellId, ref requiredVersion, ref requiredModules, ref requiredAssemblies);
                        }
                        else
                        {
                            this.ReportError(ast.CommandElements[j].Extent, () => DiscoveryExceptions.ScriptRequiresInvalidFormat, new object[0]);
                        }
                    }
                    if (snapinName != null)
                    {
                        PSSnapInSpecification item = new PSSnapInSpecification(snapinName) {
                            Version = snapinVersion
                        };
                        list.Add(item);
                    }
                }
            }
            return new ScriptRequirements { RequiredApplicationId = requiredShellId, RequiredPSVersion = requiredVersion, RequiresPSSnapIns = (list != null) ? new ReadOnlyCollection<PSSnapInSpecification>(list) : ScriptRequirements.EmptySnapinCollection, RequiredAssemblies = (requiredAssemblies != null) ? new ReadOnlyCollection<string>(requiredAssemblies) : ScriptRequirements.EmptyAssemblyCollection, RequiredModules = (requiredModules != null) ? new ReadOnlyCollection<ModuleSpecification>(requiredModules) : ScriptRequirements.EmptyModuleCollection };
        }

        internal StringToken GetVerbatimCommandArgument()
        {
            char ch;
            this.SkipWhiteSpace();
            this._tokenStart = this._currentIndex;
            bool flag = false;
        Label_0014:
            ch = this.GetChar();
            if (((ch == '\r') || (ch == '\n')) || ((ch == '\0') && this.AtEof()))
            {
                this.UngetChar();
            }
            else
            {
                if (ch.IsDoubleQuote())
                {
                    flag = !flag;
                    goto Label_0014;
                }
                if (flag || ((ch != '|') && (((ch != '&') || this.AtEof()) || (this.PeekChar() != '&'))))
                {
                    goto Label_0014;
                }
                this.UngetChar();
            }
            string text = this.CurrentExtent().Text;
            return this.NewStringLiteralToken(text, TokenKind.Generic, TokenFlags.None);
        }

        private List<string> HandleRequiresAssemblyArgument(Ast argumentAst, object arg, List<string> requiredAssemblies)
        {
            if (!(arg is string))
            {
                this.ReportError(argumentAst.Extent, () => ParserStrings.RequiresInvalidStringArgument, new object[] { "assembly" });
                return requiredAssemblies;
            }
            if (requiredAssemblies == null)
            {
                requiredAssemblies = new List<string>();
            }
            requiredAssemblies.Add((string) arg);
            return requiredAssemblies;
        }

        private void HandleRequiresParameter(CommandParameterAst parameter, ReadOnlyCollection<CommandElementAst> commandElements, bool snapinSpecified, ref int index, ref string snapinName, ref Version snapinVersion, ref string requiredShellId, ref Version requiredVersion, ref List<ModuleSpecification> requiredModules, ref List<string> requiredAssemblies)
        {
            int num = 0;
            Ast ast = parameter.Argument ?? (((index + 1) < commandElements.Count) ? ((Ast) commandElements[num]) : null);
            if (ast == null)
            {
                this.ReportError(parameter.Extent, () => ParserStrings.ParameterRequiresArgument, new object[] { parameter.ParameterName });
            }
            else
            {
                object obj2;
                if (!IsConstantValueVisitor.IsConstant(ast, out obj2, false, true))
                {
                    this.ReportError(ast.Extent, () => ParserStrings.RequiresArgumentMustBeConstant, new object[0]);
                }
                else if ("shellid".StartsWith(parameter.ParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    if (requiredShellId != null)
                    {
                        object[] args = new object[2];
                        args[1] = "shellid";
                        this.ReportError(parameter.Extent, () => ParameterBinderStrings.ParameterAlreadyBound, args);
                    }
                    else if (!(obj2 is string))
                    {
                        this.ReportError(ast.Extent, () => ParserStrings.RequiresInvalidStringArgument, new object[] { "shellid" });
                    }
                    else
                    {
                        requiredShellId = (string) obj2;
                    }
                }
                else if ("pssnapin".StartsWith(parameter.ParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!(obj2 is string))
                    {
                        this.ReportError(ast.Extent, () => ParserStrings.RequiresInvalidStringArgument, new object[] { "pssnapin" });
                    }
                    else if (snapinName != null)
                    {
                        object[] objArray6 = new object[2];
                        objArray6[1] = "pssnapin";
                        this.ReportError(parameter.Extent, () => ParameterBinderStrings.ParameterAlreadyBound, objArray6);
                    }
                    else if (!PSSnapInInfo.IsPSSnapinIdValid((string) obj2))
                    {
                        this.ReportError(ast.Extent, () => MshSnapInCmdletResources.InvalidPSSnapInName, new object[0]);
                    }
                    else
                    {
                        snapinName = (string) obj2;
                    }
                }
                else if ("version".StartsWith(parameter.ParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    string versionString = obj2 as string;
                    if (versionString == null)
                    {
                        versionString = ast.Extent.Text;
                    }
                    Version version = Utils.StringToVersion(versionString);
                    if (version == null)
                    {
                        this.ReportError(ast.Extent, () => ParserStrings.RequiresVersionInvalid, new object[0]);
                    }
                    else if (snapinSpecified)
                    {
                        if (snapinVersion != null)
                        {
                            object[] objArray7 = new object[2];
                            objArray7[1] = "version";
                            this.ReportError(parameter.Extent, () => ParameterBinderStrings.ParameterAlreadyBound, objArray7);
                        }
                        else
                        {
                            snapinVersion = version;
                        }
                    }
                    else if ((requiredVersion != null) && !requiredVersion.Equals(version))
                    {
                        object[] objArray8 = new object[2];
                        objArray8[1] = "version";
                        this.ReportError(parameter.Extent, () => ParameterBinderStrings.ParameterAlreadyBound, objArray8);
                    }
                    else
                    {
                        requiredVersion = version;
                    }
                }
                else if ("assembly".StartsWith(parameter.ParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    if ((obj2 is string) || !(obj2 is IEnumerable))
                    {
                        requiredAssemblies = this.HandleRequiresAssemblyArgument(ast, obj2, requiredAssemblies);
                    }
                    else
                    {
                        foreach (object obj3 in (IEnumerable) obj2)
                        {
                            requiredAssemblies = this.HandleRequiresAssemblyArgument(ast, obj3, requiredAssemblies);
                        }
                    }
                }
                else if ("modules".StartsWith(parameter.ParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    object[] objArray = (obj2 as object[]) ?? new object[] { obj2 };
                    foreach (object obj4 in objArray)
                    {
                        ModuleSpecification specification;
                        try
                        {
                            specification = LanguagePrimitives.ConvertTo<ModuleSpecification>(obj4);
                        }
                        catch (InvalidCastException exception)
                        {
                            this.ReportError(ast.Extent, () => ParserStrings.RequiresModuleInvalid, new object[] { exception.Message });
                            return;
                        }
                        catch (ArgumentException exception2)
                        {
                            this.ReportError(ast.Extent, () => ParserStrings.RequiresModuleInvalid, new object[] { exception2.Message });
                            return;
                        }
                        if (requiredModules == null)
                        {
                            requiredModules = new List<ModuleSpecification>();
                        }
                        requiredModules.Add(specification);
                    }
                }
                else
                {
                    this.ReportError(parameter.Extent, () => DiscoveryExceptions.ScriptRequiresInvalidFormat, new object[0]);
                }
            }
        }

        private bool InCommandMode()
        {
            return (this.Mode == TokenizerMode.Command);
        }

        private bool InExpressionMode()
        {
            return (this.Mode == TokenizerMode.Expression);
        }

        internal void Initialize(string fileName, string input, List<Token> tokenList)
        {
            this._positionHelper = new PositionHelper(fileName, input);
            if (input == null) input = string.Empty;
            this._script = input;
            this.TokenList = tokenList;
            this.FirstToken = null;
            this.LastToken = null;
            this.RequiresTokens = null;
            List<int> list = new List<int>(100) { 0 };
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '\r':
                        if (((i + 1) < input.Length) && (input[i + 1] == '\n'))
                        {
                            i++;
                        }
                        list.Add(i + 1);
                        break;

                    case '\n':
                        list.Add(i + 1);
                        break;
                }
            }
            this._currentIndex = 0;
            this.Mode = TokenizerMode.Command;
            this._positionHelper.LineStartMap = list.ToArray();
        }

        private bool InTypeNameMode()
        {
            return (this.Mode == TokenizerMode.TypeName);
        }

        internal bool IsAtEndOfScript(IScriptExtent extent, bool checkCommentsAndWhitespace = false)
        {
            InternalScriptExtent extent2 = (InternalScriptExtent) extent;
            return ((extent2.EndOffset >= this._script.Length) || (checkCommentsAndWhitespace && this.OnlyWhitespaceOrCommentsAfterExtent(extent2)));
        }

        internal static bool IsKeyword(string str)
        {
            return _keywordTable.ContainsKey(str);
        }

        private Token NewCommentToken()
        {
            return this.SaveToken<Token>(new Token(this.CurrentExtent(), TokenKind.Comment, TokenFlags.None));
        }

        private Token NewFileRedirectionToken(int from, bool append, bool fromSpecifiedExplicitly)
        {
            if (!fromSpecifiedExplicitly || !this.InExpressionMode())
            {
                return this.SaveToken<FileRedirectionToken>(new FileRedirectionToken(this.CurrentExtent(), (RedirectionStream) from, append));
            }
            this.UngetChar();
            if (append)
            {
                this.UngetChar();
            }
            return this.NewNumberToken(from);
        }

        private Token NewGenericExpandableToken(string value, string formatString, List<Token> nestedTokens)
        {
            return this.NewStringExpandableToken(value, formatString, TokenKind.Generic, nestedTokens, TokenFlags.None);
        }

        private Token NewGenericToken(string value)
        {
            return this.NewStringLiteralToken(value, TokenKind.Generic, TokenFlags.None);
        }

        private Token NewInputRedirectionToken()
        {
            return this.SaveToken<InputRedirectionToken>(new InputRedirectionToken(this.CurrentExtent()));
        }

        private LabelToken NewLabelToken(string value)
        {
            return this.SaveToken<LabelToken>(new LabelToken(this.CurrentExtent(), TokenFlags.None, value));
        }

        private Token NewMergingRedirectionToken(int from, int to)
        {
            return this.SaveToken<MergingRedirectionToken>(new MergingRedirectionToken(this.CurrentExtent(), (RedirectionStream) from, (RedirectionStream) to));
        }

        private Token NewNumberToken(object value)
        {
            return this.SaveToken<NumberToken>(new NumberToken(this.CurrentExtent(), value, TokenFlags.None));
        }

        private Token NewParameterToken(string name, bool sawColon)
        {
            return this.SaveToken<ParameterToken>(new ParameterToken(this.CurrentExtent(), name, sawColon));
        }

        private InternalScriptExtent NewScriptExtent(int start, int end)
        {
            return new InternalScriptExtent(this._positionHelper, start + this._nestedTokensAdjustment, end + this._nestedTokensAdjustment);
        }

        private StringToken NewStringExpandableToken(string value, string formatString, TokenKind tokenKind, List<Token> nestedTokens, TokenFlags flags)
        {
            if ((nestedTokens != null) && !nestedTokens.Any<Token>())
            {
                nestedTokens = null;
            }
            else if (((flags & TokenFlags.TokenInError) == TokenFlags.None) && (from tok in nestedTokens
                where tok.HasError
                select tok).Any<Token>())
            {
                flags |= TokenFlags.TokenInError;
            }
            return this.SaveToken<StringExpandableToken>(new StringExpandableToken(this.CurrentExtent(), tokenKind, value, formatString, nestedTokens, flags));
        }

        private StringToken NewStringLiteralToken(string value, TokenKind tokenKind, TokenFlags flags)
        {
            return this.SaveToken<StringLiteralToken>(new StringLiteralToken(this.CurrentExtent(), flags, tokenKind, value));
        }

        private Token NewToken(TokenKind kind)
        {
            return this.SaveToken<Token>(new Token(this.CurrentExtent(), kind, TokenFlags.None));
        }

        private VariableToken NewVariableToken(VariablePath path, bool splatted)
        {
            return this.SaveToken<VariableToken>(new VariableToken(this.CurrentExtent(), path, TokenFlags.None, splatted));
        }

        internal Token NextToken()
        {
            char ch;
        Label_0000:
            this._tokenStart = this._currentIndex;
            char firstChar = this.GetChar();
            switch (firstChar)
            {
                case '\0':
                    if (!this.AtEof())
                    {
                        return this.ScanGenericToken(firstChar);
                    }
                    return this.SaveToken<Token>(new Token(this.NewScriptExtent(this._tokenStart + 1, this._tokenStart + 1), TokenKind.EndOfInput, TokenFlags.None));

                case '\t':
                case '\v':
                case '\f':
                case ' ':
                case '\x0085':
                case '\x00a0':
                    this.SkipWhiteSpace();
                    goto Label_0000;

                case '\n':
                case '\r':
                    this.ScanNewline(firstChar);
                    return this.NewToken(TokenKind.NewLine);

                case '!':
                    return this.CheckOperatorInCommandMode(firstChar, TokenKind.Exclaim);

                case '"':
                case '“':
                case '”':
                case '„':
                    return this.ScanStringExpandable();

                case '#':
                    this.ScanLineComment();
                    goto Label_0000;

                case '$':
                    if (this.PeekChar() != '(')
                    {
                        return this.ScanVariable(false, false);
                    }
                    this.SkipChar();
                    return this.NewToken(TokenKind.DollarParen);

                case '%':
                    ch = this.PeekChar();
                    if (ch != '=')
                    {
                        return this.CheckOperatorInCommandMode(firstChar, TokenKind.Rem);
                    }
                    this.SkipChar();
                    return this.CheckOperatorInCommandMode(firstChar, ch, TokenKind.RemainderEquals);

                case '&':
                    if (this.PeekChar() != '&')
                    {
                        return this.NewToken(TokenKind.Ampersand);
                    }
                    this.SkipChar();
                    return this.NewToken(TokenKind.AndAnd);

                case '\'':
                case '‘':
                case '’':
                case '‚':
                case '‛':
                    return this.ScanStringLiteral();

                case '(':
                    return this.NewToken(TokenKind.LParen);

                case ')':
                    return this.NewToken(TokenKind.RParen);

                case '*':
                    ch = this.PeekChar();
                    if (ch != '=')
                    {
                        if (ch != '>')
                        {
                            return this.CheckOperatorInCommandMode(firstChar, TokenKind.Multiply);
                        }
                        this.SkipChar();
                        ch = this.PeekChar();
                        switch (ch)
                        {
                            case '>':
                                this.SkipChar();
                                return this.NewFileRedirectionToken(0, true, false);

                            case '&':
                                this.SkipChar();
                                if (this.PeekChar() == '1')
                                {
                                    this.SkipChar();
                                    return this.NewMergingRedirectionToken(0, 1);
                                }
                                this.UngetChar();
                                break;
                        }
                        return this.NewFileRedirectionToken(0, false, false);
                    }
                    this.SkipChar();
                    return this.CheckOperatorInCommandMode(firstChar, ch, TokenKind.MultiplyEquals);

                case '+':
                    ch = this.PeekChar();
                    switch (ch)
                    {
                        case '+':
                            this.SkipChar();
                            return this.CheckOperatorInCommandMode(firstChar, ch, TokenKind.PlusPlus);

                        case '=':
                            this.SkipChar();
                            return this.CheckOperatorInCommandMode(firstChar, ch, TokenKind.PlusEquals);
                    }
                    if (!this.AllowSignedNumbers || (!char.IsDigit(ch) && (ch != '.')))
                    {
                        return this.CheckOperatorInCommandMode(firstChar, TokenKind.Plus);
                    }
                    return this.ScanNumber(firstChar);

                case ',':
                    return this.NewToken(TokenKind.Comma);

                case '-':
                case '–':
                case '—':
                case '―':
                    ch = this.PeekChar();
                    if (ch.IsDash())
                    {
                        this.SkipChar();
                        return this.CheckOperatorInCommandMode(firstChar, ch, TokenKind.MinusMinus);
                    }
                    if (ch == '=')
                    {
                        this.SkipChar();
                        return this.CheckOperatorInCommandMode(firstChar, ch, TokenKind.MinusEquals);
                    }
                    if ((!char.IsLetter(ch) && (ch != '_')) && (ch != '?'))
                    {
                        if (!this.AllowSignedNumbers || (!char.IsDigit(ch) && (ch != '.')))
                        {
                            return this.CheckOperatorInCommandMode(firstChar, TokenKind.Minus);
                        }
                        return this.ScanNumber(firstChar);
                    }
                    return this.ScanParameter();

                case '.':
                    return this.ScanDot();

                case '/':
                    ch = this.PeekChar();
                    if (ch != '=')
                    {
                        return this.CheckOperatorInCommandMode(firstChar, TokenKind.Divide);
                    }
                    this.SkipChar();
                    return this.CheckOperatorInCommandMode(firstChar, ch, TokenKind.DivideEquals);

                case '0':
                case '7':
                case '8':
                case '9':
                    return this.ScanNumber(firstChar);

                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                    if (this.PeekChar() != '>')
                    {
                        return this.ScanNumber(firstChar);
                    }
                    this.SkipChar();
                    ch = this.PeekChar();
                    switch (ch)
                    {
                        case '>':
                            this.SkipChar();
                            return this.NewFileRedirectionToken(firstChar - '0', true, true);

                        case '&':
                            this.SkipChar();
                            ch = this.PeekChar();
                            if ((ch == '1') || (ch == '2'))
                            {
                                this.SkipChar();
                                return this.NewMergingRedirectionToken(firstChar - '0', ch - '0');
                            }
                            this.UngetChar();
                            break;
                    }
                    return this.NewFileRedirectionToken(firstChar - '0', false, true);

                case ':':
                {
                    if (this.PeekChar() != ':')
                    {
                        return this.ScanLabel();
                    }
                    this.SkipChar();
                    if ((!this.InCommandMode() || this.WantSimpleName) || this.PeekChar().ForceStartNewToken())
                    {
                        return this.NewToken(TokenKind.ColonColon);
                    }
                    StringBuilder sb = new StringBuilder("::");
                    return this.ScanGenericToken(sb);
                }
                case ';':
                    return this.NewToken(TokenKind.Semi);

                case '<':
                    if (this.PeekChar() != '#')
                    {
                        return this.NewInputRedirectionToken();
                    }
                    this.SkipChar();
                    this.ScanBlockComment();
                    goto Label_0000;

                case '=':
                    return this.CheckOperatorInCommandMode(firstChar, TokenKind.Equals);

                case '>':
                    if (this.PeekChar() != '>')
                    {
                        return this.NewFileRedirectionToken(1, false, false);
                    }
                    this.SkipChar();
                    return this.NewFileRedirectionToken(1, true, false);

                case '@':
                    ch = this.GetChar();
                    switch (ch)
                    {
                        case '{':
                            return this.NewToken(TokenKind.AtCurly);

                        case '(':
                            return this.NewToken(TokenKind.AtParen);
                    }
                    if (ch.IsSingleQuote())
                    {
                        return this.ScanHereStringLiteral();
                    }
                    if (ch.IsDoubleQuote())
                    {
                        return this.ScanHereStringExpandable();
                    }
                    this.UngetChar();
                    if (ch.IsVariableStart())
                    {
                        return this.ScanVariable(true, false);
                    }
                    this.ReportError((int)(this._currentIndex - 1), () => ParserStrings.UnrecognizedToken, new object[0]);
                    return this.NewToken(TokenKind.Unknown);

                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '_':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    return this.ScanIdentifier(firstChar);

                case '[':
                    if (!this.InCommandMode() || this.PeekChar().ForceStartNewToken())
                    {
                        return this.NewToken(TokenKind.LBracket);
                    }
                    return this.ScanGenericToken('[');

                case ']':
                    return this.NewToken(TokenKind.RBracket);

                case '`':
                    ch = this.GetChar();
                    if ((ch != '\n') && (ch != '\r'))
                    {
                        if (!char.IsWhiteSpace(ch))
                        {
                            if ((ch != '\0') || !this.AtEof())
                            {
                                return this.ScanGenericToken(Backtick(ch));
                            }
                            this.ReportIncompleteInput(this._currentIndex, () => ParserStrings.IncompleteString, new object[0]);
                            this.UngetChar();
                        }
                        else
                        {
                            this.SkipWhiteSpace();
                        }
                    }
                    else
                    {
                        this.ScanNewline(ch);
                        this.NewToken(TokenKind.LineContinuation);
                    }
                    goto Label_0000;

                case '{':
                    return this.NewToken(TokenKind.LCurly);

                case '|':
                    if (this.PeekChar() != '|')
                    {
                        return this.NewToken(TokenKind.Pipe);
                    }
                    this.SkipChar();
                    return this.NewToken(TokenKind.OrOr);

                case '}':
                    return this.NewToken(TokenKind.RCurly);
            }
            if (firstChar.IsWhitespace())
            {
                this.SkipWhiteSpace();
                goto Label_0000;
            }
            if (char.IsLetter(firstChar))
            {
                return this.ScanIdentifier(firstChar);
            }
            return this.ScanGenericToken(firstChar);
        }

        private bool OnlyWhitespaceOrCommentsAfterExtent(InternalScriptExtent extent)
        {
            for (int i = extent.EndOffset; i < this._script.Length; i++)
            {
                if (this._script[i] == '#')
                {
                    i = this.SkipLineComment(i + 1) - 1;
                }
                else if (((this._script[i] == '<') && ((i + 1) < this._script.Length)) && (this._script[i + 1] == '#'))
                {
                    i = this.SkipBlockComment(i + 2) - 1;
                }
                else if (!this._script[i].IsWhitespace())
                {
                    return false;
                }
            }
            return true;
        }

        private char PeekChar()
        {
            if (this._currentIndex == this._script.Length)
            {
                return '\0';
            }
            return this._script[this._currentIndex];
        }

        internal void RemoveTokensFromListDuringResync(List<Token> tokenList, int start)
        {
            int index = 0;
            for (int i = tokenList.Count - 1; i >= 0; i--)
            {
                if (((InternalScriptExtent) tokenList[i].Extent).EndOffset <= start)
                {
                    index = i + 1;
                    break;
                }
            }
            tokenList.RemoveRange(index, tokenList.Count - index);
        }

        internal void ReplaceSavedTokens(Token firstOldToken, Token lastOldToken, Token newToken)
        {
            int startOffset = ((InternalScriptExtent) firstOldToken.Extent).StartOffset;
            int endOffset = ((InternalScriptExtent) lastOldToken.Extent).EndOffset;
            int num3 = -1;
            for (int i = this.TokenList.Count - 1; i >= 0; i--)
            {
                if (((InternalScriptExtent) this.TokenList[i].Extent).EndOffset == endOffset)
                {
                    num3 = i;
                }
                else if (((InternalScriptExtent) this.TokenList[i].Extent).StartOffset == startOffset)
                {
                    this.TokenList.RemoveRange(i, (num3 - i) + 1);
                    this.TokenList.Insert(i, newToken);
                    return;
                }
            }
        }

        private void ReportError(int errorOffset, Expression<Func<string>> message, params object[] args)
        {
            this._parser.ReportError(this.NewScriptExtent(errorOffset, errorOffset + 1), message, args);
        }

        private void ReportError(IScriptExtent extent, Expression<Func<string>> message, params object[] args)
        {
            this._parser.ReportError(extent, message, args);
        }

        private void ReportIncompleteInput(int errorOffset, Expression<Func<string>> message, params object[] args)
        {
            this._parser.ReportIncompleteInput(this.NewScriptExtent(errorOffset, this._currentIndex), message, args);
        }

        internal void Resync(int start)
        {
            int num = this._nestedTokensAdjustment;
            if (this._skippedCharOffsets != null)
            {
                for (int i = this._nestedTokensAdjustment; (i < (start - 1)) && (i < this._skippedCharOffsets.Count); i++)
                {
                    if (this._skippedCharOffsets[i])
                    {
                        num++;
                    }
                }
            }
            this._currentIndex = start - num;
            if ((this.FirstToken != null) && (this._currentIndex <= ((InternalScriptExtent) this.FirstToken.Extent).StartOffset))
            {
                this.FirstToken = null;
            }
            if ((this.TokenList != null) && this.TokenList.Any<Token>())
            {
                this.RemoveTokensFromListDuringResync(this.TokenList, start);
            }
            if ((this.RequiresTokens != null) && this.RequiresTokens.Any<Token>())
            {
                this.RemoveTokensFromListDuringResync(this.RequiresTokens, start);
            }
        }

        internal void Resync(Token token)
        {
            this.Resync(((InternalScriptExtent) token.Extent).StartOffset);
        }

        private T SaveToken<T>(T token) where T: Token
        {
            if (this.TokenList != null)
            {
                this.TokenList.Add(token);
            }
            switch (token.Kind)
            {
                case TokenKind.NewLine:
                case TokenKind.LineContinuation:
                case TokenKind.Comment:
                case TokenKind.EndOfInput:
                    return token;
            }
            if (this.FirstToken == null)
            {
                this.FirstToken = token;
            }
            this.LastToken = token;
            return token;
        }

        private bool ScanAfterHereStringHeader(string header)
        {
            char ch;
            int errorOffset = this._currentIndex - 2;
            do
            {
                ch = this.GetChar();
            }
            while (ch.IsWhitespace());
            switch (ch)
            {
                case '\r':
                case '\n':
                    this.ScanNewline(ch);
                    return true;

                default:
                    if ((ch == '\0') && this.AtEof())
                    {
                        this.UngetChar();
                        this.ReportIncompleteInput(errorOffset, () => ParserStrings.TerminatorExpectedAtEndOfString, new object[] { header[1] + '@' });
                        return false;
                    }
                    this.UngetChar();
                    this.ReportError(this._currentIndex, () => ParserStrings.UnexpectedCharactersAfterHereStringHeader, new object[0]);
                    do
                    {
                        ch = this.GetChar();
                        if ((ch == header[1]) && (this.PeekChar() == '@'))
                        {
                            this.SkipChar();
                            break;
                        }
                    }
                    while (((ch != '\r') && (ch != '\n')) && ((ch != '\0') || !this.AtEof()));
                    this.UngetChar();
                    break;
            }
            return false;
        }

        private void ScanAssemblyNameSpecToken(StringBuilder sb)
        {
            this.SkipWhiteSpace();
            this._tokenStart = this._currentIndex;
            while (true)
            {
                char c = this.GetChar();
                if (c.ForceStartNewTokenInAssemblyNameSpec())
                {
                    this.UngetChar();
                    break;
                }
                sb.Append(c);
            }
            Token token1 = this.NewToken(TokenKind.Identifier);
            token1.TokenFlags |= TokenFlags.TypeName;
            this.SkipWhiteSpace();
        }

        private void ScanBlockComment()
        {
            char ch;
            int errorOffset = this._currentIndex - 2;
        Label_0009:
            ch = this.GetChar();
            if ((ch == '#') && (this.PeekChar() == '>'))
            {
                this.SkipChar();
            }
            else
            {
                if ((ch == '\r') || (ch == '\n'))
                {
                    this.ScanNewline(ch);
                    goto Label_0009;
                }
                if ((ch != '\0') || !this.AtEof())
                {
                    goto Label_0009;
                }
                this.UngetChar();
                this.ReportIncompleteInput(errorOffset, () => ParserStrings.MissingTerminatorMultiLineComment, new object[0]);
            }
            this.NewCommentToken();
        }

        private int ScanDecimalDigits(StringBuilder sb)
        {
            int num = 0;
            for (char ch = this.PeekChar(); ch.IsDecimalDigit(); ch = this.PeekChar())
            {
                num++;
                this.SkipChar();
                sb.Append(ch);
            }
            return num;
        }

        private bool ScanDollarInStringExpandable(StringBuilder sb, StringBuilder formatSb, bool hereString, List<Token> nestedTokens)
        {
            int startIndex = this._currentIndex - 1;
            char c = this.PeekChar();
            int num2 = this._tokenStart;
            TokenizerMode mode = this.Mode;
            List<Token> tokenList = this.TokenList;
            Token item = null;
            try
            {
                this.TokenList = null;
                this.Mode = TokenizerMode.Expression;
                if (c == '(')
                {
                    this.SkipChar();
                    item = this.ScanSubExpression(hereString);
                }
                else if (c.IsVariableStart() || (c == '{'))
                {
                    this._tokenStart = this._currentIndex - 1;
                    item = this.ScanVariable(false, true);
                }
            }
            finally
            {
                this.TokenList = tokenList;
                this._tokenStart = num2;
                this.Mode = mode;
            }
            if (item != null)
            {
                sb.Append(this._script, startIndex, this._currentIndex - startIndex);
                formatSb.Append('{');
                formatSb.Append(nestedTokens.Count);
                formatSb.Append('}');
                nestedTokens.Add(item);
                return true;
            }
            return false;
        }

        private Token ScanDot()
        {
            char c = this.PeekChar();
            if (c == '.')
            {
                this.SkipChar();
                c = this.PeekChar();
                if (this.InCommandMode() && !c.ForceStartNewToken())
                {
                    this.UngetChar();
                    return this.ScanGenericToken('.');
                }
                return this.NewToken(TokenKind.DotDot);
            }
            if (c.IsDecimalDigit())
            {
                return this.ScanNumber('.');
            }
            if (((this.InCommandMode() && !c.ForceStartNewToken()) && ((c != '$') && (c != '"'))) && (c != '\''))
            {
                return this.ScanGenericToken('.');
            }
            return this.NewToken(TokenKind.Dot);
        }

        private void ScanExponent(StringBuilder sb, ref int signIndex, ref bool notNumber)
        {
            char c = this.PeekChar();
            if ((c == '+') || c.IsDash())
            {
                this.SkipChar();
                signIndex = sb.Length;
                sb.Append(c);
            }
            if (this.ScanDecimalDigits(sb) == 0)
            {
                notNumber = true;
            }
        }

        private Token ScanGenericToken(char firstChar)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(firstChar);
            return this.ScanGenericToken(sb);
        }

        private Token ScanGenericToken(StringBuilder sb)
        {
            List<Token> nestedTokens = new List<Token>();
            StringBuilder formatSb = new StringBuilder(sb.ToString());
            for (char ch = this.GetChar(); !ch.ForceStartNewToken(); ch = this.GetChar())
            {
                if (ch == '`')
                {
                    char c = this.PeekChar();
                    if (c != '\0')
                    {
                        this.SkipChar();
                        ch = Backtick(c);
                    }
                }
                else
                {
                    if (ch.IsSingleQuote())
                    {
                        int length = sb.Length;
                        this.ScanStringLiteral(sb);
                        for (int i = length; i < sb.Length; i++)
                        {
                            formatSb.Append(sb[i]);
                        }
                        continue;
                    }
                    if (ch.IsDoubleQuote())
                    {
                        this.ScanStringExpandable(sb, formatSb, nestedTokens);
                        continue;
                    }
                    if ((ch == '$') && this.ScanDollarInStringExpandable(sb, formatSb, false, nestedTokens))
                    {
                        continue;
                    }
                }
                sb.Append(ch);
                formatSb.Append(ch);
                switch (ch)
                {
                    case '{':
                    case '}':
                        formatSb.Append(ch);
                        break;
                }
            }
            this.UngetChar();
            if (nestedTokens.Any<Token>())
            {
                return this.NewGenericExpandableToken(sb.ToString(), formatSb.ToString(), nestedTokens);
            }
            return this.NewGenericToken(sb.ToString());
        }

        private Token ScanHereStringExpandable()
        {
            char ch;
            int errorOffset = this._currentIndex - 2;
            if (!this.ScanAfterHereStringHeader("@\""))
            {
                return this.NewStringExpandableToken("", "", TokenKind.HereStringExpandable, null, TokenFlags.TokenInError);
            }
            TokenFlags none = TokenFlags.None;
            List<Token> nestedTokens = new List<Token>();
            int falseFooterOffset = -1;
            StringBuilder sb = new StringBuilder();
            StringBuilder formatSb = new StringBuilder();
            Action<char> appendChar = delegate (char c) {
                sb.Append(c);
                formatSb.Append(c);
            };
            if (this.ScanPossibleHereStringFooter(new Func<char, bool>(CharExtensions.IsDoubleQuote), appendChar, ref falseFooterOffset))
            {
                goto Label_024A;
            }
        Label_0082:
            ch = this.GetChar();
            switch (ch)
            {
                case '\r':
                case '\n':
                {
                    int length = sb.Length;
                    int num4 = formatSb.Length;
                    sb.Append(ch);
                    formatSb.Append(ch);
                    if ((ch == '\r') && (this.PeekChar() == '\n'))
                    {
                        this.SkipChar();
                        sb.Append('\n');
                        formatSb.Append('\n');
                    }
                    if (!this.ScanPossibleHereStringFooter(new Func<char, bool>(CharExtensions.IsDoubleQuote), appendChar, ref falseFooterOffset))
                    {
                        goto Label_0082;
                    }
                    sb.Length = length;
                    formatSb.Length = num4;
                    goto Label_024A;
                }
                case '$':
                    if (!this.ScanDollarInStringExpandable(sb, formatSb, true, nestedTokens))
                    {
                        break;
                    }
                    goto Label_0082;

                case '`':
                {
                    char ch2 = this.PeekChar();
                    if (ch2 != '\0')
                    {
                        this.SkipChar();
                        ch = Backtick(ch2);
                    }
                    break;
                }
                case '{':
                case '}':
                    formatSb.Append(ch);
                    break;
            }
            if ((ch != '\0') || !this.AtEof())
            {
                sb.Append(ch);
                formatSb.Append(ch);
                goto Label_0082;
            }
            this.UngetChar();
            if (falseFooterOffset != -1)
            {
                this.ReportIncompleteInput(falseFooterOffset, () => ParserStrings.WhitespaceBeforeHereStringFooter, new object[0]);
            }
            else
            {
                this.ReportIncompleteInput(errorOffset, () => ParserStrings.TerminatorExpectedAtEndOfString, new object[] { "\"@" });
            }
            none = TokenFlags.TokenInError;
        Label_024A:
            return this.NewStringExpandableToken(sb.ToString(), formatSb.ToString(), TokenKind.HereStringExpandable, nestedTokens, none);
        }

        private Token ScanHereStringLiteral()
        {
            char ch;
            int errorOffset = this._currentIndex - 2;
            int falseFooterOffset = -1;
            if (!this.ScanAfterHereStringHeader("@'"))
            {
                return this.NewStringLiteralToken("", TokenKind.HereStringLiteral, TokenFlags.TokenInError);
            }
            TokenFlags none = TokenFlags.None;
            StringBuilder sb = new StringBuilder();
            Action<char> appendChar = delegate (char c) {
                sb.Append(c);
            };
            if (this.ScanPossibleHereStringFooter(new Func<char, bool>(CharExtensions.IsSingleQuote), appendChar, ref falseFooterOffset))
            {
                goto Label_0181;
            }
        Label_0068:
            ch = this.GetChar();
            switch (ch)
            {
                case '\r':
                case '\n':
                {
                    int length = sb.Length;
                    sb.Append(ch);
                    if ((ch == '\r') && (this.PeekChar() == '\n'))
                    {
                        this.SkipChar();
                        sb.Append('\n');
                    }
                    if (!this.ScanPossibleHereStringFooter(new Func<char, bool>(CharExtensions.IsSingleQuote), appendChar, ref falseFooterOffset))
                    {
                        goto Label_0068;
                    }
                    sb.Length = length;
                    break;
                }
                default:
                    if ((ch != '\0') || !this.AtEof())
                    {
                        sb.Append(ch);
                        goto Label_0068;
                    }
                    this.UngetChar();
                    if (falseFooterOffset != -1)
                    {
                        this.ReportIncompleteInput(falseFooterOffset, () => ParserStrings.WhitespaceBeforeHereStringFooter, new object[0]);
                    }
                    else
                    {
                        this.ReportIncompleteInput(errorOffset, () => ParserStrings.TerminatorExpectedAtEndOfString, new object[] { "'@" });
                    }
                    none = TokenFlags.TokenInError;
                    break;
            }
        Label_0181:
            return this.NewStringLiteralToken(sb.ToString(), TokenKind.HereStringLiteral, none);
        }

        private void ScanHexDigits(StringBuilder sb)
        {
            for (char ch = this.PeekChar(); ch.IsHexDigit(); ch = this.PeekChar())
            {
                this.SkipChar();
                sb.Append(ch);
            }
        }

        private Token ScanIdentifier(char firstChar)
        {
            char ch;
            TokenKind kind;
            StringBuilder sb = new StringBuilder();
            sb.Append(firstChar);
            bool wantSimpleName = this.WantSimpleName;
        Label_0015:
            ch = this.GetChar();
            if (char.IsLetter(ch))
            {
                sb.Append(ch);
                goto Label_0015;
            }
            if ((ch == '_') || ch.IsDecimalDigit())
            {
                wantSimpleName = true;
                sb.Append(ch);
                goto Label_0015;
            }
            this.UngetChar();
            if (this.InTypeNameMode())
            {
                return this.ScanTypeName();
            }
            if ((!this.WantSimpleName && this.InCommandMode()) && !ch.ForceStartNewToken())
            {
                return this.ScanGenericToken(sb);
            }
            if (((wantSimpleName || !this.InCommandMode()) || !_keywordTable.TryGetValue(sb.ToString(), out kind)) || ((kind == TokenKind.InlineScript) && !this.InWorkflowContext))
            {
                return this.NewToken(TokenKind.Identifier);
            }
            return this.NewToken(kind);
        }

        private Token ScanLabel()
        {
            StringBuilder sb = new StringBuilder();
            char c = this.GetChar();
            if (c.IsIndentifierStart())
            {
                while (c.IsIndentifierFollow())
                {
                    sb.Append(c);
                    c = this.GetChar();
                }
                if (this.InCommandMode() && !c.ForceStartNewToken())
                {
                    sb.Insert(0, ':');
                    sb.Append(c);
                    return this.ScanGenericToken(sb);
                }
                this.UngetChar();
                return this.NewLabelToken(sb.ToString());
            }
            sb.Append(':');
            if (c == '\0')
            {
                this.UngetChar();
                return this.NewGenericToken(sb.ToString());
            }
            this.UngetChar();
            return this.ScanGenericToken(sb);
        }

        private void ScanLineComment()
        {
            bool flag;
            bool flag2;
            this.ScanToEndOfLine(out flag, out flag2);
            Token item = this.NewCommentToken();
            if (flag)
            {
                this._beginSignatureExtent = this.CurrentExtent();
            }
            else if (flag2 && (this._nestedTokensAdjustment == 0))
            {
                if (this.RequiresTokens == null)
                {
                    this.RequiresTokens = new List<Token>();
                }
                this.RequiresTokens.Add(item);
            }
        }

        private void ScanNewline(char c)
        {
            if ((c == '\r') && (this.PeekChar() == '\n'))
            {
                this.SkipChar();
            }
        }

        private Token ScanNumber(char firstChar)
        {
            char ch2;
            object obj2;
            bool notNumber = false;
            bool hex = false;
            bool real = false;
            char suffix = '\0';
            long multiplier = 1L;
            int signIndex = -1;
            StringBuilder sb = new StringBuilder();
            if (firstChar.IsDash() || (firstChar == '+'))
            {
                sb.Append(firstChar);
                firstChar = this.GetChar();
            }
            if (firstChar == '.')
            {
                sb.Append('.');
                this.ScanNumberAfterDot(sb, ref signIndex, ref notNumber);
                real = true;
            }
            else
            {
                ch2 = this.PeekChar();
                if ((firstChar == '0') && ((ch2 == 'x') || (ch2 == 'X')))
                {
                    this.SkipChar();
                    this.ScanHexDigits(sb);
                    if (sb.Length == 0)
                    {
                        notNumber = true;
                    }
                    hex = true;
                }
                else
                {
                    sb.Append(firstChar);
                    this.ScanDecimalDigits(sb);
                    ch2 = this.PeekChar();
                    switch (ch2)
                    {
                        case '.':
                            this.SkipChar();
                            if (this.PeekChar() == '.')
                            {
                                this.UngetChar();
                            }
                            else
                            {
                                sb.Append(ch2);
                                real = true;
                                this.ScanNumberAfterDot(sb, ref signIndex, ref notNumber);
                            }
                            break;

                        case 'E':
                        case 'e':
                            this.SkipChar();
                            sb.Append(ch2);
                            real = true;
                            this.ScanExponent(sb, ref signIndex, ref notNumber);
                            break;
                    }
                }
            }
            ch2 = this.PeekChar();
            if (ch2.IsTypeSuffix())
            {
                this.SkipChar();
                suffix = ch2;
                ch2 = this.PeekChar();
            }
            if (ch2.IsMultiplierStart())
            {
                this.SkipChar();
                switch (ch2)
                {
                    case 'k':
                    case 'K':
                        multiplier = 0x400L;
                        break;

                    case 'm':
                    case 'M':
                        multiplier = 0x100000L;
                        break;

                    case 'g':
                    case 'G':
                        multiplier = 0x40000000L;
                        break;

                    case 't':
                    case 'T':
                        multiplier = 0x10000000000L;
                        break;

                    case 'p':
                    case 'P':
                        multiplier = 0x4000000000000L;
                        break;
                }
                char ch3 = this.PeekChar();
                if ((ch3 == 'b') || (ch3 == 'B'))
                {
                    this.SkipChar();
                    ch2 = this.PeekChar();
                }
                else
                {
                    notNumber = true;
                }
            }
            if (!ch2.ForceStartNewToken() && (!this.InExpressionMode() || !ch2.ForceStartNewTokenAfterNumber()))
            {
                notNumber = true;
            }
            if (notNumber)
            {
                this._currentIndex = this._tokenStart;
                sb.Clear();
                return this.ScanGenericToken(sb);
            }
            if (((signIndex != -1) && (sb[signIndex] != '-')) && sb[signIndex].IsDash())
            {
                sb[signIndex] = '-';
            }
            if ((sb[0] != '-') && sb[0].IsDash())
            {
                sb[0] = '-';
            }
            if (!TryGetNumberValue(sb.ToString(), hex, real, suffix, multiplier, out obj2))
            {
                if (!this.InExpressionMode())
                {
                    this._currentIndex = this._tokenStart;
                    sb.Clear();
                    return this.ScanGenericToken(sb);
                }
                this.ReportError(this._currentIndex, () => ParserStrings.BadNumericConstant, new object[] { this._script.Substring(this._tokenStart, this._currentIndex - this._tokenStart) });
            }
            return this.NewNumberToken(obj2);
        }

        private void ScanNumberAfterDot(StringBuilder sb, ref int signIndex, ref bool notNumber)
        {
            this.ScanDecimalDigits(sb);
            char ch = this.PeekChar();
            switch (ch)
            {
                case 'e':
                case 'E':
                    this.SkipChar();
                    sb.Append(ch);
                    this.ScanExponent(sb, ref signIndex, ref notNumber);
                    break;
            }
        }

        private Token ScanParameter()
        {
            TokenKind kind;
            StringBuilder sb = new StringBuilder();
            bool flag = true;
            bool sawColon = false;
            while (flag)
            {
                char c = this.GetChar();
                if (c.IsWhitespace())
                {
                    this.UngetChar();
                    break;
                }
                switch (c)
                {
                    case '\0':
                    case '\n':
                    case '\r':
                    case '&':
                    case '(':
                    case ')':
                    case ',':
                    case '.':
                    case ';':
                    case '[':
                    case '{':
                    case '|':
                    case '}':
                    {
                        this.UngetChar();
                        flag = false;
                        continue;
                    }
                    case '"':
                    case '\'':
                    case '‘':
                    case '’':
                    case '‚':
                    case '‛':
                    case '“':
                    case '”':
                    case '„':
                    {
                        if (this.InCommandMode())
                        {
                            this.UngetChar();
                            sb.Insert(0, this._script[this._tokenStart]);
                            return this.ScanGenericToken(sb);
                        }
                        this.UngetChar();
                        flag = false;
                        continue;
                    }
                    case ':':
                    {
                        flag = false;
                        sawColon = true;
                        if (!this.InCommandMode())
                        {
                            this.UngetChar();
                        }
                        continue;
                    }
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    {
                        sb.Append(c);
                        continue;
                    }
                }
                if (this.InCommandMode())
                {
                    sb.Append(c);
                }
                else
                {
                    this.UngetChar();
                    flag = false;
                }
            }
            if (this.InExpressionMode() && _operatorTable.TryGetValue(sb.ToString(), out kind))
            {
                return this.NewToken(kind);
            }
            if (sb.Length == 0)
            {
                return this.NewToken(TokenKind.Minus);
            }
            return this.NewParameterToken(sb.ToString(), sawColon);
        }

        private bool ScanPossibleHereStringFooter(Func<char, bool> test, Action<char> appendChar, ref int falseFooterOffset)
        {
            char arg = this.GetChar();
            if (!test(arg) || (this.PeekChar() != '@'))
            {
                while (arg.IsWhitespace())
                {
                    appendChar(arg);
                    arg = this.GetChar();
                }
                if (((arg == '\r') || (arg == '\n')) || ((arg == '\0') && this.AtEof()))
                {
                    this.UngetChar();
                    return false;
                }
                if (test(arg) && (this.PeekChar() == '@'))
                {
                    appendChar(arg);
                    if (falseFooterOffset == -1)
                    {
                        falseFooterOffset = this._currentIndex - 1;
                    }
                    appendChar(this.GetChar());
                }
                else
                {
                    this.UngetChar();
                }
                return false;
            }
            this.SkipChar();
            return true;
        }

        private Token ScanStringExpandable()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder formatSb = new StringBuilder();
            List<Token> nestedTokens = new List<Token>();
            TokenFlags flags = this.ScanStringExpandable(sb, formatSb, nestedTokens);
            return this.NewStringExpandableToken(sb.ToString(), formatSb.ToString(), TokenKind.StringExpandable, nestedTokens, flags);
        }

        private TokenFlags ScanStringExpandable(StringBuilder sb, StringBuilder formatSb, List<Token> nestedTokens)
        {
            TokenFlags none = TokenFlags.None;
            int errorOffset = this._currentIndex - 1;
            char c = this.GetChar();
            while ((c != '\0') || !this.AtEof())
            {
                if (c.IsDoubleQuote())
                {
                    if (!this.PeekChar().IsDoubleQuote())
                    {
                        break;
                    }
                    c = this.GetChar();
                }
                else
                {
                    if (c == '$')
                    {
                        if (!this.ScanDollarInStringExpandable(sb, formatSb, false, nestedTokens))
                        {
                            goto Label_0061;
                        }
                        goto Label_0083;
                    }
                    if (c == '`')
                    {
                        char ch2 = this.PeekChar();
                        if (ch2 != '\0')
                        {
                            this.SkipChar();
                            c = Backtick(ch2);
                        }
                    }
                }
            Label_0061:
                if ((c == '{') || (c == '}'))
                {
                    formatSb.Append(c);
                }
                sb.Append(c);
                formatSb.Append(c);
            Label_0083:
                c = this.GetChar();
            }
            if (c == '\0')
            {
                this.UngetChar();
                this.ReportIncompleteInput(errorOffset, () => ParserStrings.TerminatorExpectedAtEndOfString, new object[] { "\"" });
                none = TokenFlags.TokenInError;
            }
            return none;
        }

        private Token ScanStringLiteral()
        {
            StringBuilder sb = new StringBuilder();
            TokenFlags flags = this.ScanStringLiteral(sb);
            return this.NewStringLiteralToken(sb.ToString(), TokenKind.StringLiteral, flags);
        }

        private TokenFlags ScanStringLiteral(StringBuilder sb)
        {
            int errorOffset = this._currentIndex - 1;
            TokenFlags none = TokenFlags.None;
            char c = this.GetChar();
            while ((c != '\0') || !this.AtEof())
            {
                if (c.IsSingleQuote())
                {
                    if (!this.PeekChar().IsSingleQuote())
                    {
                        break;
                    }
                    c = this.GetChar();
                }
                sb.Append(c);
                c = this.GetChar();
            }
            if (c == '\0')
            {
                this.UngetChar();
                this.ReportIncompleteInput(errorOffset, () => ParserStrings.TerminatorExpectedAtEndOfString, new object[] { "'" });
                none = TokenFlags.TokenInError;
            }
            return none;
        }

        private Token ScanSubExpression(bool hereString)
        {
            BitArray array;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            this._tokenStart = this._currentIndex - 2;
            StringBuilder builder = new StringBuilder("$(");
            int num = 1;
            TokenFlags none = TokenFlags.None;
            bool flag = true;
            List<int> source = new List<int>();
            while (flag)
            {
                char ch = this.GetChar();
                switch (ch)
                {
                    case '\0':
                        if (this.AtEof())
                        {
                            this.UngetChar();
                            this.ReportIncompleteInput(this._tokenStart, () => ParserStrings.IncompleteDollarSubexpressionReference, new object[0]);
                            none = TokenFlags.TokenInError;
                            flag = false;
                            continue;
                        }
                        break;

                    case '"':
                    case '`':
                    case '“':
                    case '”':
                    case '„':
                    {
                        char c = this.PeekChar();
                        if (!hereString && c.IsDoubleQuote())
                        {
                            this.SkipChar();
                            builder.Append(c);
                            source.Add((this._currentIndex - 2) + this._nestedTokensAdjustment);
                        }
                        else
                        {
                            builder.Append(ch);
                        }
                        continue;
                    }
                    case '(':
                    {
                        builder.Append(ch);
                        num++;
                        continue;
                    }
                    case ')':
                    {
                        builder.Append(ch);
                        if (--num == 0)
                        {
                            flag = false;
                        }
                        continue;
                    }
                }
                builder.Append(ch);
            }
            if (source.Count > 0)
            {
                array = new BitArray(source.Last<int>() + 1);
                foreach (int num2 in source)
                {
                    array.Set(num2, true);
                }
            }
            else
            {
                array = this._skippedCharOffsets;
            }
            return new UnscannedSubExprToken(this.CurrentExtent(), none, builder.ToString(), array);
        }

        private void ScanToEndOfLine(out bool sawBeginSig, out bool matchedRequires)
        {
            char ch;
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            matchedRequires = false;
        Label_0009:
            ch = this.GetChar();
            switch (ch)
            {
                case 'A':
                case 'a':
                    num2 += 0x61;
                    num3 = -1;
                    goto Label_0009;

                case 'B':
                case 'b':
                    num2 += 0x62;
                    num3 = -1;
                    goto Label_0009;

                case 'C':
                case 'c':
                    num2 += 0x63;
                    num3 = -1;
                    goto Label_0009;

                case 'E':
                case 'e':
                    num2 += 0x65;
                    if ((num3 != 1) && (num3 != 6))
                    {
                        num3 = -1;
                    }
                    else
                    {
                        num3++;
                    }
                    goto Label_0009;

                case 'G':
                case 'g':
                    num2 += 0x67;
                    num3 = -1;
                    goto Label_0009;

                case 'I':
                case 'i':
                    if (num3 != 4)
                    {
                        num3 = -1;
                        break;
                    }
                    num3++;
                    break;

                case 'K':
                case 'k':
                    num2 += 0x6b;
                    num3 = -1;
                    goto Label_0009;

                case 'L':
                case 'l':
                    num2 += 0x6c;
                    num3 = -1;
                    goto Label_0009;

                case 'N':
                case 'n':
                    num2 += 110;
                    num3 = -1;
                    goto Label_0009;

                case 'O':
                case 'o':
                    num2 += 0x6f;
                    num3 = -1;
                    goto Label_0009;

                case 'Q':
                case 'q':
                    num++;
                    if (num3 != 2)
                    {
                        num3 = -1;
                    }
                    else
                    {
                        num3++;
                    }
                    goto Label_0009;

                case 'R':
                case 'r':
                    num2 += 0x72;
                    if ((num3 != 0) && (num3 != 5))
                    {
                        num3 = -1;
                    }
                    else
                    {
                        num3++;
                    }
                    goto Label_0009;

                case 'S':
                case 's':
                    if (num3 != 7)
                    {
                        num3 = -1;
                    }
                    else
                    {
                        matchedRequires = true;
                    }
                    num2 += 0x73;
                    goto Label_0009;

                case 'T':
                case 't':
                    num2 += 0x74;
                    num3 = -1;
                    goto Label_0009;

                case 'U':
                case 'u':
                    num2 += 0x75;
                    if (num3 != 3)
                    {
                        num3 = -1;
                    }
                    else
                    {
                        num3++;
                    }
                    goto Label_0009;

                case '#':
                    num2 += 0x23;
                    num3 = -1;
                    goto Label_0009;

                case '\r':
                case '\n':
                    goto Label_0239;

                case '\0':
                    if (!this.AtEof())
                    {
                        goto Label_0251;
                    }
                    goto Label_0239;

                default:
                    goto Label_0251;
            }
            num2 += 0x69;
            goto Label_0009;
        Label_0239:
            this.UngetChar();
            sawBeginSig = (num2 == _simpleBeginSigHash) && (num < 3);
            return;
        Label_0251:
            num3 = -1;
            if (!ch.IsWhitespace())
            {
                num++;
            }
            goto Label_0009;
        }

        private Token ScanTypeName()
        {
            char ch;
            do
            {
                ch = this.GetChar();
            }
            while (((char.IsLetterOrDigit(ch) || (ch == '.')) || ((ch == '`') || (ch == '_'))) || (ch == '+'));
            this.UngetChar();
            Token token = this.NewToken(TokenKind.Identifier);
            token.TokenFlags |= TokenFlags.TypeName;
            return token;
        }

        private Token ScanVariable(bool splatted, bool inStringExpandable)
        {
            string str;
            int errorOffset = this._currentIndex;
            StringBuilder sb = new StringBuilder(20);
            char c = this.GetChar();
            if (c != '{')
            {
                bool flag;
                if (!c.IsVariableStart())
                {
                    this.UngetChar();
                    sb.Append('$');
                    return this.ScanGenericToken(sb);
                }
                sb.Append(c);
                switch (c)
                {
                    case '$':
                    case '?':
                    case '^':
                        if (this.InCommandMode() && !this.PeekChar().ForceStartNewToken())
                        {
                            this._currentIndex = this._tokenStart;
                            sb.Clear();
                            return this.ScanGenericToken(sb);
                        }
                        goto Label_054E;

                    default:
                        flag = true;
                        break;
                }
                while (flag)
                {
                    c = this.GetChar();
                    switch (c)
                    {
                        case '\0':
                        case '\t':
                        case '\n':
                        case '\r':
                        case ' ':
                        case '&':
                        case '(':
                        case ')':
                        case ',':
                        case '.':
                        case ';':
                        case '[':
                        case '{':
                        case '|':
                        case '}':
                        {
                            this.UngetChar();
                            flag = false;
                            continue;
                        }
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '?':
                        case 'A':
                        case 'B':
                        case 'C':
                        case 'D':
                        case 'E':
                        case 'F':
                        case 'G':
                        case 'H':
                        case 'I':
                        case 'J':
                        case 'K':
                        case 'L':
                        case 'M':
                        case 'N':
                        case 'O':
                        case 'P':
                        case 'Q':
                        case 'R':
                        case 'S':
                        case 'T':
                        case 'U':
                        case 'V':
                        case 'W':
                        case 'X':
                        case 'Y':
                        case 'Z':
                        case '_':
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                        case 'g':
                        case 'h':
                        case 'i':
                        case 'j':
                        case 'k':
                        case 'l':
                        case 'm':
                        case 'n':
                        case 'o':
                        case 'p':
                        case 'q':
                        case 'r':
                        case 's':
                        case 't':
                        case 'u':
                        case 'v':
                        case 'w':
                        case 'x':
                        case 'y':
                        case 'z':
                        {
                            sb.Append(c);
                            continue;
                        }
                        case ':':
                        {
                            if (this.PeekChar() != ':')
                            {
                                break;
                            }
                            this.UngetChar();
                            flag = false;
                            continue;
                        }
                        default:
                            goto Label_04CF;
                    }
                    sb.Append(c);
                    continue;
                Label_04CF:
                    if (char.IsLetterOrDigit(c))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        if (this.InCommandMode() && !c.ForceStartNewToken())
                        {
                            this._currentIndex = this._tokenStart;
                            sb.Clear();
                            return this.ScanGenericToken(sb);
                        }
                        this.UngetChar();
                        flag = false;
                    }
                }
                goto Label_054E;
            }
            while (true)
            {
                c = this.GetChar();
                switch (c)
                {
                    case '\0':
                        if (this.AtEof())
                        {
                            this.UngetChar();
                            goto Label_012A;
                        }
                        break;

                    case '"':
                    case '“':
                    case '”':
                    case '„':
                        if (inStringExpandable)
                        {
                            char ch3 = this.GetChar();
                            if ((ch3 == '\0') && this.AtEof())
                            {
                                this.UngetChar();
                                goto Label_012A;
                            }
                            if (ch3.IsDoubleQuote())
                            {
                                c = ch3;
                            }
                            else
                            {
                                this.UngetChar();
                            }
                        }
                        break;

                    case '{':
                        this.ReportError(this._currentIndex, () => ParserStrings.OpenBraceNeedsToBeBackTickedInVariableName, new object[0]);
                        break;

                    case '}':
                        goto Label_012A;

                    case '`':
                    {
                        char ch2 = this.GetChar();
                        if ((ch2 == '\0') && this.AtEof())
                        {
                            this.UngetChar();
                            goto Label_012A;
                        }
                        c = Backtick(ch2);
                        break;
                    }
                }
                sb.Append(c);
            }
        Label_012A:
            str = sb.ToString();
            if (c != '}')
            {
                this.ReportIncompleteInput(errorOffset, () => ParserStrings.IncompleteDollarVariableReference, new object[0]);
            }
            if (str.Length == 0)
            {
                if (c == '}')
                {
                    this.ReportError((int)(this._currentIndex - 1), () => ParserStrings.EmptyVariableReference, new object[0]);
                }
                str = ":Error:";
            }
            if (this.InCommandMode())
            {
                char ch4 = this.PeekChar();
                if ((!ch4.ForceStartNewToken() && (ch4 != '.')) && (ch4 != '['))
                {
                    this._currentIndex = this._tokenStart;
                    sb.Clear();
                    return this.ScanGenericToken(sb);
                }
            }
            VariablePath path = new VariablePath(str);
            if (string.IsNullOrEmpty(path.UnqualifiedPath))
            {
                this.ReportError(this.NewScriptExtent(this._tokenStart, this._currentIndex), () => ParserStrings.InvalidBracedVariableReference, new object[0]);
            }
            return this.NewVariableToken(path, false);
        Label_054E:
            path = new VariablePath(sb.ToString());
            if (string.IsNullOrEmpty(path.UnqualifiedPath))
            {
                Expression<Func<string>> expression;
                if (path.IsDriveQualified)
                {
                    expression = () => ParserStrings.InvalidVariableReferenceWithDrive;
                }
                else
                {
                    expression = () => ParserStrings.InvalidVariableReference;
                }
                this.ReportError(this.NewScriptExtent(this._tokenStart, this._currentIndex), expression, new object[0]);
            }
            return this.NewVariableToken(path, splatted);
        }

        private int SkipBlockComment(int i)
        {
            while (i < this._script.Length)
            {
                char ch = this._script[i];
                if (((ch == '#') && ((i + 1) < this._script.Length)) && (this._script[i + 1] == '>'))
                {
                    return (i + 2);
                }
                i++;
            }
            return i;
        }

        private void SkipChar()
        {
            this._currentIndex++;
        }

        private int SkipLineComment(int i)
        {
            while (i < this._script.Length)
            {
                switch (this._script[i])
                {
                    case '\r':
                    case '\n':
                        return i;
                }
                i++;
            }
            return i;
        }

        internal void SkipNewlines(bool skipSemis, bool v3)
        {
            char ch;
        Label_0000:
            ch = this.GetChar();
            switch (ch)
            {
                case '\t':
                case '\v':
                case '\f':
                case ' ':
                case '\x0085':
                case '\x00a0':
                    this.SkipWhiteSpace();
                    goto Label_0000;

                case '\n':
                case '\r':
                    if (v3)
                    {
                        this._parser.NoteV3FeatureUsed();
                    }
                    if (this.TokenList != null)
                    {
                        this._tokenStart = this._currentIndex - 1;
                        this.ScanNewline(ch);
                        this.NewToken(TokenKind.NewLine);
                    }
                    goto Label_0000;

                case '#':
                    this._tokenStart = this._currentIndex - 1;
                    this.ScanLineComment();
                    goto Label_0000;

                case ';':
                    if (!skipSemis)
                    {
                        break;
                    }
                    if (this.TokenList != null)
                    {
                        this._tokenStart = this._currentIndex - 1;
                        this.NewToken(TokenKind.Semi);
                    }
                    goto Label_0000;

                case '<':
                    if (this.PeekChar() != '#')
                    {
                        break;
                    }
                    this._tokenStart = this._currentIndex - 1;
                    this.SkipChar();
                    this.ScanBlockComment();
                    goto Label_0000;

                case '`':
                {
                    char c = this.GetChar();
                    switch (c)
                    {
                        case '\n':
                        case '\r':
                            this._tokenStart = this._currentIndex - 2;
                            this.ScanNewline(c);
                            this.NewToken(TokenKind.LineContinuation);
                            goto Label_0000;
                    }
                    if (char.IsWhiteSpace(c))
                    {
                        this.SkipWhiteSpace();
                        goto Label_0000;
                    }
                    this.UngetChar();
                    break;
                }
                default:
                    if (ch.IsWhitespace())
                    {
                        this.SkipWhiteSpace();
                        goto Label_0000;
                    }
                    break;
            }
            this.UngetChar();
        }

        private void SkipWhiteSpace()
        {
            while (this.PeekChar().IsWhitespace())
            {
                this.SkipChar();
            }
        }

        internal TokenizerState StartNestedScan(UnscannedSubExprToken nestedText)
        {
            TokenizerState state = new TokenizerState {
                CurrentIndex = this._currentIndex,
                NestedTokensAdjustment = this._nestedTokensAdjustment,
                Script = this._script,
                TokenStart = this._tokenStart,
                SkippedCharOffsets = this._skippedCharOffsets,
                TokenList = this.TokenList
            };
            this._currentIndex = 0;
            this._nestedTokensAdjustment = ((InternalScriptExtent) nestedText.Extent).StartOffset;
            this._script = nestedText.Value;
            this._tokenStart = 0;
            this._skippedCharOffsets = nestedText.SkippedCharOffsets;
            this.TokenList = (this.TokenList != null) ? new List<Token>() : null;
            return state;
        }

        private static bool TryGetNumberValue(string strNum, bool hex, bool real, char suffix, long multiplier, out object result)
        {
            try
            {
                long num3;
                BigInteger integer;
                TypeCode @decimal;
                int num4;
                NumberStyles style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
                if ((suffix == 'd') || (suffix == 'D'))
                {
                    decimal num;
                    if (decimal.TryParse(strNum, style, NumberFormatInfo.InvariantInfo, out num))
                    {
                        result = num * multiplier;
                        return true;
                    }
                    result = null;
                    return false;
                }
                if (real)
                {
                    double num2;
                    if (double.TryParse(strNum, style, (IFormatProvider) NumberFormatInfo.InvariantInfo, out num2))
                    {
                        if ((num2 == 0.0) && (strNum[0] == '-'))
                        {
                            num2 = 0.0;
                        }
                        if ((suffix == 'l') || (suffix == 'L'))
                        {
                            result = ((long) Convert.ChangeType(num2, typeof(long), CultureInfo.InvariantCulture)) * multiplier;
                        }
                        else
                        {
                            result = num2 * multiplier;
                        }
                        return true;
                    }
                    result = null;
                    return false;
                }
                if (hex && !strNum[0].IsHexDigit())
                {
                    if (strNum[0] == '-')
                    {
                        multiplier = 0L - multiplier;
                    }
                    strNum = strNum.Substring(1);
                }
                style = hex ? NumberStyles.AllowHexSpecifier : NumberStyles.AllowLeadingSign;
                if ((suffix == 'l') || (suffix == 'L'))
                {
                    if (long.TryParse(strNum, style, NumberFormatInfo.InvariantInfo, out num3))
                    {
                        result = num3 * multiplier;
                        return true;
                    }
                    result = null;
                    return false;
                }
                if (int.TryParse(strNum, style, NumberFormatInfo.InvariantInfo, out num4))
                {
                    @decimal = TypeCode.Int32;
                    integer = num4;
                }
                else if (long.TryParse(strNum, style, NumberFormatInfo.InvariantInfo, out num3))
                {
                    @decimal = TypeCode.Int64;
                    integer = num3;
                }
                else
                {
                    decimal num5;
                    if (decimal.TryParse(strNum, style, NumberFormatInfo.InvariantInfo, out num5))
                    {
                        @decimal = TypeCode.Decimal;
                        integer = (BigInteger) num5;
                    }
                    else
                    {
                        double num6;
                        if (!hex && double.TryParse(strNum, style, (IFormatProvider) NumberFormatInfo.InvariantInfo, out num6))
                        {
                            result = num6 * multiplier;
                            return true;
                        }
                        result = null;
                        return false;
                    }
                }
                integer *= multiplier;
                if (((integer >= -2147483648L) && (integer <= 0x7fffffffL)) && (@decimal <= TypeCode.Int32))
                {
                    result = (int) integer;
                }
                else if (((integer >= -9223372036854775808L) && (integer <= 0x7fffffffffffffffL)) && (@decimal <= TypeCode.Int64))
                {
                    result = (long) integer;
                }
                else if (((integer >= ((BigInteger) (-79228162514264337593543950335M))) && (integer <= ((BigInteger) 79228162514264337593543950335M))) && (@decimal <= TypeCode.Decimal))
                {
                    result = (decimal) integer;
                }
                else
                {
                    result = (double) integer;
                }
                return true;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            result = null;
            return false;
        }

        private void UngetChar()
        {
            this._currentIndex--;
        }

        internal bool AllowSignedNumbers { get; set; }

        internal Token FirstToken { get; private set; }

        internal bool InWorkflowContext { get; set; }

        internal Token LastToken { get; private set; }

        internal TokenizerMode Mode { get; set; }

        private List<Token> RequiresTokens { get; set; }

        internal List<Token> TokenList { get; set; }

        internal bool WantSimpleName { get; set; }
    }
}

