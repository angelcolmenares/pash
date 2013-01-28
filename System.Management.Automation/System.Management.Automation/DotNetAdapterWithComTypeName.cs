namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class DotNetAdapterWithComTypeName : DotNetAdapter
    {
        private ComTypeInfo comTypeInfo;

        internal DotNetAdapterWithComTypeName(ComTypeInfo comTypeInfo)
        {
            this.comTypeInfo = comTypeInfo;
        }

        protected override ConsolidatedString GetInternedTypeNameHierarchy(object obj)
        {
            return new ConsolidatedString(this.GetTypeNameHierarchy(obj), true);
        }

        protected override IEnumerable<string> GetTypeNameHierarchy(object obj)
        {
            for (Type iteratorVariable0 = obj.GetType(); iteratorVariable0 != null; iteratorVariable0 = iteratorVariable0.BaseType)
            {
                if (iteratorVariable0.FullName.Equals("System.__ComObject"))
                {
                    yield return ComAdapter.GetComTypeName(this.comTypeInfo.Clsid);
                }
                yield return iteratorVariable0.FullName;
            }
        }

        
    }
}

