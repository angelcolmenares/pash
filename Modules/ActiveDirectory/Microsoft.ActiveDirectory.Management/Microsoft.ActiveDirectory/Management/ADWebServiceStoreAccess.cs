using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.CustomActions;
using System;
using System.DirectoryServices.Protocols;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADWebServiceStoreAccess : ADStoreAccess, IADServiceStoreAccess
	{
		internal const string TypeName = "WebService";

		private const string _debugCategory = "ADWebServiceStoreAccess";

		private static ADWebServiceStoreAccess _adwsStoreAccess;

		static ADWebServiceStoreAccess()
		{
		}

		private ADWebServiceStoreAccess() : base("WebService")
		{
		}

		private void CheckAndThrowReferralException(ADResponse response)
		{
			if (response.ResultCode != ResultCode.Referral)
			{
				return;
			}
			else
			{
				throw new ADReferralException(response.ErrorMessage, 0x202b, response.Referral);
			}
		}

		private AdwsConnection GetInternalHandle(ADSessionHandle handle)
		{
			AdwsConnection adwsConnection = null;
			if (handle != null && handle.Handle != null)
			{
				adwsConnection = handle.Handle as AdwsConnection;
			}
			return adwsConnection;
		}

		public static ADWebServiceStoreAccess GetObject()
		{
			if (ADWebServiceStoreAccess._adwsStoreAccess == null)
			{
				ADWebServiceStoreAccess._adwsStoreAccess = new ADWebServiceStoreAccess();
			}
			return ADWebServiceStoreAccess._adwsStoreAccess;
		}


		ChangePasswordResponse Microsoft.ActiveDirectory.Management.IADAccountManagement.ChangePassword(ADSessionHandle handle, ChangePasswordRequest request)
		{
			ChangePasswordResponse changePasswordResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				changePasswordResponse = internalHandle.ChangePassword(request);
			}
			return changePasswordResponse;
		}

		GetADGroupMemberResponse Microsoft.ActiveDirectory.Management.IADAccountManagement.GetADGroupMember(ADSessionHandle handle, GetADGroupMemberRequest request)
		{
			GetADGroupMemberResponse aDGroupMember = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDGroupMember = internalHandle.GetADGroupMember(request);
			}
			return aDGroupMember;
		}

		GetADPrincipalAuthorizationGroupResponse Microsoft.ActiveDirectory.Management.IADAccountManagement.GetADPrincipalAuthorizationGroup(ADSessionHandle handle, GetADPrincipalAuthorizationGroupRequest request)
		{
			GetADPrincipalAuthorizationGroupResponse aDPrincipalAuthorizationGroup = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDPrincipalAuthorizationGroup = internalHandle.GetADPrincipalAuthorizationGroup(request);
			}
			return aDPrincipalAuthorizationGroup;
		}

		GetADPrincipalGroupMembershipResponse Microsoft.ActiveDirectory.Management.IADAccountManagement.GetADPrincipalGroupMembership(ADSessionHandle handle, GetADPrincipalGroupMembershipRequest request)
		{
			GetADPrincipalGroupMembershipResponse aDPrincipalGroupMembership = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDPrincipalGroupMembership = internalHandle.GetADPrincipalGroupMembership(request);
			}
			return aDPrincipalGroupMembership;
		}

		SetPasswordResponse Microsoft.ActiveDirectory.Management.IADAccountManagement.SetPassword(ADSessionHandle handle, SetPasswordRequest request)
		{
			SetPasswordResponse setPasswordResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				setPasswordResponse = internalHandle.SetPassword(request);
			}
			return setPasswordResponse;
		}

		TranslateNameResponse Microsoft.ActiveDirectory.Management.IADAccountManagement.TranslateName(ADSessionHandle handle, TranslateNameRequest request)
		{
			TranslateNameResponse translateNameResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				translateNameResponse = internalHandle.TranslateName(request);
			}
			return translateNameResponse;
		}

		ADSessionHandle Microsoft.ActiveDirectory.Management.IADSession.Create(ADSessionInfo info)
		{
			AdwsConnection adwsConnection = new AdwsConnection(info);
			return new ADSessionHandle(adwsConnection);
		}

		bool Microsoft.ActiveDirectory.Management.IADSession.Delete(ADSessionHandle handle)
		{
			bool flag = true;
			if (handle != null && handle.Handle != null)
			{
				AdwsConnection adwsConnection = handle.Handle as AdwsConnection;
				if (adwsConnection == null)
				{
					flag = false;
				}
				else
				{
					adwsConnection.Dispose();
				}
			}
			return flag;
		}

		object Microsoft.ActiveDirectory.Management.IADSession.GetOption(ADSessionHandle handle, int option)
		{
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			object serverName = null;
			if (internalHandle != null)
			{
				ADStoreAccess.LdapSessionOption ldapSessionOption = (ADStoreAccess.LdapSessionOption)option;
				if (ldapSessionOption != ADStoreAccess.LdapSessionOption.LDAP_OPT_HOST_NAME)
				{
					object[] objArray = new object[1];
					objArray[0] = serverName;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.NotSupportedSessionOption, objArray));
				}
				else
				{
					serverName = internalHandle.ServerName;
				}
			}
			return serverName;
		}

		bool Microsoft.ActiveDirectory.Management.IADSession.SetOption(ADSessionHandle handle, int option, object value)
		{
			throw new NotSupportedException();
		}

		void Microsoft.ActiveDirectory.Management.IADSyncOperations.AbandonSearch(ADSessionHandle handle, ADSearchRequest request)
		{
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				internalHandle.AbandonSearch(request);
			}
		}

		ADAddResponse Microsoft.ActiveDirectory.Management.IADSyncOperations.Add(ADSessionHandle handle, ADAddRequest request)
		{
			ADAddResponse aDAddResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDAddResponse = internalHandle.Create(request);
				this.CheckAndThrowReferralException(aDAddResponse);
				ADStoreAccess.ThrowExceptionForResultCodeError(aDAddResponse.ResultCode, aDAddResponse.ErrorMessage, null);
			}
			return aDAddResponse;
		}

		ADDeleteResponse Microsoft.ActiveDirectory.Management.IADSyncOperations.Delete(ADSessionHandle handle, ADDeleteRequest request)
		{
			ADDeleteResponse aDDeleteResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDDeleteResponse = internalHandle.Delete(request);
				this.CheckAndThrowReferralException(aDDeleteResponse);
				ADStoreAccess.ThrowExceptionForResultCodeError(aDDeleteResponse.ResultCode, aDDeleteResponse.ErrorMessage, null);
			}
			return aDDeleteResponse;
		}

		ADModifyResponse Microsoft.ActiveDirectory.Management.IADSyncOperations.Modify(ADSessionHandle handle, ADModifyRequest request)
		{
			ADModifyResponse aDModifyResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDModifyResponse = internalHandle.Modify(request);
				this.CheckAndThrowReferralException(aDModifyResponse);
				ADStoreAccess.ThrowExceptionForResultCodeError(aDModifyResponse.ResultCode, aDModifyResponse.ErrorMessage, null);
			}
			return aDModifyResponse;
		}

		ADModifyDNResponse Microsoft.ActiveDirectory.Management.IADSyncOperations.ModifyDN(ADSessionHandle handle, ADModifyDNRequest request)
		{
			ADModifyDNResponse aDModifyDNResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDModifyDNResponse = internalHandle.ModifyDN(request);
				this.CheckAndThrowReferralException(aDModifyDNResponse);
				ADStoreAccess.ThrowExceptionForResultCodeError(aDModifyDNResponse.ResultCode, aDModifyDNResponse.ErrorMessage, null);
			}
			return aDModifyDNResponse;
		}

		ADSearchResponse Microsoft.ActiveDirectory.Management.IADSyncOperations.Search(ADSessionHandle handle, ADSearchRequest request)
		{
			ADSearchResponse aDSearchResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDSearchResponse = internalHandle.Search(request);
				this.CheckAndThrowReferralException(aDSearchResponse);
				ADStoreAccess.ThrowExceptionForResultCodeError(aDSearchResponse.ResultCode, aDSearchResponse.ErrorMessage, null);
			}
			return aDSearchResponse;
		}

		ChangeOptionalFeatureResponse Microsoft.ActiveDirectory.Management.IADTopologyManagement.ChangeOptionalFeature(ADSessionHandle handle, ChangeOptionalFeatureRequest request)
		{
			ChangeOptionalFeatureResponse changeOptionalFeatureResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				changeOptionalFeatureResponse = internalHandle.ChangeOptionalFeature(request);
			}
			return changeOptionalFeatureResponse;
		}

		GetADDomainResponse Microsoft.ActiveDirectory.Management.IADTopologyManagement.GetADDomain(ADSessionHandle handle, GetADDomainRequest request)
		{
			GetADDomainResponse aDDomain = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDDomain = internalHandle.GetADDomain(request);
			}
			return aDDomain;
		}

		GetADDomainControllerResponse Microsoft.ActiveDirectory.Management.IADTopologyManagement.GetADDomainController(ADSessionHandle handle, GetADDomainControllerRequest request)
		{
			GetADDomainControllerResponse aDDomainController = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDDomainController = internalHandle.GetADDomainController(request);
			}
			return aDDomainController;
		}

		GetADForestResponse Microsoft.ActiveDirectory.Management.IADTopologyManagement.GetADForest(ADSessionHandle handle, GetADForestRequest request)
		{
			GetADForestResponse aDForest = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				aDForest = internalHandle.GetADForest(request);
			}
			return aDForest;
		}

		MoveADOperationMasterRoleResponse Microsoft.ActiveDirectory.Management.IADTopologyManagement.MoveADOperationMasterRole(ADSessionHandle handle, MoveADOperationMasterRoleRequest request)
		{
			MoveADOperationMasterRoleResponse moveADOperationMasterRoleResponse = null;
			AdwsConnection internalHandle = this.GetInternalHandle(handle);
			if (internalHandle != null)
			{
				moveADOperationMasterRoleResponse = internalHandle.MoveADOperationMasterRole(request);
			}
			return moveADOperationMasterRoleResponse;
		}
	}
}