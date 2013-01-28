namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web.Script.Serialization;

    [Cmdlet("ConvertTo", "Json", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217032", RemotingCapability=RemotingCapability.None)]
    public class ConvertToJsonCommand : PSCmdlet
    {
        private int _depth = 2;
        private List<object> inputObjects = new List<object>();

        private void AddIndentations(int numberOfTabsToReturn, StringBuilder result)
        {
            int num = numberOfTabsToReturn * 4;
            for (int i = 0; i < num; i++)
            {
                result.Append(' ');
            }
        }

        private object AddPsProperties(object psobj, object obj, int depth, bool isPurePSObj, bool isCustomObj)
        {
            PSObject obj2 = psobj as PSObject;
            if (obj2 == null)
            {
                return obj;
            }
            if (isPurePSObj)
            {
                return obj;
            }
            bool flag = true;
            IDictionary receiver = obj as IDictionary;
            if (receiver == null)
            {
                flag = false;
                receiver = new Dictionary<string, object>();
                receiver.Add("value", obj);
            }
            this.AppendPsProperties(obj2, receiver, depth, isCustomObj);
            if (!flag && (receiver.Count == 1))
            {
                return obj;
            }
            return receiver;
        }

        private void AddSpaces(int numberOfSpacesToReturn, StringBuilder result)
        {
            for (int i = 0; i < numberOfSpacesToReturn; i++)
            {
                result.Append(' ');
            }
        }

        private void AppendPsProperties(PSObject psobj, IDictionary receiver, int depth, bool isCustomObject)
        {
            PSMemberInfoCollection<PSPropertyInfo> infos = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(psobj, isCustomObject ? PSObject.GetPropertyCollection(PSMemberViewTypes.Adapted | PSMemberViewTypes.Extended) : PSObject.GetPropertyCollection(PSMemberViewTypes.Extended));
            foreach (PSPropertyInfo info in infos)
            {
                object obj2 = null;
                try
                {
                    obj2 = info.Value;
                }
                catch (Exception exception)
                {
                    UtilityCommon.CheckForSevereException(this, exception);
                }
                if (!receiver.Contains(info.Name))
                {
                    receiver[info.Name] = this.ProcessValue(obj2, depth + 1);
                }
            }
        }

        protected override void BeginProcessing()
        {
            try
            {
                Assembly.Load("System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            }
            catch (FileNotFoundException)
            {
                base.ThrowTerminatingError(new ErrorRecord(new NotSupportedException(WebCmdletStrings.ExtendedProfileRequired), "ExtendedProfileRequired", ErrorCategory.NotInstalled, null));
            }
        }

        private int ConvertDictionary(string json, int index, StringBuilder result, string padString, int numberOfSpaces)
        {
            result.Append("\r\n");
            StringBuilder builder = new StringBuilder();
            builder.Append(padString);
            this.AddSpaces(numberOfSpaces, builder);
            this.AddIndentations(1, builder);
            bool flag = true;
            bool flag2 = true;
            int num = 0;
            for (int i = index; i < json.Length; i++)
            {
                switch (json[i])
                {
                    case '{':
                    {
                        result.Append(json[i]);
                        i = this.ConvertDictionary(json, i + 1, result, builder.ToString(), num);
                        flag = false;
                        continue;
                    }
                    case '}':
                        result.Append("\r\n");
                        result.Append(padString);
                        this.AddSpaces(numberOfSpaces, result);
                        result.Append(json[i]);
                        return i;

                    case '[':
                    {
                        result.Append(json[i]);
                        i = this.ConvertList(json, i + 1, result, builder.ToString(), num);
                        flag = false;
                        continue;
                    }
                    case ':':
                    {
                        result.Append(json[i]);
                        this.AddSpaces(2, result);
                        num += 3;
                        flag = false;
                        flag2 = false;
                        continue;
                    }
                    case '"':
                    {
                        if (flag)
                        {
                            result.Append(builder.ToString());
                        }
                        result.Append(json[i]);
                        int num3 = this.ConvertQuotedString(json, i + 1, result);
                        if (flag2)
                        {
                            num += (num3 - i) + 1;
                        }
                        i = num3;
                        flag = false;
                        continue;
                    }
                    case ',':
                    {
                        result.Append(json[i]);
                        result.Append("\r\n");
                        flag = true;
                        flag2 = true;
                        num = 0;
                        continue;
                    }
                }
                if (flag)
                {
                    result.Append(builder.ToString());
                }
                result.Append(json[i]);
                if (flag2)
                {
                    num++;
                }
                flag = false;
            }
            base.ThrowTerminatingError(this.NewError());
            return -1;
        }

        private int ConvertList(string json, int index, StringBuilder result, string padString, int numberOfSpaces)
        {
            result.Append("\r\n");
            StringBuilder builder = new StringBuilder();
            builder.Append(padString);
            this.AddSpaces(numberOfSpaces, builder);
            this.AddIndentations(1, builder);
            bool flag = true;
            for (int i = index; i < json.Length; i++)
            {
                char ch = json[i];
                if (ch <= ',')
                {
                    switch (ch)
                    {
                        case '"':
                            goto Label_010C;

                        case ',':
                            goto Label_013A;
                    }
                    goto Label_0158;
                }
                switch (ch)
                {
                    case '[':
                    {
                        result.Append(builder.ToString());
                        result.Append(json[i]);
                        i = this.ConvertList(json, i + 1, result, builder.ToString(), 0);
                        flag = false;
                        continue;
                    }
                    case '\\':
                        goto Label_0158;

                    case ']':
                        result.Append("\r\n");
                        result.Append(padString);
                        this.AddSpaces(numberOfSpaces, result);
                        result.Append(json[i]);
                        return i;

                    default:
                    {
                        if (ch != '{')
                        {
                            goto Label_0158;
                        }
                        result.Append(builder.ToString());
                        result.Append(json[i]);
                        i = this.ConvertDictionary(json, i + 1, result, builder.ToString(), 0);
                        flag = false;
                        continue;
                    }
                }
            Label_010C:
                if (flag)
                {
                    result.Append(builder.ToString());
                }
                result.Append(json[i]);
                i = this.ConvertQuotedString(json, i + 1, result);
                flag = false;
                continue;
            Label_013A:
                result.Append(json[i]);
                result.Append("\r\n");
                flag = true;
                continue;
            Label_0158:
                if (flag)
                {
                    result.Append(builder.ToString());
                }
                result.Append(json[i]);
                flag = false;
            }
            base.ThrowTerminatingError(this.NewError());
            return -1;
        }

        private int ConvertQuotedString(string json, int index, StringBuilder result)
        {
            for (int i = index; i < json.Length; i++)
            {
                result.Append(json[i]);
                if (json[i] == '"')
                {
                    return i;
                }
            }
            base.ThrowTerminatingError(this.NewError());
            return -1;
        }

        private string ConvertToPrettyJsonString(string json)
        {
            if (!json.StartsWith("{", StringComparison.OrdinalIgnoreCase) && !json.StartsWith("[", StringComparison.OrdinalIgnoreCase))
            {
                return json;
            }
            StringBuilder result = new StringBuilder();
            if (json.StartsWith("{", StringComparison.OrdinalIgnoreCase))
            {
                result.Append('{');
                this.ConvertDictionary(json, 1, result, "", 0);
            }
            else if (json.StartsWith("[", StringComparison.OrdinalIgnoreCase))
            {
                result.Append('[');
                this.ConvertList(json, 1, result, "", 0);
            }
            return result.ToString();
        }

        protected override void EndProcessing()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer {
                MaxJsonLength = 0x7fffffff
            };
            if (this.inputObjects.Count > 0)
            {
                object obj2 = (this.inputObjects.Count > 1) ? this.inputObjects.ToArray() : this.inputObjects[0];
                object obj3 = this.ProcessValue(obj2, 0);
                string json = serializer.Serialize(obj3);
                base.WriteObject((this.Compress != 0) ? json : this.ConvertToPrettyJsonString(json));
            }
        }

        private ErrorRecord NewError()
        {
            ErrorDetails details = new ErrorDetails(base.GetType().Assembly, "WebCmdletStrings", "JsonStringInBadFormat", new object[0]);
            return new ErrorRecord(new InvalidOperationException(details.Message), "JsonStringInBadFormat", ErrorCategory.InvalidOperation, this.InputObject);
        }

        private object ProcessCustomObject(object o, int depth)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            Type type = o.GetType();
            foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info.IsDefined(typeof(ScriptIgnoreAttribute), true))
                {
                    object obj2;
                    try
                    {
                        obj2 = info.GetValue(o);
                    }
                    catch (Exception)
                    {
                        obj2 = null;
                    }
                    dictionary.Add(info.Name, this.ProcessValue(obj2, depth + 1));
                }
            }
            foreach (PropertyInfo info2 in type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info2.IsDefined(typeof(ScriptIgnoreAttribute), true))
                {
                    MethodInfo getMethod = info2.GetGetMethod();
                    if ((getMethod != null) && (getMethod.GetParameters().Length <= 0))
                    {
                        object obj3;
                        try
                        {
                            obj3 = getMethod.Invoke(o, new object[0]);
                        }
                        catch (Exception)
                        {
                            obj3 = null;
                        }
                        dictionary.Add(info2.Name, this.ProcessValue(obj3, depth + 1));
                    }
                }
            }
            return dictionary;
        }

        private object ProcessDictionary(IDictionary dict, int depth)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(dict.Count);
            foreach (DictionaryEntry entry in dict)
            {
                string key = entry.Key as string;
                if (key == null)
                {
                    throw new InvalidOperationException("Embedded Dictionary contains non-string keys");
                }
                dictionary.Add(key, this.ProcessValue(entry.Value, depth + 1));
            }
            return dictionary;
        }

        private object ProcessEnumerable(IEnumerable enumerable, int depth)
        {
            List<object> list = new List<object>();
            foreach (object obj2 in enumerable)
            {
                list.Add(this.ProcessValue(obj2, depth + 1));
            }
            return list;
        }

        protected override void ProcessRecord()
        {
            if (this.InputObject != null)
            {
                this.inputObjects.Add(this.InputObject);
            }
        }

        private object ProcessValue(object obj, int depth)
        {
            PSObject valueToConvert = obj as PSObject;
            if (valueToConvert != null)
            {
                obj = valueToConvert.BaseObject;
            }
            object obj3 = obj;
            bool isPurePSObj = false;
            bool isCustomObj = false;
            if (((((obj == null) || DBNull.Value.Equals(obj)) || ((obj is string) || (obj is char))) || (((obj is bool) || (obj is DateTime)) || ((obj is DateTimeOffset) || (obj is Guid)))) || (((obj is Uri) || (obj is double)) || ((obj is float) || (obj is decimal))))
            {
                obj3 = obj;
            }
            else
            {
                Type type = obj.GetType();
                if (type.IsPrimitive)
                {
                    obj3 = obj;
                }
                else if (type.IsEnum)
                {
                    Type enumUnderlyingType = type.GetEnumUnderlyingType();
                    if (enumUnderlyingType.Equals(typeof(long)) || enumUnderlyingType.Equals(typeof(ulong)))
                    {
                        obj3 = obj.ToString();
                    }
                    else
                    {
                        obj3 = obj;
                    }
                }
                else if (depth > this.Depth)
                {
                    if ((valueToConvert != null) && valueToConvert.immediateBaseObjectIsEmpty)
                    {
                        obj3 = LanguagePrimitives.ConvertTo(valueToConvert, typeof(string), CultureInfo.InvariantCulture);
                        isPurePSObj = true;
                    }
                    else
                    {
                        obj3 = LanguagePrimitives.ConvertTo(obj, typeof(string), CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    IDictionary dict = obj as IDictionary;
                    if (dict != null)
                    {
                        obj3 = this.ProcessDictionary(dict, depth);
                    }
                    else
                    {
                        IEnumerable enumerable = obj as IEnumerable;
                        if (enumerable != null)
                        {
                            obj3 = this.ProcessEnumerable(enumerable, depth);
                        }
                        else
                        {
                            obj3 = this.ProcessCustomObject(obj, depth);
                            isCustomObj = true;
                        }
                    }
                }
            }
            return this.AddPsProperties(valueToConvert, obj3, depth, isPurePSObj, isCustomObj);
        }

        [Parameter]
        public SwitchParameter Compress { get; set; }

        [Parameter, ValidateRange(1, 0x7fffffff)]
        public int Depth
        {
            get
            {
                return this._depth;
            }
            set
            {
                this._depth = value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true), AllowNull]
        public object InputObject { get; set; }
    }
}

