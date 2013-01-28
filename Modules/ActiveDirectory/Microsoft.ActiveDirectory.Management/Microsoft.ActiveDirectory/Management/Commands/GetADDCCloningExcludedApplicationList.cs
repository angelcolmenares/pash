using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADDCCloningExcludedApplicationList", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219594", DefaultParameterSetName="Default")]
	public class GetADDCCloningExcludedApplicationList : ADCmdletBase<GetADDCCloningExcludedApplicationListParameterSet>
	{
		private readonly static string boundaryTag;

		private readonly static string AllowListElement;

		private readonly static string AllowListEndElement;

		static GetADDCCloningExcludedApplicationList()
		{
			GetADDCCloningExcludedApplicationList.boundaryTag = "Allow";
			GetADDCCloningExcludedApplicationList.AllowListElement = "<AllowList>";
			GetADDCCloningExcludedApplicationList.AllowListEndElement = "</AllowList>";
		}

		public GetADDCCloningExcludedApplicationList()
		{
			base.ProcessRecordPipeline.InsertAtStart(new CmdletSubroutine(this.GetADDCCloningExcludedApplicationListBaseProcessCSRoutine));
		}

		private void ConsoleWrite(string output)
		{
			object[] objArray = new object[1];
			objArray[0] = output;
			base.InvokeCommand.InvokeScript("Write-Host -ForegroundColor black -BackgroundColor yellow $args[0]", false, PipelineResultTypes.Output, null, objArray);
		}

		private bool GetADDCCloningExcludedApplicationListBaseProcessCSRoutine()
		{
			int num;
			string str;
			bool flag;
			string str1;
			int num1 = -1;
			int num2 = -1;
			int num3 = -1;
			bool switchParameterBooleanValue = false;
			bool flag1 = false;
			bool switchParameterBooleanValue1 = false;
			string str2 = null;
			string item = null;
			string str3 = null;
			string str4 = null;
			bool flag2 = false;
			try
			{
				IntPtr intPtr = UnsafeNativeMethods.LoadLibrary("dsrolesrv.dll");
				if (intPtr != IntPtr.Zero)
				{
					flag2 = true;
				}
			}
			catch (Win32Exception win32Exception)
			{
			}
			if (flag2)
			{
				if (this._cmdletParameters.Contains("GenerateXml"))
				{
					switchParameterBooleanValue1 = this._cmdletParameters.GetSwitchParameterBooleanValue("GenerateXml");
				}
				if (switchParameterBooleanValue1)
				{
					if (this._cmdletParameters.Contains("Force"))
					{
						switchParameterBooleanValue = this._cmdletParameters.GetSwitchParameterBooleanValue("Force");
					}
					if (this._cmdletParameters.Contains("Path"))
					{
						object obj = this._cmdletParameters["Path"];
						if (obj as string != null)
						{
							item = this._cmdletParameters["Path"] as string;
							item = Path.GetFullPath(item);
							if (!item.EndsWith("\\"))
							{
								item = string.Concat(item, "\\");
							}
						}
					}
					if (string.IsNullOrEmpty(item))
					{
						str1 = "";
					}
					else
					{
						str1 = item;
					}
					num = UnsafeNativeMethods.DsRoleGetCustomAllowListPathRank(str1, out num1, out str3, out num2);
					if (num == 0)
					{
						if (!string.IsNullOrEmpty(item))
						{
							if (num1 < 0)
							{
								object[] objArray = new object[1];
								objArray[0] = item;
								str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListInvalidPath, objArray);
								throw new ADException(str);
							}
						}
						else
						{
							item = str3;
							num1 = num2;
						}
					}
					else
					{
						Win32Exception win32Exception1 = new Win32Exception(num);
						object[] message = new object[2];
						message[0] = num;
						message[1] = win32Exception1.Message;
						str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListErrorMessage, message);
						throw new ADException(str);
					}
				}
				if (switchParameterBooleanValue1)
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
				bool flag3 = flag;
				num = UnsafeNativeMethods.DsRoleIsPassedAllowedList(flag3, out flag1, out str2, out str4, out num3);
				if (!string.IsNullOrEmpty(str4) && !switchParameterBooleanValue1)
				{
					object[] objArray1 = new object[1];
					objArray1[0] = str4;
					str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListCustomerAllowListFileNameMessage, objArray1);
					this.ConsoleWrite(str);
				}
				if (num == 0)
				{
					if (!flag1)
					{
						if (switchParameterBooleanValue1)
						{
							if (num3 >= 0)
							{
								if (num1 <= num3)
								{
									if (num1 == num3 && File.Exists(str4) && !switchParameterBooleanValue)
									{
										object[] objArray2 = new object[1];
										objArray2[0] = str4;
										str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListFileExists, objArray2);
										throw new ADException(str);
									}
								}
								else
								{
									if (!File.Exists(str4))
									{
										object[] directoryName = new object[2];
										directoryName[0] = item;
										directoryName[1] = Path.GetDirectoryName(str4);
										str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListPathPriority, directoryName);
									}
									else
									{
										object[] objArray3 = new object[2];
										objArray3[0] = item;
										objArray3[1] = str4;
										str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListFilePriority, objArray3);
									}
									throw new ADException(str);
								}
							}
							str2 = str2.Substring(GetADDCCloningExcludedApplicationList.AllowListElement.Length);
							str2 = str2.Substring(0, str2.LastIndexOf(GetADDCCloningExcludedApplicationList.AllowListEndElement));
							string str5 = string.Concat("<?xml version=\"1.0\" encoding=\"utf-8\"?><dc:CustomDCCloneAllowList xmlns:dc=\"uri:microsoft.com:schemas:CustomDCCloneAllowList\">", str2, "</dc:CustomDCCloneAllowList>");
							XmlDocument xmlDocument = new XmlDocument();
							xmlDocument.LoadXml(str5);
							item = string.Concat(item, "CustomDCCloneAllowList.xml");
							XmlTextWriter xmlTextWriter = new XmlTextWriter(item, new UTF8Encoding(false));
							xmlTextWriter.Formatting = Formatting.Indented;
							xmlDocument.WriteContentTo(xmlTextWriter);
							xmlTextWriter.Close();
							object[] objArray4 = new object[1];
							objArray4[0] = item;
							str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListNewAllowList, objArray4);
							this.ConsoleWrite(str);
							base.WriteObject(null);
						}
						else
						{
							XmlDocument xmlDocument1 = new XmlDocument();
							xmlDocument1.LoadXml(str2);
							XmlNodeList elementsByTagName = xmlDocument1.GetElementsByTagName(GetADDCCloningExcludedApplicationList.boundaryTag);
							foreach (XmlNode xmlNodes in elementsByTagName)
							{
								if (!xmlNodes.HasChildNodes)
								{
									continue;
								}
								ADDCCloningExcludedApplication aDDCCloningExcludedApplication = new ADDCCloningExcludedApplication();
								for (int i = 0; i < xmlNodes.ChildNodes.Count; i++)
								{
									aDDCCloningExcludedApplication.Add(xmlNodes.ChildNodes[i].Name, xmlNodes.ChildNodes[i].InnerText);
								}
								base.WriteObject(aDDCCloningExcludedApplication);
							}
						}
						return false;
					}
					else
					{
						object[] objArray5 = new object[1];
						objArray5[0] = str4;
						str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListNoAppsFound, objArray5);
						this.ConsoleWrite(str);
						base.WriteObject(null);
						return false;
					}
				}
				else
				{
					Win32Exception win32Exception2 = new Win32Exception(num);
					object[] message1 = new object[2];
					message1[0] = num;
					message1[1] = win32Exception2.Message;
					str = string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListErrorMessage, message1);
					throw new ADException(str);
				}
			}
			else
			{
				base.WriteWarning(string.Format(CultureInfo.CurrentCulture, StringResources.ADDCCloningExcludedApplicationListLocalMachineNotADCMessage, new object[0]));
				return false;
			}
		}
	}
}