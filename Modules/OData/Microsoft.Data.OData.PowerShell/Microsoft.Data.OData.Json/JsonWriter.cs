namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class JsonWriter
    {
        private readonly Stack<Scope> scopes;
        private readonly IndentedTextWriter writer;

        internal JsonWriter(TextWriter writer, bool indent)
        {
            this.writer = new IndentedTextWriter(writer, indent);
            this.scopes = new Stack<Scope>();
        }

        internal void EndArrayScope()
        {
            this.writer.WriteLine();
            this.writer.DecreaseIndentation();
            this.scopes.Pop();
            this.writer.Write("]");
        }

        internal void EndObjectScope()
        {
            this.writer.WriteLine();
            this.writer.DecreaseIndentation();
            this.scopes.Pop();
            this.writer.Write("}");
        }

        internal void Flush()
        {
            this.writer.Flush();
        }

        internal void StartArrayScope()
        {
            this.StartScope(ScopeType.Array);
        }

        internal void StartObjectScope()
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
            this.writer.IncreaseIndentation();
            this.writer.WriteLine();
        }

        internal void WriteDataArrayName()
        {
            this.WriteName("results");
        }

        internal void WriteDataWrapper()
        {
            this.writer.Write("\"d\":");
        }

        internal void WriteName(string name)
        {
            Scope scope = this.scopes.Peek();
            if (scope.ObjectCount != 0)
            {
                this.writer.Write(",");
            }
            scope.ObjectCount++;
            JsonValueUtils.WriteEscapedJsonString(this.writer, name);
            this.writer.Write(":");
        }

        internal void WriteValue(bool value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(byte value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(decimal value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(double value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(Guid value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(short value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(int value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(long value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(sbyte value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(float value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(string value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(TimeSpan value)
        {
            this.WriteValueSeparator();
            JsonValueUtils.WriteValue(this.writer, value);
        }

        internal void WriteValue(DateTime value, ODataVersion odataVersion)
        {
            this.WriteValueSeparator();
            if (odataVersion < ODataVersion.V3)
            {
                JsonValueUtils.WriteValue(this.writer, value, ODataJsonDateTimeFormat.ODataDateTime);
            }
            else
            {
                JsonValueUtils.WriteValue(this.writer, value, ODataJsonDateTimeFormat.ISO8601DateTime);
            }
        }

        internal void WriteValue(DateTimeOffset value, ODataVersion odataVersion)
        {
            this.WriteValueSeparator();
            if (odataVersion < ODataVersion.V3)
            {
                JsonValueUtils.WriteValue(this.writer, value, ODataJsonDateTimeFormat.ODataDateTime);
            }
            else
            {
                JsonValueUtils.WriteValue(this.writer, value, ODataJsonDateTimeFormat.ISO8601DateTime);
            }
        }

        private void WriteValueSeparator()
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

