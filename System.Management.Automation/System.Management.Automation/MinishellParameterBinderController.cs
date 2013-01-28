namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Xml;

    internal class MinishellParameterBinderController : NativeCommandParameterBinderController
    {
        internal const string ArgsParameter = "-args";
        internal const string CommandParameter = "-command";
        internal const string EncodedArgsParameter = "-encodedarguments";
        internal const string EncodedCommandParameter = "-encodedCommand";
        internal const string InputFormatParameter = "-inputFormat";
        private NativeCommandIOFormat inputFormatValue;
        private bool nonInteractive;
        internal const string NonInteractiveParameter = "-noninteractive";
        internal const string OutputFormatParameter = "-outputFormat";
        private NativeCommandIOFormat outputFormatValue;
        internal const string TextFormatValue = "text";
        internal const string XmlFormatValue = "xml";

        internal MinishellParameterBinderController(NativeCommand command) : base(command)
        {
            this.inputFormatValue = NativeCommandIOFormat.Xml;
        }

        internal override Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> parameters)
        {
            return null;
        }

        internal Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> parameters, bool outputRedirected, string hostName)
        {
            ArrayList args = new ArrayList();
            foreach (CommandParameterInternal internal2 in parameters)
            {
                object argumentValue = internal2.ArgumentValue;
                if (internal2.ParameterNameSpecified)
                {
                    args.Add(internal2.ParameterText);
                    if ((argumentValue != AutomationNull.Value) && (argumentValue != UnboundParameter.Value))
                    {
                        args.Add(argumentValue);
                    }
                }
                else
                {
                    args.Add(argumentValue);
                }
            }
            ArrayList list2 = this.ProcessMinishellParameters(args, outputRedirected, hostName);
            base.DefaultParameterBinder.BindParameter(null, list2.ToArray());
            return new Collection<CommandParameterInternal>();
        }

        private static ArrayList ConvertArgsValueToArrayList(object value)
        {
            ArrayList list = new ArrayList();
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(value);
            if (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
                return list;
            }
            list.Add(value);
            return list;
        }

        private static string ConvertArgsValueToEncodedString(object value)
        {
            ArrayList source = ConvertArgsValueToArrayList(value);
            StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter writer = new XmlTextWriter(w);
            Serializer serializer = new Serializer(writer);
            serializer.Serialize(source);
            serializer.Done();
            writer.Flush();
            return StringToBase64Converter.StringToBase64String(w.ToString());
        }

        private void HandleSeenParameter(ref MinishellParameters seen, MinishellParameters parameter, string parameterName)
        {
            if ((seen & parameter) == parameter)
            {
                throw this.NewParameterBindingException(null, ErrorCategory.InvalidArgument, parameterName, null, null, "ParameterSpecifiedAlready", new object[] { parameterName });
            }
            seen |= parameter;
        }

        private ParameterBindingException NewParameterBindingException(Exception innerException, ErrorCategory errorCategory, string parameterName, Type parameterType, Type typeSpecified, string errorIdAndResourceId, params object[] args)
        {
            return new ParameterBindingException(innerException, errorCategory, base.InvocationInfo, null, parameterName, parameterType, typeSpecified, "NativeCP", errorIdAndResourceId, args);
        }

        private string ProcessFormatParameterValue(string parameterName, object value)
        {
            string str;
            try
            {
                str = (string) LanguagePrimitives.ConvertTo(value, typeof(string), CultureInfo.InvariantCulture);
            }
            catch (PSInvalidCastException exception)
            {
                throw this.NewParameterBindingException(exception, ErrorCategory.InvalidArgument, parameterName, typeof(string), value.GetType(), "StringValueExpectedForFormatParameter", new object[] { parameterName });
            }
            if (StartsWith("xml", str))
            {
                return "xml";
            }
            if (!StartsWith("text", str))
            {
                throw this.NewParameterBindingException(null, ErrorCategory.InvalidArgument, parameterName, typeof(string), value.GetType(), "IncorrectValueForFormatParameter", new object[] { str, parameterName });
            }
            return "text";
        }

        private ArrayList ProcessMinishellParameters(ArrayList args, bool outputRedirected, string hostName)
        {
            ArrayList list = new ArrayList();
            string str = null;
            string str2 = null;
            MinishellParameters seen = 0;
            for (int i = 0; i < args.Count; i++)
            {
                object obj2 = args[i];
                if (StartsWith("-command", obj2))
                {
                    this.HandleSeenParameter(ref seen, MinishellParameters.Command, "-command");
                    list.Add("-encodedCommand");
                    if ((i + 1) >= args.Count)
                    {
                        throw this.NewParameterBindingException(null, ErrorCategory.InvalidArgument, "-command", typeof(ScriptBlock), null, "NoValueForCommandParameter", new object[0]);
                    }
                    ScriptBlock block = args[i + 1] as ScriptBlock;
                    if (block == null)
                    {
                        throw this.NewParameterBindingException(null, ErrorCategory.InvalidArgument, "-command", typeof(ScriptBlock), args[i + 1].GetType(), "IncorrectValueForCommandParameter", new object[0]);
                    }
                    string str3 = StringToBase64Converter.StringToBase64String(block.ToString());
                    list.Add(str3);
                    i++;
                }
                else if (obj2 is ScriptBlock)
                {
                    this.HandleSeenParameter(ref seen, MinishellParameters.Command, "-command");
                    list.Add("-encodedCommand");
                    string str4 = StringToBase64Converter.StringToBase64String(obj2.ToString());
                    list.Add(str4);
                }
                else if (StartsWith("-inputFormat", obj2))
                {
                    this.HandleSeenParameter(ref seen, MinishellParameters.InputFormat, "-inputFormat");
                    list.Add("-inputFormat");
                    if ((i + 1) >= args.Count)
                    {
                        throw this.NewParameterBindingException(null, ErrorCategory.InvalidArgument, "-inputFormat", typeof(string), null, "NoValueForInputFormatParameter", new object[0]);
                    }
                    str = this.ProcessFormatParameterValue("-inputFormat", args[i + 1]);
                    i++;
                    list.Add(str);
                }
                else if (StartsWith("-outputFormat", obj2))
                {
                    this.HandleSeenParameter(ref seen, MinishellParameters.OutputFormat, "-outputFormat");
                    list.Add("-outputFormat");
                    if ((i + 1) >= args.Count)
                    {
                        throw this.NewParameterBindingException(null, ErrorCategory.InvalidArgument, "-outputFormat", typeof(string), null, "NoValueForOutputFormatParameter", new object[0]);
                    }
                    str2 = this.ProcessFormatParameterValue("-outputFormat", args[i + 1]);
                    i++;
                    list.Add(str2);
                }
                else if (StartsWith("-args", obj2))
                {
                    this.HandleSeenParameter(ref seen, MinishellParameters.Arguments, "-args");
                    list.Add("-encodedarguments");
                    if ((i + 1) >= args.Count)
                    {
                        throw this.NewParameterBindingException(null, ErrorCategory.InvalidArgument, "-args", typeof(string), null, "NoValuesSpecifiedForArgs", new object[0]);
                    }
                    string str5 = ConvertArgsValueToEncodedString(args[i + 1]);
                    i++;
                    list.Add(str5);
                }
                else
                {
                    list.Add(obj2);
                }
            }
            if (str == null)
            {
                list.Add("-inputFormat");
                list.Add("xml");
                str = "xml";
            }
            if (str2 == null)
            {
                list.Add("-outputFormat");
                if (outputRedirected)
                {
                    list.Add("xml");
                    str2 = "xml";
                }
                else
                {
                    list.Add("text");
                    str2 = "text";
                }
            }
            if (StartsWith(str, "xml"))
            {
                this.inputFormatValue = NativeCommandIOFormat.Xml;
            }
            else
            {
                this.inputFormatValue = NativeCommandIOFormat.Text;
            }
            if (StartsWith(str2, "xml"))
            {
                this.outputFormatValue = NativeCommandIOFormat.Xml;
            }
            else
            {
                this.outputFormatValue = NativeCommandIOFormat.Text;
            }
            if (string.IsNullOrEmpty(hostName) || !hostName.Equals("ConsoleHost", StringComparison.OrdinalIgnoreCase))
            {
                this.nonInteractive = true;
                list.Insert(0, "-noninteractive");
            }
            return list;
        }

        private static bool StartsWith(string lhs, object value)
        {
            string str = value as string;
            if (str == null)
            {
                return false;
            }
            return lhs.StartsWith(str, StringComparison.OrdinalIgnoreCase);
        }

        internal NativeCommandIOFormat InputFormat
        {
            get
            {
                return this.inputFormatValue;
            }
        }

        internal bool NonInteractive
        {
            get
            {
                return this.nonInteractive;
            }
        }

        internal NativeCommandIOFormat OutputFormat
        {
            get
            {
                return this.outputFormatValue;
            }
        }

        [Flags]
        private enum MinishellParameters
        {
            Arguments = 2,
            Command = 1,
            InputFormat = 4,
            OutputFormat = 8
        }
    }
}

