namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Language;

    internal static class ReservedNameMembers
    {
        private static object GenerateMemberSet(string name, object obj)
        {
            PSObject psObject = PSObject.AsPSObject(obj);
            PSMemberInfo member = psObject.InstanceMembers[name];
            if (member == null)
            {
                PSInternalMemberSet set = new PSInternalMemberSet(name, psObject) {
                    ShouldSerialize = false,
                    isHidden = true,
                    isReservedMember = true
                };
                member = set;
                psObject.InstanceMembers.Add(member);
                member.instance = psObject;
            }
            return member;
        }

        internal static object GeneratePSAdaptedMemberSet(object obj)
        {
            return GenerateMemberSet("psadapted", obj);
        }

        internal static object GeneratePSBaseMemberSet(object obj)
        {
            return GenerateMemberSet("psbase", obj);
        }

        internal static object GeneratePSExtendedMemberSet(object obj)
        {
            PSObject mshObject = PSObject.AsPSObject(obj);
            PSMemberInfo member = mshObject.InstanceMembers["psextended"];
            if (member == null)
            {
                PSMemberSet set = new PSMemberSet("psextended", mshObject) {
                    ShouldSerialize = false,
                    isHidden = true,
                    isReservedMember = true
                };
                member = set;
                member.ReplicateInstance(mshObject);
                member.instance = mshObject;
                mshObject.InstanceMembers.Add(member);
            }
            return member;
        }

        internal static object GeneratePSObjectMemberSet(object obj)
        {
            return GenerateMemberSet("psobject", obj);
        }

        internal static void GeneratePSTypeNames(object obj)
        {
            PSObject obj2 = PSObject.AsPSObject(obj);
            if (obj2.InstanceMembers["pstypenames"] == null)
            {
                PSCodeProperty member = new PSCodeProperty("pstypenames", CachedReflectionInfo.ReservedNameMembers_PSTypeNames) {
                    shouldSerialize = false,
                    instance = obj2,
                    isHidden = true,
                    isReservedMember = true
                };
                obj2.InstanceMembers.Add(member);
            }
        }

        public static Collection<string> PSTypeNames(PSObject o)
        {
            return o.TypeNames;
        }
    }
}

