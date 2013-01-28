namespace Microsoft.ActiveDirectory.Management
{
    using System;
    using System.IO;

    internal class QueryParser
    {
        private IADOPathNode _filterExprTree;
        private string _query;
        private ConvertSearchFilterDelegate _searchFilterConverter;
        private VariableExpressionConverter _varExpressionConverter;
        public static short AND = 0x109;
        public static short HEXNUMVAL = 0x105;
        private Yylex lexer;
        public static short LPAREN = 280;
        public static short NOT = 0x10a;
        public static short NUMVAL = 260;
        public static short OP_APPROX = 0x10d;
        public static short OP_BAND = 270;
        public static short OP_BOR = 0x10f;
        public static short OP_EQ = 0x10b;
        public static short OP_GE = 0x113;
        public static short OP_GT = 0x111;
        public static short OP_LE = 0x114;
        public static short OP_LIKE = 0x115;
        public static short OP_LT = 0x112;
        public static short OP_NE = 0x10c;
        public static short OP_NOTLIKE = 0x116;
        public static short OP_NOTSUP = 0x117;
        public static short OP_RECURSIVEMATCH = 0x110;
        public static short OR = 0x108;
        private int pos;
        public static short PROP = 0x101;
        public static short RPAREN = 0x119;
        private int stateptr;
        private int[] statestk;
        public static short STRVAL1 = 0x102;
        public static short STRVAL2 = 0x103;
        private int valptr;
        private object[] valstk;
        public static short VAR1 = 0x106;
        public static short VAR2 = 0x107;
        internal string version = (YYMAJOR.ToString() + '.' + YYMINOR.ToString());
        private int yychar;
        private static short[] yycheck = new short[] { 
            0x109, 0, 0, 0, 0x117, 0, 0x101, 10, 0, 0x108, 0x109, 0x106, 0x107, 0, 0x117, 0x10a, 
            0, 0x108, 0x109, -1, 4, 5, -1, -1, 0x117, -1, 0x119, 4, 5, 280, 14, 15, 
            0x117, -1, 0x12, -1, -1, 14, 15, -1, -1, 0x12, 0x10b, 0x10c, 0x10d, 270, 0x10f, 0x110, 
            0x111, 0x112, 0x113, 0x114, 0x115, 0x116, 0x117, 20, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 
            0x1d, 30, 0x1f, 0x102, 0x103, 260, 0x105, 0x106, 0x107, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
            -1, -1, -1, -1, -1, -1, -1, -1, -1, 0x108, 0x109, 0x108, 0x109, 0x108, 0x101, -1, 
            0x108, 0x109, -1, 0x106, 0x107, -1, -1, 0x10a, -1, -1, 0x119, 0x119, 0x119, -1, 0x119, 0x117, 
            -1, -1, -1, -1, -1, 280
         };
        private bool yydebug;
        private static short[] yydefred = new short[] { 
            0, 0x1d, 30, 0x1f, 0, 0, 1, 0, 0, 3, 0, 0x1c, 0, 0, 0, 0, 
            0x16, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            6, 0, 0, 0, 0x18, 0x19, 0x1a, 0x1b, 9, 0x17, 10, 0x11, 0x12, 0x13, 20, 11, 
            12, 13, 14, 15, 0x10, 0x15
         };
        private static short[] yydgoto = new short[] { 7, 8, 9, 0x12, 10, 40, 0x29 };
        internal static short YYERRCODE = 0x100;
        private int yyerrflag;
        private static short YYFINAL = 7;
        private static short[] yygindex = new short[] { 0, 0x17, 0, -3, 0, 0x23, 0x10 };
        private static short[] yylen = new short[] { 
            2, 1, 2, 1, 3, 3, 3, 2, 3, 3, 3, 3, 3, 3, 3, 3, 
            3, 3, 3, 3, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
         };
        private static short[] yylhs = new short[] { 
            -1, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 
            2, 2, 2, 2, 2, 2, 3, 5, 5, 5, 5, 5, 4, 4, 6, 6
         };
        private object yylval;
        private static int YYMAJOR = 1;
        private static short YYMAXTOKEN = 0x119;
        private static int YYMINOR = 9;
        private static string[] yyname;
        private int yynerrs;
        private static short[] yyrindex = new short[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 5, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0
         };
        private static string[] yyrule;
        private static short[] yysindex = new short[] { 
            13, 0, 0, 0, -251, -251, 0, 0, 8, 0, -225, 0, -275, -255, -251, -251, 
            0, 0, -251, -191, -191, -191, -191, -191, -191, -191, -191, -191, -191, -191, -191, -191, 
            0, -265, -275, -247, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0
         };
        private static int YYSTACKSIZE = 500;
        private static short[] yytable = new short[] { 
            15, 7, 8, 4, 0x10, 5, 1, 0x1f, 0x11, 14, 15, 2, 3, 6, 0x10, 4, 
            11, 14, 15, 0, 11, 11, 0, 0, 0x10, 0, 0x20, 12, 13, 5, 11, 11, 
            0x10, 0, 11, 0, 0, 0x21, 0x22, 0, 0, 0x23, 0x13, 20, 0x15, 0x16, 0x17, 0x18, 
            0x19, 0x1a, 0x1b, 0x1c, 0x1d, 30, 0x10, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 50, 
            0x33, 0x34, 0x35, 0x24, 0x25, 0x26, 0x27, 2, 3, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 7, 4, 4, 5, 1, 0, 
            14, 15, 0, 2, 3, 0, 0, 4, 0, 0, 7, 8, 4, 0, 5, 0x10, 
            0, 0, 0, 0, 0, 5
         };
        private static int YYTABLESIZE = 0x125;
        private object yyval;

        static QueryParser()
        {
            string[] strArray = new string[0x11a];
            strArray[0] = "end-of-file";
            strArray[0x101] = "PROP";
            strArray[0x102] = "STRVAL1";
            strArray[0x103] = "STRVAL2";
            strArray[260] = "NUMVAL";
            strArray[0x105] = "HEXNUMVAL";
            strArray[0x106] = "VAR1";
            strArray[0x107] = "VAR2";
            strArray[0x108] = "OR";
            strArray[0x109] = "AND";
            strArray[0x10a] = "NOT";
            strArray[0x10b] = "OP_EQ";
            strArray[0x10c] = "OP_NE";
            strArray[0x10d] = "OP_APPROX";
            strArray[270] = "OP_BAND";
            strArray[0x10f] = "OP_BOR";
            strArray[0x110] = "OP_RECURSIVEMATCH";
            strArray[0x111] = "OP_GT";
            strArray[0x112] = "OP_LT";
            strArray[0x113] = "OP_GE";
            strArray[0x114] = "OP_LE";
            strArray[0x115] = "OP_LIKE";
            strArray[0x116] = "OP_NOTLIKE";
            strArray[0x117] = "OP_NOTSUP";
            strArray[280] = "LPAREN";
            strArray[0x119] = "RPAREN";
            yyname = strArray;
            yyrule = new string[] { 
                "$accept : query", @"query : '\000'", @"query : exp '\000'", "exp : relexp", "exp : exp AND exp", "exp : exp OR exp", "exp : LPAREN exp RPAREN", "exp : NOT exp", "exp : exp opnotsupported exp", "relexp : property OP_EQ value", "relexp : property OP_NE value", "relexp : property OP_GT value", "relexp : property OP_LT value", "relexp : property OP_GE value", "relexp : property OP_LE value", "relexp : property OP_LIKE value", 
                "relexp : property OP_NOTLIKE value", "relexp : property OP_APPROX value", "relexp : property OP_BAND value", "relexp : property OP_BOR value", "relexp : property OP_RECURSIVEMATCH value", "relexp : property opnotsupported value", "opnotsupported : OP_NOTSUP", "value : variable", "value : STRVAL1", "value : STRVAL2", "value : NUMVAL", "value : HEXNUMVAL", "property : variable", "property : PROP", "variable : VAR1", "variable : VAR2"
             };
        }

        internal QueryParser(string query, VariableExpressionConverter varExpressionConverter, ConvertSearchFilterDelegate searchFilterConverterDelegate)
        {
            if (varExpressionConverter == null)
            {
                throw new ArgumentNullException("varExpressionConverter");
            }
            if (searchFilterConverterDelegate == null)
            {
                throw new ArgumentNullException("searchFilterConverterDelegate");
            }
            if ((query == null) || string.IsNullOrEmpty(query.Trim()))
            {
                throw new ADFilterParsingException("Filter query cannot be null or empty");
            }
            this._query = query;
            this._varExpressionConverter = varExpressionConverter;
            this._searchFilterConverter = searchFilterConverterDelegate;
            this.yydebug = false;
            this.Parse(query);
        }

        private void debug(string msg)
        {
            if (this.yydebug)
            {
                Console.WriteLine(msg);
            }
        }

        private void dump_stacks(int count)
        {
            Console.WriteLine(string.Concat(new object[] { "=index==state====value=     s:", this.stateptr, "  v:", this.valptr }));
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(string.Concat(new object[] { " ", i, "    ", this.statestk[i], "      ", this.valstk[i] }));
            }
            Console.WriteLine("======================");
        }

        private bool init_stacks()
        {
            this.statestk = new int[YYSTACKSIZE];
            this.stateptr = -1;
            this.val_init();
            return true;
        }

        private void Parse(string query)
        {
            StringReader reader = new StringReader(query);
            this.lexer = new Yylex(reader);
            this.yyparse();
        }

        private void state_drop(int cnt)
        {
            int num = this.stateptr - cnt;
            if (num >= 0)
            {
                this.stateptr = num;
            }
        }

        private int state_peek(int relative)
        {
            int index = this.stateptr - relative;
            if (index < 0)
            {
                return -1;
            }
            return this.statestk[index];
        }

        private int state_pop()
        {
            if (this.stateptr < 0)
            {
                return -1;
            }
            return this.statestk[this.stateptr--];
        }

        private void state_push(int state)
        {
            if (this.stateptr < YYSTACKSIZE)
            {
                this.statestk[++this.stateptr] = state;
            }
        }

        private void val_drop(int cnt)
        {
            int num = this.valptr - cnt;
            if (num >= 0)
            {
                this.valptr = num;
            }
        }

        private void val_init()
        {
            this.valstk = new object[YYSTACKSIZE];
            this.yyval = 0;
            this.yylval = 0;
            this.valptr = -1;
        }

        private object val_peek(int relative)
        {
            int index = this.valptr - relative;
            if (index < 0)
            {
                return -1;
            }
            return this.valstk[index];
        }

        private object val_pop()
        {
            if (this.valptr < 0)
            {
                return -1;
            }
            return this.valstk[this.valptr--];
        }

        private void val_push(object val)
        {
            if (this.valptr < YYSTACKSIZE)
            {
                this.valstk[++this.valptr] = val;
            }
        }

        private void yyerror(string s)
        {
            throw new ADFilterParsingException(this._query, s, this.pos + 1);
        }

        private short yylex()
        {
            object obj2 = null;
            try
            {
                obj2 = this.lexer.yylex();
            }
            catch (ApplicationException exception)
            {
                this.pos = this.lexer.yy_char();
                this.pos++;
                throw new ADFilterParsingException(this._query, exception.Message, this.pos + 1);
            }
            if (obj2 == null)
            {
                return 0;
            }
            this.pos = this.lexer.yy_char();
            return (short) obj2;
        }

        private void yylexdebug(int state, int ch)
        {
            string str = null;
            if (ch < 0)
            {
                ch = 0;
            }
            if (ch <= YYMAXTOKEN)
            {
                str = yyname[ch];
            }
            if (str == null)
            {
                str = "illegal-symbol";
            }
            this.debug(string.Concat(new object[] { "state ", state, ", reading ", ch, " (", str, ")" }));
        }

        private int yyparse()
        {
            int num;
            int num2;
            this.init_stacks();
            this.yynerrs = 0;
            this.yyerrflag = 0;
            this.yychar = -1;
            int state = 0;
            this.state_push(state);
        Label_0025:
            num = yydefred[state];
            if (num == 0)
            {
                if (this.yychar < 0)
                {
                    this.yychar = this.yylex();
                    if (this.yychar < 0)
                    {
                        this.yychar = 0;
                    }
                }
                num = yysindex[state];
                if (((num != 0) && ((num += this.yychar) >= 0)) && ((num <= YYTABLESIZE) && (yycheck[num] == this.yychar)))
                {
                    state = yytable[num];
                    this.state_push(state);
                    this.val_push(this.yylval);
                    this.yychar = -1;
                    if (this.yyerrflag > 0)
                    {
                        this.yyerrflag--;
                    }
                }
                else
                {
                    num = yyrindex[state];
                    if (((num != 0) && ((num += this.yychar) >= 0)) && ((num <= YYTABLESIZE) && (yycheck[num] == this.yychar)))
                    {
                        num = yytable[num];
                        goto Label_01D9;
                    }
                    if (this.yyerrflag == 0)
                    {
                        this.yyerror("syntax error");
                        this.yynerrs++;
                    }
                    if (this.yyerrflag < 3)
                    {
                        this.yyerrflag = 3;
                        while (true)
                        {
                            if (this.stateptr < 0)
                            {
                                goto Label_0849;
                            }
                            num = yysindex[this.state_peek(0)];
                            if (((num != 0) && ((num += YYERRCODE) >= 0)) && ((num <= YYTABLESIZE) && (yycheck[num] == YYERRCODE)))
                            {
                                if (this.stateptr < 0)
                                {
                                    goto Label_0849;
                                }
                                state = yytable[num];
                                this.state_push(state);
                                this.val_push(this.yylval);
                                goto Label_0025;
                            }
                            if (this.stateptr < 0)
                            {
                                goto Label_0849;
                            }
                            this.state_pop();
                            this.val_pop();
                        }
                    }
                    if (this.yychar == 0)
                    {
                        goto Label_0854;
                    }
                    this.yychar = -1;
                }
                goto Label_0025;
            }
        Label_01D9:
            num2 = yylen[num];
            this.yyval = this.val_peek(num2 - 1);
            switch (num)
            {
                case 1:
                    this._filterExprTree = null;
                    break;

                case 2:
                    this._filterExprTree = (IADOPathNode) this.val_peek(1);
                    break;

                case 3:
                    this.yyval = this.val_peek(0);
                    break;

                case 4:
                    this.yyval = new CompositeADOPathNode(ADOperator.And, new IADOPathNode[] { (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0) });
                    break;

                case 5:
                    this.yyval = new CompositeADOPathNode(ADOperator.Or, new IADOPathNode[] { (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0) });
                    break;

                case 6:
                    this.yyval = this.val_peek(1);
                    break;

                case 7:
                    this.yyval = new UnaryADOPathNode(ADOperator.Not, (IADOPathNode) this.val_peek(0));
                    break;

                case 8:
                    this.yyval = this.val_peek(1);
                    break;

                case 9:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Eq, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 10:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Ne, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 11:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Gt, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 12:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Lt, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 13:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Ge, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 14:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Le, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 15:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Like, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 0x10:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.NotLike, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 0x11:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Approx, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 0x12:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Band, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 0x13:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.Bor, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 20:
                    this.yyval = ADOPathUtil.CreateRelationalExpressionNode(ADOperator.RecursiveMatch, (IADOPathNode) this.val_peek(2), (IADOPathNode) this.val_peek(0), this._searchFilterConverter);
                    break;

                case 0x15:
                    this.yyval = this.val_peek(1);
                    break;

                case 0x16:
                    if (this.val_peek(0) != null)
                    {
                        throw new ADFilterParsingException(this._query, "Operator Not supported: " + this.lexer.yytext(), this.pos + 1);
                    }
                    break;

                case 0x17:
                    this.yyval = this.val_peek(0);
                    break;

                case 0x18:
                {
                    string data = this.lexer.yytext().Substring(1, this.lexer.yytext().Length - 2).Replace("''", "'");
                    this.yyval = new TextDataADOPathNode(data);
                    break;
                }
                case 0x19:
                {
                    string str2 = this.lexer.yytext().Substring(1, this.lexer.yytext().Length - 2).Replace("`\"", "\"");
                    this.yyval = new TextDataADOPathNode(str2);
                    break;
                }
                case 0x1a:
                    this.yyval = new NumberADOPathNode(this.lexer.yytext());
                    break;

                case 0x1b:
                    this.yyval = new NumberADOPathNode(this.lexer.yytext());
                    break;

                case 0x1c:
                    this.yyval = this.val_peek(0);
                    break;

                case 0x1d:
                    this.yyval = new PropertyADOPathNode(this.lexer.yytext());
                    break;

                case 30:
                {
                    string varNameStr = this.lexer.yytext().Substring(1);
                    this.yyval = new VariableADOPathNode(varNameStr, new EvaluateVariableDelegate(this._varExpressionConverter.GetVariableExpressionValue));
                    break;
                }
                case 0x1f:
                {
                    string str4 = this.lexer.yytext();
                    str4 = str4.Substring(2, str4.Length - 2);
                    int index = str4.IndexOf("}");
                    str4 = str4.Remove(index, 1);
                    this.yyval = new VariableADOPathNode(str4, new EvaluateVariableDelegate(this._varExpressionConverter.GetVariableExpressionValue));
                    break;
                }
            }
            this.state_drop(num2);
            state = this.state_peek(0);
            this.val_drop(num2);
            num2 = yylhs[num];
            if ((state == 0) && (num2 == 0))
            {
                state = YYFINAL;
                this.state_push(YYFINAL);
                this.val_push(this.yyval);
                if (this.yychar < 0)
                {
                    this.yychar = this.yylex();
                    if (this.yychar < 0)
                    {
                        this.yychar = 0;
                    }
                }
                if (this.yychar == 0)
                {
                    return 0;
                }
                goto Label_0025;
            }
            num = yygindex[num2];
            if (((num != 0) && ((num += state) >= 0)) && ((num <= YYTABLESIZE) && (yycheck[num] == state)))
            {
                state = yytable[num];
            }
            else
            {
                state = yydgoto[num2];
            }
            if (this.stateptr >= 0)
            {
                this.state_push(state);
                this.val_push(this.yyval);
                goto Label_0025;
            }
        Label_0849:
            this.yyerror("yacc stack overflow");
        Label_0854:
            return 1;
        }

        internal IADOPathNode FilterExpressionTree
        {
            get
            {
                return this._filterExprTree;
            }
        }
    }
}

