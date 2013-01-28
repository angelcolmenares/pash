using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.Provider;
using Microsoft.ActiveDirectory.CustomActions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADAccountManagement : IDisposable
	{
		private ADSession _adSession;

		private ADSessionHandle _sessionHandle;

		private IADAccountManagement _acctMgmt;

		private bool _disposed;

		internal ADAccountManagement() : this(null)
		{
		}

		internal ADAccountManagement(ADSessionInfo sessionInfo)
		{
			this._adSession = ADSession.ConstructSession(sessionInfo);
		}

		internal void ChangePassword(string partitionDN, string accountDN, string oldPassword, string newPassword)
		{
			this.Init();
			ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest();
			changePasswordRequest.PartitionDN = partitionDN;
			changePasswordRequest.AccountDN = accountDN;
			changePasswordRequest.NewPassword = newPassword;
			changePasswordRequest.OldPassword = oldPassword;
			this._acctMgmt.ChangePassword(this._sessionHandle, changePasswordRequest);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Uninit();
			}
			this._disposed = true;
		}

		~ADAccountManagement()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		internal ADGroup[] GetAuthorizationGroups(string partitionDN, string principalDN)
		{
			this.Init();
			GetADPrincipalAuthorizationGroupRequest getADPrincipalAuthorizationGroupRequest = new GetADPrincipalAuthorizationGroupRequest();
			getADPrincipalAuthorizationGroupRequest.PrincipalDN = principalDN;
			getADPrincipalAuthorizationGroupRequest.PartitionDN = partitionDN;
			GetADPrincipalAuthorizationGroupResponse aDPrincipalAuthorizationGroup = this._acctMgmt.GetADPrincipalAuthorizationGroup(this._sessionHandle, getADPrincipalAuthorizationGroupRequest);
			List<ADGroup> aDGroups = new List<ADGroup>();
			if (aDPrincipalAuthorizationGroup.MemberOf == null)
			{
				return new ADGroup[0];
			}
			else
			{
				ActiveDirectoryGroup[] memberOf = aDPrincipalAuthorizationGroup.MemberOf;
				for (int i = 0; i < (int)memberOf.Length; i++)
				{
					ActiveDirectoryGroup activeDirectoryGroup = memberOf[i];
					ADGroup aDGroup = new ADGroup();
					this.PopulateADGroupFromWebServiceData(activeDirectoryGroup, aDGroup);
					aDGroups.Add(aDGroup);
				}
				return aDGroups.ToArray();
			}
		}

		internal ADPrincipal[] GetGroupMembers(string partitionDN, string groupDN, bool recursive)
		{
			this.Init();
			GetADGroupMemberRequest getADGroupMemberRequest = new GetADGroupMemberRequest();
			getADGroupMemberRequest.GroupDN = groupDN;
			getADGroupMemberRequest.PartitionDN = partitionDN;
			getADGroupMemberRequest.Recursive = recursive;
			GetADGroupMemberResponse aDGroupMember = this._acctMgmt.GetADGroupMember(this._sessionHandle, getADGroupMemberRequest);
			List<ADPrincipal> aDPrincipals = new List<ADPrincipal>();
			if (aDGroupMember.Members == null)
			{
				return new ADPrincipal[0];
			}
			else
			{
				ActiveDirectoryPrincipal[] members = aDGroupMember.Members;
				for (int i = 0; i < (int)members.Length; i++)
				{
					ActiveDirectoryPrincipal activeDirectoryPrincipal = members[i];
					ADPrincipal aDPrincipal = new ADPrincipal();
					this.PopulateADPrincipalFromWebServiceData(activeDirectoryPrincipal, aDPrincipal);
					aDPrincipals.Add(aDPrincipal);
				}
				return aDPrincipals.ToArray();
			}
		}

		private ActiveDirectoryNameFormat GetNameFormat(ADPathFormat pathFormat)
		{
			ActiveDirectoryNameFormat activeDirectoryNameFormat = ActiveDirectoryNameFormat.DistinguishedName;
			ADPathFormat aDPathFormat = pathFormat;
			switch (aDPathFormat)
			{
				case ADPathFormat.X500:
				{
					activeDirectoryNameFormat = ActiveDirectoryNameFormat.DistinguishedName;
					break;
				}
				case ADPathFormat.Canonical:
				{
					activeDirectoryNameFormat = ActiveDirectoryNameFormat.CanonicalName;
					break;
				}
			}
			return activeDirectoryNameFormat;
		}

		internal ADGroup[] GetPrincipalGroupMembership(string partitionDN, string principalDN, string resourceContextServer, string resourceContextPartition)
		{
			this.Init();
			GetADPrincipalGroupMembershipRequest getADPrincipalGroupMembershipRequest = new GetADPrincipalGroupMembershipRequest();
			getADPrincipalGroupMembershipRequest.PrincipalDN = principalDN;
			getADPrincipalGroupMembershipRequest.PartitionDN = partitionDN;
			getADPrincipalGroupMembershipRequest.ResourceContextServer = resourceContextServer;
			getADPrincipalGroupMembershipRequest.ResourceContextPartition = resourceContextPartition;
			GetADPrincipalGroupMembershipResponse aDPrincipalGroupMembership = this._acctMgmt.GetADPrincipalGroupMembership(this._sessionHandle, getADPrincipalGroupMembershipRequest);
			List<ADGroup> aDGroups = new List<ADGroup>();
			if (aDPrincipalGroupMembership.MemberOf == null)
			{
				return new ADGroup[0];
			}
			else
			{
				ActiveDirectoryGroup[] memberOf = aDPrincipalGroupMembership.MemberOf;
				for (int i = 0; i < (int)memberOf.Length; i++)
				{
					ActiveDirectoryGroup activeDirectoryGroup = memberOf[i];
					ADGroup aDGroup = new ADGroup();
					this.PopulateADGroupFromWebServiceData(activeDirectoryGroup, aDGroup);
					aDGroups.Add(aDGroup);
				}
				return aDGroups.ToArray();
			}
		}

		private void Init()
		{
			if (!this._disposed)
			{
				if (this._acctMgmt == null)
				{
					this._sessionHandle = this._adSession.GetSessionHandle();
					this._acctMgmt = this._adSession.GetAccountManagementInterface();
				}
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private void PopulateADGroupFromWebServiceData(ActiveDirectoryGroup inputWSGroup, ADGroup groupToPopulate)
		{
			ActiveDirectoryGroupScope groupScope = inputWSGroup.GroupScope;
			switch (groupScope)
			{
				case ActiveDirectoryGroupScope.DomainLocal:
				{
					groupToPopulate.GroupScope = new ADGroupScope?(ADGroupScope.DomainLocal);
					break;
				}
				case ActiveDirectoryGroupScope.Global:
				{
					groupToPopulate.GroupScope = new ADGroupScope?(ADGroupScope.Global);
					break;
				}
				case ActiveDirectoryGroupScope.Universal:
				{
					groupToPopulate.GroupScope = new ADGroupScope?(ADGroupScope.Universal);
					break;
				}
			}
			ActiveDirectoryGroupType groupType = inputWSGroup.GroupType;
			switch (groupType)
			{
				case ActiveDirectoryGroupType.Distribution:
				{
					groupToPopulate.GroupCategory = new ADGroupCategory?(ADGroupCategory.Distribution);
					break;
				}
				case ActiveDirectoryGroupType.Security:
				{
					groupToPopulate.GroupCategory = new ADGroupCategory?(ADGroupCategory.Security);
					break;
				}
			}
			this.PopulateADPrincipalFromWebServiceData(inputWSGroup, groupToPopulate);
		}

		private void PopulateADObjectFromWebServiceData(ActiveDirectoryObject inputWSObject, ADObject adobjectToPopulate)
		{
			adobjectToPopulate.DistinguishedName = inputWSObject.DistinguishedName;
			adobjectToPopulate.SetValue("name", inputWSObject.Name);
			adobjectToPopulate.ObjectClass = inputWSObject.ObjectClass;
			adobjectToPopulate.ObjectGuid = new Guid?(inputWSObject.ObjectGuid);
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
			aDPropertyValueCollection.AddRange(inputWSObject.ObjectTypes);
			adobjectToPopulate.ObjectTypes = aDPropertyValueCollection;
			adobjectToPopulate.SessionInfo = new ADSessionInfo(inputWSObject.ReferenceServer);
			if (this._adSession.SessionInfo.Credential != null)
			{
				adobjectToPopulate.SessionInfo.Credential = this._adSession.SessionInfo.Credential;
			}
			adobjectToPopulate.SessionInfo.AuthType = this._adSession.SessionInfo.AuthType;
			adobjectToPopulate.IsSearchResult = true;
		}

		private void PopulateADPrincipalFromWebServiceData(ActiveDirectoryPrincipal inputWSPrincipal, ADPrincipal principalToPopulate)
		{
			principalToPopulate.SID = new SecurityIdentifier(inputWSPrincipal.SID, 0);
			principalToPopulate.SamAccountName = inputWSPrincipal.SamAccountName;
			this.PopulateADObjectFromWebServiceData(inputWSPrincipal, principalToPopulate);
		}

		internal void SetPassword(string partitionDN, string accountDN, string newPassword)
		{
			this.Init();
			SetPasswordRequest setPasswordRequest = new SetPasswordRequest();
			setPasswordRequest.PartitionDN = partitionDN;
			setPasswordRequest.AccountDN = accountDN;
			setPasswordRequest.NewPassword = newPassword;
			this._acctMgmt.SetPassword(this._sessionHandle, setPasswordRequest);
		}

		internal string TranslateName(string name, ADPathFormat formatOffered, ADPathFormat formatDesired)
		{
			this.Init();
			TranslateNameRequest translateNameRequest = new TranslateNameRequest();
			translateNameRequest.Names = new string[1];
			translateNameRequest.Names[0] = name;
			translateNameRequest.FormatOffered = this.GetNameFormat(formatOffered);
			translateNameRequest.FormatDesired = this.GetNameFormat(formatDesired);
			TranslateNameResponse translateNameResponse = this._acctMgmt.TranslateName(this._sessionHandle, translateNameRequest);
			ActiveDirectoryNameTranslateResult[] nameTranslateResult = translateNameResponse.NameTranslateResult;
			if (nameTranslateResult[0].Result != 0)
			{
				object[] objArray = new object[1];
				objArray[0] = name;
				throw ExceptionHelper.GetExceptionFromErrorCode(Convert.ToInt32 (nameTranslateResult[0].Result), string.Format(CultureInfo.CurrentCulture, StringResources.TranslateNameError, objArray), null);
			}
			else
			{
				string str = nameTranslateResult[0].Name;
				return str;
			}
		}

		private void Uninit()
		{
			if (this._adSession != null)
			{
				this._adSession.Delete();
				this._adSession = null;
			}
		}
	}
}