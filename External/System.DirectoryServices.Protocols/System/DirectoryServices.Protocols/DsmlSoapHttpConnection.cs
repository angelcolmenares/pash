using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DsmlSoapHttpConnection : DsmlSoapConnection
	{
		private HttpWebRequest dsmlHttpConnection;

		private string dsmlSoapAction;

		private AuthType dsmlAuthType;

		private string dsmlSessionID;

		private Hashtable httpConnectionTable;

		private string debugResponse;

		public AuthType AuthType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dsmlAuthType;
			}
			set
			{
				if (value < AuthType.Anonymous || value > AuthType.Kerberos)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(AuthType));
				}
				else
				{
					if (value == AuthType.Anonymous || value == AuthType.Ntlm || value == AuthType.Basic || value == AuthType.Negotiate || value == AuthType.Digest)
					{
						this.dsmlAuthType = value;
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = value;
						throw new ArgumentException(Res.GetString("WrongAuthType", objArray), "value");
					}
				}
			}
		}

		private string ResponseString
		{
			get
			{
				return this.debugResponse;
			}
		}

		public override string SessionId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dsmlSessionID;
			}
		}

		public string SoapActionHeader
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dsmlSoapAction;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dsmlSoapAction = value;
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
					if (value.TotalMilliseconds <= 2147483647)
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

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public DsmlSoapHttpConnection(Uri uri) : this(new DsmlDirectoryIdentifier(uri))
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		[WebPermission(SecurityAction.Assert, Unrestricted=true)]
		public DsmlSoapHttpConnection(DsmlDirectoryIdentifier identifier)
		{
			this.dsmlSoapAction = "\"#batchRequest\"";
			this.dsmlAuthType = AuthType.Negotiate;
			if (identifier != null)
			{
				this.directoryIdentifier = identifier;
				this.dsmlHttpConnection = (HttpWebRequest)WebRequest.Create(((DsmlDirectoryIdentifier)this.directoryIdentifier).ServerUri);
				Hashtable hashtables = new Hashtable();
				this.httpConnectionTable = Hashtable.Synchronized(hashtables);
				return;
			}
			else
			{
				throw new ArgumentNullException("identifier");
			}
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
		public DsmlSoapHttpConnection(DsmlDirectoryIdentifier identifier, NetworkCredential credential) : this(identifier)
		{
			NetworkCredential networkCredential;
			DsmlSoapHttpConnection dsmlSoapHttpConnection = this;
			if (credential != null)
			{
				networkCredential = new NetworkCredential(credential.UserName, credential.Password, credential.Domain);
			}
			else
			{
				networkCredential = null;
			}
			dsmlSoapHttpConnection.directoryCredential = networkCredential;
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public DsmlSoapHttpConnection(DsmlDirectoryIdentifier identifier, NetworkCredential credential, AuthType authType) : this(identifier, credential)
		{
			this.AuthType = authType;
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public void Abort(IAsyncResult asyncResult)
		{
			if (asyncResult != null)
			{
				if (asyncResult as DsmlAsyncResult != null)
				{
					if (this.httpConnectionTable.Contains(asyncResult))
					{
						HttpWebRequest item = (HttpWebRequest)this.httpConnectionTable[asyncResult];
						this.httpConnectionTable.Remove(asyncResult);
						item.Abort();
						DsmlAsyncResult dsmlAsyncResult = (DsmlAsyncResult)asyncResult;
						dsmlAsyncResult.resultObject.abortCalled = true;
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
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

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		[NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true)]
		[WebPermission(SecurityAction.Assert, Unrestricted=true)]
		public IAsyncResult BeginSendRequest(DsmlRequestDocument request, AsyncCallback callback, object state)
		{
			if (request != null)
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(((DsmlDirectoryIdentifier)this.directoryIdentifier).ServerUri);
				this.PrepareHttpWebRequest(httpWebRequest);
				StringBuilder stringBuilder = new StringBuilder(0x400);
				this.BeginSOAPRequest(ref stringBuilder);
				stringBuilder.Append(request.ToXml().InnerXml);
				this.EndSOAPRequest(ref stringBuilder);
				RequestState requestState = new RequestState();
				requestState.request = httpWebRequest;
				requestState.requestString = stringBuilder.ToString();
				DsmlAsyncResult dsmlAsyncResult = new DsmlAsyncResult(callback, state);
				dsmlAsyncResult.resultObject = requestState;
				if (request.Count > 0)
				{
					dsmlAsyncResult.hasValidRequest = true;
				}
				requestState.dsmlAsync = dsmlAsyncResult;
				this.httpConnectionTable.Add(dsmlAsyncResult, httpWebRequest);
				httpWebRequest.BeginGetRequestStream(new AsyncCallback(DsmlSoapHttpConnection.RequestStreamCallback), requestState);
				return dsmlAsyncResult;
			}
			else
			{
				throw new ArgumentNullException("request");
			}
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		[NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true)]
		[WebPermission(SecurityAction.Assert, Unrestricted=true)]
		public override void BeginSession()
		{
			if (this.dsmlSessionID == null)
			{
				try
				{
					this.PrepareHttpWebRequest(this.dsmlHttpConnection);
					StreamWriter webRequestStreamWriter = this.GetWebRequestStreamWriter();
					try
					{
						webRequestStreamWriter.Write("<se:Envelope xmlns:se=\"http://schemas.xmlsoap.org/soap/envelope/\">");
						webRequestStreamWriter.Write("<se:Header>");
						webRequestStreamWriter.Write("<ad:BeginSession xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" se:mustUnderstand=\"1\"/>");
						if (this.soapHeaders != null)
						{
							webRequestStreamWriter.Write(this.soapHeaders.OuterXml);
						}
						webRequestStreamWriter.Write("</se:Header>");
						webRequestStreamWriter.Write("<se:Body xmlns=\"urn:oasis:names:tc:DSML:2:0:core\">");
						webRequestStreamWriter.Write((new DsmlRequestDocument()).ToXml().InnerXml);
						webRequestStreamWriter.Write("</se:Body>");
						webRequestStreamWriter.Write("</se:Envelope>");
						webRequestStreamWriter.Flush();
					}
					finally
					{
						webRequestStreamWriter.BaseStream.Close();
						webRequestStreamWriter.Close();
					}
					HttpWebResponse response = (HttpWebResponse)this.dsmlHttpConnection.GetResponse();
					try
					{
						this.dsmlSessionID = this.ExtractSessionID(response);
					}
					finally
					{
						response.Close();
					}
				}
				finally
				{
					this.dsmlHttpConnection = (HttpWebRequest)WebRequest.Create(((DsmlDirectoryIdentifier)this.directoryIdentifier).ServerUri);
				}
				return;
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("SessionInUse"));
			}
		}

		private void BeginSOAPRequest(ref StringBuilder buffer)
		{
			buffer.Append("<se:Envelope xmlns:se=\"http://schemas.xmlsoap.org/soap/envelope/\">");
			if (this.dsmlSessionID != null || this.soapHeaders != null)
			{
				buffer.Append("<se:Header>");
				if (this.dsmlSessionID != null)
				{
					buffer.Append("<ad:Session xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" ad:SessionID=\"");
					buffer.Append(this.dsmlSessionID);
					buffer.Append("\" se:mustUnderstand=\"1\"/>");
				}
				if (this.soapHeaders != null)
				{
					buffer.Append(this.soapHeaders.OuterXml);
				}
				buffer.Append("</se:Header>");
			}
			buffer.Append("<se:Body xmlns=\"urn:oasis:names:tc:DSML:2:0:core\">");
		}

		public DsmlResponseDocument EndSendRequest(IAsyncResult asyncResult)
		{
			if (asyncResult != null)
			{
				if (asyncResult as DsmlAsyncResult != null)
				{
					if (this.httpConnectionTable.Contains(asyncResult))
					{
						this.httpConnectionTable.Remove(asyncResult);
						DsmlAsyncResult dsmlAsyncResult = (DsmlAsyncResult)asyncResult;
						asyncResult.AsyncWaitHandle.WaitOne();
						if (dsmlAsyncResult.resultObject.exception == null)
						{
							DsmlResponseDocument dsmlResponseDocuments = new DsmlResponseDocument(dsmlAsyncResult.resultObject.responseString, "se:Envelope/se:Body/dsml:batchResponse");
							this.debugResponse = dsmlResponseDocuments.ResponseString;
							if (!dsmlAsyncResult.hasValidRequest || dsmlResponseDocuments.Count != 0)
							{
								return dsmlResponseDocuments;
							}
							else
							{
								throw new DsmlInvalidDocumentException(Res.GetString("MissingResponse"));
							}
						}
						else
						{
							throw dsmlAsyncResult.resultObject.exception;
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
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

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		[NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true)]
		[WebPermission(SecurityAction.Assert, Unrestricted=true)]
		public override void EndSession()
		{
			if (this.dsmlSessionID != null)
			{
				try
				{
					try
					{
						this.PrepareHttpWebRequest(this.dsmlHttpConnection);
						StreamWriter webRequestStreamWriter = this.GetWebRequestStreamWriter();
						try
						{
							webRequestStreamWriter.Write("<se:Envelope xmlns:se=\"http://schemas.xmlsoap.org/soap/envelope/\">");
							webRequestStreamWriter.Write("<se:Header>");
							webRequestStreamWriter.Write("<ad:EndSession xmlns:ad=\"urn:schema-microsoft-com:activedirectory:dsmlv2\" ad:SessionID=\"");
							webRequestStreamWriter.Write(this.dsmlSessionID);
							webRequestStreamWriter.Write("\" se:mustUnderstand=\"1\"/>");
							if (this.soapHeaders != null)
							{
								webRequestStreamWriter.Write(this.soapHeaders.OuterXml);
							}
							webRequestStreamWriter.Write("</se:Header>");
							webRequestStreamWriter.Write("<se:Body xmlns=\"urn:oasis:names:tc:DSML:2:0:core\">");
							webRequestStreamWriter.Write((new DsmlRequestDocument()).ToXml().InnerXml);
							webRequestStreamWriter.Write("</se:Body>");
							webRequestStreamWriter.Write("</se:Envelope>");
							webRequestStreamWriter.Flush();
						}
						finally
						{
							webRequestStreamWriter.BaseStream.Close();
							webRequestStreamWriter.Close();
						}
						HttpWebResponse response = (HttpWebResponse)this.dsmlHttpConnection.GetResponse();
						response.Close();
					}
					catch (WebException webException1)
					{
						WebException webException = webException1;
						if (webException.Status != WebExceptionStatus.ConnectFailure && webException.Status != WebExceptionStatus.NameResolutionFailure && webException.Status != WebExceptionStatus.ProxyNameResolutionFailure && webException.Status != WebExceptionStatus.SendFailure && webException.Status != WebExceptionStatus.TrustFailure)
						{
							this.dsmlSessionID = null;
						}
						throw;
					}
					this.dsmlSessionID = null;
				}
				finally
				{
					this.dsmlHttpConnection = (HttpWebRequest)WebRequest.Create(((DsmlDirectoryIdentifier)this.directoryIdentifier).ServerUri);
				}
				return;
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("NoCurrentSession"));
			}
		}

		private void EndSOAPRequest(ref StringBuilder buffer)
		{
			buffer.Append("</se:Body>");
			buffer.Append("</se:Envelope>");
		}

		private string ExtractSessionID(HttpWebResponse resp)
		{
			string value;
			Stream responseStream = resp.GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream);
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.Load(streamReader);
				}
				catch (XmlException xmlException)
				{
					throw new DsmlInvalidDocumentException();
				}
				XmlNamespaceManager dsmlNamespaceManager = NamespaceUtils.GetDsmlNamespaceManager();
				XmlAttribute xmlAttribute = (XmlAttribute)xmlDocument.SelectSingleNode("se:Envelope/se:Header/ad:Session/@ad:SessionID", dsmlNamespaceManager);
				if (xmlAttribute == null)
				{
					xmlAttribute = (XmlAttribute)xmlDocument.SelectSingleNode("se:Envelope/se:Header/ad:Session/@SessionID", dsmlNamespaceManager);
					if (xmlAttribute == null)
					{
						throw new DsmlInvalidDocumentException(Res.GetString("NoSessionIDReturned"));
					}
				}
				value = xmlAttribute.Value;
			}
			finally
			{
				streamReader.Close();
			}
			return value;
		}

		private StreamWriter GetWebRequestStreamWriter()
		{
			Stream requestStream = this.dsmlHttpConnection.GetRequestStream();
			StreamWriter streamWriter = new StreamWriter(requestStream);
			return streamWriter;
		}

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		private void PrepareHttpWebRequest(HttpWebRequest dsmlConnection)
		{
			if (this.directoryCredential != null)
			{
				string str = "negotiate";
				if (this.dsmlAuthType != AuthType.Ntlm)
				{
					if (this.dsmlAuthType != AuthType.Basic)
					{
						if (this.dsmlAuthType != AuthType.Anonymous)
						{
							if (this.dsmlAuthType == AuthType.Digest)
							{
								str = "digest";
							}
						}
						else
						{
							str = "anonymous";
						}
					}
					else
					{
						str = "basic";
					}
				}
				else
				{
					str = "NTLM";
				}
				CredentialCache credentialCaches = new CredentialCache();
				credentialCaches.Add(dsmlConnection.RequestUri, str, this.directoryCredential);
				dsmlConnection.Credentials = credentialCaches;
			}
			else
			{
				dsmlConnection.Credentials = CredentialCache.DefaultCredentials;
			}
			foreach (X509Certificate clientCertificate in base.ClientCertificates)
			{
				dsmlConnection.ClientCertificates.Add(clientCertificate);
			}
			if (this.connectionTimeOut.Ticks != (long)0)
			{
				dsmlConnection.Timeout = (int)(this.connectionTimeOut.Ticks / (long)0x2710);
			}
			if (this.dsmlSoapAction != null)
			{
				WebHeaderCollection headers = dsmlConnection.Headers;
				headers.Set("SOAPAction", this.dsmlSoapAction);
			}
			dsmlConnection.Method = "POST";
		}

		private static void ReadCallback(IAsyncResult asyncResult)
		{
			RequestState asyncState = (RequestState)asyncResult.AsyncState;
			try
			{
				int num = asyncState.responseStream.EndRead(asyncResult);
				if (num <= 0)
				{
					asyncState.responseStream.Close();
					DsmlSoapHttpConnection.WakeupRoutine(asyncState);
				}
				else
				{
					string str = asyncState.encoder.GetString(asyncState.bufferRead);
					int num1 = Math.Min(str.Length, num);
					asyncState.responseString.Append(str, 0, num1);
					asyncState.responseStream.BeginRead(asyncState.bufferRead, 0, 0x400, new AsyncCallback(DsmlSoapHttpConnection.ReadCallback), asyncState);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				asyncState.responseStream.Close();
				asyncState.exception = exception;
				DsmlSoapHttpConnection.WakeupRoutine(asyncState);
			}
		}

		private static void RequestStreamCallback(IAsyncResult asyncResult)
		{
			RequestState asyncState = (RequestState)asyncResult.AsyncState;
			HttpWebRequest httpWebRequest = asyncState.request;
			try
			{
				asyncState.requestStream = httpWebRequest.EndGetRequestStream(asyncResult);
				byte[] bytes = asyncState.encoder.GetBytes(asyncState.requestString);
				asyncState.requestStream.BeginWrite(bytes, 0, (int)bytes.Length, new AsyncCallback(DsmlSoapHttpConnection.WriteCallback), asyncState);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (asyncState.requestStream != null)
				{
					asyncState.requestStream.Close();
				}
				asyncState.exception = exception;
				DsmlSoapHttpConnection.WakeupRoutine(asyncState);
			}
		}

		private static void ResponseCallback(IAsyncResult asyncResult)
		{
			RequestState asyncState = (RequestState)asyncResult.AsyncState;
			try
			{
				WebResponse webResponse = asyncState.request.EndGetResponse(asyncResult);
				asyncState.responseStream = webResponse.GetResponseStream();
				asyncState.responseStream.BeginRead(asyncState.bufferRead, 0, 0x400, new AsyncCallback(DsmlSoapHttpConnection.ReadCallback), asyncState);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (asyncState.responseStream != null)
				{
					asyncState.responseStream.Close();
				}
				asyncState.exception = exception;
				DsmlSoapHttpConnection.WakeupRoutine(asyncState);
			}
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override DirectoryResponse SendRequest(DirectoryRequest request)
		{
			if (request != null)
			{
				DsmlRequestDocument dsmlRequestDocuments = new DsmlRequestDocument();
				dsmlRequestDocuments.Add(request);
				DsmlResponseDocument dsmlResponseDocuments = this.SendRequestHelper(dsmlRequestDocuments.ToXml().InnerXml);
				if (dsmlResponseDocuments.Count != 0)
				{
					DirectoryResponse item = dsmlResponseDocuments[0];
					if (item as DsmlErrorResponse == null)
					{
						ResultCode resultCode = item.ResultCode;
						if (resultCode == ResultCode.Success || resultCode == ResultCode.CompareFalse || resultCode == ResultCode.CompareTrue || resultCode == ResultCode.Referral || resultCode == ResultCode.ReferralV2)
						{
							return item;
						}
						else
						{
							throw new DirectoryOperationException(item, OperationErrorMappings.MapResultCode((int)resultCode));
						}
					}
					else
					{
						ErrorResponseException errorResponseException = new ErrorResponseException((DsmlErrorResponse)item);
						throw errorResponseException;
					}
				}
				else
				{
					throw new DsmlInvalidDocumentException(Res.GetString("MissingResponse"));
				}
			}
			else
			{
				throw new ArgumentNullException("request");
			}
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public DsmlResponseDocument SendRequest(DsmlRequestDocument request)
		{
			if (request != null)
			{
				DsmlResponseDocument dsmlResponseDocuments = this.SendRequestHelper(request.ToXml().InnerXml);
				if (request.Count <= 0 || dsmlResponseDocuments.Count != 0)
				{
					return dsmlResponseDocuments;
				}
				else
				{
					throw new DsmlInvalidDocumentException(Res.GetString("MissingResponse"));
				}
			}
			else
			{
				throw new ArgumentNullException("request");
			}
		}

		[NetworkInformationPermission(SecurityAction.Assert, Unrestricted=true)]
		[WebPermission(SecurityAction.Assert, Unrestricted=true)]
		private DsmlResponseDocument SendRequestHelper(string reqstring)
		{
			DsmlResponseDocument dsmlResponseDocuments;
			DsmlResponseDocument dsmlResponseDocuments1;
			StringBuilder stringBuilder = new StringBuilder(0x400);
			try
			{
				this.PrepareHttpWebRequest(this.dsmlHttpConnection);
				StreamWriter webRequestStreamWriter = this.GetWebRequestStreamWriter();
				try
				{
					this.BeginSOAPRequest(ref stringBuilder);
					stringBuilder.Append(reqstring);
					this.EndSOAPRequest(ref stringBuilder);
					webRequestStreamWriter.Write(stringBuilder.ToString());
					webRequestStreamWriter.Flush();
				}
				finally
				{
					webRequestStreamWriter.BaseStream.Close();
					webRequestStreamWriter.Close();
				}
				HttpWebResponse response = (HttpWebResponse)this.dsmlHttpConnection.GetResponse();
				try
				{
					dsmlResponseDocuments = new DsmlResponseDocument(response, "se:Envelope/se:Body/dsml:batchResponse");
					this.debugResponse = dsmlResponseDocuments.ResponseString;
				}
				finally
				{
					response.Close();
				}
				dsmlResponseDocuments1 = dsmlResponseDocuments;
			}
			finally
			{
				this.dsmlHttpConnection = (HttpWebRequest)WebRequest.Create(((DsmlDirectoryIdentifier)this.directoryIdentifier).ServerUri);
			}
			return dsmlResponseDocuments1;
		}

		private static void WakeupRoutine(RequestState rs)
		{
			rs.dsmlAsync.manualResetEvent.Set();
			rs.dsmlAsync.completed = true;
			if (rs.dsmlAsync.callback != null && !rs.abortCalled)
			{
				rs.dsmlAsync.callback(rs.dsmlAsync);
			}
		}

		private static void WriteCallback(IAsyncResult asyncResult)
		{
			RequestState asyncState = (RequestState)asyncResult.AsyncState;
			try
			{
				try
				{
					asyncState.requestStream.EndWrite(asyncResult);
					asyncState.request.BeginGetResponse(new AsyncCallback(DsmlSoapHttpConnection.ResponseCallback), asyncState);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					asyncState.exception = exception;
					DsmlSoapHttpConnection.WakeupRoutine(asyncState);
				}
			}
			finally
			{
				asyncState.requestStream.Close();
			}
		}
	}
}