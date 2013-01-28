using System;
using System.Globalization;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace System.DirectoryServices.AccountManagement
{
	internal abstract class StoreCtx : IDisposable
	{
		private PrincipalContext owningContext;

		internal static string[] principalProperties;

		internal static string[] authenticablePrincipalProperties;

		internal static string[] userProperties;

		internal static string[] groupProperties;

		internal static string[] computerProperties;

		internal abstract string BasePath
		{
			get;
		}

		internal PrincipalContext OwningContext
		{
			[SecurityCritical]
			get
			{
				return this.owningContext;
			}
			[SecurityCritical]
			set
			{
				this.owningContext = value;
			}
		}

		internal abstract bool SupportsNativeMembershipTest
		{
			get;
		}

		internal abstract bool SupportsSearchNatively
		{
			get;
		}

		static StoreCtx()
		{
			string[] strArrays = new string[10];
			strArrays[0] = "Principal.DisplayName";
			strArrays[1] = "Principal.Description";
			strArrays[2] = "Principal.SamAccountName";
			strArrays[3] = "Principal.UserPrincipalName";
			strArrays[4] = "Principal.Guid";
			strArrays[5] = "Principal.Sid";
			strArrays[6] = "Principal.StructuralObjectClass";
			strArrays[7] = "Principal.Name";
			strArrays[8] = "Principal.DistinguishedName";
			strArrays[9] = "Principal.ExtensionCache";
			StoreCtx.principalProperties = strArrays;
			string[] strArrays1 = new string[9];
			strArrays1[0] = "AuthenticablePrincipal.Enabled";
			strArrays1[1] = "AuthenticablePrincipal.Certificates";
			strArrays1[2] = "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt";
			strArrays1[3] = "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";
			strArrays1[4] = "AuthenticablePrincipal.AccountInfoExpired";
			strArrays1[5] = "AuthenticablePrincipal.AccountInfo.LastLogon";
			strArrays1[6] = "AuthenticablePrincipal.AccountInfo.AccountLockoutTime";
			strArrays1[7] = "AuthenticablePrincipal.AccountInfo.BadLogonCount";
			strArrays1[8] = "AuthenticablePrincipal.PasswordInfo.LastPasswordSet";
			StoreCtx.authenticablePrincipalProperties = strArrays1;
			string[] strArrays2 = new string[17];
			strArrays2[0] = "UserPrincipal.GivenName";
			strArrays2[1] = "UserPrincipal.MiddleName";
			strArrays2[2] = "UserPrincipal.Surname";
			strArrays2[3] = "UserPrincipal.EmailAddress";
			strArrays2[4] = "UserPrincipal.VoiceTelephoneNumber";
			strArrays2[5] = "UserPrincipal.EmployeeId";
			strArrays2[6] = "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";
			strArrays2[7] = "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";
			strArrays2[8] = "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";
			strArrays2[9] = "AuthenticablePrincipal.AccountInfo.DelegationPermitted";
			strArrays2[10] = "AuthenticablePrincipal.AccountInfo.HomeDirectory";
			strArrays2[11] = "AuthenticablePrincipal.AccountInfo.HomeDrive";
			strArrays2[12] = "AuthenticablePrincipal.AccountInfo.ScriptPath";
			strArrays2[13] = "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";
			strArrays2[14] = "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";
			strArrays2[15] = "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";
			strArrays2[16] = "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";
			StoreCtx.userProperties = strArrays2;
			string[] strArrays3 = new string[2];
			strArrays3[0] = "GroupPrincipal.IsSecurityGroup";
			strArrays3[1] = "GroupPrincipal.GroupScope";
			StoreCtx.groupProperties = strArrays3;
			string[] strArrays4 = new string[1];
			strArrays4[0] = "ComputerPrincipal.ServicePrincipalNames";
			StoreCtx.computerProperties = strArrays4;
		}

		protected StoreCtx()
		{
		}

		internal abstract bool AccessCheck(Principal p, PrincipalAccessMask targetPermission);

		[SecurityCritical]
		private void BuildFilterSet(Principal p, string[] propertySet, QbeFilterDescription qbeFilterDescription)
		{
			string[] strArrays = propertySet;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (p.GetChangeStatusForProperty(str))
				{
					object valueForProperty = p.GetValueForProperty(str);
					if (valueForProperty as PrincipalValueCollection<string> == null)
					{
						if (valueForProperty as X509Certificate2Collection == null)
						{
							object obj = FilterFactory.CreateFilter(str);
							if (valueForProperty != null)
							{
								if (!valueForProperty as bool)
								{
									if (valueForProperty as string == null)
									{
										if (valueForProperty as GroupScope == GroupScope.Local)
										{
											if (valueForProperty as byte[] == null)
											{
												if (valueForProperty as DateTime? == null)
												{
													if (valueForProperty as ExtensionCache == null)
													{
														if (valueForProperty as QbeMatchType != null)
														{
															((FilterBase)obj).Value = (QbeMatchType)valueForProperty;
														}
													}
													else
													{
														((FilterBase)obj).Value = (ExtensionCache)valueForProperty;
													}
												}
												else
												{
													((FilterBase)obj).Value = (DateTime?)valueForProperty;
												}
											}
											else
											{
												((FilterBase)obj).Value = (byte[])valueForProperty;
											}
										}
										else
										{
											((FilterBase)obj).Value = (GroupScope)valueForProperty;
										}
									}
									else
									{
										((FilterBase)obj).Value = (string)valueForProperty;
									}
								}
								else
								{
									((FilterBase)obj).Value = (bool)valueForProperty;
								}
							}
							else
							{
								((FilterBase)obj).Value = null;
							}
							qbeFilterDescription.FiltersToApply.Add(obj);
						}
						else
						{
							X509Certificate2Collection x509Certificate2Collection = (X509Certificate2Collection)valueForProperty;
							X509Certificate2Enumerator enumerator = x509Certificate2Collection.GetEnumerator();
							while (enumerator.MoveNext())
							{
								X509Certificate2 current = enumerator.Current;
								object obj1 = FilterFactory.CreateFilter(str);
								((FilterBase)obj1).Value = current;
								qbeFilterDescription.FiltersToApply.Add(obj1);
							}
						}
					}
					else
					{
						PrincipalValueCollection<string> strs = (PrincipalValueCollection<string>)valueForProperty;
						foreach (string inserted in strs.Inserted)
						{
							object obj2 = FilterFactory.CreateFilter(str);
							((FilterBase)obj2).Value = inserted;
							qbeFilterDescription.FiltersToApply.Add(obj2);
						}
					}
				}
			}
		}

		[SecurityCritical]
		protected QbeFilterDescription BuildQbeFilterDescription(Principal p)
		{
			QbeFilterDescription qbeFilterDescription = new QbeFilterDescription();
			if (p != null)
			{
				this.BuildFilterSet(p, StoreCtx.principalProperties, qbeFilterDescription);
			}
			if (p as AuthenticablePrincipal != null)
			{
				this.BuildFilterSet(p, StoreCtx.authenticablePrincipalProperties, qbeFilterDescription);
			}
			if (p as UserPrincipal != null)
			{
				if (!p.GetChangeStatusForProperty("AuthenticablePrincipal.AccountInfo.AccountExpirationDate") || !p.GetChangeStatusForProperty("AuthenticablePrincipal.AccountInfoExpired"))
				{
					this.BuildFilterSet(p, StoreCtx.userProperties, qbeFilterDescription);
				}
				else
				{
					object[] externalForm = new object[1];
					externalForm[0] = PropertyNamesExternal.GetExternalForm("AuthenticablePrincipal.AccountInfo.AccountExpirationDate");
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxMultipleFiltersForPropertyUnsupported, externalForm));
				}
			}
			if (p as GroupPrincipal != null)
			{
				this.BuildFilterSet(p, StoreCtx.groupProperties, qbeFilterDescription);
			}
			if (p as ComputerPrincipal != null)
			{
				this.BuildFilterSet(p, StoreCtx.computerProperties, qbeFilterDescription);
			}
			return qbeFilterDescription;
		}

		internal abstract bool CanGroupBeCleared(GroupPrincipal g, out string explanationForFailure);

		internal abstract bool CanGroupMemberBeRemoved(GroupPrincipal g, Principal member, out string explanationForFailure);

		internal abstract void ChangePassword(AuthenticablePrincipal p, string oldPassword, string newPassword);

		internal abstract Principal ConstructFakePrincipalFromSID(byte[] sid);

		internal abstract void Delete(Principal p);

		public virtual void Dispose()
		{
		}

		internal abstract void ExpirePassword(AuthenticablePrincipal p);

		internal abstract ResultSet FindByBadPasswordAttempt(DateTime dt, MatchType matchType, Type principalType);

		internal abstract ResultSet FindByExpirationTime(DateTime dt, MatchType matchType, Type principalType);

		internal abstract ResultSet FindByLockoutTime(DateTime dt, MatchType matchType, Type principalType);

		internal abstract ResultSet FindByLogonTime(DateTime dt, MatchType matchType, Type principalType);

		internal abstract ResultSet FindByPasswordSetTime(DateTime dt, MatchType matchType, Type principalType);

		internal abstract Principal FindPrincipalByIdentRef(Type principalType, string urnScheme, string urnValue, DateTime referenceDate);

		internal abstract Principal GetAsPrincipal(object storeObject, object discriminant);

		internal abstract BookmarkableResultSet GetGroupMembership(GroupPrincipal g, bool recursive);

		internal abstract ResultSet GetGroupsMemberOf(Principal p);

		internal abstract ResultSet GetGroupsMemberOf(Principal foreignPrincipal, StoreCtx foreignContext);

		internal abstract ResultSet GetGroupsMemberOfAZ(Principal p);

		internal abstract void InitializeUserAccountControl(AuthenticablePrincipal p);

		internal abstract void Insert(Principal p);

		internal abstract bool IsLockedOut(AuthenticablePrincipal p);

		internal abstract bool IsMemberOfInStore(GroupPrincipal g, Principal p);

		internal abstract bool IsValidProperty(Principal p, string propertyName);

		internal abstract void Load(Principal p);

		internal abstract void Load(Principal p, string principalPropertyName);

		internal abstract void Move(StoreCtx originalStore, Principal p);

		internal abstract Type NativeType(Principal p);

		internal abstract object PushChangesToNative(Principal p);

		internal abstract object PushFilterToNativeSearcher(PrincipalSearcher ps);

		internal abstract ResultSet Query(PrincipalSearcher ps, int sizeLimit);

		internal abstract Principal ResolveCrossStoreRefToPrincipal(object o);

		internal abstract Type SearcherNativeType();

		internal abstract void SetPassword(AuthenticablePrincipal p, string newPassword);

		internal abstract CredentialTypes SupportedCredTypes(AuthenticablePrincipal p);

		internal abstract bool SupportsAccounts(AuthenticablePrincipal p);

		internal abstract void UnexpirePassword(AuthenticablePrincipal p);

		internal abstract void UnlockAccount(AuthenticablePrincipal p);

		internal abstract void Update(Principal p);
	}
}