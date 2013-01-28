using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.Faults;
using Microsoft.ActiveDirectory.Management.IMDA;
using Microsoft.ActiveDirectory.Management.Provider;
using Microsoft.ActiveDirectory.Management.WSE;
using Microsoft.ActiveDirectory.Management.WST;
using Microsoft.ActiveDirectory.WebServices.Proxy;
using Microsoft.ActiveDirectory.CustomActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management
{
	internal class AdwsConnection : IDisposable
	{
		private const string _debugCategory = "AdwsConnection";

		private int _debugInstance;

		private ADSessionInfo _sessionInfo;

		private ChannelFactory<ResourceFactory> _cfResourceFactory;

		private ResourceFactory _resFactory;

		private CommunicationException _resFactoryException;

		private ChannelFactory<Resource> _cfResource;

		private Resource _resource;

		private CommunicationException _resourceException;

		private ChannelFactory<Search> _cfSearch;

		private Search _search;

		private CommunicationException _searchException;

		private ChannelFactory<AccountManagement> _cfAcctMgmt;

		private AccountManagement _acctMgmt;

		private CommunicationException _acctMgmtException;

		private ChannelFactory<TopologyManagement> _cfTopoMgmt;

		private TopologyManagement _topoMgmt;

		private CommunicationException _topoMgmtException;

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
						DebugLogger.WriteLine("AdwsConnection", string.Concat("AuthType is ", this._authenticationType.Value.ToString()));
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
					DebugLogger.WriteLineIf(this._autoReconnect.Value, "AdwsConnection", "AutoReconnect is enabled");
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
					this._instanceInfo = string.Concat("ldap:", portNumber.ToString());
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
					DebugLogger.WriteLine("AdwsConnection", string.Concat("PortNumber is ", value.ToString()));
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

		public AdwsConnection(ADSessionInfo info)
		{
			this._portNumber = null;
			this._authenticationType = null;
			this._autoReconnect = null;
			this._debugInstance = this.GetHashCode();
			object[] objArray = new object[1];
			objArray[0] = this._debugInstance;
			DebugLogger.WriteLine("AdwsConnection", "Constructor AdwsConnection 0x{0:X}", objArray);
			if (info != null)
			{
				this._sessionInfo = info;
				return;
			}
			else
			{
				DebugLogger.LogWarning("AdwsConnection", "Constructor(AdwsConnection) called with null info");
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
			this.InitializeChannel<TopologyManagement>(ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: ChangeOptionalFeatureRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tDistinguishedName: {0}", request.DistinguishedName);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tEnable: {0}", request.Enable);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tFeatureId: {0}", request.FeatureId);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					changeOptionalFeatureResponse = this._topoMgmt.ChangeOptionalFeature(request);
					flag = false;
					object[] objArray = new object[1];
					objArray[0] = this._debugInstance;
					DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Got ChangeOptionalFeatureResponse", objArray);
				}
				catch (FaultException<ChangeOptionalFeatureFault> faultException1)
				{
					FaultException<ChangeOptionalFeatureFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._topoMgmtException = communicationException;
					this.InitializeForAutoReconnect<TopologyManagement>(ref flag, ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return changeOptionalFeatureResponse;
		}

		public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<AccountManagement>(ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: ChangePasswordRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tAccountDN: {0}", request.AccountDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					changePasswordResponse = this._acctMgmt.ChangePassword(request);
					flag = false;
					object[] objArray = new object[1];
					objArray[0] = this._debugInstance;
					DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Got ChangePasswordResponse", objArray);
				}
				catch (FaultException<ChangePasswordFault> faultException1)
				{
					FaultException<ChangePasswordFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._acctMgmtException = communicationException;
					this.InitializeForAutoReconnect<AccountManagement>(ref flag, ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return changePasswordResponse;
		}

		private static void CloseChannelFactory(IChannelFactory chFactory)
		{
			try
			{
				if (chFactory.State != CommunicationState.Opened)
				{
					if (chFactory.State == CommunicationState.Faulted)
					{
						chFactory.Abort();
					}
				}
				else
				{
					chFactory.Close();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] type = new object[2];
				type[0] = exception.GetType();
				type[1] = exception.Message;
				DebugLogger.LogWarning("AdwsConnection", "IChannelFactory.Close thrown exception: {0}: {1}", type);
			}
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
			this.InitializeChannel<ResourceFactory>(ref this._resFactory, ref this._cfResourceFactory, "ResourceFactory", ref this._resFactoryException);
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
				Message message = null;
				MessageBuffer messageBuffer = null;
				do
				{
					ADCreateRequestMsg aDCreateRequestMsg = new ADCreateRequestMsg(this.InstanceInfo, str1, str, AdwsConnection.CreateControlArray(request.Controls), directoryAttributeArray);
					try
					{
						try
						{
							if (DebugLogger.Level < DebugLogLevel.Verbose)
							{
								message = this._resFactory.Create(aDCreateRequestMsg);
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = this._debugInstance;
								DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Create request message:", objArray);
								messageBuffer = aDCreateRequestMsg.CreateBufferedCopy(0x7fffffff);
								DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
								message = this._resFactory.Create(messageBuffer.CreateMessage());
								messageBuffer.Close();
								messageBuffer = null;
								DebugLogger.WriteLine("AdwsConnection", "Create response message:");
								DebugLogger.WriteLine("AdwsConnection", message.ToString());
							}
							flag = false;
						}
						catch (CommunicationException communicationException1)
						{
							CommunicationException communicationException = communicationException1;
							this._resFactoryException = communicationException;
							AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
							this.InitializeForAutoReconnect<ResourceFactory>(ref flag, ref this._resFactory, ref this._cfResourceFactory, "ResourceFactory", ref this._resFactoryException);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							AdwsConnection.CommonCatchAll(exception);
						}
					}
					finally
					{
						if (messageBuffer != null)
						{
							messageBuffer.Close();
							messageBuffer = null;
						}
					}
				}
				while (flag);
				if (message.IsFault)
				{
					AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
					Uri[] uriArray = AdwsConnection.ExtractReferralsFromFault(adwsFault);
					if (uriArray == null)
					{
						AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
					}
					else
					{
						Win32Exception win32Exception = new Win32Exception(0x202b);
						return new ADAddResponse(null, null, ResultCode.Referral, win32Exception.Message, uriArray);
					}
				}
				ADCreateResponseMsg aDCreateResponseMsg = new ADCreateResponseMsg(message);
				ADAddResponse aDAddResponse = new ADAddResponse(request.DistinguishedName, AdwsConnection.CreateArray<DirectoryControl>(aDCreateResponseMsg.Controls), ResultCode.Success, null);
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

		private Binding CreateBinding()
		{
			NetTcpBinding netTcpBinding;
			if (this.AuthType != AuthType.Basic)
			{
				netTcpBinding = new NetTcpBinding(SecurityMode.Transport);
				netTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
				netTcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
				netTcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
			}
			else
			{
				netTcpBinding = new NetTcpBinding(SecurityMode.TransportWithMessageCredential);
				netTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
				netTcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
				netTcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
			}
			TimeSpan? timeout = this._sessionInfo.Timeout;
			if (!timeout.HasValue)
			{
				netTcpBinding.SendTimeout = WSConstants.DefaultWcfSendTimeout;
			}
			else
			{
				TimeSpan? nullable = this._sessionInfo.Timeout;
				netTcpBinding.OpenTimeout = nullable.Value;
				TimeSpan? timeout1 = this._sessionInfo.Timeout;
				netTcpBinding.CloseTimeout = timeout1.Value;
				TimeSpan? nullable1 = this._sessionInfo.Timeout;
				netTcpBinding.SendTimeout = nullable1.Value;
				TimeSpan? timeout2 = this._sessionInfo.Timeout;
				netTcpBinding.ReceiveTimeout = timeout2.Value;
			}
			netTcpBinding.MaxReceivedMessageSize = (long)0x7fffffff;
			return netTcpBinding;
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

		private EndpointAddress CreateEndpointAddress(string configName)
		{
			EndpointIdentity endpointIdentity;
			StringBuilder stringBuilder = new StringBuilder();
			if (!this._sessionInfo.Connectionless)
			{
				stringBuilder.Append("net.tcp://");
				this.DiscoverServerName(false);
				stringBuilder.Append(this._serverName);
				stringBuilder.Append(":9389/ActiveDirectoryWebServices/");
				if (this.AuthType != AuthType.Basic)
				{
					stringBuilder.Append("Windows/");
					StringBuilder stringBuilder1 = new StringBuilder("ldap/");
					stringBuilder1.Append(this._serverName);
					if (!string.IsNullOrEmpty(this._domainName))
					{
						stringBuilder1.Append("/");
						stringBuilder1.Append(this._domainName);
					}
					endpointIdentity = EndpointIdentity.CreateSpnIdentity(stringBuilder1.ToString());
				}
				else
				{
					stringBuilder.Append("UserName/");
					endpointIdentity = EndpointIdentity.CreateDnsIdentity(this._serverName);
				}
				stringBuilder.Append(configName);
				DebugLogger.WriteLine("AdwsConnection", string.Concat("Endpoint: ", stringBuilder.ToString()));
				Uri uri = null;
				try
				{
					uri = new Uri(stringBuilder.ToString());
				}
				catch (UriFormatException uriFormatException)
				{
					object[] objArray = new object[1];
					objArray[0] = this._serverName;
					throw new UriFormatException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidUriFormat, objArray));
				}
				System.Diagnostics.Debug.WriteLine ("ADWS Connection: " + uri.ToString ());
				EndpointAddress endpointAddress = new EndpointAddress(uri, endpointIdentity, new AddressHeader[0]);
				return endpointAddress;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		private string CreateEnumerationContext(ADSearchRequest request, IList<string> attributes)
		{
			this.InitializeChannel<Search>(ref this._search, ref this._cfSearch, "Enumeration", ref this._searchException);
			bool flag = false;
			Message message = null;
			MessageBuffer messageBuffer = null;
			do
			{
				ADEnumerateLdapRequest aDEnumerateLdapRequest = new ADEnumerateLdapRequest(this.InstanceInfo, (string)request.Filter, request.DistinguishedName, request.Scope.ToString(), attributes);
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._search.Enumerate(aDEnumerateLdapRequest);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Enumerate request message:", objArray);
							messageBuffer = aDEnumerateLdapRequest.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._search.Enumerate(messageBuffer.CreateMessage());
							messageBuffer.Close();
							messageBuffer = null;
							DebugLogger.WriteLine("AdwsConnection", "Enumerate response message:");
							DebugLogger.WriteLine("AdwsConnection", message.ToString());
						}
						flag = false;
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._searchException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						this.InitializeForAutoReconnect<Search>(ref flag, ref this._search, ref this._cfSearch, "Enumeration", ref this._searchException);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
						messageBuffer = null;
					}
				}
			}
			while (flag);
			if (message.IsFault)
			{
				AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
				AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
			}
			ADEnumerateLdapResponse aDEnumerateLdapResponse = new ADEnumerateLdapResponse(message);
			return aDEnumerateLdapResponse.EnumerationContext;
		}

		public ADDeleteResponse Delete(ADDeleteRequest request)
		{
			this.InitializeChannel<Resource>(ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
			bool flag = false;
			Message message = null;
			MessageBuffer messageBuffer = null;
			do
			{
				ADDeleteRequestMsg aDDeleteRequestMsg = new ADDeleteRequestMsg(this.InstanceInfo, request.DistinguishedName, AdwsConnection.CreateControlArray(request.Controls));
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._resource.Delete(aDDeleteRequestMsg);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Delete request message:", objArray);
							messageBuffer = aDDeleteRequestMsg.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._resource.Delete(messageBuffer.CreateMessage());
							messageBuffer.Close();
							messageBuffer = null;
							DebugLogger.WriteLine("AdwsConnection", "Delete response message:");
							DebugLogger.WriteLine("AdwsConnection", message.ToString());
						}
						flag = false;
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._resourceException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						this.InitializeForAutoReconnect<Resource>(ref flag, ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
						messageBuffer = null;
					}
				}
			}
			while (flag);
			if (message.IsFault)
			{
				AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
				Uri[] uriArray = AdwsConnection.ExtractReferralsFromFault(adwsFault);
				if (uriArray == null)
				{
					AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
				}
				else
				{
					Win32Exception win32Exception = new Win32Exception(0x202b);
					return new ADDeleteResponse(null, null, ResultCode.Referral, win32Exception.Message, uriArray);
				}
			}
			ADDeleteResponseMsg aDDeleteResponseMsg = new ADDeleteResponseMsg(message);
			ADDeleteResponse aDDeleteResponse = new ADDeleteResponse(request.DistinguishedName, AdwsConnection.CreateArray<DirectoryControl>(aDDeleteResponseMsg.Controls), ResultCode.Success, null);
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
					DebugLogger.WriteLineIf(forceDiscovery, "AdwsConnection", "Forcing rediscovery of server name");
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
					DebugLogger.WriteLine("AdwsConnection", "calling DsGetDcName for server {0} with flags {1}", objArray);
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
								DebugLogger.LogWarning("AdwsConnection", "DsGetDCName returned invalid flags error for input: {0}", objArray1);
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
			object[] objArray = new object[1];
			objArray[0] = this._debugInstance;
			DebugLogger.WriteLine("AdwsConnection", "AdwsConnection Dispose 0x{0:X}", objArray);
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.UninitializeChannel<ResourceFactory>(ref this._resFactory, ref this._cfResourceFactory, ref this._resFactoryException);
				this.UninitializeChannel<Resource>(ref this._resource, ref this._cfResource, ref this._resourceException);
				this.UninitializeChannel<Search>(ref this._search, ref this._cfSearch, ref this._searchException);
				this.UninitializeChannel<AccountManagement>(ref this._acctMgmt, ref this._cfAcctMgmt, ref this._acctMgmtException);
				this.UninitializeChannel<TopologyManagement>(ref this._topoMgmt, ref this._cfTopoMgmt, ref this._topoMgmtException);
			}
			this._disposed = true;
		}

		private static Uri[] ExtractReferralsFromFault(AdwsFault adwsFault)
		{
			Uri[] uri = null;
			if (adwsFault.HasDetail && adwsFault.Detail as FaultDetail != null)
			{
				FaultDetail detail = (FaultDetail)adwsFault.Detail;
				if (detail.DirectoryError != null)
				{
					DirectoryErrorDetail directoryError = detail.DirectoryError;
					if (!string.IsNullOrEmpty(directoryError.ErrorCode) && directoryError.Referral != null)
					{
						ResultCode resultCode = (ResultCode)int.Parse(directoryError.ErrorCode, NumberFormatInfo.InvariantInfo);
						if (resultCode == ResultCode.Referral && (int)directoryError.Referral.Length > 0)
						{
							uri = new Uri[(int)directoryError.Referral.Length];
							for (int i = 0; i < (int)directoryError.Referral.Length; i++)
							{
								uri[i] = new Uri(directoryError.Referral[i]);
							}
						}
					}
				}
			}
			return uri;
		}

		private static string FaultExceptionMessage(FaultException faultException)
		{
			if (faultException.Code != null && faultException.Code.SubCode != null)
			{
				string subCodeMessage = AdwsFaultUtil.GetSubCodeMessage(faultException.Code.SubCode);
				if (!string.IsNullOrEmpty(subCodeMessage))
				{
					return subCodeMessage;
				}
			}
			if (string.IsNullOrEmpty(faultException.Message))
			{
				return faultException.Reason.ToString();
			}
			else
			{
				return faultException.Message;
			}
		}

		~AdwsConnection()
		{
			try
			{
				object[] objArray = new object[1];
				objArray[0] = this._debugInstance;
				DebugLogger.WriteLine("AdwsConnection", "Destructor AdwsConnection 0x{0:X}", objArray);
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		public GetADDomainResponse GetADDomain(GetADDomainRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<TopologyManagement>(ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: GetADDomainRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					aDDomain = this._topoMgmt.GetADDomain(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: GetADDomainResponse Domain", this._debugInstance);
						if (aDDomain.Domain != null)
						{
							stringBuilder1.AppendLine();
							stringBuilder1.AppendFormat("\tName:{0} DN:{1}", aDDomain.Domain.Name, aDDomain.Domain.DistinguishedName);
						}
						DebugLogger.WriteLine("AdwsConnection", stringBuilder1.ToString());
					}
				}
				catch (FaultException<GetADDomainFault> faultException1)
				{
					FaultException<GetADDomainFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._topoMgmtException = communicationException;
					this.InitializeForAutoReconnect<TopologyManagement>(ref flag, ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDDomain;
		}

		public GetADDomainControllerResponse GetADDomainController(GetADDomainControllerRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<TopologyManagement>(ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: GetADDomainControllerRequest", this._debugInstance);
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
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					aDDomainController = this._topoMgmt.GetADDomainController(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: GetADDomainControllerResponse DomainControllers", this._debugInstance);
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
						DebugLogger.WriteLine("AdwsConnection", stringBuilder1.ToString());
					}
				}
				catch (FaultException<GetADDomainControllerFault> faultException1)
				{
					FaultException<GetADDomainControllerFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._topoMgmtException = communicationException;
					this.InitializeForAutoReconnect<TopologyManagement>(ref flag, ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDDomainController;
		}

		public GetADForestResponse GetADForest(GetADForestRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<TopologyManagement>(ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: GetADForestRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					aDForest = this._topoMgmt.GetADForest(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: GetADForestResponse Forest", this._debugInstance);
						if (aDForest.Forest != null)
						{
							stringBuilder1.AppendLine();
							stringBuilder1.AppendFormat("\tName: {0}", aDForest.Forest.Name);
						}
						DebugLogger.WriteLine("AdwsConnection", stringBuilder1.ToString());
					}
				}
				catch (FaultException<GetADForestFault> faultException1)
				{
					FaultException<GetADForestFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._topoMgmtException = communicationException;
					this.InitializeForAutoReconnect<TopologyManagement>(ref flag, ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDForest;
		}

		public GetADGroupMemberResponse GetADGroupMember(GetADGroupMemberRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<AccountManagement>(ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: GetADGroupMemberRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tGroupDN: {0}", request.GroupDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tRecursive: {0}", request.Recursive);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					aDGroupMember = this._acctMgmt.GetADGroupMember(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: GetADGroupMemberResponse Members", this._debugInstance);
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
						DebugLogger.WriteLine("AdwsConnection", stringBuilder1.ToString());
					}
				}
				catch (FaultException<GetADGroupMemberFault> faultException1)
				{
					FaultException<GetADGroupMemberFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._acctMgmtException = communicationException;
					this.InitializeForAutoReconnect<AccountManagement>(ref flag, ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDGroupMember;
		}

		public GetADPrincipalAuthorizationGroupResponse GetADPrincipalAuthorizationGroup(GetADPrincipalAuthorizationGroupRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<AccountManagement>(ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: GetADPrincipalAuthorizationGroupRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPrincipalDN: {0}", request.PrincipalDN);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					aDPrincipalAuthorizationGroup = this._acctMgmt.GetADPrincipalAuthorizationGroup(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: GetADPrincipalAuthorizationGroupResponse MemberOf", this._debugInstance);
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
						DebugLogger.WriteLine("AdwsConnection", stringBuilder1.ToString());
					}
				}
				catch (FaultException<GetADPrincipalAuthorizationGroupFault> faultException1)
				{
					FaultException<GetADPrincipalAuthorizationGroupFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._acctMgmtException = communicationException;
					this.InitializeForAutoReconnect<AccountManagement>(ref flag, ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDPrincipalAuthorizationGroup;
		}

		public GetADPrincipalGroupMembershipResponse GetADPrincipalGroupMembership(GetADPrincipalGroupMembershipRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<AccountManagement>(ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: GetADPrincipalGroupMembershipRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPrincipalDN: {0}", request.PrincipalDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tResourceContextPartition: {0}", request.ResourceContextPartition);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					aDPrincipalGroupMembership = this._acctMgmt.GetADPrincipalGroupMembership(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: GetADPrincipalGroupMembershipResponse MemberOf", this._debugInstance);
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
						DebugLogger.WriteLine("AdwsConnection", stringBuilder1.ToString());
					}
				}
				catch (FaultException<GetADPrincipalGroupMembershipFault> faultException1)
				{
					FaultException<GetADPrincipalGroupMembershipFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._acctMgmtException = communicationException;
					this.InitializeForAutoReconnect<AccountManagement>(ref flag, ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return aDPrincipalGroupMembership;
		}

		private void InitializeChannel<TChannel>(ref TChannel channel, ref ChannelFactory<TChannel> chFactory, string endpointName, ref CommunicationException commException)
		{
			if (!this._disposed)
			{
				if (commException != null)
				{
					object[] message = new object[4];
					message[0] = "InitializeChannel<";
					message[1] = typeof(TChannel);
					message[2] = ">: ";
					message[3] = commException.Message;
					DebugLogger.LogWarning("AdwsConnection", string.Concat(message));
					if (!this.AutoReconnect)
					{
						throw new ADServerDownException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDown, new object[0]), commException, this._serverName);
					}
					else
					{
						this.UninitializeChannel<TChannel>(ref channel, ref chFactory, ref commException);
						this._serverName = null;
					}
				}
				if (channel == null)
				{
					chFactory = new ChannelFactory<TChannel>(this.CreateBinding(), this.CreateEndpointAddress(endpointName));
					this.UpdateClientCredential(chFactory);
					if (typeof(TChannel) == typeof(AccountManagement) || typeof(TChannel) == typeof(TopologyManagement))
					{
						foreach (OperationDescription operation in chFactory.Endpoint.Contract.Operations)
						{
							DataContractSerializerOperationBehavior dataContractSerializerOperationBehavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
							if (dataContractSerializerOperationBehavior == null)
							{
								continue;
							}
							dataContractSerializerOperationBehavior.MaxItemsInObjectGraph = 0x7fffffff;
						}
					}
					channel = chFactory.CreateChannel();
					if (typeof(TChannel) == typeof(AccountManagement))
					{
						((IContextChannel)(object)channel).OperationTimeout = WSConstants.DefaultAccountManagementCATimeout;
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private void InitializeForAutoReconnect<TChannel>(ref bool isAutoReconnecting, ref TChannel channel, ref ChannelFactory<TChannel> chFactory, string endpointName, ref CommunicationException commException)
		{
			if (!this.AutoReconnect || !isAutoReconnecting)
			{
				throw new ADServerDownException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDown, new object[0]), commException, this._serverName);
			}
			else
			{
				isAutoReconnecting = true;
				this.UninitializeChannel<TChannel>(ref channel, ref chFactory, ref commException);
				this.DiscoverServerName(true);
				this.InitializeChannel<TChannel>(ref channel, ref chFactory, endpointName, ref commException);
				return;
			}
		}

		private static bool IsResultCodeError(CustomActionFault fault)
		{
			if (fault as GetADDomainControllerFault != null || fault as GetADDomainFault != null || fault as GetADForestFault != null || fault as ChangeOptionalFeatureFault != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public ADModifyResponse Modify(ADModifyRequest request)
		{
			ADPutRequestMsg aDPutRequestMsg;
			bool flag = false;
			this.InitializeChannel<Resource>(ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
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
			Message message = null;
			MessageBuffer messageBuffer = null;
			do
			{
				if (!flag)
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, request.DistinguishedName, AdwsConnection.CreateControlArray(request.Controls), directoryAttributeModificationArray);
				}
				else
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, "11111111-1111-1111-1111-111111111111", AdwsConnection.CreateControlArray(request.Controls), directoryAttributeModificationArray);
				}
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._resource.Put(aDPutRequestMsg);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Modify request message:", objArray);
							messageBuffer = aDPutRequestMsg.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._resource.Put(messageBuffer.CreateMessage());
							messageBuffer.Close();
							messageBuffer = null;
							DebugLogger.WriteLine("AdwsConnection", "Modify response message:");
							DebugLogger.WriteLine("AdwsConnection", message.ToString());
						}
						flag1 = false;
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._resourceException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						this.InitializeForAutoReconnect<Resource>(ref flag1, ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
						messageBuffer = null;
					}
				}
			}
			while (flag1);
			if (message.IsFault)
			{
				AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
				Uri[] uriArray = AdwsConnection.ExtractReferralsFromFault(adwsFault);
				if (uriArray == null)
				{
					AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
				}
				else
				{
					Win32Exception win32Exception = new Win32Exception(0x202b);
					return new ADModifyResponse(null, null, ResultCode.Referral, win32Exception.Message, uriArray);
				}
			}
			ADPutResponseMsg aDPutResponseMsg = new ADPutResponseMsg(message);
			ADModifyResponse aDModifyResponse = new ADModifyResponse(request.DistinguishedName, AdwsConnection.CreateArray<DirectoryControl>(aDPutResponseMsg.Controls), ResultCode.Success, null);
			return aDModifyResponse;
		}

		public ADModifyDNResponse ModifyDN(ADModifyDNRequest request)
		{
			ADPutRequestMsg aDPutRequestMsg;
			this.InitializeChannel<Resource>(ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
			bool flag = false;
			Message message = null;
			MessageBuffer messageBuffer = null;
			do
			{
				if (!string.IsNullOrEmpty(request.NewParentDistinguishedName))
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, request.DistinguishedName, AdwsConnection.CreateControlArray(request.Controls), request.NewName, request.NewParentDistinguishedName);
				}
				else
				{
					aDPutRequestMsg = new ADPutRequestMsg(this.InstanceInfo, request.DistinguishedName, AdwsConnection.CreateControlArray(request.Controls), request.NewName);
				}
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._resource.Put(aDPutRequestMsg);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: ModifyDN request message:", objArray);
							messageBuffer = aDPutRequestMsg.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._resource.Put(messageBuffer.CreateMessage());
							messageBuffer.Close();
							messageBuffer = null;
							DebugLogger.WriteLine("AdwsConnection", "ModifyDN response message:");
							DebugLogger.WriteLine("AdwsConnection", message.ToString());
						}
						flag = false;
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._resourceException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						this.InitializeForAutoReconnect<Resource>(ref flag, ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
						messageBuffer = null;
					}
				}
			}
			while (flag);
			if (message.IsFault)
			{
				AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
				Uri[] uriArray = AdwsConnection.ExtractReferralsFromFault(adwsFault);
				if (uriArray == null)
				{
					AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
				}
				else
				{
					Win32Exception win32Exception = new Win32Exception(0x202b);
					return new ADModifyDNResponse(null, null, ResultCode.Referral, win32Exception.Message, uriArray);
				}
			}
			ADPutResponseMsg aDPutResponseMsg = new ADPutResponseMsg(message);
			ADModifyDNResponse aDModifyDNResponse = new ADModifyDNResponse(request.DistinguishedName, AdwsConnection.CreateArray<DirectoryControl>(aDPutResponseMsg.Controls), ResultCode.Success, null);
			return aDModifyDNResponse;
		}

		public MoveADOperationMasterRoleResponse MoveADOperationMasterRole(MoveADOperationMasterRoleRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<TopologyManagement>(ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: MoveADOperationMasterRoleRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tOperationMasterRole: {0}", request.OperationMasterRole);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tSeize: {0}", request.Seize);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					moveADOperationMasterRoleResponse = this._topoMgmt.MoveADOperationMasterRole(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: MoveADOperationMasterRoleResponse", this._debugInstance);
						stringBuilder1.AppendLine();
						stringBuilder1.AppendFormat("\tWasSeized: {0}", moveADOperationMasterRoleResponse.WasSeized);
					}
				}
				catch (FaultException<MoveADOperationMasterRoleFault> faultException1)
				{
					FaultException<MoveADOperationMasterRoleFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._topoMgmtException = communicationException;
					this.InitializeForAutoReconnect<TopologyManagement>(ref flag, ref this._topoMgmt, ref this._cfTopoMgmt, "TopologyManagement", ref this._topoMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
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
				Message message = null;
				MessageBuffer messageBuffer = null;
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._search.Release(aDReleaseRequest);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Release request message:", objArray);
							messageBuffer = aDReleaseRequest.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._search.Release(messageBuffer.CreateMessage());
						}
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._searchException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						throw new ADServerDownException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDown, new object[0]), communicationException, this._serverName);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
					}
				}
				if (DebugLogger.Level >= DebugLogLevel.Warning)
				{
					DebugLogger.WriteLine("AdwsConnection", "Release response message:");
					DebugLogger.WriteLine("AdwsConnection", message.ToString());
					if (message.IsFault)
					{
						AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
						object[] str = new object[1];
						str[0] = adwsFault.Reason.ToString();
						DebugLogger.LogWarning("AdwsConnection", "AbandonSearch returned fault: {0}", str);
					}
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
			ADPullRequest aDPullRequest;
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
			if (request.Scope == SearchScope.Base)
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
			Message message = null;
			MessageBuffer messageBuffer = null;
			bool flag3 = false;
			do
			{
				if (cookie == null)
				{
					cookie = this.CreateEnumerationContext(request, strs1);
					flag3 = true;
				}
				if (request.TimeLimit == TimeSpan.Zero)
				{
					aDPullRequest = new ADPullRequest(this.InstanceInfo, cookie, AdwsConnection.CreateControlArray(directoryControlCollection));
				}
				else
				{
					aDPullRequest = new ADPullRequest(this.InstanceInfo, cookie, request.TimeLimit, AdwsConnection.CreateControlArray(directoryControlCollection));
				}
				int sizeLimit = request.SizeLimit;
				if (aDPageResultRequestControl != null && aDPageResultRequestControl.PageSize > 0 && (sizeLimit == 0 || sizeLimit > aDPageResultRequestControl.PageSize))
				{
					sizeLimit = aDPageResultRequestControl.PageSize;
				}
				if (sizeLimit > 0)
				{
					aDPullRequest.MaxElements = new uint?(Convert.ToUInt32 (sizeLimit));
				}
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._search.Pull(aDPullRequest);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Pull request message:", objArray);
							messageBuffer = aDPullRequest.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._search.Pull(messageBuffer.CreateMessage());
							messageBuffer.Close();
							messageBuffer = null;
							DebugLogger.WriteLine("AdwsConnection", "Pull response message:");
							DebugLogger.WriteLine("AdwsConnection", message.ToString());
						}
						flag2 = false;
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._searchException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						if (!flag3)
						{
							throw new ADServerDownException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDown, new object[0]), communicationException, this._serverName);
						}
						else
						{
							this.InitializeForAutoReconnect<Search>(ref flag2, ref this._search, ref this._cfSearch, "Enumeration", ref this._searchException);
							flag3 = false;
							cookie = null;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
						messageBuffer = null;
					}
				}
			}
			while (flag2);
			if (message.IsFault)
			{
				this.ReleaseEnumerationContext(cookie);
				AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
				Uri[] uriArray = AdwsConnection.ExtractReferralsFromFault(adwsFault);
				if (uriArray == null)
				{
					AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
				}
				else
				{
					Win32Exception win32Exception = new Win32Exception(0x202b);
					return new ADSearchResponse(null, null, ResultCode.Referral, win32Exception.Message, uriArray);
				}
			}
			ADPullResponse aDPullResponse = new ADPullResponse(message);
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
			aDSearchResponse1 = new ADSearchResponse(distinguishedName, AdwsConnection.CreateControlArray(arrayLists), ResultCode.Success, null);
			aDSearchResponse1.Entries = aDObjects;
			return aDSearchResponse1;
		}

		private ADSearchResponse SearchAnObject(ADSearchRequest request)
		{
			Microsoft.ActiveDirectory.Management.WST.ADGetRequestMsg aDGetRequestMsg;
			ADSearchResponse aDSearchResponse;
			this.InitializeChannel<Resource>(ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
			bool flag = false;
			if (string.IsNullOrEmpty(request.DistinguishedName))
			{
				flag = true;
			}
			bool flag1 = false;
			Message message = null;
			MessageBuffer messageBuffer = null;
			do
			{
				if (!flag)
				{
					aDGetRequestMsg = new Microsoft.ActiveDirectory.Management.WST.ADGetRequestMsg(this.InstanceInfo, request.DistinguishedName, AdwsConnection.CreateControlArray(request.Controls));
				}
				else
				{
					aDGetRequestMsg = new Microsoft.ActiveDirectory.Management.WST.ADGetRequestMsg(this.InstanceInfo, "11111111-1111-1111-1111-111111111111", AdwsConnection.CreateControlArray(request.Controls));
				}
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._resource.Get(aDGetRequestMsg);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: WS-T Get request message:", objArray);
							messageBuffer = aDGetRequestMsg.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._resource.Get(messageBuffer.CreateMessage());
							messageBuffer.Close();
							messageBuffer = null;
						}
						DebugLogger.WriteLine("AdwsConnection", "WS-T Get response message:");
						DebugLogger.WriteLine("AdwsConnection", message.ToString());
						flag1 = false;
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._resourceException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						this.InitializeForAutoReconnect<Resource>(ref flag1, ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
						messageBuffer = null;
					}
				}
			}
			while (flag1);
			if (message.IsFault)
			{
				AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
				Uri[] uriArray = AdwsConnection.ExtractReferralsFromFault(adwsFault);
				if (uriArray == null)
				{
					AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
				}
				else
				{
					Win32Exception win32Exception = new Win32Exception(0x202b);
					return new ADSearchResponse(null, null, ResultCode.Referral, win32Exception.Message, uriArray);
				}
			}
			Microsoft.ActiveDirectory.Management.WST.ADGetResponseMsg aDGetResponseMsg = new Microsoft.ActiveDirectory.Management.WST.ADGetResponseMsg(message);
			if (aDGetResponseMsg.Entry == null)
			{
				aDSearchResponse = new ADSearchResponse(null, AdwsConnection.CreateArray<DirectoryControl>(aDGetResponseMsg.Controls), ResultCode.Success, null);
			}
			else
			{
				aDSearchResponse = new ADSearchResponse(aDGetResponseMsg.Entry.DistinguishedName, AdwsConnection.CreateArray<DirectoryControl>(aDGetResponseMsg.Controls), ResultCode.Success, null);
				if (aDGetResponseMsg.Entry.DirObject != null)
				{
					if (!string.IsNullOrEmpty(aDGetResponseMsg.Entry.DistinguishedName))
					{
						aDGetResponseMsg.Entry.DirObject.DistinguishedName = aDGetResponseMsg.Entry.DistinguishedName;
					}
					aDSearchResponse.Entries.Add(aDGetResponseMsg.Entry.DirObject);
				}
			}
			return aDSearchResponse;
		}

		private ADSearchResponse SearchAnObject(ADSearchRequest request, IList<string> attributes)
		{
			Microsoft.ActiveDirectory.Management.IMDA.ADGetRequestMsg aDGetRequestMsg;
			ADSearchResponse aDSearchResponse;
			this.InitializeChannel<Resource>(ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
			bool flag = false;
			if (string.IsNullOrEmpty(request.DistinguishedName))
			{
				flag = true;
			}
			bool flag1 = false;
			Message message = null;
			MessageBuffer messageBuffer = null;
			do
			{
				if (!flag)
				{
					aDGetRequestMsg = new Microsoft.ActiveDirectory.Management.IMDA.ADGetRequestMsg(this.InstanceInfo, request.DistinguishedName, AdwsConnection.CreateControlArray(request.Controls), attributes);
				}
				else
				{
					aDGetRequestMsg = new Microsoft.ActiveDirectory.Management.IMDA.ADGetRequestMsg(this.InstanceInfo, "11111111-1111-1111-1111-111111111111", AdwsConnection.CreateControlArray(request.Controls), attributes);
				}
				try
				{
					try
					{
						if (DebugLogger.Level < DebugLogLevel.Verbose)
						{
							message = this._resource.Get(aDGetRequestMsg);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this._debugInstance;
							DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: IMDA Get request message:", objArray);
							messageBuffer = aDGetRequestMsg.CreateBufferedCopy(0x7fffffff);
							DebugLogger.WriteLine("AdwsConnection", AdwsMessage.MessageToString(messageBuffer.CreateMessage(), true));
							message = this._resource.Get(messageBuffer.CreateMessage());
							messageBuffer.Close();
							messageBuffer = null;
							DebugLogger.WriteLine("AdwsConnection", "IMDA Get response message:");
							DebugLogger.WriteLine("AdwsConnection", message.ToString());
						}
						flag1 = false;
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this._resourceException = communicationException;
						AdwsConnection.ThrowAuthenticationRelatedExceptionIfAny(communicationException);
						this.InitializeForAutoReconnect<Resource>(ref flag1, ref this._resource, ref this._cfResource, "Resource", ref this._resourceException);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						AdwsConnection.CommonCatchAll(exception);
					}
				}
				finally
				{
					if (messageBuffer != null)
					{
						messageBuffer.Close();
						messageBuffer = null;
					}
				}
			}
			while (flag1);
			if (message.IsFault)
			{
				AdwsFault adwsFault = AdwsFaultUtil.ConstructFault(message);
				Uri[] uriArray = AdwsConnection.ExtractReferralsFromFault(adwsFault);
				if (uriArray == null)
				{
					AdwsConnection.ThrowException(adwsFault, AdwsFaultUtil.ConstructFaultException(adwsFault.Message));
				}
				else
				{
					Win32Exception win32Exception = new Win32Exception(0x202b);
					return new ADSearchResponse(null, null, ResultCode.Referral, win32Exception.Message, uriArray);
				}
			}
			Microsoft.ActiveDirectory.Management.IMDA.ADGetResponseMsg aDGetResponseMsg = new Microsoft.ActiveDirectory.Management.IMDA.ADGetResponseMsg(message);
			if (aDGetResponseMsg.Entry == null)
			{
				aDSearchResponse = new ADSearchResponse(null, AdwsConnection.CreateArray<DirectoryControl>(aDGetResponseMsg.Controls), ResultCode.Success, null);
			}
			else
			{
				aDSearchResponse = new ADSearchResponse(aDGetResponseMsg.Entry.DistinguishedName, AdwsConnection.CreateArray<DirectoryControl>(aDGetResponseMsg.Controls), ResultCode.Success, null);
				if (aDGetResponseMsg.Entry.DirObject != null)
				{
					if (!string.IsNullOrEmpty(aDGetResponseMsg.Entry.DistinguishedName))
					{
						aDGetResponseMsg.Entry.DirObject.DistinguishedName = aDGetResponseMsg.Entry.DistinguishedName;
					}
					aDSearchResponse.Entries.Add(aDGetResponseMsg.Entry.DirObject);
				}
			}
			return aDSearchResponse;
		}

		public SetPasswordResponse SetPassword(SetPasswordRequest request)
		{
			this.ValidateServerNotGC();
			this.InitializeChannel<AccountManagement>(ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: SetPasswordRequest", this._debugInstance);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tServer: {0}", request.Server);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tAccountDN: {0}", request.AccountDN);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\tPartitionDN: {0}", request.PartitionDN);
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					setPasswordResponse = this._acctMgmt.SetPassword(request);
					flag = false;
					object[] objArray = new object[1];
					objArray[0] = this._debugInstance;
					DebugLogger.WriteLine("AdwsConnection", "AdwsConnection 0x{0:X}: Got SetPasswordResponse", objArray);
				}
				catch (FaultException<SetPasswordFault> faultException1)
				{
					FaultException<SetPasswordFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._acctMgmtException = communicationException;
					this.InitializeForAutoReconnect<AccountManagement>(ref flag, ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return setPasswordResponse;
		}

		private static void ThrowAuthenticationRelatedExceptionIfAny(CommunicationException exception)
		{
			SecurityAccessDeniedException securityAccessDeniedException = exception as SecurityAccessDeniedException;
			if (securityAccessDeniedException == null)
			{
				SecurityNegotiationException securityNegotiationException = exception as SecurityNegotiationException;
				if (securityNegotiationException == null)
				{
					return;
				}
				else
				{
					throw new AuthenticationException(securityNegotiationException.Message, securityNegotiationException);
				}
			}
			else
			{
				throw new UnauthorizedAccessException(securityAccessDeniedException.Message, securityAccessDeniedException);
			}
		}

		private static void ThrowException(FaultException faultException)
		{
			throw new ADException(AdwsConnection.FaultExceptionMessage(faultException), faultException, faultException.Reason.ToString());
		}

		private static void ThrowException(AdwsFault adwsFault, FaultException faultException)
		{
			Win32Exception win32Exception;
			if (adwsFault.HasDetail)
			{
				if (adwsFault.Detail as EnumerateFault != null)
				{
					EnumerateFault detail = (EnumerateFault)adwsFault.Detail;
					if (!string.IsNullOrEmpty(detail.InvalidProperty))
					{
						throw new ArgumentException(StringResources.InvalidProperty, XmlUtility.RemovePrefix("addata", detail.InvalidProperty), faultException);
					}
				}
				if (adwsFault.Detail as FaultDetail != null)
				{
					AdwsConnection.ThrowExceptionForFaultDetail((FaultDetail)adwsFault.Detail, faultException);
				}
				if (adwsFault.Detail as AttributeTypeNotValid != null)
				{
					AttributeTypeNotValid attributeTypeNotValid = (AttributeTypeNotValid)adwsFault.Detail;
					if (attributeTypeNotValid.AttributeTypeNotValidForEntry == null)
					{
						if (attributeTypeNotValid.AttributeTypeNotValidForDialect != null)
						{
							win32Exception = new Win32Exception(0x200b);
							throw new ArgumentException(win32Exception.Message, XmlUtility.RemovePrefix("addata", attributeTypeNotValid.AttributeTypeNotValidForDialect.AttributeType), faultException);
						}
					}
					else
					{
						win32Exception = new Win32Exception(0x200a);
						throw new ArgumentException(win32Exception.Message, XmlUtility.RemovePrefix("addata", attributeTypeNotValid.AttributeTypeNotValidForEntry.AttributeType), faultException);
					}
				}
			}
			if (adwsFault.Code != null && adwsFault.Code.SubCode != null)
			{
				string subCodeMessage = AdwsFaultUtil.GetSubCodeMessage(adwsFault.Code.SubCode);
				if (!string.IsNullOrEmpty(subCodeMessage))
				{
					throw new ADException(subCodeMessage, faultException, adwsFault.Reason.ToString());
				}
			}
			throw new ADException(adwsFault.Reason.ToString(), faultException);
		}

		internal static void ThrowException(CustomActionFault caFault, FaultException faultException)
		{
			string shortErrorMessage;
			if (caFault.ArgumentError == null)
			{
				if (caFault.DirectoryError != null)
				{
					DirectoryErrorDetailCA directoryError = caFault.DirectoryError;
					if (string.IsNullOrEmpty(directoryError.Win32ErrorCode))
					{
						if (!AdwsConnection.IsResultCodeError(caFault))
						{
							AdwsConnection.ThrowExceptionForErrorCode(directoryError.Message, directoryError.ErrorCode, directoryError.ExtendedErrorMessage, faultException);
						}
						else
						{
							AdwsConnection.ThrowExceptionForResultCode(directoryError.Message, directoryError.ErrorCode, directoryError.ExtendedErrorMessage, faultException);
						}
					}
					else
					{
						AdwsConnection.ThrowExceptionForErrorCode(directoryError.Message, directoryError.Win32ErrorCode, directoryError.ExtendedErrorMessage, faultException);
					}
					if (!string.IsNullOrEmpty(directoryError.ShortMessage))
					{
						string str = AdwsFaultUtil.GetShortErrorMessage(directoryError.ShortMessage);
						if (!string.IsNullOrEmpty(str))
						{
							throw new ADException(str, faultException, directoryError.Message);
						}
					}
				}
				if (caFault as SetPasswordFault == null)
				{
					if (caFault as ChangePasswordFault == null)
					{
						if (!string.IsNullOrEmpty(caFault.ShortError))
						{
							string shortErrorMessage1 = AdwsFaultUtil.GetShortErrorMessage(caFault.ShortError);
							if (!string.IsNullOrEmpty(shortErrorMessage1))
							{
								throw new ADException(shortErrorMessage1, faultException, caFault.Error);
							}
						}
						throw new ADException(AdwsConnection.FaultExceptionMessage(faultException), faultException, caFault.Error);
					}
					else
					{
						if (!string.IsNullOrEmpty(caFault.ShortError))
						{
							if (string.Compare(caFault.ShortError, "EPassword", StringComparison.OrdinalIgnoreCase) != 0)
							{
								shortErrorMessage = AdwsFaultUtil.GetShortErrorMessage(caFault.ShortError);
							}
							else
							{
								shortErrorMessage = StringResources.ChangePasswordErrorMessage;
							}
							if (!string.IsNullOrEmpty(shortErrorMessage))
							{
								throw new ADPasswordException(shortErrorMessage, faultException, caFault.Error);
							}
						}
						throw new ADPasswordException(AdwsConnection.FaultExceptionMessage(faultException), faultException, caFault.Error);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(caFault.ShortError))
					{
						if (string.Compare(caFault.ShortError, "EPassword", StringComparison.OrdinalIgnoreCase) != 0)
						{
							string str1 = AdwsFaultUtil.GetShortErrorMessage(caFault.ShortError);
							if (!string.IsNullOrEmpty(str1))
							{
								throw new ADPasswordException(str1, faultException, caFault.Error);
							}
						}
						else
						{
							Win32Exception win32Exception = new Win32Exception(0x52d);
							throw new ADPasswordComplexityException(win32Exception.Message, faultException, caFault.Error);
						}
					}
					throw new ADPasswordException(AdwsConnection.FaultExceptionMessage(faultException), faultException, caFault.Error);
				}
			}
			else
			{
				string invalidProperty = StringResources.InvalidProperty;
				if (!string.IsNullOrEmpty(caFault.ArgumentError.ShortMessage))
				{
					string shortErrorMessage2 = AdwsFaultUtil.GetShortErrorMessage(caFault.ArgumentError.ShortMessage);
					if (!string.IsNullOrEmpty(shortErrorMessage2))
					{
						invalidProperty = shortErrorMessage2;
					}
				}
				throw new ArgumentException(invalidProperty, caFault.ArgumentError.ParameterName, faultException);
			}
		}

		private static void ThrowExceptionForErrorCode(string message, string errorCode, string extendedErrorMessage, Exception innerException)
		{
			AdwsConnection.ThrowExceptionForExtendedError(extendedErrorMessage, innerException);
			if (string.IsNullOrEmpty(errorCode))
			{
				return;
			}
			else
			{
				int num = AdwsConnection.ParseErrorCode(errorCode);
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

		private static void ThrowExceptionForFaultDetail(FaultDetail faultDetail, FaultException faultException)
		{
			if (faultDetail.ArgumentError == null)
			{
				if (faultDetail.DirectoryError != null)
				{
					DirectoryErrorDetail directoryError = faultDetail.DirectoryError;
					if (string.IsNullOrEmpty(directoryError.Win32ErrorCode))
					{
						AdwsConnection.ThrowExceptionForResultCode(directoryError.Message, directoryError.ErrorCode, directoryError.ExtendedErrorMessage, faultException);
					}
					else
					{
						AdwsConnection.ThrowExceptionForErrorCode(directoryError.Message, directoryError.Win32ErrorCode, directoryError.ExtendedErrorMessage, faultException);
					}
					if (!string.IsNullOrEmpty(directoryError.ShortMessage))
					{
						string shortErrorMessage = AdwsFaultUtil.GetShortErrorMessage(directoryError.ShortMessage);
						if (!string.IsNullOrEmpty(shortErrorMessage))
						{
							throw new ADException(shortErrorMessage, faultException, directoryError.Message);
						}
					}
				}
				if (string.IsNullOrEmpty(faultDetail.InvalidAttributeType))
				{
					if (string.IsNullOrEmpty(faultDetail.InvalidOperation))
					{
						if (faultDetail.InvalidChange == null)
						{
							if (!string.IsNullOrEmpty(faultDetail.ShortError))
							{
								string str = AdwsFaultUtil.GetShortErrorMessage(faultDetail.ShortError);
								if (!string.IsNullOrEmpty(str))
								{
									throw new ADException(str, faultException, faultDetail.Error);
								}
							}
							return;
						}
						else
						{
							throw new ADInvalidOperationException(faultDetail.InvalidChange.Operation, faultException);
						}
					}
					else
					{
						throw new ADInvalidOperationException(faultDetail.InvalidOperation, faultException);
					}
				}
				else
				{
					throw new ArgumentException(StringResources.InvalidProperty, XmlUtility.RemovePrefix("addata", faultDetail.InvalidAttributeType), faultException);
				}
			}
			else
			{
				string invalidProperty = StringResources.InvalidProperty;
				if (!string.IsNullOrEmpty(faultDetail.ArgumentError.ShortMessage))
				{
					string shortErrorMessage1 = AdwsFaultUtil.GetShortErrorMessage(faultDetail.ArgumentError.ShortMessage);
					if (!string.IsNullOrEmpty(shortErrorMessage1))
					{
						invalidProperty = shortErrorMessage1;
					}
				}
				throw new ArgumentException(invalidProperty, XmlUtility.RemovePrefix("addata", faultDetail.ArgumentError.ParameterName), faultException);
			}
		}

		private static void ThrowExceptionForResultCode(string message, string resultCode, string extendedErrorMessage, Exception innerException)
		{
			AdwsConnection.ThrowExceptionForExtendedError(extendedErrorMessage, innerException);
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
			this.InitializeChannel<AccountManagement>(ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
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
						stringBuilder.AppendFormat("AdwsConnection 0x{0:X}: TranslateNameRequest", this._debugInstance);
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
						DebugLogger.WriteLine("AdwsConnection", stringBuilder.ToString());
					}
					translateNameResponse = this._acctMgmt.TranslateName(request);
					flag = false;
					if (DebugLogger.Level >= DebugLogLevel.Verbose)
					{
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.AppendFormat("AdwsConnection 0x{0:X}: TranslateNameResponse NameTranslateResult", this._debugInstance);
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
						DebugLogger.WriteLine("AdwsConnection", stringBuilder1.ToString());
					}
				}
				catch (FaultException<TranslateNameFault> faultException1)
				{
					FaultException<TranslateNameFault> faultException = faultException1;
					AdwsConnection.ThrowException(faultException.Detail, faultException);
				}
				catch (FaultException faultException3)
				{
					FaultException faultException2 = faultException3;
					AdwsConnection.ThrowException(faultException2);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this._acctMgmtException = communicationException;
					this.InitializeForAutoReconnect<AccountManagement>(ref flag, ref this._acctMgmt, ref this._cfAcctMgmt, "AccountManagement", ref this._acctMgmtException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					AdwsConnection.CommonCatchAll(exception);
				}
			}
			while (flag);
			return translateNameResponse;
		}

		private void UninitializeChannel<TChannel>(ref TChannel channel, ref ChannelFactory<TChannel> chFactory, ref CommunicationException commException)
		{
			if (chFactory != null)
			{
				AdwsConnection.CloseChannelFactory(chFactory);
				chFactory = null;
			}
			channel = default(TChannel);
			commException = null;
		}

		private void UpdateClientCredential(ChannelFactory chFactory)
		{
			if (this.AuthType != AuthType.Basic)
			{
				chFactory.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Delegation;
				if (this._sessionInfo.Credential != null)
				{
					chFactory.Credentials.Windows.ClientCredential = this._sessionInfo.Credential.GetNetworkCredential();
				}
			}
			else
			{
				if (this._sessionInfo.Credential != null)
				{
					NetworkCredential networkCredential = this._sessionInfo.Credential.GetNetworkCredential();
					if (string.IsNullOrEmpty(networkCredential.Domain))
					{
						chFactory.Credentials.UserName.UserName = networkCredential.UserName;
					}
					else
					{
						chFactory.Credentials.UserName.UserName = string.Concat(networkCredential.Domain, "\\", networkCredential.UserName);
					}
					chFactory.Credentials.UserName.Password = networkCredential.Password;
					return;
				}
			}
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