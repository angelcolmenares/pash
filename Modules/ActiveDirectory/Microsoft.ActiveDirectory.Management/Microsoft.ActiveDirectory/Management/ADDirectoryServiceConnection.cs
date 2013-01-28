using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.IMDA;
using Microsoft.ActiveDirectory.Management.Provider;
using Microsoft.ActiveDirectory.Management.WSE;
using Microsoft.ActiveDirectory.Management.WST;
using Microsoft.ActiveDirectory.CustomActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Principal;
using System.Text;
using System.Management.Automation;
using System.DirectoryServices;
using System.Collections.Specialized;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADDirectoryServiceConnection : IDisposable
	{
		private const string _debugCategory = "ADDirectoryServiceConnection";
		
		private int _debugInstance;
		
		private ADSessionInfo _sessionInfo;
		
		private string _serverName;
		
		private string _domainName;
		
		private int? _portNumber;
		
		private AuthType? _authenticationType;
		
		private bool? _autoReconnect;
		
		private string _instanceInfo;
		
		private bool _disposed;
		
		private AuthType AuthType
		{
			get
			{
				if (!this._authenticationType.HasValue)
				{
					if (this._sessionInfo.AuthType == AuthType.Negotiate || this._sessionInfo.AuthType == AuthType.Basic)
					{
						this._authenticationType = new AuthType?(this._sessionInfo.AuthType);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", string.Concat("AuthType is ", this._authenticationType.Value.ToString()));
					}
					else
					{
						throw new NotSupportedException(Enum.GetName(typeof(AuthType), this._sessionInfo.AuthType));
					}
				}
				return this._authenticationType.Value;
			}
		}
		
		private bool AutoReconnect
		{
			get
			{
				if (!this._autoReconnect.HasValue)
				{
					this._autoReconnect = new bool?(false);
					bool? autoReconnect = this._sessionInfo.Options.AutoReconnect;
					if (autoReconnect.HasValue)
					{
						bool? nullable = this._sessionInfo.Options.AutoReconnect;
						this._autoReconnect = new bool?(nullable.Value);
					}
					DebugLogger.WriteLineIf(this._autoReconnect.Value, "ADDirectoryServiceConnection", "AutoReconnect is enabled");
				}
				return this._autoReconnect.Value;
			}
		}
		
		private string InstanceInfo
		{
			get
			{
				if (this._instanceInfo == null)
				{
					int portNumber = this.PortNumber;
					this._serverName = this._sessionInfo.Server;
					this._instanceInfo = string.Concat("LDAP://", _serverName); //, ":", portNumber.ToString());
				}
				return this._instanceInfo;
			}
		}
		
		private bool IsGCPort
		{
			get
			{
				return this._sessionInfo.ConnectedToGC;
			}
		}
		
		private int PortNumber
		{
			get
			{
				if (!this._portNumber.HasValue)
				{
					this._portNumber = new int?(this._sessionInfo.EffectivePortNumber);
					int value = this._portNumber.Value;
					DebugLogger.WriteLine("ADDirectoryServiceConnection", string.Concat("PortNumber is ", value.ToString()));
				}
				return this._portNumber.Value;
			}
		}
		
		public string ServerName
		{
			get
			{
				return this._serverName;
			}
		}
		
		public ADSessionInfo SessionInfo
		{
			get
			{
				return this._sessionInfo;
			}
		}
		
		public ADDirectoryServiceConnection(ADSessionInfo info)
		{
			this._portNumber = null;
			this._authenticationType = null;
			this._autoReconnect = null;
			this._debugInstance = this.GetHashCode();
			object[] objArray = new object[1];
			objArray[0] = this._debugInstance;
			DebugLogger.WriteLine("ADDirectoryServiceConnection", "Constructor ADDirectoryServiceConnection 0x{0:X}", objArray);
			if (info != null)
			{
				this._sessionInfo = info;
				return;
			}
			else
			{
				DebugLogger.LogWarning("ADDirectoryServiceConnection", "Constructor(ADDirectoryServiceConnection) called with null info");
				throw new ArgumentNullException("info");
			}
		}
		
		public void AbandonSearch(ADSearchRequest request)
		{
			if (request == null || request.Controls == null)
			{
				return;
			}
			else
			{
				string cookie = null;
				int num = 0;
				while (num < request.Controls.Count)
				{
					ADPageResultRequestControl item = request.Controls[num] as ADPageResultRequestControl;
					if (item == null)
					{
						num++;
					}
					else
					{
						cookie = item.Cookie as string;
						break;
					}
				}
				this.ReleaseEnumerationContext(cookie);
				return;
			}
		}
		
		public ChangeOptionalFeatureResponse ChangeOptionalFeature(ChangeOptionalFeatureRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			ChangeOptionalFeatureResponse changeOptionalFeatureResponse = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: ChangeOptionalFeatureRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tDistinguishedName: {0}", request.DistinguishedName);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tEnable: {0}", request.Enable);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tFeatureId: {0}", request.FeatureId);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: changeOptionalFeatureResponse = this._topoMgmt.ChangeOptionalFeature(request);
					flag = false;
					object[] objArray = new object[1];
					objArray[0] = this._debugInstance;
					DebugLogger.WriteLine("ADDirectoryServiceConnection", "ADDirectoryServiceConnection 0x{0:X}: Got ChangeOptionalFeatureResponse", objArray);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return changeOptionalFeatureResponse;
		}
		
		public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			ChangePasswordResponse changePasswordResponse = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: ChangePasswordRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tAccountDN: {0}", request.AccountDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: changePasswordResponse = this._acctMgmt.ChangePassword(request);
					flag = false;
					object[] objArray = new object[1];
					objArray[0] = this._debugInstance;
					DebugLogger.WriteLine("ADDirectoryServiceConnection", "ADDirectoryServiceConnection 0x{0:X}: Got ChangePasswordResponse", objArray);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return changePasswordResponse;
		}
		
		private static void CommonCatchAll(Exception exception)
		{
			TimeoutException timeoutException = exception as TimeoutException;
			if (timeoutException == null)
			{
				throw new ADException(exception.Message, exception);
			}
			else
			{
				throw new TimeoutException(StringResources.TimeoutError, timeoutException);
			}
		}
		
		private string ConvertDCLocatorFlagsToString(uint dcLocatorFlags)
		{
			string str = "";
			char[] chrArray = new char[2];
			chrArray[0] = '|';
			chrArray[1] = ' ';
			char[] chrArray1 = chrArray;
			if ((dcLocatorFlags & 1) != 0)
			{
				str = string.Concat(str, "ForceRediscover | ");
			}
			if ((dcLocatorFlags & 16) != 0)
			{
				str = string.Concat(str, "MinimumDirectoryServiceVersion:Windows2000 | ");
			}
			if ((dcLocatorFlags & 32) != 0)
			{
				str = string.Concat(str, "DirectoryServicesPreferred | ");
			}
			if ((dcLocatorFlags & 64) != 0)
			{
				str = string.Concat(str, "GlobalCatalog | ");
			}
			if ((dcLocatorFlags & 128) != 0)
			{
				str = string.Concat(str, "PrimaryDC | ");
			}
			if ((dcLocatorFlags & 0x200) != 0)
			{
				str = string.Concat(str, "IpRequired | ");
			}
			if ((dcLocatorFlags & 0x400) != 0)
			{
				str = string.Concat(str, "KDC | ");
			}
			if ((dcLocatorFlags & 0x2000) != 0)
			{
				str = string.Concat(str, "ReliableTimeService | ");
			}
			if ((dcLocatorFlags & 0x800) != 0)
			{
				str = string.Concat(str, "TimeService | ");
			}
			if ((dcLocatorFlags & 0x1000) != 0)
			{
				str = string.Concat(str, "Writable | ");
			}
			if ((dcLocatorFlags & 0x4000) != 0)
			{
				str = string.Concat(str, "AvoidSelf | ");
			}
			if ((dcLocatorFlags & 0x8000) != 0)
			{
				str = string.Concat(str, "OnlyLdapNeeded | ");
			}
			if ((dcLocatorFlags & 0x10000) != 0)
			{
				str = string.Concat(str, "IsFlatName | ");
			}
			if ((dcLocatorFlags & 0x20000) != 0)
			{
				str = string.Concat(str, "IsDnsName | ");
			}
			if ((dcLocatorFlags & 0x40000) != 0)
			{
				str = string.Concat(str, "NextClosestSite | ");
			}
			if ((dcLocatorFlags & 0x80000) != 0)
			{
				str = string.Concat(str, "MinimumDirectoryServiceVersion:Windows2008 | ");
			}
			if ((dcLocatorFlags & 0x200000) != 0)
			{
				str = string.Concat(str, "MinimumDirectoryServiceVersion:Windows2012 | ");
			}
			if ((dcLocatorFlags & 0x100000) != 0)
			{
				str = string.Concat(str, "ADWS | ");
			}
			if ((dcLocatorFlags & 0x40000000) != 0)
			{
				str = string.Concat(str, "ReturnDnsName | ");
			}
			if ((dcLocatorFlags & -2147483648) != 0)
			{
				str = string.Concat(str, "ReturnFlatName | ");
			}
			if (!string.IsNullOrEmpty(str))
			{
				str = str.TrimEnd(chrArray1);
			}
			return str;
		}
		
		public ADAddResponse Create(ADAddRequest request)
		{
			int num = X500Path.IndexOfFirstDelimiter(request.DistinguishedName);
			if (num != -1)
			{
				string str = request.DistinguishedName.Substring(0, num);
				string str1 = request.DistinguishedName.Substring(num + 1);
				DirectoryAttribute[] directoryAttributeArray = null;
				if (request.Attributes.Count > 0)
				{
					directoryAttributeArray = new DirectoryAttribute[request.Attributes.Count];
					request.Attributes.CopyTo(directoryAttributeArray, 0);
				}
				bool flag = false;
				do
				{
					ADCreateRequestMsg aDCreateRequestMsg = new ADCreateRequestMsg(this.InstanceInfo, str1, str, ADDirectoryServiceConnection.CreateControlArray(request.Controls), directoryAttributeArray);
					try
					{
						try
						{
							if (DebugLogger.Level < DebugLogLevel.Verbose)
							{
								//SVC CALL: message = this._resFactory.Create(aDCreateRequestMsg);
							}
							else
							{

							}
							flag = false;
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							ADDirectoryServiceConnection.CommonCatchAll(exception);
						}
					}
					finally
					{

					}
				}
				while (flag);

				ADCreateResponseMsg aDCreateResponseMsg = new ADCreateResponseMsg();
				ADAddResponse aDAddResponse = new ADAddResponse(request.DistinguishedName, ADDirectoryServiceConnection.CreateArray<DirectoryControl>(aDCreateResponseMsg.Controls), ResultCode.Success, null);
				return aDAddResponse;
			}
			else
			{
				throw new ArgumentException(request.DistinguishedName, "distinguishedName");
			}
		}
		
		private static T[] CreateArray<T>(ICollection<T> collection)
		{
			T[] tArray = null;
			if (collection.Count > 0)
			{
				tArray = new T[collection.Count];
				collection.CopyTo(tArray, 0);
			}
			return tArray;
		}

		private static DirectoryControl[] CreateControlArray(ICollection controls)
		{
			DirectoryControl[] directoryControlArray = null;
			if (controls.Count > 0)
			{
				directoryControlArray = new DirectoryControl[controls.Count];
				controls.CopyTo(directoryControlArray, 0);
			}
			return directoryControlArray;
		}
		
		private string CreateEndpointAddress(string configName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!this._sessionInfo.Connectionless)
			{
				stringBuilder.Append("LDAP://");
				this.DiscoverServerName(false);
				stringBuilder.Append(this._serverName);
				stringBuilder.Append("/");
				return stringBuilder.ToString ();
			}
			else
			{
				throw new NotSupportedException();
			}
		}
		
		private string CreateEnumerationContext(ADSearchRequest request, IList<string> attributes)
		{
			bool flag = false;
			do
			{
				ADEnumerateLdapRequest aDEnumerateLdapRequest = new ADEnumerateLdapRequest(this.InstanceInfo, (string)request.Filter, request.DistinguishedName, request.Scope.ToString(), attributes);
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{

						}
						else
						{

						}
						flag = false;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
			}
			while (flag);

			ADEnumerateLdapResponse aDEnumerateLdapResponse = new ADEnumerateLdapResponse();
			return aDEnumerateLdapResponse.EnumerationContext ?? "default";
		}
		
		public ADDeleteResponse Delete(ADDeleteRequest request)
		{
			bool flag = false;
			do
			{
				ADDeleteRequestMsg aDDeleteRequestMsg = new ADDeleteRequestMsg(this.InstanceInfo, request.DistinguishedName, ADDirectoryServiceConnection.CreateControlArray(request.Controls));
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							//SVC CALL: message = this._resource.Delete(aDDeleteRequestMsg);
						}
						else
						{

						}
						flag = false;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
			}
			while (flag);

			ADDeleteResponseMsg aDDeleteResponseMsg = new ADDeleteResponseMsg();
			ADDeleteResponse aDDeleteResponse = new ADDeleteResponse(request.DistinguishedName, ADDirectoryServiceConnection.CreateArray<DirectoryControl>(aDDeleteResponseMsg.Controls), ResultCode.Success, null);
			return aDDeleteResponse;
		}
		
		private void DiscoverServerName(bool forceDiscovery)
		{
			if (forceDiscovery || this._serverName == null)
			{
				this._serverName = null;
				this._domainName = null;
				if (this._sessionInfo.Server != null)
				{
					this._serverName = this._sessionInfo.ServerNameOnly;
					if (this._sessionInfo.FullyQualifiedDnsHostName)
					{
						return;
					}
				}
				IntPtr zero = IntPtr.Zero;
				uint num = Convert.ToUInt32(ADLocatorFlags.WebServiceRequired | ADLocatorFlags.ReturnDnsName);
				if (this.IsGCPort)
				{
					num = num | Convert.ToUInt32(ADLocatorFlags.GCRequired);
				}
				if (forceDiscovery)
				{
					DebugLogger.WriteLineIf(forceDiscovery, "ADDirectoryServiceConnection", "Forcing rediscovery of server name");
					num = num | Convert.ToUInt32(ADLocatorFlags.ForceRediscovery);
				}
				ADLocatorFlags? locatorFlag = this._sessionInfo.Options.LocatorFlag;
				if (!locatorFlag.HasValue)
				{
					num = num | Convert.ToUInt32(ADLocatorFlags.DirectoryServicesRequired);
				}
				else
				{
					ADLocatorFlags? nullable = this._sessionInfo.Options.LocatorFlag;
					num = num | (uint)nullable.GetValueOrDefault ();
				}
				int num1 = 0;
				try
				{
					object[] objArray = new object[2];
					objArray[0] = this._serverName;
					objArray[1] = num;
					DebugLogger.WriteLine("ADDirectoryServiceConnection", "calling DsGetDcName for server {0} with flags {1}", objArray);
					if (OSHelper.IsUnix)
					{
						this._serverName = "192.168.1.20"; //TODO: REPLACE!!
					}
					else {
						num1 = UnsafeNativeMethods.DsGetDcName(null, this._serverName, 0, null, num, out zero);
						if (num1 != 0)
						{
							if (num1 == 0x3ec)
							{
								object[] objArray1 = new object[1];
								objArray1[0] = num;
								DebugLogger.LogWarning("ADDirectoryServiceConnection", "DsGetDCName returned invalid flags error for input: {0}", objArray1);
							}
						}
						else
						{
							DOMAIN_CONTROLLER_INFO structure = (DOMAIN_CONTROLLER_INFO)Marshal.PtrToStructure(zero, typeof(DOMAIN_CONTROLLER_INFO));
							if (!structure.DomainControllerName.StartsWith("\\\\"))
							{
								this._serverName = structure.DomainControllerName;
							}
							else
							{
								this._serverName = structure.DomainControllerName.Substring(2);
							}
							this._domainName = structure.DomainName;
						}
					}
				}
				finally
				{
					if (OSHelper.IsWindows) UnsafeNativeMethods.NetApiBufferFree(zero);
				}
				if (!string.IsNullOrEmpty(this._serverName))
				{
					return;
				}
				else
				{
					string str = this.ConvertDCLocatorFlagsToString(num);
					object[] objArray2 = new object[1];
					objArray2[0] = str;
					throw new ADServerDownException(StringResources.DefaultADWSServerNotFound, new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.DefaultServerNotFound, objArray2), num1), null, num1);
				}
			}
			else
			{
				return;
			}
		}
		
		public void Dispose()
		{
			/*
			object[] objArray = new object[1];
			objArray[0] = this._debugInstance;
			DebugLogger.WriteLine("ADDirectoryServiceConnection", "ADDirectoryServiceConnection Dispose 0x{0:X}", objArray);
			this.Dispose(true);
			GC.SuppressFinalize(this);
			*/
		}
		
		private void Dispose(bool disposing)
		{
			if (disposing)
			{

			}
			this._disposed = true;
		}

		~ADDirectoryServiceConnection()
		{
			try
			{
				object[] objArray = new object[1];
				objArray[0] = this._debugInstance;
				DebugLogger.WriteLine("ADDirectoryServiceConnection", "Destructor ADDirectoryServiceConnection 0x{0:X}", objArray);
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		public string GetDefaultNamingContext ()
		{
			var entry = CreateDirectoryEntry (CreateEndpointAddress (""));
			return (string)entry.Properties["defaultNamingContext"].Value;
		}
		
		public GetADDomainResponse GetADDomain(GetADDomainRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			GetADDomainResponse aDDomain = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADDomainRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}

					var domain = new ActiveDirectoryDomain();
					var reqDomain = new ADSearchRequest(GetDefaultNamingContext (), "(&(objectClass=domainDNS))", System.DirectoryServices.Protocols.SearchScope.Base, new string[] { "*" });
					var domainResp = SearchAnObject (reqDomain);
					var entryObj = domainResp.Entries.ElementAt (0);

					var rootDomain = GetDirectoryEntry (entryObj.DistinguishedName);
					domain.Name = (string)rootDomain.Properties["name"][0];

					/*  Child Domains */
					var childDomains = new List<string>();
					var domainSearcher = new DirectorySearcher(rootDomain, "(&(objectClass=domain))");
					var respChild = domainSearcher.FindAll ();
					foreach(SearchResult childEntry in respChild)
					{
						string domainName =  (string)childEntry.Properties["distinguishedName"][0];
						if (!domainName.Equals ((string)rootDomain.Properties["distinguishedName"].Value, StringComparison.OrdinalIgnoreCase))
						{
							childDomains.Add (domainName);
						}
					}
					domain.ChildDomains = childDomains.ToArray();

					/* NETBIOS Name */
					var partitionsEntry = GetDirectoryEntry (string.Concat ("CN=Partitions,CN=Configuration,", entryObj.DistinguishedName));
					var biosSearcher = new DirectorySearcher(partitionsEntry, "netbiosname=*", new string[] { "cn" });
					var respBios = biosSearcher.FindOne ();
					if (respBios != null) {
						domain.NetBIOSName =  (string)respBios.Properties["cn"][0];
					}

					/* Parent Domain */ /* TODO */
					/*
					var parentDomain = rootDomain.Parent;
					var parentDomainPath = parentDomain.Path;
					if (parentDomain != null)
					{
						var parentName = parentDomain.Properties["distinguishedName"];
						if  (parentName != null)
						domain.ParentDomain = (string)parentName.Value;
					}
					 */
					domain.ParentDomain  = "";

					/*  Linked Policies */
					var linkedPolicies = new List<string>();
					var policySearcher = new DirectorySearcher(rootDomain, "(&(objectClass=groupPolicyContainer))");
					var respPolicies = policySearcher.FindAll ();
					foreach(SearchResult childEntry in respPolicies)
					{
						linkedPolicies.Add ( (string)childEntry.Properties["distinguishedName"][0]);
					}
					domain.AppliedGroupPolicies = linkedPolicies.ToArray();

					/*  Replica Server */
					var replServers = new List<string>();
					var configEntry = GetDirectoryEntry (string.Concat ("CN=Configuration,", entryObj.DistinguishedName));
					var rplSearcher = new DirectorySearcher(configEntry, "(&(objectClass=server))");
					var rplObjects = rplSearcher.FindAll ();
					foreach(SearchResult childEntry in rplObjects)
					{
						replServers.Add ( (string)childEntry.Properties["dNSHostName"][0]);
					}
					domain.ReplicaDirectoryServer = replServers.ToArray();
					domain.ReadOnlyReplicaDirectoryServer = new string[0];

					/* PDC Master */
					string pdcSite = (string)rootDomain.Properties["fSMORoleOwner"].Value;
					DirectoryEntry pdcItem = GetDirectoryEntry(pdcSite);
					if (pdcItem != null) {
						var parentPdcItem = pdcItem.Parent;
						string parentpdcItemPath = parentPdcItem.Path;
						domain.PDCEmulator = (string)parentPdcItem.Properties["dNSHostName"].Value;
					}
					domain.ReferenceServer = domain.PDCEmulator;

					/* Infrastrucutre Master */
					var infraChild = new ADSearchRequest(entryObj.DistinguishedName, "(&(objectClass=infrastructureUpdate))", System.DirectoryServices.Protocols.SearchScope.Subtree, new string[] { "*" });
					var infraResp = SearchAnObject (infraChild);
					var infraEntry = infraResp.Entries.FirstOrDefault ();
					if (infraEntry != null)
					{
						string infraSite = (string)infraEntry.GetValue ("fSMORoleOwner");
						DirectoryEntry infraItem = GetDirectoryEntry(infraSite);
						if (infraItem != null) {
							var parentInfraItem = infraItem.Parent;
							string parentInfraItemPath = parentInfraItem.Path;
							domain.InfrastructureMaster = (string)parentInfraItem.Properties["dNSHostName"].Value;
						}
					}
					
					/* RID Master */
					var ridChild = new ADSearchRequest(entryObj.DistinguishedName, "(&(objectClass=rIDManager))", System.DirectoryServices.Protocols.SearchScope.Subtree, new string[] { "*" });
					var ridResp = SearchAnObject (ridChild);
					var ridEntry = ridResp.Entries.FirstOrDefault ();
					if (ridEntry != null)
					{
						string ridSite = (string)infraEntry.GetValue ("fSMORoleOwner");
						DirectoryEntry ridItem = GetDirectoryEntry(ridSite);
						if (ridItem != null) {
							var parentRIDItem = ridItem.Parent;
							string parentRIDItemPath = parentRIDItem.Path;
							domain.RIDMaster = (string)parentRIDItem.Properties["dNSHostName"].Value;
						}
					}


					string[] allowedDNSSuffix = (string[])entryObj["ms-DS-Allowed-DNS-Suffixes"].Value;

					string partitionDN = entryObj.DistinguishedName;
					string computerPath = Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.ComputersContainerGuid);
					string usersPath = Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.UsersContainerGuid);
					string foreignPath = Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.ForeignSecurityPrincipalContainerGuid);
					string quotasPath = Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.NtdsQuotasContainerGuid);
					string systemPath = Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.SystemsContainerGuid);
					string controllerPath = Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.DCContainerGuid);
					string lostPath = string.Concat ("CN=LostAndFound,",partitionDN); // Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.LostAndFoundContainerGuid);
					string deletedPath = string.Concat ("CN=Deleted Objects,",partitionDN); // Microsoft.ActiveDirectory.Management.Commands.Utils.GetWellKnownGuidDN(this._sessionInfo, partitionDN, Microsoft.ActiveDirectory.Management.Commands.WellKnownGuids.DeletedObjectsContainerGuid);

					int i = 0;
					domain.ObjectGuid = entryObj.ObjectGuid.GetValueOrDefault ();
					domain.DistinguishedName = entryObj.DistinguishedName;
					domain.ObjectClass =  entryObj.ObjectClass;
					domain.DomainSID = (byte[])entryObj.GetValue ("objectSid");
					domain.AllowedDNSSuffixes = allowedDNSSuffix;
					int domainMode = 0;
					if (int.TryParse ((string)entryObj["msDS-Behavior-Version"].Value, out domainMode))
					{
						domain.DomainMode = domainMode;
					}
					domain.ManagedBy = (string)entryObj["managedBy"].Value;
					var replInternal = (string)entryObj["msDS-LogonTimeSyncInterval"].Value;
					if (!string.IsNullOrEmpty (replInternal)) {
						domain.LastLogonReplicationInterval = new TimeSpan?(new TimeSpan(long.Parse (replInternal)));
					}
					domain.SubordinateReferences = (string[])entryObj["subRefs"].Value;
					domain.DNSRoot = (string)entryObj["dnsRoot"].Value;
					domain.LostAndFoundContainer = lostPath;
					domain.DeletedObjectsContainer = deletedPath;
					domain.QuotasContainer = quotasPath;
	                domain.ComputersContainer = computerPath;
					domain.DomainControllersContainer = controllerPath;
	                domain.ForeignSecurityPrincipalsContainer = foreignPath;
					domain.SystemsContainer = systemPath;
					domain.UsersContainer = usersPath;
					aDDomain = new GetADDomainResponse(domain);

					//SVC CALL: aDDomain = this._topoMgmt.GetADDomain(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADDomainResponse Domain", this._debugInstance);
						if (aDDomain.Domain != null)
						{
							stringBuilder1.AppendLine();
							stringBuilder1.AppendFormat("\tName:{0} DN:{1}", aDDomain.Domain.Name, aDDomain.Domain.DistinguishedName);
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder1.ToString());
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDDomain;
		}

		
		public GetADDomainControllerResponse GetADDomainController(GetADDomainControllerRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			GetADDomainControllerResponse aDDomainController = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADDomainControllerRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.Append("\tDCName: ");
						if (request.NtdsSettingsDN != null)
						{
							string[] ntdsSettingsDN = request.NtdsSettingsDN;
							for (int i = 0; i < (int)ntdsSettingsDN.Length; i++)
							{
								string str = ntdsSettingsDN[i];
								stringBuilder.AppendLine();
								stringBuilder.AppendFormat("\t\t{0}", str);
							}
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: aDDomainController = this._topoMgmt.GetADDomainController(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADDomainControllerResponse DomainControllers", this._debugInstance);
						if (aDDomainController.DomainControllers != null)
						{
							ActiveDirectoryDomainController[] domainControllers = aDDomainController.DomainControllers;
							for (int j = 0; j < (int)domainControllers.Length; j++)
							{
								ActiveDirectoryDomainController activeDirectoryDomainController = domainControllers[j];
								stringBuilder1.AppendLine();
								stringBuilder1.AppendFormat("\tHostName:{0}", activeDirectoryDomainController.HostName);
								stringBuilder1.AppendFormat("\t\tName:{0}", activeDirectoryDomainController.Name);
								stringBuilder1.AppendFormat("\t\tDomain:{0}", activeDirectoryDomainController.Domain);
								stringBuilder1.AppendFormat("\t\tForest:{0}", activeDirectoryDomainController.Forest);
							}
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder1.ToString());
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDDomainController;
		}
		
		public GetADForestResponse GetADForest(GetADForestRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			GetADForestResponse aDForest = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADForestRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					var forest = new ActiveDirectoryForest();
					aDForest = new GetADForestResponse(forest);
					var context = new System.DirectoryServices.ActiveDirectory.DirectoryContext(System.DirectoryServices.ActiveDirectory.DirectoryContextType.Forest, request.Server, this._sessionInfo.Credential.UserName, GetPassword (this._sessionInfo.Credential.Password));
					System.DirectoryServices.ActiveDirectory.Forest f = System.DirectoryServices.ActiveDirectory.Forest.GetForest (context);
					forest.Name = f.Name;
					forest.RootDomain = f.RootDomain.Name;
					string[] domainNames = new string[f.Domains.Count];
					int i = 0;
					foreach(System.DirectoryServices.ActiveDirectory.Domain d in f.Domains)
					{
						domainNames[i] = d.Name;
						i++;
					}
					forest.Domains = domainNames;
					i = 0;
					string[] siteNames = new string[f.Sites.Count];
					foreach(System.DirectoryServices.ActiveDirectory.ActiveDirectorySite site in f.Sites)
					{
						siteNames[i] = site.Name;
						i++;
					}
					forest.Sites = siteNames;
					string[] partitions = new string[f.ApplicationPartitions.Count];
					i = 0;
					foreach(System.DirectoryServices.ActiveDirectory.ApplicationPartition part in f.ApplicationPartitions)
					{
						partitions[i] = part.Name;
						i++;
					}
					forest.DomainNamingMaster = f.SchemaRoleOwner.Name;
					forest.SchemaMaster = f.SchemaRoleOwner.Name;

					forest.ApplicationPartitions = partitions;
					forest.ForestMode = (int)f.ForestMode;
					string[] globalCatalogNames = new string[f.GlobalCatalogs.Count];
					i = 0;
					foreach(System.DirectoryServices.ActiveDirectory.GlobalCatalog globalCatalog in f.GlobalCatalogs)
					{
						globalCatalogNames[i] = globalCatalog.Name;
						i++;
					}
					forest.GlobalCatalogs = globalCatalogNames;
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADForestResponse Forest", this._debugInstance);
						if (aDForest.Forest != null)
						{
							stringBuilder1.AppendLine();
							stringBuilder1.AppendFormat("\tName: {0}", aDForest.Forest.Name);
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder1.ToString());
					}
				}
				catch (Exception exception1)

				{

					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDForest;
		}

		private void AddGroupMembers (DirectoryEntry entry, ref List<ActiveDirectoryPrincipal> members, bool recursive)
		{
			foreach (object o in (object[])entry.Properties ["member"].Value) 
			{
				var member = new ActiveDirectoryPrincipal ();
				string path = (string)o;
				var memberEntry = GetDirectoryEntry (path);
				member.DistinguishedName = (string)memberEntry.Properties ["distinguishedName"].Value;
				member.SID = (byte[])memberEntry.Properties ["objectSid"].Value;
				member.Name = (string)memberEntry.Properties ["name"].Value;
				if (members.Exists(x => x.DistinguishedName == member.DistinguishedName)) continue;
				var classObj = memberEntry.Properties ["distinguishedName"];
				member.ObjectClass = (string)classObj [classObj.Count - 1];
				member.ObjectGuid = new Guid ((byte[])memberEntry.Properties ["objectGUID"].Value);
				member.ObjectTypes = new string[0];
				member.SamAccountName = (string)memberEntry.Properties ["samAccountName"].Value;
				member.ReferenceServer = _serverName;
				members.Add (member);
				if (recursive && member.ObjectClass == "group") 
				{
					AddGroupMembers(memberEntry, ref members, recursive);
				}
			}
		}
		
		public GetADGroupMemberResponse GetADGroupMember(GetADGroupMemberRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			GetADGroupMemberResponse aDGroupMember = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADGroupMemberRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tGroupDN: {0}", request.GroupDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tRecursive: {0}", request.Recursive);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					var entry = GetDirectoryEntry (request.GroupDN);
					var items = new List<ActiveDirectoryPrincipal>();
					AddGroupMembers (entry, ref items, request.Recursive);
					aDGroupMember = new GetADGroupMemberResponse(items.OrderBy (x => x.ObjectClass).ToArray());
					//SVC CALL: aDGroupMember = this._acctMgmt.GetADGroupMember(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADGroupMemberResponse Members", this._debugInstance);
						if (aDGroupMember.Members != null)
						{
							ActiveDirectoryPrincipal[] members = aDGroupMember.Members;
							for (int i = 0; i < (int)members.Length; i++)
							{
								ActiveDirectoryPrincipal activeDirectoryPrincipal = members[i];
								stringBuilder1.AppendLine();
								stringBuilder1.AppendFormat("\tDN: {0}", activeDirectoryPrincipal.DistinguishedName);
							}
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder1.ToString());
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDGroupMember;
		}
		
		public GetADPrincipalAuthorizationGroupResponse GetADPrincipalAuthorizationGroup(GetADPrincipalAuthorizationGroupRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			GetADPrincipalAuthorizationGroupResponse aDPrincipalAuthorizationGroup = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADPrincipalAuthorizationGroupRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPrincipalDN: {0}", request.PrincipalDN);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: aDPrincipalAuthorizationGroup = this._acctMgmt.GetADPrincipalAuthorizationGroup(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADPrincipalAuthorizationGroupResponse MemberOf", this._debugInstance);
						if (aDPrincipalAuthorizationGroup.MemberOf != null)
						{
							ActiveDirectoryGroup[] memberOf = aDPrincipalAuthorizationGroup.MemberOf;
							for (int i = 0; i < (int)memberOf.Length; i++)
							{
								ActiveDirectoryGroup activeDirectoryGroup = memberOf[i];
								stringBuilder1.AppendLine();
								stringBuilder1.AppendFormat("\tDN:{0} GroupScope:{1} GroupType:{2}", activeDirectoryGroup.DistinguishedName, activeDirectoryGroup.GroupScope, activeDirectoryGroup.GroupType);
							}
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder1.ToString());
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDPrincipalAuthorizationGroup;
		}
		
		public GetADPrincipalGroupMembershipResponse GetADPrincipalGroupMembership(GetADPrincipalGroupMembershipRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			GetADPrincipalGroupMembershipResponse aDPrincipalGroupMembership = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADPrincipalGroupMembershipRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPrincipalDN: {0}", request.PrincipalDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tResourceContextPartition: {0}", request.ResourceContextPartition);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: aDPrincipalGroupMembership = this._acctMgmt.GetADPrincipalGroupMembership(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: GetADPrincipalGroupMembershipResponse MemberOf", this._debugInstance);
						if (aDPrincipalGroupMembership.MemberOf != null)
						{
							ActiveDirectoryGroup[] memberOf = aDPrincipalGroupMembership.MemberOf;
							for (int i = 0; i < (int)memberOf.Length; i++)
							{
								ActiveDirectoryGroup activeDirectoryGroup = memberOf[i];
								stringBuilder1.AppendLine();
								stringBuilder1.AppendFormat("\tDN:{0} GroupScope:{1} GroupType:{2}", activeDirectoryGroup.DistinguishedName, activeDirectoryGroup.GroupScope, activeDirectoryGroup.GroupType);
							}
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder1.ToString());
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDPrincipalGroupMembership;
		}

		
		public ADModifyResponse Modify(ADModifyRequest request)
		{
			ADPutRequestMsg aDPutRequestMsg;
			bool flag = false;
			DirectoryAttributeModification[] directoryAttributeModificationArray = null;
			if (request.Modifications.Count > 0)
			{
				directoryAttributeModificationArray = new DirectoryAttributeModification[request.Modifications.Count];
				request.Modifications.CopyTo(directoryAttributeModificationArray, 0);
			}
			if (string.IsNullOrEmpty(request.DistinguishedName))
			{
				flag = true;
			}
			bool flag1 = false;
			do
			{
				if (!flag)
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, request.DistinguishedName, ADDirectoryServiceConnection.CreateControlArray(request.Controls), directoryAttributeModificationArray);
				}
				else
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, "11111111-1111-1111-1111-111111111111", ADDirectoryServiceConnection.CreateControlArray(request.Controls), directoryAttributeModificationArray);
				}
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{

						}
						else
						{

						}
						flag1 = false;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
			}
			while (flag1);
			ADPutResponseMsg aDPutResponseMsg = new ADPutResponseMsg();
			ADModifyResponse aDModifyResponse = new ADModifyResponse(request.DistinguishedName, ADDirectoryServiceConnection.CreateArray<DirectoryControl>(aDPutResponseMsg.Controls), ResultCode.Success, null);
			return aDModifyResponse;
		}
		
		public ADModifyDNResponse ModifyDN(ADModifyDNRequest request)
		{
			ADPutRequestMsg aDPutRequestMsg;
			bool flag = false;
			do
			{
				if (!string.IsNullOrEmpty(request.NewParentDistinguishedName))
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, request.DistinguishedName, ADDirectoryServiceConnection.CreateControlArray(request.Controls), request.NewName, request.NewParentDistinguishedName);
				}
				else
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, request.DistinguishedName, ADDirectoryServiceConnection.CreateControlArray(request.Controls), request.NewName);
				}
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{

						}
						else
						{

						}
						flag = false;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
			}
			while (flag);

			ADPutResponseMsg aDPutResponseMsg = new ADPutResponseMsg();
			ADModifyDNResponse aDModifyDNResponse = new ADModifyDNResponse(request.DistinguishedName, ADDirectoryServiceConnection.CreateArray<DirectoryControl>(aDPutResponseMsg.Controls), ResultCode.Success, null);
			return aDModifyDNResponse;
		}
		
		public MoveADOperationMasterRoleResponse MoveADOperationMasterRole(MoveADOperationMasterRoleRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			MoveADOperationMasterRoleResponse moveADOperationMasterRoleResponse = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: MoveADOperationMasterRoleRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tOperationMasterRole: {0}", request.OperationMasterRole);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tSeize: {0}", request.Seize);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: moveADOperationMasterRoleResponse = this._topoMgmt.MoveADOperationMasterRole(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: MoveADOperationMasterRoleResponse", this._debugInstance);
						stringBuilder1.AppendLine();
						stringBuilder1.AppendFormat("\tWasSeized: {0}", moveADOperationMasterRoleResponse.WasSeized);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return moveADOperationMasterRoleResponse;
		}
		
		private static int ParseErrorCode(string sErrorCode)
		{
			int num = int.Parse(sErrorCode, NumberFormatInfo.InvariantInfo);
			string str = string.Format("{0:X}", num);
			if (str.StartsWith("8007"))
			{
				str = str.Substring("8007".Length);
				int num1 = 0;
				if (int.TryParse(str, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out num1))
				{
					num = num1;
				}
			}
			return num;
		}
		
		public void ReleaseEnumerationContext(string enumContext)
		{
			if (!string.IsNullOrEmpty(enumContext))
			{
				ADReleaseRequest aDReleaseRequest = new ADReleaseRequest(this.InstanceInfo, enumContext);
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{

						}
						else
						{

						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
				if (DebugLogger.Level >= DebugLogLevel.Warning)
				{
					DebugLogger.WriteLine("ADDirectoryServiceConnection", "Release response message:");
				}
				return;
			}
			else
			{
				return;
			}
		}
		
		public ADSearchResponse Search(ADSearchRequest request)
		{
			ADSearchResponse aDSearchResponse;
			ADSearchResponse aDSearchResponse1 = null;
			bool flag = false;
			bool flag1 = false;
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (request.Attributes.Count != 0)
			{
				foreach (string attribute in request.Attributes)
				{
					if (string.Compare(attribute, ADObjectSearcher.AllProperties, StringComparison.OrdinalIgnoreCase) != 0)
					{
						flag1 = true;
						strs.Add(attribute);
					}
					else
					{
						flag = true;
					}
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				strs.Add("distinguishedName");
			}
			IList<string> strs1 = new List<string>(strs);
			if (request.Scope == System.DirectoryServices.Protocols.SearchScope.Base)
			{
				if (!string.IsNullOrEmpty(request.DistinguishedName))
				{
					if (ADObjectSearcher.IsDefaultSearchFilter((string)request.Filter) && request.ObjectScopedControls)
					{
						if (flag)
						{
							aDSearchResponse1 = this.SearchAnObject(request);
						}
						if (flag1)
						{
							if (aDSearchResponse1 == null || aDSearchResponse1.Entries.Count <= 0)
							{
								aDSearchResponse1 = this.SearchAnObject(request, strs1);
							}
							else
							{
								aDSearchResponse = this.SearchAnObject(request, strs1);
								if (aDSearchResponse.Entries.Count > 0)
								{
									ADObject item = aDSearchResponse.Entries[0];
									foreach (string propertyName in item.PropertyNames)
									{
										aDSearchResponse1.Entries[0].SetValue(propertyName, item[propertyName]);
									}
								}
							}
						}
						return aDSearchResponse1;
					}
				}
				else
				{
					if (flag)
					{
						aDSearchResponse1 = this.SearchAnObject(request);
					}
					if (flag1)
					{
						if (aDSearchResponse1 == null || aDSearchResponse1.Entries.Count <= 0)
						{
							aDSearchResponse1 = this.SearchAnObject(request, strs1);
						}
						else
						{
							aDSearchResponse = this.SearchAnObject(request, strs1);
							if (aDSearchResponse.Entries.Count > 0)
							{
								ADObject aDObject = aDSearchResponse.Entries[0];
								foreach (string str in aDObject.PropertyNames)
								{
									aDSearchResponse1.Entries[0].SetValue(str, aDObject[str]);
								}
							}
						}
					}
					return aDSearchResponse1;
				}
			}
			if (flag)
			{
				strs1.Add("ad:all");
			}
			DirectoryControlCollection directoryControlCollection = new DirectoryControlCollection();
			ADPageResultRequestControl aDPageResultRequestControl = null;
			for (int i = 0; i < request.Controls.Count; i++)
			{
				if (request.Controls[i] as ADPageResultRequestControl == null)
				{
					directoryControlCollection.Add(request.Controls[i]);
				}
				else
				{
					aDPageResultRequestControl = (ADPageResultRequestControl)request.Controls[i];
				}
			}
			string cookie = null;
			if (aDPageResultRequestControl != null)
			{
				cookie = aDPageResultRequestControl.Cookie as string;
			}
			bool flag2 = false;
			bool flag3 = false;
			do
			{
				if (cookie == null)
				{
					cookie = this.CreateEnumerationContext(request, strs1);
					flag3 = true;
				}

				int sizeLimit = request.SizeLimit;
				if (aDPageResultRequestControl != null && aDPageResultRequestControl.PageSize > 0 && (sizeLimit == 0 || sizeLimit > aDPageResultRequestControl.PageSize))
				{
					sizeLimit = aDPageResultRequestControl.PageSize;
				}
				if (sizeLimit > 0)
				{

				}
				try
				{
					try
					{
						aDSearchResponse1 = this.SearchAnObject(request);

						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{

						}
						else
						{

						}
						flag2 = false;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
			}
			while (flag2);

			/*
			ADPullResponse aDPullResponse = new ADPullResponse();
			string distinguishedName = null;
			if (aDPullResponse.Results.Count > 0)
			{
				distinguishedName = aDPullResponse.Results[0].DistinguishedName;
			}
			IList<ADObject> aDObjects = new List<ADObject>();
			foreach (ADWSResultEntry result in aDPullResponse.Results)
			{
				if (!string.IsNullOrEmpty(result.DistinguishedName))
				{
					result.DirObject.DistinguishedName = result.DistinguishedName;
				}
				aDObjects.Add(result.DirObject);
			}
			ArrayList arrayLists = new ArrayList();
			foreach (DirectoryControl control in aDPullResponse.Controls)
			{
				arrayLists.Add(control);
			}
			if (aDPageResultRequestControl != null)
			{
				arrayLists.Add(new ADPageResultResponseControl(aDPullResponse.Results.Count, aDPullResponse.EnumerationContext, true, null));
			}
			aDSearchResponse1 = new ADSearchResponse(distinguishedName, ADDirectoryServiceConnection.CreateControlArray(arrayLists), ResultCode.Success, null);
			aDSearchResponse1.Entries = aDObjects;
			*/

			return aDSearchResponse1;
		}

		public DirectoryEntry GetDirectoryEntry (string dn)
		{
			string endpoint = string.Concat (this.InstanceInfo, "/", dn);
			DirectoryEntry searchRoot = CreateDirectoryEntry(endpoint);
			return searchRoot;
		}
		
		private ADSearchResponse SearchAnObject(ADSearchRequest request)
		{
			ADSearchResponse aDSearchResponse = null;
			bool flag = false;
			if (string.IsNullOrEmpty(request.DistinguishedName))
			{
				flag = true;
			}
			bool flag1 = false;
			do
			{
				try
				{
					try
					{
						DirectoryEntry searchRoot = GetDirectoryEntry(request.DistinguishedName);
						ADObject adObj = ToADObject (request.DistinguishedName, searchRoot, request.Attributes);
						aDSearchResponse = new ADSearchResponse(request.DistinguishedName, null, ResultCode.Success, "");
						var filter = request.Filter as string;

						if (!string.IsNullOrEmpty (filter))
						{
							string[] propertiesToLoad = new string[request.Attributes.Count];
							request.Attributes.CopyTo(propertiesToLoad, 0);
							DirectorySearcher searcher = new DirectorySearcher(searchRoot, filter);
							searcher.PropertiesToLoad.AddRange(propertiesToLoad);
							searcher.SearchScope = TranslateScope (request.Scope);
							var items = searcher.FindAll ();
							int i = 0;

							foreach(SearchResult item in items)
							{
								if (request.SizeLimit > 0 && i >= request.SizeLimit) break;
								aDSearchResponse.Entries.Add (ToADObject (null, item.GetDirectoryEntry (), request.Attributes));
								i++;
							}
						}
						else {
							aDSearchResponse.Entries.Add (adObj);
						}

						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							
						}
						else
						{

						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", "WS-T Get response message:");
						DebugLogger.WriteLine("ADDirectoryServiceConnection", "message");
						flag1 = false;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
			}
			while (flag1);
			if (aDSearchResponse == null) new ADSearchResponse(request.DistinguishedName, null, ResultCode.InvalidDNSyntax, "");
			return aDSearchResponse;
		}

		public ADObject ToADObject (string dn, DirectoryEntry searchRoot, StringCollection attributes)
		{
			if (string.IsNullOrEmpty (dn)) {
				var dnObj = searchRoot.Properties ["distinguishedName"];
				if (dnObj != null && dnObj.Value != null) dn = dnObj.Value as string;
			}
			string[] newprops = new string[attributes.Count];
			attributes.CopyTo (newprops, 0);
			//var properties = NativeEntryFactory.GetProperties (this._serverName, this.PortNumber, dn, this._sessionInfo.Credential.UserName, GetPassword (this._sessionInfo.Credential.Password), newprops);
			ADObject adObj = null;
			if (string.IsNullOrEmpty (dn)) {
				adObj = new ADObject(new Guid("11111111-1111-1111-1111-111111111111"));
			}
			else {
				adObj = new ADObject();
			}
			FilleADObject (ref adObj, searchRoot, attributes);
			return adObj;
		}


		private static void FilleADObject (ref ADObject adObj, DirectoryEntry searchRoot, StringCollection attributes)
		{
			foreach(string propertyName in searchRoot.Properties.PropertyNames)
			{
				bool exists = false;
				if (attributes != null) {
					foreach(string p in attributes)
					{
						if (p.Equals (propertyName, StringComparison.OrdinalIgnoreCase))
						{
							exists = true;
							break;
						}
					}
				}
				if (attributes == null || exists || attributes.Contains ("*"))
				{
					ADPropertyValueCollection col = new ADPropertyValueCollection();
					col.AddRange (searchRoot.Properties[propertyName].OfType<object>().ToArray ());
					adObj.Add(propertyName, col);
				}
			}
		}

		private static System.DirectoryServices.SearchScope TranslateScope (System.DirectoryServices.Protocols.SearchScope scope)
		{
			switch (scope) {
			case System.DirectoryServices.Protocols.SearchScope.Base:
				return System.DirectoryServices.SearchScope.Base;
			case System.DirectoryServices.Protocols.SearchScope.OneLevel:
				return System.DirectoryServices.SearchScope.OneLevel;
			case System.DirectoryServices.Protocols.SearchScope.Subtree:
				return System.DirectoryServices.SearchScope.Subtree;
			}
			return System.DirectoryServices.SearchScope.Base;
		}
		                                                                 
		
		private ADSearchResponse SearchAnObject(ADSearchRequest request, IList<string> attributes)
		{
			ADSearchResponse aDSearchResponse = null;
			bool flag = false;
			if (string.IsNullOrEmpty(request.DistinguishedName))
			{
				flag = true;
			}
			bool flag1 = false;
			do
			{
				try
				{
					try
					{
						foreach(string att in attributes)
						{
							if (!request.Attributes.Contains (att))
							{
								request.Attributes.Add (att);
							}
						}

						DirectoryEntry searchRoot = GetDirectoryEntry(request.DistinguishedName);
						ADObject adObj = ToADObject (request.DistinguishedName, searchRoot, request.Attributes);
						aDSearchResponse = new ADSearchResponse(request.DistinguishedName, null, ResultCode.Success, "");

						var filter = request.Filter as string;
						if (!string.IsNullOrEmpty (filter) && !string.IsNullOrEmpty (request.DistinguishedName))
						{
							string[] propertiesToLoad = new string[request.Attributes.Count];
							request.Attributes.CopyTo(propertiesToLoad, 0 );
							DirectorySearcher searcher = new DirectorySearcher(searchRoot, filter);
							searcher.PropertiesToLoad.AddRange(propertiesToLoad);
							searcher.SearchScope = TranslateScope (request.Scope);
							var items = searcher.FindAll ();
							int i = 0; 

							foreach(SearchResult item in items)
							{
								if (request.SizeLimit > 0 && i >= request.SizeLimit) break;
								aDSearchResponse.Entries.Add (ToADObject (null, item.GetDirectoryEntry (), request.Attributes));
								i++;
							}
						}
						else {
							aDSearchResponse.Entries.Add (adObj);
						}
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{

						}
						else
						{

						}
						flag1 = false;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						ADDirectoryServiceConnection.CommonCatchAll(exception);
					}
				}
				finally
				{

				}
			}
			while (flag1);
			if (aDSearchResponse == null) new ADSearchResponse(request.DistinguishedName, null, ResultCode.InvalidDNSyntax, "");
			return aDSearchResponse;
		}
		
		public SetPasswordResponse SetPassword(SetPasswordRequest request)
		{
			this.ValidateServerNotGC();
			bool flag = false;
			SetPasswordResponse setPasswordResponse = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: SetPasswordRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tAccountDN: {0}", request.AccountDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: setPasswordResponse = this._acctMgmt.SetPassword(request);
					flag = false;
					object[] objArray = new object[1];
					objArray[0] = this._debugInstance;
					DebugLogger.WriteLine("ADDirectoryServiceConnection", "ADDirectoryServiceConnection 0x{0:X}: Got SetPasswordResponse", objArray);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return setPasswordResponse;
		}

		private DirectoryEntry CreateDirectoryEntry (string endpoint)
		{
			string pwd = GetPassword (this._sessionInfo.Credential.Password);
			return new DirectoryEntry(endpoint, this._sessionInfo.Credential.UserName, pwd, AuthenticationTypes.Secure | AuthenticationTypes.ServerBind | AuthenticationTypes.Signing | AuthenticationTypes.FastBind);
		}

		private string GetPassword (System.Security.SecureString s)
		{
			return ByteArrayToString (GetData (s));
		}

		internal static string ByteArrayToString (byte[] data)
		{
			var ret = new List<byte> ();
			foreach (var b in data) {
				if (b != 0)
				{
					ret.Add(b);
				}
			}
			return Encoding.UTF8.GetString (ret.ToArray ());
		}

		internal static byte[] GetData(System.Security.SecureString s)
		{
			System.Reflection.FieldInfo fi = s.GetType().GetField ("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return (byte[])fi.GetValue (s);
		}

		private static void ThrowExceptionForErrorCode(string message, string errorCode, string extendedErrorMessage, Exception innerException)
		{
			ADDirectoryServiceConnection.ThrowExceptionForExtendedError(extendedErrorMessage, innerException);
			if (string.IsNullOrEmpty(errorCode))
			{
				return;
			}
			else
			{
				int num = ADDirectoryServiceConnection.ParseErrorCode(errorCode);
				Win32Exception win32Exception = new Win32Exception(num);
				throw ExceptionHelper.GetExceptionFromErrorCode(num, win32Exception.Message, message, innerException);
			}
		}
		
		private static void ThrowExceptionForExtendedError(string extendedErrorMessage, Exception innerException)
		{
			string message;
			int num = 0;
			if (!string.IsNullOrEmpty(extendedErrorMessage))
			{
				if (extendedErrorMessage.Length >= 8)
				{
					string str = extendedErrorMessage.Substring(0, 8);
					if (int.TryParse(str, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out num) && num != 0)
					{
						int num1 = num;
						if (num1 != 0x52d)
						{
							Win32Exception win32Exception = new Win32Exception(num);
							message = win32Exception.Message;
						}
						else
						{
							message = StringResources.PasswordRestrictionErrorMessage;
						}
						throw ExceptionHelper.GetExceptionFromErrorCode(num, message, extendedErrorMessage, innerException);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static void ThrowExceptionForResultCode(string message, string resultCode, string extendedErrorMessage, Exception innerException)
		{
			ADDirectoryServiceConnection.ThrowExceptionForExtendedError(extendedErrorMessage, innerException);
			if (string.IsNullOrEmpty(resultCode))
			{
				return;
			}
			else
			{
				int errorCode = int.Parse(resultCode, NumberFormatInfo.InvariantInfo);
				errorCode = ADStoreAccess.MapResultCodeToErrorCode((ResultCode)errorCode);
				Win32Exception win32Exception = new Win32Exception(errorCode);
				throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, win32Exception.Message, message, innerException);
			}
		}
		
		public TranslateNameResponse TranslateName(TranslateNameRequest request)
		{
			bool flag = false;
			TranslateNameResponse translateNameResponse = null;
			do
			{
				try
				{
					request.Server = this.InstanceInfo;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: TranslateNameRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tFormatDesired: {0}", request.FormatDesired);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tFormatOffered: {0}", request.FormatOffered);
						stringBuilder.AppendLine();
						stringBuilder.Append("\tNames: ");
						if (request.Names != null)
						{
							string[] names = request.Names;
							for (int i = 0; i < (int)names.Length; i++)
							{
								string str = names[i];
								stringBuilder.AppendLine();
								stringBuilder.AppendFormat("\t\t{0}", str);
							}
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder.ToString());
					}
					//SVC CALL: translateNameResponse = this._acctMgmt.TranslateName(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("ADDirectoryServiceConnection 0x{0:X}: TranslateNameResponse NameTranslateResult", this._debugInstance);
						if (translateNameResponse.NameTranslateResult != null)
						{
							ActiveDirectoryNameTranslateResult[] nameTranslateResult = translateNameResponse.NameTranslateResult;
							for (int j = 0; j < (int)nameTranslateResult.Length; j++)
							{
								ActiveDirectoryNameTranslateResult activeDirectoryNameTranslateResult = nameTranslateResult[j];
								stringBuilder1.AppendLine();
								stringBuilder1.AppendFormat("\t{0}", activeDirectoryNameTranslateResult.Name);
							}
						}
						DebugLogger.WriteLine("ADDirectoryServiceConnection", stringBuilder1.ToString());
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ADDirectoryServiceConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return translateNameResponse;
		}

		private void ValidateServerNotGC()
		{
			if (!this.IsGCPort)
			{
				return;
			}
			else
			{
				throw new NotSupportedException(StringResources.NotSupportedGCPort);
			}
		}
	}
}