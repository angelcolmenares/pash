namespace System.DirectoryServices.ActiveDirectory
{
	internal enum DSROLE_MACHINE_ROLE
	{
		DsRole_RoleStandaloneWorkstation,
		DsRole_RoleMemberWorkstation,
		DsRole_RoleStandaloneServer,
		DsRole_RoleMemberServer,
		DsRole_RoleBackupDomainController,
		DsRole_RolePrimaryDomainController,
		DsRole_WorkstationWithSharedAccountDomain,
		DsRole_ServerWithSharedAccountDomain,
		DsRole_MemberWorkstationWithSharedAccountDomain,
		DsRole_MemberServerWithSharedAccountDomain
	}
}