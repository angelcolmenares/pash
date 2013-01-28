using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Set", "WSManQuickConfig", HelpUri="http://go.microsoft.com/fwlink/?LinkID=141463")]
	public class SetWSManQuickConfigCommand : PSCmdlet, IDisposable
	{
		private SwitchParameter usessl;

		private WSManHelper helper;

		private bool force;

		private bool skipNetworkProfileCheck;

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		[Parameter]
		public SwitchParameter SkipNetworkProfileCheck
		{
			get
			{
				return this.skipNetworkProfileCheck;
			}
			set
			{
				this.skipNetworkProfileCheck = value;
			}
		}

		[Parameter]
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SSL")]
		public SwitchParameter UseSSL
		{
			get
			{
				return this.usessl;
			}
			set
			{
				this.usessl = value;
			}
		}

		public SetWSManQuickConfigCommand()
		{
		}

		protected override void BeginProcessing()
		{
			WSManHelper.ThrowIfNotAdministrator();
			this.helper = new WSManHelper(this);
			string resourceMsgFromResourcetext = this.helper.GetResourceMsgFromResourcetext("QuickConfigContinueQuery");
			string str = this.helper.GetResourceMsgFromResourcetext("QuickConfigContinueCaption");
			if (this.force || base.ShouldContinue(resourceMsgFromResourcetext, str))
			{
				this.QuickConfigRemoting(true);
				this.QuickConfigRemoting(false);
				return;
			}
			else
			{
				return;
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose(IWSManSession sessionObject)
		{
			sessionObject = null;
			this.Dispose();
		}

		private void QuickConfigRemoting(bool serviceonly)
		{
			string str;
			string str1;
			string str2;
			string str3;
			string str4;
			string str5;
			string str6;
			string str7;
			string resourceString;
			string empty;
			IWSManSession wSManSession = null;
			try
			{
				IWSManEx wSManClass = (IWSManEx)(new WSManClass());
				wSManSession = (IWSManSession)wSManClass.CreateSession(null, 0, null);
				if (this.usessl)
				{
					str = "https";
				}
				else
				{
					str = "http";
				}
				if (!serviceonly)
				{
					if (this.skipNetworkProfileCheck)
					{
						empty = "<Force/>";
					}
					else
					{
						empty = string.Empty;
					}
					string str8 = empty;
					string[] strArrays = new string[5];
					strArrays[0] = "<Analyze_INPUT xmlns=\"http://schemas.microsoft.com/wbem/wsman/1/config/service\"><Transport>";
					strArrays[1] = str;
					strArrays[2] = "</Transport>";
					strArrays[3] = str8;
					strArrays[4] = "</Analyze_INPUT>";
					str4 = string.Concat(strArrays);
					str5 = "Analyze";
				}
				else
				{
					str4 = "<AnalyzeService_INPUT xmlns=\"http://schemas.microsoft.com/wbem/wsman/1/config/service\"></AnalyzeService_INPUT>";
					str5 = "AnalyzeService";
				}
				string str9 = wSManSession.Invoke(str5, "winrm/config/service", str4, 0);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(str9);
				if (!serviceonly)
				{
					str1 = "/cfg:Analyze_OUTPUT/cfg:RemotingEnabled";
					str2 = "/cfg:Analyze_OUTPUT/cfg:Results";
					str3 = "/cfg:Analyze_OUTPUT/cfg:EnableRemoting_INPUT";
				}
				else
				{
					str1 = "/cfg:AnalyzeService_OUTPUT/cfg:RemotingEnabled";
					str2 = "/cfg:AnalyzeService_OUTPUT/cfg:Results";
					str3 = "/cfg:AnalyzeService_OUTPUT/cfg:EnableService_INPUT";
				}
				XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManagers.AddNamespace("cfg", "http://schemas.microsoft.com/wbem/wsman/1/config/service");
				string innerText = xmlDocument.SelectSingleNode(str1, xmlNamespaceManagers).InnerText;
				XmlNode namedItem = xmlDocument.SelectSingleNode(str1, xmlNamespaceManagers).Attributes.GetNamedItem("Source");
				string value = null;
				if (namedItem != null)
				{
					value = namedItem.Value;
				}
				if (!innerText.Equals("true"))
				{
					if (innerText.Equals("false"))
					{
						string innerText1 = xmlDocument.SelectSingleNode(str2, xmlNamespaceManagers).InnerText;
						if (value == null || !value.Equals("GPO"))
						{
							string outerXml = xmlDocument.SelectSingleNode(str3, xmlNamespaceManagers).OuterXml;
							if (innerText1.Equals("") || outerXml.Equals(""))
							{
								ArgumentException argumentException = new ArgumentException(string.Concat(WSManResourceLoader.GetResourceString("L_ERR_Message"), WSManResourceLoader.GetResourceString("L_QuickConfig_MissingUpdateXml_0_ErrorMessage")));
								ErrorRecord errorRecord = new ErrorRecord(argumentException, "InvalidOperation", ErrorCategory.InvalidOperation, null);
								base.WriteError(errorRecord);
							}
							else
							{
								if (!serviceonly)
								{
									str5 = "EnableRemoting";
								}
								else
								{
									str5 = "EnableService";
								}
								string str10 = wSManSession.Invoke(str5, "winrm/config/service", outerXml, 0);
								XmlDocument xmlDocument1 = new XmlDocument();
								xmlDocument1.LoadXml(str10);
								if (!serviceonly)
								{
									str6 = "/cfg:EnableRemoting_OUTPUT/cfg:Status";
									str7 = "/cfg:EnableRemoting_OUTPUT/cfg:Results";
								}
								else
								{
									str6 = "/cfg:EnableService_OUTPUT/cfg:Status";
									str7 = "/cfg:EnableService_OUTPUT/cfg:Results";
								}
								if (!xmlDocument1.SelectSingleNode(str6, xmlNamespaceManagers).InnerText.ToString().Equals("succeeded"))
								{
									this.helper.AssertError(string.Concat(WSManResourceLoader.GetResourceString("L_ERR_Message"), WSManResourceLoader.GetResourceString("L_QuickConfigUpdateFailed_ErrorMessage")), false, null);
								}
								else
								{
									if (!serviceonly)
									{
										base.WriteObject(WSManResourceLoader.GetResourceString("L_QuickConfigUpdated_Message"));
									}
									else
									{
										base.WriteObject(WSManResourceLoader.GetResourceString("L_QuickConfigUpdatedService_Message"));
									}
									base.WriteObject(xmlDocument1.SelectSingleNode(str7, xmlNamespaceManagers).InnerText);
								}
							}
						}
						else
						{
							string resourceString1 = WSManResourceLoader.GetResourceString("L_QuickConfig_RemotingDisabledbyGP_00_ErrorMessage");
							resourceString1 = string.Concat(resourceString1, " ", innerText1);
							ArgumentException argumentException1 = new ArgumentException(resourceString1);
							base.WriteError(new ErrorRecord(argumentException1, "NotSpecified", ErrorCategory.NotSpecified, null));
						}
					}
					else
					{
						ArgumentException argumentException2 = new ArgumentException(WSManResourceLoader.GetResourceString("L_QuickConfig_InvalidBool_0_ErrorMessage"));
						ErrorRecord errorRecord1 = new ErrorRecord(argumentException2, "InvalidOperation", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord1);
					}
				}
				else
				{
					if (!serviceonly)
					{
						resourceString = WSManResourceLoader.GetResourceString("L_QuickConfigNoChangesNeeded_Message");
					}
					else
					{
						resourceString = WSManResourceLoader.GetResourceString("L_QuickConfigNoServiceChangesNeeded_Message");
					}
					base.WriteObject(resourceString);
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(wSManSession.Error))
				{
					this.helper.AssertError(wSManSession.Error, true, null);
				}
				if (wSManSession != null)
				{
					this.Dispose(wSManSession);
				}
			}
		}
	}
}