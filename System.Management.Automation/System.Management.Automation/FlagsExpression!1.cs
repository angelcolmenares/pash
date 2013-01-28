namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    public sealed class FlagsExpression<T> where T: struct, IConvertible
    {
        private Node _root;
        private Type _underType;

        public FlagsExpression(object[] expression)
        {
            if (!typeof(T).IsEnum)
            {
                throw InterpreterError.NewInterpreterException(expression, typeof(RuntimeException), null, "InvalidGenericType", EnumExpressionEvaluatorStrings.InvalidGenericType, new object[0]);
            }
            this._underType = Enum.GetUnderlyingType(typeof(T));
            if (expression == null)
            {
                throw InterpreterError.NewInterpreterException(null, typeof(ArgumentNullException), null, "EmptyInputString", EnumExpressionEvaluatorStrings.EmptyInputString, new object[0]);
            }
            foreach (string str in expression)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    throw InterpreterError.NewInterpreterException(expression, typeof(RuntimeException), null, "EmptyInputString", EnumExpressionEvaluatorStrings.EmptyInputString, new object[0]);
                }
            }
            List<Token> tokenList = new List<Token>();
            foreach (string str2 in expression)
            {
                tokenList.AddRange(this.TokenizeInput(str2));
                tokenList.Add(new Token(TokenKind.Or));
            }
            this.CheckSyntaxError(tokenList);
            this._root = this.ConstructExpressionTree(tokenList);
        }

        public FlagsExpression(string expression)
        {
            if (!typeof(T).IsEnum)
            {
                throw InterpreterError.NewInterpreterException(expression, typeof(RuntimeException), null, "InvalidGenericType", EnumExpressionEvaluatorStrings.InvalidGenericType, new object[0]);
            }
            this._underType = Enum.GetUnderlyingType(typeof(T));
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw InterpreterError.NewInterpreterException(expression, typeof(RuntimeException), null, "EmptyInputString", EnumExpressionEvaluatorStrings.EmptyInputString, new object[0]);
            }
            List<Token> tokenList = this.TokenizeInput(expression);
            tokenList.Add(new Token(TokenKind.Or));
            this.CheckSyntaxError(tokenList);
            this._root = this.ConstructExpressionTree(tokenList);
        }

        private void CheckSyntaxError(List<Token> tokenList)
        {
            TokenKind or = TokenKind.Or;
            for (int i = 0; i < tokenList.Count; i++)
            {
                Token token = tokenList[i];
                switch (or)
                {
                    case TokenKind.Or:
                    case TokenKind.And:
                        if ((token.Kind == TokenKind.Or) || (token.Kind == TokenKind.And))
                        {
                            throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "SyntaxErrorUnexpectedBinaryOperator", EnumExpressionEvaluatorStrings.SyntaxErrorUnexpectedBinaryOperator, new object[0]);
                        }
                        break;

                    default:
                        if (or == TokenKind.Not)
                        {
                            if (token.Kind != TokenKind.Identifier)
                            {
                                throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "SyntaxErrorIdentifierExpected", EnumExpressionEvaluatorStrings.SyntaxErrorIdentifierExpected, new object[0]);
                            }
                        }
                        else if ((or == TokenKind.Identifier) && ((token.Kind == TokenKind.Identifier) || (token.Kind == TokenKind.Not)))
                        {
                            throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "SyntaxErrorBinaryOperatorExpected", EnumExpressionEvaluatorStrings.SyntaxErrorBinaryOperatorExpected, new object[0]);
                        }
                        break;
                }
                if (token.Kind == TokenKind.Identifier)
                {
                    string text = token.Text;
                    CompareInfo.GetCompareInfo(CultureInfo.InvariantCulture.LCID);
                    token.Text = EnumMinimumDisambiguation.EnumDisambiguate(text, typeof(T));
                }
                or = token.Kind;
            }
        }

        private Node ConstructExpressionTree(List<Token> tokenList)
        {
            bool flag = false;
            Queue<Node> queue = new Queue<Node>();
            Queue<Node> queue2 = new Queue<Node>();
            for (int i = 0; i < tokenList.Count; i++)
            {
                Token token = tokenList[i];
                TokenKind kind = token.Kind;
                if (kind == TokenKind.Identifier)
                {
                    Node item = new OperandNode(token.Text);
                    if (flag)
                    {
                        Node node2 = new NotNode {
                            Operand1 = item
                        };
                        flag = false;
                        queue.Enqueue(node2);
                    }
                    else
                    {
                        queue.Enqueue(item);
                    }
                }
                else if (kind == TokenKind.Not)
                {
                    flag = true;
                }
                else if ((kind != TokenKind.And) && (kind == TokenKind.Or))
                {
                    Node node3 = queue.Dequeue();
                    while (queue.Count > 0)
                    {
                        node3 = new AndNode(node3) {
                            Operand1 = queue.Dequeue()
                        };
                    }
                    queue2.Enqueue(node3);
                }
            }
            Node n = queue2.Dequeue();
            while (queue2.Count > 0)
            {
                n = new OrNode(n) {
                    Operand1 = queue2.Dequeue()
                };
            }
            return n;
        }

        public bool Evaluate(T value)
        {
            object val = LanguagePrimitives.ConvertTo(value, this._underType, CultureInfo.InvariantCulture);
            return this._root.Eval(val);
        }

        internal bool ExistsInExpression(T flagName)
        {
            object enumVal = LanguagePrimitives.ConvertTo(flagName, this._underType, CultureInfo.InvariantCulture);
            return this._root.ExistEnum(enumVal);
        }

        private void FindNextToken(string input, ref int _offset)
        {
            while (_offset < input.Length)
            {
                char c = input[_offset++];
                if (!char.IsWhiteSpace(c))
                {
                    _offset--;
                    return;
                }
            }
        }

        private Token GetNextToken(string input, ref int _offset)
        {
            string str;
            StringBuilder builder = new StringBuilder();
            for (bool flag = false; _offset < input.Length; flag = true)
            {
                char ch = input[_offset++];
                switch (ch)
                {
                    case ',':
                    case '+':
                    case '!':
                        if (!flag)
                        {
                            builder.Append(ch);
                        }
                        else
                        {
                            _offset--;
                        }
                        goto Label_0054;
                }
                builder.Append(ch);
            }
        Label_0054:
            str = builder.ToString().Trim();
            if ((str.Length >= 2) && (((str[0] == '\'') && (str[str.Length - 1] == '\'')) || ((str[0] == '"') && (str[str.Length - 1] == '"'))))
            {
                str = str.Substring(1, str.Length - 2);
            }
            str = str.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                throw InterpreterError.NewInterpreterException(input, typeof(RuntimeException), null, "EmptyTokenString", EnumExpressionEvaluatorStrings.EmptyTokenString, new object[] { EnumMinimumDisambiguation.EnumAllValues(typeof(T)) });
            }
            if (str[0] == '(')
            {
                int index = input.IndexOf(')', _offset);
                if ((str[str.Length - 1] == ')') || (index >= 0))
                {
                    throw InterpreterError.NewInterpreterException(input, typeof(RuntimeException), null, "NoIdentifierGroupingAllowed", EnumExpressionEvaluatorStrings.NoIdentifierGroupingAllowed, new object[0]);
                }
            }
            if (str.Equals(","))
            {
                return new Token(TokenKind.Or);
            }
            if (str.Equals("+"))
            {
                return new Token(TokenKind.And);
            }
            if (str.Equals("!"))
            {
                return new Token(TokenKind.Not);
            }
            return new Token(str);
        }

        private List<Token> TokenizeInput(string input)
        {
            List<Token> list = new List<Token>();
            int num = 0;
            while (num < input.Length)
            {
                this.FindNextToken(input, ref num);
                if (num < input.Length)
                {
                    list.Add(this.GetNextToken(input, ref num));
                }
            }
            return list;
        }

        internal Node Root
        {
            get
            {
                return this._root;
            }
            set
            {
                this._root = value;
            }
        }

        internal class AndNode : FlagsExpression<T>.Node
        {
            private FlagsExpression<T>.Node _operand2;

            public AndNode(FlagsExpression<T>.Node n)
            {
                this._operand2 = n;
            }

            internal override bool Eval(object val)
            {
                return (base.Operand1.Eval(val) && this.Operand2.Eval(val));
            }

            internal override bool ExistEnum(object enumVal)
            {
                return (base.Operand1.ExistEnum(enumVal) || this.Operand2.ExistEnum(enumVal));
            }

            public FlagsExpression<T>.Node Operand2
            {
                get
                {
                    return this._operand2;
                }
                set
                {
                    this._operand2 = value;
                }
            }
        }

        internal abstract class Node
        {
            private FlagsExpression<T>.Node _operand1;

            protected Node()
            {
            }

            internal abstract bool Eval(object val);
            internal abstract bool ExistEnum(object enumVal);

            public FlagsExpression<T>.Node Operand1
            {
                get
                {
                    return this._operand1;
                }
                set
                {
                    this._operand1 = value;
                }
            }
        }

        internal class NotNode : FlagsExpression<T>.Node
        {
            internal override bool Eval(object val)
            {
                return !base.Operand1.Eval(val);
            }

            internal override bool ExistEnum(object enumVal)
            {
                return base.Operand1.ExistEnum(enumVal);
            }
        }

        internal class OperandNode : FlagsExpression<T>.Node
        {
            internal object _operandValue;

            internal OperandNode(string enumString)
            {
                Type enumType = typeof(T);
                Type underlyingType = Enum.GetUnderlyingType(enumType);
                FieldInfo field = enumType.GetField(enumString);
                this._operandValue = LanguagePrimitives.ConvertTo(field.GetValue(enumType), underlyingType, CultureInfo.InvariantCulture);
            }

            internal override bool Eval(object val)
            {
                Type underlyingType = Enum.GetUnderlyingType(typeof(T));
                if (this.isUnsigned(underlyingType))
                {
                    ulong num = (ulong) LanguagePrimitives.ConvertTo(val, typeof(ulong), CultureInfo.InvariantCulture);
                    ulong num2 = (ulong) LanguagePrimitives.ConvertTo(this._operandValue, typeof(ulong), CultureInfo.InvariantCulture);
                    return (num2 == (num & num2));
                }
                long num3 = (long) LanguagePrimitives.ConvertTo(val, typeof(long), CultureInfo.InvariantCulture);
                long num4 = (long) LanguagePrimitives.ConvertTo(this._operandValue, typeof(long), CultureInfo.InvariantCulture);
                return (num4 == (num3 & num4));
            }

            internal override bool ExistEnum(object enumVal)
            {
                Type underlyingType = Enum.GetUnderlyingType(typeof(T));
                if (this.isUnsigned(underlyingType))
                {
                    ulong num = (ulong) LanguagePrimitives.ConvertTo(enumVal, typeof(ulong), CultureInfo.InvariantCulture);
                    ulong num2 = (ulong) LanguagePrimitives.ConvertTo(this._operandValue, typeof(ulong), CultureInfo.InvariantCulture);
                    return (num == (num & num2));
                }
                long num3 = (long) LanguagePrimitives.ConvertTo(enumVal, typeof(long), CultureInfo.InvariantCulture);
                long num4 = (long) LanguagePrimitives.ConvertTo(this._operandValue, typeof(long), CultureInfo.InvariantCulture);
                return (num3 == (num3 & num4));
            }

            private bool isUnsigned(Type type)
            {
                if ((!(type == typeof(ulong)) && !(type == typeof(int))) && !(type == typeof(ushort)))
                {
                    return (type == typeof(byte));
                }
                return true;
            }

            public object OperandValue
            {
                get
                {
                    return this._operandValue;
                }
                set
                {
                    this._operandValue = value;
                }
            }
        }

        internal class OrNode : FlagsExpression<T>.Node
        {
            private FlagsExpression<T>.Node _operand2;

            public OrNode(FlagsExpression<T>.Node n)
            {
                this._operand2 = n;
            }

            internal override bool Eval(object val)
            {
                return (base.Operand1.Eval(val) || this.Operand2.Eval(val));
            }

            internal override bool ExistEnum(object enumVal)
            {
                return (base.Operand1.ExistEnum(enumVal) || this.Operand2.ExistEnum(enumVal));
            }

            public FlagsExpression<T>.Node Operand2
            {
                get
                {
                    return this._operand2;
                }
                set
                {
                    this._operand2 = value;
                }
            }
        }

        internal class Token
        {
            private FlagsExpression<T>.TokenKind _kind;
            private string _text;

            internal Token(FlagsExpression<T>.TokenKind kind)
            {
                this._kind = kind;
                switch (kind)
                {
                    case FlagsExpression<T>.TokenKind.And:
                        this._text = "AND";
                        return;

                    case FlagsExpression<T>.TokenKind.Or:
                        this._text = "OR";
                        return;

                    case FlagsExpression<T>.TokenKind.Not:
                        this._text = "NOT";
                        return;
                }
            }

            internal Token(string identifier)
            {
                this.Kind = FlagsExpression<T>.TokenKind.Identifier;
                this.Text = identifier;
            }

            public FlagsExpression<T>.TokenKind Kind
            {
                get
                {
                    return this._kind;
                }
                set
                {
                    this._kind = value;
                }
            }

            public string Text
            {
                get
                {
                    return this._text;
                }
                set
                {
                    this._text = value;
                }
            }
        }

        internal enum TokenKind
        {
            And,
            Identifier,
            Not,
            Or
        }
    }
}

