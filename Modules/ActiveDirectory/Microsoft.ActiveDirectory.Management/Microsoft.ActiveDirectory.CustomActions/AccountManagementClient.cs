using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class AccountManagementClient : ClientBase<AccountManagement>, AccountManagement
	{
		public AccountManagementClient()
		{
		}

		public AccountManagementClient(string endpointConfigurationName) : base(endpointConfigurationName)
		{
		}

		public AccountManagementClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public AccountManagementClient(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
		{
		}

		public AccountManagementClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
		{
		}

		public void ChangePassword(string Server, string AccountDN, string NewPassword, string OldPassword, string PartitionDN)
		{
			ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest();
			changePasswordRequest.Server = Server;
			changePasswordRequest.AccountDN = AccountDN;
			changePasswordRequest.NewPassword = NewPassword;
			changePasswordRequest.OldPassword = OldPassword;
			changePasswordRequest.PartitionDN = PartitionDN;
			this.Channel.ChangePassword(changePasswordRequest);
		}

		public ActiveDirectoryPrincipal[] GetADGroupMember(string Server, string GroupDN, string PartitionDN, bool Recursive)
		{
			GetADGroupMemberRequest getADGroupMemberRequest = new GetADGroupMemberRequest();
			getADGroupMemberRequest.Server = Server;
			getADGroupMemberRequest.GroupDN = GroupDN;
			getADGroupMemberRequest.PartitionDN = PartitionDN;
			getADGroupMemberRequest.Recursive = Recursive;
			GetADGroupMemberResponse aDGroupMember = this.Channel.GetADGroupMember(getADGroupMemberRequest);
			return aDGroupMember.Members;
		}

		public ActiveDirectoryGroup[] GetADPrincipalAuthorizationGroup(string Server, string PartitionDN, string PrincipalDN)
		{
			GetADPrincipalAuthorizationGroupRequest getADPrincipalAuthorizationGroupRequest = new GetADPrincipalAuthorizationGroupRequest();
			getADPrincipalAuthorizationGroupRequest.Server = Server;
			getADPrincipalAuthorizationGroupRequest.PartitionDN = PartitionDN;
			getADPrincipalAuthorizationGroupRequest.PrincipalDN = PrincipalDN;
			GetADPrincipalAuthorizationGroupResponse aDPrincipalAuthorizationGroup = this.Channel.GetADPrincipalAuthorizationGroup(getADPrincipalAuthorizationGroupRequest);
			return aDPrincipalAuthorizationGroup.MemberOf;
		}

		public ActiveDirectoryGroup[] GetADPrincipalGroupMembership(string Server, string PartitionDN, string PrincipalDN, string ResourceContextPartition, string ResourceContextServer)
		{
			GetADPrincipalGroupMembershipRequest getADPrincipalGroupMembershipRequest = new GetADPrincipalGroupMembershipRequest();
			getADPrincipalGroupMembershipRequest.Server = Server;
			getADPrincipalGroupMembershipRequest.PartitionDN = PartitionDN;
			getADPrincipalGroupMembershipRequest.PrincipalDN = PrincipalDN;
			getADPrincipalGroupMembershipRequest.ResourceContextPartition = ResourceContextPartition;
			getADPrincipalGroupMembershipRequest.ResourceContextServer = ResourceContextServer;
			GetADPrincipalGroupMembershipResponse aDPrincipalGroupMembership = this.Channel.GetADPrincipalGroupMembership(getADPrincipalGroupMembershipRequest);
			return aDPrincipalGroupMembership.MemberOf;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		ChangePasswordResponse Microsoft.ActiveDirectory.CustomActions.AccountManagement.ChangePassword(ChangePasswordRequest request)
		{
			return base.Channel.ChangePassword(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetADGroupMemberResponse Microsoft.ActiveDirectory.CustomActions.AccountManagement.GetADGroupMember(GetADGroupMemberRequest request)
		{
			return base.Channel.GetADGroupMember(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetADPrincipalAuthorizationGroupResponse Microsoft.ActiveDirectory.CustomActions.AccountManagement.GetADPrincipalAuthorizationGroup(GetADPrincipalAuthorizationGroupRequest request)
		{
			return base.Channel.GetADPrincipalAuthorizationGroup(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetADPrincipalGroupMembershipResponse Microsoft.ActiveDirectory.CustomActions.AccountManagement.GetADPrincipalGroupMembership(GetADPrincipalGroupMembershipRequest request)
		{
			return base.Channel.GetADPrincipalGroupMembership(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		SetPasswordResponse Microsoft.ActiveDirectory.CustomActions.AccountManagement.SetPassword(SetPasswordRequest request)
		{
			return base.Channel.SetPassword(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		TranslateNameResponse Microsoft.ActiveDirectory.CustomActions.AccountManagement.TranslateName(TranslateNameRequest request)
		{
			return base.Channel.TranslateName(request);
		}

		public void SetPassword(string Server, string AccountDN, string NewPassword, string PartitionDN)
		{
			SetPasswordRequest setPasswordRequest = new SetPasswordRequest();
			setPasswordRequest.Server = Server;
			setPasswordRequest.AccountDN = AccountDN;
			setPasswordRequest.NewPassword = NewPassword;
			setPasswordRequest.PartitionDN = PartitionDN;
			this.Channel.SetPassword(setPasswordRequest);
		}

		public ActiveDirectoryNameTranslateResult[] TranslateName(string Server, ActiveDirectoryNameFormat FormatDesired, ActiveDirectoryNameFormat FormatOffered, string[] Names)
		{
			TranslateNameRequest translateNameRequest = new TranslateNameRequest();
			translateNameRequest.Server = Server;
			translateNameRequest.FormatDesired = FormatDesired;
			translateNameRequest.FormatOffered = FormatOffered;
			translateNameRequest.Names = Names;
			TranslateNameResponse translateNameResponse = this.Channel.TranslateName(translateNameRequest);
			return translateNameResponse.NameTranslateResult;
		}
	}
}