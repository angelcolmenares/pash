using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="0#")]
	internal class WSManHelper
	{
		private const string PTRN_URI_LAST = "([a-z_][-a-z0-9._]*)$";

		private const string PTRN_OPT = "^-([a-z]+):(.*)";

		private const string PTRN_HASH_TOK = "\\s*([\\w:]+)\\s*=\\s*(\\$null|\"([^\"]*)\")\\s*";

		private const string URI_IPMI = "http://schemas.dmtf.org/wbem/wscim/1/cim-schema";

		private const string URI_WMI = "http://schemas.microsoft.com/wbem/wsman/1/wmi";

		private const string NS_IPMI = "http://schemas.dmtf.org/wbem/wscim/1/cim-schema";

		private const string NS_CIMBASE = "http://schemas.dmtf.org/wbem/wsman/1/base";

		private const string NS_WSMANL = "http://schemas.microsoft.com";

		private const string NS_XSI = "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

		private const string ATTR_NIL = "xsi:nil=\"true\"";

		private const string ATTR_NIL_NAME = "xsi:nil";

		private const string NS_XSI_URI = "http://www.w3.org/2001/XMLSchema-instance";

		private const string ALIAS_XPATH = "xpath";

		private const string URI_XPATH_DIALECT = "http://www.w3.org/TR/1999/REC-xpath-19991116";

		private const string NODE_ATTRIBUTE = "2";

		private const int NODE_TEXT = 3;

		internal string CredSSP_RUri;

		internal string CredSSP_XMLNmsp;

		internal string CredSSP_SNode;

		internal string Client_uri;

		internal string urlprefix_node;

		internal string Client_XMLNmsp;

		internal string Service_Uri;

		internal string Service_UrlPrefix_Node;

		internal string Service_XMLNmsp;

		internal string Service_CredSSP_Uri;

		internal string Service_CredSSP_XMLNmsp;

		internal string Registry_Path_Credentials_Delegation;

		internal string Key_Allow_Fresh_Credentials;

		internal string Key_Concatenate_Defaults_AllowFresh;

		internal string Delegate;

		internal string keyAllowcredssp;

		internal string ALIAS_WQL;

		internal string ALIAS_ASSOCIATION;

		internal string ALIAS_SELECTOR;

		internal string URI_WQL_DIALECT;

		internal string URI_SELECTOR_DIALECT;

		internal string URI_ASSOCIATION_DIALECT;

		internal string WSManOp;

		private ResourceManager _resourceMgr;

		private PSCmdlet cmdletname;

		private NavigationCmdletProvider _provider;

		private FileStream _fs;

		private StreamReader _sr;

		private static ResourceManager g_resourceMgr;

		internal static WSManHelper.Sessions AutoSession;

		static WSManHelper()
		{
            WSManHelper.g_resourceMgr = new ResourceManager("Microsoft.WSMan.Management.WsManResources", Assembly.GetExecutingAssembly());
			WSManHelper.AutoSession = new WSManHelper.Sessions();
		}

		internal WSManHelper()
		{
			this.CredSSP_RUri = "winrm/config/client/auth";
			this.CredSSP_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/client/auth";
			this.CredSSP_SNode = "/cfg:Auth/cfg:CredSSP";
			this.Client_uri = "winrm/config/client";
			this.urlprefix_node = "/cfg:Client/cfg:URLPrefix";
			this.Client_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/client";
			this.Service_Uri = "winrm/config/service";
			this.Service_UrlPrefix_Node = "/cfg:Service/cfg:URLPrefix";
			this.Service_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/service";
			this.Service_CredSSP_Uri = "winrm/config/service/auth";
			this.Service_CredSSP_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/service/auth";
			this.Registry_Path_Credentials_Delegation = "SOFTWARE\\Policies\\Microsoft\\Windows";
			this.Key_Allow_Fresh_Credentials = "AllowFreshCredentials";
			this.Key_Concatenate_Defaults_AllowFresh = "ConcatenateDefaults_AllowFresh";
			this.Delegate = "delegate";
			this.keyAllowcredssp = "AllowCredSSP";
			this.ALIAS_WQL = "wql";
			this.ALIAS_ASSOCIATION = "association";
			this.ALIAS_SELECTOR = "selector";
			this.URI_WQL_DIALECT = "http://schemas.microsoft.com/wbem/wsman/1/WQL";
			this.URI_SELECTOR_DIALECT = "http://schemas.dmtf.org/wbem/wsman/1/wsman/SelectorFilter";
			this.URI_ASSOCIATION_DIALECT = " http://schemas.dmtf.org/wbem/wsman/1/cimbinding/associationFilter";
            this._resourceMgr = new ResourceManager("Microsoft.WSMan.Management.WsManResources", Assembly.GetExecutingAssembly());
		}

		internal WSManHelper(PSCmdlet cmdlet)
		{
			this.CredSSP_RUri = "winrm/config/client/auth";
			this.CredSSP_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/client/auth";
			this.CredSSP_SNode = "/cfg:Auth/cfg:CredSSP";
			this.Client_uri = "winrm/config/client";
			this.urlprefix_node = "/cfg:Client/cfg:URLPrefix";
			this.Client_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/client";
			this.Service_Uri = "winrm/config/service";
			this.Service_UrlPrefix_Node = "/cfg:Service/cfg:URLPrefix";
			this.Service_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/service";
			this.Service_CredSSP_Uri = "winrm/config/service/auth";
			this.Service_CredSSP_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/service/auth";
			this.Registry_Path_Credentials_Delegation = "SOFTWARE\\Policies\\Microsoft\\Windows";
			this.Key_Allow_Fresh_Credentials = "AllowFreshCredentials";
			this.Key_Concatenate_Defaults_AllowFresh = "ConcatenateDefaults_AllowFresh";
			this.Delegate = "delegate";
			this.keyAllowcredssp = "AllowCredSSP";
			this.ALIAS_WQL = "wql";
			this.ALIAS_ASSOCIATION = "association";
			this.ALIAS_SELECTOR = "selector";
			this.URI_WQL_DIALECT = "http://schemas.microsoft.com/wbem/wsman/1/WQL";
			this.URI_SELECTOR_DIALECT = "http://schemas.dmtf.org/wbem/wsman/1/wsman/SelectorFilter";
			this.URI_ASSOCIATION_DIALECT = " http://schemas.dmtf.org/wbem/wsman/1/cimbinding/associationFilter";
			this.cmdletname = cmdlet;
			this._resourceMgr = new ResourceManager("WsManResources", Assembly.GetExecutingAssembly());
		}

		internal WSManHelper(NavigationCmdletProvider provider)
		{
			this.CredSSP_RUri = "winrm/config/client/auth";
			this.CredSSP_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/client/auth";
			this.CredSSP_SNode = "/cfg:Auth/cfg:CredSSP";
			this.Client_uri = "winrm/config/client";
			this.urlprefix_node = "/cfg:Client/cfg:URLPrefix";
			this.Client_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/client";
			this.Service_Uri = "winrm/config/service";
			this.Service_UrlPrefix_Node = "/cfg:Service/cfg:URLPrefix";
			this.Service_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/service";
			this.Service_CredSSP_Uri = "winrm/config/service/auth";
			this.Service_CredSSP_XMLNmsp = "http://schemas.microsoft.com/wbem/wsman/1/config/service/auth";
			this.Registry_Path_Credentials_Delegation = "SOFTWARE\\Policies\\Microsoft\\Windows";
			this.Key_Allow_Fresh_Credentials = "AllowFreshCredentials";
			this.Key_Concatenate_Defaults_AllowFresh = "ConcatenateDefaults_AllowFresh";
			this.Delegate = "delegate";
			this.keyAllowcredssp = "AllowCredSSP";
			this.ALIAS_WQL = "wql";
			this.ALIAS_ASSOCIATION = "association";
			this.ALIAS_SELECTOR = "selector";
			this.URI_WQL_DIALECT = "http://schemas.microsoft.com/wbem/wsman/1/WQL";
			this.URI_SELECTOR_DIALECT = "http://schemas.dmtf.org/wbem/wsman/1/wsman/SelectorFilter";
			this.URI_ASSOCIATION_DIALECT = " http://schemas.dmtf.org/wbem/wsman/1/cimbinding/associationFilter";
			this._provider = provider;
            this._resourceMgr = new ResourceManager("Microsoft.WSMan.Management.WsManResources", Assembly.GetExecutingAssembly());
		}

		internal void AddtoDictionary(string key, object value)
		{
			key = key.ToLower(CultureInfo.InvariantCulture);
			lock (WSManHelper.Sessions.SessionObjCache)
			{
				if (WSManHelper.Sessions.SessionObjCache.ContainsKey(key))
				{
					object obj = null;
					WSManHelper.Sessions.SessionObjCache.TryGetValue(key, out obj);
					try
					{
						Marshal.ReleaseComObject(obj);
					}
					catch (ArgumentException argumentException)
					{
					}
					WSManHelper.Sessions.SessionObjCache.Remove(key);
					WSManHelper.Sessions.SessionObjCache.Add(key, value);
				}
				else
				{
					WSManHelper.Sessions.SessionObjCache.Add(key, value);
				}
			}
		}

		internal void AssertError(string ErrorMessage, bool IsWSManError, object targetobject)
		{
			if (!IsWSManError)
			{
				InvalidOperationException invalidOperationException = new InvalidOperationException(ErrorMessage);
				ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, "WsManError", ErrorCategory.InvalidOperation, targetobject);
				if (this.cmdletname == null)
				{
					this._provider.ThrowTerminatingError(errorRecord);
					return;
				}
				else
				{
					this.cmdletname.ThrowTerminatingError(errorRecord);
					return;
				}
			}
			else
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(ErrorMessage);
				InvalidOperationException invalidOperationException1 = new InvalidOperationException(xmlDocument.OuterXml);
				ErrorRecord errorRecord1 = new ErrorRecord(invalidOperationException1, "WsManError", ErrorCategory.InvalidOperation, targetobject);
				if (this.cmdletname == null)
				{
					this._provider.ThrowTerminatingError(errorRecord1);
					return;
				}
				else
				{
					this.cmdletname.ThrowTerminatingError(errorRecord1);
					return;
				}
			}
		}

		internal void CleanUp()
		{
			if (this._sr != null)
			{
				this._sr.Close();
				this._sr = null;
			}
			if (this._fs != null)
			{
				this._fs.Close();
				this._fs = null;
			}
		}

		internal string CreateConnectionString(Uri ConnUri, int port, string computername, string applicationname)
		{
			string originalString;
			if (ConnUri == null)
			{
				if (computername == null && (port != 0 || applicationname != null))
				{
					computername = "localhost";
				}
				originalString = computername;
				if (port != 0)
				{
					originalString = string.Concat(originalString, ":", port);
				}
				if (applicationname != null)
				{
					originalString = string.Concat(originalString, "/", applicationname);
				}
			}
			else
			{
				originalString = ConnUri.OriginalString;
			}
			return originalString;
		}

		internal IWSManSession CreateSessionObject(IWSManEx wsmanObject, AuthenticationMechanism authentication, SessionOption sessionoption, PSCredential credential, string connectionString, string certificateThumbprint, bool usessl)
		{
			WSManHelper.ValidateSpecifiedAuthentication(authentication, credential, certificateThumbprint);
			int num = 0;
			if (authentication.ToString() != null)
			{
				if (authentication.Equals(AuthenticationMechanism.None))
				{
					num = num | 0x8000;
				}
				if (authentication.Equals(AuthenticationMechanism.Basic))
				{
					num = num | 0x40000 | 0x1000;
				}
				if (authentication.Equals(AuthenticationMechanism.Negotiate))
				{
					num = num | 0x20000;
				}
				if (authentication.Equals(AuthenticationMechanism.Kerberos))
				{
					num = num | 0x80000;
				}
				if (authentication.Equals(AuthenticationMechanism.Digest))
				{
					num = num | 0x10000 | 0x1000;
				}
				if (authentication.Equals(AuthenticationMechanism.Credssp))
				{
					num = num | 0x1000000 | 0x1000;
				}
				if (authentication.Equals(AuthenticationMechanism.ClientCertificate))
				{
					num = num | 0x200000;
				}
			}
			IWSManConnectionOptionsEx2 userName = (IWSManConnectionOptionsEx2)wsmanObject.CreateConnectionOptions();
			if (credential != null)
			{
				if (credential.UserName != null)
				{
					NetworkCredential networkCredential = credential.GetNetworkCredential();
					if (!string.IsNullOrEmpty(networkCredential.Domain))
					{
						userName.UserName = string.Concat(networkCredential.Domain, "\\", networkCredential.UserName);
					}
					else
					{
						if (authentication.Equals(AuthenticationMechanism.Digest) || authentication.Equals(AuthenticationMechanism.Basic))
						{
							userName.UserName = networkCredential.UserName;
						}
						else
						{
							userName.UserName = string.Concat("\\", networkCredential.UserName);
						}
					}
					userName.Password = networkCredential.Password;
					if (!authentication.Equals(AuthenticationMechanism.Credssp) || !authentication.Equals(AuthenticationMechanism.Digest) || authentication.Equals(AuthenticationMechanism.Basic))
					{
						num = num | 0x1000;
					}
				}
			}
			if (certificateThumbprint != null)
			{
				userName.CertificateThumbprint = certificateThumbprint;
				num = num | 0x200000;
			}
			if (sessionoption == null)
			{
				num = num | 1;
			}
			else
			{
				if (sessionoption.ProxyAuthentication != 0)
				{
					int num1 = 0;
					int num2 = 0;
					if (!sessionoption.ProxyAccessType.Equals(ProxyAccessType.ProxyIEConfig))
					{
						if (!sessionoption.ProxyAccessType.Equals(ProxyAccessType.ProxyAutoDetect))
						{
							if (!sessionoption.ProxyAccessType.Equals(ProxyAccessType.ProxyNoProxyServer))
							{
								if (sessionoption.ProxyAccessType.Equals(ProxyAccessType.ProxyWinHttpConfig))
								{
									num1 = userName.ProxyWinHttpConfig();
								}
							}
							else
							{
								num1 = userName.ProxyNoProxyServer();
							}
						}
						else
						{
							num1 = userName.ProxyAutoDetect();
						}
					}
					else
					{
						num1 = userName.ProxyIEConfig();
					}
					if (!sessionoption.ProxyAuthentication.Equals(ProxyAuthentication.Basic))
					{
						if (!sessionoption.ProxyAuthentication.Equals(ProxyAuthentication.Negotiate))
						{
							if (sessionoption.ProxyAuthentication.Equals(ProxyAuthentication.Digest))
							{
								num2 = userName.ProxyAuthenticationUseDigest();
							}
						}
						else
						{
							num2 = userName.ProxyAuthenticationUseNegotiate();
						}
					}
					else
					{
						num2 = userName.ProxyAuthenticationUseBasic();
					}
					if (sessionoption.ProxyCredential == null)
					{
                        userName.SetProxy((int)sessionoption.ProxyAccessType, (int)sessionoption.ProxyAuthentication, null, null);
					}
					else
					{
						try
						{
							userName.SetProxy(num1, num2, sessionoption.ProxyCredential.UserName, sessionoption.ProxyCredential.Password);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							this.AssertError(exception.Message, false, null);
						}
					}
				}
				if (sessionoption.SkipCACheck)
				{
					num = num | 0x2000;
				}
				if (sessionoption.SkipCNCheck)
				{
					num = num | 0x4000;
				}
				if (sessionoption.SPNPort > 0)
				{
					num = num | 0x400000;
				}
				if (!sessionoption.UseUtf16)
				{
					num = num | 1;
				}
				else
				{
					num = num | 0x800000;
				}
				if (!sessionoption.UseEncryption)
				{
					num = num | 0x100000;
				}
				if (sessionoption.SkipRevocationCheck)
				{
					num = num | 0x2000000;
				}
			}
			if (usessl)
			{
				num = num | 0x8000000;
			}
			IWSManSession operationTimeout = null;
			try
			{
				operationTimeout = (IWSManSession)wsmanObject.CreateSession(connectionString, num, userName);
				if (sessionoption != null && sessionoption.OperationTimeout > 0)
				{
					operationTimeout.Timeout = sessionoption.OperationTimeout;
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				this.AssertError(cOMException.Message, false, null);
			}
			return operationTimeout;
		}

		internal void CreateWsManConnection(string ParameterSetName, Uri connectionuri, int port, string computername, string applicationname, bool usessl, AuthenticationMechanism authentication, SessionOption sessionoption, PSCredential credential, string certificateThumbprint)
		{
			IWSManEx wSManClass = (IWSManEx)(new WSManClass());
			try
			{
				try
				{
					string str = this.CreateConnectionString(connectionuri, port, computername, applicationname);
					if (connectionuri != null)
					{
						string[] strArrays = new string[1];
						object[] objArray = new object[4];
						objArray[0] = ":";
						objArray[1] = port;
						objArray[2] = "/";
						objArray[3] = applicationname;
						strArrays[0] = string.Concat(objArray);
						string[] strArrays1 = str.Split(strArrays, StringSplitOptions.None);
						string[] strArrays2 = new string[1];
						strArrays2[0] = "//";
						string[] strArrays3 = strArrays1[0].Split(strArrays2, StringSplitOptions.None);
						computername = strArrays3[1].Trim();
					}
					IWSManSession wSManSession = this.CreateSessionObject(wSManClass, authentication, sessionoption, credential, str, certificateThumbprint, usessl);
					wSManSession.Identify(0);
					string str1 = computername;
					if (str1 == null)
					{
						str1 = "localhost";
					}
					this.AddtoDictionary(str1, wSManSession);
				}
				catch (IndexOutOfRangeException indexOutOfRangeException)
				{
					this.AssertError(this._resourceMgr.GetString("NotProperURI"), false, connectionuri);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.AssertError(exception.Message, false, computername);
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(wSManClass.Error))
				{
					this.AssertError(wSManClass.Error, true, computername);
				}
			}
		}

		internal string FormatResourceMsgFromResourcetext(string resourceName, object[] args)
		{
			return WSManHelper.FormatResourceMsgFromResourcetextS(this._resourceMgr, resourceName, args);
		}

		internal static string FormatResourceMsgFromResourcetextS(string rscname, object[] args)
		{
			ResourceManager resourceManager = new ResourceManager("WsManResources", typeof(WSManHelper).Assembly);
			return WSManHelper.FormatResourceMsgFromResourcetextS(resourceManager, rscname, args);
		}

		private static string FormatResourceMsgFromResourcetextS(ResourceManager resourceManager, string resourceName, object[] args)
		{
			if (resourceManager != null)
			{
				if (!string.IsNullOrEmpty(resourceName))
				{
					string str = resourceManager.GetString(resourceName);
					string str1 = null;
					if (str != null)
					{
						str1 = string.Format(Thread.CurrentThread.CurrentCulture, str, args);
					}
					return str1;
				}
				else
				{
					throw new ArgumentNullException("resourceName");
				}
			}
			else
			{
				throw new ArgumentNullException("resourceManager");
			}
		}

		internal string GetFilterString(Hashtable seletorset)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (DictionaryEntry dictionaryEntry in seletorset)
			{
				if (dictionaryEntry.Key == null || dictionaryEntry.Value == null)
				{
					continue;
				}
				stringBuilder.Append(dictionaryEntry.Key.ToString());
				stringBuilder.Append("=");
				stringBuilder.Append(dictionaryEntry.Value.ToString());
				stringBuilder.Append("+");
			}
			stringBuilder.Remove(stringBuilder.ToString().Length - 1, 1);
			return stringBuilder.ToString();
		}

		internal string GetResourceMsgFromResourcetext(string rscname)
		{
			return this._resourceMgr.GetString(rscname);
		}

		internal string GetRootNodeName(string operation, string resourceUri, string actionStr)
		{
			string str = null;
			if (resourceUri != null)
			{
				str = resourceUri;
				str = this.StripParams(str);
				Regex regex = new Regex("([a-z_][-a-z0-9._]*)$", RegexOptions.IgnoreCase);
				MatchCollection matchCollections = regex.Matches(str);
				if (matchCollections.Count > 0)
				{
					if (!operation.Equals("invoke", StringComparison.OrdinalIgnoreCase))
					{
						str = matchCollections[0].ToString();
					}
					else
					{
						string str1 = "_INPUT";
						str = string.Concat(actionStr, str1);
					}
				}
			}
			return str;
		}

		internal static Dictionary<string, object> GetSessionObjCache()
		{
			try
			{
				lock (WSManHelper.Sessions.SessionObjCache)
				{
					if (!WSManHelper.Sessions.SessionObjCache.ContainsKey("localhost"))
					{
						IWSManEx wSManClass = (IWSManEx)(new WSManClass());
						IWSManSession wSManSession = (IWSManSession)wSManClass.CreateSession(null, 0, null);
						WSManHelper.Sessions.SessionObjCache.Add("localhost", wSManSession);
					}
				}
			}
			catch (IOException oException)
			{
			}
			catch (SecurityException securityException)
			{
			}
			catch (UnauthorizedAccessException unauthorizedAccessException)
			{
			}
			catch (COMException cOMException)
			{
			}
			return WSManHelper.Sessions.SessionObjCache;
		}

		internal string GetURIWithFilter(string uri, string filter, Hashtable selectorset, string operation)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(uri);
			stringBuilder.Append("?");
			if (operation.Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				stringBuilder.Append(this.GetFilterString(selectorset));
				if (stringBuilder.ToString().EndsWith("?", StringComparison.OrdinalIgnoreCase))
				{
					stringBuilder.Remove(stringBuilder.Length - 1, 1);
				}
			}
			return stringBuilder.ToString();
		}

		internal XmlNode GetXmlNode(string xmlString, string xpathpattern, string xmlnamespace)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlString);
			XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
			if (!string.IsNullOrEmpty(xmlnamespace))
			{
				xmlNamespaceManagers.AddNamespace("cfg", xmlnamespace);
			}
			XmlNode xmlNodes = xmlDocument.SelectSingleNode(xpathpattern, xmlNamespaceManagers);
			return xmlNodes;
		}

		internal string GetXmlNs(string resUri)
		{
			string str;
			if (resUri.ToLower(CultureInfo.InvariantCulture).Contains("http://schemas.dmtf.org/wbem/wscim/1/cim-schema") || resUri.ToLower(CultureInfo.InvariantCulture).Contains("http://schemas.microsoft.com/wbem/wsman/1/wmi"))
			{
				str = this.StripParams(resUri);
			}
			else
			{
				str = this.StripParams(resUri);
			}
			return string.Concat("xmlns:p=\"", str, "\"");
		}

		internal IWSManResourceLocator InitializeResourceLocator(Hashtable optionset, Hashtable selectorset, string fragment, Uri dialect, IWSManEx wsmanObj, Uri resourceuri)
		{
			string str = null;
			if (resourceuri != null)
			{
				str = resourceuri.ToString();
			}
			if (selectorset != null)
			{
				str = string.Concat(str, "?");
				int num = 0;
				foreach (DictionaryEntry dictionaryEntry in selectorset)
				{
					num++;
					str = string.Concat(str, dictionaryEntry.Key.ToString(), "=", dictionaryEntry.Value.ToString());
					if (num >= selectorset.Count)
					{
						continue;
					}
					str = string.Concat(str, "+");
				}
			}
			IWSManResourceLocator wSManResourceLocator = null;
			try
			{
				wSManResourceLocator = (IWSManResourceLocator)wsmanObj.CreateResourceLocator(str);
				if (optionset != null)
				{
					foreach (DictionaryEntry dictionaryEntry1 in optionset)
					{
						if (dictionaryEntry1.Value.ToString() != null)
						{
							wSManResourceLocator.AddOption(dictionaryEntry1.Key.ToString(), dictionaryEntry1.Value, 1);
						}
						else
						{
							wSManResourceLocator.AddOption(dictionaryEntry1.Key.ToString(), null, 1);
						}
					}
				}
				if (!string.IsNullOrEmpty(fragment))
				{
					wSManResourceLocator.FragmentPath = fragment;
				}
				if (dialect != null)
				{
					wSManResourceLocator.FragmentDialect = dialect.ToString();
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				this.AssertError(cOMException.Message, false, null);
			}
			return wSManResourceLocator;
		}

		internal string ProcessInput(IWSManEx wsman, string filepath, string operation, string root, Hashtable valueset, IWSManResourceLocator resourceUri, IWSManSession sessionObj)
		{
			string outerXml = null;
			if (string.IsNullOrEmpty(filepath) || valueset != null)
			{
				string str = operation;
				string str1 = str;
				if (str != null)
				{
					if (str1 == "new" || str1 == "invoke")
					{
						string str2 = null;
						string str3 = null;
						string xmlNs = this.GetXmlNs(resourceUri.resourceUri);
						if (valueset != null)
						{
							foreach (DictionaryEntry dictionaryEntry in valueset)
							{
								str2 = string.Concat(str2, "<p:", dictionaryEntry.Key.ToString());
								if (dictionaryEntry.Value.ToString() == null)
								{
									str2 = string.Concat(str2, " xsi:nil=\"true\"");
									str3 = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
								}
								string[] strArrays = new string[6];
								strArrays[0] = str2;
								strArrays[1] = ">";
								strArrays[2] = dictionaryEntry.Value.ToString();
								strArrays[3] = "</p:";
								strArrays[4] = dictionaryEntry.Key.ToString();
								strArrays[5] = ">";
								str2 = string.Concat(strArrays);
							}
						}
						string[] strArrays1 = new string[10];
						strArrays1[0] = "<p:";
						strArrays1[1] = root;
						strArrays1[2] = " ";
						strArrays1[3] = xmlNs;
						strArrays1[4] = str3;
						strArrays1[5] = ">";
						strArrays1[6] = str2;
						strArrays1[7] = "</p:";
						strArrays1[8] = root;
						strArrays1[9] = ">";
						outerXml = string.Concat(strArrays1);
					}
					else
					{
						if (str1 == "set")
						{
							string str4 = sessionObj.Get(resourceUri, 0);
							XmlDocument xmlDocument = new XmlDocument();
							xmlDocument.LoadXml(str4);
							if (valueset != null)
							{
								foreach (DictionaryEntry dictionaryEntry1 in valueset)
								{
									string str5 = string.Concat("/*/*[local-name()=\"", dictionaryEntry1.Key, "\"]");
									if (dictionaryEntry1.Key.ToString().Equals("location", StringComparison.OrdinalIgnoreCase))
									{
										str5 = string.Concat("/*/*[local-name()=\"", dictionaryEntry1.Key, "\" and namespace-uri() != \"http://schemas.dmtf.org/wbem/wsman/1/base\"]");
									}
									XmlNodeList xmlNodeLists = xmlDocument.SelectNodes(str5);
									if (xmlNodeLists.Count != 0)
									{
										if (xmlNodeLists.Count <= 1)
										{
											XmlNode itemOf = xmlNodeLists[0];
											if (itemOf.HasChildNodes)
											{
												if (itemOf.ChildNodes.Count <= 1)
												{
													XmlNode xmlNodes = itemOf.ChildNodes[0];
													if (!xmlNodes.NodeType.ToString().Equals("text", StringComparison.OrdinalIgnoreCase))
													{
														throw new ArgumentException(this._resourceMgr.GetString("NOAttributeMatch"));
													}
												}
												else
												{
													throw new ArgumentException(this._resourceMgr.GetString("NOAttributeMatch"));
												}
											}
											if (!string.IsNullOrEmpty(dictionaryEntry1.Key.ToString()))
											{
												itemOf.Attributes.RemoveNamedItem("xsi:nil");
												itemOf.InnerText = dictionaryEntry1.Value.ToString();
											}
											else
											{
												XmlAttribute xmlAttribute = xmlDocument.CreateAttribute(XmlNodeType.Attribute.ToString(), "xsi:nil", "http://www.w3.org/2001/XMLSchema-instance");
												xmlAttribute.Value = "true";
												itemOf.Attributes.Append(xmlAttribute);
												itemOf.Value = "";
											}
										}
										else
										{
											throw new ArgumentException(this._resourceMgr.GetString("MultipleResourceMatch"));
										}
									}
									else
									{
										throw new ArgumentException(this._resourceMgr.GetString("NoResourceMatch"));
									}
								}
							}
							outerXml = xmlDocument.OuterXml;
						}
					}
				}
				return outerXml;
			}
			else
			{
				if (File.Exists(filepath))
				{
					outerXml = this.ReadFile(filepath);
					return outerXml;
				}
				else
				{
					throw new FileNotFoundException(this._resourceMgr.GetString("InvalidFileName"));
				}
			}
		}

		internal string ReadFile(string path)
		{
			if (File.Exists(path))
			{
				string end = null;
				try
				{
					try
					{
						this._fs = new FileStream(path, FileMode.Open, FileAccess.Read);
						this._sr = new StreamReader(this._fs);
						end = this._sr.ReadToEnd();
					}
					catch (ArgumentNullException argumentNullException1)
					{
						ArgumentNullException argumentNullException = argumentNullException1;
						ErrorRecord errorRecord = new ErrorRecord(argumentNullException, "ArgumentNullException", ErrorCategory.InvalidArgument, null);
						this.cmdletname.ThrowTerminatingError(errorRecord);
					}
					catch (UnauthorizedAccessException unauthorizedAccessException1)
					{
						UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
						ErrorRecord errorRecord1 = new ErrorRecord(unauthorizedAccessException, "UnauthorizedAccessException", ErrorCategory.PermissionDenied, null);
						this.cmdletname.ThrowTerminatingError(errorRecord1);
					}
					catch (FileNotFoundException fileNotFoundException1)
					{
						FileNotFoundException fileNotFoundException = fileNotFoundException1;
						ErrorRecord errorRecord2 = new ErrorRecord(fileNotFoundException, "FileNotFoundException", ErrorCategory.ObjectNotFound, null);
						this.cmdletname.ThrowTerminatingError(errorRecord2);
					}
					catch (DirectoryNotFoundException directoryNotFoundException1)
					{
						DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
						ErrorRecord errorRecord3 = new ErrorRecord(directoryNotFoundException, "DirectoryNotFoundException", ErrorCategory.ObjectNotFound, null);
						this.cmdletname.ThrowTerminatingError(errorRecord3);
					}
					catch (SecurityException securityException1)
					{
						SecurityException securityException = securityException1;
						ErrorRecord errorRecord4 = new ErrorRecord(securityException, "SecurityException", ErrorCategory.SecurityError, null);
						this.cmdletname.ThrowTerminatingError(errorRecord4);
					}
				}
				finally
				{
					if (this._sr != null)
					{
						this._sr.Close();
					}
					if (this._fs != null)
					{
						this._fs.Close();
					}
				}
				return end;
			}
			else
			{
				throw new ArgumentException(this.GetResourceMsgFromResourcetext("InvalidFileName"));
			}
		}

		internal static void ReleaseSessions()
		{
			object obj = null;
			lock (WSManHelper.Sessions.SessionObjCache)
			{
				foreach (string key in WSManHelper.Sessions.SessionObjCache.Keys)
				{
					WSManHelper.Sessions.SessionObjCache.TryGetValue(key, out obj);
					try
					{
						Marshal.ReleaseComObject(obj);
					}
					catch (ArgumentException argumentException)
					{
					}
					obj = null;
				}
				WSManHelper.Sessions.SessionObjCache.Clear();
			}
		}

		internal object RemoveFromDictionary(string computer)
		{
			object obj = null;
			computer = computer.ToLower(CultureInfo.InvariantCulture);
			lock (WSManHelper.Sessions.SessionObjCache)
			{
				if (WSManHelper.Sessions.SessionObjCache.ContainsKey(computer))
				{
					WSManHelper.Sessions.SessionObjCache.TryGetValue(computer, out obj);
					try
					{
						Marshal.ReleaseComObject(obj);
					}
					catch (ArgumentException argumentException)
					{
					}
					WSManHelper.Sessions.SessionObjCache.Remove(computer);
				}
			}
			return obj;
		}

		internal string StripParams(string uri)
		{
			int num = uri.IndexOf('?');
			if (num <= 0)
			{
				return uri;
			}
			else
			{
				return uri.Substring(num, uri.Length - num);
			}
		}

		internal static void ThrowIfNotAdministrator()
		{
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
			if (OSHelper.IsUnix || windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
			{
				return;
			}
			else
			{
				string str = WSManHelper.g_resourceMgr.GetString("ErrorElevationNeeded");
				throw new InvalidOperationException(str);
			}
		}

		internal bool ValidateCreadSSPRegistryRetry(bool AllowFreshCredentialsValueShouldBePresent, string[] DelegateComputer, string applicationname)
		{
			int num = 0;
			while (num < 60)
			{
				if (this.ValidateCredSSPRegistry(AllowFreshCredentialsValueShouldBePresent, DelegateComputer, applicationname))
				{
					return true;
				}
				else
				{
					Thread.Sleep(0x3e8);
					num++;
				}
			}
			return false;
		}

		internal bool ValidateCredSSPRegistry(bool AllowFreshCredentialsValueShouldBePresent, string[] DelegateComputer, string applicationname)
		{
			bool allowFreshCredentialsValueShouldBePresent;
			IntPtr intPtr = GpoNativeApi.EnterCriticalPolicySection(true);
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(string.Concat(this.Registry_Path_Credentials_Delegation, "\\CredentialsDelegation"), RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
				if (registryKey != null)
				{
					registryKey = registryKey.OpenSubKey(this.Key_Allow_Fresh_Credentials, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
					if (registryKey != null)
					{
						string[] valueNames = registryKey.GetValueNames();
						if ((int)valueNames.Length > 0)
						{
							List<string> strs = new List<string>();
							string[] strArrays = valueNames;
							for (int i = 0; i < (int)strArrays.Length; i++)
							{
								string str = strArrays[i];
								object value = registryKey.GetValue(str);
								if (value != null && value.ToString().StartsWith(applicationname, StringComparison.OrdinalIgnoreCase))
								{
									if (AllowFreshCredentialsValueShouldBePresent)
									{
										strs.Add(value.ToString());
									}
									else
									{
										allowFreshCredentialsValueShouldBePresent = false;
										return allowFreshCredentialsValueShouldBePresent;
									}
								}
							}
							if (AllowFreshCredentialsValueShouldBePresent)
							{
								string[] delegateComputer = DelegateComputer;
								int num = 0;
								while (num < (int)delegateComputer.Length)
								{
									string str1 = delegateComputer[num];
									if (strs.Contains(string.Concat(applicationname, "/", str1)))
									{
										num++;
									}
									else
									{
										allowFreshCredentialsValueShouldBePresent = false;
										return allowFreshCredentialsValueShouldBePresent;
									}
								}
							}
						}
						else
						{
							allowFreshCredentialsValueShouldBePresent = !AllowFreshCredentialsValueShouldBePresent;
							return allowFreshCredentialsValueShouldBePresent;
						}
					}
					else
					{
						allowFreshCredentialsValueShouldBePresent = !AllowFreshCredentialsValueShouldBePresent;
						return allowFreshCredentialsValueShouldBePresent;
					}
				}
				return true;
			}
			finally
			{
				GpoNativeApi.LeaveCriticalPolicySection(intPtr);
			}
			return allowFreshCredentialsValueShouldBePresent;
		}

		internal static void ValidateSpecifiedAuthentication(AuthenticationMechanism authentication, PSCredential credential, string certificateThumbprint)
		{
			if (credential == null || certificateThumbprint == null)
			{
				if (authentication == AuthenticationMechanism.Default || authentication == AuthenticationMechanism.ClientCertificate || certificateThumbprint == null)
				{
					return;
				}
				else
				{
					object[] str = new object[2];
					str[0] = "CertificateThumbPrint";
					str[1] = authentication.ToString();
					string str1 = WSManHelper.FormatResourceMsgFromResourcetextS("AmbiguosAuthentication", str);
					throw new InvalidOperationException(str1);
				}
			}
			else
			{
				object[] objArray = new object[2];
				objArray[0] = "CertificateThumbPrint";
				objArray[1] = "credential";
				string str2 = WSManHelper.FormatResourceMsgFromResourcetextS("AmbiguosAuthentication", objArray);
				throw new InvalidOperationException(str2);
			}
		}

		internal class Sessions
		{
			internal static Dictionary<string, object> SessionObjCache;

			static Sessions()
			{
				WSManHelper.Sessions.SessionObjCache = new Dictionary<string, object>();
			}

			public Sessions()
			{
			}

            ~Sessions()
			{
				try
				{
					WSManHelper.ReleaseSessions();
				}
				finally
				{
					
				}
			}
		}
	}
}