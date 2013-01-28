namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;
    using System.Text;

    internal class NativeCommandParameterBinder : ParameterBinderBase
    {
        private StringBuilder arguments;
        private NativeCommand nativeCommand;
        [TraceSource("NativeCommandParameterBinder", "The parameter binder for native commands")]
        private static PSTraceSource tracer = PSTraceSource.GetTracer("NativeCommandParameterBinder", "The parameter binder for native commands");
        private static readonly char[] whiteSpace = new char[] { ' ', '\t' };

        internal NativeCommandParameterBinder(NativeCommand command) : base(command.MyInvocation, command.Context, command)
        {
            this.arguments = new StringBuilder();
            this.nativeCommand = command;
        }

        private static void appendNativeArguments(ExecutionContext context, StringBuilder argumentBuilder, object arg)
        {
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(arg);
            bool flag = false;
            if (enumerator == null)
            {
                appendOneNativeArgument(context, argumentBuilder, true, arg);
            }
            else
            {
                while (enumerator.MoveNext())
                {
                    object current = enumerator.Current;
                    string name = string.Empty;
                    if (current != null)
                    {
                        name = current.ToString();
                    }
                    if (flag)
                    {
                        argumentBuilder.Append(" ");
                        name = Environment.ExpandEnvironmentVariables(name);
                        argumentBuilder.Append(name);
                    }
                    else if (string.Equals("--%", name, StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                    }
                    else
                    {
                        appendOneNativeArgument(context, argumentBuilder, true, current);
                    }
                }
            }
        }

        internal static void appendOneNativeArgument(ExecutionContext context, StringBuilder argumentBuilder, bool needInitialSpace, object obj)
        {
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(obj);
            bool flag = needInitialSpace;
            do
            {
                string str;
                if (enumerator == null)
                {
                    str = PSObject.ToStringParser(context, obj);
                }
                else
                {
                    if (!ParserOps.MoveNext(context, null, enumerator))
                    {
                        break;
                    }
                    str = PSObject.ToStringParser(context, ParserOps.Current(null, enumerator));
                }
                if (!string.IsNullOrEmpty(str))
                {
                    if (flag)
                    {
                        argumentBuilder.Append(' ');
                    }
                    else
                    {
                        flag = true;
                    }
                    if (((str.IndexOfAny(whiteSpace) >= 0) && (str.Length > 1)) && (str[0] != '"'))
                    {
                        argumentBuilder.Append('"');
                        argumentBuilder.Append(str);
                        argumentBuilder.Append('"');
                    }
                    else
                    {
                        argumentBuilder.Append(str);
                    }
                }
            }
            while (enumerator != null);
        }

        internal override void BindParameter(string name, object value)
        {
            if (name != null)
            {
                appendNativeArguments(base.Context, this.arguments, name);
            }
            if ((value != AutomationNull.Value) && (value != UnboundParameter.Value))
            {
                appendNativeArguments(base.Context, this.arguments, value);
            }
        }

        internal override object GetDefaultParameterValue(string name)
        {
            return null;
        }

        internal string Arguments
        {
            get
            {
                tracer.WriteLine("Raw argument string: " + this.arguments.ToString(), new object[0]);
                string[] strArray = CommandLineParameterBinderNativeMethods.PreParseCommandLine(this.arguments.ToString());
                for (int i = 0; i < strArray.Length; i++)
                {
                    tracer.WriteLine("Argument {0}: {1}", new object[] { i, strArray[i] });
                }
                return this.arguments.ToString();
            }
        }
    }
}

