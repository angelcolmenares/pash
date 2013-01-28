namespace System.Data.Services.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml;

    internal sealed class JsonWriter
    {
        internal static readonly long DatetimeMinTimeTicks;
        private const int EscapedCharLength = 6;
        private const string JsonDateTimeFormat = @"\/Date({0})\/";
        private readonly Stack<Scope> scopes;
        private readonly TextWriter writer;

        static JsonWriter()
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DatetimeMinTimeTicks = time.Ticks;
        }

        public JsonWriter(TextWriter writer)
        {
            this.writer = writer;
            this.scopes = new Stack<Scope>();
        }

        public void EndScope()
        {
            Scope scope = this.scopes.Pop();
            this.writer.Write((scope.Type == ScopeType.Array) ? "]" : "}");
        }

        public void Flush()
        {
            this.writer.Flush();
        }

        private static string QuoteJScriptString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            StringBuilder builder = null;
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if ((((ch == '\r') || (ch == '\t')) || ((ch == '"') || (ch == '\\'))) || (((ch == '\n') || (ch < ' ')) || (((ch > '\x007f') || (ch == '\b')) || (ch == '\f'))))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(s.Length + 6);
                    }
                    if (count > 0)
                    {
                        builder.Append(s, startIndex, count);
                    }
                    startIndex = i + 1;
                    count = 0;
                }
                switch (ch)
                {
                    case '\b':
                    {
                        builder.Append(@"\b");
                        continue;
                    }
                    case '\t':
                    {
                        builder.Append(@"\t");
                        continue;
                    }
                    case '\n':
                    {
                        builder.Append(@"\n");
                        continue;
                    }
                    case '\f':
                    {
                        builder.Append(@"\f");
                        continue;
                    }
                    case '\r':
                    {
                        builder.Append(@"\r");
                        continue;
                    }
                    case '"':
                    {
                        builder.Append("\\\"");
                        continue;
                    }
                    case '\\':
                    {
                        builder.Append(@"\\");
                        continue;
                    }
                }
                if ((ch < ' ') || (ch > '\x007f'))
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, @"\u{0:x4}", new object[] { (int) ch });
                }
                else
                {
                    count++;
                }
            }
            string str = s;
            if (builder == null)
            {
                return str;
            }
            if (count > 0)
            {
                builder.Append(s, startIndex, count);
            }
            return builder.ToString();
        }

        public void StartArrayScope()
        {
            this.StartScope(ScopeType.Array);
        }

        public void StartObjectScope()
        {
            this.StartScope(ScopeType.Object);
        }

        private void StartScope(ScopeType type)
        {
            if (this.scopes.Count != 0)
            {
                Scope scope = this.scopes.Peek();
                if ((scope.Type == ScopeType.Array) && (scope.ObjectCount != 0))
                {
                    this.writer.Write(",");
                }
                scope.ObjectCount++;
            }
            Scope item = new Scope(type);
            this.scopes.Push(item);
            this.writer.Write((type == ScopeType.Array) ? "[" : "{");
        }

        private void WriteCore(string text, bool quotes)
        {
            if (this.scopes.Count != 0)
            {
                Scope scope = this.scopes.Peek();
                if (scope.Type == ScopeType.Array)
                {
                    if (scope.ObjectCount != 0)
                    {
                        this.writer.Write(",");
                    }
                    scope.ObjectCount++;
                }
            }
            if (quotes)
            {
                this.writer.Write('"');
            }
            this.writer.Write(text);
            if (quotes)
            {
                this.writer.Write('"');
            }
        }

        public void WriteName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Scope scope = this.scopes.Peek();
            if (scope.Type == ScopeType.Object)
            {
                if (scope.ObjectCount != 0)
                {
                    this.writer.Write(",");
                }
                scope.ObjectCount++;
            }
            this.WriteCore(QuoteJScriptString(name), true);
            this.writer.Write(":");
        }

        public void WriteValue(bool value)
        {
            this.WriteCore(value ? "true" : "false", false);
        }

        public void WriteValue(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Unspecified:
                    dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
                    break;

                case DateTimeKind.Local:
                    dateTime = dateTime.ToUniversalTime();
                    break;
            }
            this.WriteCore(string.Format(CultureInfo.InvariantCulture, @"\/Date({0})\/", new object[] { (dateTime.Ticks - DatetimeMinTimeTicks) / 0x2710L }), true);
        }

        public void WriteValue(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                this.WriteCore(value.ToString(null, CultureInfo.InvariantCulture), true);
            }
            else
            {
                this.WriteCore(XmlConvert.ToString(value), false);
            }
        }

        public void WriteValue(int value)
        {
            this.WriteCore(value.ToString(CultureInfo.InvariantCulture), false);
        }

        public void WriteValue(string s)
        {
            if (s == null)
            {
                this.WriteCore("null", false);
            }
            else
            {
                this.WriteCore(QuoteJScriptString(s), true);
            }
        }

        private sealed class Scope
        {
            private readonly JsonWriter.ScopeType type;

            public Scope(JsonWriter.ScopeType type)
            {
                this.type = type;
            }

            public int ObjectCount { get; set; }

            public JsonWriter.ScopeType Type
            {
                get
                {
                    return this.type;
                }
            }
        }

        private enum ScopeType
        {
            Array,
            Object
        }
    }
}

