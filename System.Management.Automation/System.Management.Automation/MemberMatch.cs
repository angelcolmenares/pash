namespace System.Management.Automation
{
    using System;

    internal class MemberMatch
    {
        internal static WildcardPattern GetNamePattern(string name)
        {
            if ((name != null) && WildcardPattern.ContainsWildcardCharacters(name))
            {
                return new WildcardPattern(name, WildcardOptions.IgnoreCase);
            }
            return null;
        }

        internal static PSMemberInfoInternalCollection<T> Match<T>(PSMemberInfoInternalCollection<T> memberList, string name, WildcardPattern nameMatch, PSMemberTypes memberTypes) where T: PSMemberInfo
        {
            PSMemberInfoInternalCollection<T> internals = new PSMemberInfoInternalCollection<T>();
            if (memberList == null)
            {
                throw PSTraceSource.NewArgumentNullException("memberList");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (nameMatch == null)
            {
                T member = memberList[name];
                if ((member != null) && ((member.MemberType & memberTypes) != 0))
                {
                    internals.Add(member);
                }
                return internals;
            }
            foreach (T local2 in memberList)
            {
                if (nameMatch.IsMatch(local2.Name) && ((local2.MemberType & memberTypes) != 0))
                {
                    internals.Add(local2);
                }
            }
            return internals;
        }
    }
}

