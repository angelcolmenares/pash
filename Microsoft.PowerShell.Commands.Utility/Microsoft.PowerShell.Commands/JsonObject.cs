namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Web.Script.Serialization;

    public static class JsonObject
    {
        public static object ConvertFromJson(string input, out ErrorRecord error)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            error = null;
            JsonObjectTypeResolver resolver = new JsonObjectTypeResolver();
            object obj2 = new JavaScriptSerializer(resolver).DeserializeObject(input);
            if (obj2 is IDictionary<string, object>)
            {
                IDictionary<string, object> entries = obj2 as IDictionary<string, object>;
                return PopulateFromDictionary(entries, out error);
            }
            if (obj2 is ICollection<object>)
            {
                ICollection<object> list = obj2 as ICollection<object>;
                obj2 = PopulateFromList(list, out error);
            }
            return obj2;
        }

        private static PSObject PopulateFromDictionary(IDictionary<string, object> entries, out ErrorRecord error)
        {
            error = null;
            PSObject obj2 = new PSObject();
            foreach (KeyValuePair<string, object> pair in entries)
            {
                PSPropertyInfo info = obj2.Properties[pair.Key];
                if (info != null)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, WebCmdletStrings.DuplicateKeysInJsonString, new object[] { info.Name, pair.Key });
                    error = new ErrorRecord(new InvalidOperationException(message), "DuplicateKeysInJsonString", ErrorCategory.InvalidOperation, null);
                    return null;
                }
                if (pair.Value is IDictionary<string, object>)
                {
                    IDictionary<string, object> dictionary = pair.Value as IDictionary<string, object>;
                    PSObject obj3 = PopulateFromDictionary(dictionary, out error);
                    if (error != null)
                    {
                        return null;
                    }
                    obj2.Properties.Add(new PSNoteProperty(pair.Key, obj3));
                }
                else if (pair.Value is ICollection<object>)
                {
                    ICollection<object> list = pair.Value as ICollection<object>;
                    ICollection<object> is3 = PopulateFromList(list, out error);
                    if (error != null)
                    {
                        return null;
                    }
                    obj2.Properties.Add(new PSNoteProperty(pair.Key, is3));
                }
                else
                {
                    obj2.Properties.Add(new PSNoteProperty(pair.Key, pair.Value));
                }
            }
            return obj2;
        }

        private static ICollection<object> PopulateFromList(ICollection<object> list, out ErrorRecord error)
        {
            error = null;
            List<object> list2 = new List<object>();
            foreach (object obj2 in list)
            {
                if (obj2 is IDictionary<string, object>)
                {
                    IDictionary<string, object> entries = obj2 as IDictionary<string, object>;
                    PSObject item = PopulateFromDictionary(entries, out error);
                    if (error != null)
                    {
                        return null;
                    }
                    list2.Add(item);
                }
                else if (obj2 is ICollection<object>)
                {
                    ICollection<object> is2 = obj2 as ICollection<object>;
                    ICollection<object> is3 = PopulateFromList(is2, out error);
                    if (error != null)
                    {
                        return null;
                    }
                    list2.Add(is3);
                }
                else
                {
                    list2.Add(obj2);
                }
            }
            return list2.ToArray();
        }
    }
}

