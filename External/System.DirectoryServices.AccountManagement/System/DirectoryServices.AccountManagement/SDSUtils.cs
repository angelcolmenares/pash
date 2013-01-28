using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.DirectoryServices.AccountManagement
{
	internal class SDSUtils
	{
		internal const int AD_DefaultUAC = 0x222;

		internal const int AD_DefaultUAC_Machine = 0x1022;

		internal const int SAM_DefaultUAC = 0x201;

		private SDSUtils()
		{
		}

		[SecurityCritical]
		internal static void AccountControlFromDirectoryEntry(dSPropertyCollection properties, string suggestedProperty, Principal p, string propertyName, bool testCantChangePassword)
		{
			dSPropertyValueCollection item = properties[suggestedProperty];
			if (item.Count != 0)
			{
				int num = (int)item[0];
				bool flag = SDSUtils.StatusFromAccountControl(num, propertyName);
				p.LoadValueIntoProperty(propertyName, flag);
			}
		}

		[SecurityCritical]
		internal static void AccountControlToDirectoryEntry(Principal p, string propertyName, DirectoryEntry de, string suggestedProperty, bool isSAM, bool isUnpersisted)
		{
			uint num;
			bool valueForProperty = (bool)p.GetValueForProperty(propertyName);
			if (de.Properties[suggestedProperty].Count <= 0)
			{
				throw new PrincipalOperationException(StringResources.ADStoreCtxUnableToReadExistingAccountControlFlagsForUpdate);
			}
			else
			{
				int item = (int)de.Properties[suggestedProperty][0];
				if (!isSAM && de.Properties["msDS-User-Account-Control-Computed"].Count > 0)
				{
					item = item | (int)de.Properties["msDS-User-Account-Control-Computed"][0];
				}
				string str = propertyName;
				string str1 = str;
				if (str != null)
				{
					if (str1 == "AuthenticablePrincipal.Enabled")
					{
						if (!isUnpersisted || isSAM)
						{
							num = 2;
							valueForProperty = !valueForProperty;
							if (!valueForProperty)
							{
								Utils.ClearBit(ref item, num);
							}
							else
							{
								Utils.SetBit(ref item, num);
							}
							de.Properties[suggestedProperty].Value = (object)item;
							return;
						}
						else
						{
							num = 0;
							if (!valueForProperty)
							{
								Utils.ClearBit(ref item, num);
							}
							else
							{
								Utils.SetBit(ref item, num);
							}
							de.Properties[suggestedProperty].Value = (object)item;
							return;
						}
					}
					else if (str1 == "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired")
					{
						num = 0x40000;
						if (!valueForProperty)
						{
							Utils.ClearBit(ref item, num);
						}
						else
						{
							Utils.SetBit(ref item, num);
						}
						de.Properties[suggestedProperty].Value = (object)item;
						return;
					}
					else if (str1 == "AuthenticablePrincipal.AccountInfo.DelegationPermitted")
					{
						num = 0x100000;
						valueForProperty = !valueForProperty;
						if (!valueForProperty)
						{
							Utils.ClearBit(ref item, num);
						}
						else
						{
							Utils.SetBit(ref item, num);
						}
						de.Properties[suggestedProperty].Value = (object)item;
						return;
					}
					else if (str1 == "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired")
					{
						num = 32;
						if (!valueForProperty)
						{
							Utils.ClearBit(ref item, num);
						}
						else
						{
							Utils.SetBit(ref item, num);
						}
						de.Properties[suggestedProperty].Value = (object)item;
						return;
					}
					else if (str1 == "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires")
					{
						num = 0x10000;
						if (!valueForProperty)
						{
							Utils.ClearBit(ref item, num);
						}
						else
						{
							Utils.SetBit(ref item, num);
						}
						de.Properties[suggestedProperty].Value = (object)item;
						return;
					}
					else if (str1 == "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption")
					{
						num = 128;
						if (!valueForProperty)
						{
							Utils.ClearBit(ref item, num);
						}
						else
						{
							Utils.SetBit(ref item, num);
						}
						de.Properties[suggestedProperty].Value = (object)item;
						return;
					}
					else if (str1 == "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword")
					{
						if (!isSAM)
						{
							num = 0;
							if (!valueForProperty)
							{
								Utils.ClearBit(ref item, num);
							}
							else
							{
								Utils.SetBit(ref item, num);
							}
							de.Properties[suggestedProperty].Value = (object)item;
							return;
						}
						num = 64;
						if (!valueForProperty)
						{
							Utils.ClearBit(ref item, num);
						}
						else
						{
							Utils.SetBit(ref item, num);
						}
						de.Properties[suggestedProperty].Value = (object)item;
						return;
					}
					num = 0;
					if (!valueForProperty)
					{
						Utils.ClearBit(ref item, num);
					}
					else
					{
						Utils.SetBit(ref item, num);
					}
					de.Properties[suggestedProperty].Value = (object)item;
					return;
				}
				else
				{
					num = 0;
					if (!valueForProperty)
					{
						Utils.ClearBit(ref item, num);
					}
					else
					{
						Utils.SetBit(ref item, num);
					}
					de.Properties[suggestedProperty].Value = (object)item;
					return;
				}
				if (!valueForProperty)
				{
					Utils.ClearBit(ref item, num);
				}
				else
				{
					Utils.SetBit(ref item, num);
				}
				de.Properties[suggestedProperty].Value = item;
				return;
			}
		}

		[SecurityCritical]
		internal static void ApplyChangesToDirectory(Principal p, StoreCtx storeCtx, SDSUtils.GroupMembershipUpdater updateGroupMembership, NetCred credentials, AuthenticationTypes authTypes)
		{
			DirectoryEntry native = (DirectoryEntry)storeCtx.PushChangesToNative(p);
			try
			{
				native.CommitChanges();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
			if (p as GroupPrincipal != null && p.GetChangeStatusForProperty("GroupPrincipal.Members"))
			{
				updateGroupMembership(p, native, credentials, authTypes);
			}
		}

		[SecurityCritical]
		internal static DirectoryEntry BuildDirectoryEntry(string path, NetCred credentials, AuthenticationTypes authTypes)
		{
			string userName;
			string password;
			string str = path;
			if (credentials != null)
			{
				userName = credentials.UserName;
			}
			else
			{
				userName = null;
			}
			if (credentials != null)
			{
				password = credentials.Password;
			}
			else
			{
				password = null;
			}
			DirectoryEntry directoryEntry = new DirectoryEntry(str, userName, password, authTypes);
			return directoryEntry;
		}

		[SecurityCritical]
		internal static DirectoryEntry BuildDirectoryEntry(NetCred credentials, AuthenticationTypes authTypes)
		{
			string userName;
			string password;
			DirectoryEntry directoryEntry = new DirectoryEntry();
			DirectoryEntry directoryEntry1 = directoryEntry;
			if (credentials != null)
			{
				userName = credentials.UserName;
			}
			else
			{
				userName = null;
			}
			directoryEntry1.Username = userName;
			DirectoryEntry directoryEntry2 = directoryEntry;
			if (credentials != null)
			{
				password = credentials.Password;
			}
			else
			{
				password = null;
			}
			directoryEntry2.Password = password;
			directoryEntry.AuthenticationType = authTypes;
			return directoryEntry;
		}

		[SecurityCritical]
		internal static void ChangePassword(DirectoryEntry de, string oldPassword, string newPassword)
		{
			try
			{
				object[] objArray = new object[2];
				objArray[0] = oldPassword;
				objArray[1] = newPassword;
				de.Invoke("ChangePassword", objArray);
			}
			catch (TargetInvocationException targetInvocationException1)
			{
				TargetInvocationException targetInvocationException = targetInvocationException1;
				if (targetInvocationException.InnerException as COMException == null)
				{
					throw;
				}
				else
				{
					if (((COMException)targetInvocationException.InnerException).ErrorCode != ExceptionHelper.ERROR_HRESULT_CONSTRAINT_VIOLATION)
					{
						throw ExceptionHelper.GetExceptionFromCOMException((COMException)targetInvocationException.InnerException);
					}
					else
					{
						throw new PasswordException(((COMException)targetInvocationException.InnerException).Message, (COMException)targetInvocationException.InnerException);
					}
				}
			}
		}

		internal static string ConstructDnsDomainNameFromDn(string dn)
		{
			char[] chrArray = new char[1];
			chrArray[0] = ',';
			string[] strArrays = dn.Split(chrArray);
			StringBuilder stringBuilder = new StringBuilder();
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i];
				if (str.Length > 3 && string.Compare(str.Substring(0, 3), "DC=", StringComparison.OrdinalIgnoreCase) == 0)
				{
					stringBuilder.Append(str.Substring(3));
					stringBuilder.Append(".");
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		internal static DirectorySearcher ConstructSearcher(DirectoryEntry de)
		{
			DirectorySearcher directorySearcher = new DirectorySearcher(de);
			directorySearcher.ClientTimeout = new TimeSpan(0, 0, 30);
			directorySearcher.PageSize = 0x100;
			return directorySearcher;
		}

		[SecurityCritical]
		internal static void DeleteDirectoryEntry(DirectoryEntry deToDelete)
		{
			DirectoryEntry parent = deToDelete.Parent;
			try
			{
				parent.Children.Remove(deToDelete);
			}
			finally
			{
				parent.Dispose();
			}
		}

		[SecurityCritical]
		internal static Principal DirectoryEntryToPrincipal(DirectoryEntry de, PrincipalContext owningContext, Type principalType)
		{
			Principal principal;
			if (typeof(UserPrincipal) != principalType)
			{
				if (typeof(ComputerPrincipal) != principalType)
				{
					if (typeof(GroupPrincipal) != principalType)
					{
						if (null == principalType || typeof(AuthenticablePrincipal) == principalType || typeof(Principal) == principalType)
						{
							if (!SDSUtils.IsOfObjectClass(de, "computer"))
							{
								if (!SDSUtils.IsOfObjectClass(de, "user"))
								{
									if (!SDSUtils.IsOfObjectClass(de, "group"))
									{
										principal = AuthenticablePrincipal.MakeAuthenticablePrincipal(owningContext);
									}
									else
									{
										principal = GroupPrincipal.MakeGroup(owningContext);
									}
								}
								else
								{
									principal = UserPrincipal.MakeUser(owningContext);
								}
							}
							else
							{
								principal = ComputerPrincipal.MakeComputer(owningContext);
							}
						}
						else
						{
							principal = Principal.MakePrincipal(owningContext, principalType);
						}
					}
					else
					{
						principal = GroupPrincipal.MakeGroup(owningContext);
					}
				}
				else
				{
					principal = ComputerPrincipal.MakeComputer(owningContext);
				}
			}
			else
			{
				principal = UserPrincipal.MakeUser(owningContext);
			}
			principal.UnderlyingObject = de;
			return principal;
		}

		[SecurityCritical]
		internal static void InsertPrincipal(Principal p, StoreCtx storeCtx, SDSUtils.GroupMembershipUpdater updateGroupMembership, NetCred credentials, AuthenticationTypes authTypes, bool needToSetPassword)
		{
			if (p as UserPrincipal != null || p as GroupPrincipal != null || p as AuthenticablePrincipal != null || p as ComputerPrincipal != null)
			{
				SDSUtils.ApplyChangesToDirectory(p, storeCtx, updateGroupMembership, credentials, authTypes);
				if (needToSetPassword && p.GetChangeStatusForProperty("AuthenticablePrincipal.PasswordInfo.Password"))
				{
					string valueForProperty = (string)p.GetValueForProperty("AuthenticablePrincipal.PasswordInfo.Password");
					storeCtx.SetPassword((AuthenticablePrincipal)p, valueForProperty);
				}
				if (p.GetChangeStatusForProperty("AuthenticablePrincipal.PasswordInfo.ExpireImmediately"))
				{
					bool flag = (bool)p.GetValueForProperty("AuthenticablePrincipal.PasswordInfo.ExpireImmediately");
					if (flag)
					{
						storeCtx.ExpirePassword((AuthenticablePrincipal)p);
					}
				}
				return;
			}
			else
			{
				object[] str = new object[1];
				str[0] = p.GetType().ToString();
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPrincipalTypeForSave, str));
			}
		}

		[SecuritySafeCritical]
		internal static bool IsObjectFromGC(string path)
		{
			return path.StartsWith("GC:", StringComparison.OrdinalIgnoreCase);
		}

		[SecurityCritical]
		private static bool IsOfObjectClass(SearchResult sr, string className)
		{
			return ADUtils.IsOfObjectClass(sr, className);
		}

		[SecurityCritical]
		private static bool IsOfObjectClass(DirectoryEntry de, string className)
		{
			if (!de.Path.StartsWith("WinNT:", StringComparison.Ordinal))
			{
				return ADUtils.IsOfObjectClass(de, className);
			}
			else
			{
				return SAMUtils.IsOfObjectClass(de, className);
			}
		}

		internal static AuthenticationTypes MapOptionsToAuthTypes(ContextOptions options)
		{
			AuthenticationTypes authenticationType = AuthenticationTypes.Secure;
			if ((options & ContextOptions.SimpleBind) != 0)
			{
				authenticationType = AuthenticationTypes.None;
			}
			if ((options & ContextOptions.ServerBind) != 0)
			{
				authenticationType = authenticationType | AuthenticationTypes.ServerBind;
			}
			if ((options & ContextOptions.SecureSocketLayer) != 0)
			{
				authenticationType = authenticationType | AuthenticationTypes.Encryption;
			}
			if ((options & ContextOptions.Signing) != 0)
			{
				authenticationType = authenticationType | AuthenticationTypes.Signing;
			}
			if ((options & ContextOptions.Sealing) != 0)
			{
				authenticationType = authenticationType | AuthenticationTypes.Sealing;
			}
			return authenticationType;
		}

		[SecurityCritical]
		internal static void MoveDirectoryEntry(DirectoryEntry deToMove, DirectoryEntry newParent, string newName)
		{
			if (newName == null)
			{
				deToMove.MoveTo(newParent);
				return;
			}
			else
			{
				deToMove.MoveTo(newParent, newName);
				return;
			}
		}

		[SecurityCritical]
		internal static void MultiScalarFromDirectoryEntry<T>(dSPropertyCollection properties, string suggestedProperty, Principal p, string propertyName)
		{
			dSPropertyValueCollection item = properties[suggestedProperty];
			List<T> ts = new List<T>();
			foreach (object obj in item)
			{
				ts.Add((T)obj);
			}
			p.LoadValueIntoProperty(propertyName, ts);
		}

		[SecurityCritical]
		internal static void MultiStringToDirectoryEntryConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedProperty)
		{
			PrincipalValueCollection<string> valueForProperty = (PrincipalValueCollection<string>)p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty != null)
			{
				List<string> inserted = valueForProperty.Inserted;
				List<string> removed = valueForProperty.Removed;
				List<Pair<string, string>> changedValues = valueForProperty.ChangedValues;
				PropertyValueCollection item = de.Properties[suggestedProperty];
				foreach (string str in removed)
				{
					if (str == null || !item.Contains(str))
					{
						continue;
					}
					item.Remove(str);
				}
				foreach (Pair<string, string> changedValue in changedValues)
				{
					item.Remove(changedValue.Left);
					if (changedValue.Right == null || item.Contains(changedValue.Right))
					{
						continue;
					}
					item.Add(changedValue.Right);
				}
				foreach (string str1 in inserted)
				{
					if (str1 == null || item.Contains(str1))
					{
						continue;
					}
					item.Add(str1);
				}
				return;
			}
			else
			{
				return;
			}
		}

		[SecurityCritical]
		internal static Principal SearchResultToPrincipal(SearchResult sr, PrincipalContext owningContext, Type principalType)
		{
			Principal principal;
			if (typeof(UserPrincipal) != principalType)
			{
				if (typeof(ComputerPrincipal) != principalType)
				{
					if (typeof(GroupPrincipal) != principalType)
					{
						if (null == principalType || typeof(AuthenticablePrincipal) == principalType || typeof(Principal) == principalType)
						{
							if (!SDSUtils.IsOfObjectClass(sr, "computer"))
							{
								if (!SDSUtils.IsOfObjectClass(sr, "user"))
								{
									if (!SDSUtils.IsOfObjectClass(sr, "group"))
									{
										principal = AuthenticablePrincipal.MakeAuthenticablePrincipal(owningContext);
									}
									else
									{
										principal = GroupPrincipal.MakeGroup(owningContext);
									}
								}
								else
								{
									principal = UserPrincipal.MakeUser(owningContext);
								}
							}
							else
							{
								principal = ComputerPrincipal.MakeComputer(owningContext);
							}
						}
						else
						{
							principal = Principal.MakePrincipal(owningContext, principalType);
						}
					}
					else
					{
						principal = GroupPrincipal.MakeGroup(owningContext);
					}
				}
				else
				{
					principal = ComputerPrincipal.MakeComputer(owningContext);
				}
			}
			else
			{
				principal = UserPrincipal.MakeUser(owningContext);
			}
			principal.UnderlyingSearchObject = sr;
			return principal;
		}

		[SecurityCritical]
		internal static void SetPassword(DirectoryEntry de, string newPassword)
		{
			try
			{
				object[] objArray = new object[1];
				objArray[0] = newPassword;
				de.Invoke("SetPassword", objArray);
			}
			catch (TargetInvocationException targetInvocationException1)
			{
				TargetInvocationException targetInvocationException = targetInvocationException1;
				if (targetInvocationException.InnerException as COMException == null)
				{
					throw;
				}
				else
				{
					if (((COMException)targetInvocationException.InnerException).ErrorCode != ExceptionHelper.ERROR_HRESULT_CONSTRAINT_VIOLATION)
					{
						throw ExceptionHelper.GetExceptionFromCOMException((COMException)targetInvocationException.InnerException);
					}
					else
					{
						throw new PasswordException(((COMException)targetInvocationException.InnerException).Message, (COMException)targetInvocationException.InnerException);
					}
				}
			}
		}

		[SecurityCritical]
		internal static void SingleScalarFromDirectoryEntry<T>(dSPropertyCollection properties, string suggestedProperty, Principal p, string propertyName)
		{
			if (properties[suggestedProperty].Count != 0 && properties[suggestedProperty][0] != null)
			{
				p.LoadValueIntoProperty(propertyName, (T)properties[suggestedProperty][0]);
			}
		}

		internal static bool StatusFromAccountControl(int uacValue, string propertyName)
		{
			bool flag = false;
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "AuthenticablePrincipal.Enabled")
				{
					flag = (uacValue & 2) == 0;
					return flag;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired")
				{
					flag = (uacValue & 0x40000) != 0;
					return flag;
				}
				else if (str1 == "AuthenticablePrincipal.AccountInfo.DelegationPermitted")
				{
					flag = (uacValue & 0x100000) == 0;
					return flag;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired")
				{
					flag = (uacValue & 32) != 0;
					return flag;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires")
				{
					flag = (uacValue & 0x10000) != 0;
					return flag;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword")
				{
					flag = (uacValue & 64) != 0;
					return flag;
				}
				else if (str1 == "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption")
				{
					flag = (uacValue & 128) != 0;
					return flag;
				}
				flag = false;
				return flag;
			}
			else
			{
				flag = false;
				return flag;
			}
			return flag;
		}

		[SecurityCritical]
		internal static void WriteAttribute<T>(string dePath, string attribute, T value, NetCred credentials, AuthenticationTypes authTypes)
		{
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					directoryEntry = SDSUtils.BuildDirectoryEntry(dePath, credentials, authTypes);
					string[] strArrays = new string[1];
					strArrays[0] = attribute;
					directoryEntry.RefreshCache(strArrays);
					directoryEntry.Properties[attribute].Value = value;
					directoryEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
			}
		}

		[SecurityCritical]
		internal static void WriteAttribute(string dePath, string attribute, int value, NetCred credentials, AuthenticationTypes authTypes)
		{
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					directoryEntry = SDSUtils.BuildDirectoryEntry(dePath, credentials, authTypes);
					string[] strArrays = new string[1];
					strArrays[0] = attribute;
					directoryEntry.RefreshCache(strArrays);
					directoryEntry.Properties[attribute].Value = value;
					directoryEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
			}
		}

		internal delegate void GroupMembershipUpdater(Principal p, DirectoryEntry de, NetCred credentials, AuthenticationTypes authTypes);
	}
}