using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Mono.Data.PowerShell.Provider
{
    public static class Extensions
    {
        public static PSObject ToPSObject( this object t )
        {
            return PSObject.AsPSObject(t);
        }

        public static void SafeAdd( this PSMemberInfoCollection<PSPropertyInfo> t, PSPropertyInfo item ) 
        {
            if( null == t[item.Name])
            {
                t.Add( item );
            }
        }
    }
}
