using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("New", "ADDCCloneConfigFile", HelpUri="http://go.microsoft.com/fwlink/?LinkId=246280", DefaultParameterSetName="IPv4DynamicSettings")]
	public class NewADDCCloneConfigFile : ADCmdletBase<NewADDCCloneConfigFileParameterSet>
	{
		private const int MAX_DNS_NUM = 4;

		private const int MAX_CONFIG_NUM = 3;

		private const int MAX_PATH = 0x100;

		private bool _offline;

		private string _path;

		private string _computerName;

		private string _siteName;

		private string _ipv4Address;

		private string _ipv4DefaultGateway;

		private string _ipv4SubnetMask;

		private string[] _ipv4DnsResolver;

		private string[] _ipv6DnsResolver;

		private string _preferredWinsServer;

		private string _alternateWinsServer;

		private bool _static;

		private bool _ipv4Static;

		private bool _ipv6Static;

		private bool _ipv4Dynamic;

		private bool _ipv6Dynamic;

		public NewADDCCloneConfigFile()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.NewADDCCloneConfigFileBaseProcessCSRoutine));
		}

		private bool CheckExistingFiles(bool fPrintResult)
		{
			bool flag = false;
			int num = 0x300;
			char[] chrArray = new char[num];
			for (int i = 0; i < num; i++)
			{
				chrArray[i] = ' ';
			}
			string str = new string(chrArray);
			UnsafeNativeMethods.DsRolepCheckExistingCloneConfigFile(out flag, str, num);
			if (!flag)
			{
				this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNotExistingMessage, new object[0]));
			}
			else
			{
				if (fPrintResult)
				{
					str = str.Trim();
					object[] objArray = new object[1];
					objArray[0] = str;
					this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileExistingMessage, objArray));
				}
			}
			return flag;
		}

		private void ConsoleWrite(string output)
		{
			object[] objArray = new object[1];
			objArray[0] = output;
			base.InvokeCommand.InvokeScript("Write-Host $args[0]", false, PipelineResultTypes.None, null, objArray);
		}

		private string GenerateXmlContentString()
		{
			string str = "<?xml version=\"1.0\"?><d3c:DCCloneConfig xmlns:d3c=\"uri:microsoft.com:schemas:DCCloneConfig\">";
			if (!string.IsNullOrEmpty(this._computerName))
			{
				str = string.Concat(str, "<ComputerName>", this._computerName, "</ComputerName>");
			}
			if (!string.IsNullOrEmpty(this._siteName))
			{
				str = string.Concat(str, "<SiteName>", this._siteName, "</SiteName>");
			}
			if (this._ipv4Static || this._ipv6Static || this._ipv4Dynamic || this._ipv6Dynamic)
			{
				str = string.Concat(str, "<IPSettings>");
				if (this._ipv4Static || this._ipv4Dynamic)
				{
					str = string.Concat(str, "<IPv4Settings>");
					if (this._ipv4Static)
					{
						str = string.Concat(str, "<StaticSettings>");
						if (!string.IsNullOrEmpty(this._ipv4Address))
						{
							str = string.Concat(str, "<Address>", this._ipv4Address, "</Address>");
						}
						if (!string.IsNullOrEmpty(this._ipv4SubnetMask))
						{
							str = string.Concat(str, "<SubnetMask>", this._ipv4SubnetMask, "</SubnetMask>");
						}
						if (!string.IsNullOrEmpty(this._ipv4DefaultGateway))
						{
							str = string.Concat(str, "<DefaultGateway>", this._ipv4DefaultGateway, "</DefaultGateway>");
						}
						if (this._ipv4DnsResolver != null)
						{
							string[] strArrays = this._ipv4DnsResolver;
							for (int i = 0; i < (int)strArrays.Length; i++)
							{
								string str1 = strArrays[i];
								string str2 = str1.Trim();
								if (!string.IsNullOrEmpty(str2))
								{
									str = string.Concat(str, "<DNSResolver>", str2, "</DNSResolver>");
								}
							}
						}
						if (!string.IsNullOrEmpty(this._preferredWinsServer))
						{
							str = string.Concat(str, "<PreferredWINSServer>", this._preferredWinsServer, "</PreferredWINSServer>");
						}
						if (!string.IsNullOrEmpty(this._alternateWinsServer))
						{
							str = string.Concat(str, "<AlternateWINSServer>", this._alternateWinsServer, "</AlternateWINSServer>");
						}
						str = string.Concat(str, "</StaticSettings>");
					}
					if (this._ipv4Dynamic)
					{
						str = string.Concat(str, "<DynamicSettings>");
						if (this._ipv4DnsResolver != null)
						{
							string[] strArrays1 = this._ipv4DnsResolver;
							for (int j = 0; j < (int)strArrays1.Length; j++)
							{
								string str3 = strArrays1[j];
								string str4 = str3.Trim();
								if (!string.IsNullOrEmpty(str4))
								{
									str = string.Concat(str, "<DNSResolver>", str4, "</DNSResolver>");
								}
							}
						}
						if (!string.IsNullOrEmpty(this._preferredWinsServer))
						{
							str = string.Concat(str, "<PreferredWINSServer>", this._preferredWinsServer, "</PreferredWINSServer>");
						}
						if (!string.IsNullOrEmpty(this._alternateWinsServer))
						{
							str = string.Concat(str, "<AlternateWINSServer>", this._alternateWinsServer, "</AlternateWINSServer>");
						}
						str = string.Concat(str, "</DynamicSettings>");
					}
					str = string.Concat(str, "</IPv4Settings>");
				}
				if (this._ipv6Static || this._ipv6Dynamic)
				{
					str = string.Concat(str, "<IPv6Settings>");
					if (this._ipv6Static)
					{
						str = string.Concat(str, "<StaticSettings>");
						if (this._ipv6DnsResolver != null)
						{
							string[] strArrays2 = this._ipv6DnsResolver;
							for (int k = 0; k < (int)strArrays2.Length; k++)
							{
								string str5 = strArrays2[k];
								string str6 = str5.Trim();
								if (!string.IsNullOrEmpty(str6))
								{
									str = string.Concat(str, "<DNSResolver>", str6, "</DNSResolver>");
								}
							}
						}
						str = string.Concat(str, "</StaticSettings>");
					}
					if (this._ipv6Dynamic)
					{
						str = string.Concat(str, "<DynamicSettings>");
						if (this._ipv6DnsResolver != null)
						{
							string[] strArrays3 = this._ipv6DnsResolver;
							for (int l = 0; l < (int)strArrays3.Length; l++)
							{
								string str7 = strArrays3[l];
								string str8 = str7.Trim();
								if (!string.IsNullOrEmpty(str8))
								{
									str = string.Concat(str, "<DNSResolver>", str8, "</DNSResolver>");
								}
							}
						}
						str = string.Concat(str, "</DynamicSettings>");
					}
					str = string.Concat(str, "</IPv6Settings>");
				}
				str = string.Concat(str, "</IPSettings>");
			}
			str = string.Concat(str, "</d3c:DCCloneConfig>");
			return str;
		}

		private string GenerateXmlFile()
		{
			this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileGenerationMessage, new object[0]));
			string outputFileFullName = this.GetOutputFileFullName();
			object[] objArray = new object[1];
			objArray[0] = outputFileFullName;
			this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileFullNameMessage, objArray));
			this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileGeneratingContentMessage, new object[0]));
			string str = this.GenerateXmlContentString();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(str);
			XmlTextWriter xmlTextWriter = new XmlTextWriter(outputFileFullName, new UTF8Encoding(false));
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlDocument.WriteContentTo(xmlTextWriter);
			xmlTextWriter.Close();
			this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileGeneratedMessage, new object[0]));
			this.ConsoleWrite("");
			return outputFileFullName;
		}

		private ADGroup GetADGroup(string cloneableGroupName)
		{
			ADGroup aDGroup;
			string str = string.Concat("Get-ADGroup -Identity \"", cloneableGroupName, "\"");
			Collection<PSObject> pSObjects = null;
			try
			{
				pSObjects = base.InvokeCommand.InvokeScript(str);
				goto Label0;
			}
			catch (RuntimeException runtimeException1)
			{
				RuntimeException runtimeException = runtimeException1;
				base.WriteError(new ErrorRecord(runtimeException, "0", ErrorCategory.ReadError, str));
				aDGroup = null;
			}
			return aDGroup;
		Label0:
			if (pSObjects == null || pSObjects.Count == 0)
			{
				return null;
			}
			else
			{
				return pSObjects[0].BaseObject as ADGroup;
			}
		}

		private string GetDitLocation()
		{
			string str = "HKLM:\\SYSTEM\\CurrentControlSet\\Services\\NTDS\\Parameters";
			string str1 = "DSA Working Directory";
			this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileGetDitLocationMessage, new object[0]));
			object[] objArray = new object[2];
			objArray[0] = str;
			objArray[1] = str1;
			Collection<PSObject> pSObjects = base.InvokeCommand.InvokeScript("Get-ItemProperty -Path $args[0] -Name $args[1]", false, PipelineResultTypes.None, null, objArray);
			if (pSObjects == null || pSObjects.Count == 0)
			{
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNoDitLocationMessage, new object[0]));
			}
			return pSObjects[0].Members[str1].Value.ToString();
		}

		private Collection<PSObject> GetLocalDCMembership(string localDcDn, string pdcHostName)
		{
			Collection<PSObject> pSObjects;
			string[] strArrays = new string[5];
			strArrays[0] = "Get-ADPrincipalGroupMembership -Identity \"";
			strArrays[1] = localDcDn;
			strArrays[2] = "\" -Server \"";
			strArrays[3] = pdcHostName;
			strArrays[4] = "\"";
			string str = string.Concat(strArrays);
			Collection<PSObject> pSObjects1 = null;
			try
			{
				pSObjects1 = base.InvokeCommand.InvokeScript(str);
				goto Label0;
			}
			catch (RuntimeException runtimeException1)
			{
				RuntimeException runtimeException = runtimeException1;
				base.WriteError(new ErrorRecord(runtimeException, "0", ErrorCategory.ReadError, str));
				pSObjects = null;
			}
			return pSObjects;
		Label0:
			if (pSObjects1 == null || pSObjects1.Count == 0)
			{
				return null;
			}
			else
			{
				return pSObjects1;
			}
		}

		private string GetOutputFileFullName()
		{
			string ditLocation;
			if (!string.IsNullOrEmpty(this._path))
			{
				ditLocation = this._path;
			}
			else
			{
				ditLocation = this.GetDitLocation();
			}
			ditLocation = Path.GetFullPath(ditLocation);
			if (!ditLocation.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
			{
				ditLocation = string.Concat(ditLocation, "\\");
			}
			ditLocation = string.Concat(ditLocation, "DCCloneConfig.xml");
			return ditLocation;
		}

		private void GetPassedInParameters()
		{
			if (this._cmdletParameters.Contains("Offline"))
			{
				this._offline = this._cmdletParameters.GetSwitchParameterBooleanValue("Offline");
			}
			if (this._cmdletParameters.Contains("Path"))
			{
				this._path = this._cmdletParameters["Path"] as string;
				this._path = this._path.Trim();
			}
			if (this._cmdletParameters.Contains("CloneComputerName"))
			{
				this._computerName = this._cmdletParameters["CloneComputerName"] as string;
				this._computerName = this._computerName.Trim();
			}
			if (this._cmdletParameters.Contains("SiteName"))
			{
				this._siteName = this._cmdletParameters["SiteName"] as string;
				this._siteName = this._siteName.Trim();
			}
			if (this._cmdletParameters.Contains("IPv4Address"))
			{
				this._ipv4Address = this._cmdletParameters["IPv4Address"] as string;
				this._ipv4Address = this._ipv4Address.Trim();
			}
			if (this._cmdletParameters.Contains("IPv4DefaultGateway"))
			{
				this._ipv4DefaultGateway = this._cmdletParameters["IPv4DefaultGateway"] as string;
				this._ipv4DefaultGateway = this._ipv4DefaultGateway.Trim();
			}
			if (this._cmdletParameters.Contains("IPv4SubnetMask"))
			{
				this._ipv4SubnetMask = this._cmdletParameters["IPv4SubnetMask"] as string;
				this._ipv4SubnetMask = this._ipv4SubnetMask.Trim();
			}
			if (this._cmdletParameters.Contains("IPv4DNSResolver"))
			{
				this._ipv4DnsResolver = this._cmdletParameters["IPv4DNSResolver"] as string[];
			}
			if (this._cmdletParameters.Contains("IPv6DNSResolver"))
			{
				this._ipv6DnsResolver = this._cmdletParameters["IPv6DNSResolver"] as string[];
			}
			if (this._cmdletParameters.Contains("PreferredWINSServer"))
			{
				this._preferredWinsServer = this._cmdletParameters["PreferredWINSServer"] as string;
				this._preferredWinsServer = this._preferredWinsServer.Trim();
			}
			if (this._cmdletParameters.Contains("AlternateWINSServer"))
			{
				this._alternateWinsServer = this._cmdletParameters["AlternateWINSServer"] as string;
				this._alternateWinsServer = this._alternateWinsServer.Trim();
			}
			if (this._cmdletParameters.Contains("Static"))
			{
				this._static = this._cmdletParameters.GetSwitchParameterBooleanValue("Static");
			}
			if (this._static && !string.IsNullOrEmpty(this._ipv4Address) && !string.IsNullOrEmpty(this._ipv4SubnetMask) && this._ipv4DnsResolver != null)
			{
				this._ipv4Static = true;
			}
			if (!this._ipv4Static && (this._ipv4DnsResolver != null || !string.IsNullOrEmpty(this._preferredWinsServer) || !string.IsNullOrEmpty(this._alternateWinsServer)))
			{
				this._ipv4Dynamic = true;
			}
			if (this._static && this._ipv6DnsResolver != null)
			{
				this._ipv6Static = true;
			}
			if (!this._ipv6Static && this._ipv6DnsResolver != null)
			{
				this._ipv6Dynamic = true;
			}
		}

		private bool IsMemberOfCloneableGroup(Collection<PSObject> adgroups)
		{
			bool flag = false;
			this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileGetCloneableGroupMessage, new object[0]));
			string str = "Cloneable Domain Controllers";
			ADGroup aDGroup = this.GetADGroup(str);
			if (aDGroup != null)
			{
				foreach (PSObject adgroup in adgroups)
				{
					ADGroup baseObject = adgroup.BaseObject as ADGroup;
					if (!baseObject.SID.Equals(aDGroup.SID))
					{
						continue;
					}
					flag = true;
					break;
				}
				return flag;
			}
			else
			{
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNoCloneableGroupMessage, new object[0]));
				return false;
			}
		}

		private bool IsWhiteListComplete()
		{
			bool flag;
			string str = "Get-ADDCCloningExcludedApplicationList";
			Collection<PSObject> pSObjects = null;
			try
			{
				pSObjects = base.InvokeCommand.InvokeScript(str);
				goto Label0;
			}
			catch (RuntimeException runtimeException)
			{
				flag = false;
			}
			return flag;
		Label0:
			if (pSObjects == null || pSObjects.Count == 0 || 1 == pSObjects.Count && pSObjects[0] == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private ADDomainController LocateLocalDC()
		{
			ADDomainController aDDomainController;
			string str = "Get-ADDomainController ($env:computername)";
			Collection<PSObject> pSObjects = null;
			try
			{
				pSObjects = base.InvokeCommand.InvokeScript(str);
				goto Label0;
			}
			catch (RuntimeException runtimeException)
			{
				aDDomainController = null;
			}
			return aDDomainController;
		Label0:
			if (pSObjects == null || pSObjects.Count == 0)
			{
				return null;
			}
			else
			{
				return pSObjects[0].BaseObject as ADDomainController;
			}
		}

		private ADDomainController LocateWin8PDC()
		{
			ADDomainController aDDomainController;
			string str = "Get-ADDomainController -Discover -ForceDiscover -Service PrimaryDC -MinimumDirectoryServiceVersion 3";
			Collection<PSObject> pSObjects = null;
			try
			{
				pSObjects = base.InvokeCommand.InvokeScript(str);
				goto Label0;
			}
			catch (RuntimeException runtimeException1)
			{
				RuntimeException runtimeException = runtimeException1;
				base.WriteError(new ErrorRecord(runtimeException, "0", ErrorCategory.ReadError, str));
				aDDomainController = null;
			}
			return aDDomainController;
		Label0:
			if (pSObjects == null || pSObjects.Count == 0)
			{
				return null;
			}
			else
			{
				return pSObjects[0].BaseObject as ADDomainController;
			}
		}

		private bool NewADDCCloneConfigFileBaseProcessCSRoutine()
		{
			bool flag;
			bool flag1 = false;
			this.GetPassedInParameters();
			if (!this._offline)
			{
				this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileLocalModeMessage, new object[0]));
				ADDomainController aDDomainController = this.LocateLocalDC();
				if (aDDomainController != null)
				{
					flag1 = this.PreCloningCheck();
					if (flag1)
					{
						flag = this.CheckExistingFiles(true);
						if (flag)
						{
							base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileFoundMessage, new object[0]));
							flag1 = false;
						}
					}
				}
				else
				{
					base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileLocalModeNoLocalDCMessage, new object[0]));
					return false;
				}
			}
			else
			{
				this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileOfflineModeMessage, new object[0]));
				flag1 = true;
			}
			this.ConsoleWrite("");
			if (!this.ValidateIpv4StaticSettings())
			{
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileInvalidIpv4StaticMessage, new object[0]));
				this.ConsoleWrite("");
				flag1 = false;
			}
			if (this._ipv4DnsResolver != null && 4 < (int)this._ipv4DnsResolver.Length || this._ipv6DnsResolver != null && 4 < (int)this._ipv6DnsResolver.Length)
			{
				object[] objArray = new object[1];
				objArray[0] = 4;
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileMoreDnsMessage, objArray));
				this.ConsoleWrite("");
				flag1 = false;
			}
			if (!flag1)
			{
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNoGenerateFileMessage, new object[0]));
			}
			else
			{
				this.WritePassInfo(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileGenerateFileMessage, new object[0]));
			}
			this.ConsoleWrite("");
			if (flag1)
			{
				string str = this.GenerateXmlFile();
				if (!this._offline)
				{
					flag = this.CheckExistingFiles(false);
					if (!flag)
					{
						object[] objArray1 = new object[1];
						objArray1[0] = str;
						base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileAtWrongLocationMessage, objArray1));
						File.Delete(str);
					}
				}
			}
			return true;
		}

		private bool PreCloningCheck()
		{
			bool flag = true;
			this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileLocatingWin8PDCMessage, new object[0]));
			ADDomainController aDDomainController = this.LocateWin8PDC();
			if (aDDomainController != null)
			{
				object[] value = new object[1];
				value[0] = (string)aDDomainController["HostName"].Value;
				this.WritePassInfo(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileFoundWin8PDCMessage, value));
				this.ConsoleWrite("");
				this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileCheckCloningPrivilegeMessage, new object[0]));
				ADDomainController aDDomainController1 = this.LocateLocalDC();
				if (aDDomainController1 != null)
				{
					object[] objArray = new object[1];
					objArray[0] = (string)aDDomainController1["HostName"].Value;
					this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileFoundLocalDCMessage, objArray));
					Collection<PSObject> localDCMembership = this.GetLocalDCMembership((string)aDDomainController1["ComputerObjectDN"].Value, (string)aDDomainController["HostName"].Value);
					if (localDCMembership != null)
					{
						bool flag1 = this.IsMemberOfCloneableGroup(localDCMembership);
						if (flag1)
						{
							this.WritePassInfo(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileHasCloningPrivilegeMessage, new object[0]));
						}
						else
						{
							base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNoCloningPrivilegeMessage, new object[0]));
							flag = false;
						}
					}
					else
					{
						base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNoLocalDCMembershipMessage, new object[0]));
						flag = false;
					}
				}
				else
				{
					base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNoLocalDCMessage, new object[0]));
					flag = false;
				}
				this.ConsoleWrite("");
				this.ConsoleWrite(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileTestWhiteListMessage, new object[0]));
				bool flag2 = this.IsWhiteListComplete();
				if (flag2)
				{
					this.WritePassInfo(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileWhiteListCompleteMessage, new object[0]));
				}
				else
				{
					base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileWhiteListNotCompleteMessage, new object[0]));
					flag = false;
				}
				this.ConsoleWrite("");
				return flag;
			}
			else
			{
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.NewADDCCloneConfigFileNoWin8PDCMessage, new object[0]));
				flag = false;
				return flag;
			}
		}

		private bool ValidateIpv4StaticSettings()
		{
			bool flag;
			bool flag1 = true;
			if (!string.IsNullOrEmpty(this._ipv4Address) || !string.IsNullOrEmpty(this._ipv4DefaultGateway) || !string.IsNullOrEmpty(this._ipv4SubnetMask))
			{
				if (string.IsNullOrEmpty(this._ipv4Address) || string.IsNullOrEmpty(this._ipv4SubnetMask) || this._ipv4DnsResolver == null)
				{
					flag = false;
				}
				else
				{
					flag = this._static;
				}
				flag1 = flag;
			}
			return flag1;
		}

		private void WritePassInfo(string output)
		{
			object[] objArray = new object[1];
			objArray[0] = output;
			base.InvokeCommand.InvokeScript("Write-Host -ForegroundColor black -BackgroundColor green $args[0]", false, PipelineResultTypes.None, null, objArray);
		}
	}
}