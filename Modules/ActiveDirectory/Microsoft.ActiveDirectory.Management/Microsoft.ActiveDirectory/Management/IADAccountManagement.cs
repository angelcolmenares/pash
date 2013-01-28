using Microsoft.ActiveDirectory.CustomActions;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IADAccountManagement
	{
		ChangePasswordResponse ChangePassword(ADSessionHandle handle, ChangePasswordRequest request);

		GetADGroupMemberResponse GetADGroupMember(ADSessionHandle handle, GetADGroupMemberRequest request);

		GetADPrincipalAuthorizationGroupResponse GetADPrincipalAuthorizationGroup(ADSessionHandle handle, GetADPrincipalAuthorizationGroupRequest request);

		GetADPrincipalGroupMembershipResponse GetADPrincipalGroupMembership(ADSessionHandle handle, GetADPrincipalGroupMembershipRequest request);

		SetPasswordResponse SetPassword(ADSessionHandle handle, SetPasswordRequest request);

		TranslateNameResponse TranslateName(ADSessionHandle handle, TranslateNameRequest request);
	}
}