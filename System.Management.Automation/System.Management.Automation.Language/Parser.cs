using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Management.Automation.Language
{
    public sealed class Parser
    {
        internal const string VERBATIM_ARGUMENT = "--%";

        internal const string VERBATIM_PARAMETERNAME = "-%";

        private readonly Tokenizer _tokenizer;

        private readonly List<ParseError> _errorList;

        internal Token _ungotToken;

        private bool _disableCommaOperator;

        private bool _savingTokens;

        private string _previousFirstToken;

        private string _previousLastToken;

        internal List<ParseError> ErrorList
        {
            get
            {
                return this._errorList;
            }
        }

        internal bool ProduceV2Tokens
        {
            get;
            set;
        }

        internal Parser()
        {
            this._tokenizer = new Tokenizer(this);
            this._errorList = new List<ParseError>();
        }

        internal static IScriptExtent After(IScriptExtent extent)
        {
            InternalScriptExtent internalScriptExtent = (InternalScriptExtent)extent;
            int endOffset = internalScriptExtent.EndOffset;
            return new InternalScriptExtent(internalScriptExtent.PositionHelper, endOffset, endOffset);
        }

        private static IScriptExtent After(Ast ast)
        {
            return Parser.After(ast.Extent);
        }

        private static IScriptExtent After(Token token)
        {
            return Parser.After(token.Extent);
        }

        private ExpressionAst ArrayLiteralRule()
        {
            ExpressionAst errorExpressionAst = this.UnaryExpressionRule();
            ExpressionAst expressionAst = errorExpressionAst;
            Token token = this.PeekToken();
            if (token.Kind != TokenKind.Comma || this._disableCommaOperator)
            {
                return errorExpressionAst;
            }
            else
            {
                List<ExpressionAst> expressionAsts = new List<ExpressionAst>();
                expressionAsts.Add(errorExpressionAst);
                List<ExpressionAst> expressionAsts1 = expressionAsts;
                while (token.Kind == TokenKind.Comma)
                {
                    this.SkipToken();
                    this.SkipNewlines();
                    errorExpressionAst = this.UnaryExpressionRule();
                    if (errorExpressionAst != null)
                    {
                        expressionAsts1.Add(errorExpressionAst);
                        token = this.PeekToken();
                    }
                    else
                    {
                        object[] text = new object[1];
                        text[0] = token.Text;
                        this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingExpressionAfterToken, text);
                        errorExpressionAst = new ErrorExpressionAst(token.Extent, null);
                        expressionAsts1.Add(errorExpressionAst);
                        break;
                    }
                }
                return new ArrayLiteralAst(Parser.ExtentOf(expressionAst, errorExpressionAst), expressionAsts1);
            }
        }

        private void AttributeArgumentsRule(ICollection<ExpressionAst> positionalArguments, ICollection<NamedAttributeArgumentAst> namedArguments, ref IScriptExtent lastItemExtent)
        {
            ExpressionAst constantExpressionAst;
            bool flag = this._disableCommaOperator;
            Token token = null;
            HashSet<string> strs = new HashSet<string>();
            try
            {
                this._disableCommaOperator = true;
                while (true)
                {
                    this.SkipNewlines();
                    StringConstantExpressionAst stringConstantExpressionAst = this.SimpleNameRule();
                    bool flag1 = false;
                    if (stringConstantExpressionAst == null)
                    {
                        constantExpressionAst = this.ExpressionRule();
                    }
                    else
                    {
                        Token token1 = this.PeekToken();
                        if (token1.Kind != TokenKind.Equals)
                        {
                            constantExpressionAst = new ConstantExpressionAst(stringConstantExpressionAst.Extent, (object)(true));
                            flag1 = true;
                            this.NoteV3FeatureUsed();
                        }
                        else
                        {
                            token1 = this.NextToken();
                            this.SkipNewlines();
                            constantExpressionAst = this.ExpressionRule();
                            if (constantExpressionAst == null)
                            {
                                IScriptExtent scriptExtent = Parser.After(token1);
                                this.ReportIncompleteInput(scriptExtent, ParserStrings.MissingExpressionInNamedArgument, new object[0]);
                                constantExpressionAst = new ErrorExpressionAst(scriptExtent, null);
                                TokenKind[] tokenKindArray = new TokenKind[4];
                                tokenKindArray[0] = TokenKind.Comma;
                                tokenKindArray[1] = TokenKind.RParen;
                                tokenKindArray[2] = TokenKind.RBracket;
                                tokenKindArray[3] = TokenKind.NewLine;
                                this.SyncOnError(tokenKindArray);
                            }
                            lastItemExtent = constantExpressionAst.Extent;
                        }
                    }
                    if (stringConstantExpressionAst == null)
                    {
                        if (constantExpressionAst == null)
                        {
                            if (token != null)
                            {
                                IScriptExtent scriptExtent1 = Parser.After(token);
                                object[] objArray = new object[1];
                                objArray[0] = token.Kind.Text();
                                this.ReportIncompleteInput(scriptExtent1, ParserStrings.MissingExpressionAfterToken, objArray);
                                positionalArguments.Add(new ErrorExpressionAst(scriptExtent1, null));
                                lastItemExtent = scriptExtent1;
                            }
                        }
                        else
                        {
                            positionalArguments.Add(constantExpressionAst);
                            lastItemExtent = constantExpressionAst.Extent;
                        }
                    }
                    else
                    {
                        if (!strs.Contains(stringConstantExpressionAst.Value))
                        {
                            namedArguments.Add(new NamedAttributeArgumentAst(Parser.ExtentOf(stringConstantExpressionAst, constantExpressionAst), stringConstantExpressionAst.Value, constantExpressionAst, flag1));
                        }
                        else
                        {
                            object[] value = new object[1];
                            value[0] = stringConstantExpressionAst.Value;
                            this.ReportError(stringConstantExpressionAst.Extent, ParserStrings.DuplicateNamedArgument, value);
                        }
                    }
                    this.SkipNewlines();
                    token = this.PeekToken();
                    if (token.Kind != TokenKind.Comma)
                    {
                        break;
                    }
                    lastItemExtent = token.Extent;
                    this.SkipToken();
                }
            }
            finally
            {
                this._disableCommaOperator = flag;
            }
        }

        private List<AttributeBaseAst> AttributeListRule(bool inExpressionMode)
        {
            List<AttributeBaseAst> attributeBaseAsts = new List<AttributeBaseAst>();
            for (AttributeBaseAst i = this.AttributeRule(); i != null; i = this.AttributeRule())
            {
                attributeBaseAsts.Add(i);
                if (!inExpressionMode || i as AttributeAst != null)
                {
                    this.SkipNewlines();
                }
            }
            if (attributeBaseAsts.Any<AttributeBaseAst>())
            {
                return attributeBaseAsts;
            }
            else
            {
                return null;
            }
        }

        private AttributeBaseAst AttributeRule()
        {
            Token token = null;
            Token token1 = this.NextLBracket();
            if (token1 != null)
            {
                this.V3SkipNewlines();
                ITypeName typeName = this.TypeNameRule(out token);
                if (typeName != null)
                {
                    Token token2 = this.NextToken();
                    if (token2.Kind != TokenKind.LParen)
                    {
                        if (this.ProduceV2Tokens)
                        {
                            Token token3 = new Token((InternalScriptExtent)Parser.ExtentOf(token1, token2), TokenKind.Identifier, TokenFlags.TypeName);
                            this._tokenizer.ReplaceSavedTokens(token1, token2, token3);
                        }
                        if (token2.Kind != TokenKind.RBracket)
                        {
                            this.UngetToken(token2);
                            this.ReportError(Parser.Before(token2), ParserStrings.EndSquareBracketExpectedAtEndOfAttribute, new object[0]);
                            token2 = null;
                        }
                        object[] extent = new object[2];
                        extent[0] = token2;
                        extent[1] = typeName.Extent;
                        return new TypeConstraintAst(Parser.ExtentOf(token1, Parser.ExtentFromFirstOf(extent)), typeName);
                    }
                    else
                    {
                        this.SkipNewlines();
                        List<ExpressionAst> expressionAsts = new List<ExpressionAst>();
                        List<NamedAttributeArgumentAst> namedAttributeArgumentAsts = new List<NamedAttributeArgumentAst>();
                        IScriptExtent scriptExtent = token2.Extent;
                        TokenizerMode mode = this._tokenizer.Mode;
                        try
                        {
                            this.SetTokenizerMode(TokenizerMode.Expression);
                            this.AttributeArgumentsRule(expressionAsts, namedAttributeArgumentAsts, ref scriptExtent);
                        }
                        finally
                        {
                            this.SetTokenizerMode(mode);
                        }
                        this.SkipNewlines();
                        Token token4 = this.NextToken();
                        if (token4.Kind != TokenKind.RParen)
                        {
                            this.UngetToken(token4);
                            token4 = null;
                            this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingEndParenthesisInExpression, new object[0]);
                        }
                        this.SkipNewlines();
                        Token token5 = this.NextToken();
                        if (token5.Kind != TokenKind.RBracket)
                        {
                            this.UngetToken(token5);
                            token5 = null;
                            if (token4 != null)
                            {
                                this.ReportIncompleteInput(Parser.After(token4), ParserStrings.EndSquareBracketExpectedAtEndOfAttribute, new object[0]);
                            }
                        }
                        Token tokenFlags = token;
                        tokenFlags.TokenFlags = tokenFlags.TokenFlags | TokenFlags.AttributeName;
                        object[] objArray = new object[3];
                        objArray[0] = token5;
                        objArray[1] = token4;
                        objArray[2] = scriptExtent;
                        return new AttributeAst(Parser.ExtentOf(token1, Parser.ExtentFromFirstOf(objArray)), typeName, expressionAsts, namedAttributeArgumentAsts);
                    }
                }
                else
                {
                    this.Resync(token1);
                    this.ReportIncompleteInput(Parser.After(token1), ParserStrings.MissingTypename, new object[0]);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        internal static IScriptExtent Before(IScriptExtent extent)
        {
            InternalScriptExtent internalScriptExtent = (InternalScriptExtent)extent;
            int startOffset = internalScriptExtent.StartOffset - 1;
            if (startOffset < 0)
            {
                startOffset = 0;
            }
            return new InternalScriptExtent(internalScriptExtent.PositionHelper, startOffset, startOffset);
        }

        private static IScriptExtent Before(Token token)
        {
            return Parser.Before(token.Extent);
        }

        private StatementAst BlockStatementRule(Token kindToken)
        {
            StatementBlockAst statementBlockAst = this.StatementBlockRule();
            if (statementBlockAst != null)
            {
                return new BlockStatementAst(Parser.ExtentOf(kindToken, statementBlockAst), kindToken, statementBlockAst);
            }
            else
            {
                object[] text = new object[1];
                text[0] = kindToken.Text;
                this.ReportIncompleteInput(Parser.After(kindToken.Extent), ParserStrings.MissingStatementAfterKeyword, text);
                return new ErrorStatementAst(Parser.ExtentOf(kindToken, kindToken), (IEnumerable<Ast>)null);
            }
        }

        private BreakStatementAst BreakStatementRule(Token breakToken)
        {
            IScriptExtent extent;
            ExpressionAst expressionAst = this.LabelOrKeyRule();
            if (expressionAst != null)
            {
                extent = Parser.ExtentOf(breakToken, expressionAst);
            }
            else
            {
                extent = breakToken.Extent;
            }
            IScriptExtent scriptExtent = extent;
            return new BreakStatementAst(scriptExtent, expressionAst);
        }

        private CatchClauseAst CatchBlockRule(ref IScriptExtent endErrorStatement, ref List<TypeConstraintAst> errorAsts)
        {
            IScriptExtent extent;
            this.SkipNewlines();
            Token token = this.NextToken();
            if (token.Kind == TokenKind.Catch)
            {
                List<TypeConstraintAst> typeConstraintAsts = null;
                Token token1 = null;
                while (true)
                {
                    int restorePoint = this._tokenizer.GetRestorePoint();
                    this.SkipNewlines();
                    AttributeBaseAst attributeBaseAst = this.AttributeRule();
                    if (attributeBaseAst != null)
                    {
                        TypeConstraintAst typeConstraintAst = attributeBaseAst as TypeConstraintAst;
                        if (typeConstraintAst != null)
                        {
                            if (typeConstraintAsts == null)
                            {
                                typeConstraintAsts = new List<TypeConstraintAst>();
                            }
                            typeConstraintAsts.Add(typeConstraintAst);
                            this.SkipNewlines();
                            token1 = this.PeekToken();
                            if (token1.Kind != TokenKind.Comma)
                            {
                                break;
                            }
                            this.SkipToken();
                        }
                        else
                        {
                            this.Resync(restorePoint);
                            break;
                        }
                    }
                    else
                    {
                        if (token1 == null)
                        {
                            break;
                        }
                        endErrorStatement = token1.Extent;
                        this.ReportIncompleteInput(Parser.After(endErrorStatement), ParserStrings.MissingTypeLiteralToken, new object[0]);
                        break;
                    }
                }
                StatementBlockAst statementBlockAst = this.StatementBlockRule();
                if (statementBlockAst != null)
                {
                    return new CatchClauseAst(Parser.ExtentOf(token, statementBlockAst), typeConstraintAsts, statementBlockAst);
                }
                else
                {
                    if (token1 == null || endErrorStatement != token1.Extent)
                    {
                        IScriptExtent scriptExtentPointer = endErrorStatement;
                        if (typeConstraintAsts != null)
                        {
                            extent = typeConstraintAsts.Last<TypeConstraintAst>().Extent;
                        }
                        else
                        {
                            extent = token.Extent;
                        }
                        scriptExtentPointer = extent;
                        this.ReportIncompleteInput(Parser.After(endErrorStatement), ParserStrings.MissingCatchHandlerBlock, new object[0]);
                    }
                    if (typeConstraintAsts != null)
                    {
                        if (errorAsts != null)
                        {
                            errorAsts.Concat<TypeConstraintAst>(typeConstraintAsts);
                        }
                        else
                        {
                            errorAsts = typeConstraintAsts;
                        }
                    }
                    return null;
                }
            }
            else
            {
                this.UngetToken(token);
                return null;
            }
        }

        private ExpressionAst CheckPostPrimaryExpressionOperators(Token token, ExpressionAst expr)
        {
            while (token != null)
            {
                this.V3SkipNewlines();
                if (token.Kind == TokenKind.Dot || token.Kind == TokenKind.ColonColon)
                {
                    expr = this.MemberAccessRule(expr, token);
                }
                else
                {
                    if (token.Kind == TokenKind.LBracket)
                    {
                        expr = this.ElementAccessRule(expr, token);
                    }
                }
                token = this.NextMemberAccessToken(true);
            }
            return expr;
        }

        private ExpressionAst CheckUsingVariable(VariableToken variableToken, bool withMemberAccess)
        {
            VariablePath variablePath = variableToken.VariablePath;
            if (!variablePath.IsDriveQualified || !variablePath.DriveName.Equals("using", StringComparison.OrdinalIgnoreCase) || variablePath.UnqualifiedPath.Length <= 0)
            {
                return new VariableExpressionAst(variableToken);
            }
            else
            {
                VariablePath variablePath1 = new VariablePath(variablePath.UnqualifiedPath);
                ExpressionAst variableExpressionAst = new VariableExpressionAst(variableToken.Extent, variablePath1, variableToken.Kind == TokenKind.SplattedVariable);
                if (withMemberAccess)
                {
                    variableExpressionAst = this.CheckPostPrimaryExpressionOperators(this.NextMemberAccessToken(true), variableExpressionAst);
                }
                return new UsingExpressionAst(variableExpressionAst.Extent, variableExpressionAst);
            }
        }

        internal CommandBaseAst CommandRule()
    {
        Token token;
        bool flag;
        bool flag2;
        IScriptExtent extent;
        bool flag3 = false;
        bool flag4 = false;
        RedirectionAst[] redirections = null;
        List<CommandElementAst> elements = new List<CommandElementAst>();
        TokenizerMode mode = this._tokenizer.Mode;
        try
        {
            CommandArgumentContext commandNameAfterInvoationOperator;
            this.SetTokenizerMode(TokenizerMode.Command);
            Token first = this.NextToken();
            token = first;
            extent = first.Extent;
            flag = false;
            flag2 = false;
            if (first.Kind == TokenKind.Dot)
            {
                flag = true;
                first = this.NextToken();
                commandNameAfterInvoationOperator = CommandArgumentContext.CommandNameAfterInvocationOperator;
            }
            else if (first.Kind == TokenKind.Ampersand)
            {
                flag2 = true;
                first = this.NextToken();
                commandNameAfterInvoationOperator = CommandArgumentContext.CommandNameAfterInvocationOperator;
            }
            else
            {
                commandNameAfterInvoationOperator = CommandArgumentContext.CommandName;
            }
            bool flag5 = true;
            while (flag5)
            {
                switch (first.Kind)
                {
                    case TokenKind.Parameter:
                        if (((commandNameAfterInvoationOperator & CommandArgumentContext.CommandName) != 0) || flag3)
                        {
                            extent = first.Extent;
                            first.TokenFlags |= TokenFlags.CommandName;
                            StringConstantExpressionAst item = new StringConstantExpressionAst(first.Extent, first.Text, StringConstantType.BareWord);
                            elements.Add(item);
                        }
                        else
                        {
                            ExpressionAst commandArgument;
                            IScriptExtent extent2;
                            ParameterToken token3 = (ParameterToken) first;
                            if (token3.UsedColon && (this.PeekToken().Kind != TokenKind.Comma))
                            {
                                commandArgument = this.GetCommandArgument(CommandArgumentContext.CommandArgument, this.NextToken());
                                if (commandArgument == null)
                                {
                                    extent2 = token3.Extent;
                                    this.ReportError(After(extent2), ParserStrings.ParameterRequiresArgument, new object[] { token3.Text });
                                }
                                else
                                {
                                    extent2 = ExtentOf(first, commandArgument);
                                }
                            }
                            else
                            {
                                commandArgument = null;
                                extent2 = first.Extent;
                            }
                            extent = extent2;
                            CommandParameterAst ast3 = new CommandParameterAst(extent2, token3.ParameterName, commandArgument, first.Extent);
                            elements.Add(ast3);
                        }
                        goto Label_0404;

                    case TokenKind.NewLine:
                    case TokenKind.RParen:
                    case TokenKind.RCurly:
                    case TokenKind.Semi:
                    case TokenKind.AndAnd:
                    case TokenKind.OrOr:
                    case TokenKind.Pipe:
                    case TokenKind.EndOfInput:
                    {
                        this.UngetToken(first);
                        flag5 = false;
                        continue;
                    }
                    case TokenKind.Ampersand:
                        extent = first.Extent;
                        this.ReportError(first.Extent, ParserStrings.MissingArgument, new object[0]);
                        goto Label_0404;

                    case TokenKind.Comma:
                        extent = first.Extent;
                        this.ReportError(first.Extent, ParserStrings.MissingArgument, new object[0]);
                        this.SkipNewlines();
                        goto Label_0404;

                    case TokenKind.MinusMinus:
                        extent = first.Extent;
                        elements.Add(flag3 ? ((CommandElementAst) new StringConstantExpressionAst(first.Extent, first.Text, StringConstantType.BareWord)) : ((CommandElementAst) new CommandParameterAst(first.Extent, "-", null, first.Extent)));
                        flag3 = true;
                        goto Label_0404;

                    case TokenKind.Redirection:
                    case TokenKind.RedirectInStd:
                        if ((commandNameAfterInvoationOperator & CommandArgumentContext.CommandName) != 0)
                        {
                            break;
                        }
                        if (redirections == null)
                        {
                            redirections = new RedirectionAst[7];
                        }
                        this.RedirectionRule((RedirectionToken) first, redirections, ref extent);
                        goto Label_0404;

                    default:
                    {
                        if ((first.Kind == TokenKind.InlineScript) && (commandNameAfterInvoationOperator == CommandArgumentContext.CommandName))
                        {
                            flag5 = this.InlineScriptRule(first, elements);
                            extent = elements.Last<CommandElementAst>().Extent;
                            if (flag5)
                            {
                                goto Label_0404;
                            }
                            continue;
                        }
                        ExpressionAst ast4 = this.GetCommandArgument(commandNameAfterInvoationOperator, first);
                        StringToken token4 = first as StringToken;
                        if ((token4 != null) && string.Equals(token4.Value, "--%", StringComparison.OrdinalIgnoreCase))
                        {
                            elements.Add(ast4);
                            extent = ast4.Extent;
                            StringToken verbatimCommandArgumentToken = this.GetVerbatimCommandArgumentToken();
                            if (verbatimCommandArgumentToken != null)
                            {
                                flag4 = true;
                                flag5 = false;
                                ast4 = new StringConstantExpressionAst(verbatimCommandArgumentToken.Extent, verbatimCommandArgumentToken.Value, StringConstantType.BareWord);
                                elements.Add(ast4);
                                extent = ast4.Extent;
                            }
                        }
                        else
                        {
                            extent = ast4.Extent;
                            elements.Add(ast4);
                        }
                        goto Label_0404;
                    }
                }
                extent = first.Extent;
                elements.Add(new StringConstantExpressionAst(first.Extent, first.Text, StringConstantType.BareWord));
            Label_0404:
                if (!flag4)
                {
                    commandNameAfterInvoationOperator = CommandArgumentContext.CommandArgument;
                    first = this.NextToken();
                }
            }
        }
        finally
        {
            this.SetTokenizerMode(mode);
        }
        if (elements.Count == 0)
        {
            if (flag || flag2)
            {
                IScriptExtent extent3 = token.Extent;
                this.ReportError(extent3, ParserStrings.MissingExpression, new object[] { token.Text });
                return new CommandExpressionAst(extent3, new ErrorExpressionAst(extent3, null), null);
            }
            return null;
        }
        return new CommandAst(ExtentOf(token, extent), elements, (flag || flag2) ? token.Kind : TokenKind.Unknown, (redirections != null) ? redirections.Where<RedirectionAst>(r => r != null) : null);
    }

        private ITypeName CompleteArrayTypeName(ITypeName elementType, TypeName typeForAssemblyQualification, Token firstTokenAfterLBracket)
        {
            Token token;
            Token token1;
            while (true)
            {
                TokenKind kind = firstTokenAfterLBracket.Kind;
                if (kind == TokenKind.EndOfInput)
                {
                    this.UngetToken(firstTokenAfterLBracket);
                    this.ReportError(Parser.Before(firstTokenAfterLBracket), ParserStrings.EndSquareBracketExpectedAtEndOfAttribute, new object[0]);
                }
                else
                {
                    if (kind == TokenKind.RBracket)
                    {
                        elementType = new ArrayTypeName(Parser.ExtentOf(elementType.Extent, firstTokenAfterLBracket.Extent), elementType, 1);
                    }
                    else
                    {
                        if (kind != TokenKind.Comma)
                        {
                            object[] text = new object[1];
                            text[0] = firstTokenAfterLBracket.Text;
                            this.ReportError(firstTokenAfterLBracket.Extent, ParserStrings.UnexpectedToken, text);
                            TokenKind[] tokenKindArray = new TokenKind[1];
                            tokenKindArray[0] = TokenKind.RBracket;
                            this.SyncOnError(tokenKindArray);
                        }
                        else
                        {
                            int num = 1;
                            token = firstTokenAfterLBracket;
                            do
                            {
                                token1 = token;
                                num++;
                                token = this.NextToken();
                            }
                            while (token.Kind == TokenKind.Comma);
                            if (token.Kind != TokenKind.RBracket)
                            {
                                this.UngetToken(token);
                                this.ReportError(Parser.After(token1), ParserStrings.EndSquareBracketExpectedAtEndOfAttribute, new object[0]);
                            }
                            elementType = new ArrayTypeName(Parser.ExtentOf(elementType.Extent, token.Extent), elementType, num);
                        }
                    }
                }
                token = this.PeekToken();
                if (token.Kind != TokenKind.Comma)
                {
                    if (token.Kind != TokenKind.LBracket)
                    {
                        break;
                    }
                    this.SkipToken();
                    firstTokenAfterLBracket = this.NextToken();
                }
                else
                {
                    this.SkipToken();
                    string assemblyNameSpec = this._tokenizer.GetAssemblyNameSpec();
                    if (!string.IsNullOrEmpty(assemblyNameSpec))
                    {
                        typeForAssemblyQualification.AssemblyName = assemblyNameSpec;
                        break;
                    }
                    else
                    {
                        this.ReportError(Parser.After(token), ParserStrings.MissingAssemblyNameSpecification, new object[0]);
                        break;
                    }
                }
            }
            return elementType;
        }

        private bool CompleteScriptBlockBody(Token lCurly, ref IScriptExtent bodyExtent, out IScriptExtent fullBodyExtent)
        {
            IScriptExtent extent;
            if (lCurly == null)
            {
                fullBodyExtent = this._tokenizer.GetScriptExtent();
                Token token = this.NextToken();
                if (token.Kind != TokenKind.EndOfInput)
                {
                    object[] text = new object[1];
                    text[0] = token.Text;
                    this.ReportError(token.Extent, ParserStrings.UnexpectedToken, text);
                    return false;
                }
            }
            else
            {
                Token token1 = this.NextToken();
                if (token1.Kind == TokenKind.RCurly)
                {
                    extent = token1.Extent;
                    if (bodyExtent == null && lCurly.Extent.EndColumnNumber != token1.Extent.StartColumnNumber)
                    {
                        bodyExtent = Parser.ExtentOf(Parser.After(lCurly), Parser.Before(token1));
                    }
                }
                else
                {
                    this.UngetToken(token1);
                    IScriptExtent scriptExtent = bodyExtent;
                    IScriptExtent extent1 = scriptExtent;
                    if (scriptExtent == null)
                    {
                        extent1 = lCurly.Extent;
                    }
                    extent = extent1;
                    this.ReportIncompleteInput(lCurly.Extent, token1.Extent, ParserStrings.MissingEndCurlyBrace, new object[0]);
                }
                fullBodyExtent = Parser.ExtentOf(lCurly, extent);
            }
            return true;
        }

        private ContinueStatementAst ContinueStatementRule(Token continueToken)
        {
            IScriptExtent extent;
            ExpressionAst expressionAst = this.LabelOrKeyRule();
            if (expressionAst != null)
            {
                extent = Parser.ExtentOf(continueToken, expressionAst);
            }
            else
            {
                extent = continueToken.Extent;
            }
            IScriptExtent scriptExtent = extent;
            return new ContinueStatementAst(scriptExtent, expressionAst);
        }

        private StatementAst DataStatementRule(Token dataToken)
        {
            Token token = null;
            string value;
            IScriptExtent extent;
            IScriptExtent scriptExtent;
            IScriptExtent extent1 = null;
            this.SkipNewlines();
            StringConstantExpressionAst stringConstantExpressionAst = this.SimpleNameRule();
            if (stringConstantExpressionAst != null)
            {
                value = stringConstantExpressionAst.Value;
            }
            else
            {
                value = null;
            }
            string str = value;
            this.SkipNewlines();
            Token token1 = this.PeekToken();
            List<ExpressionAst> expressionAsts = null;
            if (token1.Kind == TokenKind.Parameter)
            {
                this.SkipToken();
                if (!Parser.IsSpecificParameter(token1, "SupportedCommand"))
                {
                    extent1 = token1.Extent;
                    object[] parameterName = new object[1];
                    parameterName[0] = ((ParameterToken)token1).ParameterName;
                    this.ReportError(extent1, ParserStrings.InvalidParameterForDataSectionStatement, parameterName);
                }
                expressionAsts = new List<ExpressionAst>();
                while (true)
                {
                    this.SkipNewlines();
                    ExpressionAst singleCommandArgument = this.GetSingleCommandArgument(Parser.CommandArgumentContext.CommandName);
                    if (singleCommandArgument == null)
                    {
                        break;
                    }
                    expressionAsts.Add(singleCommandArgument);
                    token = this.PeekToken();
                    if (token.Kind != TokenKind.Comma)
                    {
                        goto Label0;
                    }
                    this.SkipToken();
                }
                if (extent1 == null)
                {
                    Parser parser = this;
                    Token token2 = token;
                    Token token3 = token2;
                    if (token2 == null)
                    {
                        token3 = token1;
                    }
                    parser.ReportIncompleteInput(Parser.After(token3), ParserStrings.MissingValueForSupportedCommandInDataSectionStatement, new object[0]);
                }
                if (token != null)
                {
                    scriptExtent = token.Extent;
                }
                else
                {
                    scriptExtent = token1.Extent;
                }
                extent1 = scriptExtent;
            }
        Label0:
            StatementBlockAst statementBlockAst = null;
            if (extent1 == null)
            {
                statementBlockAst = this.StatementBlockRule();
                if (statementBlockAst == null)
                {
                    if (expressionAsts != null)
                    {
                        extent = expressionAsts.Last<ExpressionAst>().Extent;
                    }
                    else
                    {
                        object[] objArray = new object[2];
                        objArray[0] = stringConstantExpressionAst;
                        objArray[1] = dataToken;
                        extent = Parser.ExtentFromFirstOf(objArray);
                    }
                    extent1 = extent;
                    this.ReportIncompleteInput(Parser.After(extent1), ParserStrings.MissingStatementBlockForDataSection, new object[0]);
                }
            }
            if (extent1 == null)
            {
                return new DataStatementAst(Parser.ExtentOf(dataToken, statementBlockAst), str, expressionAsts, statementBlockAst);
            }
            else
            {
                object[] objArray1 = new object[1];
                objArray1[0] = expressionAsts;
                return new ErrorStatementAst(Parser.ExtentOf(dataToken, extent1), Parser.GetNestedErrorAsts(objArray1));
            }
        }

        private StatementAst DoWhileStatementRule(LabelToken labelToken, Token doToken)
        {
            string labelText;
            LabelToken labelToken1 = labelToken;
            Token token = labelToken1;
            if (labelToken1 == null)
            {
                token = doToken;
            }
            IScriptExtent extent = token.Extent;
            IScriptExtent scriptExtent = null;
            Token token1 = null;
            Token token2 = null;
            PipelineBaseAst pipelineBaseAst = null;
            StatementBlockAst statementBlockAst = this.StatementBlockRule();
            if (statementBlockAst != null)
            {
                this.SkipNewlines();
                token2 = this.NextToken();
                if (token2.Kind == TokenKind.While || token2.Kind == TokenKind.Until)
                {
                    this.SkipNewlines();
                    Token token3 = this.NextToken();
                    if (token3.Kind == TokenKind.LParen)
                    {
                        this.SkipNewlines();
                        pipelineBaseAst = this.PipelineRule();
                        if (pipelineBaseAst == null)
                        {
                            scriptExtent = token3.Extent;
                            object[] objArray = new object[1];
                            objArray[0] = token2.Kind.Text();
                            this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingExpressionAfterKeyword, objArray);
                        }
                        this.SkipNewlines();
                        token1 = this.NextToken();
                        if (token1.Kind != TokenKind.RParen)
                        {
                            this.UngetToken(token1);
                            if (pipelineBaseAst != null)
                            {
                                scriptExtent = pipelineBaseAst.Extent;
                                object[] objArray1 = new object[1];
                                objArray1[0] = token2.Kind.Text();
                                this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingEndParenthesisAfterStatement, objArray1);
                            }
                        }
                    }
                    else
                    {
                        this.UngetToken(token3);
                        scriptExtent = token2.Extent;
                        object[] objArray2 = new object[1];
                        objArray2[0] = token2.Kind.Text();
                        this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingOpenParenthesisAfterKeyword, objArray2);
                    }
                }
                else
                {
                    this.UngetToken(token2);
                    scriptExtent = statementBlockAst.Extent;
                    this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingWhileOrUntilInDoWhile, new object[0]);
                }
            }
            else
            {
                scriptExtent = doToken.Extent;
                object[] objArray3 = new object[1];
                objArray3[0] = TokenKind.Do.Text();
                this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingLoopStatement, objArray3);
            }
            if (scriptExtent == null)
            {
                IScriptExtent scriptExtent1 = Parser.ExtentOf(extent, token1);
                if (labelToken != null)
                {
                    labelText = labelToken.LabelText;
                }
                else
                {
                    labelText = null;
                }
                string str = labelText;
                if (token2.Kind != TokenKind.Until)
                {
                    return new DoWhileStatementAst(scriptExtent1, str, pipelineBaseAst, statementBlockAst);
                }
                else
                {
                    return new DoUntilStatementAst(scriptExtent1, str, pipelineBaseAst, statementBlockAst);
                }
            }
            else
            {
                object[] objArray4 = new object[2];
                objArray4[0] = statementBlockAst;
                objArray4[1] = pipelineBaseAst;
                return new ErrorStatementAst(Parser.ExtentOf(extent, scriptExtent), Parser.GetNestedErrorAsts(objArray4));
            }
        }

        private ExpressionAst ElementAccessRule(ExpressionAst primaryExpression, Token lBracket)
        {
            this.SkipNewlines();
            ExpressionAst errorExpressionAst = this.ExpressionRule();
            if (errorExpressionAst == null)
            {
                IScriptExtent scriptExtent = Parser.After(lBracket);
                this.ReportIncompleteInput(scriptExtent, ParserStrings.MissingArrayIndexExpression, new object[0]);
                errorExpressionAst = new ErrorExpressionAst(lBracket.Extent, null);
            }
            this.SkipNewlines();
            Token token = this.NextToken();
            if (token.Kind != TokenKind.RBracket)
            {
                this.UngetToken(token);
                if (errorExpressionAst as ErrorExpressionAst == null)
                {
                    this.ReportIncompleteInput(Parser.After(errorExpressionAst), ParserStrings.MissingEndSquareBracket, new object[0]);
                }
                token = null;
            }
            object[] objArray = new object[2];
            objArray[0] = token;
            objArray[1] = errorExpressionAst;
            return new IndexExpressionAst(Parser.ExtentOf(primaryExpression, Parser.ExtentFromFirstOf(objArray)), primaryExpression, errorExpressionAst);
        }

        private ExpressionAst ErrorRecoveryParameterInExpression(ParameterToken paramToken, ExpressionAst expr)
        {
            object[] text = new object[1];
            text[0] = paramToken.Text;
            this.ReportError(paramToken.Extent, ParserStrings.UnexpectedToken, text);
            this.SkipToken();
            Ast[] commandParameterAst = new Ast[2];
            commandParameterAst[0] = expr;
            commandParameterAst[1] = new CommandParameterAst(paramToken.Extent, paramToken.ParameterName, null, paramToken.Extent);
            return new ErrorExpressionAst(Parser.ExtentOf(expr, paramToken), commandParameterAst);
        }

        private ExitStatementAst ExitStatementRule(Token token)
        {
            IScriptExtent extent;
            PipelineBaseAst pipelineBaseAst = this.PipelineRule();
            if (pipelineBaseAst != null)
            {
                extent = Parser.ExtentOf(token, pipelineBaseAst);
            }
            else
            {
                extent = token.Extent;
            }
            IScriptExtent scriptExtent = extent;
            return new ExitStatementAst(scriptExtent, pipelineBaseAst);
        }

        private ExpressionAst ExpandableStringRule(StringExpandableToken strToken)
        {
            ExpressionAst stringConstantExpressionAst;
            if (strToken.NestedTokens == null)
            {
                stringConstantExpressionAst = new StringConstantExpressionAst(strToken);
            }
            else
            {
                List<ExpressionAst> expressionAsts = this.ParseNestedExpressions(strToken);
                stringConstantExpressionAst = new ExpandableStringExpressionAst(strToken, strToken.Value, strToken.FormatString, expressionAsts);
            }
            return stringConstantExpressionAst;
        }

        private ExpressionAst ExpressionRule()
        {
            ExpressionAst expressionAst;
            ExpressionAst binaryExpressionAst;
            ParameterToken parameterToken;
            ExpressionAst expressionAst1;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            TokenizerMode mode = this._tokenizer.Mode;
            try
            {
                this.SetTokenizerMode(TokenizerMode.Expression);
                ExpressionAst errorExpressionAst = this.ArrayLiteralRule();
                if (errorExpressionAst != null)
                {
                    Token token = this.PeekToken();
                    if (token.Kind.HasTrait(TokenFlags.BinaryOperator))
                    {
                        this.SkipToken();
                        Stack<ExpressionAst> expressionAsts = new Stack<ExpressionAst>();
                        Stack<Token> tokens = new Stack<Token>();
                        expressionAsts.Push(errorExpressionAst);
                        tokens.Push(token);
                        int binaryPrecedence = token.Kind.GetBinaryPrecedence();
                        while (true)
                        {
                            this.SkipNewlines();
                            errorExpressionAst = this.ArrayLiteralRule();
                            if (errorExpressionAst == null)
                            {
                                IScriptExtent scriptExtent = Parser.After(token);
                                object[] text = new object[1];
                                text[0] = token.Text;
                                this.ReportIncompleteInput(scriptExtent, ParserStrings.ExpectedValueExpression, text);
                                errorExpressionAst = new ErrorExpressionAst(scriptExtent, null);
                            }
                            expressionAsts.Push(errorExpressionAst);
                            token = this.NextToken();
                            if (!token.Kind.HasTrait(TokenFlags.BinaryOperator))
                            {
                                break;
                            }
                            int num = token.Kind.GetBinaryPrecedence();
                            while (num <= binaryPrecedence)
                            {
                                binaryExpressionAst = expressionAsts.Pop();
                                expressionAst = expressionAsts.Pop();
                                Token token1 = tokens.Pop();
                                expressionAsts.Push(new BinaryExpressionAst(Parser.ExtentOf(expressionAst, binaryExpressionAst), expressionAst, token1.Kind, binaryExpressionAst, token1.Extent));
                                if (tokens.Count == 0)
                                {
                                    break;
                                }
                                binaryPrecedence = tokens.Peek().Kind.GetBinaryPrecedence();
                            }
                            tokens.Push(token);
                            binaryPrecedence = num;
                        }
                        parameterToken = token as ParameterToken;
                        this.UngetToken(token);
                        binaryExpressionAst = expressionAsts.Pop();
                        while (expressionAsts.Any<ExpressionAst>())
                        {
                            expressionAst = expressionAsts.Pop();
                            token = tokens.Pop();
                            binaryExpressionAst = new BinaryExpressionAst(Parser.ExtentOf(expressionAst, binaryExpressionAst), expressionAst, token.Kind, binaryExpressionAst, token.Extent);
                        }
                        if (parameterToken == null)
                        {
                            expressionAst1 = binaryExpressionAst;
                        }
                        else
                        {
                            expressionAst1 = this.ErrorRecoveryParameterInExpression(parameterToken, binaryExpressionAst);
                        }
                    }
                    else
                    {
                        parameterToken = token as ParameterToken;
                        if (parameterToken == null)
                        {
                            expressionAst1 = errorExpressionAst;
                        }
                        else
                        {
                            expressionAst1 = this.ErrorRecoveryParameterInExpression(parameterToken, errorExpressionAst);
                        }
                    }
                }
                else
                {
                    expressionAst1 = null;
                }
            }
            finally
            {
                this.SetTokenizerMode(mode);
            }
            return expressionAst1;
        }

        private static IScriptExtent ExtentFromFirstOf(object[] objs)
        {
            IScriptExtent extent;
            object[] objArray = objs;
            int num = 0;
            while (num < (int)objArray.Length)
            {
                object obj = objArray[num];
                if (obj == null)
                {
                    num++;
                }
                else
                {
                    Token token = obj as Token;
                    if (token == null)
                    {
                        Ast ast = obj as Ast;
                        if (ast == null)
                        {
                            ITypeName typeName = obj as ITypeName;
                            if (typeName == null)
                            {
                                extent = (IScriptExtent)obj;
                            }
                            else
                            {
                                extent = typeName.Extent;
                            }
                        }
                        else
                        {
                            extent = ast.Extent;
                        }
                    }
                    else
                    {
                        extent = token.Extent;
                    }
                    return extent;
                }
            }
            return PositionUtilities.EmptyExtent;
        }

        private static IScriptExtent ExtentOf(IScriptExtent first, IScriptExtent last)
        {
            if (first as EmptyScriptExtent == null)
            {
                if (last as EmptyScriptExtent == null)
                {
                    InternalScriptExtent internalScriptExtent = (InternalScriptExtent)first;
                    InternalScriptExtent internalScriptExtent1 = (InternalScriptExtent)last;
                    return new InternalScriptExtent(internalScriptExtent.PositionHelper, internalScriptExtent.StartOffset, internalScriptExtent1.EndOffset);
                }
                else
                {
                    return first;
                }
            }
            else
            {
                return last;
            }
        }

        private static IScriptExtent ExtentOf(Token first, Token last)
        {
            return Parser.ExtentOf(first.Extent, last.Extent);
        }

        private static IScriptExtent ExtentOf(Ast first, Ast last)
        {
            return Parser.ExtentOf(first.Extent, last.Extent);
        }

        private static IScriptExtent ExtentOf(Ast first, Token last)
        {
            return Parser.ExtentOf(first.Extent, last.Extent);
        }

        private static IScriptExtent ExtentOf(Token first, Ast last)
        {
            return Parser.ExtentOf(first.Extent, last.Extent);
        }

        private static IScriptExtent ExtentOf(IScriptExtent first, Ast last)
        {
            return Parser.ExtentOf(first, last.Extent);
        }

        private static IScriptExtent ExtentOf(IScriptExtent first, Token last)
        {
            return Parser.ExtentOf(first, last.Extent);
        }

        private static IScriptExtent ExtentOf(Ast first, IScriptExtent last)
        {
            return Parser.ExtentOf(first.Extent, last);
        }

        private static IScriptExtent ExtentOf(Token first, IScriptExtent last)
        {
            return Parser.ExtentOf(first.Extent, last);
        }

        private ITypeName FinishTypeNameRule(Token typeName, bool unBracketedGenericArg = false)
        {
            TypeName typeName1;
            Token token = this.PeekToken();
            if (token.Kind != TokenKind.LBracket)
            {
                if (token.Kind != TokenKind.Comma || unBracketedGenericArg)
                {
                    return new TypeName(typeName.Extent, typeName.Text);
                }
                else
                {
                    this.SkipToken();
                    string assemblyNameSpec = this._tokenizer.GetAssemblyNameSpec();
                    if (!string.IsNullOrWhiteSpace(assemblyNameSpec))
                    {
                        return new TypeName(Parser.ExtentOf(typeName.Extent, this._tokenizer.CurrentExtent()), typeName.Text, assemblyNameSpec);
                    }
                    else
                    {
                        this.ReportError(Parser.After(token), ParserStrings.MissingAssemblyNameSpecification, new object[0]);
                        return new TypeName(typeName.Extent, typeName.Text);
                    }
                }
            }
            else
            {
                Token token1 = token;
                this.SkipToken();
                this.V3SkipNewlines();
                token = this.NextToken();
                TokenKind kind = token.Kind;
                if (kind != TokenKind.Identifier)
                {
                    if (kind == TokenKind.LBracket)
                    {
                        return this.GenericTypeArgumentsRule(typeName, token, unBracketedGenericArg);
                    }
                    else if (kind == TokenKind.RBracket)
                    {
                        typeName1 = new TypeName(typeName.Extent, typeName.Text);
                        return this.CompleteArrayTypeName(typeName1, typeName1, token);
                    }
                    if (kind != TokenKind.Comma)
                    {
                        if (token.Kind == TokenKind.EndOfInput)
                        {
                            this.UngetToken(token);
                            this.ReportIncompleteInput(Parser.After(token1), ParserStrings.MissingTypename, new object[0]);
                        }
                        else
                        {
                            object[] text = new object[1];
                            text[0] = token.Text;
                            this.ReportError(token.Extent, ParserStrings.UnexpectedToken, text);
                            TokenKind[] tokenKindArray = new TokenKind[1];
                            tokenKindArray[0] = TokenKind.RBracket;
                            this.SyncOnError(tokenKindArray);
                        }
                        return new TypeName(typeName.Extent, typeName.Text);
                    }
                    typeName1 = new TypeName(typeName.Extent, typeName.Text);
                    return this.CompleteArrayTypeName(typeName1, typeName1, token);
                }
                return this.GenericTypeArgumentsRule(typeName, token, unBracketedGenericArg);
            }
        }

        private StatementAst ForeachStatementRule(LabelToken labelToken, Token forEachToken)
        {
            IScriptExtent extent;
            string labelText;
            if (labelToken != null)
            {
                extent = labelToken.Extent;
            }
            else
            {
                extent = forEachToken.Extent;
            }
            IScriptExtent scriptExtent = extent;
            IScriptExtent extent1 = null;
            this.SkipNewlines();
            Token token = this.PeekToken();
            ForEachFlags forEachFlag = ForEachFlags.None;
            while (token.Kind == TokenKind.Parameter)
            {
                this.SkipToken();
                if (!Parser.IsSpecificParameter(token, "parallel"))
                {
                    extent1 = token.Extent;
                    object[] parameterName = new object[1];
                    parameterName[0] = ((ParameterToken)token).ParameterName;
                    this.ReportError(token.Extent, ParserStrings.InvalidForeachFlag, parameterName);
                }
                else
                {
                    forEachFlag = forEachFlag | ForEachFlags.Parallel;
                }
                this.SkipNewlines();
                token = this.PeekToken();
            }
            Token token1 = this.NextToken();
            if (token1.Kind == TokenKind.LParen)
            {
                this.SkipNewlines();
                Token token2 = this.NextToken();
                if (token2.Kind == TokenKind.Variable || token2.Kind == TokenKind.SplattedVariable)
                {
                    VariableExpressionAst variableExpressionAst = new VariableExpressionAst((VariableToken)token2);
                    this.SkipNewlines();
                    PipelineBaseAst pipelineBaseAst = null;
                    StatementBlockAst statementBlockAst = null;
                    Token token3 = this.NextToken();
                    if (token3.Kind == TokenKind.In)
                    {
                        this.SkipNewlines();
                        pipelineBaseAst = this.PipelineRule();
                        if (pipelineBaseAst != null)
                        {
                            this.SkipNewlines();
                            Token token4 = this.NextToken();
                            if (token4.Kind == TokenKind.RParen)
                            {
                                statementBlockAst = this.StatementBlockRule();
                                if (statementBlockAst == null)
                                {
                                    extent1 = token4.Extent;
                                    this.ReportIncompleteInput(Parser.After(extent1), ParserStrings.MissingForeachStatement, new object[0]);
                                }
                            }
                            else
                            {
                                this.UngetToken(token4);
                                extent1 = pipelineBaseAst.Extent;
                                this.ReportIncompleteInput(Parser.After(extent1), ParserStrings.MissingEndParenthesisAfterForeach, new object[0]);
                            }
                        }
                        else
                        {
                            extent1 = token3.Extent;
                            this.ReportIncompleteInput(Parser.After(extent1), ParserStrings.MissingForeachExpression, new object[0]);
                        }
                    }
                    else
                    {
                        this.UngetToken(token3);
                        extent1 = variableExpressionAst.Extent;
                        this.ReportIncompleteInput(Parser.After(extent1), ParserStrings.MissingInInForeach, new object[0]);
                    }
                    if (extent1 == null)
                    {
                        IScriptExtent scriptExtent1 = Parser.ExtentOf(scriptExtent, statementBlockAst);
                        if (labelToken != null)
                        {
                            labelText = labelToken.LabelText;
                        }
                        else
                        {
                            labelText = null;
                        }
                        return new ForEachStatementAst(scriptExtent1, labelText, forEachFlag, variableExpressionAst, pipelineBaseAst, statementBlockAst);
                    }
                    else
                    {
                        object[] objArray = new object[3];
                        objArray[0] = variableExpressionAst;
                        objArray[1] = pipelineBaseAst;
                        objArray[2] = statementBlockAst;
                        return new ErrorStatementAst(Parser.ExtentOf(scriptExtent, extent1), Parser.GetNestedErrorAsts(objArray));
                    }
                }
                else
                {
                    this.UngetToken(token2);
                    extent1 = token1.Extent;
                    this.ReportIncompleteInput(Parser.After(extent1), ParserStrings.MissingVariableNameAfterForeach, new object[0]);
                    return new ErrorStatementAst(Parser.ExtentOf(scriptExtent, extent1), (IEnumerable<Ast>)null);
                }
            }
            else
            {
                this.UngetToken(token1);
                extent1 = forEachToken.Extent;
                object[] objArray1 = new object[1];
                objArray1[0] = forEachToken.Kind.Text();
                this.ReportIncompleteInput(Parser.After(extent1), ParserStrings.MissingOpenParenthesisAfterKeyword, objArray1);
				return new ErrorStatementAst(Parser.ExtentOf(scriptExtent, extent1), (IEnumerable<Ast>)null);
            }
        }

        private StatementAst ForStatementRule(LabelToken labelToken, Token forToken)
        {
            string labelText;
            IScriptExtent extent = null;
            this.SkipNewlines();
            Token token = this.NextToken();
            if (token.Kind == TokenKind.LParen)
            {
                this.SkipNewlines();
                PipelineBaseAst pipelineBaseAst = this.PipelineRule();
                if (pipelineBaseAst != null)
                {
                    extent = pipelineBaseAst.Extent;
                }
                if (this.PeekToken().Kind == TokenKind.Semi)
                {
                    extent = this.NextToken().Extent;
                }
                this.SkipNewlines();
                PipelineBaseAst pipelineBaseAst1 = this.PipelineRule();
                if (pipelineBaseAst1 != null)
                {
                    extent = pipelineBaseAst1.Extent;
                }
                if (this.PeekToken().Kind == TokenKind.Semi)
                {
                    extent = this.NextToken().Extent;
                }
                this.SkipNewlines();
                PipelineBaseAst pipelineBaseAst2 = this.PipelineRule();
                if (pipelineBaseAst2 != null)
                {
                    extent = pipelineBaseAst2.Extent;
                }
                this.SkipNewlines();
                Token token1 = this.NextToken();
                StatementBlockAst statementBlockAst = null;
                if (token1.Kind == TokenKind.RParen)
                {
                    statementBlockAst = this.StatementBlockRule();
                    if (statementBlockAst == null)
                    {
                        extent = token1.Extent;
                        object[] objArray = new object[1];
                        objArray[0] = forToken.Kind.Text();
                        this.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingLoopStatement, objArray);
                    }
                }
                else
                {
                    this.UngetToken(token1);
                    if (extent == null)
                    {
                        extent = token.Extent;
                    }
                    object[] objArray1 = new object[1];
                    objArray1[0] = forToken.Kind.Text();
                    this.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingEndParenthesisAfterStatement, objArray1);
                }
                if (statementBlockAst != null)
                {
                    LabelToken labelToken1 = labelToken;
                    Token token2 = labelToken1;
                    if (labelToken1 == null)
                    {
                        token2 = forToken;
                    }
                    IScriptExtent scriptExtent = Parser.ExtentOf(token2, statementBlockAst);
                    if (labelToken != null)
                    {
                        labelText = labelToken.LabelText;
                    }
                    else
                    {
                        labelText = null;
                    }
                    return new ForStatementAst(scriptExtent, labelText, pipelineBaseAst, pipelineBaseAst1, pipelineBaseAst2, statementBlockAst);
                }
                else
                {
                    LabelToken labelToken2 = labelToken;
                    Token token3 = labelToken2;
                    if (labelToken2 == null)
                    {
                        token3 = forToken;
                    }
                    object[] objArray2 = new object[3];
                    objArray2[0] = pipelineBaseAst;
                    objArray2[1] = pipelineBaseAst1;
                    objArray2[2] = pipelineBaseAst2;
                    return new ErrorStatementAst(Parser.ExtentOf(token3, extent), Parser.GetNestedErrorAsts(objArray2));
                }
            }
            else
            {
                this.UngetToken(token);
                extent = forToken.Extent;
                object[] objArray3 = new object[1];
                objArray3[0] = forToken.Kind.Text();
                this.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingOpenParenthesisAfterKeyword, objArray3);
                LabelToken labelToken3 = labelToken;
                Token token4 = labelToken3;
                if (labelToken3 == null)
                {
                    token4 = forToken;
                }
				return new ErrorStatementAst(Parser.ExtentOf(token4, extent), (IEnumerable<Ast>)null);
            }
        }

        private StatementAst FunctionDeclarationRule(Token functionToken)
        {
            IScriptExtent scriptExtent;
            Token token;
            Token token1;
            Token token2;
            bool kind;
            bool flag;
            bool inWorkflowContext;
            ScriptBlockAst scriptBlockAst;
            string str;
            FunctionDefinitionAst functionDefinitionAst;
            StatementAst statementAst;
            object[] text;
            object[] objArray;
            string value;
            IScriptExtent extent;
            List<ParameterAst> parameterAsts = null;
            this.SkipNewlines();
            Token token3 = this.NextToken();
            TokenKind tokenKind = token3.Kind;
            switch (tokenKind)
            {
                case TokenKind.Variable:
                case TokenKind.SplattedVariable:
                case TokenKind.EndOfInput:
                case TokenKind.StringLiteral:
                case TokenKind.StringExpandable:
                case TokenKind.HereStringLiteral:
                case TokenKind.HereStringExpandable:
                case TokenKind.LParen:
                case TokenKind.RParen:
                case TokenKind.LCurly:
                case TokenKind.RCurly:
                case TokenKind.AtParen:
                case TokenKind.AtCurly:
                case TokenKind.Semi:
                case TokenKind.AndAnd:
                case TokenKind.OrOr:
                case TokenKind.Ampersand:
                case TokenKind.Pipe:
                    {
                        this.UngetToken(token3);
                        text = new object[1];
                        text[0] = functionToken.Text;
                        this.ReportIncompleteInput(Parser.After(functionToken), ParserStrings.MissingNameAfterFunctionKeyword, text);
				return new ErrorStatementAst(functionToken.Extent, (IEnumerable<Ast>)null);
                    }
                case TokenKind.Parameter:
                case TokenKind.Number:
                case TokenKind.Label:
                case TokenKind.Identifier:
                case TokenKind.Generic:
                case TokenKind.NewLine:
                case TokenKind.LineContinuation:
                case TokenKind.Comment:
                case TokenKind.LBracket:
                case TokenKind.RBracket:
                case TokenKind.DollarParen:
                    {
                        scriptExtent = null;
                        this.SkipNewlines();
                        token = null;
                        token1 = this.PeekToken();
                        if (token1.Kind == TokenKind.LParen)
                        {
                            this.SkipToken();
                            parameterAsts = this.ParameterListRule();
                            this.SkipNewlines();
                            token = this.NextToken();
                            if (token.Kind != TokenKind.RParen)
                            {
                                this.UngetToken(token);
                                if (parameterAsts.Any<ParameterAst>())
                                {
                                    extent = parameterAsts.Last<ParameterAst>().Extent;
                                }
                                else
                                {
                                    extent = token1.Extent;
                                }
                                scriptExtent = extent;
                                this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingEndParenthesisInFunctionParameterList, new object[0]);
                            }
                            this.SkipNewlines();
                        }
                        token2 = this.NextToken();
                        if (token2.Kind != TokenKind.LCurly)
                        {
                            this.UngetToken(token2);
                            if (scriptExtent == null)
                            {
                                objArray = new object[2];
                                objArray[0] = token;
                                objArray[1] = token3;
                                scriptExtent = Parser.ExtentFromFirstOf(objArray);
                                this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingFunctionBody, new object[0]);
                            }
                        }
                        if (scriptExtent != null)
                        {
                            break;
                        }
                        kind = functionToken.Kind == TokenKind.Filter;
                        flag = functionToken.Kind == TokenKind.Workflow;
                        inWorkflowContext = this._tokenizer.InWorkflowContext;
                        try
                        {
                            this._tokenizer.InWorkflowContext = flag;
                            scriptBlockAst = this.ScriptBlockRule(token2, kind);
                            if (token3.Kind == TokenKind.Generic)
                            {
                                value = ((StringToken)token3).Value;
                            }
                            else
                            {
                                value = token3.Text;
                            }
                            str = value;
                            functionDefinitionAst = new FunctionDefinitionAst(Parser.ExtentOf(functionToken, scriptBlockAst), kind, flag, str, parameterAsts, scriptBlockAst);
                            statementAst = functionDefinitionAst;
                        }
                        finally
                        {
                            this._tokenizer.InWorkflowContext = inWorkflowContext;
                        }
                        return statementAst;
                    }
                default:
                    {
                        if (tokenKind == TokenKind.Redirection || tokenKind == TokenKind.RedirectInStd)
                        {
                            this.UngetToken(token3);
                            text = new object[1];
                            text[0] = functionToken.Text;
                            this.ReportIncompleteInput(Parser.After(functionToken), ParserStrings.MissingNameAfterFunctionKeyword, text);
					return new ErrorStatementAst(functionToken.Extent, (IEnumerable<Ast>)null);
                        }
                        scriptExtent = null;
                        this.SkipNewlines();
                        token = null;
                        token1 = this.PeekToken();
                        if (token1.Kind == TokenKind.LParen)
                        {
                            this.SkipToken();
                            parameterAsts = this.ParameterListRule();
                            this.SkipNewlines();
                            token = this.NextToken();
                            if (token.Kind != TokenKind.RParen)
                            {
                                this.UngetToken(token);
                                if (parameterAsts.Any<ParameterAst>())
                                {
                                    extent = parameterAsts.Last<ParameterAst>().Extent;
                                }
                                else
                                {
                                    extent = token1.Extent;
                                }
                                scriptExtent = extent;
                                this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingEndParenthesisInFunctionParameterList, new object[0]);
                            }
                            this.SkipNewlines();
                        }
                        token2 = this.NextToken();
                        if (token2.Kind != TokenKind.LCurly)
                        {
                            this.UngetToken(token2);
                            if (scriptExtent == null)
                            {
                                objArray = new object[2];
                                objArray[0] = token;
                                objArray[1] = token3;
                                scriptExtent = Parser.ExtentFromFirstOf(objArray);
                                this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingFunctionBody, new object[0]);
                            }
                        }
                        if (scriptExtent != null)
                        {
                            break;
                        }
                        kind = functionToken.Kind == TokenKind.Filter;
                        flag = functionToken.Kind == TokenKind.Workflow;
                        inWorkflowContext = this._tokenizer.InWorkflowContext;
                        try
                        {
                            this._tokenizer.InWorkflowContext = flag;
                            scriptBlockAst = this.ScriptBlockRule(token2, kind);
                            if (token3.Kind == TokenKind.Generic)
                            {
                                value = ((StringToken)token3).Value;
                            }
                            else
                            {
                                value = token3.Text;
                            }
                            str = value;
                            functionDefinitionAst = new FunctionDefinitionAst(Parser.ExtentOf(functionToken, scriptBlockAst), kind, flag, str, parameterAsts, scriptBlockAst);
                            statementAst = functionDefinitionAst;
                        }
                        finally
                        {
                            this._tokenizer.InWorkflowContext = inWorkflowContext;
                        }
                        return statementAst;
                    }
            }
            return new ErrorStatementAst(Parser.ExtentOf(functionToken, scriptExtent), parameterAsts);
        }

        private ITypeName GenericTypeArgumentsRule(Token genericTypeName, Token firstToken, bool unBracketedGenericArg)
        {
            Token token;
            Token token1;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            List<ITypeName> typeNames = new List<ITypeName>();
            ITypeName singleGenericArgument = this.GetSingleGenericArgument(firstToken);
            typeNames.Add(singleGenericArgument);
            while (true)
            {
                this.V3SkipNewlines();
                token = this.NextToken();
                if (token.Kind != TokenKind.Comma)
                {
                    break;
                }
                this.V3SkipNewlines();
                token1 = this.PeekToken();
                if (token1.Kind == TokenKind.Identifier || token1.Kind == TokenKind.LBracket)
                {
                    this.SkipToken();
                    singleGenericArgument = this.GetSingleGenericArgument(token1);
                }
                else
                {
                    this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingTypename, new object[0]);
                    singleGenericArgument = new TypeName(token.Extent, ":ErrorTypeName:");
                }
                typeNames.Add(singleGenericArgument);
            }
            if (token.Kind != TokenKind.RBracket)
            {
                this.UngetToken(token);
                this.ReportIncompleteInput(Parser.Before(token), ParserStrings.EndSquareBracketExpectedAtEndOfAttribute, new object[0]);
                token = null;
            }
            TypeName typeName = new TypeName(genericTypeName.Extent, genericTypeName.Text);
            object[] objArray = new object[3];
            objArray[0] = token;
            objArray[1] = typeNames.LastOrDefault<ITypeName>();
            objArray[2] = firstToken;
            GenericTypeName genericTypeName1 = new GenericTypeName(Parser.ExtentOf(genericTypeName.Extent, Parser.ExtentFromFirstOf(objArray)), typeName, typeNames);
            token1 = this.PeekToken();
            if (token1.Kind != TokenKind.LBracket)
            {
                if (token1.Kind == TokenKind.Comma && !unBracketedGenericArg)
                {
                    this.SkipToken();
                    string assemblyNameSpec = this._tokenizer.GetAssemblyNameSpec();
                    if (!string.IsNullOrEmpty(assemblyNameSpec))
                    {
                        typeName.AssemblyName = assemblyNameSpec;
                    }
                    else
                    {
                        this.ReportError(Parser.After(token1), ParserStrings.MissingAssemblyNameSpecification, new object[0]);
                    }
                }
                return genericTypeName1;
            }
            else
            {
                this.SkipToken();
                return this.CompleteArrayTypeName(genericTypeName1, typeName, this.NextToken());
            }
        }

        private ExpressionAst GetCommandArgument(CommandArgumentContext context, Token token)
        {
            ExpressionAst ast;
            List<ExpressionAst> source = null;
            Token token2 = null;
            bool flag = false;
        Label_0006:
            switch (token.Kind)
            {
                case TokenKind.Variable:
                case TokenKind.SplattedVariable:
                case TokenKind.Number:
                case TokenKind.StringLiteral:
                case TokenKind.StringExpandable:
                case TokenKind.HereStringLiteral:
                case TokenKind.HereStringExpandable:
                case TokenKind.LParen:
                case TokenKind.LCurly:
                case TokenKind.AtParen:
                case TokenKind.AtCurly:
                case TokenKind.DollarParen:
                    this.UngetToken(token);
                    ast = this.PrimaryExpressionRule(true);
                    break;

                case TokenKind.Generic:
                    {
                        if ((context & CommandArgumentContext.CommandName) != 0)
                        {
                            token.TokenFlags |= TokenFlags.CommandName;
                        }
                        StringToken token3 = (StringToken)token;
                        StringExpandableToken expandableStringToken = token3 as StringExpandableToken;
                        if ((expandableStringToken != null) && (context != CommandArgumentContext.CommandName))
                        {
                            List<ExpressionAst> nestedExpressions = this.ParseNestedExpressions(expandableStringToken);
                            ast = new ExpandableStringExpressionAst(expandableStringToken, expandableStringToken.Value, expandableStringToken.FormatString, nestedExpressions);
                        }
                        else
                        {
                            ast = new StringConstantExpressionAst(token3.Extent, token3.Value, StringConstantType.BareWord);
                            if (string.Equals(token3.Value, "--%", StringComparison.OrdinalIgnoreCase))
                            {
                                flag = true;
                            }
                        }
                        break;
                    }
                case TokenKind.NewLine:
                case TokenKind.EndOfInput:
                case TokenKind.RParen:
                case TokenKind.RCurly:
                case TokenKind.Semi:
                case TokenKind.AndAnd:
                case TokenKind.OrOr:
                case TokenKind.Ampersand:
                case TokenKind.Pipe:
                case TokenKind.Comma:
                case TokenKind.MinusMinus:
                case TokenKind.Redirection:
                case TokenKind.RedirectInStd:
                    this.UngetToken(token);
                    if (token2 != null)
                    {
                        this.ReportIncompleteInput(After(token2), ParserStrings.MissingExpression, new object[] { "," });
                        return new ErrorExpressionAst(ExtentOf(source.First<ExpressionAst>(), token2), (IEnumerable<Ast>)source);
                    }
                    return null;

                default:
                    ast = new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord);
                    switch (context)
                    {
                        case CommandArgumentContext.CommandName:
                        case CommandArgumentContext.CommandNameAfterInvocationOperator:
                            token.TokenFlags |= TokenFlags.CommandName;
                            break;

                        case CommandArgumentContext.FileName:
                        case CommandArgumentContext.CommandArgument:
                        case CommandArgumentContext.SwitchCondition:
                            token.SetIsCommandArgument();
                            break;
                    }
                    break;
            }
            if ((context == CommandArgumentContext.CommandArgument) && !flag)
            {
                token = this.PeekToken();
                if (token.Kind == TokenKind.Comma)
                {
                    token2 = token;
                    if (source == null)
                    {
                        source = new List<ExpressionAst>();
                    }
                    source.Add(ast);
                    this.SkipToken();
                    this.SkipNewlines();
                    token = this.NextToken();
                    goto Label_0006;
                }
            }
            if (source != null)
            {
                source.Add(ast);
                return new ArrayLiteralAst(ExtentOf(source.First<ExpressionAst>(), source.Last<ExpressionAst>()), source);
            }
            return ast;
        }

        private Tuple<ExpressionAst, StatementAst> GetKeyValuePair()
        {
            Token token;
            ExpressionAst expressionAst;
            StatementAst errorStatementAst;
            Tuple<ExpressionAst, StatementAst> tuple;
            TokenizerMode mode = this._tokenizer.Mode;
            try
            {
                this.SetTokenizerMode(TokenizerMode.Expression);
                expressionAst = this.LabelOrKeyRule();
                if (expressionAst != null)
                {
                    token = this.NextToken();
                    goto Label0;
                }
                else
                {
                    tuple = null;
                }
            }
            finally
            {
                this.SetTokenizerMode(mode);
            }
            return tuple;
        Label0:
            if (token.Kind == TokenKind.Equals)
            {
                try
                {
                    this.SetTokenizerMode(TokenizerMode.Command);
                    this.SkipNewlines();
                    errorStatementAst = this.StatementRule();
                    if (errorStatementAst == null)
                    {
                        IScriptExtent scriptExtent = Parser.After(token);
                        this.ReportIncompleteInput(scriptExtent, ParserStrings.MissingStatementInHashLiteral, new object[0]);
						errorStatementAst = new ErrorStatementAst(scriptExtent, (IEnumerable<Ast>)null);
                    }
                }
                finally
                {
                    this.SetTokenizerMode(mode);
                }
                return new Tuple<ExpressionAst, StatementAst>(expressionAst, errorStatementAst);
            }
            else
            {
                this.UngetToken(token);
                IScriptExtent scriptExtent1 = Parser.After(expressionAst);
                this.ReportError(scriptExtent1, ParserStrings.MissingEqualsInHashLiteral, new object[0]);
                TokenKind[] tokenKindArray = new TokenKind[3];
                tokenKindArray[0] = TokenKind.RCurly;
                tokenKindArray[1] = TokenKind.Semi;
                tokenKindArray[2] = TokenKind.NewLine;
                this.SyncOnError(tokenKindArray);
				return new Tuple<ExpressionAst, StatementAst>(expressionAst, new ErrorStatementAst(scriptExtent1, (IEnumerable<Ast>)null));
            }
        }

        private static IEnumerable<Ast> GetNestedErrorAsts(object[] asts)
        {
            object[] objArray = asts;
            for (int i = 0; i < (int)objArray.Length; i++)
            {
                object obj = objArray[i];
                if (obj != null)
                {
                    Ast ast = obj as Ast;
                    if (ast == null)
                    {
                        IEnumerable<Ast> asts1 = obj as IEnumerable<Ast>;
                        if (asts1 != null)
                        {
                            foreach (Ast ast1 in asts1)
                            {
                                if (ast1 == null)
                                {
                                    continue;
                                }
                                yield return ast1;
                            }
                        }
                    }
                    else
                    {
                        yield return ast;
                    }
                }
            }
        }

        private ExpressionAst GetSingleCommandArgument(Parser.CommandArgumentContext context)
        {
            ExpressionAst commandArgument;
            if (this.PeekToken().Kind != TokenKind.Comma)
            {
                TokenizerMode mode = this._tokenizer.Mode;
                try
                {
                    this.SetTokenizerMode(TokenizerMode.Command);
                    commandArgument = this.GetCommandArgument(context, this.NextToken());
                }
                finally
                {
                    this.SetTokenizerMode(mode);
                }
                return commandArgument;
            }
            else
            {
                return null;
            }
        }

        private ITypeName GetSingleGenericArgument(Token firstToken)
        {
            if (firstToken.Kind != TokenKind.Identifier)
            {
                Token token = this.NextToken();
                if (token.Kind == TokenKind.Identifier)
                {
                    ITypeName typeName = this.FinishTypeNameRule(token, false);
                    if (typeName != null)
                    {
                        Token token1 = this.NextToken();
                        if (token1.Kind != TokenKind.RBracket)
                        {
                            this.UngetToken(token1);
                            this.ReportIncompleteInput(Parser.Before(token1), ParserStrings.EndSquareBracketExpectedAtEndOfType, new object[0]);
                        }
                    }
                    return typeName;
                }
                else
                {
                    this.UngetToken(token);
                    this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingTypename, new object[0]);
                    return new TypeName(firstToken.Extent, ":ErrorTypeName:");
                }
            }
            else
            {
                return this.FinishTypeNameRule(firstToken, true);
            }
        }

        private StringToken GetVerbatimCommandArgumentToken()
        {
            if (this._ungotToken == null || this._ungotToken.Kind == TokenKind.Parameter)
            {
                this._ungotToken = null;
                return this._tokenizer.GetVerbatimCommandArgument();
            }
            else
            {
                return null;
            }
        }

        private ExpressionAst HashExpressionRule(Token atCurlyToken)
        {
            IScriptExtent extent;
            IScriptExtent scriptExtent;
            this.SkipNewlines();
            List<Tuple<ExpressionAst, StatementAst>> tuples = new List<Tuple<ExpressionAst, StatementAst>>();
            while (true)
            {
                Tuple<ExpressionAst, StatementAst> keyValuePair = this.GetKeyValuePair();
                if (keyValuePair == null)
                {
                    break;
                }
                tuples.Add(keyValuePair);
                Token token = this.PeekToken();
                if (token.Kind != TokenKind.NewLine && token.Kind != TokenKind.Semi)
                {
                    break;
                }
                this.SkipNewlinesAndSemicolons();
            }
            Token token1 = this.NextToken();
            if (token1.Kind == TokenKind.RCurly)
            {
                extent = token1.Extent;
            }
            else
            {
                this.UngetToken(token1);
                Parser parser = this;
                if (tuples.Any<Tuple<ExpressionAst, StatementAst>>())
                {
                    scriptExtent = Parser.After(tuples.Last<Tuple<ExpressionAst, StatementAst>>().Item2);
                }
                else
                {
                    scriptExtent = Parser.After(atCurlyToken);
                }
                parser.ReportIncompleteInput(scriptExtent, token1.Extent, ParserStrings.IncompleteHashLiteral, new object[0]);
                extent = Parser.Before(token1);
            }
            return new HashtableAst(Parser.ExtentOf(atCurlyToken, extent), tuples);
        }

        private StatementAst IfStatementRule(Token ifToken)
        {
            IScriptExtent extent;
            List<Tuple<PipelineBaseAst, StatementBlockAst>> tuples = new List<Tuple<PipelineBaseAst, StatementBlockAst>>();
            List<Ast> asts = new List<Ast>();
            StatementBlockAst statementBlockAst = null;
            Token token = ifToken;
            while (true)
            {
                this.SkipNewlines();
                Token token1 = this.NextToken();
                if (token1.Kind != TokenKind.LParen)
                {
                    this.UngetToken(token1);
                    object[] text = new object[1];
                    text[0] = token.Text;
                    this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingOpenParenthesisInIfStatement, text);
                    return new ErrorStatementAst(Parser.ExtentOf(ifToken, token), asts);
                }
                this.SkipNewlines();
                PipelineBaseAst errorStatementAst = this.PipelineRule();
                if (errorStatementAst != null)
                {
                    asts.Add(errorStatementAst);
                }
                else
                {
                    IScriptExtent scriptExtent = Parser.After(token1);
                    object[] objArray = new object[1];
                    objArray[0] = token.Text;
                    this.ReportIncompleteInput(scriptExtent, ParserStrings.IfStatementMissingCondition, objArray);
					errorStatementAst = new ErrorStatementAst(scriptExtent, (IEnumerable<Ast>)null);
                }
                this.SkipNewlines();
                Token token2 = this.NextToken();
                if (token2.Kind != TokenKind.RParen)
                {
                    this.UngetToken(token2);
                    if (errorStatementAst as ErrorStatementAst == null)
                    {
                        object[] text1 = new object[1];
                        text1[0] = token.Text;
                        this.ReportIncompleteInput(token2.Extent, ParserStrings.MissingEndParenthesisAfterStatement, text1);
                    }
                    return new ErrorStatementAst(Parser.ExtentOf(ifToken, Parser.Before(token2)), asts);
                }
                this.SkipNewlines();
                StatementBlockAst statementBlockAst1 = this.StatementBlockRule();
                if (statementBlockAst1 == null)
                {
                    object[] objArray1 = new object[1];
                    objArray1[0] = token.Text;
                    this.ReportIncompleteInput(token2.Extent, ParserStrings.MissingStatementBlock, objArray1);
                    return new ErrorStatementAst(Parser.ExtentOf(ifToken, token2), asts);
                }
                asts.Add(statementBlockAst1);
                tuples.Add(new Tuple<PipelineBaseAst, StatementBlockAst>(errorStatementAst, statementBlockAst1));
                this.SkipNewlines();
                token = this.PeekToken();
                if (token.Kind != TokenKind.ElseIf)
                {
                    break;
                }
                this.SkipToken();
            }
            if (token.Kind == TokenKind.Else)
            {
                this.SkipToken();
                this.SkipNewlines();
                statementBlockAst = this.StatementBlockRule();
                if (statementBlockAst == null)
                {
                    this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingStatementBlockAfterElse, new object[0]);
                    return new ErrorStatementAst(Parser.ExtentOf(ifToken, token), asts);
                }
            }
            if (statementBlockAst != null)
            {
                extent = statementBlockAst.Extent;
            }
            else
            {
                extent = tuples[tuples.Count - 1].Item2.Extent;
            }
            IScriptExtent scriptExtent1 = extent;
            IScriptExtent scriptExtent2 = Parser.ExtentOf(ifToken, scriptExtent1);
            return new IfStatementAst(scriptExtent2, tuples, statementBlockAst);
        }

        private static bool IgnoreTokenWhenUpdatingPreviousFirstLast(Token token)
        {
            if (token.Kind == TokenKind.Variable || token.Kind == TokenKind.Generic)
            {
                if (token.Text.Equals("$^", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return token.Text.Equals("$$", StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                return false;
            }
        }

        private bool InlineScriptRule(Token inlineScriptToken, List<CommandElementAst> elements)
        {
            StringConstantExpressionAst stringConstantExpressionAst = new StringConstantExpressionAst(inlineScriptToken.Extent, inlineScriptToken.Text, StringConstantType.BareWord);
            Token tokenFlags = inlineScriptToken;
            tokenFlags.TokenFlags = tokenFlags.TokenFlags | TokenFlags.CommandName;
            elements.Add(stringConstantExpressionAst);
            this.SkipNewlines();
            Token token = this.NextToken();
            if (token.Kind == TokenKind.LCurly)
            {
                ExpressionAst expressionAst = this.ScriptBlockExpressionRule(token);
                elements.Add(expressionAst);
                return true;
            }
            else
            {
                this.UngetToken(token);
                object[] text = new object[1];
                text[0] = inlineScriptToken.Text;
                this.ReportIncompleteInput(Parser.After(inlineScriptToken), ParserStrings.MissingStatementAfterKeyword, text);
                return false;
            }
        }

        private static bool IsSpecificParameter(Token token, string parameter)
        {
            ParameterToken parameterToken = (ParameterToken)token;
            return parameter.StartsWith(parameterToken.ParameterName, StringComparison.OrdinalIgnoreCase);
        }

        private StatementAst LabeledStatementRule(LabelToken label)
        {
            StatementAst statementAst;
            Token token = this.NextToken();
            TokenKind kind = token.Kind;
            if (kind > TokenKind.Foreach)
            {
                if (kind == TokenKind.Switch)
                {
                    statementAst = this.SwitchStatementRule(label, token);
                }
                else
                {
                    if (kind != TokenKind.While)
                    {
                        this.Resync(label);
                        statementAst = this.PipelineRule();
                        return statementAst;
                    }
                    statementAst = this.WhileStatementRule(label, token);
                }
            }
            else
            {
                if (kind == TokenKind.Do)
                {
                    statementAst = this.DoWhileStatementRule(label, token);
                }
                else
                {
                    if (kind == TokenKind.For)
                    {
                        statementAst = this.ForStatementRule(label, token);
                        return statementAst;
                    }
                    else if (kind == TokenKind.Foreach)
                    {
                        statementAst = this.ForeachStatementRule(label, token);
                        return statementAst;
                    }
                    this.Resync(label);
                    statementAst = this.PipelineRule();
                    return statementAst;
                }
            }
            return statementAst;
        }

        private ExpressionAst LabelOrKeyRule()
        {
            ExpressionAst expressionAst;
            StringConstantExpressionAst stringConstantExpressionAst = this.SimpleNameRule();
            if (stringConstantExpressionAst == null)
            {
                Token token = this.PeekToken();
                if (token.Kind != TokenKind.NewLine && token.Kind != TokenKind.Semi)
                {
                    bool flag = this._disableCommaOperator;
                    try
                    {
                        this._disableCommaOperator = true;
                        expressionAst = this.UnaryExpressionRule();
                    }
                    finally
                    {
                        this._disableCommaOperator = flag;
                    }
                    if (expressionAst != null)
                    {
                        return expressionAst;
                    }
                }
                return null;
            }
            else
            {
                return stringConstantExpressionAst;
            }
        }

        private ExpressionAst MemberAccessRule(ExpressionAst targetExpr, Token operatorToken)
        {
            CommandElementAst commandElementAst = this.MemberNameRule();
            if (commandElementAst != null)
            {
                Token token = this.NextInvokeMemberToken();
                if (token != null)
                {
                    return this.MemberInvokeRule(targetExpr, token, operatorToken, commandElementAst);
                }
            }
            else
            {
                this.ReportIncompleteInput(Parser.After(operatorToken), ParserStrings.MissingPropertyName, new object[0]);
                ExpressionAst singleCommandArgument = this.GetSingleCommandArgument(Parser.CommandArgumentContext.CommandArgument);
                CommandElementAst errorExpressionAst = singleCommandArgument;
                if (singleCommandArgument == null)
                {
                    errorExpressionAst = new ErrorExpressionAst(Parser.ExtentOf(targetExpr, operatorToken), null);
                }
                commandElementAst = errorExpressionAst;
            }
            return new MemberExpressionAst(Parser.ExtentOf(targetExpr, commandElementAst), targetExpr, commandElementAst, operatorToken.Kind == TokenKind.ColonColon);
        }

        private ExpressionAst MemberInvokeRule(ExpressionAst targetExpr, Token lParen, Token operatorToken, CommandElementAst member)
        {
            IScriptExtent scriptExtent;
            List<ExpressionAst> expressionAsts = new List<ExpressionAst>();
            bool flag = false;
            bool flag1 = this._disableCommaOperator;
            Token token = null;
            try
            {
                this._disableCommaOperator = true;
                while (true)
                {
                    this.SkipNewlines();
                    ExpressionAst expressionAst = this.ExpressionRule();
                    if (expressionAst != null)
                    {
                        expressionAsts.Add(expressionAst);
                        this.SkipNewlines();
                        token = this.NextToken();
                        if (token.Kind != TokenKind.Comma)
                        {
                            this.UngetToken(token);
                            token = null;
                            break;
                        }
                    }
                    else
                    {
                        if (token == null)
                        {
                            break;
                        }
                        object[] objArray = new object[1];
                        objArray[0] = TokenKind.Comma.Text();
                        this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingExpressionAfterToken, objArray);
                        flag = true;
                        break;
                    }
                }
            }
            finally
            {
                this._disableCommaOperator = flag1;
            }
            this.SkipNewlines();
            Token token1 = this.NextToken();
            if (token1.Kind != TokenKind.RParen)
            {
                this.UngetToken(token1);
                if (!flag)
                {
                    Parser parser = this;
                    if (expressionAsts.Any<ExpressionAst>())
                    {
                        scriptExtent = Parser.After(expressionAsts.Last<ExpressionAst>());
                    }
                    else
                    {
                        scriptExtent = Parser.After(lParen);
                    }
                    parser.ReportIncompleteInput(scriptExtent, ParserStrings.MissingEndParenthesisInMethodCall, new object[0]);
                }
                token1 = null;
            }
            object[] objArray1 = new object[4];
            objArray1[0] = token1;
            objArray1[1] = token;
            objArray1[2] = expressionAsts.LastOrDefault<ExpressionAst>();
            objArray1[3] = lParen;
            return new InvokeMemberExpressionAst(Parser.ExtentOf(targetExpr, Parser.ExtentFromFirstOf(objArray1)), targetExpr, member, expressionAsts, operatorToken.Kind == TokenKind.ColonColon);
        }

        private ExpressionAst MemberNameRule()
        {
            ExpressionAst expressionAst = this.SimpleNameRule();
            if (expressionAst == null)
            {
                Token token = this.PeekToken();
                if (token.Kind.HasTrait(TokenFlags.UnaryOperator) || token.Kind == TokenKind.LBracket)
                {
                    return this.UnaryExpressionRule();
                }
                else
                {
                    return this.PrimaryExpressionRule(false);
                }
            }
            else
            {
                return expressionAst;
            }
        }

        private ScriptBlockAst NamedBlockListRule(Token lCurly, ParamBlockAst paramBlockAst)
        {
            IScriptExtent extent = null;
            IScriptExtent scriptExtent;
            Token token;
            IScriptExtent scriptExtent1 = null;
            IScriptExtent extent1;
            NamedBlockAst namedBlockAst = null;
            NamedBlockAst namedBlockAst1 = null;
            NamedBlockAst namedBlockAst2 = null;
            NamedBlockAst namedBlockAst3 = null;
            if (lCurly != null)
            {
                extent1 = lCurly.Extent;
            }
            else
            {
                if (paramBlockAst != null)
                {
                    extent1 = paramBlockAst.Extent;
                }
                else
                {
                    extent1 = null;
                }
            }
            IScriptExtent extent2 = extent1;
            while (true)
            {
                token = this.NextToken();
                TokenKind kind = token.Kind;
                if (kind > TokenKind.Dynamicparam)
                {
                    if (kind != TokenKind.End && kind != TokenKind.Process)
                    {
                        break;
                    }
                }
                else
                {
                    if (kind != TokenKind.Begin && kind != TokenKind.Dynamicparam)
                    {
                        break;
                    }
                }
                if (extent2 == null)
                {
                    extent2 = token.Extent;
                }
                extent = token.Extent;
                StatementBlockAst statementBlockAst = this.StatementBlockRule();
                if (statementBlockAst != null)
                {
                    extent = statementBlockAst.Extent;
                }
                else
                {
                    object[] objArray = new object[1];
                    objArray[0] = token.Kind.Text();
                    this.ReportIncompleteInput(Parser.After(token.Extent), ParserStrings.MissingNamedStatementBlock, objArray);
                    statementBlockAst = new StatementBlockAst(token.Extent, new StatementAst[0], null);
                }
                scriptExtent = Parser.ExtentOf(token, extent);
                if (token.Kind != TokenKind.Begin || namedBlockAst1 != null)
                {
                    if (token.Kind != TokenKind.Process || namedBlockAst2 != null)
                    {
                        if (token.Kind != TokenKind.End || namedBlockAst3 != null)
                        {
                            if (token.Kind != TokenKind.Dynamicparam || namedBlockAst != null)
                            {
                                object[] objArray1 = new object[1];
                                objArray1[0] = token.Kind.Text();
                                this.ReportError(scriptExtent, ParserStrings.DuplicateScriptCommandClause, objArray1);
                            }
                            else
                            {
                                namedBlockAst = new NamedBlockAst(scriptExtent, TokenKind.Dynamicparam, statementBlockAst, false);
                            }
                        }
                        else
                        {
                            namedBlockAst3 = new NamedBlockAst(scriptExtent, TokenKind.End, statementBlockAst, false);
                        }
                    }
                    else
                    {
                        namedBlockAst2 = new NamedBlockAst(scriptExtent, TokenKind.Process, statementBlockAst, false);
                    }
                }
                else
                {
                    namedBlockAst1 = new NamedBlockAst(scriptExtent, TokenKind.Begin, statementBlockAst, false);
                }
                this.SkipNewlinesAndSemicolons();
            }
            this.UngetToken(token);
            scriptExtent = Parser.ExtentOf(extent2, extent);
            this.CompleteScriptBlockBody(lCurly, ref scriptExtent, out scriptExtent1);
            return new ScriptBlockAst(scriptExtent1, paramBlockAst, namedBlockAst1, namedBlockAst2, namedBlockAst3, namedBlockAst);
        }

        private Token NextInvokeMemberToken()
        {
            if (this._ungotToken == null)
            {
                return this._tokenizer.GetInvokeMemberOpenParen();
            }
            else
            {
                return null;
            }
        }

        private Token NextLBracket()
        {
            if (this._ungotToken == null)
            {
                return this._tokenizer.GetLBracket();
            }
            else
            {
                if (this._ungotToken.Kind != TokenKind.LBracket)
                {
                    return null;
                }
                else
                {
                    return this.NextToken();
                }
            }
        }

        private Token NextMemberAccessToken(bool allowLBracket)
        {
            if (this._ungotToken == null)
            {
                return this._tokenizer.GetMemberAccessOperator(allowLBracket);
            }
            else
            {
                return null;
            }
        }

        private Token NextToken()
        {
            Token token = this._ungotToken;
            Token token1 = token;
            if (token == null)
            {
                token1 = this._tokenizer.NextToken();
            }
            Token token2 = token1;
            this._ungotToken = null;
            return token2;
        }

        internal void NoteV3FeatureUsed()
        {
        }

        private ParamBlockAst ParamBlockRule()
        {
            IScriptExtent extent;
            this.SkipNewlines();
            List<AttributeBaseAst> attributeBaseAsts = this.AttributeListRule(false);
            this.SkipNewlines();
            Token token = this.PeekToken();
            if (token.Kind == TokenKind.Param)
            {
                this.SkipToken();
                this.SkipNewlines();
                Token token1 = this.NextToken();
                if (token1.Kind == TokenKind.LParen)
                {
                    List<ParameterAst> parameterAsts = this.ParameterListRule();
                    this.SkipNewlines();
                    Token token2 = this.NextToken();
                    IScriptExtent scriptExtent = token2.Extent;
                    if (token2.Kind != TokenKind.RParen)
                    {
                        this.UngetToken(token2);
                        scriptExtent = Parser.Before(token2);
                        Parser parser = this;
                        if (parameterAsts == null || !parameterAsts.Any<ParameterAst>())
                        {
                            extent = token1.Extent;
                        }
                        else
                        {
                            extent = parameterAsts.Last<ParameterAst>().Extent;
                        }
                        parser.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingEndParenthesisInFunctionParameterList, new object[0]);
                    }
                    List<AttributeAst> attributeAsts = new List<AttributeAst>();
                    if (attributeBaseAsts != null)
                    {
                        foreach (AttributeBaseAst attributeBaseAst in attributeBaseAsts)
                        {
                            AttributeAst attributeAst = attributeBaseAst as AttributeAst;
                            if (attributeAst == null)
                            {
                                object[] fullName = new object[1];
                                fullName[0] = attributeBaseAst.TypeName.FullName;
                                this.ReportError(attributeBaseAst.Extent, ParserStrings.TypeNotAllowedBeforeParam, fullName);
                            }
                            else
                            {
                                attributeAsts.Add(attributeAst);
                            }
                        }
                    }
                    return new ParamBlockAst(Parser.ExtentOf(token, scriptExtent), attributeAsts, parameterAsts);
                }
                else
                {
                    this.UngetToken(token1);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private List<ParameterAst> ParameterListRule()
        {
            List<ParameterAst> parameterAsts = new List<ParameterAst>();
            Token token = null;
            while (true)
            {
                ParameterAst parameterAst = this.ParameterRule();
                if (parameterAst != null)
                {
                    parameterAsts.Add(parameterAst);
                    this.SkipNewlines();
                    token = this.PeekToken();
                    if (token.Kind != TokenKind.Comma)
                    {
                        break;
                    }
                    this.SkipToken();
                }
                else
                {
                    if (token == null)
                    {
                        break;
                    }
                    object[] objArray = new object[1];
                    objArray[0] = token.Kind.Text();
                    this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingExpressionAfterToken, objArray);
                    break;
                }
            }
            return parameterAsts;
        }

        private ParameterAst ParameterRule()
        {
            ParameterAst parameterAst;
            IScriptExtent extent;
            IScriptExtent scriptExtent;
            ExpressionAst expressionAst = null;
            bool flag = this._disableCommaOperator;
            TokenizerMode mode = this._tokenizer.Mode;
            try
            {
                this._disableCommaOperator = true;
                this.SetTokenizerMode(TokenizerMode.Expression);
                this.SkipNewlines();
                List<AttributeBaseAst> attributeBaseAsts = this.AttributeListRule(false);
                this.SkipNewlines();
                Token token = this.NextToken();
                if (token.Kind == TokenKind.Variable || token.Kind == TokenKind.SplattedVariable)
                {
                    VariableToken variableToken = (VariableToken)token;
                    this.SkipNewlines();
                    Token token1 = this.PeekToken();
                    if (token1.Kind == TokenKind.Equals)
                    {
                        this.SkipToken();
                        this.SkipNewlines();
                        expressionAst = this.ExpressionRule();
                        if (expressionAst == null)
                        {
                            object[] objArray = new object[1];
                            objArray[0] = token1.Kind.Text();
                            this.ReportIncompleteInput(Parser.After(token1), ParserStrings.MissingExpressionAfterToken, objArray);
                        }
                    }
                    if (attributeBaseAsts == null)
                    {
                        extent = variableToken.Extent;
                    }
                    else
                    {
                        extent = attributeBaseAsts[0].Extent;
                    }
                    IScriptExtent scriptExtent1 = extent;
                    if (expressionAst == null)
                    {
                        scriptExtent = variableToken.Extent;
                    }
                    else
                    {
                        scriptExtent = expressionAst.Extent;
                    }
                    IScriptExtent scriptExtent2 = scriptExtent;
                    return new ParameterAst(Parser.ExtentOf(scriptExtent1, scriptExtent2), new VariableExpressionAst(variableToken), attributeBaseAsts, expressionAst);
                }
                else
                {
                    this.UngetToken(token);
                    if (attributeBaseAsts == null)
                    {
                        parameterAst = null;
                    }
                    else
                    {
                        this.ReportIncompleteInput(Parser.After(attributeBaseAsts.Last<AttributeBaseAst>()), ParserStrings.InvalidFunctionParameter, new object[0]);
                        TokenKind[] tokenKindArray = new TokenKind[1];
                        tokenKindArray[0] = TokenKind.RParen;
                        this.SyncOnError(tokenKindArray);
                        IScriptExtent scriptExtent3 = Parser.ExtentOf(attributeBaseAsts[0].Extent, attributeBaseAsts[attributeBaseAsts.Count - 1].Extent);
                        parameterAst = new ParameterAst(scriptExtent3, new VariableExpressionAst(scriptExtent3, "__error__", false), attributeBaseAsts, null);
                    }
                }
            }
            finally
            {
                this._disableCommaOperator = flag;
                this.SetTokenizerMode(mode);
            }
            return parameterAst;
        }

        private ExpressionAst ParenthesizedExpressionRule(Token lParen)
        {
            Token token;
            PipelineBaseAst errorStatementAst;
            TokenizerMode mode = this._tokenizer.Mode;
            bool flag = this._disableCommaOperator;
            try
            {
                this.SetTokenizerMode(TokenizerMode.Command);
                this._disableCommaOperator = false;
                this.SkipNewlines();
                errorStatementAst = this.PipelineRule();
                if (errorStatementAst == null)
                {
                    IScriptExtent scriptExtent = Parser.After(lParen);
                    this.ReportIncompleteInput(scriptExtent, ParserStrings.ExpectedExpression, new object[0]);
					errorStatementAst = new ErrorStatementAst(scriptExtent, (IEnumerable<Ast>)null);
                }
                this.SkipNewlines();
                token = this.NextToken();
                if (token.Kind != TokenKind.RParen)
                {
                    this.UngetToken(token);
                    this.ReportIncompleteInput(Parser.After(errorStatementAst), ParserStrings.MissingEndParenthesisInExpression, new object[0]);
                    token = null;
                }
            }
            finally
            {
                this._disableCommaOperator = flag;
                this.SetTokenizerMode(mode);
            }
            object[] objArray = new object[2];
            objArray[0] = token;
            objArray[1] = errorStatementAst;
            return new ParenExpressionAst(Parser.ExtentOf(lParen, Parser.ExtentFromFirstOf(objArray)), errorStatementAst);
        }

        internal ScriptBlockAst Parse(string fileName, string input, List<Token> tokenList, out ParseError[] errors)
        {
            ScriptBlockAst scriptBlockAst;
            try
            {
                scriptBlockAst = this.ParseTask(fileName, input, tokenList, false);
            }
            finally
            {
                errors = this._errorList.ToArray();
            }
            return scriptBlockAst;
        }

        public static ScriptBlockAst ParseFile(string fileName, out Token[] tokens, out ParseError[] errors)
        {
            ScriptBlockAst scriptBlockAst;
            Parser parser = new Parser();
            ExternalScriptInfo externalScriptInfo = new ExternalScriptInfo(fileName, fileName);
            List<Token> tokens1 = new List<Token>();
            try
            {
                scriptBlockAst = parser.Parse(fileName, externalScriptInfo.ScriptContents, tokens1, out errors);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                throw new ParseException(ParserStrings.UnrecoverableParserError, exception);
            }
            tokens = tokens1.ToArray();
            return scriptBlockAst;
        }

        public static ScriptBlockAst ParseInput(string input, out Token[] tokens, out ParseError[] errors)
        {
            return Parser.ParseInput(input, null, out tokens, out errors);
        }

        internal static ScriptBlockAst ParseInput(string input, string fileName, out Token[] tokens, out ParseError[] errors)
        {
            ScriptBlockAst scriptBlockAst;
            Parser parser = new Parser();
            List<Token> tokens1 = new List<Token>();
            try
            {
                scriptBlockAst = parser.Parse(fileName, input, tokens1, out errors);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                throw new ParseException(ParserStrings.UnrecoverableParserError, exception);
            }
            tokens = tokens1.ToArray();
            return scriptBlockAst;
        }

        private List<ExpressionAst> ParseNestedExpressions(StringExpandableToken expandableStringToken)
        {
            ExpressionAst expressionAst;
            List<Token> tokens;
            List<ExpressionAst> expressionAsts = new List<ExpressionAst>();
            if (this._savingTokens)
            {
                tokens = new List<Token>();
            }
            else
            {
                tokens = null;
            }
            List<Token> tokens1 = tokens;
            foreach (Token nestedToken in expandableStringToken.NestedTokens)
            {
                VariableToken variableToken = nestedToken as VariableToken;
                if (variableToken == null)
                {
                    TokenizerState tokenizerState = null;
                    try
                    {
                        tokenizerState = this._tokenizer.StartNestedScan((UnscannedSubExprToken)nestedToken);
                        expressionAst = this.PrimaryExpressionRule(true);
                        if (this._savingTokens)
                        {
                            tokens1.AddRange(this._tokenizer.TokenList);
                        }
                    }
                    finally
                    {
                        this._ungotToken = null;
                        this._tokenizer.FinishNestedScan(tokenizerState);
                    }
                }
                else
                {
                    expressionAst = this.CheckUsingVariable(variableToken, false);
                    if (this._savingTokens)
                    {
                        tokens1.Add(variableToken);
                    }
                }
                expressionAsts.Add(expressionAst);
            }
            if (this._savingTokens)
            {
                expandableStringToken.NestedTokens = new ReadOnlyCollection<Token>(tokens1);
            }
            return expressionAsts;
        }

        private ScriptBlockAst ParseTask(string fileName, string input, List<Token> tokenList, bool recursed)
        {
            Func<ScriptBlockAst> func = null;
            ScriptBlockAst scriptRequirements = null;
            this._tokenizer.Initialize(fileName, input, tokenList);
            this._savingTokens = tokenList != null;
            this._errorList.Clear();
            try
            {
                scriptRequirements = this.ScriptBlockRule(null, false);
                scriptRequirements.ScriptRequirements = this._tokenizer.GetScriptRequirements();
                SemanticChecks.CheckAst(this, scriptRequirements);
            }
            catch (InsufficientExecutionStackException insufficientExecutionStackException)
            {
                if (recursed)
                {
                    this.ReportError(this._tokenizer.CurrentExtent(), ParserStrings.ScriptTooComplicated, new object[0]);
                }
                else
                {
                    if (func == null)
                    {
                        func = () => this.ParseTask(fileName, input, tokenList, true);
                    }
                    Task<ScriptBlockAst> task = new Task<ScriptBlockAst>(func);
                    task.Start();
                    task.Wait();
                    scriptRequirements = task.Result;
                }
            }
            return scriptRequirements;
        }

        private Token PeekToken()
        {
            Token token = this._ungotToken;
            Token token1 = token;
            if (token == null)
            {
                token1 = this._tokenizer.NextToken();
            }
            Token token2 = token1;
            if (this._ungotToken == null)
            {
                this._ungotToken = token2;
            }
            return token2;
        }

        private PipelineBaseAst PipelineRule()
        {
            List<CommandBaseAst> source = new List<CommandBaseAst>();
            IScriptExtent first = null;
            Token token = null;
            bool flag = true;
            while (flag)
            {
                CommandBaseAst ast;
                ExpressionAst ast2;
                Token token2 = null;
                TokenizerMode mode = this._tokenizer.Mode;
                try
                {
                    this.SetTokenizerMode(TokenizerMode.Expression);
                    ast2 = this.ExpressionRule();
                    if (ast2 != null)
                    {
                        Token token3 = this.PeekToken();
                        if (token3.Kind.HasTrait(TokenFlags.AssignmentOperator))
                        {
                            this.SkipToken();
                            token2 = token3;
                        }
                    }
                }
                finally
                {
                    this.SetTokenizerMode(mode);
                }
                if (ast2 != null)
                {
                    if (source.Any<CommandBaseAst>())
                    {
                        this.ReportError(ast2.Extent, ParserStrings.ExpressionsMustBeFirstInPipeline, new object[0]);
                    }
                    if (token2 != null)
                    {
                        this.SkipNewlines();
                        StatementAst right = this.StatementRule();
                        if (right == null)
                        {
                            IScriptExtent extent2 = After(token2);
                            this.ReportIncompleteInput(extent2, ParserStrings.ExpectedValueExpression, new object[] { token2.Kind.Text() });
							right = new ErrorStatementAst(extent2, (IEnumerable<Ast>)null);
                        }
                        return new AssignmentStatementAst(ExtentOf((Ast)ast2, (Ast)right), ast2, token2.Kind, right, token2.Extent);
                    }
                    RedirectionAst[] redirections = null;
                    RedirectionToken redirectionToken = this.PeekToken() as RedirectionToken;
                    RedirectionAst ast4 = null;
                    while (redirectionToken != null)
                    {
                        this.SkipToken();
                        if (redirections == null)
                        {
                            redirections = new RedirectionAst[7];
                        }
                        IScriptExtent extent3 = null;
                        ast4 = this.RedirectionRule(redirectionToken, redirections, ref extent3);
                        redirectionToken = this.PeekToken() as RedirectionToken;
                    }
                    IScriptExtent extent = (ast4 != null) ? ExtentOf((Ast)ast2, (Ast)ast4) : ast2.Extent;
                    ast = new CommandExpressionAst(extent, ast2, (redirections != null) ? (from r in redirections
                                                                                           where r != null
                                                                                           select r) : null);
                }
                else
                {
                    ast = this.CommandRule();
                }
                if (ast != null)
                {
                    if (first == null)
                    {
                        first = ast.Extent;
                    }
                    source.Add(ast);
                }
                else if (source.Any<CommandBaseAst>() || (this.PeekToken().Kind == TokenKind.Pipe))
                {
                    IScriptExtent extent5 = (token != null) ? After(token) : this.PeekToken().Extent;
                    this.ReportIncompleteInput(extent5, ParserStrings.EmptyPipeElement, new object[0]);
                }
                token = this.PeekToken();
                switch (token.Kind)
                {
                    case TokenKind.NewLine:
                    case TokenKind.EndOfInput:
                    case TokenKind.RParen:
                    case TokenKind.RCurly:
                    case TokenKind.Semi:
                        {
                            flag = false;
                            continue;
                        }
                    case TokenKind.AndAnd:
                    case TokenKind.OrOr:
                        {
                            this.SkipToken();
                            this.SkipNewlines();
                            this.ReportError(token.Extent, ParserStrings.InvalidEndOfLine, new object[] { token.Text });
                            if (this.PeekToken().Kind == TokenKind.EndOfInput)
                            {
                                flag = false;
                            }
                            continue;
                        }
                    case TokenKind.Pipe:
                        {
                            this.SkipToken();
                            this.SkipNewlines();
                            if (this.PeekToken().Kind == TokenKind.EndOfInput)
                            {
                                flag = false;
                                this.ReportIncompleteInput(After(token), ParserStrings.EmptyPipeElement, new object[0]);
                            }
                            continue;
                        }
                }
                this.ReportError(token.Extent, ParserStrings.UnexpectedToken, new object[] { token.Text });
                flag = false;
            }
            if (source.Count == 0)
            {
                return null;
            }
            return new PipelineAst(ExtentOf(first, source[source.Count - 1]), source);
        }

        private ExpressionAst PrimaryExpressionRule(bool withMemberAccess)
        {
            ExpressionAst constantExpressionAst;
            Token token = this.NextToken();
            TokenKind kind = token.Kind;
            if (kind == TokenKind.Variable || kind == TokenKind.SplattedVariable)
            {
                constantExpressionAst = this.CheckUsingVariable((VariableToken)token, withMemberAccess);
                if (withMemberAccess)
                {
                    return this.CheckPostPrimaryExpressionOperators(this.NextMemberAccessToken(true), constantExpressionAst);
                }
                else
                {
                    return constantExpressionAst;
                }
                this.UngetToken(token);
                return null;
            }
            else if (kind == TokenKind.Parameter)
            {
                this.UngetToken(token);
                return null;
            }
            else if (kind == TokenKind.Number)
            {
                constantExpressionAst = new ConstantExpressionAst((NumberToken)token);
                if (withMemberAccess)
                {
                    return this.CheckPostPrimaryExpressionOperators(this.NextMemberAccessToken(true), constantExpressionAst);
                }
                else
                {
                    return constantExpressionAst;
                }
                this.UngetToken(token);
                return null;
            }
            if (kind == TokenKind.StringLiteral || kind == TokenKind.HereStringLiteral)
            {
                constantExpressionAst = new StringConstantExpressionAst((StringToken)token);
            }
            else if (kind == TokenKind.StringExpandable || kind == TokenKind.HereStringExpandable)
            {
                constantExpressionAst = this.ExpandableStringRule((StringExpandableToken)token);
            }
            else if (kind == TokenKind.LParen)
            {
                constantExpressionAst = this.ParenthesizedExpressionRule(token);
            }
            else if (kind == TokenKind.RParen || kind == TokenKind.RCurly || kind == TokenKind.LBracket || kind == TokenKind.RBracket)
            {
                this.UngetToken(token);
                return null;
            }
            else if (kind == TokenKind.LCurly)
            {
                constantExpressionAst = this.ScriptBlockExpressionRule(token);
            }
            else if (kind == TokenKind.AtParen || kind == TokenKind.DollarParen)
            {
                constantExpressionAst = this.SubExpressionRule(token);
            }
            else if (kind == TokenKind.AtCurly)
            {
                constantExpressionAst = this.HashExpressionRule(token);
            }
            else
            {
                this.UngetToken(token);
                return null;
            }
            if (withMemberAccess)
            {
                return this.CheckPostPrimaryExpressionOperators(this.NextMemberAccessToken(true), constantExpressionAst);
            }
            else
            {
                return constantExpressionAst;
            }
            this.UngetToken(token);
            return null;
        }

        private RedirectionAst RedirectionRule(RedirectionToken redirectionToken, RedirectionAst[] redirections, ref IScriptExtent extent)
        {
            RedirectionAst fileRedirectionAst;
            string allStream;
            FileRedirectionToken fileRedirectionToken = redirectionToken as FileRedirectionToken;
            if (fileRedirectionToken != null || redirectionToken as InputRedirectionToken != null)
            {
                ExpressionAst singleCommandArgument = this.GetSingleCommandArgument(Parser.CommandArgumentContext.FileName);
                if (singleCommandArgument == null)
                {
                    this.ReportError(Parser.After(redirectionToken), ParserStrings.MissingFileSpecification, new object[0]);
                    singleCommandArgument = new ErrorExpressionAst(redirectionToken.Extent, null);
                }
                if (fileRedirectionToken != null)
                {
                    fileRedirectionAst = new FileRedirectionAst(Parser.ExtentOf(fileRedirectionToken, singleCommandArgument), fileRedirectionToken.FromStream, singleCommandArgument, fileRedirectionToken.Append);
                }
                else
                {
                    object[] text = new object[1];
                    text[0] = redirectionToken.Text;
                    this.ReportError(redirectionToken.Extent, ParserStrings.RedirectionNotSupported, text);
                    extent = Parser.ExtentOf(redirectionToken, singleCommandArgument);
                    return null;
                }
            }
            else
            {
                MergingRedirectionToken mergingRedirectionToken = (MergingRedirectionToken)redirectionToken;
                RedirectionStream fromStream = mergingRedirectionToken.FromStream;
                RedirectionStream toStream = mergingRedirectionToken.ToStream;
                if (toStream == RedirectionStream.Output)
                {
                    if (fromStream == toStream)
                    {
                        object[] objArray = new object[1];
                        objArray[0] = mergingRedirectionToken.Text;
                        this.ReportError(redirectionToken.Extent, ParserStrings.RedirectionNotSupported, objArray);
                    }
                }
                else
                {
                    object[] text1 = new object[1];
                    text1[0] = mergingRedirectionToken.Text;
                    this.ReportError(redirectionToken.Extent, ParserStrings.RedirectionNotSupported, text1);
                    toStream = RedirectionStream.Output;
                }
                fileRedirectionAst = new MergingRedirectionAst(mergingRedirectionToken.Extent, mergingRedirectionToken.FromStream, toStream);
            }
            if (redirections[(int)fileRedirectionAst.FromStream] != null)
            {
                RedirectionStream redirectionStream = fileRedirectionAst.FromStream;
                if (redirectionStream == RedirectionStream.All)
                {
                    allStream = ParserStrings.AllStream;
                }
                else if (redirectionStream == RedirectionStream.Output)
                {
                    allStream = ParserStrings.OutputStream;
                }
                else if (redirectionStream == RedirectionStream.Error)
                {
                    allStream = ParserStrings.ErrorStream;
                }
                else if (redirectionStream == RedirectionStream.Warning)
                {
                    allStream = ParserStrings.WarningStream;
                }
                else if (redirectionStream == RedirectionStream.Verbose)
                {
                    allStream = ParserStrings.VerboseStream;
                }
                else if (redirectionStream == RedirectionStream.Debug)
                {
                    allStream = ParserStrings.DebugStream;
                }
                else if (redirectionStream == RedirectionStream.Host)
                {
                    allStream = ParserStrings.HostStream;
                }
                else
                {
                    throw PSTraceSource.NewArgumentOutOfRangeException("result.FromStream", (object)fileRedirectionAst.FromStream);
                }
                object[] objArray1 = new object[1];
                objArray1[0] = allStream;
                this.ReportError(fileRedirectionAst.Extent, ParserStrings.StreamAlreadyRedirected, objArray1);
            }
            else
            {
                redirections[(int)fileRedirectionAst.FromStream] = fileRedirectionAst;
            }
            extent = fileRedirectionAst.Extent;
            return fileRedirectionAst;
        }

        internal void ReportError(IScriptExtent extent, Expression<Func<string>> errorExpr, object[] args)
        {
            this.SaveError(extent, errorExpr, false, args);
        }

        internal void ReportError(IScriptExtent extent, string errorExpr, object[] args)
        {
            this.SaveError(extent, () => errorExpr, false, args);
        }

        internal void ReportError(ParseError error)
        {
            this.SaveError(error);
        }

        internal bool ReportIncompleteInput(IScriptExtent extent, Expression<Func<string>> errorExpr, object[] args)
        {
            bool flag = this._tokenizer.IsAtEndOfScript(extent, true);
            this.SaveError(extent, errorExpr, flag, args);
            return flag;
        }

        internal bool ReportIncompleteInput(IScriptExtent extent, string errorExpr, object[] args)
        {
            bool flag = this._tokenizer.IsAtEndOfScript(extent, true);
            this.SaveError(extent, () => errorExpr, flag, args);
            return flag;
        }

        internal bool ReportIncompleteInput(IScriptExtent errorPosition, IScriptExtent errorDetectedPosition, Expression<Func<string>> errorExpr, object[] args)
        {
            bool flag = this._tokenizer.IsAtEndOfScript(errorDetectedPosition, true);
            this.SaveError(errorPosition, errorExpr, flag, args);
            return flag;
        }

        internal bool ReportIncompleteInput(IScriptExtent errorPosition, IScriptExtent errorDetectedPosition, string errorExpr, object[] args)
        {
            bool flag = this._tokenizer.IsAtEndOfScript(errorDetectedPosition, true);
            this.SaveError(errorPosition, () => errorExpr, flag, args);
            return flag;
        }

        private void Resync(Token token)
        {
            this._ungotToken = null;
            this._tokenizer.Resync(token);
        }

        private void Resync(int restorePoint)
        {
            this._ungotToken = null;
            this._tokenizer.Resync(restorePoint);
        }

        private ReturnStatementAst ReturnStatementRule(Token token)
        {
            IScriptExtent extent;
            PipelineBaseAst pipelineBaseAst = this.PipelineRule();
            if (pipelineBaseAst != null)
            {
                extent = Parser.ExtentOf(token, pipelineBaseAst);
            }
            else
            {
                extent = token.Extent;
            }
            IScriptExtent scriptExtent = extent;
            return new ReturnStatementAst(scriptExtent, pipelineBaseAst);
        }

        private void SaveError(ParseError error)
        {
            Func<ParseError, bool> func = null;
            if (this._errorList.Any<ParseError>())
            {
                List<ParseError> parseErrors = this._errorList;
                if (func == null)
                {
                    func = (ParseError err) =>
                    {
                        if (!err.ErrorId.Equals(error.ErrorId, StringComparison.Ordinal) || err.Extent.EndColumnNumber != error.Extent.EndColumnNumber || err.Extent.EndLineNumber != error.Extent.EndLineNumber || err.Extent.StartColumnNumber != error.Extent.StartColumnNumber)
                        {
                            return false;
                        }
                        else
                        {
                            return err.Extent.StartLineNumber == error.Extent.StartLineNumber;
                        }
                    }
                    ;
                }
                if (parseErrors.Where<ParseError>(func).Any<ParseError>())
                {
                    return;
                }
            }
            this._errorList.Add(error);
        }

        private void SaveError(IScriptExtent extent, Expression<Func<string>> errorExpr, bool incompleteInput, object[] args)
        {
            string str = null;
            string name = null;
            MemberExpression body = errorExpr.Body as MemberExpression;
            if (body != null)
            {
                PropertyInfo member = body.Member as PropertyInfo;
                if (member != null)
                {
                    MethodInfo getMethod = member.GetGetMethod(true);
                    if (getMethod != null && getMethod.IsStatic && getMethod.ReturnType.Equals(typeof(string)))
                    {
                        str = (string)getMethod.Invoke(null, null);
                        name = member.Name;
                    }
                }
            }
            if (str == null)
            {
                str = errorExpr.Compile()();
                name = "ParserError";
            }
            if (args != null && args.Any<object>())
            {
                str = string.Format(CultureInfo.CurrentCulture, str, args);
            }
            ParseError parseError = new ParseError(extent, name, str, incompleteInput);
            this.SaveError(parseError);
        }

        internal static object ScanNumber(string str, Type toType)
        {
            str = str.Trim();
            if (str.Length != 0)
            {
                Tokenizer parser = (new Parser())._tokenizer;
                parser.Initialize(null, str, null);
                parser.AllowSignedNumbers = true;
                NumberToken numberToken = parser.NextToken() as NumberToken;
                if (numberToken == null || !parser.IsAtEndOfScript(numberToken.Extent, false))
                {
                    return LanguagePrimitives.ConvertTo(str, toType, CultureInfo.InvariantCulture);
                }
                else
                {
                    return numberToken.Value;
                }
            }
            else
            {
                return 0;
            }
        }

        internal static ExpressionAst ScanString(string str)
        {
            string str1 = string.Concat((char)34, str.Replace("\"", "\"\""), (char)34);
            Parser parser = new Parser();
            Tokenizer tokenizer = new Tokenizer(parser);
            tokenizer.Initialize(null, str1, null);
            StringExpandableToken stringExpandableToken = (StringExpandableToken)tokenizer.NextToken();
            ExpressionAst expressionAst = parser.ExpandableStringRule(stringExpandableToken);
            if (!parser._errorList.Any<ParseError>())
            {
                return expressionAst;
            }
            else
            {
                throw new ParseException(parser._errorList.ToArray());
            }
        }

        internal static ITypeName ScanType(string typename, bool ignoreErrors)
        {
            Token token = null;
            typename = typename.Trim();
            if (typename.Length != 0)
            {
                Parser parser = new Parser();
                Tokenizer tokenizer = parser._tokenizer;
                tokenizer.Initialize(null, typename, null);
                ITypeName typeName = parser.TypeNameRule(out token);
                SemanticChecks.CheckArrayTypeNameDepth(typeName, PositionUtilities.EmptyExtent, parser);
                if (!ignoreErrors && parser.ErrorList.Any<ParseError>())
                {
                    typeName = null;
                }
                return typeName;
            }
            else
            {
                return null;
            }
        }

        private ScriptBlockAst ScriptBlockBodyRule(Token lCurly, ParamBlockAst paramBlockAst, bool isFilter)
        {
            IScriptExtent scriptExtent = null;
            IScriptExtent extent;
            Token token = this.PeekToken();
            if ((token.TokenFlags & TokenFlags.ScriptBlockBlockName) != TokenFlags.ScriptBlockBlockName)
            {
                List<TrapStatementAst> trapStatementAsts = new List<TrapStatementAst>();
                List<StatementAst> statementAsts = new List<StatementAst>();
                if (paramBlockAst != null)
                {
                    extent = paramBlockAst.Extent;
                }
                else
                {
                    extent = null;
                }
                IScriptExtent scriptExtent1 = extent;
                do
                {
                    IScriptExtent scriptExtent2 = this.StatementListRule(statementAsts, trapStatementAsts);
                    if (scriptExtent1 != null)
                    {
                        if (scriptExtent2 == null)
                        {
                            continue;
                        }
                        scriptExtent1 = Parser.ExtentOf(scriptExtent1, scriptExtent2);
                    }
                    else
                    {
                        scriptExtent1 = scriptExtent2;
                    }
                }
                while (!this.CompleteScriptBlockBody(lCurly, ref scriptExtent1, out scriptExtent));
                IScriptExtent scriptExtent3 = scriptExtent;
                ParamBlockAst paramBlockAst1 = paramBlockAst;
                IScriptExtent scriptExtent4 = scriptExtent1;
                IScriptExtent emptyExtent = scriptExtent4;
                if (scriptExtent4 == null)
                {
                    emptyExtent = PositionUtilities.EmptyExtent;
                }
                return new ScriptBlockAst(scriptExtent3, paramBlockAst1, new StatementBlockAst(emptyExtent, statementAsts, trapStatementAsts), isFilter);
            }
            else
            {
                return this.NamedBlockListRule(lCurly, paramBlockAst);
            }
        }

        private ExpressionAst ScriptBlockExpressionRule(Token lCurly)
        {
            ScriptBlockAst scriptBlockAst;
            bool flag = this._disableCommaOperator;
            TokenizerMode mode = this._tokenizer.Mode;
            try
            {
                this._disableCommaOperator = false;
                this.SetTokenizerMode(TokenizerMode.Command);
                this.SkipNewlines();
                scriptBlockAst = this.ScriptBlockRule(lCurly, false);
            }
            finally
            {
                this._disableCommaOperator = flag;
                this.SetTokenizerMode(mode);
            }
            return new ScriptBlockExpressionAst(scriptBlockAst.Extent, scriptBlockAst);
        }

        private ScriptBlockAst ScriptBlockRule(Token lCurly, bool isFilter)
        {
            int restorePoint = this._tokenizer.GetRestorePoint();
            ParamBlockAst paramBlockAst = this.ParamBlockRule();
            if (paramBlockAst == null)
            {
                this.Resync(restorePoint);
            }
            this.SkipNewlinesAndSemicolons();
            return this.ScriptBlockBodyRule(lCurly, paramBlockAst, isFilter);
        }

        internal void SetPreviousFirstLastToken(ExecutionContext context)
        {
            if (this._tokenizer.FirstToken != null)
            {
                context.SetVariable(SpecialVariables.FirstTokenVarPath, this._previousFirstToken);
                if (!Parser.IgnoreTokenWhenUpdatingPreviousFirstLast(this._tokenizer.FirstToken))
                {
                    this._previousFirstToken = this._tokenizer.FirstToken.Text;
                }
                context.SetVariable(SpecialVariables.LastTokenVarPath, this._previousLastToken);
                if (!Parser.IgnoreTokenWhenUpdatingPreviousFirstLast(this._tokenizer.LastToken))
                {
                    this._previousLastToken = this._tokenizer.LastToken.Text;
                }
            }
        }

        private void SetTokenizerMode(TokenizerMode mode)
        {
            if (mode != this._tokenizer.Mode && this._ungotToken != null && !this._ungotToken.Kind.HasTrait(TokenFlags.ParseModeInvariant))
            {
                this.Resync(this._ungotToken);
            }
            this._tokenizer.Mode = mode;
        }

        private StringConstantExpressionAst SimpleNameRule()
        {
            Token token;
            try
            {
                this._tokenizer.WantSimpleName = true;
                token = this.PeekToken();
            }
            finally
            {
                this._tokenizer.WantSimpleName = false;
            }
            if (token.Kind != TokenKind.Identifier)
            {
                return null;
            }
            else
            {
                Token tokenFlags = token;
                tokenFlags.TokenFlags = tokenFlags.TokenFlags | TokenFlags.MemberName;
                this.SkipToken();
                return new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord);
            }
        }

        private void SkipNewlines()
        {
            if (this._ungotToken == null || this._ungotToken.Kind == TokenKind.NewLine)
            {
                this._ungotToken = null;
                this._tokenizer.SkipNewlines(false, false);
            }
        }

        private void SkipNewlinesAndSemicolons()
        {
            if (this._ungotToken == null || this._ungotToken.Kind == TokenKind.NewLine || this._ungotToken.Kind == TokenKind.Semi)
            {
                this._ungotToken = null;
                this._tokenizer.SkipNewlines(true, false);
            }
        }

        private void SkipToken()
        {
            this._ungotToken = null;
        }

        private StatementBlockAst StatementBlockRule()
        {
            IScriptExtent extent;
            this.SkipNewlines();
            Token token = this.NextToken();
            if (token.Kind == TokenKind.LCurly)
            {
                List<TrapStatementAst> trapStatementAsts = new List<TrapStatementAst>();
                List<StatementAst> statementAsts = new List<StatementAst>();
                IScriptExtent scriptExtent = this.StatementListRule(statementAsts, trapStatementAsts);
                Token token1 = this.NextToken();
                if (token1.Kind == TokenKind.RCurly)
                {
                    extent = token1.Extent;
                }
                else
                {
                    this.UngetToken(token1);
                    IScriptExtent scriptExtent1 = scriptExtent;
                    IScriptExtent extent1 = scriptExtent1;
                    if (scriptExtent1 == null)
                    {
                        extent1 = token.Extent;
                    }
                    extent = extent1;
                    this.ReportIncompleteInput(token.Extent, token1.Extent, ParserStrings.MissingEndCurlyBrace, new object[0]);
                }
                return new StatementBlockAst(Parser.ExtentOf(token, extent), statementAsts, trapStatementAsts);
            }
            else
            {
                this.UngetToken(token);
                return null;
            }
        }

        private IScriptExtent StatementListRule(List<StatementAst> statements, List<TrapStatementAst> traps)
        {
            StatementAst statementAst = null;
            Token token;
            StatementAst statementAst1 = null;
            this.SkipNewlinesAndSemicolons();
            do
            {
                StatementAst statementAst2 = this.StatementRule();
                if (statementAst2 == null)
                {
                    break;
                }
                this._tokenizer.CheckAstIsBeforeSignature(statementAst2);
                if (statementAst2 as TrapStatementAst == null)
                {
                    statements.Add(statementAst2);
                }
                else
                {
                    traps.Add((TrapStatementAst)statementAst2);
                }
                if (statementAst1 == null)
                {
                    statementAst1 = statementAst2;
                }
                statementAst = statementAst2;
                this.SkipNewlinesAndSemicolons();
                token = this.PeekToken();
            }
            while (token.Kind != TokenKind.RParen && token.Kind != TokenKind.RCurly);
            if (statementAst1 == null)
            {
                return null;
            }
            else
            {
                return Parser.ExtentOf(statementAst1, statementAst);
            }
        }

        private StatementAst StatementRule()
        {
            StatementAst errorStatementAst;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            Token token = this.NextToken();
            TokenKind kind = token.Kind;
            if (kind == TokenKind.Label)
            {
                this.SkipNewlines();
                errorStatementAst = this.LabeledStatementRule((LabelToken)token);
            }
            else
            {
                if (kind == TokenKind.EndOfInput)
                {
                    this.UngetToken(token);
                    errorStatementAst = null;
                }
                else
                {
                    switch (kind)
                    {
                        case TokenKind.Break:
                            {
                                errorStatementAst = this.BreakStatementRule(token);
                                break;
                            }
                        case TokenKind.Catch:
                        case TokenKind.Else:
                        case TokenKind.ElseIf:
                        case TokenKind.Until:
                            {
                                if (!this._errorList.Any<ParseError>())
                                {
                                    this.UngetToken(token);
                                    errorStatementAst = this.PipelineRule();
                                    break;
                                }
                                this.SkipNewlines();
                                return this.StatementRule();
                            }
                        case TokenKind.Class:
                        case TokenKind.Define:
                        case TokenKind.From:
                        case TokenKind.Using:
                        case TokenKind.Var:
                            {
                                object[] objArray = new object[1];
                                objArray[0] = token.Kind.Text();
                                this.ReportError(token.Extent, ParserStrings.ReservedKeywordNotAllowed, objArray);
						errorStatementAst = new ErrorStatementAst(token.Extent, (IEnumerable<Ast>)null);
                                break;
                            }
                        case TokenKind.Continue:
                            {
                                errorStatementAst = this.ContinueStatementRule(token);
                                break;
                            }
                        case TokenKind.Data:
                            {
                                errorStatementAst = this.DataStatementRule(token);
                                break;
                            }
                        case TokenKind.Do:
                            {
                                errorStatementAst = this.DoWhileStatementRule(null, token);
                                break;
                            }
                        case TokenKind.Dynamicparam:
                        case TokenKind.End:
                        case TokenKind.Finally:
                        case TokenKind.In:
                        case TokenKind.Param:
                        case TokenKind.Process:
                            {
                                this.UngetToken(token);
                                errorStatementAst = this.PipelineRule();
                                break;
                            }
                        case TokenKind.Exit:
                            {
                                errorStatementAst = this.ExitStatementRule(token);
                                break;
                            }
                        case TokenKind.Filter:
                        case TokenKind.Function:
                        case TokenKind.Workflow:
                            {
                                errorStatementAst = this.FunctionDeclarationRule(token);
                                break;
                            }
                        case TokenKind.For:
                            {
                                errorStatementAst = this.ForStatementRule(null, token);
                                break;
                            }
                        case TokenKind.Foreach:
                            {
                                errorStatementAst = this.ForeachStatementRule(null, token);
                                break;
                            }
                        case TokenKind.If:
                            {
                                errorStatementAst = this.IfStatementRule(token);
                                break;
                            }
                        case TokenKind.Return:
                            {
                                errorStatementAst = this.ReturnStatementRule(token);
                                break;
                            }
                        case TokenKind.Switch:
                            {
                                errorStatementAst = this.SwitchStatementRule(null, token);
                                break;
                            }
                        case TokenKind.Throw:
                            {
                                errorStatementAst = this.ThrowStatementRule(token);
                                break;
                            }
                        case TokenKind.Trap:
                            {
                                errorStatementAst = this.TrapStatementRule(token);
                                break;
                            }
                        case TokenKind.Try:
                            {
                                errorStatementAst = this.TryStatementRule(token);
                                break;
                            }
                        case TokenKind.While:
                            {
                                errorStatementAst = this.WhileStatementRule(null, token);
                                break;
                            }
                        case TokenKind.Parallel:
                        case TokenKind.Sequence:
                            {
                                errorStatementAst = this.BlockStatementRule(token);
                                break;
                            }
                        default:
                            {
                                this.UngetToken(token);
                                errorStatementAst = this.PipelineRule();
                                break;
                            }
                    }
                }
            }
            return errorStatementAst;
        }

        private ExpressionAst SubExpressionRule(Token firstToken)
        {
            IScriptExtent scriptExtent;
            Token token;
            IScriptExtent extent;
            List<TrapStatementAst> trapStatementAsts = new List<TrapStatementAst>();
            List<StatementAst> statementAsts = new List<StatementAst>();
            bool flag = this._disableCommaOperator;
            TokenizerMode mode = this._tokenizer.Mode;
            try
            {
                this._disableCommaOperator = false;
                this.SetTokenizerMode(TokenizerMode.Command);
                this.SkipNewlines();
                scriptExtent = this.StatementListRule(statementAsts, trapStatementAsts);
                this.SkipNewlines();
                token = this.NextToken();
                if (token.Kind != TokenKind.RParen)
                {
                    this.UngetToken(token);
                    this.ReportIncompleteInput(token.Extent, ParserStrings.MissingEndParenthesisInSubexpression, new object[0]);
                }
            }
            finally
            {
                this._disableCommaOperator = flag;
                this.SetTokenizerMode(mode);
            }
            Token token1 = firstToken;
            if (token.Kind == TokenKind.RParen)
            {
                extent = token.Extent;
            }
            else
            {
                IScriptExtent scriptExtent1 = scriptExtent;
                extent = scriptExtent1;
                if (scriptExtent1 == null)
                {
                    extent = firstToken.Extent;
                }
            }
            IScriptExtent scriptExtent2 = Parser.ExtentOf(token1, extent);
            if (firstToken.Kind != TokenKind.DollarParen)
            {
                IScriptExtent scriptExtent3 = scriptExtent2;
                IScriptExtent scriptExtent4 = scriptExtent;
                IScriptExtent emptyExtent = scriptExtent4;
                if (scriptExtent4 == null)
                {
                    emptyExtent = PositionUtilities.EmptyExtent;
                }
                return new ArrayExpressionAst(scriptExtent3, new StatementBlockAst(emptyExtent, statementAsts, trapStatementAsts));
            }
            else
            {
                IScriptExtent scriptExtent5 = scriptExtent2;
                IScriptExtent scriptExtent6 = scriptExtent;
                IScriptExtent emptyExtent1 = scriptExtent6;
                if (scriptExtent6 == null)
                {
                    emptyExtent1 = PositionUtilities.EmptyExtent;
                }
                return new SubExpressionAst(scriptExtent5, new StatementBlockAst(emptyExtent1, statementAsts, trapStatementAsts));
            }
        }

        private StatementAst SwitchStatementRule(LabelToken labelToken, Token switchToken)
        {
            IScriptExtent extent = null;
            Token token;
            string labelText;
            IEnumerable<Ast> nestedErrorAsts;
            LabelToken labelToken1 = labelToken;
            Token token1 = labelToken1;
            if (labelToken1 == null)
            {
                token1 = switchToken;
            }
            IScriptExtent scriptExtent = token1.Extent;
            bool flag = false;
            bool flag1 = false;
            this.SkipNewlines();
            bool flag2 = false;
            PipelineBaseAst pipelineAst = null;
            Dictionary<string, Tuple<Token, Ast>> strs = null;
            Token token2 = this.PeekToken();
            SwitchFlags switchFlag = SwitchFlags.None;
            while (token2.Kind == TokenKind.Parameter)
            {
                this.SkipToken();
                extent = token2.Extent;
                Dictionary<string, Tuple<Token, Ast>> strs1 = strs;
                Dictionary<string, Tuple<Token, Ast>> strs2 = strs1;
                if (strs1 == null)
                {
                    strs2 = new Dictionary<string, Tuple<Token, Ast>>();
                }
                strs = strs2;
                if (!Parser.IsSpecificParameter(token2, "regex"))
                {
                    if (!Parser.IsSpecificParameter(token2, "wildcard"))
                    {
                        if (!Parser.IsSpecificParameter(token2, "exact"))
                        {
                            if (!Parser.IsSpecificParameter(token2, "casesensitive"))
                            {
                                if (!Parser.IsSpecificParameter(token2, "parallel"))
                                {
                                    if (!Parser.IsSpecificParameter(token2, "file"))
                                    {
                                        flag = true;
                                        object[] parameterName = new object[1];
                                        parameterName[0] = ((ParameterToken)token2).ParameterName;
                                        this.ReportError(token2.Extent, ParserStrings.InvalidSwitchFlag, parameterName);
                                    }
                                    else
                                    {
                                        switchFlag = switchFlag | SwitchFlags.File;
                                        this.SkipNewlines();
                                        ExpressionAst singleCommandArgument = this.GetSingleCommandArgument(Parser.CommandArgumentContext.FileName);
                                        if (singleCommandArgument != null)
                                        {
                                            extent = singleCommandArgument.Extent;
                                            pipelineAst = new PipelineAst(singleCommandArgument.Extent, new CommandExpressionAst(singleCommandArgument.Extent, singleCommandArgument, null));
                                            if (!strs.ContainsKey("file"))
                                            {
                                                strs.Add("file", new Tuple<Token, Ast>(token2, pipelineAst));
                                            }
                                        }
                                        else
                                        {
                                            flag = true;
                                            flag1 = this.ReportIncompleteInput(Parser.After(token2), ParserStrings.MissingFilenameOption, new object[0]);
                                            if (!strs.ContainsKey("file"))
                                            {
                                                strs.Add("file", new Tuple<Token, Ast>(token2, null));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    switchFlag = switchFlag | SwitchFlags.Parallel;
                                    if (!strs.ContainsKey("parallel"))
                                    {
                                        strs.Add("parallel", new Tuple<Token, Ast>(token2, null));
                                    }
                                }
                            }
                            else
                            {
                                switchFlag = switchFlag | SwitchFlags.CaseSensitive;
                                if (!strs.ContainsKey("casesensitive"))
                                {
                                    strs.Add("casesensitive", new Tuple<Token, Ast>(token2, null));
                                }
                            }
                        }
                        else
                        {
                            switchFlag = switchFlag & (SwitchFlags.File | SwitchFlags.Wildcard | SwitchFlags.Exact | SwitchFlags.CaseSensitive | SwitchFlags.Parallel);
                            switchFlag = switchFlag & (SwitchFlags.File | SwitchFlags.Regex | SwitchFlags.Exact | SwitchFlags.CaseSensitive | SwitchFlags.Parallel);
                            if (!strs.ContainsKey("exact"))
                            {
                                strs.Add("exact", new Tuple<Token, Ast>(token2, null));
                            }
                        }
                    }
                    else
                    {
                        switchFlag = switchFlag | SwitchFlags.Wildcard;
                        switchFlag = switchFlag & (SwitchFlags.File | SwitchFlags.Wildcard | SwitchFlags.Exact | SwitchFlags.CaseSensitive | SwitchFlags.Parallel);
                        if (!strs.ContainsKey("wildcard"))
                        {
                            strs.Add("wildcard", new Tuple<Token, Ast>(token2, null));
                        }
                    }
                }
                else
                {
                    switchFlag = switchFlag | SwitchFlags.Regex;
                    switchFlag = switchFlag & (SwitchFlags.File | SwitchFlags.Regex | SwitchFlags.Exact | SwitchFlags.CaseSensitive | SwitchFlags.Parallel);
                    if (!strs.ContainsKey("regex"))
                    {
                        strs.Add("regex", new Tuple<Token, Ast>(token2, null));
                    }
                }
                token2 = this.PeekToken();
            }
            if (token2.Kind == TokenKind.Minus)
            {
                Dictionary<string, Tuple<Token, Ast>> strs3 = strs;
                Dictionary<string, Tuple<Token, Ast>> strs4 = strs3;
                if (strs3 == null)
                {
                    strs4 = new Dictionary<string, Tuple<Token, Ast>>();
                }
                strs = strs4;
                strs.Add("--%", new Tuple<Token, Ast>(token2, null));
            }
            Token token3 = this.PeekToken();
            if (token3.Kind != TokenKind.LParen)
            {
                if (pipelineAst == null && (switchFlag & SwitchFlags.File) == SwitchFlags.None)
                {
                    flag = true;
                    flag1 = this.ReportIncompleteInput(Parser.After(extent), ParserStrings.PipelineValueRequired, new object[0]);
                }
            }
            else
            {
                extent = token3.Extent;
                this.SkipToken();
                if ((switchFlag & SwitchFlags.File) == SwitchFlags.File)
                {
                    flag = true;
                    this.ReportError(token3.Extent, ParserStrings.PipelineValueRequired, new object[0]);
                }
                flag2 = true;
                this.SkipNewlines();
                pipelineAst = this.PipelineRule();
                if (pipelineAst != null)
                {
                    extent = pipelineAst.Extent;
                }
                else
                {
                    flag = true;
                    flag1 = this.ReportIncompleteInput(Parser.After(token3), ParserStrings.PipelineValueRequired, new object[0]);
                }
                this.SkipNewlines();
                Token token4 = this.NextToken();
                if (token4.Kind == TokenKind.RParen)
                {
                    extent = token4.Extent;
                }
                else
                {
                    this.UngetToken(token4);
                    if (!flag1)
                    {
                        flag = true;
                        flag1 = this.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingEndParenthesisInSwitchStatement, new object[0]);
                    }
                }
            }
            this.SkipNewlines();
            Token token5 = this.NextToken();
            StatementBlockAst statementBlockAst = null;
            List<Tuple<ExpressionAst, StatementBlockAst>> tuples = new List<Tuple<ExpressionAst, StatementBlockAst>>();
            List<Ast> asts = new List<Ast>();
            Token token6 = null;
            if (token5.Kind == TokenKind.LCurly)
            {
                extent = token5.Extent;
                this.SkipNewlines();
                do
                {
                    ExpressionAst expressionAst = this.GetSingleCommandArgument(Parser.CommandArgumentContext.SwitchCondition);
                    if (expressionAst != null)
                    {
                        asts.Add(expressionAst);
                        extent = expressionAst.Extent;
                        StatementBlockAst statementBlockAst1 = this.StatementBlockRule();
                        if (statementBlockAst1 != null)
                        {
                            asts.Add(statementBlockAst1);
                            extent = statementBlockAst1.Extent;
                            StringConstantExpressionAst stringConstantExpressionAst = expressionAst as StringConstantExpressionAst;
                            if (stringConstantExpressionAst == null || stringConstantExpressionAst.StringConstantType != StringConstantType.BareWord || !stringConstantExpressionAst.Value.Equals("default", StringComparison.OrdinalIgnoreCase))
                            {
                                tuples.Add(new Tuple<ExpressionAst, StatementBlockAst>(expressionAst, statementBlockAst1));
                            }
                            else
                            {
                                if (statementBlockAst != null)
                                {
                                    flag = true;
                                    this.ReportError(expressionAst.Extent, ParserStrings.MultipleSwitchDefaultClauses, new object[0]);
                                }
                                statementBlockAst = statementBlockAst1;
                            }
                        }
                        else
                        {
                            flag = true;
                            flag1 = this.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingSwitchStatementClause, new object[0]);
                        }
                        this.SkipNewlinesAndSemicolons();
                        token = this.PeekToken();
                        if (token.Kind != TokenKind.RCurly)
                        {
                            continue;
                        }
                        token6 = token;
                        this.SkipToken();
                        goto Label0;
                    }
                    else
                    {
                        flag = true;
                        this.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingSwitchConditionExpression, new object[0]);
                        if (this.PeekToken().Kind != TokenKind.RCurly)
                        {
                            goto Label0;
                        }
                        this.SkipToken();
                        goto Label0;
                    }
                }
                while (token.Kind != TokenKind.EndOfInput);
                if (!flag1)
                {
                    flag = true;
                    this.ReportIncompleteInput(token5.Extent, token.Extent, ParserStrings.MissingEndCurlyBrace, new object[0]);
                }
            }
            else
            {
                this.UngetToken(token5);
                if (!flag1)
                {
                    flag = true;
                    this.ReportIncompleteInput(Parser.After(extent), ParserStrings.MissingCurlyBraceInSwitchStatement, new object[0]);
                }
            }
        Label0:
            if (!flag)
            {
                LabelToken labelToken2 = labelToken;
                Token token7 = labelToken2;
                if (labelToken2 == null)
                {
                    token7 = switchToken;
                }
                IScriptExtent scriptExtent1 = Parser.ExtentOf(token7, token6);
                if (labelToken != null)
                {
                    labelText = labelToken.LabelText;
                }
                else
                {
                    labelText = null;
                }
                return new SwitchStatementAst(scriptExtent1, labelText, pipelineAst, switchFlag, tuples, statementBlockAst);
            }
            else
            {
                IScriptExtent scriptExtent2 = Parser.ExtentOf(scriptExtent, extent);
                Token token8 = switchToken;
                Dictionary<string, Tuple<Token, Ast>> strs5 = strs;
                if (flag2)
                {
                    object[] objArray = new object[1];
                    objArray[0] = pipelineAst;
                    nestedErrorAsts = Parser.GetNestedErrorAsts(objArray);
                }
                else
                {
                    nestedErrorAsts = null;
                }
                object[] objArray1 = new object[1];
                objArray1[0] = asts;
                return new ErrorStatementAst(scriptExtent2, token8, strs5, nestedErrorAsts, Parser.GetNestedErrorAsts(objArray1));
            }
        }

        private void SyncOnError(TokenKind[] syncTokens)
        {
            Token token;
            int num;
            int num1;
            int num2;
            if (syncTokens.Contains<TokenKind>(TokenKind.RParen))
            {
                num = 1;
            }
            else
            {
                num = 0;
            }
            int num3 = num;
            if (syncTokens.Contains<TokenKind>(TokenKind.RCurly))
            {
                num1 = 1;
            }
            else
            {
                num1 = 0;
            }
            int num4 = num1;
            if (syncTokens.Contains<TokenKind>(TokenKind.RBracket))
            {
                num2 = 1;
            }
            else
            {
                num2 = 0;
            }
            int num5 = num2;
            do
            {
                token = this.NextToken();
                TokenKind kind = token.Kind;
                if (kind == TokenKind.EndOfInput)
                {
                    this.UngetToken(token);
                    return;
                }
                else if (kind == TokenKind.StringLiteral || kind == TokenKind.StringExpandable || kind == TokenKind.HereStringLiteral || kind == TokenKind.HereStringExpandable)
                {
                    continue;
                }
                else if (kind == TokenKind.LParen)
                {
                    num3++;
                    continue;
                }
                else if (kind == TokenKind.RParen)
                {
                    num3--;
                    if (num3 != 0 || !syncTokens.Contains<TokenKind>(TokenKind.RParen))
                    {
                        continue;
                    }
                    return;
                }
                else if (kind == TokenKind.LCurly)
                {
                    num4++;
                    continue;
                }
                else if (kind == TokenKind.RCurly)
                {
                    num4--;
                    if (num4 != 0 || !syncTokens.Contains<TokenKind>(TokenKind.RCurly))
                    {
                        continue;
                    }
                    return;
                }
                else if (kind == TokenKind.LBracket)
                {
                    num5++;
                    continue;
                }
                else if (kind == TokenKind.RBracket)
                {
                    num5--;
                    if (num5 != 0 || !syncTokens.Contains<TokenKind>(TokenKind.RBracket))
                    {
                        continue;
                    }
                    return;
                }
            }
            while (!syncTokens.Contains<TokenKind>(token.Kind) || num3 != 0 || num4 != 0 || num5 != 0);
        }

        private ThrowStatementAst ThrowStatementRule(Token token)
        {
            IScriptExtent extent;
            PipelineBaseAst pipelineBaseAst = this.PipelineRule();
            if (pipelineBaseAst != null)
            {
                extent = Parser.ExtentOf(token, pipelineBaseAst);
            }
            else
            {
                extent = token.Extent;
            }
            IScriptExtent scriptExtent = extent;
            return new ThrowStatementAst(scriptExtent, pipelineBaseAst);
        }

        private StatementAst TrapStatementRule(Token trapToken)
        {
            int restorePoint = this._tokenizer.GetRestorePoint();
            this.SkipNewlines();
            AttributeBaseAst attributeBaseAst = this.AttributeRule();
            TypeConstraintAst typeConstraintAst = attributeBaseAst as TypeConstraintAst;
            if (attributeBaseAst != null && typeConstraintAst == null)
            {
                this.Resync(restorePoint);
            }
            StatementBlockAst statementBlockAst = this.StatementBlockRule();
            if (statementBlockAst != null)
            {
                return new TrapStatementAst(Parser.ExtentOf(trapToken, statementBlockAst), typeConstraintAst, statementBlockAst);
            }
            else
            {
                object[] objArray = new object[2];
                objArray[0] = typeConstraintAst;
                objArray[1] = trapToken;
                IScriptExtent scriptExtent = Parser.ExtentFromFirstOf(objArray);
                this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingTrapStatement, new object[0]);
                object[] objArray1 = new object[1];
                objArray1[0] = typeConstraintAst;
                return new ErrorStatementAst(Parser.ExtentOf(trapToken, scriptExtent), Parser.GetNestedErrorAsts(objArray1));
            }
        }

        private StatementAst TryStatementRule(Token tryToken)
        {
            IScriptExtent extent;
            this.SkipNewlines();
            StatementBlockAst statementBlockAst = this.StatementBlockRule();
            if (statementBlockAst != null)
            {
                IScriptExtent scriptExtent = null;
                List<CatchClauseAst> catchClauseAsts = new List<CatchClauseAst>();
                List<TypeConstraintAst> typeConstraintAsts = null;
                while (true)
                {
                    CatchClauseAst catchClauseAst = this.CatchBlockRule(ref scriptExtent, ref typeConstraintAsts);
                    CatchClauseAst catchClauseAst1 = catchClauseAst;
                    if (catchClauseAst == null)
                    {
                        break;
                    }
                    catchClauseAsts.Add(catchClauseAst1);
                }
                this.SkipNewlines();
                Token token = this.PeekToken();
                StatementBlockAst statementBlockAst1 = null;
                if (token.Kind == TokenKind.Finally)
                {
                    this.SkipToken();
                    statementBlockAst1 = this.StatementBlockRule();
                    if (statementBlockAst1 == null)
                    {
                        scriptExtent = token.Extent;
                        object[] objArray = new object[1];
                        objArray[0] = token.Kind.Text();
                        this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingFinallyStatementBlock, objArray);
                    }
                }
                if (!catchClauseAsts.Any<CatchClauseAst>() && statementBlockAst1 == null && scriptExtent == null)
                {
                    scriptExtent = statementBlockAst.Extent;
                    this.ReportIncompleteInput(Parser.After(scriptExtent), ParserStrings.MissingCatchOrFinally, new object[0]);
                }
                if (scriptExtent == null)
                {
                    Token token1 = tryToken;
                    if (statementBlockAst1 != null)
                    {
                        extent = statementBlockAst1.Extent;
                    }
                    else
                    {
                        extent = catchClauseAsts.Last<CatchClauseAst>().Extent;
                    }
                    return new TryStatementAst(Parser.ExtentOf(token1, extent), statementBlockAst, catchClauseAsts, statementBlockAst1);
                }
                else
                {
                    object[] objArray1 = new object[3];
                    objArray1[0] = statementBlockAst;
                    objArray1[1] = catchClauseAsts;
                    objArray1[2] = typeConstraintAsts;
                    return new ErrorStatementAst(Parser.ExtentOf(tryToken, scriptExtent), Parser.GetNestedErrorAsts(objArray1));
                }
            }
            else
            {
                this.ReportIncompleteInput(Parser.After(tryToken), ParserStrings.MissingTryStatementBlock, new object[0]);
				return new ErrorStatementAst(tryToken.Extent, (IEnumerable<Ast>)null);
            }
        }

        private ITypeName TypeNameRule(out Token firstTypeNameToken)
        {
            ITypeName typeName;
            TokenizerMode mode = this._tokenizer.Mode;
            try
            {
                this.SetTokenizerMode(TokenizerMode.TypeName);
                Token token = this.NextToken();
                if (token.Kind == TokenKind.Identifier)
                {
                    firstTypeNameToken = token;
                    typeName = this.FinishTypeNameRule(token, false);
                }
                else
                {
                    this.UngetToken(token);
                    firstTypeNameToken = null;
                    typeName = null;
                }
            }
            finally
            {
                this.SetTokenizerMode(mode);
            }
            return typeName;
        }

        private ExpressionAst UnaryExpressionRule()
        {
            Token token;
            ExpressionAst expressionAst;
            TokenKind tokenKind;
            Token token1;
            ExpressionAst convertExpressionAst;
            RuntimeHelpers.EnsureSufficientExecutionStack();
            ExpressionAst typeExpressionAst = null;
            bool allowSignedNumbers = this._tokenizer.AllowSignedNumbers;
            try
            {
                this._tokenizer.AllowSignedNumbers = true;
                if (this._ungotToken != null && this._ungotToken.Kind == TokenKind.Minus)
                {
                    this.Resync(this._ungotToken);
                }
                token = this.PeekToken();
            }
            finally
            {
                this._tokenizer.AllowSignedNumbers = allowSignedNumbers;
            }
            if (!token.Kind.HasTrait(TokenFlags.UnaryOperator))
            {
                if (token.Kind != TokenKind.LBracket)
                {
                    typeExpressionAst = this.PrimaryExpressionRule(true);
                }
                else
                {
                    List<AttributeBaseAst> attributeBaseAsts = this.AttributeListRule(true);
                    if (attributeBaseAsts != null)
                    {
                        AttributeBaseAst attributeBaseAst = attributeBaseAsts.Last<AttributeBaseAst>();
                        if (attributeBaseAst as AttributeAst == null)
                        {
                            if (this._ungotToken != null)
                            {
                                token1 = null;
                            }
                            else
                            {
                                token1 = this.NextMemberAccessToken(false);
                            }
                            Token token2 = token1;
                            if (token2 == null)
                            {
                                token = this.PeekToken();
                                if (token.Kind != TokenKind.NewLine && token.Kind != TokenKind.Comma)
                                {
                                    expressionAst = this.UnaryExpressionRule();
                                    if (expressionAst != null)
                                    {
                                        typeExpressionAst = new ConvertExpressionAst(Parser.ExtentOf(attributeBaseAst, expressionAst), (TypeConstraintAst)attributeBaseAst, expressionAst);
                                    }
                                }
                            }
                            else
                            {
                                typeExpressionAst = this.CheckPostPrimaryExpressionOperators(token2, new TypeExpressionAst(attributeBaseAst.Extent, attributeBaseAst.TypeName));
                            }
                            if (typeExpressionAst == null)
                            {
                                typeExpressionAst = new TypeExpressionAst(attributeBaseAst.Extent, attributeBaseAst.TypeName);
                            }
                        }
                        else
                        {
                            this.SkipNewlines();
                            expressionAst = this.UnaryExpressionRule();
                            if (expressionAst != null)
                            {
                                typeExpressionAst = new AttributedExpressionAst(Parser.ExtentOf(attributeBaseAst, expressionAst), attributeBaseAst, expressionAst);
                            }
                            else
                            {
                                object[] fullName = new object[1];
                                fullName[0] = attributeBaseAst.TypeName.FullName;
                                this.ReportIncompleteInput(attributeBaseAst.Extent, ParserStrings.UnexpectedAttribute, fullName);
                                return new ErrorExpressionAst(Parser.ExtentOf(token, attributeBaseAst), null);
                            }
                        }
                        for (int i = attributeBaseAsts.Count - 2; i >= 0; i--)
                        {
                            TypeConstraintAst item = attributeBaseAsts[i] as TypeConstraintAst;
                            if (item != null)
                            {
                                convertExpressionAst = new ConvertExpressionAst(Parser.ExtentOf(item, typeExpressionAst), item, typeExpressionAst);
                            }
                            else
                            {
                                convertExpressionAst = new AttributedExpressionAst(Parser.ExtentOf(attributeBaseAsts[i], typeExpressionAst), attributeBaseAsts[i], typeExpressionAst);
                            }
                            typeExpressionAst = convertExpressionAst;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                if (!this._disableCommaOperator || token.Kind != TokenKind.Comma)
                {
                    this.SkipToken();
                    this.SkipNewlines();
                    expressionAst = this.UnaryExpressionRule();
                    if (expressionAst == null)
                    {
                        object[] text = new object[1];
                        text[0] = token.Text;
                        this.ReportIncompleteInput(Parser.After(token), ParserStrings.MissingExpressionAfterOperator, text);
                        return new ErrorExpressionAst(token.Extent, null);
                    }
                    else
                    {
                        if (token.Kind != TokenKind.Comma)
                        {
                            typeExpressionAst = new UnaryExpressionAst(Parser.ExtentOf(token, expressionAst), token.Kind, expressionAst);
                        }
                        else
                        {
                            ExpressionAst[] expressionAstArray = new ExpressionAst[1];
                            expressionAstArray[0] = expressionAst;
                            typeExpressionAst = new ArrayLiteralAst(Parser.ExtentOf(token, expressionAst), expressionAstArray);
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            if (typeExpressionAst != null)
            {
                token = this.PeekToken();
                if (token.Kind == TokenKind.PlusPlus)
                {
                    tokenKind = TokenKind.PostfixPlusPlus;
                }
                else
                {
                    if (token.Kind == TokenKind.MinusMinus)
                    {
                        tokenKind = TokenKind.PostfixMinusMinus;
                    }
                    else
                    {
                        tokenKind = TokenKind.Unknown;
                    }
                }
                TokenKind tokenKind1 = tokenKind;
                if (tokenKind1 != TokenKind.Unknown)
                {
                    this.SkipToken();
                    typeExpressionAst = new UnaryExpressionAst(Parser.ExtentOf(typeExpressionAst, token), tokenKind1, typeExpressionAst);
                }
            }
            return typeExpressionAst;
        }

        private void UngetToken(Token token)
        {
            this._ungotToken = token;
        }

        private void V3SkipNewlines()
        {
            if (this._ungotToken == null || this._ungotToken.Kind == TokenKind.NewLine)
            {
                this._ungotToken = null;
                this._tokenizer.SkipNewlines(false, true);
            }
        }

        private StatementAst WhileStatementRule(LabelToken labelToken, Token whileToken)
        {
            string labelText;
            this.SkipNewlines();
            Token token = this.NextToken();
            if (token.Kind == TokenKind.LParen)
            {
                this.SkipNewlines();
                PipelineBaseAst errorStatementAst = this.PipelineRule();
                PipelineBaseAst pipelineBaseAst = null;
                if (errorStatementAst != null)
                {
                    pipelineBaseAst = errorStatementAst;
                }
                else
                {
                    IScriptExtent scriptExtent = Parser.After(token);
                    object[] objArray = new object[1];
                    objArray[0] = whileToken.Kind.Text();
                    this.ReportIncompleteInput(scriptExtent, ParserStrings.MissingExpressionAfterKeyword, objArray);
					errorStatementAst = new ErrorStatementAst(scriptExtent, (IEnumerable<Ast>)null);
                }
                this.SkipNewlines();
                Token token1 = this.NextToken();
                if (token1.Kind == TokenKind.RParen)
                {
                    this.SkipNewlines();
                    StatementBlockAst statementBlockAst = this.StatementBlockRule();
                    if (statementBlockAst != null)
                    {
                        LabelToken labelToken1 = labelToken;
                        Token token2 = labelToken1;
                        if (labelToken1 == null)
                        {
                            token2 = whileToken;
                        }
                        IScriptExtent scriptExtent1 = Parser.ExtentOf(token2, statementBlockAst);
                        if (labelToken != null)
                        {
                            labelText = labelToken.LabelText;
                        }
                        else
                        {
                            labelText = null;
                        }
                        return new WhileStatementAst(scriptExtent1, labelText, errorStatementAst, statementBlockAst);
                    }
                    else
                    {
                        object[] objArray1 = new object[1];
                        objArray1[0] = whileToken.Kind.Text();
                        this.ReportIncompleteInput(Parser.After(token1), ParserStrings.MissingLoopStatement, objArray1);
                        LabelToken labelToken2 = labelToken;
                        Token token3 = labelToken2;
                        if (labelToken2 == null)
                        {
                            token3 = whileToken;
                        }
                        object[] objArray2 = new object[1];
                        objArray2[0] = pipelineBaseAst;
                        return new ErrorStatementAst(Parser.ExtentOf(token3, token1), Parser.GetNestedErrorAsts(objArray2));
                    }
                }
                else
                {
                    this.UngetToken(token1);
                    if (errorStatementAst as ErrorStatementAst == null)
                    {
                        object[] objArray3 = new object[1];
                        objArray3[0] = whileToken.Kind.Text();
                        this.ReportIncompleteInput(Parser.After(errorStatementAst), ParserStrings.MissingEndParenthesisAfterStatement, objArray3);
                    }
                    LabelToken labelToken3 = labelToken;
                    Token token4 = labelToken3;
                    if (labelToken3 == null)
                    {
                        token4 = whileToken;
                    }
                    object[] objArray4 = new object[1];
                    objArray4[0] = pipelineBaseAst;
                    return new ErrorStatementAst(Parser.ExtentOf(token4, errorStatementAst), Parser.GetNestedErrorAsts(objArray4));
                }
            }
            else
            {
                this.UngetToken(token);
                object[] text = new object[1];
                text[0] = whileToken.Text;
                this.ReportIncompleteInput(Parser.After(whileToken), ParserStrings.MissingOpenParenthesisAfterKeyword, text);
                LabelToken labelToken4 = labelToken;
                Token token5 = labelToken4;
                if (labelToken4 == null)
                {
                    token5 = whileToken;
                }
				return new ErrorStatementAst(Parser.ExtentOf(token5, whileToken), (IEnumerable<Ast>)null);
            }
        }

        [Flags]
        private enum CommandArgumentContext
        {
            CommandName = 1,
            CommandNameUnknown = 2,
            CommandNameAfterInvocationOperator = 3,
            FileName = 4,
            CommandArgument = 8,
            SwitchCondition = 16
        }
    }
}