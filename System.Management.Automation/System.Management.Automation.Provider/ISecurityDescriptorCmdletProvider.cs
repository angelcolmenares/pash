namespace System.Management.Automation.Provider
{
    using System;
    using System.Security.AccessControl;

    public interface ISecurityDescriptorCmdletProvider
    {
        void GetSecurityDescriptor(string path, AccessControlSections includeSections);
        ObjectSecurity NewSecurityDescriptorFromPath(string path, AccessControlSections includeSections);
        ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections includeSections);
        void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor);
    }
}

