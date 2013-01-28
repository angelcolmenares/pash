namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class PSMemberSetAdapter : MemberRedirectionAdapter
    {
        protected override T GetMember<T>(object obj, string memberName)
        {
            return (((PSMemberSet) obj).Members[memberName] as T);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            PSMemberInfoInternalCollection<T> internals = new PSMemberInfoInternalCollection<T>();
            foreach (PSMemberInfo info in ((PSMemberSet) obj).Members)
            {
                T member = info as T;
                if (member != null)
                {
                    internals.Add(member);
                }
            }
            return internals;
        }

        protected override IEnumerable<string> GetTypeNameHierarchy(object obj)
        {
            yield return typeof(PSMemberSet).FullName;
        }

        
    }
}

