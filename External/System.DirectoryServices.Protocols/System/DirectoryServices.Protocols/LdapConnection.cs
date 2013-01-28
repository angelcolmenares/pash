using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class LdapConnection : DirectoryConnection, IDisposable
	{
		private AuthType connectionAuthType;

		private LdapSessionOptions options;

		internal ConnectionHandle ldapHandle;

		internal bool disposed;

		private bool bounded;

		private bool needRebind;

		internal static Hashtable handleTable;

		internal static object objectLock;

		private GetLdapResponseCallback fd;

		private static Hashtable asyncResultTable;

		private static LdapPartialResultsProcessor partialResultsProcessor;

		private static ManualResetEvent waitHandle;

		private static PartialResultsRetriever retriever;

		private bool setFQDNDone;

		internal bool automaticBind;

		internal bool needDispose;

		private bool connected;

		internal QUERYCLIENTCERT clientCertificateRoutine;

		private const int LDAP_MOD_BVALUES = 128;

		public AuthType AuthType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.connectionAuthType;
			}
			set
			{
				if (value < AuthType.Anonymous || value > AuthType.Kerberos)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(AuthType));
				}
				else
				{
					if (this.bounded && value != this.connectionAuthType)
					{
						this.needRebind = true;
					}
					this.connectionAuthType = value;
					return;
				}
			}
		}

		public bool AutoBind
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.automaticBind;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.automaticBind = value;
			}
		}

		public override NetworkCredential Credential
		{
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
			[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				NetworkCredential networkCredential;
				if (this.bounded && !this.SameCredential(this.directoryCredential, value))
				{
					this.needRebind = true;
				}
				LdapConnection ldapConnection = this;
				if (value != null)
				{
					networkCredential = new NetworkCredential(value.UserName, value.Password, value.Domain);
				}
				else
				{
					networkCredential = null;
				}
				ldapConnection.directoryCredential = networkCredential;
			}
		}

		public LdapSessionOptions SessionOptions
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.options;
			}
		}

		public override TimeSpan Timeout
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.connectionTimeOut;
			}
			set
			{
				if (value >= TimeSpan.Zero)
				{
					if (value.TotalSeconds <= 2147483647)
					{
						this.connectionTimeOut = value;
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
				}
			}
		}

		static LdapConnection()
		{
			LdapConnection.handleTable = new Hashtable();
			LdapConnection.objectLock = new object();
			Hashtable hashtables = new Hashtable();
			LdapConnection.asyncResultTable = Hashtable.Synchronized(hashtables);
			LdapConnection.waitHandle = new ManualResetEvent(false);
			LdapConnection.partialResultsProcessor = new LdapPartialResultsProcessor(LdapConnection.waitHandle);
			LdapConnection.retriever = new PartialResultsRetriever(LdapConnection.waitHandle, LdapConnection.partialResultsProcessor);
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public LdapConnection(string server) : this(new LdapDirectoryIdentifier(server))
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public LdapConnection(LdapDirectoryIdentifier identifier) : this(identifier, null, (AuthType)2)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public LdapConnection(LdapDirectoryIdentifier identifier, NetworkCredential credential) : this(identifier, credential, (AuthType)2)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
		public LdapConnection(LdapDirectoryIdentifier identifier, NetworkCredential credential, AuthType authType)
		{
			NetworkCredential networkCredential;
			this.connectionAuthType = AuthType.Negotiate;
			this.automaticBind = true;
			this.needDispose = true;
			this.fd = new GetLdapResponseCallback(this.ConstructResponse);
			this.directoryIdentifier = identifier;
			LdapConnection ldapConnection = this;
			if (credential != null)
			{
				networkCredential = new NetworkCredential(credential.UserName, credential.Password, credential.Domain);
			}
			else
			{
				networkCredential = null;
			}
			ldapConnection.directoryCredential = networkCredential;
			this.connectionAuthType = authType;
			if (authType < AuthType.Anonymous || authType > AuthType.Kerberos)
			{
				throw new InvalidEnumArgumentException("authType", (int)authType, typeof(AuthType));
			}
			else
			{
				if (this.AuthType != AuthType.Anonymous || this.directoryCredential == null || (this.directoryCredential.Password == null || this.directoryCredential.Password.Length == 0) && (this.directoryCredential.UserName == null || this.directoryCredential.UserName.Length == 0))
				{
					this.Init();
					this.options = new LdapSessionOptions(this);
					this.clientCertificateRoutine = new QUERYCLIENTCERT(this.ProcessClientCertificate);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidAuthCredential"));
				}
			}
		}

		internal LdapConnection(LdapDirectoryIdentifier identifier, NetworkCredential credential, AuthType authType, IntPtr handle)
		{
			this.connectionAuthType = AuthType.Negotiate;
			this.automaticBind = true;
			this.needDispose = true;
			this.directoryIdentifier = identifier;
			this.ldapHandle = new ConnectionHandle(handle);
			this.directoryCredential = credential;
			this.connectionAuthType = authType;
			this.options = new LdapSessionOptions(this);
			this.needDispose = false;
			this.clientCertificateRoutine = new QUERYCLIENTCERT(this.ProcessClientCertificate);
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public void Abort(IAsyncResult asyncResult)
		{
			if (!this.disposed)
			{
				if (asyncResult != null)
				{
					if (asyncResult as LdapAsyncResult != null)
					{
						int item = -1;
						LdapAsyncResult ldapAsyncResult = (LdapAsyncResult)asyncResult;
						if (ldapAsyncResult.partialResults)
						{
							LdapConnection.partialResultsProcessor.Remove((LdapPartialAsyncResult)asyncResult);
							item = ((LdapPartialAsyncResult)asyncResult).messageID;
						}
						else
						{
							if (LdapConnection.asyncResultTable.Contains(asyncResult))
							{
								item = (int)LdapConnection.asyncResultTable[asyncResult];
								LdapConnection.asyncResultTable.Remove(asyncResult);
							}
							else
							{
								throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
							}
						}
						Wldap32.ldap_abandon(this.ldapHandle, item);
						LdapRequestState ldapRequestState = ldapAsyncResult.resultObject;
						if (ldapRequestState != null)
						{
							ldapRequestState.abortCalled = true;
						}
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "asyncResult";
						throw new ArgumentException(Res.GetString("NotReturnedAsyncResult", objArray));
					}
				}
				else
				{
					throw new ArgumentNullException("asyncResult");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public IAsyncResult BeginSendRequest(DirectoryRequest request, PartialResultProcessing partialMode, AsyncCallback callback, object state)
		{
			return this.BeginSendRequest(request, this.connectionTimeOut, partialMode, callback, state);
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public IAsyncResult BeginSendRequest(DirectoryRequest request, TimeSpan requestTimeout, PartialResultProcessing partialMode, AsyncCallback callback, object state)
		{
			int num = 0;
			if (!this.disposed)
			{
				if (request != null)
				{
					if (partialMode < PartialResultProcessing.NoPartialResultSupport || partialMode > PartialResultProcessing.ReturnPartialResultsAndNotifyCallback)
					{
						throw new InvalidEnumArgumentException("partialMode", (int)partialMode, typeof(PartialResultProcessing));
					}
					else
					{
						if (partialMode == PartialResultProcessing.NoPartialResultSupport || request as SearchRequest != null)
						{
							if (partialMode != PartialResultProcessing.ReturnPartialResultsAndNotifyCallback || callback != null)
							{
								int num1 = this.SendRequestHelper(request, ref num);
								LdapOperation ldapOperation = LdapOperation.LdapSearch;
								if (request as DeleteRequest == null)
								{
									if (request as AddRequest == null)
									{
										if (request as ModifyRequest == null)
										{
											if (request as SearchRequest == null)
											{
												if (request as ModifyDNRequest == null)
												{
													if (request as CompareRequest == null)
													{
														if (request as ExtendedRequest != null)
														{
															ldapOperation = LdapOperation.LdapExtendedRequest;
														}
													}
													else
													{
														ldapOperation = LdapOperation.LdapCompare;
													}
												}
												else
												{
													ldapOperation = LdapOperation.LdapModifyDn;
												}
											}
											else
											{
												ldapOperation = LdapOperation.LdapSearch;
											}
										}
										else
										{
											ldapOperation = LdapOperation.LdapModify;
										}
									}
									else
									{
										ldapOperation = LdapOperation.LdapAdd;
									}
								}
								else
								{
									ldapOperation = LdapOperation.LdapDelete;
								}
								if (num1 != 0 || num == -1)
								{
									if (num1 == 0)
									{
										num1 = Wldap32.LdapGetLastError();
									}
									throw this.ConstructException(num1, ldapOperation);
								}
								else
								{
									if (partialMode != PartialResultProcessing.NoPartialResultSupport)
									{
										bool flag = false;
										if (partialMode == PartialResultProcessing.ReturnPartialResultsAndNotifyCallback)
										{
											flag = true;
										}
										LdapPartialAsyncResult ldapPartialAsyncResult = new LdapPartialAsyncResult(num, callback, state, true, this, flag, requestTimeout);
										LdapConnection.partialResultsProcessor.Add(ldapPartialAsyncResult);
										return ldapPartialAsyncResult;
									}
									else
									{
										LdapRequestState ldapRequestState = new LdapRequestState();
										LdapAsyncResult ldapAsyncResult = new LdapAsyncResult(callback, state, false);
										ldapRequestState.ldapAsync = ldapAsyncResult;
										ldapAsyncResult.resultObject = ldapRequestState;
										LdapConnection.asyncResultTable.Add(ldapAsyncResult, num);
										this.fd.BeginInvoke(num, ldapOperation, ResultAll.LDAP_MSG_ALL, requestTimeout, true, new AsyncCallback(this.ResponseCallback), ldapRequestState);
										return ldapAsyncResult;
									}
								}
							}
							else
							{
								throw new ArgumentException(Res.GetString("CallBackIsNull"), "callback");
							}
						}
						else
						{
							throw new NotSupportedException(Res.GetString("PartialResultsNotSupported"));
						}
					}
				}
				else
				{
					throw new ArgumentNullException("request");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public void Bind()
		{
			this.BindHelper(this.directoryCredential, false);
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Bind(NetworkCredential newCredential)
		{
			this.BindHelper(newCredential, true);
		}

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
		private void BindHelper(NetworkCredential newCredential, bool needSetCredential)
		{
			int num;
			string str;
			string str1;
			string str2;
			string str3;
			NetworkCredential networkCredential;
			string userName;
			string domain;
			string password;
			int length;
			int length1;
			int num1;
			object obj;
			if (!this.disposed)
			{
				if (this.AuthType != AuthType.Anonymous || newCredential == null || (newCredential.Password == null || newCredential.Password.Length == 0) && (newCredential.UserName == null || newCredential.UserName.Length == 0))
				{
					if (!needSetCredential)
					{
						networkCredential = this.directoryCredential;
					}
					else
					{
						LdapConnection ldapConnection = this;
						if (newCredential != null)
						{
							obj = new NetworkCredential(newCredential.UserName, newCredential.Password, newCredential.Domain);
						}
						else
						{
							obj = null;
						}
						networkCredential = (NetworkCredential)obj;
						ldapConnection.directoryCredential = (NetworkCredential)obj;
					}
					if (!this.connected)
					{
						this.Connect();
						this.connected = true;
					}
					if (networkCredential == null || networkCredential.UserName.Length != 0 || networkCredential.Password.Length != 0 || networkCredential.Domain.Length != 0)
					{
						if (networkCredential == null)
						{
							userName = null;
						}
						else
						{
							userName = networkCredential.UserName;
						}
						str1 = userName;
						if (networkCredential == null)
						{
							domain = null;
						}
						else
						{
							domain = networkCredential.Domain;
						}
						str2 = domain;
						if (networkCredential == null)
						{
							password = null;
						}
						else
						{
							password = networkCredential.Password;
						}
						str3 = password;
					}
					else
					{
						str1 = null;
						str2 = null;
						str3 = null;
					}
					if (this.AuthType != AuthType.Anonymous)
					{
						if (this.AuthType != AuthType.Basic)
						{
							SEC_WINNT_AUTH_IDENTITY_EX sECWINNTAUTHIDENTITYEX = new SEC_WINNT_AUTH_IDENTITY_EX();
							sECWINNTAUTHIDENTITYEX.version = 0x200;
							sECWINNTAUTHIDENTITYEX.length = Marshal.SizeOf(typeof(SEC_WINNT_AUTH_IDENTITY_EX));
							sECWINNTAUTHIDENTITYEX.flags = 2;
							if (this.AuthType == AuthType.Kerberos)
							{
								sECWINNTAUTHIDENTITYEX.packageList = "Kerberos";
								sECWINNTAUTHIDENTITYEX.packageListLength = sECWINNTAUTHIDENTITYEX.packageList.Length;
							}
							if (networkCredential != null)
							{
								sECWINNTAUTHIDENTITYEX.user = str1;
								SEC_WINNT_AUTH_IDENTITY_EX sECWINNTAUTHIDENTITYEX1 = sECWINNTAUTHIDENTITYEX;
								if (str1 == null)
								{
									length = 0;
								}
								else
								{
									length = str1.Length;
								}
								sECWINNTAUTHIDENTITYEX1.userLength = length;
								sECWINNTAUTHIDENTITYEX.domain = str2;
								SEC_WINNT_AUTH_IDENTITY_EX sECWINNTAUTHIDENTITYEX2 = sECWINNTAUTHIDENTITYEX;
								if (str2 == null)
								{
									length1 = 0;
								}
								else
								{
									length1 = str2.Length;
								}
								sECWINNTAUTHIDENTITYEX2.domainLength = length1;
								sECWINNTAUTHIDENTITYEX.password = str3;
								SEC_WINNT_AUTH_IDENTITY_EX sECWINNTAUTHIDENTITYEX3 = sECWINNTAUTHIDENTITYEX;
								if (str3 == null)
								{
									num1 = 0;
								}
								else
								{
									num1 = str3.Length;
								}
								sECWINNTAUTHIDENTITYEX3.passwordLength = num1;
							}
							BindMethod bindMethod = BindMethod.LDAP_AUTH_NEGOTIATE;
							AuthType authType = this.AuthType;
							switch (authType)
							{
								case AuthType.Negotiate:
								{
									bindMethod = BindMethod.LDAP_AUTH_NEGOTIATE;
									break;
								}
								case AuthType.Ntlm:
								{
									bindMethod = BindMethod.LDAP_AUTH_NTLM;
									break;
								}
								case AuthType.Digest:
								{
									bindMethod = BindMethod.LDAP_AUTH_DIGEST;
									break;
								}
								case AuthType.Sicily:
								{
									bindMethod = BindMethod.LDAP_AUTH_SICILY;
									break;
								}
								case AuthType.Dpa:
								{
									bindMethod = BindMethod.LDAP_AUTH_DPA;
									break;
								}
								case AuthType.Msn:
								{
									bindMethod = BindMethod.LDAP_AUTH_MSN;
									break;
								}
								case AuthType.External:
								{
									bindMethod = BindMethod.LDAP_AUTH_EXTERNAL;
									break;
								}
								case AuthType.Kerberos:
								{
									bindMethod = BindMethod.LDAP_AUTH_NEGOTIATE;
									break;
								}
							}
							if (networkCredential != null || this.AuthType != AuthType.External)
							{
								num = Wldap32.ldap_bind_s(this.ldapHandle, null, sECWINNTAUTHIDENTITYEX, bindMethod);
							}
							else
							{
								num = Wldap32.ldap_bind_s(this.ldapHandle, null, null, bindMethod);
							}
						}
						else
						{
							StringBuilder stringBuilder = new StringBuilder(100);
							if (str2 != null && str2.Length != 0)
							{
								stringBuilder.Append(str2);
								stringBuilder.Append("\\");
							}
							stringBuilder.Append(str1);
							num = Wldap32.ldap_simple_bind_s(this.ldapHandle, stringBuilder.ToString(), str3);
						}
					}
					else
					{
						num = Wldap32.ldap_simple_bind_s(this.ldapHandle, null, null);
					}
					if (num == 0)
					{
						this.bounded = true;
						this.needRebind = false;
						return;
					}
					else
					{
						if (!Utility.IsResultCode((ResultCode)num))
						{
							if (!Utility.IsLdapError((LdapError)num))
							{
								throw new LdapException(num);
							}
							else
							{
								str = LdapErrorMappings.MapResultCode(num);
								string serverErrorMessage = this.options.ServerErrorMessage;
								if (serverErrorMessage == null || serverErrorMessage.Length <= 0)
								{
									throw new LdapException(num, str);
								}
								else
								{
									throw new LdapException(num, str, serverErrorMessage);
								}
							}
						}
						else
						{
							str = OperationErrorMappings.MapResultCode(num);
							throw new DirectoryOperationException(null, str);
						}
					}
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("InvalidAuthCredential"));
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		internal LdapMod[] BuildAttributes(CollectionBase directoryAttributes, ArrayList ptrToFree)
		{
			DirectoryAttribute item;
			byte[] bytes;
			IntPtr intPtr;
			LdapMod[] ldapMod = null;
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			DirectoryAttributeCollection directoryAttributeCollection = null;
			DirectoryAttributeModificationCollection directoryAttributeModificationCollection = null;
			if (directoryAttributes != null && directoryAttributes.Count != 0)
			{
				if (directoryAttributes as DirectoryAttributeModificationCollection == null)
				{
					directoryAttributeCollection = (DirectoryAttributeCollection)directoryAttributes;
				}
				else
				{
					directoryAttributeModificationCollection = (DirectoryAttributeModificationCollection)directoryAttributes;
				}
				ldapMod = new LdapMod[directoryAttributes.Count];
				for (int i = 0; i < directoryAttributes.Count; i++)
				{
					if (directoryAttributeCollection == null)
					{
						item = directoryAttributeModificationCollection[i];
					}
					else
					{
						item = directoryAttributeCollection[i];
					}
					ldapMod[i] = new LdapMod();
					if (item as DirectoryAttributeModification == null)
					{
						ldapMod[i].type = 0;
					}
					else
					{
						ldapMod[i].type = (int)((DirectoryAttributeModification)item).Operation;
					}
					LdapMod ldapMod1 = ldapMod[i];
					ldapMod1.type = ldapMod1.type | 128;
					ldapMod[i].attribute = Marshal.StringToHGlobalUni(item.Name);
					int count = 0;
					berval[] _berval = null;
					if (item.Count > 0)
					{
						count = item.Count;
						_berval = new berval[count];
						for (int j = 0; j < count; j++)
						{
							if (item[j] as string == null)
							{
								if (item[j] as Uri == null)
								{
									bytes = (byte[])item[j];
								}
								else
								{
									bytes = uTF8Encoding.GetBytes(((Uri)item[j]).ToString());
								}
							}
							else
							{
								bytes = uTF8Encoding.GetBytes((string)item[j]);
							}
							_berval[j] = new berval();
							_berval[j].bv_len = (int)bytes.Length;
							_berval[j].bv_val = Marshal.AllocHGlobal(_berval[j].bv_len);
							ptrToFree.Add(_berval[j].bv_val);
							Marshal.Copy(bytes, 0, _berval[j].bv_val, _berval[j].bv_len);
						}
					}
					ldapMod[i].values = Marshal.AllocHGlobal((count + 1) * Marshal.SizeOf(typeof(IntPtr)));
					int num = Marshal.SizeOf(typeof(berval));
					int num1 = 0;
					num1 = 0;
					while (num1 < count)
					{
						IntPtr intPtr1 = Marshal.AllocHGlobal(num);
						ptrToFree.Add(intPtr1);
						Marshal.StructureToPtr(_berval[num1], intPtr1, false);
						intPtr = (IntPtr)((long)ldapMod[i].values + (long)(Marshal.SizeOf(typeof(IntPtr)) * num1));
						Marshal.WriteIntPtr(intPtr, intPtr1);
						num1++;
					}
					intPtr = (IntPtr)((long)ldapMod[i].values + (long)(Marshal.SizeOf(typeof(IntPtr)) * num1));
					Marshal.WriteIntPtr(intPtr, (IntPtr)0);
				}
			}
			return ldapMod;
		}

		internal LdapControl[] BuildControlArray(DirectoryControlCollection controls, bool serverControl)
		{
			LdapControl[] ldapControl = null;
			if (controls != null && controls.Count != 0)
			{
				ArrayList arrayLists = new ArrayList();
				foreach (DirectoryControl control in controls)
				{
					if (!serverControl)
					{
						if (control.ServerSide)
						{
							continue;
						}
						arrayLists.Add(control);
					}
					else
					{
						if (!control.ServerSide)
						{
							continue;
						}
						arrayLists.Add(control);
					}
				}
				if (arrayLists.Count != 0)
				{
					int count = arrayLists.Count;
					ldapControl = new LdapControl[count];
					for (int i = 0; i < count; i++)
					{
						ldapControl[i] = new LdapControl();
						ldapControl[i].ldctl_oid = Marshal.StringToHGlobalUni(((DirectoryControl)arrayLists[i]).Type);
						ldapControl[i].ldctl_iscritical = ((DirectoryControl)arrayLists[i]).IsCritical;
						DirectoryControl item = (DirectoryControl)arrayLists[i];
						byte[] value = item.GetValue();
						if (value == null || (int)value.Length == 0)
						{
							ldapControl[i].ldctl_value = new berval();
							ldapControl[i].ldctl_value.bv_len = 0;
							ldapControl[i].ldctl_value.bv_val = (IntPtr)0;
						}
						else
						{
							ldapControl[i].ldctl_value = new berval();
							ldapControl[i].ldctl_value.bv_len = (int)value.Length;
							ldapControl[i].ldctl_value.bv_val = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * ldapControl[i].ldctl_value.bv_len);
							Marshal.Copy(value, 0, ldapControl[i].ldctl_value.bv_val, ldapControl[i].ldctl_value.bv_len);
						}
					}
				}
			}
			return ldapControl;
		}

		private void Connect()
		{
			if (base.ClientCertificates.Count <= 1)
			{
				if (base.ClientCertificates.Count != 0)
				{
					int num = Wldap32.ldap_set_option_clientcert(this.ldapHandle, LdapOption.LDAP_OPT_CLIENT_CERTIFICATE, this.clientCertificateRoutine);
					if (num == 0)
					{
						this.automaticBind = false;
					}
					else
					{
						if (!Utility.IsLdapError((LdapError)num))
						{
							throw new LdapException(num);
						}
						else
						{
							string str = LdapErrorMappings.MapResultCode(num);
							throw new LdapException(num, str);
						}
					}
				}
				if (((LdapDirectoryIdentifier)this.Directory).FullyQualifiedDnsHostName && !this.setFQDNDone)
				{
					this.SessionOptions.FQDN = true;
					this.setFQDNDone = true;
				}
				LDAP_TIMEVAL lDAPTIMEVAL = new LDAP_TIMEVAL();
				lDAPTIMEVAL.tv_sec = (int)(this.connectionTimeOut.Ticks / (long)0x989680);
				int num1 = Wldap32.ldap_connect(this.ldapHandle, lDAPTIMEVAL);
				if (num1 == 0)
				{
					return;
				}
				else
				{
					if (!Utility.IsLdapError((LdapError)num1))
					{
						throw new LdapException(num1);
					}
					else
					{
						string str1 = LdapErrorMappings.MapResultCode(num1);
						throw new LdapException(num1, str1);
					}
				}
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("InvalidClientCertificates"));
			}
		}

		internal DirectoryAttribute ConstructAttribute(IntPtr entryMessage, IntPtr attributeName)
		{
			DirectoryAttribute directoryAttribute = new DirectoryAttribute();
			directoryAttribute.isSearchResult = true;
			string stringUni = Marshal.PtrToStringUni(attributeName);
			directoryAttribute.Name = stringUni;
			IntPtr intPtr = Wldap32.ldap_get_values_len(this.ldapHandle, entryMessage, stringUni);
			try
			{
				int num = 0;
				if (intPtr != (IntPtr)0)
				{
					for (IntPtr i = Marshal.ReadIntPtr(intPtr, Marshal.SizeOf(typeof(IntPtr)) * num); i != (IntPtr)0; i = Marshal.ReadIntPtr(intPtr, Marshal.SizeOf(typeof(IntPtr)) * num))
					{
						berval _berval = new berval();
						Marshal.PtrToStructure(i, _berval);
						if (_berval.bv_len > 0 && _berval.bv_val != (IntPtr)0)
						{
							byte[] numArray = new byte[_berval.bv_len];
							Marshal.Copy(_berval.bv_val, numArray, 0, _berval.bv_len);
							directoryAttribute.Add(numArray);
						}
						num++;
					}
				}
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Wldap32.ldap_value_free_len(intPtr);
				}
			}
			return directoryAttribute;
		}

		private DirectoryControl ConstructControl(IntPtr controlPtr)
		{
			LdapControl ldapControl = new LdapControl();
			Marshal.PtrToStructure(controlPtr, ldapControl);
			string stringUni = Marshal.PtrToStringUni(ldapControl.ldctl_oid);
			byte[] numArray = new byte[ldapControl.ldctl_value.bv_len];
			Marshal.Copy(ldapControl.ldctl_value.bv_val, numArray, 0, ldapControl.ldctl_value.bv_len);
			bool ldctlIscritical = ldapControl.ldctl_iscritical;
			return new DirectoryControl(stringUni, numArray, ldctlIscritical, true);
		}

		internal SearchResultEntry ConstructEntry(IntPtr entryMessage)
		{
			SearchResultEntry searchResultEntry;
			IntPtr intPtr = (IntPtr)0;
			string stringUni = null;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			try
			{
				intPtr = Wldap32.ldap_get_dn(this.ldapHandle, entryMessage);
				if (intPtr != (IntPtr)0)
				{
					stringUni = Marshal.PtrToStringUni(intPtr);
					Wldap32.ldap_memfree(intPtr);
					intPtr = (IntPtr)0;
				}
				SearchResultEntry searchResultEntry1 = new SearchResultEntry(stringUni);
				SearchResultAttributeCollection attributes = searchResultEntry1.Attributes;
				intPtr1 = Wldap32.ldap_first_attribute(this.ldapHandle, entryMessage, ref intPtr2);
				int num = 0;
				while (intPtr1 != (IntPtr)0)
				{
					DirectoryAttribute directoryAttribute = this.ConstructAttribute(entryMessage, intPtr1);
					attributes.Add(directoryAttribute.Name, directoryAttribute);
					Wldap32.ldap_memfree(intPtr1);
					num++;
					intPtr1 = Wldap32.ldap_next_attribute(this.ldapHandle, entryMessage, intPtr2);
				}
				if (intPtr2 != (IntPtr)0)
				{
					Wldap32.ber_free(intPtr2, 0);
					intPtr2 = (IntPtr)0;
				}
				searchResultEntry = searchResultEntry1;
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Wldap32.ldap_memfree(intPtr);
				}
				if (intPtr1 != (IntPtr)0)
				{
					Wldap32.ldap_memfree(intPtr1);
				}
				if (intPtr2 != (IntPtr)0)
				{
					Wldap32.ber_free(intPtr2, 0);
				}
			}
			return searchResultEntry;
		}

		private DirectoryException ConstructException(int error, LdapOperation operation)
		{
			DirectoryResponse extendedResponse = null;
			if (!Utility.IsResultCode((ResultCode)error))
			{
				if (!Utility.IsLdapError((LdapError)error))
				{
					return new LdapException(error);
				}
				else
				{
					string str = LdapErrorMappings.MapResultCode(error);
					string serverErrorMessage = this.options.ServerErrorMessage;
					if (serverErrorMessage == null || serverErrorMessage.Length <= 0)
					{
						return new LdapException(error, str);
					}
					else
					{
						throw new LdapException(error, str, serverErrorMessage);
					}
				}
			}
			else
			{
				if (operation != LdapOperation.LdapAdd)
				{
					if (operation != LdapOperation.LdapModify)
					{
						if (operation != LdapOperation.LdapDelete)
						{
							if (operation != LdapOperation.LdapModifyDn)
							{
								if (operation != LdapOperation.LdapCompare)
								{
									if (operation != LdapOperation.LdapSearch)
									{
										if (operation == LdapOperation.LdapExtendedRequest)
										{
											extendedResponse = new ExtendedResponse(null, null, (ResultCode)error, OperationErrorMappings.MapResultCode(error), null);
										}
									}
									else
									{
										extendedResponse = new SearchResponse(null, null, (ResultCode)error, OperationErrorMappings.MapResultCode(error), null);
									}
								}
								else
								{
									extendedResponse = new CompareResponse(null, null, (ResultCode)error, OperationErrorMappings.MapResultCode(error), null);
								}
							}
							else
							{
								extendedResponse = new ModifyDNResponse(null, null, (ResultCode)error, OperationErrorMappings.MapResultCode(error), null);
							}
						}
						else
						{
							extendedResponse = new DeleteResponse(null, null, (ResultCode)error, OperationErrorMappings.MapResultCode(error), null);
						}
					}
					else
					{
						extendedResponse = new ModifyResponse(null, null, (ResultCode)error, OperationErrorMappings.MapResultCode(error), null);
					}
				}
				else
				{
					extendedResponse = new AddResponse(null, null, (ResultCode)error, OperationErrorMappings.MapResultCode(error), null);
				}
				string str1 = OperationErrorMappings.MapResultCode(error);
				return new DirectoryOperationException(extendedResponse, str1);
			}
		}

		internal unsafe int ConstructParsedResult(IntPtr ldapResult, ref int serverError, ref string responseDn, ref string responseMessage, ref Uri[] responseReferral, ref DirectoryControl[] responseControl)
		{
			IntPtr intPtr = (IntPtr)0;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			IntPtr intPtr3 = (IntPtr)0;
			int num = 0;
			try
			{
				num = Wldap32.ldap_parse_result(this.ldapHandle, ldapResult, ref serverError, ref intPtr, ref intPtr1, ref intPtr2, ref intPtr3, 0);
				if (num != 0)
				{
					if (num == 82)
					{
						int num1 = Wldap32.ldap_result2error(this.ldapHandle, ldapResult, 0);
						if (num1 != 0)
						{
							num = num1;
						}
					}
				}
				else
				{
					responseDn = Marshal.PtrToStringUni(intPtr);
					responseMessage = Marshal.PtrToStringUni(intPtr1);
					if (intPtr2 != (IntPtr)0)
					{
						char** chrPointer = (char**)((void*)intPtr2);
						char* chrPointer1 = (char*)((void*)(*(chrPointer)));
						int num2 = 0;
						ArrayList arrayLists = new ArrayList();
						while (chrPointer1 != null)
						{
							string stringUni = Marshal.PtrToStringUni((IntPtr)chrPointer1);
							arrayLists.Add(stringUni);
							num2++;
							chrPointer1 = (char*)((void*)(*(chrPointer + num2 * sizeof(char*))));
						}
						if (arrayLists.Count > 0)
						{
							responseReferral = new Uri[arrayLists.Count];
							for (int i = 0; i < arrayLists.Count; i++)
							{
								responseReferral[i] = new Uri((string)arrayLists[i]);
							}
						}
					}
					if (intPtr3 != (IntPtr)0)
					{
						int num3 = 0;
						IntPtr intPtr4 = intPtr3;
						IntPtr intPtr5 = Marshal.ReadIntPtr(intPtr4, 0);
						ArrayList arrayLists1 = new ArrayList();
						while (intPtr5 != (IntPtr)0)
						{
							DirectoryControl directoryControl = this.ConstructControl(intPtr5);
							arrayLists1.Add(directoryControl);
							num3++;
							intPtr5 = Marshal.ReadIntPtr(intPtr4, num3 * Marshal.SizeOf(typeof(IntPtr)));
						}
						responseControl = new DirectoryControl[arrayLists1.Count];
						arrayLists1.CopyTo(responseControl);
					}
				}
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Wldap32.ldap_memfree(intPtr);
				}
				if (intPtr1 != (IntPtr)0)
				{
					Wldap32.ldap_memfree(intPtr1);
				}
				if (intPtr2 != (IntPtr)0)
				{
					Wldap32.ldap_value_free(intPtr2);
				}
				if (intPtr3 != (IntPtr)0)
				{
					Wldap32.ldap_controls_free(intPtr3);
				}
			}
			return num;
		}

		internal SearchResultReference ConstructReference(IntPtr referenceMessage)
		{
			SearchResultReference searchResultReference = null;
			ArrayList arrayLists = new ArrayList();
			IntPtr intPtr = (IntPtr)0;
			int num = Wldap32.ldap_parse_reference(this.ldapHandle, referenceMessage, ref intPtr);
			try
			{
				if (num == 0)
				{
					int num1 = 0;
					if (intPtr != (IntPtr)0)
					{
						for (IntPtr i = Marshal.ReadIntPtr(intPtr, Marshal.SizeOf(typeof(IntPtr)) * num1); i != (IntPtr)0; i = Marshal.ReadIntPtr(intPtr, Marshal.SizeOf(typeof(IntPtr)) * num1))
						{
							string stringUni = Marshal.PtrToStringUni(i);
							arrayLists.Add(stringUni);
							num1++;
						}
						Wldap32.ldap_value_free(intPtr);
						intPtr = (IntPtr)0;
					}
					if (arrayLists.Count > 0)
					{
						Uri[] uri = new Uri[arrayLists.Count];
						for (int j = 0; j < arrayLists.Count; j++)
						{
							uri[j] = new Uri((string)arrayLists[j]);
						}
						searchResultReference = new SearchResultReference(uri);
					}
				}
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Wldap32.ldap_value_free(intPtr);
				}
			}
			return searchResultReference;
		}

		internal DirectoryResponse ConstructResponse(int messageId, LdapOperation operation, ResultAll resultType, TimeSpan requestTimeOut, bool exceptionOnTimeOut)
		{
			DirectoryResponse directoryResponse;
			LDAP_TIMEVAL lDAPTIMEVAL = new LDAP_TIMEVAL();
			lDAPTIMEVAL.tv_sec = (int)(requestTimeOut.Ticks / (long)0x989680);
			IntPtr intPtr = (IntPtr)0;
			DirectoryResponse searchResponse = null;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			bool flag = true;
			if (resultType != ResultAll.LDAP_MSG_ALL)
			{
				lDAPTIMEVAL.tv_sec = 0;
				lDAPTIMEVAL.tv_usec = 0;
				if (resultType == ResultAll.LDAP_MSG_POLLINGALL)
				{
					resultType = ResultAll.LDAP_MSG_ALL;
				}
				flag = false;
			}
			int num = Wldap32.ldap_result(this.ldapHandle, messageId, (int)resultType, lDAPTIMEVAL, ref intPtr);
			if (num == -1 || num == 0)
			{
				if (num != 0)
				{
					num = Wldap32.LdapGetLastError();
				}
				else
				{
					if (!exceptionOnTimeOut)
					{
						return null;
					}
					else
					{
						num = 85;
					}
				}
				if (flag)
				{
					Wldap32.ldap_abandon(this.ldapHandle, messageId);
				}
			}
			else
			{
				int num1 = 0;
				try
				{
					int num2 = 0;
					string str = null;
					string str1 = null;
					Uri[] uriArray = null;
					DirectoryControl[] directoryControlArray = null;
					if (num != 100 && num != 115)
					{
						num2 = this.ConstructParsedResult(intPtr, ref num1, ref str, ref str1, ref uriArray, ref directoryControlArray);
					}
					if (num2 != 0)
					{
						num = num2;
						throw this.ConstructException(num, operation);
					}
					else
					{
						num2 = num1;
						if (num != 105)
						{
							if (num != 103)
							{
								if (num != 107)
								{
									if (num != 109)
									{
										if (num != 111)
										{
											if (num != 120)
											{
												if (num == 101 || num == 100 || num == 115)
												{
													searchResponse = new SearchResponse(str, directoryControlArray, (ResultCode)num2, str1, uriArray);
													if (num == 101)
													{
														((SearchResponse)searchResponse).searchDone = true;
													}
													SearchResultEntryCollection searchResultEntryCollection = new SearchResultEntryCollection();
													SearchResultReferenceCollection searchResultReferenceCollection = new SearchResultReferenceCollection();
													IntPtr intPtr3 = Wldap32.ldap_first_entry(this.ldapHandle, intPtr);
													int num3 = 0;
													while (intPtr3 != (IntPtr)0)
													{
														SearchResultEntry searchResultEntry = this.ConstructEntry(intPtr3);
														if (searchResultEntry != null)
														{
															searchResultEntryCollection.Add(searchResultEntry);
														}
														num3++;
														intPtr3 = Wldap32.ldap_next_entry(this.ldapHandle, intPtr3);
													}
													IntPtr intPtr4 = Wldap32.ldap_first_reference(this.ldapHandle, intPtr);
													while (intPtr4 != (IntPtr)0)
													{
														SearchResultReference searchResultReference = this.ConstructReference(intPtr4);
														if (searchResultReference != null)
														{
															searchResultReferenceCollection.Add(searchResultReference);
														}
														intPtr4 = Wldap32.ldap_next_reference(this.ldapHandle, intPtr4);
													}
													((SearchResponse)searchResponse).SetEntries(searchResultEntryCollection);
													((SearchResponse)searchResponse).SetReferences(searchResultReferenceCollection);
												}
											}
											else
											{
												searchResponse = new ExtendedResponse(str, directoryControlArray, (ResultCode)num2, str1, uriArray);
												if (num2 == 0)
												{
													num2 = Wldap32.ldap_parse_extended_result(this.ldapHandle, intPtr, ref intPtr1, ref intPtr2, 0);
													if (num2 == 0)
													{
														string stringUni = null;
														if (intPtr1 != (IntPtr)0)
														{
															stringUni = Marshal.PtrToStringUni(intPtr1);
														}
														byte[] numArray = null;
														if (intPtr2 != (IntPtr)0)
														{
															berval _berval = new berval();
															Marshal.PtrToStructure(intPtr2, _berval);
															if (_berval.bv_len != 0 && _berval.bv_val != (IntPtr)0)
															{
																numArray = new byte[_berval.bv_len];
																Marshal.Copy(_berval.bv_val, numArray, 0, _berval.bv_len);
															}
														}
														((ExtendedResponse)searchResponse).name = stringUni;
														((ExtendedResponse)searchResponse).@value = numArray;
													}
												}
											}
										}
										else
										{
											searchResponse = new CompareResponse(str, directoryControlArray, (ResultCode)num2, str1, uriArray);
										}
									}
									else
									{
										searchResponse = new ModifyDNResponse(str, directoryControlArray, (ResultCode)num2, str1, uriArray);
									}
								}
								else
								{
									searchResponse = new DeleteResponse(str, directoryControlArray, (ResultCode)num2, str1, uriArray);
								}
							}
							else
							{
								searchResponse = new ModifyResponse(str, directoryControlArray, (ResultCode)num2, str1, uriArray);
							}
						}
						else
						{
							searchResponse = new AddResponse(str, directoryControlArray, (ResultCode)num2, str1, uriArray);
						}
						if (num2 == 0 || num2 == 5 || num2 == 6 || num2 == 10 || num2 == 9)
						{
							directoryResponse = searchResponse;
						}
						else
						{
							if (!Utility.IsResultCode((ResultCode)num2))
							{
								throw new DirectoryOperationException(searchResponse);
							}
							else
							{
								throw new DirectoryOperationException(searchResponse, OperationErrorMappings.MapResultCode(num2));
							}
						}
					}
				}
				finally
				{
					if (intPtr1 != (IntPtr)0)
					{
						Wldap32.ldap_memfree(intPtr1);
					}
					if (intPtr2 != (IntPtr)0)
					{
						Wldap32.ldap_memfree(intPtr2);
					}
					if (intPtr != (IntPtr)0)
					{
						Wldap32.ldap_msgfree(intPtr);
					}
				}
				return directoryResponse;
			}
			throw this.ConstructException(num, operation);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (LdapConnection.objectLock)
				{
					LdapConnection.handleTable.Remove(this.ldapHandle);
				}
			}
			if (this.needDispose && this.ldapHandle != null && !this.ldapHandle.IsInvalid)
			{
				this.ldapHandle.Dispose();
			}
			this.ldapHandle = null;
			this.disposed = true;
		}

		public DirectoryResponse EndSendRequest(IAsyncResult asyncResult)
		{
			if (!this.disposed)
			{
				if (asyncResult != null)
				{
					if (asyncResult as LdapAsyncResult != null)
					{
						LdapAsyncResult ldapAsyncResult = (LdapAsyncResult)asyncResult;
						if (ldapAsyncResult.partialResults)
						{
							LdapConnection.partialResultsProcessor.NeedCompleteResult((LdapPartialAsyncResult)asyncResult);
							asyncResult.AsyncWaitHandle.WaitOne();
							return LdapConnection.partialResultsProcessor.GetCompleteResult((LdapPartialAsyncResult)asyncResult);
						}
						else
						{
							if (LdapConnection.asyncResultTable.Contains(asyncResult))
							{
								LdapConnection.asyncResultTable.Remove(asyncResult);
								asyncResult.AsyncWaitHandle.WaitOne();
								if (ldapAsyncResult.resultObject.exception == null)
								{
									return ldapAsyncResult.resultObject.response;
								}
								else
								{
									throw ldapAsyncResult.resultObject.exception;
								}
							}
							else
							{
								throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
							}
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "asyncResult";
						throw new ArgumentException(Res.GetString("NotReturnedAsyncResult", objArray));
					}
				}
				else
				{
					throw new ArgumentNullException("asyncResult");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		~LdapConnection()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//base.Finalize();
			}
		}

		public PartialResultsCollection GetPartialResults(IAsyncResult asyncResult)
		{
			if (!this.disposed)
			{
				if (asyncResult != null)
				{
					if (asyncResult as LdapAsyncResult != null)
					{
						if (asyncResult as LdapPartialAsyncResult != null)
						{
							return LdapConnection.partialResultsProcessor.GetPartialResults((LdapPartialAsyncResult)asyncResult);
						}
						else
						{
							throw new InvalidOperationException(Res.GetString("NoPartialResults"));
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "asyncResult";
						throw new ArgumentException(Res.GetString("NotReturnedAsyncResult", objArray));
					}
				}
				else
				{
					throw new ArgumentNullException("asyncResult");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		internal void Init()
		{
			string[] servers;
			string str = null;
			if (this.directoryIdentifier == null)
			{
				servers = null;
			}
			else
			{
				servers = ((LdapDirectoryIdentifier)this.directoryIdentifier).Servers;
			}
			string[] strArrays = servers;
			if (strArrays != null && (int)strArrays.Length != 0)
			{
				StringBuilder stringBuilder = new StringBuilder(200);
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					if (strArrays[i] != null)
					{
						stringBuilder.Append(strArrays[i]);
						if (i < (int)strArrays.Length - 1)
						{
							stringBuilder.Append(" ");
						}
					}
				}
				if (stringBuilder.Length != 0)
				{
					str = stringBuilder.ToString();
				}
			}
			if (!((LdapDirectoryIdentifier)this.directoryIdentifier).Connectionless)
			{
				this.ldapHandle = new ConnectionHandle(Wldap32.ldap_init(str, ((LdapDirectoryIdentifier)this.directoryIdentifier).PortNumber));
			}
			else
			{
				this.ldapHandle = new ConnectionHandle(Wldap32.cldap_open(str, ((LdapDirectoryIdentifier)this.directoryIdentifier).PortNumber));
			}
			lock (LdapConnection.objectLock)
			{
				if (LdapConnection.handleTable[this.ldapHandle] != null)
				{
					LdapConnection.handleTable.Remove(this.ldapHandle);
				}
				LdapConnection.handleTable.Add(this.ldapHandle, new WeakReference(this));
			}
		}

		private bool ProcessClientCertificate(IntPtr ldapHandle, IntPtr CAs, ref IntPtr certificate)
		{
			int count;
			ArrayList arrayLists = new ArrayList();
			byte[][] item = null;
			if (base.ClientCertificates == null)
			{
				count = 0;
			}
			else
			{
				count = base.ClientCertificates.Count;
			}
			int num = count;
			if (num != 0 || this.options.clientCertificateDelegate != null)
			{
				if (this.options.clientCertificateDelegate != null)
				{
					if (CAs != (IntPtr)0)
					{
						SecPkgContext_IssuerListInfoEx structure = (SecPkgContext_IssuerListInfoEx)Marshal.PtrToStructure(CAs, typeof(SecPkgContext_IssuerListInfoEx));
						int num1 = structure.cIssuers;
						for (int i = 0; i < num1; i++)
						{
							IntPtr intPtr = (IntPtr)((long)structure.aIssuers + (long)(Marshal.SizeOf(typeof(CRYPTOAPI_BLOB)) * i));
							CRYPTOAPI_BLOB cRYPTOAPIBLOB = (CRYPTOAPI_BLOB)Marshal.PtrToStructure(intPtr, typeof(CRYPTOAPI_BLOB));
							int num2 = cRYPTOAPIBLOB.cbData;
							byte[] numArray = new byte[num2];
							Marshal.Copy(cRYPTOAPIBLOB.pbData, numArray, 0, num2);
							arrayLists.Add(numArray);
						}
					}
					if (arrayLists.Count != 0)
					{
						item = new byte[arrayLists.Count][];
						for (int j = 0; j < arrayLists.Count; j++)
						{
							item[j] = (byte[])arrayLists[j];
						}
					}
					X509Certificate x509Certificate = this.options.clientCertificateDelegate(this, item);
					if (x509Certificate == null)
					{
						certificate = (IntPtr)0;
						return false;
					}
					else
					{
						certificate = x509Certificate.Handle;
						return true;
					}
				}
				else
				{
					certificate = base.ClientCertificates[0].Handle;
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		private void ResponseCallback(IAsyncResult asyncResult)
		{
			LdapRequestState asyncState = (LdapRequestState)asyncResult.AsyncState;
			try
			{
				DirectoryResponse directoryResponse = this.fd.EndInvoke(asyncResult);
				asyncState.response = directoryResponse;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				asyncState.exception = exception;
				asyncState.response = null;
			}
			asyncState.ldapAsync.manualResetEvent.Set();
			asyncState.ldapAsync.completed = true;
			if (asyncState.ldapAsync.callback != null && !asyncState.abortCalled)
			{
				asyncState.ldapAsync.callback(asyncState.ldapAsync);
			}
		}

		private bool SameCredential(NetworkCredential oldCredential, NetworkCredential newCredential)
		{
			if (oldCredential != null || newCredential != null)
			{
				if (oldCredential != null || newCredential == null)
				{
					if (oldCredential == null || newCredential != null)
					{
						if (!(oldCredential.Domain == newCredential.Domain) || !(oldCredential.UserName == newCredential.UserName) || !(oldCredential.Password == newCredential.Password))
						{
							return false;
						}
						else
						{
							return true;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override DirectoryResponse SendRequest(DirectoryRequest request)
		{
			return this.SendRequest(request, this.connectionTimeOut);
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public DirectoryResponse SendRequest(DirectoryRequest request, TimeSpan requestTimeout)
		{
			if (!this.disposed)
			{
				if (request != null)
				{
					if (request as DsmlAuthRequest == null)
					{
						int num = 0;
						int num1 = this.SendRequestHelper(request, ref num);
						LdapOperation ldapOperation = LdapOperation.LdapSearch;
						if (request as DeleteRequest == null)
						{
							if (request as AddRequest == null)
							{
								if (request as ModifyRequest == null)
								{
									if (request as SearchRequest == null)
									{
										if (request as ModifyDNRequest == null)
										{
											if (request as CompareRequest == null)
											{
												if (request as ExtendedRequest != null)
												{
													ldapOperation = LdapOperation.LdapExtendedRequest;
												}
											}
											else
											{
												ldapOperation = LdapOperation.LdapCompare;
											}
										}
										else
										{
											ldapOperation = LdapOperation.LdapModifyDn;
										}
									}
									else
									{
										ldapOperation = LdapOperation.LdapSearch;
									}
								}
								else
								{
									ldapOperation = LdapOperation.LdapModify;
								}
							}
							else
							{
								ldapOperation = LdapOperation.LdapAdd;
							}
						}
						else
						{
							ldapOperation = LdapOperation.LdapDelete;
						}
						if (num1 != 0 || num == -1)
						{
							if (num1 == 0)
							{
								num1 = Wldap32.LdapGetLastError();
							}
							throw this.ConstructException(num1, ldapOperation);
						}
						else
						{
							return this.ConstructResponse(num, ldapOperation, ResultAll.LDAP_MSG_ALL, requestTimeout, true);
						}
					}
					else
					{
						throw new NotSupportedException(Res.GetString("DsmlAuthRequestNotSupported"));
					}
				}
				else
				{
					throw new ArgumentNullException("request");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		private int SendRequestHelper(DirectoryRequest request, ref int messageID)
		{
			IntPtr hGlobalUni;
			IntPtr intPtr;
			int num;
			int length;
			int count;
			int num1;
			IntPtr intPtr1 = (IntPtr)0;
			LdapControl[] ldapControlArray = null;
			IntPtr intPtr2 = (IntPtr)0;
			LdapControl[] ldapControlArray1 = null;
			string str = null;
			ArrayList arrayLists = new ArrayList();
			LdapMod[] ldapModArray = null;
			IntPtr intPtr3 = (IntPtr)0;
			int num2 = 0;
			berval _berval = null;
			IntPtr intPtr4 = (IntPtr)0;
			int num3 = 0;
			int num4 = 0;
			if (!this.connected)
			{
				this.Connect();
				this.connected = true;
			}
			if (this.AutoBind && (!this.bounded || this.needRebind) && !((LdapDirectoryIdentifier)this.Directory).Connectionless)
			{
				this.Bind();
			}
			try
			{
				ldapControlArray = this.BuildControlArray(request.Controls, true);
				int num5 = Marshal.SizeOf(typeof(LdapControl));
				if (ldapControlArray != null)
				{
					intPtr1 = Utility.AllocHGlobalIntPtrArray((int)ldapControlArray.Length + 1);
					for (int i = 0; i < (int)ldapControlArray.Length; i++)
					{
						hGlobalUni = Marshal.AllocHGlobal(num5);
						Marshal.StructureToPtr(ldapControlArray[i], hGlobalUni, false);
						intPtr = (IntPtr)((long)intPtr1 + (long)(Marshal.SizeOf(typeof(IntPtr)) * i));
						Marshal.WriteIntPtr(intPtr, hGlobalUni);
					}
					intPtr = (IntPtr)((long)intPtr1 + (long)(Marshal.SizeOf(typeof(IntPtr)) * (int)ldapControlArray.Length));
					Marshal.WriteIntPtr(intPtr, (IntPtr)0);
				}
				ldapControlArray1 = this.BuildControlArray(request.Controls, false);
				if (ldapControlArray1 != null)
				{
					intPtr2 = Utility.AllocHGlobalIntPtrArray((int)ldapControlArray1.Length + 1);
					for (int j = 0; j < (int)ldapControlArray1.Length; j++)
					{
						hGlobalUni = Marshal.AllocHGlobal(num5);
						Marshal.StructureToPtr(ldapControlArray1[j], hGlobalUni, false);
						intPtr = (IntPtr)((long)intPtr2 + (long)(Marshal.SizeOf(typeof(IntPtr)) * j));
						Marshal.WriteIntPtr(intPtr, hGlobalUni);
					}
					intPtr = (IntPtr)((long)intPtr2 + (long)(Marshal.SizeOf(typeof(IntPtr)) * (int)ldapControlArray1.Length));
					Marshal.WriteIntPtr(intPtr, (IntPtr)0);
				}
				if (request as DeleteRequest == null)
				{
					if (request as ModifyDNRequest == null)
					{
						if (request as CompareRequest == null)
						{
							if (request as AddRequest != null || request as ModifyRequest != null)
							{
								if (request as AddRequest == null)
								{
									ldapModArray = this.BuildAttributes(((ModifyRequest)request).Modifications, arrayLists);
								}
								else
								{
									ldapModArray = this.BuildAttributes(((AddRequest)request).Attributes, arrayLists);
								}
								if (ldapModArray == null)
								{
									length = 1;
								}
								else
								{
									length = (int)ldapModArray.Length + 1;
								}
								num2 = length;
								intPtr3 = Utility.AllocHGlobalIntPtrArray(num2);
								int num6 = Marshal.SizeOf(typeof(LdapMod));
								int k = 0;
								for (k = 0; k < num2 - 1; k++)
								{
									hGlobalUni = Marshal.AllocHGlobal(num6);
									Marshal.StructureToPtr(ldapModArray[k], hGlobalUni, false);
									intPtr = (IntPtr)((long)intPtr3 + (long)(Marshal.SizeOf(typeof(IntPtr)) * k));
									Marshal.WriteIntPtr(intPtr, hGlobalUni);
								}
								intPtr = (IntPtr)((long)intPtr3 + (long)(Marshal.SizeOf(typeof(IntPtr)) * k));
								Marshal.WriteIntPtr(intPtr, (IntPtr)0);
								if (request as AddRequest == null)
								{
									num4 = Wldap32.ldap_modify(this.ldapHandle, ((ModifyRequest)request).DistinguishedName, intPtr3, intPtr1, intPtr2, ref messageID);
								}
								else
								{
									num4 = Wldap32.ldap_add(this.ldapHandle, ((AddRequest)request).DistinguishedName, intPtr3, intPtr1, intPtr2, ref messageID);
								}
							}
							else
							{
								if (request as ExtendedRequest == null)
								{
									if (request as SearchRequest == null)
									{
										throw new NotSupportedException(Res.GetString("InvliadRequestType"));
									}
									else
									{
										SearchRequest searchRequest = (SearchRequest)request;
										object filter = searchRequest.Filter;
										if (filter == null || filter as XmlDocument == null)
										{
											string str1 = (string)filter;
											if (searchRequest.Attributes == null)
											{
												count = 0;
											}
											else
											{
												count = searchRequest.Attributes.Count;
											}
											num3 = count;
											if (num3 != 0)
											{
												intPtr4 = Utility.AllocHGlobalIntPtrArray(num3 + 1);
												int l = 0;
												for (l = 0; l < num3; l++)
												{
													hGlobalUni = Marshal.StringToHGlobalUni(searchRequest.Attributes[l]);
													intPtr = (IntPtr)((long)intPtr4 + (long)(Marshal.SizeOf(typeof(IntPtr)) * l));
													Marshal.WriteIntPtr(intPtr, hGlobalUni);
												}
												intPtr = (IntPtr)((long)intPtr4 + (long)(Marshal.SizeOf(typeof(IntPtr)) * l));
												Marshal.WriteIntPtr(intPtr, (IntPtr)0);
											}
											int scope = (int)searchRequest.Scope;
											TimeSpan timeLimit = searchRequest.TimeLimit;
											int ticks = (int)(timeLimit.Ticks / (long)0x989680);
											DereferenceAlias derefAlias = this.options.DerefAlias;
											this.options.DerefAlias = searchRequest.Aliases;
											try
											{
												num4 = Wldap32.ldap_search(this.ldapHandle, searchRequest.DistinguishedName, scope, str1, intPtr4, searchRequest.TypesOnly, intPtr1, intPtr2, ticks, searchRequest.SizeLimit, ref messageID);
											}
											finally
											{
												this.options.DerefAlias = derefAlias;
											}
										}
										else
										{
											throw new ArgumentException(Res.GetString("InvalidLdapSearchRequestFilter"));
										}
									}
								}
								else
								{
									string requestName = ((ExtendedRequest)request).RequestName;
									byte[] requestValue = ((ExtendedRequest)request).RequestValue;
									if (requestValue != null && (int)requestValue.Length != 0)
									{
										_berval = new berval();
										_berval.bv_len = (int)requestValue.Length;
										_berval.bv_val = Marshal.AllocHGlobal((int)requestValue.Length);
										Marshal.Copy(requestValue, 0, _berval.bv_val, (int)requestValue.Length);
									}
									num4 = Wldap32.ldap_extended_operation(this.ldapHandle, requestName, _berval, intPtr1, intPtr2, ref messageID);
								}
							}
						}
						else
						{
							DirectoryAttribute assertion = ((CompareRequest)request).Assertion;
							if (assertion != null)
							{
								if (assertion.Count == 1)
								{
									byte[] item = assertion[0] as byte[];
									if (item == null)
									{
										str = assertion[0].ToString();
									}
									else
									{
										if (item != null && (int)item.Length != 0)
										{
											_berval = new berval();
											_berval.bv_len = (int)item.Length;
											_berval.bv_val = Marshal.AllocHGlobal((int)item.Length);
											Marshal.Copy(item, 0, _berval.bv_val, (int)item.Length);
										}
									}
									num4 = Wldap32.ldap_compare(this.ldapHandle, ((CompareRequest)request).DistinguishedName, assertion.Name, str, _berval, intPtr1, intPtr2, ref messageID);
								}
								else
								{
									throw new ArgumentException(Res.GetString("WrongNumValuesCompare"));
								}
							}
							else
							{
								throw new ArgumentException(Res.GetString("WrongAssertionCompare"));
							}
						}
					}
					else
					{
						ConnectionHandle connectionHandle = this.ldapHandle;
						string distinguishedName = ((ModifyDNRequest)request).DistinguishedName;
						string newName = ((ModifyDNRequest)request).NewName;
						string newParentDistinguishedName = ((ModifyDNRequest)request).NewParentDistinguishedName;
						if (((ModifyDNRequest)request).DeleteOldRdn)
						{
							num1 = 1;
						}
						else
						{
							num1 = 0;
						}
						num4 = Wldap32.ldap_rename(connectionHandle, distinguishedName, newName, newParentDistinguishedName, num1, intPtr1, intPtr2, ref messageID);
					}
				}
				else
				{
					num4 = Wldap32.ldap_delete_ext(this.ldapHandle, ((DeleteRequest)request).DistinguishedName, intPtr1, intPtr2, ref messageID);
				}
				if (num4 == 85)
				{
					num4 = 112;
				}
				num = num4;
			}
			finally
			{
				GC.KeepAlive(ldapModArray);
				if (intPtr1 != (IntPtr)0)
				{
					for (int m = 0; m < (int)ldapControlArray.Length; m++)
					{
						IntPtr intPtr5 = Marshal.ReadIntPtr(intPtr1, Marshal.SizeOf(typeof(IntPtr)) * m);
						if (intPtr5 != (IntPtr)0)
						{
							Marshal.FreeHGlobal(intPtr5);
						}
					}
					Marshal.FreeHGlobal(intPtr1);
				}
				if (ldapControlArray != null)
				{
					for (int n = 0; n < (int)ldapControlArray.Length; n++)
					{
						if (ldapControlArray[n].ldctl_oid != (IntPtr)0)
						{
							Marshal.FreeHGlobal(ldapControlArray[n].ldctl_oid);
						}
						if (ldapControlArray[n].ldctl_value != null && ldapControlArray[n].ldctl_value.bv_val != (IntPtr)0)
						{
							Marshal.FreeHGlobal(ldapControlArray[n].ldctl_value.bv_val);
						}
					}
				}
				if (intPtr2 != (IntPtr)0)
				{
					for (int o = 0; o < (int)ldapControlArray1.Length; o++)
					{
						IntPtr intPtr6 = Marshal.ReadIntPtr(intPtr2, Marshal.SizeOf(typeof(IntPtr)) * o);
						if (intPtr6 != (IntPtr)0)
						{
							Marshal.FreeHGlobal(intPtr6);
						}
					}
					Marshal.FreeHGlobal(intPtr2);
				}
				if (ldapControlArray1 != null)
				{
					for (int p = 0; p < (int)ldapControlArray1.Length; p++)
					{
						if (ldapControlArray1[p].ldctl_oid != (IntPtr)0)
						{
							Marshal.FreeHGlobal(ldapControlArray1[p].ldctl_oid);
						}
						if (ldapControlArray1[p].ldctl_value != null && ldapControlArray1[p].ldctl_value.bv_val != (IntPtr)0)
						{
							Marshal.FreeHGlobal(ldapControlArray1[p].ldctl_value.bv_val);
						}
					}
				}
				if (intPtr3 != (IntPtr)0)
				{
					for (int q = 0; q < num2 - 1; q++)
					{
						IntPtr intPtr7 = Marshal.ReadIntPtr(intPtr3, Marshal.SizeOf(typeof(IntPtr)) * q);
						if (intPtr7 != (IntPtr)0)
						{
							Marshal.FreeHGlobal(intPtr7);
						}
					}
					Marshal.FreeHGlobal(intPtr3);
				}
				int num7 = 0;
				while (num7 < arrayLists.Count)
				{
					IntPtr item1 = (IntPtr)arrayLists[num7];
					Marshal.FreeHGlobal(item1);
					num7++;
				}
				if (_berval != null && _berval.bv_val != (IntPtr)0)
				{
					Marshal.FreeHGlobal(_berval.bv_val);
				}
				if (intPtr4 != (IntPtr)0)
				{
					for (int r = 0; r < num3; r++)
					{
						IntPtr intPtr8 = Marshal.ReadIntPtr(intPtr4, Marshal.SizeOf(typeof(IntPtr)) * r);
						if (intPtr8 != (IntPtr)0)
						{
							Marshal.FreeHGlobal(intPtr8);
						}
					}
					Marshal.FreeHGlobal(intPtr4);
				}
			}
			return num;
		}

		internal enum LdapResult
		{
			LDAP_RES_SEARCH_ENTRY = 100,
			LDAP_RES_SEARCH_RESULT = 101,
			LDAP_RES_MODIFY = 103,
			LDAP_RES_ADD = 105,
			LDAP_RES_DELETE = 107,
			LDAP_RES_MODRDN = 109,
			LDAP_RES_COMPARE = 111,
			LDAP_RES_REFERRAL = 115,
			LDAP_RES_EXTENDED = 120
		}
	}
}