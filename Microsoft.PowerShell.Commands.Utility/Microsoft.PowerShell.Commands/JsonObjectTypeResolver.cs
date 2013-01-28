namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;

    internal class JsonObjectTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id)
        {
            return typeof(Dictionary<string, object>);
        }

        public override string ResolveTypeId(Type type)
        {
            return string.Empty;
        }
    }
}

