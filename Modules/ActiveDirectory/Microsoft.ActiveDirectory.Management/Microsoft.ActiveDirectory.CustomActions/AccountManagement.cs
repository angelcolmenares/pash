using System.CodeDom.Compiler;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[ServiceContract(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", ConfigurationName="AccountManagement", SessionMode=SessionMode.Required)]
	internal interface AccountManagement
	{
		[FaultContract(typeof(ChangePasswordFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="ChangePasswordFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/ChangePassword", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/ChangePasswordResponse")]
		ChangePasswordResponse ChangePassword(ChangePasswordRequest request);

		[FaultContract(typeof(GetADGroupMemberFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="GetADGroupMemberFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/GetADGroupMember", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/GetADGroupMemberResponse")]
		GetADGroupMemberResponse GetADGroupMember(GetADGroupMemberRequest request);

		[FaultContract(typeof(GetADPrincipalAuthorizationGroupFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="GetADPrincipalAuthorizationGroupFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/GetADPrincipalAuthorizationGroup", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/GetADPrincipalAuthorizationGroupResponse")]
		GetADPrincipalAuthorizationGroupResponse GetADPrincipalAuthorizationGroup(GetADPrincipalAuthorizationGroupRequest request);

		[FaultContract(typeof(GetADPrincipalGroupMembershipFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="GetADPrincipalGroupMembershipFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/GetADPrincipalGroupMembership", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/GetADPrincipalGroupMembershipResponse")]
		GetADPrincipalGroupMembershipResponse GetADPrincipalGroupMembership(GetADPrincipalGroupMembershipRequest request);

		[FaultContract(typeof(SetPasswordFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="SetPasswordFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/SetPassword", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/SetPasswordResponse")]
		SetPasswordResponse SetPassword(SetPasswordRequest request);

		[FaultContract(typeof(TranslateNameFault), Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name="TranslateNameFault")]
		[OperationContract(Action="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/TranslateName", ReplyAction="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions/AccountManagement/TranslateNameResponse")]
		TranslateNameResponse TranslateName(TranslateNameRequest request);
	}
}