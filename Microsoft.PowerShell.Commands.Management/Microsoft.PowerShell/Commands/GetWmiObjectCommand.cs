using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "WmiObject", DefaultParameterSetName="query", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113337", RemotingCapability=RemotingCapability.OwnedByCommand)]
	public class GetWmiObjectCommand : WmiBaseCmdlet
	{
		private string wmiClass;

		private string[] property;

		private string filter;

		private SwitchParameter amended;

		private SwitchParameter directRead;

		private string objectQuery;

		private SwitchParameter list;

		private SwitchParameter recurse;

		[Parameter]
		public SwitchParameter Amended
		{
			get
			{
				return this.amended;
			}
			set
			{
				this.amended = value;
			}
		}

		[Alias(new string[] { "ClassName" })]
		[Parameter(Position=1, ParameterSetName="list")]
		[Parameter(Position=0, Mandatory=true, ParameterSetName="query")]
		public string Class
		{
			get
			{
				return this.wmiClass;
			}
			set
			{
				this.wmiClass = value;
			}
		}

		[Parameter(ParameterSetName="query")]
		[Parameter(ParameterSetName="WQLQuery")]
		public SwitchParameter DirectRead
		{
			get
			{
				return this.directRead;
			}
			set
			{
				this.directRead = value;
			}
		}

		[Parameter(ParameterSetName="query")]
		public string Filter
		{
			get
			{
				return this.filter;
			}
			set
			{
				this.filter = value;
			}
		}

		[Parameter(ParameterSetName="list")]
		public SwitchParameter List
		{
			get
			{
				return this.list;
			}
			set
			{
				this.list = value;
			}
		}

		[Parameter(Position=1, ParameterSetName="query")]
		public string[] Property
		{
			get
			{
				return (string[])this.property.Clone();
			}
			set
			{
				this.property = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="WQLQuery")]
		public string Query
		{
			get
			{
				return this.objectQuery;
			}
			set
			{
				this.objectQuery = value;
			}
		}

		[Parameter(ParameterSetName="list")]
		public SwitchParameter Recurse
		{
			get
			{
				return this.recurse;
			}
			set
			{
				this.recurse = value;
			}
		}

		public GetWmiObjectCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = "*";
			this.property = strArrays;
			this.list = false;
			this.recurse = false;
		}

		protected override void BeginProcessing()
		{
			ErrorRecord errorRecord;
			object[] message;
			string queryString;
			ConnectionOptions connectionOption = base.GetConnectionOption();
			if (!base.AsJob)
			{
				if (!this.list.IsPresent)
				{
					SwitchParameter recurse = this.Recurse;
					if (!recurse.IsPresent || !string.IsNullOrEmpty(this.wmiClass))
					{
						if (string.IsNullOrEmpty(this.Query))
						{
							queryString = this.GetQueryString();
						}
						else
						{
							queryString = this.Query;
						}
						string str = queryString;
						ObjectQuery objectQuery = new ObjectQuery(str.ToString());
						string[] computerName = base.ComputerName;
						for (int i = 0; i < (int)computerName.Length; i++)
						{
							string str1 = computerName[i];
							try
							{
								ManagementScope managementScope = new ManagementScope(WMIHelper.GetScopeString(str1, base.Namespace), connectionOption);
								EnumerationOptions enumerationOption = new EnumerationOptions();
								enumerationOption.UseAmendedQualifiers = this.amended;
								enumerationOption.DirectRead = this.directRead;
								ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption);
								foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
								{
									var exists = managementBaseObject.ObjectExits;
									if (exists) {
										base.WriteObject(managementBaseObject);
									}
								}
							}
							catch (ManagementException managementException1)
							{
								ManagementException managementException = managementException1;
								if (!managementException.ErrorCode.Equals(ManagementStatus.InvalidClass))
								{
									if (!managementException.ErrorCode.Equals(ManagementStatus.InvalidQuery))
									{
										if (!managementException.ErrorCode.Equals(ManagementStatus.InvalidNamespace))
										{
											errorRecord = new ErrorRecord(managementException, "GetWMIManagementException", ErrorCategory.InvalidOperation, null);
										}
										else
										{
											message = new object[2];
											message[0] = managementException.Message;
											message[1] = base.Namespace;
											string str2 = string.Format(CultureInfo.InvariantCulture, WmiResources.WmiQueryFailure, message);
											errorRecord = new ErrorRecord(new ManagementException(str2), "GetWMIManagementException", ErrorCategory.InvalidArgument, null);
										}
									}
									else
									{
										message = new object[2];
										message[0] = managementException.Message;
										message[1] = str;
										string str3 = string.Format(CultureInfo.InvariantCulture, WmiResources.WmiQueryFailure, message);
										errorRecord = new ErrorRecord(new ManagementException(str3), "GetWMIManagementException", ErrorCategory.InvalidArgument, null);
									}
								}
								else
								{
									string classNameFromQuery = this.GetClassNameFromQuery(str);
									message = new object[2];
									message[0] = managementException.Message;
									message[1] = classNameFromQuery;
									string str4 = string.Format(CultureInfo.InvariantCulture, WmiResources.WmiQueryFailure, message);
									errorRecord = new ErrorRecord(new ManagementException(str4), "GetWMIManagementException", ErrorCategory.InvalidType, null);
								}
								base.WriteError(errorRecord);
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								ErrorRecord errorRecord1 = new ErrorRecord(cOMException, "GetWMICOMException", ErrorCategory.InvalidOperation, null);
								base.WriteError(errorRecord1);
							}
							catch (Exception ex)
							{
								ErrorRecord errorRecord1 = new ErrorRecord(ex, "GetWMICOMException", ErrorCategory.InvalidOperation, null);
								base.WriteError(errorRecord1);
							}
						}
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "-Class";
						string str5 = string.Format(CultureInfo.InvariantCulture, WmiResources.WmiParameterMissing, objArray);
						ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str5), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord2);
						return;
					}
				}
				else
				{
					if (this.ValidateClassFormat())
					{
						string[] strArrays = base.ComputerName;
						for (int j = 0; j < (int)strArrays.Length; j++)
						{
							string str6 = strArrays[j];
							SwitchParameter switchParameter = this.Recurse;
							if (!switchParameter.IsPresent)
							{
								ManagementScope managementScope1 = new ManagementScope(WMIHelper.GetScopeString(str6, base.Namespace), connectionOption);
								try
								{
									managementScope1.Connect();
								}
								catch (ManagementException managementException3)
								{
									ManagementException managementException2 = managementException3;
									ErrorRecord errorDetail = new ErrorRecord(managementException2, "INVALID_NAMESPACE_IDENTIFIER", ErrorCategory.ObjectNotFound, null);
									object[] @namespace = new object[2];
									@namespace[0] = base.Namespace;
									@namespace[1] = managementException2.Message;
									errorDetail.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", @namespace);
									base.WriteError(errorDetail);
									goto Label0;
								}
								catch (COMException cOMException3)
								{
									COMException cOMException2 = cOMException3;
									ErrorRecord errorDetail1 = new ErrorRecord(cOMException2, "INVALID_NAMESPACE_IDENTIFIER", ErrorCategory.ObjectNotFound, null);
									object[] namespace1 = new object[2];
									namespace1[0] = base.Namespace;
									namespace1[1] = cOMException2.Message;
									errorDetail1.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", namespace1);
									base.WriteError(errorDetail1);
									goto Label0;
								}
								catch (UnauthorizedAccessException unauthorizedAccessException1)
								{
									UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
									ErrorRecord errorDetail2 = new ErrorRecord(unauthorizedAccessException, "INVALID_NAMESPACE_IDENTIFIER", ErrorCategory.ObjectNotFound, null);
									object[] message1 = new object[2];
									message1[0] = base.Namespace;
									message1[1] = unauthorizedAccessException.Message;
									errorDetail2.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", message1);
									base.WriteError(errorDetail2);
									goto Label0;
								}
								ManagementObjectSearcher objectList = this.GetObjectList(managementScope1);
								if (objectList != null)
								{
									foreach (ManagementBaseObject managementBaseObject1 in objectList.Get())
									{
										base.WriteObject(managementBaseObject1);
									}
								}
							}
							else
							{
								Queue queues = new Queue();
								queues.Enqueue(base.Namespace);
								while (queues.Count > 0)
								{
									string str7 = (string)queues.Dequeue();
									ManagementScope managementScope2 = new ManagementScope(WMIHelper.GetScopeString(str6, str7), connectionOption);
									try
									{
										managementScope2.Connect();
									}
									catch (ManagementException managementException5)
									{
										ManagementException managementException4 = managementException5;
										ErrorRecord errorRecord3 = new ErrorRecord(managementException4, "INVALID_NAMESPACE_IDENTIFIER", ErrorCategory.ObjectNotFound, null);
										object[] objArray1 = new object[2];
										objArray1[0] = str7;
										objArray1[1] = managementException4.Message;
										errorRecord3.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", objArray1);
										base.WriteError(errorRecord3);
										continue;
									}
									catch (COMException cOMException5)
									{
										COMException cOMException4 = cOMException5;
										ErrorRecord errorDetail3 = new ErrorRecord(cOMException4, "INVALID_NAMESPACE_IDENTIFIER", ErrorCategory.ObjectNotFound, null);
										object[] message2 = new object[2];
										message2[0] = str7;
										message2[1] = cOMException4.Message;
										errorDetail3.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", message2);
										base.WriteError(errorDetail3);
										continue;
									}
									catch (UnauthorizedAccessException unauthorizedAccessException3)
									{
										UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
										ErrorRecord errorRecord4 = new ErrorRecord(unauthorizedAccessException2, "INVALID_NAMESPACE_IDENTIFIER", ErrorCategory.ObjectNotFound, null);
										object[] objArray2 = new object[2];
										objArray2[0] = str7;
										objArray2[1] = unauthorizedAccessException2.Message;
										errorRecord4.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiNamespaceConnect", objArray2);
										base.WriteError(errorRecord4);
										continue;
									}
									ManagementClass managementClass = new ManagementClass(managementScope2, new ManagementPath("__Namespace"), new ObjectGetOptions());
									foreach (ManagementBaseObject instance in managementClass.GetInstances())
									{
										if (this.IsLocalizedNamespace((string)instance["Name"]))
										{
											continue;
										}
										queues.Enqueue(string.Concat(str7, "\\", instance["Name"]));
									}
									ManagementObjectSearcher objectList1 = this.GetObjectList(managementScope2);
									if (objectList1 == null)
									{
										continue;
									}
									foreach (ManagementBaseObject managementBaseObject2 in objectList1.Get())
									{
										base.WriteObject(managementBaseObject2);
									}
								}
							}
                        Label0:
                            continue;
						}
						return;
					}
					else
					{
						message = new object[1];
						message[0] = this.Class;
						ErrorRecord errorDetail4 = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, "Class", message)), "INVALID_QUERY_IDENTIFIER", ErrorCategory.InvalidArgument, null);
						object[] @class = new object[1];
						@class[0] = this.Class;
						errorDetail4.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiFilterInvalidClass", @class);
						base.WriteError(errorDetail4);
						return;
					}
				}
			}
			else
			{
				base.RunAsJob("Get-WMIObject");
				return;
			}
		}

		private string GetClassNameFromQuery(string query)
		{
			if (this.wmiClass == null)
			{
				int num = query.IndexOf(" from ", StringComparison.OrdinalIgnoreCase);
				string str = query.Substring(num + " from ".Length);
				char[] chrArray = new char[1];
				chrArray[0] = ' ';
				string str1 = str.Split(chrArray)[0];
				return str1;
			}
			else
			{
				return this.wmiClass;
			}
		}

		internal string GetFilterClassName()
		{
			if (!string.IsNullOrEmpty(this.Class))
			{
				string str = string.Copy(this.Class);
				str = str.Replace('*', '%');
				str = str.Replace('?', '\u005F');
				return str;
			}
			else
			{
				return string.Empty;
			}
		}

		internal ManagementObjectSearcher GetObjectList(ManagementScope scope)
		{
			ManagementObjectSearcher managementObjectSearcher = null;
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(this.Class))
			{
				string filterClassName = this.GetFilterClassName();
				if (filterClassName != null)
				{
					stringBuilder.Append("select * from meta_class where __class like '");
					stringBuilder.Append(filterClassName);
					stringBuilder.Append("'");
				}
				else
				{
					return managementObjectSearcher;
				}
			}
			else
			{
				stringBuilder.Append("select * from meta_class");
			}
			ObjectQuery objectQuery = new ObjectQuery(stringBuilder.ToString());
			EnumerationOptions enumerationOption = new EnumerationOptions();
			enumerationOption.EnumerateDeep = true;
			enumerationOption.UseAmendedQualifiers = this.Amended;
			managementObjectSearcher = new ManagementObjectSearcher(scope, objectQuery, enumerationOption);
			return managementObjectSearcher;
		}

		internal string GetQueryString()
		{
			StringBuilder stringBuilder = new StringBuilder("select ");
			stringBuilder.Append(string.Join(", ", this.property));
			stringBuilder.Append(" from ");
			stringBuilder.Append(this.wmiClass);
			if (!string.IsNullOrEmpty(this.filter))
			{
				stringBuilder.Append(" where ");
				stringBuilder.Append(this.filter);
			}
			return stringBuilder.ToString();
		}

		internal bool IsLocalizedNamespace(string sNamespace)
		{
			bool flag = false;
			if (sNamespace.StartsWith("ms_", StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
			}
			return flag;
		}

		internal bool ValidateClassFormat()
		{
			string @class = this.Class;
			if (!string.IsNullOrEmpty(@class))
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < @class.Length; i++)
				{
					if (!char.IsLetterOrDigit(@class[i]))
					{
						char chr = @class[i];
						if (!chr.Equals('['))
						{
							char chr1 = @class[i];
							if (!chr1.Equals(']'))
							{
								char chr2 = @class[i];
								if (!chr2.Equals('*'))
								{
									char chr3 = @class[i];
									if (!chr3.Equals('?'))
									{
										char chr4 = @class[i];
										if (chr4.Equals('-'))
										{
											goto Label1;
										}
										char chr5 = @class[i];
										if (!chr5.Equals('\u005F'))
										{
											return false;
										}
										else
										{
											stringBuilder.Append('[');
											stringBuilder.Append(@class[i]);
											stringBuilder.Append(']');
											goto Label0;
										}
									}
								}
							}
						}
					}
				Label1:
					stringBuilder.Append(@class[i]);
            Label0:
                continue;
				}
				this.Class = stringBuilder.ToString();
				return true;
			}
			else
			{
				return true;
			}
		}
	}
}