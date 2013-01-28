namespace System.Data.Services.Common
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    internal sealed class DataServiceKeyAttribute : Attribute
    {
        private readonly ReadOnlyCollection<string> keyNames;

        public DataServiceKeyAttribute(string keyName)
        {
            Util.CheckArgumentNull<string>(keyName, "keyName");
            Util.CheckArgumentNullAndEmpty(keyName, "KeyName");
            this.keyNames = new ReadOnlyCollection<string>(new string[] { keyName });
        }

        public DataServiceKeyAttribute(params string[] keyNames)
        {
            Util.CheckArgumentNull<string[]>(keyNames, "keyNames");
            if ((keyNames.Length == 0) || keyNames.Any<string>(delegate (string f) {
                if (f != null)
                {
                    return (f.Length == 0);
                }
                return true;
            }))
            {
                throw System.Data.Services.Client.Error.Argument(System.Data.Services.Client.Strings.DSKAttribute_MustSpecifyAtleastOnePropertyName, "keyNames");
            }
            this.keyNames = new ReadOnlyCollection<string>(keyNames);
        }

        public ReadOnlyCollection<string> KeyNames
        {
            get
            {
                return this.keyNames;
            }
        }
    }
}

