using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Security.AccessControl;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	[CmdletProvider("ActiveDirectory", ProviderCapabilities.Include | ProviderCapabilities.Exclude | ProviderCapabilities.Filter | ProviderCapabilities.ShouldProcess | ProviderCapabilities.Credentials)]
	public class ADProvider : NavigationCmdletProvider, IPropertyCmdletProvider, ISecurityDescriptorCmdletProvider, ICmdletProviderSupportsHelp
	{
		public const string ProviderName = "ActiveDirectory";

		public const string DefaultDriveName = "AD";

		private const string ItemTypeDirectory = "directory";

		private const string ADPSLoadDefaultDriveVariable = "ADPS_LoadDefaultDrive";

		private const string AnnotationProviderCanonicalDefaultPropertySet = "#ProviderCanonicalDefaultPropertySet";

		private const string AnnotationProviderX500DefaultPropertySet = "#ProviderX500DefaultPropertySet";

		private string _debugCategory;

		internal ADDriveInfo ExtendedDriveInfo
		{
			get
			{
				ADDriveInfo pSDriveInfo = base.PSDriveInfo as ADDriveInfo;
				if (pSDriveInfo == null)
				{
					pSDriveInfo = base.SessionState.Drive.Current as ADDriveInfo;
				}
				return pSDriveInfo;
			}
		}

		public ADProvider()
		{
			this._debugCategory = "ADProvider";
		}

		private static string AddAbsolutePathPrefix(string path)
		{
			return string.Concat("//RootDSE/", path);
		}

		private ADObjectSearcher BuildADObjectSearcher(ADSessionInfo sessionInfo)
		{
			ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(sessionInfo);
			aDObjectSearcher.PageSize = ADProviderDefaults.InternalProviderSearchPageSize;
			return aDObjectSearcher;
		}

		protected override void ClearItem(string path)
		{
			base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderClearItemNotSupported), "ADProvider:SetItem:ClearItem", path));
		}

		public void ClearProperty(string path, Collection<string> propertyToClear)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering ClearProperty");
			PSObject pSObject = new PSObject();
			if (!this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				if (propertyToClear != null)
				{
					if (base.ShouldProcess(path, "Clear"))
					{
						ADProviderCommonParameters dynamicParameters = base.DynamicParameters as ADProviderCommonParameters;
						ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
						if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
						{
							ADObject aDObject = new ADObject();
							aDObject.DistinguishedName = path;
							ADActiveObject aDActiveObject = new ADActiveObject(sessionInfo, aDObject);
							foreach (string str in propertyToClear)
							{
								aDObject.Add(str, null);
							}
							using (aDActiveObject)
							{
								try
								{
									aDActiveObject.Update();
									this.Trace(DebugLogLevel.Verbose, "ClearProperty: ADObject updated in store.");
									aDObject = this.GetValidatedADObject(path, propertyToClear, dynamicParameters, base.Credential, this.ExtendedDriveInfo);
								}
								catch (ADException aDException1)
								{
									ADException aDException = aDException1;
									base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:ClearProperty:ADError", path));
									this.Trace(DebugLogLevel.Error, "Leaving ClearProperty: ADException: AD error");
									return;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									base.WriteError(ADUtilities.GetErrorRecord(unauthorizedAccessException, "ADProvider:ClearProperty:AccessDenied", path));
									this.Trace(DebugLogLevel.Error, "Leaving ClearProperty: ADException: access denied");
									return;
								}
								catch (AuthenticationException authenticationException1)
								{
									AuthenticationException authenticationException = authenticationException1;
									base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:ClearProperty:InvalidCredentials", path));
									this.Trace(DebugLogLevel.Error, "Leaving ClearProperty: ADException: invalid credentials");
									return;
								}
								catch (InvalidOperationException invalidOperationException1)
								{
									InvalidOperationException invalidOperationException = invalidOperationException1;
									base.WriteError(ADUtilities.GetErrorRecord(invalidOperationException, "ADProvider:ClearProperty:InvalidOperation", path));
									this.Trace(DebugLogLevel.Error, "Leaving ClearProperty: ADException: invalid operation");
									return;
								}
								foreach (string str1 in propertyToClear)
								{
									if (!aDObject.Contains(str1))
									{
										continue;
									}
									pSObject.Properties.Add(new PSNoteProperty(str1, aDObject[str1].Value));
								}
								this.WriteADObjectProperties(aDObject, pSObject, dynamicParameters, this.ExtendedDriveInfo);
								this.Trace(DebugLogLevel.Verbose, "Leaving ClearProperty");
								return;
							}
							return;
						}
						else
						{
							this.Trace(DebugLogLevel.Error, "Leaving ClearProperty: ValidateDynamicParameters: returned false");
							return;
						}
					}
					else
					{
						this.Trace(DebugLogLevel.Info, "Leaving ClearProperty: ShouldProcess returned false.");
						return;
					}
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(StringResources.ADProviderPropertiesToClearNotSpecified), "ADProvider:ClearProperty:propertyToClear", path));
					this.Trace(DebugLogLevel.Error, "Leaving ClearProperty: ArgumentException:  propertyToClear is clear");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
				this.Trace(DebugLogLevel.Error, "Leaving ClearProperty: NotSupportedException: path is rootdse");
				return;
			}
		}

		public object ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
		{
			return new ADProviderCommonParameters();
		}

		private string ConvertToX500Path(string path, ADSessionInfo sessionInfo)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering ConvertToX500Path");
			path = ADPathModule.ConvertPath(sessionInfo, path, ADPathFormat.Canonical, ADPathFormat.X500);
			this.Trace(DebugLogLevel.Verbose, "Leaving ConvertToX500Path");
			return path;
		}

		protected override void CopyItem(string path, string destinationPath, bool recurse)
		{
			base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderCopyItemNotSupported), "ADProvider:CopyItem:NotSupported", path));
		}

		private ADObjectSearcher GetADObjectSearcher(string path, ADSearchScope scope, IADOPathNode filter, ADProviderSearchParameters parameters, PSCredential credential, ADDriveInfo extendedDriveInfo)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering GetADObjectSearcher");
			ADSessionInfo sessionInfo = this.GetSessionInfo(parameters, credential, extendedDriveInfo);
			ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(sessionInfo);
			aDObjectSearcher.SearchRoot = path;
			aDObjectSearcher.Scope = scope;
			if (filter != null)
			{
				aDObjectSearcher.Filter = filter;
			}
			if (parameters != null)
			{
				if (parameters.Properties != null)
				{
					aDObjectSearcher.Properties.AddRange(parameters.Properties);
				}
				aDObjectSearcher.PageSize = parameters.PageSize;
			}
			if (this.GetFormatType(parameters, extendedDriveInfo) == ADPathFormat.Canonical)
			{
				aDObjectSearcher.Properties.Add("canonicalName");
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetADObjectSearcher");
			return aDObjectSearcher;
		}

		private ADObject GetADObjectSecurityDescriptor(string path, AccessControlSections includeSections, ADProviderCommonParameters parameters, PSCredential credential, ADDriveInfo driveInfo)
		{
			ADObject aDObject;
			this.Trace(DebugLogLevel.Verbose, "Entering GetADObjectSecuritDescriptor");
			this.Trace(DebugLogLevel.Info, string.Format("GetADObjectSecuritDescriptor: path = {0}", path));
			if (driveInfo == null)
			{
				this.Trace(DebugLogLevel.Verbose, "GetADObjectSecuritDescriptor: ExtendedDriveInfo is null");
			}
			ADSessionInfo sessionInfo = this.GetSessionInfo(parameters, credential, driveInfo);
			ADObjectSearcher securityMasks = this.BuildADObjectSearcher(sessionInfo);
			using (securityMasks)
			{
				securityMasks.SearchRoot = path;
				securityMasks.Scope = ADSearchScope.Base;
				securityMasks.Properties.Add("ntSecurityDescriptor");
				securityMasks.SecurityDescriptorFlags = this.GetSecurityMasks(includeSections);
				if (this.GetFormatType(parameters, driveInfo) == ADPathFormat.Canonical)
				{
					securityMasks.Properties.Add("canonicalName");
				}
				aDObject = securityMasks.FindOne();
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetADObjectSecuritDescriptor");
			return aDObject;
		}

		private HashSet<ADObject> GetAllHostedNamingContexts(ADSessionInfo sessionInfo, ADRootDSE rootDSE, ADDriveInfo extendedDriveInfo, Collection<string> propertiesToRetrieve)
		{
			HashSet<ADObject> aDObjects = new HashSet<ADObject>();
			this.Trace(DebugLogLevel.Verbose, "Entering GetAllHostedNamingContexts");
			if (!this.IsServerGlobalCatalog(sessionInfo, rootDSE, base.DynamicParameters as ADProviderCommonParameters, extendedDriveInfo))
			{
				string[] namingContexts = rootDSE.NamingContexts;
				for (int i = 0; i < (int)namingContexts.Length; i++)
				{
					string str = namingContexts[i];
					ADPathFormat formatType = this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, extendedDriveInfo);
					ADObject validatedADObject = this.GetValidatedADObject(sessionInfo, str, propertiesToRetrieve, formatType);
					aDObjects.Add(validatedADObject);
				}
			}
			else
			{
				this.Trace(DebugLogLevel.Verbose, "GetAllHostedNamingContexts: Server is GC and we are connected to GC port.");
				string str1 = "(|(objectcategory=domainDNS)(objectcategory=DMD)(objectcategory=configuration))";
				ADObjectSearcher aDObjectSearcher = this.GetADObjectSearcher("", ADSearchScope.Subtree, new LdapFilterADOPathNode(str1), null, base.Credential, extendedDriveInfo);
				using (aDObjectSearcher)
				{
					if (propertiesToRetrieve != null)
					{
						aDObjectSearcher.Properties.AddRange(propertiesToRetrieve);
					}
					foreach (ADObject aDObject in aDObjectSearcher.FindAll())
					{
						aDObjects.Add(aDObject);
					}
				}
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetAllHostedNamingContexts");
			return aDObjects;
		}

		private AuthType GetAuthType(ADAuthType adpsAuthType)
		{
			AuthType authType = AuthType.Negotiate;
			if (adpsAuthType != ADAuthType.Negotiate)
			{
				if (adpsAuthType == ADAuthType.Basic)
				{
					authType = AuthType.Basic;
				}
			}
			else
			{
				authType = AuthType.Negotiate;
			}
			return authType;
		}

		protected override void GetChildItems(string path, bool recurse)
		{
			ADSearchScope aDSearchScope;
			this.Trace(DebugLogLevel.Verbose, "Entering GetChildItems");
			if (!this.IsValidRootDSEPath(path))
			{
				ADProvider aDProvider = this;
				string str = path;
				if (recurse)
				{
					aDSearchScope = ADSearchScope.Subtree;
				}
				else
				{
					aDSearchScope = ADSearchScope.OneLevel;
				}
				aDProvider.GetChildItemsOrNames(str, aDSearchScope, false, ReturnContainers.ReturnMatchingContainers, this.GetFilter());
			}
			else
			{
				this.GetRootDSEChildItemsOrNames(false, recurse, ReturnContainers.ReturnMatchingContainers, this.GetFilter());
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetChildItems");
		}

		protected override object GetChildItemsDynamicParameters(string path, bool recurse)
		{
			return new ADProviderSearchParameters();
		}

		private void GetChildItemsOrNames(string path, ADSearchScope scope, bool namesOnly, ReturnContainers returnContainers, string filter)
		{
			IADOPathNode ldapFilterADOPathNode;
			int sizeLimit;
			this.Trace(DebugLogLevel.Verbose, "Entering GetChildItemsOrNames");
			path = this.ValidateAndNormalizePath(path);
			this.Trace(DebugLogLevel.Info, string.Format("GetChildItemsOrNames: path = {0}", path));
			this.Trace(DebugLogLevel.Info, string.Format("GetChildItemsOrNames: scope = {0}", scope));
			this.Trace(DebugLogLevel.Info, string.Format("GetChildItemsOrNames: namesOnly = {0}", namesOnly));
			this.Trace(DebugLogLevel.Info, string.Format("GetChildItemsOrNames: returnContainers = {0}", returnContainers));
			this.Trace(DebugLogLevel.Info, string.Format("GetChildItemsOrNames: filter = {0}", filter));
			ADProviderSearchParameters dynamicParameters = base.DynamicParameters as ADProviderSearchParameters;
			ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
			if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
			{
				ADPathFormat formatType = this.GetFormatType(dynamicParameters, this.ExtendedDriveInfo);
				ADProvider aDProvider = this;
				string str = path;
				ADSearchScope aDSearchScope = scope;
				if (filter != null)
				{
					ldapFilterADOPathNode = new LdapFilterADOPathNode(filter);
				}
				else
				{
					ldapFilterADOPathNode = null;
				}
				ADObjectSearcher aDObjectSearcher = aDProvider.GetADObjectSearcher(str, aDSearchScope, ldapFilterADOPathNode, dynamicParameters, base.Credential, this.ExtendedDriveInfo);
				using (aDObjectSearcher)
				{
					try
					{
						int num = 0;
						if (dynamicParameters != null)
						{
							sizeLimit = dynamicParameters.SizeLimit;
						}
						else
						{
							sizeLimit = 0;
						}
						int num1 = sizeLimit;
						sessionInfo.ServerType = this.GetServerType(sessionInfo);
						IEnumerator<ADObject> enumerator = aDObjectSearcher.FindAll().GetEnumerator();
						using (enumerator)
						{
							do
							{
							Label1:
								if (!enumerator.MoveNext())
								{
									break;
								}
								ADObject current = enumerator.Current;
								if (formatType != ADPathFormat.Canonical || current.Contains("canonicalName"))
								{
									if (!namesOnly)
									{
										this.WriteADObject(current, sessionInfo, dynamicParameters, this.ExtendedDriveInfo);
									}
									else
									{
										this.WriteADObjectName(current, dynamicParameters, this.ExtendedDriveInfo);
									}
									num++;
								}
								else
								{
									this.Trace(DebugLogLevel.Warning, string.Format("GetChildItemsOrNames: Unable to read canonical name for object {0}, skipping..", current.DistinguishedName));
									goto Label1;
								}
							}
							while (num != num1);
						}
					}
					catch (ADException aDException1)
					{
						ADException aDException = aDException1;
						base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:GetChildItemsOrNames:ADError", path));
						this.Trace(DebugLogLevel.Error, "Leaving GetChildItemsOrNames: ADException: AD error");
						return;
					}
					catch (AuthenticationException authenticationException1)
					{
						AuthenticationException authenticationException = authenticationException1;
						base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:GetChildItemsOrNames:InvalidCredentials", path));
						this.Trace(DebugLogLevel.Error, "Leaving GetChildItemsOrNames: ADException: invalid credentials");
						return;
					}
				}
				this.Trace(DebugLogLevel.Verbose, "Leaving GetChildItemsOrNames");
				return;
			}
			else
			{
				this.Trace(DebugLogLevel.Verbose, "Leaving GetChildItemsOrNames : ValidateDynamicParameters returned false");
				return;
			}
		}

		protected override string GetChildName(string path)
		{
			string childName;
			this.Trace(DebugLogLevel.Verbose, "Entering GetChildName");
			this.Trace(DebugLogLevel.Info, string.Format("GetChildName: path = {0}", path));
			bool flag = ADProvider.PathIsAbsolute(path);
			path = ADProvider.RemoveAbsolutePathPrefix(path);
			if (this.ExtendedDriveInfo == null)
			{
				this.Trace(DebugLogLevel.Verbose, "GetChildName: ExtendedDriveInfo is null");
			}
			if (!flag || !this.IsNamingContext(path))
			{
				childName = ADPathModule.GetChildName(path, this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo));
			}
			else
			{
				childName = path;
			}
			this.Trace(DebugLogLevel.Info, string.Format("GetChildName: childName = {0}", childName));
			this.Trace(DebugLogLevel.Verbose, "Leaving GetChildName");
			return childName;
		}

		protected override void GetChildNames(string path, ReturnContainers returnContainers)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering GetChildNames");
			if (!this.IsValidRootDSEPath(path))
			{
				this.GetChildItemsOrNames(path, ADSearchScope.OneLevel, true, returnContainers, this.GetFilter());
			}
			else
			{
				this.GetRootDSEChildItemsOrNames(true, false, returnContainers, this.GetFilter());
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetChildNames");
		}

		protected override object GetChildNamesDynamicParameters(string path)
		{
			return new ADProviderSearchParameters();
		}

		private string GetContainingPartition(string path, ADProviderCommonParameters parameters, PSCredential credential, ADDriveInfo driveInfo)
		{
			bool flag = false;
			string parentPath = path;
			this.Trace(DebugLogLevel.Verbose, "Entering GetContainingPartition");
			this.Trace(DebugLogLevel.Info, string.Format("GetContainingPartition: path = {0}", path));
			if (driveInfo == null)
			{
				this.Trace(DebugLogLevel.Verbose, "GetContainingPartition: ExtendedDriveInfo is null");
			}
			ADSessionInfo sessionInfo = this.GetSessionInfo(parameters, credential, driveInfo);
			ADObjectSearcher aDObjectSearcher = this.BuildADObjectSearcher(sessionInfo);
			using (aDObjectSearcher)
			{
				aDObjectSearcher.Scope = ADSearchScope.Base;
				aDObjectSearcher.Properties.Clear();
				aDObjectSearcher.Properties.Add("instanceType");
				while (!flag && parentPath != "")
				{
					aDObjectSearcher.SearchRoot = parentPath;
					ADObject aDObject = aDObjectSearcher.FindOne();
					if (aDObject == null)
					{
						this.Trace(DebugLogLevel.Error, string.Format("Leaving GetContainingPartition: ADException: instanceType not set for {0}.", parentPath));
						object[] objArray = new object[1];
						objArray[0] = path;
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ADProviderUnableToGetPartition, objArray));
					}
					else
					{
						if (((int)aDObject["instanceType"].Value & 1) != 1)
						{
							parentPath = ADPathModule.GetParentPath(parentPath, "", ADPathFormat.X500);
						}
						else
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					this.Trace(DebugLogLevel.Error, string.Format("Leaving GetContainingPartition: ADException: could not find partition containing {0}.", path));
					object[] objArray1 = new object[1];
					objArray1[0] = path;
					throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ADProviderUnableToGetPartition, objArray1));
				}
			}
			this.Trace(DebugLogLevel.Info, string.Format("GetContainingPartition: partition path = {0}", parentPath));
			this.Trace(DebugLogLevel.Verbose, "Leaving GetContainingPartition");
			return parentPath;
		}

		private PSCredential GetCredential(PSCredential credential, ADDriveInfo extendedDriveInfo)
		{
			PSCredential pSCredential = null;
			if (credential == null || credential.UserName == null || !(credential.UserName != ""))
			{
				if (extendedDriveInfo != null)
				{
					PSCredential pSCredential1 = extendedDriveInfo.Credential;
					if (pSCredential1 != null && pSCredential1.UserName != null && pSCredential1.UserName != "")
					{
						pSCredential = pSCredential1;
					}
				}
			}
			else
			{
				pSCredential = credential;
			}
			return pSCredential;
		}

		private string GetFilter()
		{
			string filter;
			this.Trace(DebugLogLevel.Verbose, "Entering GetFilter");
			if (base.Filter != null)
			{
				filter = base.Filter;
			}
			else
			{
				filter = ADProviderDefaults.SearchFilter;
			}
			string str = filter;
			string str1 = "";
			if (base.Include != null)
			{
				foreach (string include in base.Include)
				{
					str1 = string.Concat(str1, "(name=", include, ")");
				}
			}
			if (base.Exclude != null)
			{
				foreach (string exclude in base.Exclude)
				{
					str1 = string.Concat(str1, "(!name=", exclude, ")");
				}
			}
			if (str1 != "")
			{
				string[] strArrays = new string[5];
				strArrays[0] = "(&";
				strArrays[1] = str1;
				strArrays[2] = "(";
				strArrays[3] = str;
				strArrays[4] = "))";
				str = string.Concat(strArrays);
			}
			this.Trace(DebugLogLevel.Info, string.Format("GetFilter: name filter = {0}", str1));
			this.Trace(DebugLogLevel.Info, string.Format("GetFilter: Ldap filter = {0}", str));
			this.Trace(DebugLogLevel.Verbose, "Leaving GetFilter");
			return str;
		}

		private ADPathFormat GetFormatType(ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			ADPathFormat pathFormat = ADProviderDefaults.PathFormat;
			if (parameters == null || !parameters.IsPropertySet("FormatType"))
			{
				if (extendedDriveInfo != null)
				{
					pathFormat = extendedDriveInfo.FormatType;
				}
			}
			else
			{
				pathFormat = parameters.FormatType;
			}
			return pathFormat;
		}

		private ADPathHostType GetHostType(ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			ADPathHostType hostType = ADProviderDefaults.HostType;
			if (parameters == null || !parameters.IsPropertySet("GlobalCatalog") || !parameters.GlobalCatalog)
			{
				if (extendedDriveInfo != null && extendedDriveInfo.GlobalCatalog)
				{
					hostType = ADPathHostType.GC;
				}
			}
			else
			{
				hostType = ADPathHostType.GC;
			}
			return hostType;
		}

		protected override void GetItem(string path)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering GetItems");
			if (!this.IsValidRootDSEPath(path))
			{
				this.GetChildItemsOrNames(path, ADSearchScope.Base, false, ReturnContainers.ReturnMatchingContainers, ADProviderDefaults.SearchFilter);
			}
			else
			{
				ADProviderSearchParameters dynamicParameters = base.DynamicParameters as ADProviderSearchParameters;
				Collection<string> strs = new Collection<string>();
				strs.Add("*");
				if (dynamicParameters.Properties != null)
				{
					for (int i = 0; i < (int)dynamicParameters.Properties.Length; i++)
					{
						strs.Add(dynamicParameters.Properties[i]);
					}
				}
				ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
				ADRootDSE rootDSE = this.GetRootDSE(sessionInfo, strs);
				this.WriteADRootDSE(rootDSE, sessionInfo);
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetItems");
		}

		protected override object GetItemDynamicParameters(string path)
		{
			return new ADProviderSearchParameters();
		}

		protected override string GetParentPath(string path, string root)
		{
			string parentPath;
			this.Trace(DebugLogLevel.Verbose, "Entering GetParentPath");
			this.Trace(DebugLogLevel.Info, string.Format("GetParentPath: path = {0}", path));
			this.Trace(DebugLogLevel.Info, string.Format("GetParentPath: root = {0}", root));
			if (this.ExtendedDriveInfo == null)
			{
				this.Trace(DebugLogLevel.Verbose, "GetParentPath: ExtendedDriveInfo is null");
			}
			if (root == null && this.ExtendedDriveInfo != null)
			{
				root = this.ExtendedDriveInfo.Root;
			}
			bool flag = ADProvider.PathIsAbsolute(path);
			path = ADProvider.RemoveAbsolutePathPrefix(path);
			if (!flag || !ADPathModule.ComparePath(path, ADProvider.RemoveAbsolutePathPrefix(root), this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo)))
			{
				if (!this.IsNamingContext(path))
				{
					parentPath = ADPathModule.GetParentPath(path, "", this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo));
					if (flag)
					{
						parentPath = ADProvider.AddAbsolutePathPrefix(parentPath);
					}
				}
				else
				{
					parentPath = "";
					this.Trace(DebugLogLevel.Verbose, string.Format("GetParentPath: Path is a naming context: {0}", path));
					if (flag)
					{
						parentPath = ADProvider.AddAbsolutePathPrefix(parentPath);
					}
				}
			}
			else
			{
				parentPath = "";
			}
			this.Trace(DebugLogLevel.Info, string.Format("GetParentPath: parentPath = {0}", parentPath));
			this.Trace(DebugLogLevel.Verbose, "Leaving GetParentPath");
			return parentPath;
		}

		public void GetProperty(string path, Collection<string> providerSpecificPickList)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering GetProperty");
			PSObject pSObject = new PSObject();
			ADObject validatedADObject = null;
			if (providerSpecificPickList != null)
			{
				if (!this.IsValidRootDSEPath(path))
				{
					path = this.ValidateAndNormalizePath(path);
					ADProviderCommonParameters dynamicParameters = base.DynamicParameters as ADProviderCommonParameters;
					ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
					if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
					{
						try
						{
							validatedADObject = this.GetValidatedADObject(path, providerSpecificPickList, dynamicParameters, base.Credential, this.ExtendedDriveInfo);
						}
						catch (ADException aDException1)
						{
							ADException aDException = aDException1;
							base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:GetProperty:ADError", path));
							this.Trace(DebugLogLevel.Error, "Leaving GetProperty: ADException: AD error");
							return;
						}
						catch (AuthenticationException authenticationException1)
						{
							AuthenticationException authenticationException = authenticationException1;
							base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:GetProperty:InvalidCredentials", path));
							this.Trace(DebugLogLevel.Error, "Leaving GetProperty: ADException: invalid credentials");
							return;
						}
						foreach (string str in providerSpecificPickList)
						{
							if (!validatedADObject.Contains(str))
							{
								continue;
							}
							pSObject.Properties.Add(new PSNoteProperty(str, validatedADObject[str].Value));
						}
						this.WriteADObjectProperties(validatedADObject, pSObject, dynamicParameters, this.ExtendedDriveInfo);
						this.Trace(DebugLogLevel.Verbose, "Leaving GetProperty");
						return;
					}
					else
					{
						this.Trace(DebugLogLevel.Error, "Leaving GetProperty: ValidateDynamicParameters: returned false");
						return;
					}
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
					this.Trace(DebugLogLevel.Error, "Leaving GetProperty: NotSupportedException: path is rootdse");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(StringResources.ADProviderPropertiesNotSpecified), "ADProvider:GetProperty:InvalidArgument", path));
				this.Trace(DebugLogLevel.Error, "Leaving GetProperty: ArgumentException: providerSpecificPickList is null");
				return;
			}
		}

		public object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
		{
			return new ADProviderCommonParameters();
		}

		private ADRootDSE GetRootDSE(ADSessionInfo sessionInfo, ICollection<string> propertiesToRetrieve)
		{
			ADRootDSE rootDSE;
			this.Trace(DebugLogLevel.Verbose, "Entering GetRootDSE");
			ADObjectSearcher aDObjectSearcher = this.BuildADObjectSearcher(sessionInfo);
			using (aDObjectSearcher)
			{
				rootDSE = aDObjectSearcher.GetRootDSE(propertiesToRetrieve);
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetRootDSE");
			return rootDSE;
		}

		private ADRootDSE GetRootDSE(ADSessionInfo sessionInfo)
		{
			return this.GetRootDSE(sessionInfo, null);
		}

		private void GetRootDSEChildItemsOrNames(bool namesOnly, bool recurse, ReturnContainers returnContainers, string filter)
		{
			Collection<string> strs;
			int sizeLimit;
			IADOPathNode ldapFilterADOPathNode;
			this.Trace(DebugLogLevel.Verbose, "Entering GetRootDSEChildItemsOrNames");
			this.Trace(DebugLogLevel.Info, string.Format("GetRootDSEChildItemsOrNames: namesOnly = {0}", namesOnly));
			this.Trace(DebugLogLevel.Info, string.Format("GetRootDSEChildItemsOrNames: recurse = {0}", recurse));
			this.Trace(DebugLogLevel.Info, string.Format("GetRootDSEChildItemsOrNames: returnContainers = {0}", returnContainers));
			this.Trace(DebugLogLevel.Info, string.Format("GetRootDSEChildItemsOrNames: filter = {0}", filter));
			ADProviderSearchParameters dynamicParameters = base.DynamicParameters as ADProviderSearchParameters;
			ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
			if (this.ValidateDynamicParameters("", dynamicParameters, sessionInfo))
			{
				ADObjectSearcher aDObjectSearcher = null;
				ADPathFormat formatType = this.GetFormatType(dynamicParameters, this.ExtendedDriveInfo);
				using (aDObjectSearcher)
				{
					try
					{
						int num = 0;
						if (dynamicParameters != null)
						{
							sizeLimit = dynamicParameters.SizeLimit;
						}
						else
						{
							sizeLimit = 0;
						}
						int num1 = sizeLimit;
						ADRootDSE rootDSE = this.GetRootDSE(sessionInfo);
						sessionInfo.ServerType = this.GetServerType(rootDSE);
						if (!recurse)
						{
							if (dynamicParameters == null || dynamicParameters.Properties == null)
							{
								strs = new Collection<string>();
							}
							else
							{
								strs = new Collection<string>(dynamicParameters.Properties);
							}
							HashSet<ADObject> allHostedNamingContexts = this.GetAllHostedNamingContexts(sessionInfo, rootDSE, this.ExtendedDriveInfo, strs);
							this.Trace(DebugLogLevel.Verbose, "GetRootDSEChildItemsOrNames: Clearing rootdse child cache.");
							this.ExtendedDriveInfo.NamingContexts.Clear();
							HashSet<ADObject>.Enumerator enumerator = allHostedNamingContexts.GetEnumerator();
							try
							{
								do
								{
								Label2:
									if (!enumerator.MoveNext())
									{
										break;
									}
									ADObject current = enumerator.Current;
									string distinguishedName = null;
									if (formatType != ADPathFormat.Canonical)
									{
										distinguishedName = current.DistinguishedName;
									}
									else
									{
										if (!current.Contains("canonicalName"))
										{
											this.Trace(DebugLogLevel.Warning, string.Format("NewDrive: Unable to read canonical name for naming context {0}, skipping..", current.DistinguishedName));
											goto Label2;
										}
										else
										{
											distinguishedName = (string)current["canonicalName"].Value;
										}
									}
									this.ExtendedDriveInfo.NamingContexts.Add(distinguishedName);
									this.Trace(DebugLogLevel.Verbose, string.Format("GetRootDSEChildItemsOrNames: Adding path to rootdse child cache: {0}", distinguishedName));
									if (!namesOnly)
									{
										this.WriteADObject(current, sessionInfo, dynamicParameters, this.ExtendedDriveInfo);
									}
									else
									{
										this.WriteADRootDSEChildName(current, dynamicParameters, this.ExtendedDriveInfo);
									}
									num++;
								}
								while (num != num1);
							}
							finally
							{
								enumerator.Dispose();
							}
						}
						else
						{
							if (this.IsServerGlobalCatalog(sessionInfo, rootDSE, dynamicParameters, this.ExtendedDriveInfo))
							{
								ADProvider aDProvider = this;
								string str = "";
								int num2 = 2;
								if (filter != null)
								{
									ldapFilterADOPathNode = new LdapFilterADOPathNode(filter);
								}
								else
								{
									ldapFilterADOPathNode = null;
								}
								aDObjectSearcher = aDProvider.GetADObjectSearcher(str, (ADSearchScope)num2, ldapFilterADOPathNode, dynamicParameters, base.Credential, this.ExtendedDriveInfo);
								IEnumerator<ADObject> enumerator1 = aDObjectSearcher.FindAll().GetEnumerator();
								using (enumerator1)
								{
									do
									{
									Label3:
										if (!enumerator1.MoveNext())
										{
											break;
										}
										ADObject aDObject = enumerator1.Current;
										if (formatType != ADPathFormat.Canonical || aDObject.Contains("canonicalName"))
										{
											if (!namesOnly)
											{
												this.WriteADObject(aDObject, sessionInfo, dynamicParameters, this.ExtendedDriveInfo);
											}
											else
											{
												this.WriteADRootDSEChildName(aDObject, dynamicParameters, this.ExtendedDriveInfo);
											}
											num++;
										}
										else
										{
											this.Trace(DebugLogLevel.Warning, string.Format("GetRootDSEChildItemsOrNames: Unable to read canonical name for object {0}, skipping..", aDObject.DistinguishedName));
											goto Label3;
										}
									}
									while (num != num1);
								}
							}
							else
							{
								base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSEUnlessGC), "NotSupported", ""));
								this.Trace(DebugLogLevel.Error, "Leaving MoveItem: NotSupportedException: recursive search requested on non-GC rootdse.");
								return;
							}
						}
					}
					catch (ADException aDException1)
					{
						ADException aDException = aDException1;
						base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:GetRootDSEChildItemsOrNames:ADError", ""));
						this.Trace(DebugLogLevel.Error, "Leaving GetRootDSEChildItemsOrNames: ADException: AD error");
						return;
					}
					catch (AuthenticationException authenticationException1)
					{
						AuthenticationException authenticationException = authenticationException1;
						base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:GetRootDSEChildItemsOrNames:InvalidCredentials", ""));
						this.Trace(DebugLogLevel.Error, "Leaving GetRootDSEChildItemsOrNames: ADException: invalid credentials");
						return;
					}
					this.Trace(DebugLogLevel.Verbose, "Leaving GetRootDSEChildItemsOrNames");
					return;
				}
				return;
			}
			else
			{
				this.Trace(DebugLogLevel.Verbose, "Leaving GetRootDSEChildItemsOrNames : ValidateDynamicParameters returned false");
				return;
			}
		}

		public void GetSecurityDescriptor(string path, AccessControlSections includeSections)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering GetSecurityDescriptor");
			if (!this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				try
				{
					ADObject aDObjectSecurityDescriptor = this.GetADObjectSecurityDescriptor(path, includeSections, null, base.Credential, this.ExtendedDriveInfo);
					if (aDObjectSecurityDescriptor.Contains("ntSecurityDescriptor"))
					{
						this.WriteADObjectSecurityDescriptor(aDObjectSecurityDescriptor, (ActiveDirectorySecurity)aDObjectSecurityDescriptor["ntSecurityDescriptor"].Value, null, this.ExtendedDriveInfo);
						this.Trace(DebugLogLevel.Verbose, "Leaving GetSecurityDescriptor");
						return;
					}
					else
					{
						base.WriteError(ADUtilities.GetErrorRecord(new ADException(StringResources.ADProviderSDNotSet), "ADProvider:GetSecurityDescriptor:NoSecurityDescriptor", path));
						this.Trace(DebugLogLevel.Error, "Leaving GetSecurityDescriptor: ntSecurityDescriptor property not set.");
					}
				}
				catch (ADException aDException1)
				{
					ADException aDException = aDException1;
					base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:GetSecurityDescriptor:ADError", path));
					this.Trace(DebugLogLevel.Error, "Leaving GetSecurityDescriptor: ADException: AD error");
				}
				catch (AuthenticationException authenticationException1)
				{
					AuthenticationException authenticationException = authenticationException1;
					base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:GetSecurityDescriptor:InvalidCredentials", path));
					this.Trace(DebugLogLevel.Error, "Leaving GetSecurityDescriptor: ADException: invalid credentials");
				}
				return;
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
				this.Trace(DebugLogLevel.Error, "Leaving GetSecurityDescriptor: NotSupportedException: path is rootdse");
				return;
			}
		}

		private System.DirectoryServices.Protocols.SecurityMasks GetSecurityMasks(AccessControlSections includeSections)
		{
			System.DirectoryServices.Protocols.SecurityMasks securityMask = System.DirectoryServices.Protocols.SecurityMasks.None;
			if ((includeSections & AccessControlSections.Owner) == AccessControlSections.Owner)
			{
				securityMask = securityMask | System.DirectoryServices.Protocols.SecurityMasks.Owner;
			}
			if ((includeSections & AccessControlSections.Group) == AccessControlSections.Group)
			{
				securityMask = securityMask | System.DirectoryServices.Protocols.SecurityMasks.Group;
			}
			if ((includeSections & AccessControlSections.Access) == AccessControlSections.Access)
			{
				securityMask = securityMask | System.DirectoryServices.Protocols.SecurityMasks.Dacl;
			}
			if ((includeSections & AccessControlSections.Audit) == AccessControlSections.Audit)
			{
				securityMask = securityMask | System.DirectoryServices.Protocols.SecurityMasks.Sacl;
			}
			return securityMask;
		}

		private string GetServer(ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			string server = null;
			if (parameters == null || parameters.Server == null)
			{
				if (extendedDriveInfo != null)
				{
					server = extendedDriveInfo.Server;
				}
			}
			else
			{
				server = parameters.Server;
			}
			return server;
		}

		private ADServerType GetServerType(ADSessionInfo sessionInfo)
		{
			return this.GetServerType(this.GetRootDSE(sessionInfo));
		}

		private ADServerType GetServerType(ADRootDSE rootDse)
		{
			return rootDse.ServerType;
		}

		private ADSessionInfo GetSessionInfo(ADProviderCommonParameters parameters, PSCredential credential, ADDriveInfo extendedDriveInfo)
		{
			ADAuthType authType;
			ADAuthType aDAuthType;
			this.Trace(DebugLogLevel.Verbose, "Entering GetSessionInfo");
			string server = ADProviderDefaults.Server;
			bool isGC = ADProviderDefaults.IsGC;
			ADAuthType authType1 = ADProviderDefaults.AuthType;
			if (parameters == null || !(extendedDriveInfo != null))
			{
				if (parameters == null)
				{
					if (extendedDriveInfo != null)
					{
						server = extendedDriveInfo.Server;
						isGC = extendedDriveInfo.GlobalCatalog;
						authType1 = extendedDriveInfo.AuthType;
					}
				}
				else
				{
					if (parameters.Server != null)
					{
						server = parameters.Server;
						if (parameters.IsPropertySet("GlobalCatalog"))
						{
							isGC = parameters.GlobalCatalog;
						}
					}
					if (parameters.IsPropertySet("AuthType"))
					{
						authType = parameters.AuthType;
					}
					else
					{
						authType = ADProviderDefaults.AuthType;
					}
					authType1 = authType;
				}
			}
			else
			{
				if (parameters.Server == null)
				{
					server = extendedDriveInfo.Server;
					isGC = extendedDriveInfo.GlobalCatalog;
				}
				else
				{
					server = parameters.Server;
					if (parameters.IsPropertySet("GlobalCatalog"))
					{
						isGC = parameters.GlobalCatalog;
					}
				}
				if (parameters.IsPropertySet("AuthType"))
				{
					aDAuthType = parameters.AuthType;
				}
				else
				{
					aDAuthType = extendedDriveInfo.AuthType;
				}
				authType1 = aDAuthType;
			}
			ADSessionInfo aDSessionInfo = new ADSessionInfo(server);
			if (isGC)
			{
				aDSessionInfo.SetDefaultPort(LdapConstants.LDAP_GC_PORT);
			}
			aDSessionInfo.Credential = this.GetCredential(credential, extendedDriveInfo);
			aDSessionInfo.AuthType = this.GetAuthType(authType1);
			this.Trace(DebugLogLevel.Verbose, "Leaving GetSessionInfo");
			return aDSessionInfo;
		}

		private ADObject GetValidatedADObject(string path, Collection<string> propertiesToRetrieve, ADProviderCommonParameters parameters, PSCredential credential, ADDriveInfo driveInfo)
		{
			return this.GetValidatedADObject(this.GetSessionInfo(parameters, credential, driveInfo), path, propertiesToRetrieve, this.GetFormatType(parameters, driveInfo));
		}

		private ADObject GetValidatedADObject(ADSessionInfo sessionInfo, string path, Collection<string> propertiesToRetrieve, ADPathFormat formatType)
		{
			ADObject aDObject;
			this.Trace(DebugLogLevel.Verbose, "Entering GetValidatedADObject");
			this.Trace(DebugLogLevel.Info, string.Format("GetValidatedADObject: path = {0}", path));
			ADObjectSearcher aDObjectSearcher = this.BuildADObjectSearcher(sessionInfo);
			using (aDObjectSearcher)
			{
				aDObjectSearcher.SearchRoot = path;
				aDObjectSearcher.Scope = ADSearchScope.Base;
				if (propertiesToRetrieve != null)
				{
					aDObjectSearcher.Properties.AddRange(propertiesToRetrieve);
				}
				if (formatType == ADPathFormat.Canonical)
				{
					aDObjectSearcher.Properties.Add("canonicalName");
				}
				aDObject = aDObjectSearcher.FindOne();
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving GetValidatedADObject");
			return aDObject;
		}

		protected override bool HasChildItems(string path)
		{
			bool flag;
			bool flag1;
			this.Trace(DebugLogLevel.Verbose, "Entering HasChildItems");
			if (!this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				ADSessionInfo sessionInfo = this.GetSessionInfo(base.DynamicParameters as ADProviderCommonParameters, base.Credential, this.ExtendedDriveInfo);
				ADObjectSearcher aDObjectSearcher = null;
				using (aDObjectSearcher)
				{
					aDObjectSearcher = this.BuildADObjectSearcher(sessionInfo);
					aDObjectSearcher.SearchRoot = path;
					aDObjectSearcher.Scope = ADSearchScope.OneLevel;
					ADObject aDObject = aDObjectSearcher.FindOne();
					if (aDObject != null)
					{
						flag1 = true;
					}
					else
					{
						flag1 = false;
					}
					flag = flag1;
				}
				this.Trace(DebugLogLevel.Verbose, "Leaving HasChildItems");
				return flag;
			}
			else
			{
				return true;
			}
		}

		protected override Collection<PSDriveInfo> InitializeDefaultDrives()
		{
			this.Trace(DebugLogLevel.Verbose, "Entering InitializeDefaultDrives");
			Collection<PSDriveInfo> pSDriveInfos = new Collection<PSDriveInfo>();
			try
			{
				bool flag = string.Equals(Environment.GetEnvironmentVariable("ADPS_LoadDefaultDrive"), "0", StringComparison.OrdinalIgnoreCase);
				if (!flag && OSHelper.IsWindows)
				{
					ADRootDSE rootDSE = this.GetRootDSE(null);
					PSDriveInfo pSDriveInfo = new PSDriveInfo("AD", base.ProviderInfo, "", null, null);
					pSDriveInfos.Add(pSDriveInfo);
					this.Trace(DebugLogLevel.Info, string.Format("InitializeDefaultDrives: Default drive initialized to DC {0}.", rootDSE.DNSHostName));
				}
				else
				{
					this.Trace(DebugLogLevel.Info, string.Format("InitializeDefaultDrives: environment variable {0} set to 0, skipping default drive initialization.", "ADPS_LoadDefaultDrive"));
				}
			}
			catch (ADException aDException1)
			{
				ADException aDException = aDException1;
				object[] message = new object[1];
				message[0] = aDException.Message;
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.ADProviderErrorInitializingDefaultDrive, message));
				this.Trace(DebugLogLevel.Error, string.Format("InitializeDefaultDrives:Ignoring exception while initializing default drive, exception = {0}", aDException));
			}
			catch (ADServerDownException aDServerDownException1)
			{
				ADServerDownException aDServerDownException = aDServerDownException1;
				object[] objArray = new object[1];
				objArray[0] = aDServerDownException.Message;
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.ADProviderErrorInitializingDefaultDrive, objArray));
				this.Trace(DebugLogLevel.Error, string.Format("InitializeDefaultDrives:Ignoring exception while initializing default drive, exception = {0}", aDServerDownException));
			}
			catch (AuthenticationException authenticationException1)
			{
				AuthenticationException authenticationException = authenticationException1;
				object[] message1 = new object[1];
				message1[0] = authenticationException.Message;
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.ADProviderErrorInitializingDefaultDrive, message1));
				this.Trace(DebugLogLevel.Error, string.Format("InitializeDefaultDrives:Ignoring exception while initializing default drive, exception = {0}", authenticationException));
			}
			catch (UnauthorizedAccessException unauthorizedAccessException1)
			{
				UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
				object[] objArray1 = new object[1];
				objArray1[0] = unauthorizedAccessException.Message;
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.ADProviderErrorInitializingDefaultDrive, objArray1));
				this.Trace(DebugLogLevel.Error, string.Format("InitializeDefaultDrives:Ignoring exception while initializing default drive, exception = {0}", unauthorizedAccessException));
			}
			catch (TimeoutException timeoutException1)
			{
				TimeoutException timeoutException = timeoutException1;
				object[] message2 = new object[1];
				message2[0] = timeoutException.Message;
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.ADProviderErrorInitializingDefaultDrive, message2));
				this.Trace(DebugLogLevel.Error, string.Format("InitializeDefaultDrives:Ignoring exception while initializing default drive, exception = {0}", timeoutException));
			}
			this.Trace(DebugLogLevel.Verbose, "Leaving InitializeDefaultDrives");
			return pSDriveInfos;
		}

		protected override void InvokeDefaultAction(string path)
		{
			base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderExecuteItemNotSupported), "ADProvider:SetItem:InvokeDefaultAction", path));
		}

		protected override bool IsItemContainer(string path)
		{
			return true;
		}

		private bool IsNamingContext(string path)
		{
			bool flag;
			if (this.ExtendedDriveInfo != null && ADProvider.RemoveAbsolutePathPrefix(this.ExtendedDriveInfo.Root) == "" && this.ExtendedDriveInfo.NamingContexts != null)
			{
				HashSet<string>.Enumerator enumerator = this.ExtendedDriveInfo.NamingContexts.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						if (!ADPathModule.ComparePath(ADProvider.RemoveAbsolutePathPrefix(path), current, this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo)))
						{
							continue;
						}
						flag = true;
						return flag;
					}
					return false;
				}
				finally
				{
					enumerator.Dispose();
				}
				return flag;
			}
			return false;
		}

		private bool IsServerGlobalCatalog(ADSessionInfo sessionInfo, ADRootDSE rootDSE, ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			bool hasValue;
			bool flag = false;
			if (this.GetHostType(parameters, extendedDriveInfo) != ADPathHostType.GC)
			{
				bool? globalCatalogReady = rootDSE.GlobalCatalogReady;
				if (!globalCatalogReady.GetValueOrDefault())
				{
					hasValue = false;
				}
				else
				{
					hasValue = globalCatalogReady.HasValue;
				}
				if (hasValue)
				{
					string server = null;
					if (sessionInfo.Server != null)
					{
						server = sessionInfo.Server;
					}
					if (server != null)
					{
						this.Trace(DebugLogLevel.Info, string.Format("IsServerGlobalCatalog: server = {0}", server));
						if (server.EndsWith(":3268") || server.EndsWith(":3269"))
						{
							flag = true;
						}
					}
				}
			}
			else
			{
				flag = true;
			}
			this.Trace(DebugLogLevel.Info, string.Format("IsServerGlobalCatalog: result = {0}", flag));
			return flag;
		}

		protected override bool IsValidPath(string path)
		{
			bool flag = false;
			this.Trace(DebugLogLevel.Verbose, "Entering IsValidPath");
			this.Trace(DebugLogLevel.Info, string.Format("IsValidPath: path = {0}", path));
			if (path != null)
			{
				path = ADProvider.RemoveAbsolutePathPrefix(path);
				if (this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo) == ADPathFormat.Canonical && !string.IsNullOrEmpty(path) && CanonicalPath.IndexOfFirstDelimiter(path) == -1)
				{
					path = string.Concat(path, "/");
				}
				flag = ADPathModule.IsValidPath(path, this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo));
			}
			this.Trace(DebugLogLevel.Info, string.Format("IsValidPath: result = {0}", flag));
			this.Trace(DebugLogLevel.Verbose, "Leaving IsValidPath");
			return flag;
		}

		private bool IsValidRootDSEPath(string path)
		{
			bool flag = false;
			this.Trace(DebugLogLevel.Verbose, "Entering IsValidRootDSEPath");
			path = ADProvider.RemoveAbsolutePathPrefix(path);
			if (path == "")
			{
				if (this.ExtendedDriveInfo == null)
				{
					flag = true;
				}
				else
				{
					flag = ADProvider.RemoveAbsolutePathPrefix(this.ExtendedDriveInfo.Root) == "";
				}
			}
			this.Trace(DebugLogLevel.Verbose, string.Format("Leaving IsValidRootDSEPath, result = {0}", flag));
			return flag;
		}

		protected override bool ItemExists(string path)
		{
			bool flag;
			bool flag1 = true;
			this.Trace(DebugLogLevel.Verbose, "Entering ItemExists");
			if (path == null)
			{
				flag1 = false;
			}
			if (!this.IsValidPath(path))
			{
				flag1 = false;
			}
			if (flag1 && !this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				try
				{
					ADObject validatedADObject = this.GetValidatedADObject(path, null, null, base.Credential, this.ExtendedDriveInfo);
					if (validatedADObject == null)
					{
						flag1 = false;
					}
					this.Trace(DebugLogLevel.Info, string.Format("ItemExists: result = {0}", (object)flag1));
					this.Trace(DebugLogLevel.Verbose, "Leaving ItemExists");
					return flag1;
				}
				catch (ADException aDException1)
				{
					ADException aDException = aDException1;
					base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:ItemExists::ADError", path));
					this.Trace(DebugLogLevel.Error, "Leaving ItemExists: ADException: AD error.");
					flag = false;
				}
				catch (AuthenticationException authenticationException1)
				{
					AuthenticationException authenticationException = authenticationException1;
					base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:ItemExists:InvalidCredentials", path));
					this.Trace(DebugLogLevel.Error, "Leaving ItemExists: AuthenticationException: Invalid credentials.");
					flag = false;
				}
				return flag;
			}
			this.Trace(DebugLogLevel.Info, string.Format("ItemExists: result = {0}", flag1));
			this.Trace(DebugLogLevel.Verbose, "Leaving ItemExists");
			return flag1;
		}

		protected override string MakePath(string parent, string child)
		{
			string parentPath;
			this.Trace(DebugLogLevel.Verbose, "Entering MakePath");
			this.Trace(DebugLogLevel.Info, string.Format("MakePath: parent = {0}", parent));
			this.Trace(DebugLogLevel.Info, string.Format("MakePath: child = {0}", child));
			string value = "";
			Regex regex = new Regex("^[^,/:\\\\]+::\\\\?");
			Regex regex1 = new Regex("^[^,/:\\\\]+:\\\\?");
			if (!regex.IsMatch(parent))
			{
				if (regex1.IsMatch(parent))
				{
					Match match = regex1.Match(parent);
					value = match.Value;
				}
			}
			else
			{
				Match match1 = regex.Match(parent);
				value = match1.Value;
			}
			parent = parent.Substring(value.Length);
			if (this.ExtendedDriveInfo == null)
			{
				this.Trace(DebugLogLevel.Verbose, "MakePath: ExtendedDriveInfo is null");
			}
			bool flag = ADProvider.PathIsAbsolute(parent);
			parent = ADProvider.RemoveAbsolutePathPrefix(parent);
			if ((parent == "." || parent == "..") && (child == "." || child == ".."))
			{
				parentPath = ADPathModule.MakePath(parent, child, this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo));
			}
			else
			{
				if (parent != ".")
				{
					if (child != ".")
					{
						if (parent != "..")
						{
							if (child != "..")
							{
								parentPath = ADPathModule.MakePath(parent, child, this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo));
							}
							else
							{
								parentPath = this.GetParentPath(parent, this.ExtendedDriveInfo.Root);
							}
						}
						else
						{
							parentPath = this.GetParentPath(child, this.ExtendedDriveInfo.Root);
						}
					}
					else
					{
						parentPath = parent;
					}
				}
				else
				{
					parentPath = string.Concat(".\\", child);
				}
			}
			if (flag)
			{
				parentPath = ADProvider.AddAbsolutePathPrefix(parentPath);
			}
			this.Trace(DebugLogLevel.Info, string.Format("MakePath: path = {0}", parentPath));
			this.Trace(DebugLogLevel.Verbose, "Leaving MakePath");
			return string.Concat(value, parentPath);
		}

		protected override void MoveItem(string path, string destination)
		{
			bool crossDomain;
			this.Trace(DebugLogLevel.Verbose, "Entering MoveItem");
			if (!this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				string childName = ADPathModule.GetChildName(path, ADPathFormat.X500);
				if (base.ShouldProcess(path, "Move"))
				{
					ADProviderMoveParameters dynamicParameters = base.DynamicParameters as ADProviderMoveParameters;
					if (dynamicParameters != null)
					{
						crossDomain = dynamicParameters.CrossDomain != null;
					}
					else
					{
						crossDomain = false;
					}
					bool flag = crossDomain;
					ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
					if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
					{
						ADObject aDObject = new ADObject();
						aDObject.DistinguishedName = path;
						ADActiveObject aDActiveObject = null;
						using (aDActiveObject)
						{
							try
							{
								aDActiveObject = new ADActiveObject(sessionInfo, aDObject);
								if (!flag)
								{
									destination = this.ValidateAndNormalizePath(destination);
									aDActiveObject.Move(destination, childName);
								}
								else
								{
									ADSessionInfo aDSessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
									aDSessionInfo.Server = dynamicParameters.CrossDomain;
									destination = this.ValidateAndNormalizePath(destination, aDSessionInfo);
									aDActiveObject.CrossDomainMove(destination, childName, dynamicParameters.CrossDomain);
									sessionInfo = aDSessionInfo;
								}
								string str = ADPathModule.MakePath(destination, childName, ADPathFormat.X500);
								this.Trace(DebugLogLevel.Info, string.Format("MoveItem: newPath = {0}.", str));
								aDObject = this.GetValidatedADObject(sessionInfo, str, null, this.GetFormatType(dynamicParameters, this.ExtendedDriveInfo));
								sessionInfo.ServerType = this.GetServerType(sessionInfo);
							}
							catch (ADException aDException1)
							{
								ADException aDException = aDException1;
								base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:MoveItem:ADError", path));
								this.Trace(DebugLogLevel.Error, "Leaving MoveItem: ADException: AD error");
								return;
							}
							catch (UnauthorizedAccessException unauthorizedAccessException1)
							{
								UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
								base.WriteError(ADUtilities.GetErrorRecord(unauthorizedAccessException, "ADProvider:MoveItem:AccessDenied", path));
								this.Trace(DebugLogLevel.Error, "Leaving MoveItem: ADException: access denied");
								return;
							}
							catch (AuthenticationException authenticationException1)
							{
								AuthenticationException authenticationException = authenticationException1;
								base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:MoveItem:InvalidCredentials", path));
								this.Trace(DebugLogLevel.Error, "Leaving MoveItem: ADException: invalid credentials");
								return;
							}
							catch (InvalidOperationException invalidOperationException1)
							{
								InvalidOperationException invalidOperationException = invalidOperationException1;
								base.WriteError(ADUtilities.GetErrorRecord(invalidOperationException, "ADProvider:MoveItem:InvalidOperation", path));
								this.Trace(DebugLogLevel.Error, "Leaving MoveItem: ADException: invalid operation");
								return;
							}
							this.WriteADObject(aDObject, sessionInfo, base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo);
							this.Trace(DebugLogLevel.Verbose, "Leaving MoveItem");
							return;
						}
						return;
					}
					else
					{
						this.Trace(DebugLogLevel.Info, "Leaving MoveItem: ValidateDynamicParameters returned false.");
						return;
					}
				}
				else
				{
					this.Trace(DebugLogLevel.Info, "Leaving MoveItem: ShouldProcess returned false.");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
				this.Trace(DebugLogLevel.Error, "Leaving MoveItem: NotSupportedException: path is rootdse");
				return;
			}
		}

		protected override object MoveItemDynamicParameters(string path, string destination)
		{
			return new ADProviderMoveParameters();
		}

		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
			PSDriveInfo pSDriveInfo;
			this.Trace(DebugLogLevel.Verbose, "Entering NewDrive");
			ADSession session = null;
			if (drive != null)
			{
				if (drive.Root != null)
				{
					object[] name = new object[1];
					name[0] = drive.Name;
					ProgressRecord progressRecord = new ProgressRecord(0, string.Format(CultureInfo.CurrentCulture, StringResources.LoadingDriveProgressMessage, name), " ");
					progressRecord.PercentComplete = 0;
					try
					{
						base.WriteProgress(progressRecord);
						string str = ADProvider.RemoveAbsolutePathPrefix(drive.Root);
						ADDriveInfo aDDriveInfo = new ADDriveInfo(drive.Name, drive.Provider, str, ADProvider.AddAbsolutePathPrefix(str), drive.Description, drive.Credential);
						ADProviderCommonParameters dynamicParameters = base.DynamicParameters as ADProviderCommonParameters;
						if (dynamicParameters != null)
						{
							aDDriveInfo.FormatType = dynamicParameters.FormatType;
							aDDriveInfo.Server = dynamicParameters.Server;
							aDDriveInfo.AuthType = dynamicParameters.AuthType;
							aDDriveInfo.GlobalCatalog = dynamicParameters.GlobalCatalog;
						}
						ProgressRecord percentComplete = progressRecord;
						percentComplete.PercentComplete = percentComplete.PercentComplete + 25;
						base.WriteProgress(progressRecord);
						ADSessionInfo sessionInfo = this.GetSessionInfo(null, null, aDDriveInfo);
						if (this.ValidateDynamicParameters(aDDriveInfo.RootWithoutAbsolutePathToken, dynamicParameters, sessionInfo))
						{
							this.Trace(DebugLogLevel.Info, string.Format("NewDrive: Root = {0}", aDDriveInfo.Root));
							this.Trace(DebugLogLevel.Info, string.Format("NewDrive: FormatType = {0}", aDDriveInfo.FormatType));
							this.Trace(DebugLogLevel.Info, string.Format("NewDrive: Server = {0}", aDDriveInfo.Server));
							this.Trace(DebugLogLevel.Info, string.Format("NewDrive: AuthType = {0}", aDDriveInfo.AuthType));
							this.Trace(DebugLogLevel.Info, string.Format("NewDrive: GlobalCatalog = {0}", aDDriveInfo.GlobalCatalog));
							try
							{
								try
								{
									aDDriveInfo.SessionInfo = sessionInfo;
									aDDriveInfo.Session = ADSession.ConstructSession(aDDriveInfo.SessionInfo);
									session = aDDriveInfo.Session;
									this.Trace(DebugLogLevel.Verbose, "NewDrive: Session created.");
									ProgressRecord percentComplete1 = progressRecord;
									percentComplete1.PercentComplete = percentComplete1.PercentComplete + 25;
									base.WriteProgress(progressRecord);
									if (!this.IsValidRootDSEPath(aDDriveInfo.RootWithoutAbsolutePathToken))
									{
										string x500Path = ADProvider.RemoveAbsolutePathPrefix(aDDriveInfo.Root);
										if (this.GetFormatType(dynamicParameters, null) == ADPathFormat.Canonical)
										{
											x500Path = this.ConvertToX500Path(x500Path, sessionInfo);
											this.Trace(DebugLogLevel.Info, string.Format("NewDrive: rootPath (after conversion to X500) = {0}", x500Path));
										}
										ADObject validatedADObject = this.GetValidatedADObject(x500Path, null, null, null, aDDriveInfo);
										ProgressRecord progressRecord1 = progressRecord;
										progressRecord1.PercentComplete = progressRecord1.PercentComplete + 25;
										base.WriteProgress(progressRecord);
										if (validatedADObject != null)
										{
											aDDriveInfo.RootPartitionPath = this.GetContainingPartition(x500Path, null, null, aDDriveInfo);
										}
										else
										{
											base.WriteError(ADUtilities.GetErrorRecord(new ArgumentNullException("drive.Root"), "ADProvider:NewDrive:InvalidRoot", drive));
											this.Trace(DebugLogLevel.Error, "Leaving NewDrive: ArgumentNullException: Invalid drive root.");
											aDDriveInfo = null;
											pSDriveInfo = null;
											return pSDriveInfo;
										}
									}
									else
									{
										ADRootDSE rootDSE = this.GetRootDSE(sessionInfo);
										ProgressRecord percentComplete2 = progressRecord;
										percentComplete2.PercentComplete = percentComplete2.PercentComplete + 25;
										base.WriteProgress(progressRecord);
										aDDriveInfo.RootPartitionPath = aDDriveInfo.RootWithoutAbsolutePathToken;
										aDDriveInfo.NamingContexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
										foreach (ADObject allHostedNamingContext in this.GetAllHostedNamingContexts(sessionInfo, rootDSE, aDDriveInfo, null))
										{
											string distinguishedName = null;
											if (aDDriveInfo.FormatType != ADPathFormat.Canonical)
											{
												distinguishedName = allHostedNamingContext.DistinguishedName;
											}
											else
											{
												if (allHostedNamingContext.Contains("canonicalName"))
												{
													distinguishedName = (string)allHostedNamingContext["canonicalName"].Value;
												}
												else
												{
													this.Trace(DebugLogLevel.Warning, string.Format("NewDrive: Unable to read canonical name for naming context {0}, skipping..", allHostedNamingContext.DistinguishedName));
													continue;
												}
											}
											aDDriveInfo.NamingContexts.Add(distinguishedName);
										}
									}
								}
								catch (ADException aDException1)
								{
									ADException aDException = aDException1;
									base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:NewDrive:InvalidRoot:ADError", drive));
									this.Trace(DebugLogLevel.Error, "Leaving NewDrive: ADException: AD error.");
									aDDriveInfo = null;
									pSDriveInfo = null;
									return pSDriveInfo;
								}
								catch (AuthenticationException authenticationException1)
								{
									AuthenticationException authenticationException = authenticationException1;
									base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:NewDrive:InvalidRoot:InvalidCredentials", drive));
									this.Trace(DebugLogLevel.Error, "Leaving NewDrive: AuthenticationException: Invalid credentials.");
									aDDriveInfo = null;
									pSDriveInfo = null;
									return pSDriveInfo;
								}
							}
							finally
							{
								if (null == aDDriveInfo && session != null)
								{
									session.Delete();
									this.Trace(DebugLogLevel.Info, string.Format("Leaving NewDrive:Session to server terminated.", new object[0]));
								}
							}
							this.Trace(DebugLogLevel.Verbose, "Leaving NewDrive");
							ProgressRecord progressRecord2 = progressRecord;
							progressRecord2.PercentComplete = progressRecord2.PercentComplete + 25;
							base.WriteProgress(progressRecord);
							pSDriveInfo = aDDriveInfo;
						}
						else
						{
							this.Trace(DebugLogLevel.Error, "Leaving NewDrive: ValidateDynamicParameters returned false.");
							pSDriveInfo = null;
						}
					}
					finally
					{
						progressRecord.RecordType = ProgressRecordType.Completed;
						base.WriteProgress(progressRecord);
					}
					return pSDriveInfo;
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ArgumentNullException("drive.Root"), "ADProvider:NewDrive:NullRoot", drive));
					this.Trace(DebugLogLevel.Error, "Leaving NewDrive: ArgumentNullException: drive.Root is null.");
					return null;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new ArgumentNullException("drive"), "ADProvider:NewDrive:NullDrive", null));
				this.Trace(DebugLogLevel.Error, "Leaving NewDrive: ArgumentNullException: drive is null.");
				return null;
			}
		}

		protected override object NewDriveDynamicParameters()
		{
			return new ADProviderDriveParameters();
		}

		protected override void NewItem(string path, string itemTypeName, object newItemValue)
		{
			string root;
			this.Trace(DebugLogLevel.Verbose, "Entering NewItem");
			Collection<string> strs = new Collection<string>();
			if (this.IsValidPath(path))
			{
				string childName = this.GetChildName(path);
				ADProvider aDProvider = this;
				string str = path;
				if (this.ExtendedDriveInfo != null)
				{
					root = this.ExtendedDriveInfo.Root;
				}
				else
				{
					root = "";
				}
				string parentPath = aDProvider.GetParentPath(str, root);
				if (!this.IsValidRootDSEPath(parentPath))
				{
					parentPath = this.ValidateAndNormalizePath(parentPath);
					string str1 = ADPathModule.MakePath(parentPath, childName, ADPathFormat.X500);
					if (string.IsNullOrEmpty(itemTypeName))
					{
						base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException("itemTypeName"), "ADProvider:NewItem:InvalidItemTypeName", path));
						this.Trace(DebugLogLevel.Error, "Leaving NewItem: ArgumentException: itemType is invalid (null or empty)");
					}
					if (base.ShouldProcess(path, "New"))
					{
						ADProviderCommonParameters dynamicParameters = base.DynamicParameters as ADProviderCommonParameters;
						ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
						if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
						{
							if (itemTypeName.Equals("directory", StringComparison.OrdinalIgnoreCase))
							{
								itemTypeName = "organizationalUnit";
							}
							ADObject aDObject = new ADObject(str1, itemTypeName);
							if (newItemValue != null)
							{
								Hashtable hashtables = newItemValue as Hashtable;
								if (hashtables == null)
								{
									base.WriteError(ADUtilities.GetErrorRecord(new ArgumentNullException("newItemValue"), "ADProvider:NewItem:InvalidNewItemValue", path));
									this.Trace(DebugLogLevel.Error, "Leaving NewItem: ArgumentNullException: newItemValue is null");
								}
								foreach (string key in hashtables.Keys)
								{
									object item = hashtables[key];
									if (item != null)
									{
										aDObject.Add(key, hashtables[key]);
										strs.Add(key);
									}
									else
									{
										base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException("newItemValue", "Property value to be added cannot be null."), "ADProvider: NewItem: InvalidArgument", path));
										this.Trace(DebugLogLevel.Error, "Leaving NewItem: ArgumentException: newItemValue has null value.");
										return;
									}
								}
							}
							ADActiveObject aDActiveObject = null;
							try
							{
								try
								{
									aDActiveObject = new ADActiveObject(sessionInfo, aDObject);
									aDActiveObject.Create();
									if (ProtectedFromDeletionUtil.ShouldProtectByDefault(aDObject.ObjectClass))
									{
										aDObject.TrackChanges = true;
										if (ProtectedFromDeletionUtil.ProtectFromAccidentalDeletion(aDObject, sessionInfo))
										{
											aDActiveObject.Update();
										}
									}
									aDObject = this.GetValidatedADObject(str1, strs, base.DynamicParameters as ADProviderCommonParameters, base.Credential, this.ExtendedDriveInfo);
									sessionInfo.ServerType = this.GetServerType(sessionInfo);
								}
								catch (ADException aDException1)
								{
									ADException aDException = aDException1;
									base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:NewItem:ADError", path));
									this.Trace(DebugLogLevel.Error, "Leaving NewItem: ADException: AD error");
									return;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									base.WriteError(ADUtilities.GetErrorRecord(unauthorizedAccessException, "ADProvider:NewItem:AccessDenied", path));
									this.Trace(DebugLogLevel.Error, "Leaving NewItem: ADException: access denied");
									return;
								}
								catch (AuthenticationException authenticationException1)
								{
									AuthenticationException authenticationException = authenticationException1;
									base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:NewItem:InvalidCredentials", path));
									this.Trace(DebugLogLevel.Error, "Leaving NewItem: ADException: invalid credentials");
									return;
								}
								catch (InvalidOperationException invalidOperationException1)
								{
									InvalidOperationException invalidOperationException = invalidOperationException1;
									base.WriteError(ADUtilities.GetErrorRecord(invalidOperationException, "ADProvider:NewItem:InvalidOperation", path));
									this.Trace(DebugLogLevel.Error, "Leaving NewItem: ADException: invalid operation");
									return;
								}
								this.WriteADObject(aDObject, sessionInfo, base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo);
								this.Trace(DebugLogLevel.Verbose, "Leaving NewItem");
								return;
							}
							finally
							{
								if (aDActiveObject != null)
								{
									this.Trace(DebugLogLevel.Verbose, "Leaving NewItem: Calling Dispose on ADActiveObject.");
									aDActiveObject.Dispose();
								}
							}
							return;
						}
						else
						{
							this.Trace(DebugLogLevel.Info, "Leaving NewItem: ValidateDynamicParameters returned false.");
							return;
						}
					}
					else
					{
						this.Trace(DebugLogLevel.Info, "Leaving NewItem: ShouldProcess returned false.");
						return;
					}
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
					this.Trace(DebugLogLevel.Error, "Leaving NewItem: NotSupportedException: path is rootdse");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(path), "InvalidPath", path));
				this.Trace(DebugLogLevel.Error, "Leaving NewItem: ArgumentException: path is invalid");
				return;
			}
		}

		protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
		{
			return new ADProviderCommonParameters();
		}

		public ObjectSecurity NewSecurityDescriptorFromPath(string path, AccessControlSections includeSections)
		{
			return new ActiveDirectorySecurity();
		}

		public ObjectSecurity NewSecurityDescriptorOfType(string type, AccessControlSections includeSections)
		{
			return new ActiveDirectorySecurity();
		}

		protected override string NormalizeRelativePath(string path, string basePath)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering NormalizeRelativePath");
			if (path != null)
			{
				string str = "";
				ADPathFormat formatType = this.GetFormatType(base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo);
				this.Trace(DebugLogLevel.Info, string.Format("NormalizeRelativePath: path = {0}", path));
				this.Trace(DebugLogLevel.Info, string.Format("NormalizeRelativePath: basePath = {0}", basePath));
				if (!(this.ExtendedDriveInfo != null) || !ADPathModule.ComparePath(ADProvider.RemoveAbsolutePathPrefix(path), ADProvider.RemoveAbsolutePathPrefix(this.ExtendedDriveInfo.Root), formatType) || !ADPathModule.ComparePath(ADProvider.RemoveAbsolutePathPrefix(basePath), ADProvider.RemoveAbsolutePathPrefix(this.ExtendedDriveInfo.Root), formatType))
				{
					string str1 = "";
					string childName = "";
					Stack<string> stack = this.TokenizePathToStack(path);
					Stack<string> strs = this.TokenizePathToStack(basePath);
					while (stack.Count > 0 && strs.Count > 0)
					{
						if (ADPathModule.ComparePath(stack.Peek(), strs.Peek(), formatType))
						{
							stack.Pop();
							strs.Pop();
						}
						else
						{
							break;
						}
					}
					while (strs.Count > 0)
					{
						strs.Pop();
						str1 = ADPathHelper.MakePshPath("..", str1);
					}
					while (stack.Count > 0)
					{
						childName = this.MakePath(childName, stack.Pop());
					}
					if (string.IsNullOrEmpty(childName))
					{
						str1 = ADPathHelper.MakePshPath("..", str1);
						childName = this.GetChildName(path);
					}
					str = ADPathHelper.MakePshPath(str1, childName);
				}
				else
				{
					str = "";
				}
				this.Trace(DebugLogLevel.Info, string.Format("NormalizeRelativePath: normalizedPath = {0}", str));
				this.Trace(DebugLogLevel.Verbose, "Leaving NormalizeRelativePath");
				return str;
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new ArgumentNullException(path), "ADProvider:NormalizeRelativePath:NullPath", path));
				this.Trace(DebugLogLevel.Error, "Leaving NormalizeRelativePath: ArgumentNullException: path is null");
				return null;
			}
		}

		private static bool PathIsAbsolute(string path)
		{
			if (path != null)
			{
				return path.StartsWith("//RootDSE/", StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				return false;
			}
		}

		private static string RemoveAbsolutePathPrefix(string path)
		{
			if (path != null)
			{
				if (!path.StartsWith("//RootDSE/", StringComparison.OrdinalIgnoreCase))
				{
					return path;
				}
				else
				{
					return path.Substring("//RootDSE/".Length);
				}
			}
			else
			{
				return null;
			}
		}

		protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering RemoveDrive");
			if (drive != null)
			{
				ADDriveInfo aDDriveInfo = drive as ADDriveInfo;
				if (aDDriveInfo.Session != null)
				{
					aDDriveInfo.Session.Delete();
					this.Trace(DebugLogLevel.Info, string.Format("RemoveDrive:Session to server {0} terminated.", aDDriveInfo.Server));
				}
				this.Trace(DebugLogLevel.Verbose, "Leaving RemoveDrive");
				return aDDriveInfo;
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new ArgumentNullException("drive"), "ADProvider:RemoveDrive:NullDrive", drive));
				this.Trace(DebugLogLevel.Error, "Leaving RemoveDrive: ArgumentNullException: drive is null.");
				return null;
			}
		}

		protected override void RemoveItem(string path, bool recurse)
		{
			this.Trace(DebugLogLevel.Verbose, "Entering RemoveItem");
			if (!this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				string promptForRemove = StringResources.PromptForRemove;
				if (recurse)
				{
					promptForRemove = StringResources.PromptForRecursiveRemove;
				}
				if (base.ShouldProcess(path, "Remove"))
				{
					if (base.Force || base.ShouldContinue(path, promptForRemove))
					{
						ADProviderCommonParameters dynamicParameters = base.DynamicParameters as ADProviderCommonParameters;
						ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
						if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
						{
							ADObject aDObject = new ADObject();
							aDObject.DistinguishedName = path;
							ADActiveObject aDActiveObject = null;
							using (aDActiveObject)
							{
								try
								{
									aDActiveObject = new ADActiveObject(sessionInfo, aDObject);
									if (!recurse)
									{
										aDActiveObject.Delete();
									}
									else
									{
										aDActiveObject.DeleteTree();
									}
								}
								catch (ADException aDException1)
								{
									ADException aDException = aDException1;
									base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:RemoveItem:ADError", path));
									this.Trace(DebugLogLevel.Error, "Leaving RemoveItem: ADException: AD error");
									return;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									base.WriteError(ADUtilities.GetErrorRecord(unauthorizedAccessException, "ADProvider:RemoveItem:AccessDenied", path));
									this.Trace(DebugLogLevel.Error, "Leaving RemoveItem: ADException: access denied");
									return;
								}
								catch (AuthenticationException authenticationException1)
								{
									AuthenticationException authenticationException = authenticationException1;
									base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:RemoveItem:InvalidCredentials", path));
									this.Trace(DebugLogLevel.Error, "Leaving RemoveItem: ADException: invalid credentials");
									return;
								}
								catch (InvalidOperationException invalidOperationException1)
								{
									InvalidOperationException invalidOperationException = invalidOperationException1;
									base.WriteError(ADUtilities.GetErrorRecord(invalidOperationException, "ADProvider:RemoveItem:InvalidOperation", path));
									this.Trace(DebugLogLevel.Error, "Leaving RemoveItem: ADException: invalid operation");
									return;
								}
								this.Trace(DebugLogLevel.Verbose, "Leaving RemoveItem");
								return;
							}
							return;
						}
						else
						{
							this.Trace(DebugLogLevel.Info, "Leaving RemoveItem: ValidateDynamicParameters returned false.");
							return;
						}
					}
					else
					{
						this.Trace(DebugLogLevel.Info, "Leaving RemoveItem: -Force not used or ShouldContinue returned false.");
						return;
					}
				}
				else
				{
					this.Trace(DebugLogLevel.Info, "Leaving RemoveItem: ShouldProcess returned false.");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
				this.Trace(DebugLogLevel.Error, "Leaving RemoveItem: NotSupportedException: path is rootdse");
				return;
			}
		}

		protected override object RemoveItemDynamicParameters(string path, bool recurse)
		{
			return new ADProviderCommonParameters();
		}

		protected override void RenameItem(string path, string newName)
		{
			string root;
			this.Trace(DebugLogLevel.Verbose, "Entering RenameItem");
			if (!this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				if (!string.IsNullOrEmpty(newName))
				{
					if (base.ShouldProcess(path, "Rename"))
					{
						ADProviderCommonParameters dynamicParameters = base.DynamicParameters as ADProviderCommonParameters;
						ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
						if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
						{
							ADObject aDObject = new ADObject();
							aDObject.DistinguishedName = path;
							ADActiveObject aDActiveObject = null;
							using (aDActiveObject)
							{
								try
								{
									aDActiveObject = new ADActiveObject(sessionInfo, aDObject);
									aDActiveObject.Rename(newName);
									ADProvider aDProvider = this;
									ADProvider aDProvider1 = this;
									string str = path;
									if (this.ExtendedDriveInfo != null)
									{
										root = this.ExtendedDriveInfo.Root;
									}
									else
									{
										root = "";
									}
									string str1 = aDProvider.MakePath(aDProvider1.GetParentPath(str, root), newName);
									this.Trace(DebugLogLevel.Info, string.Format("RenameItem: newPath = {0}.", str1));
									aDObject = this.GetValidatedADObject(str1, null, base.DynamicParameters as ADProviderCommonParameters, base.Credential, this.ExtendedDriveInfo);
									sessionInfo.ServerType = this.GetServerType(sessionInfo);
								}
								catch (ADException aDException1)
								{
									ADException aDException = aDException1;
									base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:RenameItem:ADError", path));
									this.Trace(DebugLogLevel.Error, "Leaving RenameItem: ADException: AD error");
									return;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									base.WriteError(ADUtilities.GetErrorRecord(unauthorizedAccessException, "ADProvider:RenameItem:AccessDenied", path));
									this.Trace(DebugLogLevel.Error, "Leaving RenameItem: ADException: access denied");
									return;
								}
								catch (AuthenticationException authenticationException1)
								{
									AuthenticationException authenticationException = authenticationException1;
									base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:RenameItem:InvalidCredentials", path));
									this.Trace(DebugLogLevel.Error, "Leaving RenameItem: ADException: invalid credentials");
									return;
								}
								catch (InvalidOperationException invalidOperationException1)
								{
									InvalidOperationException invalidOperationException = invalidOperationException1;
									base.WriteError(ADUtilities.GetErrorRecord(invalidOperationException, "ADProvider:RenameItem:InvalidOperation", path));
									this.Trace(DebugLogLevel.Error, "Leaving RenameItem: ADException: invalid operation");
									return;
								}
								this.WriteADObject(aDObject, sessionInfo, base.DynamicParameters as ADProviderCommonParameters, this.ExtendedDriveInfo);
								this.Trace(DebugLogLevel.Verbose, "Leaving RenameItem");
								return;
							}
							return;
						}
						else
						{
							this.Trace(DebugLogLevel.Info, "Leaving RenameItem: ValidateDynamicParameters returned false.");
							return;
						}
					}
					else
					{
						this.Trace(DebugLogLevel.Info, "Leaving RenameItem: ShouldProcess returned false.");
						return;
					}
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(newName), "newName", path));
					this.Trace(DebugLogLevel.Error, "Leaving RenameItem: ArgumentNullException: newItemValue is null");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
				this.Trace(DebugLogLevel.Error, "Leaving RenameItem: NotSupportedException: path is rootdse");
				return;
			}
		}

		protected override object RenameItemDynamicParameters(string path, string newName)
		{
			return new ADProviderCommonParameters();
		}

		protected override void SetItem(string path, object value)
		{
			base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderSetItemNotSupported), "ADProvider:SetItem:NotSupported", path));
		}

		public void SetProperty(string path, PSObject propertyValue)
		{
			ADPropertyValueCollection aDPropertyValueCollection;
			ADPropertyValueCollection item;
			ADPropertyValueCollection aDPropertyValueCollection1;
			ADPropertyValueCollection item1;
			PSObject pSObject = new PSObject();
			Collection<string> strs = new Collection<string>();
			this.Trace(DebugLogLevel.Verbose, "Entering SetProperty");
			if (propertyValue != null)
			{
				if (!this.IsValidRootDSEPath(path))
				{
					path = this.ValidateAndNormalizePath(path);
					ADProviderSetPropertyParameters dynamicParameters = base.DynamicParameters as ADProviderSetPropertyParameters;
					ADSessionInfo sessionInfo = this.GetSessionInfo(dynamicParameters, base.Credential, this.ExtendedDriveInfo);
					if (this.ValidateDynamicParameters(path, dynamicParameters, sessionInfo))
					{
						ADObject aDObject = new ADObject();
						aDObject.DistinguishedName = path;
						ADActiveObject aDActiveObject = new ADActiveObject(sessionInfo, aDObject);
						if (dynamicParameters != null)
						{
							if (dynamicParameters.ReplacePropertyValue != null)
							{
								foreach (string key in dynamicParameters.ReplacePropertyValue.Keys)
								{
									object obj = dynamicParameters.ReplacePropertyValue[key];
									if (!aDObject.Contains(key))
									{
										aDPropertyValueCollection = new ADPropertyValueCollection();
										aDObject.Add(key, aDPropertyValueCollection);
									}
									else
									{
										aDPropertyValueCollection = aDObject[key];
										aDPropertyValueCollection.Clear();
									}
									if (obj.GetType() != typeof(object[]))
									{
										aDPropertyValueCollection.Add(obj);
									}
									else
									{
										object[] objArray = (object[])obj;
										for (int i = 0; i < (int)objArray.Length; i++)
										{
											object obj1 = objArray[i];
											aDPropertyValueCollection.Add(obj1);
										}
									}
									strs.Add(key);
								}
							}
							if (dynamicParameters.AddPropertyValue != null)
							{
								foreach (string str in dynamicParameters.AddPropertyValue.Keys)
								{
									object item2 = dynamicParameters.AddPropertyValue[str];
									if (item2 != null)
									{
										if (!aDObject.Contains(str))
										{
											item = new ADPropertyValueCollection();
											item.TrackChanges = true;
											aDObject.Add(str, item);
										}
										else
										{
											item = aDObject[str];
										}
										if (item2.GetType() != typeof(object[]))
										{
											item.Add(item2);
										}
										else
										{
											object[] objArray1 = (object[])item2;
											for (int j = 0; j < (int)objArray1.Length; j++)
											{
												object obj2 = objArray1[j];
												item.Add(obj2);
											}
										}
										strs.Add(str);
									}
									else
									{
										base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(StringResources.ADProviderPropertyValueCannotBeNull), "ADProvider: SetProperty: InvalidArgument", path));
										this.Trace(DebugLogLevel.Error, "Leaving SetProperty: ArgumentException: addPropertyValue has null value.");
										return;
									}
								}
							}
							if (dynamicParameters.RemovePropertyValue != null)
							{
								foreach (string key1 in dynamicParameters.RemovePropertyValue.Keys)
								{
									object item3 = dynamicParameters.RemovePropertyValue[key1];
									if (!aDObject.Contains(key1))
									{
										aDPropertyValueCollection1 = new ADPropertyValueCollection();
										aDPropertyValueCollection1.TrackChanges = true;
										aDObject.Add(key1, aDPropertyValueCollection1);
									}
									else
									{
										aDPropertyValueCollection1 = aDObject[key1];
									}
									if (item3.GetType() != typeof(object[]))
									{
										aDPropertyValueCollection1.Remove(item3);
									}
									else
									{
										object[] objArray2 = (object[])item3;
										for (int k = 0; k < (int)objArray2.Length; k++)
										{
											object obj3 = objArray2[k];
											aDPropertyValueCollection1.Remove(obj3);
										}
									}
									strs.Add(key1);
								}
							}
						}
						foreach (PSMemberInfo property in propertyValue.Properties)
						{
							string name = property.Name;
							object value = property.Value;
							if (!aDObject.Contains(name))
							{
								item1 = new ADPropertyValueCollection();
								item1.TrackChanges = true;
								aDObject.Add(name, item1);
							}
							else
							{
								item1 = aDObject[name];
							}
							if (value.GetType() != typeof(object[]))
							{
								item1.Value = value;
							}
							else
							{
								item1.Clear();
								object[] objArray3 = (object[])value;
								for (int l = 0; l < (int)objArray3.Length; l++)
								{
									object obj4 = objArray3[l];
									item1.Add(obj4);
								}
							}
							strs.Add(name);
						}
						if (base.ShouldProcess(path, "Set"))
						{
							using (aDActiveObject)
							{
								try
								{
									aDActiveObject.Update();
									aDObject = this.GetValidatedADObject(path, strs, dynamicParameters, base.Credential, this.ExtendedDriveInfo);
								}
								catch (ADException aDException1)
								{
									ADException aDException = aDException1;
									base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:SetProperty:ADError", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetProperty: ADException: AD error");
									return;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									base.WriteError(ADUtilities.GetErrorRecord(unauthorizedAccessException, "ADProvider:SetProperty:AccessDenied", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetProperty: ADException: access denied");
									return;
								}
								catch (AuthenticationException authenticationException1)
								{
									AuthenticationException authenticationException = authenticationException1;
									base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:SetProperty:InvalidCredentials", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetProperty: ADException: invalid credentials");
									return;
								}
								catch (InvalidOperationException invalidOperationException1)
								{
									InvalidOperationException invalidOperationException = invalidOperationException1;
									base.WriteError(ADUtilities.GetErrorRecord(invalidOperationException, "ADProvider:SetProperty:InvalidOperation", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetProperty: ADException: invalid operation");
									return;
								}
							}
							foreach (string str1 in strs)
							{
								pSObject.Properties.Add(new PSNoteProperty(str1, aDObject[str1].Value));
							}
							this.WriteADObjectProperties(aDObject, pSObject, dynamicParameters, this.ExtendedDriveInfo);
							this.Trace(DebugLogLevel.Verbose, "Leaving SetProperty");
							return;
						}
						else
						{
							this.Trace(DebugLogLevel.Info, "Leaving SetProperty: ShouldProcess returned false.");
						}
						return;
					}
					else
					{
						this.Trace(DebugLogLevel.Error, "Leaving GetProperty: ValidateDynamicParameters: returned false");
						return;
					}
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
					this.Trace(DebugLogLevel.Error, "Leaving SetProperty: NotSupportedException: path is rootdse");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new ArgumentNullException("propertyValue"), "ADProvider:SetProperty:NullArgument", path));
				this.Trace(DebugLogLevel.Error, "Leaving SetProperty: ArgumentNullException: propertyValue is null");
				return;
			}
		}

		public object SetPropertyDynamicParameters(string path, PSObject propertyValue)
		{
			return new ADProviderSetPropertyParameters();
		}

		public void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor)
		{
			ADPropertyValueCollection aDPropertyValueCollection;
			this.Trace(DebugLogLevel.Verbose, "Entering SetSecurityDescriptor");
			Collection<string> strs = new Collection<string>();
			if (!this.IsValidRootDSEPath(path))
			{
				path = this.ValidateAndNormalizePath(path);
				if (securityDescriptor != null)
				{
					ActiveDirectorySecurity activeDirectorySecurity = securityDescriptor as ActiveDirectorySecurity;
					if (activeDirectorySecurity != null)
					{
						if (base.ShouldProcess(path, "Set"))
						{
							ADSessionInfo sessionInfo = this.GetSessionInfo(null, base.Credential, this.ExtendedDriveInfo);
							ADObject aDObject = new ADObject();
							aDObject.DistinguishedName = path;
							ADActiveObject aDActiveObject = new ADActiveObject(sessionInfo, aDObject);
							strs.Add("ntSecurityDescriptor");
							aDActiveObject.SecurityDescriptorFlags = SecurityMasks.None;
							using (aDActiveObject)
							{
								try
								{
									if (!aDObject.Contains("ntSecurityDescriptor"))
									{
										aDPropertyValueCollection = new ADPropertyValueCollection();
										aDObject.Add("ntSecurityDescriptor", aDPropertyValueCollection);
									}
									else
									{
										aDPropertyValueCollection = aDObject["ntSecurityDescriptor"];
									}
									aDObject["ntSecurityDescriptor"].Value = activeDirectorySecurity;
									aDActiveObject.Update();
									aDObject = this.GetValidatedADObject(path, strs, null, base.Credential, this.ExtendedDriveInfo);
								}
								catch (ADException aDException1)
								{
									ADException aDException = aDException1;
									base.WriteError(ADUtilities.GetErrorRecord(aDException, "ADProvider:SetSecurityDescriptor:ADError", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetSecurityDescriptor: ADException: AD error");
									return;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									base.WriteError(ADUtilities.GetErrorRecord(unauthorizedAccessException, "ADProvider:SetSecurityDescriptor:AccessDenied", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetSecurityDescriptor: ADException: access denied");
									return;
								}
								catch (AuthenticationException authenticationException1)
								{
									AuthenticationException authenticationException = authenticationException1;
									base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "ADProvider:SetSecurityDescriptor:InvalidCredentials", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetSecurityDescriptor: ADException: invalid credentials");
									return;
								}
								catch (InvalidOperationException invalidOperationException1)
								{
									InvalidOperationException invalidOperationException = invalidOperationException1;
									base.WriteError(ADUtilities.GetErrorRecord(invalidOperationException, "ADProvider:SetSecurityDescriptor:InvalidOperation", path));
									this.Trace(DebugLogLevel.Error, "Leaving SetSecurityDescriptor: ADException: invalid operation");
									return;
								}
								this.WriteADObjectSecurityDescriptor(aDObject, activeDirectorySecurity, null, this.ExtendedDriveInfo);
								this.Trace(DebugLogLevel.Verbose, "Leaving SetSecurityDescriptor");
								return;
							}
							return;
						}
						else
						{
							this.Trace(DebugLogLevel.Info, "Leaving SetSecurityDescriptor: ShouldProcess returned false.");
							return;
						}
					}
					else
					{
						base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException("securityDescriptor"), "ADProvider:SetSecurityDescriptor:InvalidSecurityDescriptor", path));
						this.Trace(DebugLogLevel.Error, "Leaving SetSecurityDescriptor: ArgumentException: securityDescriptor is of incorrect type");
						return;
					}
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException("securityDescriptor"), "ADProvider:SetSecurityDescriptor:InvalidSecurityDescriptor", path));
					this.Trace(DebugLogLevel.Error, "Leaving SetSecurityDescriptor: ArgumentException: securityDescriptor is null");
					return;
				}
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new NotSupportedException(StringResources.ADProviderOperationNotSupportedForRootDSE), "NotSupported", path));
				this.Trace(DebugLogLevel.Error, "Leaving SetSecurityDescriptor: NotSupportedException: path is rootdse");
				return;
			}
		}

		string System.Management.Automation.Provider.ICmdletProviderSupportsHelp.GetHelpMaml(string helpItemName, string path)
		{
			string str = null;
			string str1 = null;
			if (!string.IsNullOrEmpty(helpItemName))
			{
				int num = helpItemName.IndexOf('-');
				if (num != -1)
				{
					str = helpItemName.Substring(0, num);
					str1 = helpItemName.Substring(num + 1);
				}
			}
			if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str1))
			{
				return string.Empty;
			}
			else
			{
				XmlDocument xmlDocument = new XmlDocument();
				CultureInfo currentUICulture = base.Host.CurrentUICulture;
				string[] moduleBase = new string[5];
				moduleBase[0] = base.ProviderInfo.Module.ModuleBase;
				moduleBase[1] = "\\";
				moduleBase[2] = currentUICulture.ToString();
				moduleBase[3] = "\\";
				moduleBase[4] = base.ProviderInfo.HelpFile;
				xmlDocument.Load(string.Concat(moduleBase));
				XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManagers.AddNamespace("command", "http://schemas.microsoft.com/maml/dev/command/2004/10");
				string[] strArrays = new string[5];
				strArrays[0] = "/helpItems/ProviderHelp/CmdletHelpPaths/CmdletHelpPath/command:command[command:details/command:verb='";
				strArrays[1] = str;
				strArrays[2] = "' and command:details/command:noun='";
				strArrays[3] = str1;
				strArrays[4] = "']";
				string str2 = string.Concat(strArrays);
				XmlNode xmlNodes = xmlDocument.SelectSingleNode(str2, xmlNamespaceManagers);
				if (xmlNodes == null)
				{
					return string.Empty;
				}
				else
				{
					return xmlNodes.OuterXml;
				}
			}
		}

		private Stack<string> TokenizePathToStack(string path)
		{
			Stack<string> strs = new Stack<string>();
			string root = "";
			if (this.ExtendedDriveInfo != null)
			{
				root = this.ExtendedDriveInfo.Root;
			}
			while (!string.IsNullOrEmpty(path))
			{
				strs.Push(this.GetChildName(path));
				path = this.GetParentPath(path, root);
			}
			return strs;
		}

		private void Trace(DebugLogLevel type, string message)
		{
			if (type != DebugLogLevel.Error)
			{
				if (type != DebugLogLevel.Info)
				{
					DebugLogger.WriteLine(this._debugCategory, message);
					return;
				}
				else
				{
					DebugLogger.LogInfo(this._debugCategory, message);
					return;
				}
			}
			else
			{
				DebugLogger.LogError(this._debugCategory, message);
				return;
			}
		}

		private string ValidateAndNormalizePath(string path)
		{
			return this.ValidateAndNormalizePath(path, this.GetSessionInfo(base.DynamicParameters as ADProviderCommonParameters, base.Credential, this.ExtendedDriveInfo));
		}

		private string ValidateAndNormalizePath(string path, ADSessionInfo sessionInfo)
		{
			string x500Path;
			this.Trace(DebugLogLevel.Verbose, "Entering ValidateAndNormalizePath");
			if (this.IsValidPath(path))
			{
				path = ADProvider.RemoveAbsolutePathPrefix(path);
				ADProviderCommonParameters dynamicParameters = base.DynamicParameters as ADProviderCommonParameters;
				this.Trace(DebugLogLevel.Info, string.Format("ValidateAndNormalizePath: path = {0}", path));
				if (this.GetFormatType(dynamicParameters, this.ExtendedDriveInfo) != ADPathFormat.Canonical)
				{
					x500Path = path;
				}
				else
				{
					x500Path = this.ConvertToX500Path(path, sessionInfo);
				}
				this.Trace(DebugLogLevel.Verbose, string.Format("Leaving ValidateAndNormalizePath, path = {0}", x500Path));
				return x500Path;
			}
			else
			{
				base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(path), "InvalidPath", path));
				this.Trace(DebugLogLevel.Error, "Leaving ValidateAndNormalizePath: ArgumentException: path is invalid");
				return null;
			}
		}

		private bool ValidateDynamicParameters(string path, ADProviderCommonParameters parameters, ADSessionInfo sessionInfo)
		{
			bool flag;
			if (parameters != null)
			{
				try
				{
					parameters.ValidateParameters();
					goto Label0;
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					base.WriteError(ADUtilities.GetErrorRecord(argumentException, "ADProvider:ValidateDynamicParameters:InvalidArgument", path));
					this.Trace(DebugLogLevel.Error, "Leaving ValidateDynamicParameters: ArgumentException: InvalidArgument");
					flag = false;
				}
				return flag;
			}
			return true;
		Label0:
			if (this.GetHostType(parameters, this.ExtendedDriveInfo) == ADPathHostType.GC)
			{
				ADRootDSE rootDSE = this.GetRootDSE(sessionInfo);
				if (rootDSE.ServerType != ADServerType.ADLDS)
				{
					if (sessionInfo.EffectivePortNumber != LdapConstants.LDAP_GC_PORT && sessionInfo.EffectivePortNumber != LdapConstants.LDAP_SSL_GC_PORT)
					{
						base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(StringResources.ADProviderGCInvalidWithAppendedPort), "ADProvider:ValidateDynamicParameters:GC with appened server port which is not equal to the GC port ", path));
						this.Trace(DebugLogLevel.Error, "Leaving ValidateDynamicParameters: ArgumentException: GC with appened server port which is not equal to the GC port ");
						return false;
					}
					else
					{
						return true;
					}
				}
				else
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ArgumentException(StringResources.ADProviderGCInvalidForADLDS), "ADProvider:ValidateDynamicParameters:GC with ADLDS", path));
					this.Trace(DebugLogLevel.Error, "Leaving ValidateDynamicParameters: ArgumentException: GC with ADLDS");
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		private PSObject WrapADObjectInPSObject(ADObject adObj, ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			PSObject pSObject = new PSObject(adObj);
			ADProviderSearchParameters aDProviderSearchParameter = parameters as ADProviderSearchParameters;
			bool flag = true;
			if (aDProviderSearchParameter != null && aDProviderSearchParameter.Properties != null && (int)aDProviderSearchParameter.Properties.Length > 0)
			{
				flag = false;
			}
			if (flag)
			{
				if (this.GetFormatType(parameters, extendedDriveInfo) != ADPathFormat.Canonical)
				{
					pSObject.TypeNames.Add(string.Concat(adObj.GetType().ToString(), "#ProviderX500DefaultPropertySet"));
				}
				else
				{
					pSObject.TypeNames.Add(string.Concat(adObj.GetType().ToString(), "#ProviderCanonicalDefaultPropertySet"));
				}
			}
			return pSObject;
		}

		private void WriteADObject(ADObject adObj, ADSessionInfo sessionInfo, ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			adObj.SessionInfo = sessionInfo;
			if (this.GetFormatType(parameters, extendedDriveInfo) != ADPathFormat.Canonical)
			{
				this.WriteItemObjectWithAbsolutePath(this.WrapADObjectInPSObject(adObj, parameters, extendedDriveInfo), adObj.DistinguishedName, true);
				return;
			}
			else
			{
				if (!adObj.Contains("canonicalName"))
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ADException(string.Format(StringResources.ADProviderUnableToReadProperty, "canonicalName", adObj.DistinguishedName)), "ADProvider:WriteADObject:UnableToReadCanonicalName", adObj.DistinguishedName));
					this.Trace(DebugLogLevel.Error, string.Format("Leaving WriteADObject: Unable to read canonical name for object {0}.", adObj.DistinguishedName));
					return;
				}
				else
				{
					this.WriteItemObjectWithAbsolutePath(this.WrapADObjectInPSObject(adObj, parameters, extendedDriveInfo), (string)adObj["canonicalName"].Value, true);
					return;
				}
			}
		}

		private void WriteADObjectName(ADObject adObj, ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			if (this.GetFormatType(parameters, extendedDriveInfo) != ADPathFormat.Canonical)
			{
				this.WriteItemObjectWithAbsolutePath(this.GetChildName(adObj.DistinguishedName), adObj.DistinguishedName, true);
				return;
			}
			else
			{
				if (!adObj.Contains("canonicalName"))
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ADException(string.Format(StringResources.ADProviderUnableToReadProperty, "canonicalName", adObj.DistinguishedName)), "ADProvider:WriteADObjectName:UnableToReadCanonicalName", adObj.DistinguishedName));
					this.Trace(DebugLogLevel.Error, string.Format("Leaving WriteADObjectName: Unable to read canonical name for object {0}.", adObj.DistinguishedName));
					return;
				}
				else
				{
					string value = (string)adObj["canonicalName"].Value;
					this.WriteItemObjectWithAbsolutePath(this.GetChildName(value), value, true);
					return;
				}
			}
		}

		private void WriteADObjectProperties(ADObject adObj, PSObject properties, ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			if (this.GetFormatType(parameters, extendedDriveInfo) != ADPathFormat.Canonical)
			{
				this.WritePropertyObjectWithAbsolutePath(properties, adObj.DistinguishedName);
				return;
			}
			else
			{
				if (!adObj.Contains("canonicalName"))
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ADException(string.Format(StringResources.ADProviderUnableToReadProperty, "canonicalName", adObj.DistinguishedName)), "ADProvider:WriteADObjectProperties:UnableToReadCanonicalName", adObj.DistinguishedName));
					this.Trace(DebugLogLevel.Error, string.Format("Leaving WriteADObjectProperties: Unable to read canonical name for object {0}.", adObj.DistinguishedName));
					return;
				}
				else
				{
					string value = (string)adObj["canonicalName"].Value;
					this.WritePropertyObjectWithAbsolutePath(properties, value);
					return;
				}
			}
		}

		private void WriteADObjectSecurityDescriptor(ADObject adObj, ActiveDirectorySecurity ads, ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			if (this.GetFormatType(parameters, extendedDriveInfo) != ADPathFormat.Canonical)
			{
				this.WriteSecurityDescriptorObjectWithAbsolutePath(ads, adObj.DistinguishedName);
				return;
			}
			else
			{
				if (!adObj.Contains("canonicalName"))
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ADException(string.Format(StringResources.ADProviderUnableToReadProperty, "canonicalName", adObj.DistinguishedName)), "ADProvider:WriteADObjectSecurityDescriptor:UnableToReadCanonicalName", adObj.DistinguishedName));
					this.Trace(DebugLogLevel.Error, string.Format("Leaving WriteADObjectSecurityDescriptor: Unable to read canonical name for object {0}.", adObj.DistinguishedName));
					return;
				}
				else
				{
					string value = (string)adObj["canonicalName"].Value;
					this.WriteSecurityDescriptorObjectWithAbsolutePath(ads, value);
					return;
				}
			}
		}

		private void WriteADRootDSE(ADRootDSE rootDse, ADSessionInfo sessionInfo)
		{
			rootDse.SessionInfo = sessionInfo;
			rootDse.SessionInfo.ServerType = rootDse.ServerType;
			this.WriteItemObjectWithAbsolutePath(rootDse, "", true);
		}

		private void WriteADRootDSEChildName(ADObject adObj, ADProviderCommonParameters parameters, ADDriveInfo extendedDriveInfo)
		{
			if (this.GetFormatType(parameters, extendedDriveInfo) != ADPathFormat.Canonical)
			{
				this.WriteItemObjectWithAbsolutePath(adObj.DistinguishedName, adObj.DistinguishedName, true);
				return;
			}
			else
			{
				if (!adObj.Contains("canonicalName"))
				{
					base.WriteError(ADUtilities.GetErrorRecord(new ADException(string.Format(StringResources.ADProviderUnableToReadProperty, "canonicalName", adObj.DistinguishedName)), "ADProvider:WriteADRootDSEChildName:UnableToReadCanonicalName", adObj.DistinguishedName));
					this.Trace(DebugLogLevel.Error, string.Format("Leaving WriteADRootDSEChildName: Unable to read canonical name for object {0}.", adObj.DistinguishedName));
					return;
				}
				else
				{
					string value = (string)adObj["canonicalName"].Value;
					this.WriteItemObjectWithAbsolutePath(value, value, true);
					return;
				}
			}
		}

		private void WriteItemObjectWithAbsolutePath(object item, string path, bool isContainer)
		{
			base.WriteItemObject(item, ADProvider.AddAbsolutePathPrefix(path), isContainer);
		}

		private void WritePropertyObjectWithAbsolutePath(object propertyValue, string path)
		{
			base.WritePropertyObject(propertyValue, ADProvider.AddAbsolutePathPrefix(path));
		}

		private void WriteSecurityDescriptorObjectWithAbsolutePath(ObjectSecurity securityDescriptor, string path)
		{
			base.WriteSecurityDescriptorObject(securityDescriptor, ADProvider.AddAbsolutePathPrefix(path));
		}
	}
}