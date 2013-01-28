namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal class PSObjectAdapter : MemberRedirectionAdapter
    {
        protected override T GetMember<T>(object obj, string memberName)
        {
            return (((PSObject) obj).Members[memberName] as T);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            PSMemberInfoInternalCollection<T> internals = new PSMemberInfoInternalCollection<T>();
            PSObject obj2 = (PSObject) obj;
            foreach (PSMemberInfo info in obj2.Members)
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
            return ((PSObject) obj).InternalTypeNames;
        }
    }
}

