namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation.Language;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    internal class CompletionAnalysis
    {
        private readonly Ast _ast;
        private readonly IScriptPosition _cursorPosition;
        private readonly Hashtable _options;
        private readonly Token[] _tokens;

        internal CompletionAnalysis(Ast ast, Token[] tokens, IScriptPosition cursorPosition, Hashtable options)
        {
            this._ast = ast;
            this._tokens = tokens;
            this._cursorPosition = cursorPosition;
            this._options = options;
        }

        private static bool CompleteAgainstStatementFlags(Ast scriptAst, Ast lastAst, Token token, out TokenKind kind)
        {
            Func<Ast, bool> predicate = null;
            kind = TokenKind.Unknown;
            ErrorStatementAst ast = lastAst as ErrorStatementAst;
            if (((ast != null) && (ast.Kind != null)) && (ast.Kind.Kind == TokenKind.Switch))
            {
                kind = TokenKind.Switch;
                return true;
            }
            ScriptBlockAst ast2 = scriptAst as ScriptBlockAst;
            if (((token != null) && (token.Kind == TokenKind.Minus)) && (ast2 != null))
            {
                Tuple<Token, Ast> tuple;
                if (predicate == null)
                {
                    predicate = a => IsCursorBeforeExtent(token.Extent.StartScriptPosition, a.Extent);
                }
                Ast parent = AstSearcher.FindAll(ast2, predicate, true).LastOrDefault<Ast>();
                ast = null;
                while (parent != null)
                {
                    ast = parent as ErrorStatementAst;
                    if (ast != null)
                    {
                        break;
                    }
                    parent = parent.Parent;
                }
                if ((((ast != null) && (ast.Kind != null)) && ((ast.Kind.Kind == TokenKind.Switch) && (ast.Flags != null))) && (ast.Flags.TryGetValue("--%", out tuple) && IsTokenTheSame(tuple.Item1, token)))
                {
                    kind = TokenKind.Switch;
                    return true;
                }
            }
            return false;
        }

        private static bool CompleteAgainstSwitchFile(Ast lastAst, Token tokenBeforeCursor)
        {
            Tuple<Token, Ast> tuple;
            ErrorStatementAst ast = lastAst as ErrorStatementAst;
            if ((((ast != null) && (ast.Flags != null)) && ((ast.Kind != null) && (tokenBeforeCursor != null))) && (ast.Kind.Kind.Equals(TokenKind.Switch) && ast.Flags.TryGetValue("file", out tuple)))
            {
                return (tuple.Item1.Extent.EndOffset == tokenBeforeCursor.Extent.EndOffset);
            }
            if (!(lastAst.Parent is CommandExpressionAst))
            {
                return false;
            }
            PipelineAst parent = lastAst.Parent.Parent as PipelineAst;
            if (parent == null)
            {
                return false;
            }
            ast = parent.Parent as ErrorStatementAst;
            if (((ast == null) || (ast.Kind == null)) || (ast.Flags == null))
            {
                return false;
            }
            return ((ast.Kind.Kind.Equals(TokenKind.Switch) && ast.Flags.TryGetValue("file", out tuple)) && (tuple.Item2 == parent));
        }

        private static List<CompletionResult> CompleteFileNameAsCommand(CompletionContext completionContext)
        {
            bool flag = CompletionCompleters.IsAmpersandNeeded(completionContext, true);
            List<CompletionResult> list = new List<CompletionResult>();
            bool flag2 = false;
            if (completionContext.Options == null)
            {
                Hashtable hashtable = new Hashtable();
                hashtable.Add("LiteralPaths", true);
                completionContext.Options = hashtable;
            }
            else if (!completionContext.Options.ContainsKey("LiteralPaths"))
            {
                completionContext.Options.Add("LiteralPaths", true);
                flag2 = true;
            }
            try
            {
                foreach (CompletionResult result in CompletionCompleters.CompleteFilename(completionContext))
                {
                    string completionText = result.CompletionText;
                    int length = completionText.Length;
                    if ((flag && (length > 2)) && (completionText[0].IsSingleQuote() && completionText[length - 1].IsSingleQuote()))
                    {
                        completionText = "& " + completionText;
                        list.Add(new CompletionResult(completionText, result.ListItemText, result.ResultType, result.ToolTip));
                    }
                    else
                    {
                        list.Add(result);
                    }
                }
            }
            finally
            {
                if (flag2)
                {
                    completionContext.Options.Remove("LiteralPaths");
                }
            }
            return list;
        }

        private static bool CompleteOperator(Token tokenAtCursor, Ast lastAst)
        {
            if (tokenAtCursor.Kind == TokenKind.Minus)
            {
                return (lastAst is BinaryExpressionAst);
            }
            return (((tokenAtCursor.Kind == TokenKind.Parameter) && (lastAst is CommandParameterAst)) && (lastAst.Parent is ExpressionAst));
        }

        internal CompletionContext CreateCompletionContext(ExecutionContext executionContext)
        {
            Func<Token, bool> predicate = null;
            Func<Token, bool> func2 = null;
            Token token = null;
            IScriptPosition positionForAstSearch = this._cursorPosition;
            bool flag = false;
            Token token2 = this._tokens.LastOrDefault<Token>(t => IsCursorWithinOrJustAfterExtent(this._cursorPosition, t.Extent) && IsInterestingToken(t));
            if (token2 == null)
            {
                if (predicate == null)
                {
                    predicate = t => IsCursorBeforeExtent(this._cursorPosition, t.Extent) && IsInterestingToken(t);
                }
                token = this._tokens.LastOrDefault<Token>(predicate);
                if (token != null)
                {
                    positionForAstSearch = token.Extent.EndScriptPosition;
                    flag = true;
                }
            }
            else
            {
                StringExpandableToken token3 = token2 as StringExpandableToken;
                if ((token3 != null) && (token3.NestedTokens != null))
                {
                    if (func2 == null)
                    {
                        func2 = t => IsCursorWithinOrJustAfterExtent(this._cursorPosition, t.Extent) && IsInterestingToken(t);
                    }
                    token2 = token3.NestedTokens.LastOrDefault<Token>(func2) ?? token3;
                }
            }
            IEnumerable<Ast> source = AstSearcher.FindAll(this._ast, ast => IsCursorWithinOrJustAfterExtent(positionForAstSearch, ast.Extent), true);
            return new CompletionContext { TokenAtCursor = token2, TokenBeforeCursor = token, CursorPosition = this._cursorPosition, RelatedAsts = source.ToList<Ast>(), Options = this._options, ExecutionContext = executionContext, ReplacementIndex = flag ? this._cursorPosition.Offset : 0 };
        }

        internal static TypeName FindTypeNameToComplete(ITypeName type, IScriptPosition cursor)
        {
            TypeName name = type as TypeName;
            if (name != null)
            {
                if ((cursor.Offset > type.Extent.StartOffset) && (cursor.Offset <= type.Extent.EndOffset))
                {
                    return name;
                }
                return null;
            }
            GenericTypeName name2 = type as GenericTypeName;
            if (name2 != null)
            {
                name = FindTypeNameToComplete(name2.TypeName, cursor);
                if (name != null)
                {
                    return name;
                }
                foreach (ITypeName name3 in name2.GenericArguments)
                {
                    name = FindTypeNameToComplete(name3, cursor);
                    if (name != null)
                    {
                        return name;
                    }
                }
                return null;
            }
            ArrayTypeName name4 = type as ArrayTypeName;
            if (name4 == null)
            {
                return null;
            }
            return (TypeName) (FindTypeNameToComplete(name4.ElementType, cursor) ?? null);
        }

        private static Ast GetLastAstAtCursor(ScriptBlockAst scriptBlockAst, IScriptPosition cursorPosition)
        {
            return AstSearcher.FindAll(scriptBlockAst, ast => IsCursorRightAfterExtent(cursorPosition, ast.Extent), true).LastOrDefault<Ast>();
        }

        private List<CompletionResult> GetResultForIdentifier(CompletionContext completionContext, ref int replacementIndex, ref int replacementLength, bool isQuotedString)
        {
            Token tokenAtCursor = completionContext.TokenAtCursor;
            Ast lastAst = completionContext.RelatedAsts.Last<Ast>();
            List<CompletionResult> source = null;
            completionContext.WordToComplete = tokenAtCursor.Text;
            StringConstantExpressionAst ast2 = lastAst as StringConstantExpressionAst;
            if ((ast2 != null) && ast2.Value.Equals("$", StringComparison.Ordinal))
            {
                completionContext.WordToComplete = "";
                return CompletionCompleters.CompleteVariable(completionContext);
            }
            if ((tokenAtCursor.TokenFlags & TokenFlags.CommandName) != TokenFlags.None)
            {
                if ((completionContext.RelatedAsts.Count > 0) && (completionContext.RelatedAsts[0] is ScriptBlockAst))
                {
                    Ast lastAstAtCursor = null;
                    InternalScriptPosition position = (InternalScriptPosition) this._cursorPosition;
                    int offset = position.Offset - tokenAtCursor.Text.Length;
                    if (offset >= 0)
                    {
                        InternalScriptPosition cursorPosition = position.CloneWithNewOffset(offset);
                        ScriptBlockAst scriptBlockAst = (ScriptBlockAst) completionContext.RelatedAsts[0];
                        lastAstAtCursor = GetLastAstAtCursor(scriptBlockAst, cursorPosition);
                    }
                    if (((lastAstAtCursor != null) && (lastAstAtCursor.Extent.EndLineNumber == tokenAtCursor.Extent.StartLineNumber)) && (lastAstAtCursor.Extent.EndColumnNumber == tokenAtCursor.Extent.StartColumnNumber))
                    {
                        if (tokenAtCursor.Text.IndexOfAny(new char[] { '\\', '/' }) == 0)
                        {
                            string str = CompletionCompleters.ConcatenateStringPathArguments(lastAstAtCursor as CommandElementAst, tokenAtCursor.Text, completionContext);
                            if (str != null)
                            {
                                completionContext.WordToComplete = str;
                                source = new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext));
                                if (source.Count > 0)
                                {
                                    replacementIndex = lastAstAtCursor.Extent.StartScriptPosition.Offset;
                                    replacementLength += lastAstAtCursor.Extent.Text.Length;
                                }
                                return source;
                            }
                            VariableExpressionAst variableAst = lastAstAtCursor as VariableExpressionAst;
                            string str2 = (variableAst != null) ? CompletionCompleters.CombineVariableWithPartialPath(variableAst, tokenAtCursor.Text, completionContext.ExecutionContext) : null;
                            if (str2 == null)
                            {
                                return source;
                            }
                            completionContext.WordToComplete = str2;
                            replacementIndex = lastAstAtCursor.Extent.StartScriptPosition.Offset;
                            replacementLength += lastAstAtCursor.Extent.Text.Length;
                            completionContext.ReplacementIndex = replacementIndex;
                            completionContext.ReplacementLength = replacementLength;
                        }
                        else if (!(lastAstAtCursor is ErrorExpressionAst) || !(lastAstAtCursor.Parent is IndexExpressionAst))
                        {
                            return source;
                        }
                    }
                }
                if (!isQuotedString)
                {
                    StringExpandableToken token2 = tokenAtCursor as StringExpandableToken;
                    if (((token2 != null) && (token2.NestedTokens != null)) && (ast2 != null))
                    {
                        try
                        {
                            string expandedString = null;
                            ExpandableStringExpressionAst expandableStringAst = new ExpandableStringExpressionAst(ast2.Extent, ast2.Value, StringConstantType.BareWord);
                            if (CompletionCompleters.IsPathSafelyExpandable(expandableStringAst, string.Empty, completionContext.ExecutionContext, out expandedString))
                            {
                                completionContext.WordToComplete = expandedString;
                            }
                            else
                            {
                                return source;
                            }
                        }
                        catch (Exception exception)
                        {
                            CommandProcessorBase.CheckForSevereException(exception);
                            return source;
                        }
                    }
                    source = CompleteFileNameAsCommand(completionContext);
                    List<CompletionResult> collection = CompletionCompleters.CompleteCommand(completionContext);
                    if ((collection != null) && (collection.Count > 0))
                    {
                        source.AddRange(collection);
                    }
                }
                return source;
            }
            if (((tokenAtCursor.Text.Length == 1) && tokenAtCursor.Text[0].IsDash()) && (lastAst.Parent is CommandAst))
            {
                if (isQuotedString)
                {
                    return source;
                }
                return CompletionCompleters.CompleteCommandParameter(completionContext);
            }
            TokenKind unknown = TokenKind.Unknown;
            bool flag = lastAst.Parent is MemberExpressionAst;
            bool @static = flag ? ((MemberExpressionAst) lastAst.Parent).Static : false;
            bool flag3 = false;
            if (!flag)
            {
                if (tokenAtCursor.Text.Equals(TokenKind.Dot.Text(), StringComparison.Ordinal))
                {
                    unknown = TokenKind.Dot;
                    flag = true;
                }
                else if (tokenAtCursor.Text.Equals(TokenKind.ColonColon.Text(), StringComparison.Ordinal))
                {
                    unknown = TokenKind.ColonColon;
                    flag = true;
                }
                else if (tokenAtCursor.Kind.Equals(TokenKind.Multiply) && (lastAst is BinaryExpressionAst))
                {
                    BinaryExpressionAst item = (BinaryExpressionAst) lastAst;
                    MemberExpressionAst left = item.Left as MemberExpressionAst;
                    IScriptExtent errorPosition = item.ErrorPosition;
                    if (((left != null) && (item.Operator == TokenKind.Multiply)) && (errorPosition.StartOffset == left.Member.Extent.EndOffset))
                    {
                        @static = left.Static;
                        unknown = @static ? TokenKind.ColonColon : TokenKind.Dot;
                        flag = true;
                        flag3 = true;
                        completionContext.RelatedAsts.Remove(item);
                        completionContext.RelatedAsts.Add(left);
                        StringConstantExpressionAst member = left.Member as StringConstantExpressionAst;
                        if (member != null)
                        {
                            replacementIndex = member.Extent.StartScriptPosition.Offset;
                            replacementLength += member.Extent.Text.Length;
                        }
                    }
                }
            }
            if (flag)
            {
                source = CompletionCompleters.CompleteMember(completionContext, @static || (unknown == TokenKind.ColonColon));
                if (source.Any<CompletionResult>())
                {
                    if (!flag3 && (unknown != TokenKind.Unknown))
                    {
                        replacementIndex += tokenAtCursor.Text.Length;
                        replacementLength = 0;
                    }
                    return source;
                }
            }
            if (lastAst.Parent is HashtableAst)
            {
                source = CompletionCompleters.CompleteHashtableKey(completionContext, (HashtableAst) lastAst.Parent);
                if ((source != null) && source.Any<CompletionResult>())
                {
                    return source;
                }
            }
            if (!isQuotedString)
            {
                bool flag4 = false;
                if ((lastAst.Parent is FileRedirectionAst) || CompleteAgainstSwitchFile(lastAst, completionContext.TokenBeforeCursor))
                {
                    string str4 = CompletionCompleters.ConcatenateStringPathArguments(lastAst as CommandElementAst, string.Empty, completionContext);
                    if (str4 != null)
                    {
                        flag4 = true;
                        completionContext.WordToComplete = str4;
                    }
                }
                else if (tokenAtCursor.Text.IndexOfAny(new char[] { '\\', '/' }) == 0)
                {
                    CommandBaseAst parent = lastAst.Parent as CommandBaseAst;
                    if ((parent != null) && parent.Redirections.Any<RedirectionAst>())
                    {
                        FileRedirectionAst ast11 = parent.Redirections[0] as FileRedirectionAst;
                        if (((ast11 != null) && (ast11.Extent.EndLineNumber == lastAst.Extent.StartLineNumber)) && (ast11.Extent.EndColumnNumber == lastAst.Extent.StartColumnNumber))
                        {
                            string str5 = CompletionCompleters.ConcatenateStringPathArguments(ast11.Location, tokenAtCursor.Text, completionContext);
                            if (str5 != null)
                            {
                                flag4 = true;
                                completionContext.WordToComplete = str5;
                                replacementIndex = ast11.Location.Extent.StartScriptPosition.Offset;
                                replacementLength += ast11.Location.Extent.EndScriptPosition.Offset - replacementIndex;
                                completionContext.ReplacementIndex = replacementIndex;
                                completionContext.ReplacementLength = replacementLength;
                            }
                        }
                    }
                }
                if (flag4)
                {
                    return new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext));
                }
                string str6 = CompletionCompleters.ConcatenateStringPathArguments(lastAst as CommandElementAst, string.Empty, completionContext);
                if (str6 != null)
                {
                    completionContext.WordToComplete = str6;
                }
                source = CompletionCompleters.CompleteCommandArgument(completionContext);
                replacementIndex = completionContext.ReplacementIndex;
                replacementLength = completionContext.ReplacementLength;
            }
            return source;
        }

        private List<CompletionResult> GetResultForString(CompletionContext completionContext, ref int replacementIndex, ref int replacementLength, bool isQuotedString)
        {
            if (isQuotedString)
            {
                return null;
            }
            Token tokenAtCursor = completionContext.TokenAtCursor;
            Ast ast = completionContext.RelatedAsts.Last<Ast>();
            List<CompletionResult> list = null;
            ExpandableStringExpressionAst ast2 = ast as ExpandableStringExpressionAst;
            StringConstantExpressionAst ast3 = ast as StringConstantExpressionAst;
            if ((ast3 != null) || (ast2 != null))
            {
                string input = (ast3 != null) ? ast3.Value : ast2.Value;
                StringConstantType type = (ast3 != null) ? ast3.StringConstantType : ast2.StringConstantType;
                string str2 = null;
                if (type == StringConstantType.DoubleQuoted)
                {
                    Match match = Regex.Match(input, @"(\$[\w\d]+\.[\w\d\*]*)$");
                    if (match.Success)
                    {
                        str2 = match.Groups[1].Value;
                    }
                    else if ((match = Regex.Match(input, @"(\[[\w\d\.]+\]::[\w\d\*]*)$")).Success)
                    {
                        str2 = match.Groups[1].Value;
                    }
                }
                if (str2 != null)
                {
                    int num3;
                    int num4;
                    int offset = tokenAtCursor.Extent.StartScriptPosition.Offset;
                    int length = (this._cursorPosition.Offset - offset) - 1;
                    if (length >= input.Length)
                    {
                        length = input.Length;
                    }
                    CompletionAnalysis analysis = new CompletionAnalysis(this._ast, this._tokens, this._cursorPosition, this._options);
                    CompletionContext context = analysis.CreateCompletionContext(completionContext.ExecutionContext);
                    context.Helper = completionContext.Helper;
                    List<CompletionResult> list2 = analysis.GetResultHelper(context, out num3, out num4, true);
                    if ((list2 != null) && (list2.Count > 0))
                    {
                        list = new List<CompletionResult>();
                        replacementIndex = (offset + 1) + (length - str2.Length);
                        replacementLength = str2.Length;
                        string str3 = str2.Substring(0, num3);
                        foreach (CompletionResult result in list2)
                        {
                            string completionText = str3 + result.CompletionText;
                            if (result.ResultType.Equals(CompletionResultType.Property))
                            {
                                completionText = TokenKind.DollarParen.Text() + completionText + TokenKind.RParen.Text();
                            }
                            else if (result.ResultType.Equals(CompletionResultType.Method))
                            {
                                completionText = TokenKind.DollarParen.Text() + completionText;
                            }
                            completionText = completionText + "\"";
                            list.Add(new CompletionResult(completionText, result.ListItemText, result.ResultType, result.ToolTip));
                        }
                    }
                    return list;
                }
                CommandElementAst stringAst = ast as CommandElementAst;
                string str5 = CompletionCompleters.ConcatenateStringPathArguments(stringAst, string.Empty, completionContext);
                if (str5 == null)
                {
                    return list;
                }
                completionContext.WordToComplete = str5;
                if ((ast.Parent is CommandAst) || (ast.Parent is CommandParameterAst))
                {
                    list = CompletionCompleters.CompleteCommandArgument(completionContext);
                    replacementIndex = completionContext.ReplacementIndex;
                    replacementLength = completionContext.ReplacementLength;
                    return list;
                }
                list = new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext));
                if (str5.IndexOf('-') != -1)
                {
                    List<CompletionResult> collection = CompletionCompleters.CompleteCommand(completionContext);
                    if ((collection != null) && (collection.Count > 0))
                    {
                        list.AddRange(collection);
                    }
                }
            }
            return list;
        }

        internal List<CompletionResult> GetResultHelper(CompletionContext completionContext, out int replacementIndex, out int replacementLength, bool isQuotedString)
        {
            replacementIndex = -1;
            replacementLength = -1;
            Token tokenAtCursor = completionContext.TokenAtCursor;
            Ast lastAst = completionContext.RelatedAsts.Last<Ast>();
            List<CompletionResult> list = null;
            if (tokenAtCursor == null)
            {
                if (!isQuotedString && (((((lastAst is CommandParameterAst) || (lastAst is CommandAst)) || ((lastAst is ExpressionAst) && (lastAst.Parent is CommandAst))) || ((lastAst is ExpressionAst) && (lastAst.Parent is CommandParameterAst))) || (((lastAst is ExpressionAst) && (lastAst.Parent is ArrayLiteralAst)) && ((lastAst.Parent.Parent is CommandAst) || (lastAst.Parent.Parent is CommandParameterAst)))))
                {
                    completionContext.WordToComplete = string.Empty;
                    HashtableAst hashtableAst = lastAst as HashtableAst;
                    if (hashtableAst != null)
                    {
                        completionContext.ReplacementIndex = replacementIndex = completionContext.CursorPosition.Offset;
                        completionContext.ReplacementLength = replacementLength = 0;
                        list = CompletionCompleters.CompleteHashtableKey(completionContext, hashtableAst);
                    }
                    else
                    {
                        list = CompletionCompleters.CompleteCommandArgument(completionContext);
                        replacementIndex = completionContext.ReplacementIndex;
                        replacementLength = completionContext.ReplacementLength;
                    }
                }
                else if (!isQuotedString)
                {
                    bool flag = false;
                    if ((lastAst is ErrorExpressionAst) && (lastAst.Parent is FileRedirectionAst))
                    {
                        flag = true;
                    }
                    else if ((lastAst is ErrorStatementAst) && CompleteAgainstSwitchFile(lastAst, completionContext.TokenBeforeCursor))
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        completionContext.WordToComplete = string.Empty;
                        list = new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext));
                        replacementIndex = completionContext.ReplacementIndex;
                        replacementLength = completionContext.ReplacementLength;
                    }
                }
            }
            else
            {
                TokenKind kind;
                replacementIndex = tokenAtCursor.Extent.StartScriptPosition.Offset;
                replacementLength = tokenAtCursor.Extent.EndScriptPosition.Offset - replacementIndex;
                completionContext.ReplacementIndex = replacementIndex;
                completionContext.ReplacementLength = replacementLength;
                switch (tokenAtCursor.Kind)
                {
                    case TokenKind.ColonColon:
                    case TokenKind.Dot:
                        replacementIndex += tokenAtCursor.Text.Length;
                        replacementLength = 0;
                        list = CompletionCompleters.CompleteMember(completionContext, tokenAtCursor.Kind == TokenKind.ColonColon);
                        goto Label_05DC;

                    case TokenKind.Multiply:
                    case TokenKind.Identifier:
                    case TokenKind.Generic:
                        list = this.GetResultForIdentifier(completionContext, ref replacementIndex, ref replacementLength, isQuotedString);
                        goto Label_05DC;

                    case TokenKind.Minus:
                        if (CompleteOperator(tokenAtCursor, lastAst))
                        {
                            list = CompletionCompleters.CompleteOperator("");
                        }
                        else if (CompleteAgainstStatementFlags(completionContext.RelatedAsts[0], null, tokenAtCursor, out kind))
                        {
                            completionContext.WordToComplete = tokenAtCursor.Text;
                            list = CompletionCompleters.CompleteStatementFlags(kind, completionContext.WordToComplete);
                        }
                        goto Label_05DC;

                    case TokenKind.Redirection:
                        if ((lastAst is ErrorExpressionAst) && (lastAst.Parent is FileRedirectionAst))
                        {
                            completionContext.WordToComplete = string.Empty;
                            completionContext.ReplacementIndex = replacementIndex += tokenAtCursor.Text.Length;
                            completionContext.ReplacementLength = replacementLength = 0;
                            list = new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext));
                        }
                        goto Label_05DC;

                    case TokenKind.Variable:
                    case TokenKind.SplattedVariable:
                        completionContext.WordToComplete = ((VariableToken) tokenAtCursor).VariablePath.UserPath;
                        list = CompletionCompleters.CompleteVariable(completionContext);
                        goto Label_05DC;

                    case TokenKind.Parameter:
                        if (!isQuotedString)
                        {
                            completionContext.WordToComplete = tokenAtCursor.Text;
                            CommandAst parent = lastAst.Parent as CommandAst;
                            if ((!(lastAst is StringConstantExpressionAst) || (parent == null)) || (parent.CommandElements.Count != 1))
                            {
                                if (CompleteAgainstStatementFlags(null, lastAst, null, out kind))
                                {
                                    list = CompletionCompleters.CompleteStatementFlags(kind, completionContext.WordToComplete);
                                }
                                else if (CompleteOperator(tokenAtCursor, lastAst))
                                {
                                    list = CompletionCompleters.CompleteOperator(completionContext.WordToComplete);
                                }
                                else if (completionContext.WordToComplete.EndsWith(":", StringComparison.Ordinal))
                                {
                                    replacementIndex = tokenAtCursor.Extent.EndScriptPosition.Offset;
                                    replacementLength = 0;
                                    completionContext.WordToComplete = string.Empty;
                                    list = CompletionCompleters.CompleteCommandArgument(completionContext);
                                }
                                else
                                {
                                    list = CompletionCompleters.CompleteCommandParameter(completionContext);
                                }
                            }
                            else
                            {
                                list = CompleteFileNameAsCommand(completionContext);
                            }
                        }
                        goto Label_05DC;

                    case TokenKind.Number:
                        if ((lastAst is ConstantExpressionAst) && (((lastAst.Parent is CommandAst) || (lastAst.Parent is CommandParameterAst)) || ((lastAst.Parent is ArrayLiteralAst) && ((lastAst.Parent.Parent is CommandAst) || (lastAst.Parent.Parent is CommandParameterAst)))))
                        {
                            completionContext.WordToComplete = tokenAtCursor.Text;
                            list = CompletionCompleters.CompleteCommandArgument(completionContext);
                            replacementIndex = completionContext.ReplacementIndex;
                            replacementLength = completionContext.ReplacementLength;
                        }
                        goto Label_05DC;

                    case TokenKind.Comment:
                        if (!isQuotedString)
                        {
                            completionContext.WordToComplete = tokenAtCursor.Text;
                            list = CompletionCompleters.CompleteComment(completionContext);
                        }
                        goto Label_05DC;

                    case TokenKind.StringLiteral:
                    case TokenKind.StringExpandable:
                        list = this.GetResultForString(completionContext, ref replacementIndex, ref replacementLength, isQuotedString);
                        goto Label_05DC;

                    case TokenKind.RBracket:
                        if (lastAst is TypeExpressionAst)
                        {
                            TypeExpressionAst targetExpr = (TypeExpressionAst) lastAst;
                            List<CompletionResult> results = new List<CompletionResult>();
                            CompletionCompleters.CompleteMemberHelper(true, "*", targetExpr, completionContext, results);
                            if (results.Count > 0)
                            {
                                replacementIndex++;
                                replacementLength = 0;
                                list = (from entry in results
                                    let completionText = TokenKind.ColonColon.Text() + entry.CompletionText
                                    select new CompletionResult(completionText, entry.ListItemText, entry.ResultType, entry.ToolTip)).ToList<CompletionResult>();
                            }
                        }
                        goto Label_05DC;

                    case TokenKind.Comma:
                        if ((lastAst is ErrorExpressionAst) && ((lastAst.Parent is CommandAst) || (lastAst.Parent is CommandParameterAst)))
                        {
                            replacementIndex += replacementLength;
                            replacementLength = 0;
                            list = CompletionCompleters.CompleteCommandArgument(completionContext);
                        }
                        goto Label_05DC;
                }
                if ((tokenAtCursor.TokenFlags & TokenFlags.Keyword) != TokenFlags.None)
                {
                    completionContext.WordToComplete = tokenAtCursor.Text;
                    list = CompleteFileNameAsCommand(completionContext);
                    List<CompletionResult> collection = CompletionCompleters.CompleteCommand(completionContext);
                    if ((collection != null) && (collection.Count > 0))
                    {
                        list.AddRange(collection);
                    }
                }
                else
                {
                    replacementIndex = -1;
                    replacementLength = -1;
                }
            }
        Label_05DC:
            if ((list == null) || (list.Count == 0))
            {
                TypeExpressionAst ast5 = completionContext.RelatedAsts.OfType<TypeExpressionAst>().FirstOrDefault<TypeExpressionAst>();
                TypeName name = null;
                if (ast5 != null)
                {
                    name = FindTypeNameToComplete(ast5.TypeName, this._cursorPosition);
                }
                else
                {
                    TypeConstraintAst ast6 = completionContext.RelatedAsts.OfType<TypeConstraintAst>().FirstOrDefault<TypeConstraintAst>();
                    if (ast6 != null)
                    {
                        name = FindTypeNameToComplete(ast6.TypeName, this._cursorPosition);
                    }
                }
                if (name != null)
                {
                    replacementIndex = name.Extent.StartOffset;
                    replacementLength = name.Extent.EndOffset - replacementIndex;
                    completionContext.WordToComplete = name.FullName;
                    list = CompletionCompleters.CompleteType(completionContext, "", "");
                }
            }
            if ((list == null) || (list.Count == 0))
            {
                HashtableAst ast7 = lastAst as HashtableAst;
                if (ast7 != null)
                {
                    completionContext.ReplacementIndex = replacementIndex = completionContext.CursorPosition.Offset;
                    completionContext.ReplacementLength = replacementLength = 0;
                    list = CompletionCompleters.CompleteHashtableKey(completionContext, ast7);
                }
            }
            if ((list == null) || (list.Count == 0))
            {
                string text = completionContext.RelatedAsts[0].Extent.Text;
                if ((Regex.IsMatch(text, @"^[\S]+$") && (completionContext.RelatedAsts.Count > 0)) && (completionContext.RelatedAsts[0] is ScriptBlockAst))
                {
                    replacementIndex = completionContext.RelatedAsts[0].Extent.StartScriptPosition.Offset;
                    replacementLength = completionContext.RelatedAsts[0].Extent.EndScriptPosition.Offset - replacementIndex;
                    completionContext.WordToComplete = text;
                    list = CompleteFileNameAsCommand(completionContext);
                }
            }
            return list;
        }

        internal List<CompletionResult> GetResults(PowerShell powerShell, out int replacementIndex, out int replacementLength)
        {
            CompletionContext completionContext = this.CreateCompletionContext(powerShell.GetContextFromTLS());
            completionContext.Helper = new CompletionExecutionHelper(powerShell);
            return this.GetResultHelper(completionContext, out replacementIndex, out replacementLength, false);
        }

        private static bool IsCursorBeforeExtent(IScriptPosition cursor, IScriptExtent extent)
        {
            return (extent.EndOffset < cursor.Offset);
        }

        private static bool IsCursorRightAfterExtent(IScriptPosition cursor, IScriptExtent extent)
        {
            return (cursor.Offset == extent.EndOffset);
        }

        private static bool IsCursorWithinOrJustAfterExtent(IScriptPosition cursor, IScriptExtent extent)
        {
            return ((cursor.Offset > extent.StartOffset) && (cursor.Offset <= extent.EndOffset));
        }

        private static bool IsInterestingToken(Token token)
        {
            return ((token.Kind != TokenKind.NewLine) && (token.Kind != TokenKind.EndOfInput));
        }

        private static bool IsTokenTheSame(Token x, Token y)
        {
            return ((((x.Kind == y.Kind) && (x.TokenFlags == y.TokenFlags)) && ((x.Extent.StartLineNumber == y.Extent.StartLineNumber) && (x.Extent.StartColumnNumber == y.Extent.StartColumnNumber))) && ((x.Extent.EndLineNumber == y.Extent.EndLineNumber) && (x.Extent.EndColumnNumber == y.Extent.EndColumnNumber)));
        }
    }
}

