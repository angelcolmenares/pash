using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell.Commands.Internal;
using Microsoft.PowerShell.Commands.Management;
using Microsoft.WSMan.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Restart", "Computer", SupportsShouldProcess=true, DefaultParameterSetName="DefaultSet", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135253", RemotingCapability=RemotingCapability.OwnedByCommand)]
	public class RestartComputerCommand : PSCmdlet, IDisposable
	{
		private const string DefaultParameterSet = "DefaultSet";

		private const string AsJobParameterSet = "AsJobSet";

		private const string CimOperatingSystemNamespace = "root/cimv2";

		private const string CimOperatingSystemShutdownMethod = "Win32shutdown";

		private const string CimQueryDialect = "WQL";

		private const string DcomProtocol = "DCOM";

		private const string WsmanProtocol = "WSMan";

		private const string TestPowershellScript = "\r\n$array = @($input)\r\n$result = @{}\r\nforeach ($computerName in $array[1])\r\n{\r\n    $ret = $null\r\n    if ($array[0] -eq $null)\r\n    {\r\n        $ret = Invoke-Command -ComputerName $computerName {$true} -SessionOption (New-PSSessionOption -NoMachineProfile) -ErrorAction SilentlyContinue\r\n    }\r\n    else\r\n    {\r\n        $ret = Invoke-Command -ComputerName $computerName {$true} -SessionOption (New-PSSessionOption -NoMachineProfile) -ErrorAction SilentlyContinue -Credential $array[0]\r\n    }\r\n\r\n    if ($ret -eq $true)\r\n    {\r\n        $result[$computerName] = $true\r\n    }\r\n    else\r\n    {\r\n        $result[$computerName] = $false\r\n    }\r\n}\r\n$result\r\n";

		private const int SecondsToWaitForRestartToBegin = 25;

		private const string StageVerification = "VerifyStage";

		private const string WmiConnectionTest = "WMI";

		private const string WinrmConnectionTest = "WinRM";

		private const string PowerShellConnectionTest = "PowerShell";

		private SwitchParameter _asjob;

		private AuthenticationLevel _dcomAuthentication;

		private bool _isDcomAuthenticationSpecified;

		private ImpersonationLevel _impersonation;

		private bool _isImpersonationSpecified;

		private string _protocol;

		private bool _isProtocolSpecified;

		private string[] _computername;

		private List<string> _validatedComputerNames;

		private readonly List<string> _waitOnComputers;

		private readonly HashSet<string> _uniqueComputerNames;

		private int _throttlelimit;

		private int _timeout;

		private bool _timeoutSpecified;

		private WaitForServiceTypes _waitFor;

		private bool _waitForSpecified;

		private int _delay;

		private bool _delaySpecified;

		private string[] _indicator;

		private int _activityId;

		private int _timeoutInMilliseconds;

		private bool _exit;

		private bool _timeUp;

		private readonly CancellationTokenSource _cancel;

		private readonly ManualResetEventSlim _waitHandler;

		private readonly Dictionary<string, RestartComputerCommand.ComputerInfo> _computerInfos;

		private readonly string _shortLocalMachineName;

		private readonly string _fullLocalMachineName;

		private int _percent;

		private string _status;

		private string _activity;

		private System.Timers.Timer _timer;

		private System.Management.Automation.PowerShell _powershell;

		[Parameter(ParameterSetName="AsJobSet")]
		public SwitchParameter AsJob
		{
			get
			{
				return this._asjob;
			}
			set
			{
				this._asjob = value;
			}
		}

		[Alias(new string[] { "CN", "__SERVER", "Server", "IPAddress" })]
		[Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this._computername;
			}
			set
			{
				this._computername = value;
			}
		}

		[Credential]
		[Parameter(Position=1)]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get;
			set;
		}

		[Alias(new string[] { "Authentication" })]
		[Parameter]
		public AuthenticationLevel DcomAuthentication
		{
			get
			{
				return this._dcomAuthentication;
			}
			set
			{
				this._dcomAuthentication = value;
				this._isDcomAuthenticationSpecified = true;
			}
		}

		[Parameter(ParameterSetName="DefaultSet")]
		[ValidateRange(1, 0x7fff)]
		public short Delay
		{
			get
			{
				return (short)this._delay;
			}
			set
			{
				this._delay = value;
				this._delaySpecified = true;
			}
		}

		[Parameter(ParameterSetName="DefaultSet")]
		public WaitForServiceTypes For
		{
			get
			{
				return this._waitFor;
			}
			set
			{
				this._waitFor = value;
				this._waitForSpecified = true;
			}
		}

		[Alias(new string[] { "f" })]
		[Parameter]
		public SwitchParameter Force
		{
			get;
			set;
		}

		[Parameter]
		public ImpersonationLevel Impersonation
		{
			get
			{
				return this._impersonation;
			}
			set
			{
				this._impersonation = value;
				this._isImpersonationSpecified = true;
			}
		}

		[Parameter(ParameterSetName="DefaultSet")]
		[ValidateSet(new string[] { "DCOM", "WSMan" })]
		public string Protocol
		{
			get
			{
				return this._protocol;
			}
			set
			{
				this._protocol = value;
				this._isProtocolSpecified = true;
			}
		}

		[Parameter(ParameterSetName="AsJobSet")]
		[ValidateRange(-2147483648, 0x3e8)]
		public int ThrottleLimit
		{
			get
			{
				return this._throttlelimit;
			}
			set
			{
				this._throttlelimit = value;
				if (this._throttlelimit <= 0)
				{
					this._throttlelimit = 32;
				}
			}
		}

		[Alias(new string[] { "TimeoutSec" })]
		[Parameter(ParameterSetName="DefaultSet")]
		[ValidateRange(-1, 0x7fffffff)]
		public int Timeout
		{
			get
			{
				return this._timeout;
			}
			set
			{
				this._timeout = value;
				this._timeoutSpecified = true;
			}
		}

		[Parameter(ParameterSetName="DefaultSet")]
		public SwitchParameter Wait
		{
			get;
			set;
		}

		[Parameter(ParameterSetName="DefaultSet")]
		[ValidateSet(new string[] { "Default", "Basic", "Negotiate", "CredSSP", "Digest", "Kerberos" })]
		public string WsmanAuthentication
		{
			get;
			set;
		}

		public RestartComputerCommand()
		{
			this._asjob = false;
			this._dcomAuthentication = AuthenticationLevel.Packet;
			this._impersonation = ImpersonationLevel.Impersonate;
			this._protocol = "DCOM";
			string[] strArrays = new string[1];
			strArrays[0] = ".";
			this._computername = strArrays;
			this._validatedComputerNames = new List<string>();
			this._waitOnComputers = new List<string>();
			this._uniqueComputerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			this._throttlelimit = 32;
			this._timeout = -1;
			this._waitFor = WaitForServiceTypes.PowerShell;
			this._delay = 5;
			string[] strArrays1 = new string[4];
			strArrays1[0] = "|";
			strArrays1[1] = "/";
			strArrays1[2] = "-";
			strArrays1[3] = "\\";
			this._indicator = strArrays1;
			this._cancel = new CancellationTokenSource();
			this._waitHandler = new ManualResetEventSlim(false);
			this._computerInfos = new Dictionary<string, RestartComputerCommand.ComputerInfo>(StringComparer.OrdinalIgnoreCase);
			this._shortLocalMachineName = Dns.GetHostName();
			this._fullLocalMachineName = Dns.GetHostEntry("").HostName;
		}

		protected override void BeginProcessing()
		{
			if (base.ParameterSetName.Equals("DefaultSet", StringComparison.OrdinalIgnoreCase))
			{
				if (this.WsmanAuthentication != null && (this._isDcomAuthenticationSpecified || this._isImpersonationSpecified))
				{
					string str = StringUtil.Format(ComputerResources.ParameterConfliction, ComputerResources.ParameterUsage);
					InvalidOperationException invalidOperationException = new InvalidOperationException(str);
					base.ThrowTerminatingError(new ErrorRecord(invalidOperationException, "ParameterConfliction", ErrorCategory.InvalidOperation, null));
				}
				bool flag = this.Protocol.Equals("DCOM", StringComparison.OrdinalIgnoreCase);
				bool flag1 = this.Protocol.Equals("WSMan", StringComparison.OrdinalIgnoreCase);
				if (this._isProtocolSpecified && flag && this.WsmanAuthentication != null)
				{
					string str1 = StringUtil.Format(ComputerResources.InvalidParameterForDCOM, ComputerResources.ParameterUsage);
					InvalidOperationException invalidOperationException1 = new InvalidOperationException(str1);
					base.ThrowTerminatingError(new ErrorRecord(invalidOperationException1, "InvalidParameterForDCOM", ErrorCategory.InvalidOperation, null));
				}
				if (this._isProtocolSpecified && flag1 && (this._isDcomAuthenticationSpecified || this._isImpersonationSpecified))
				{
					string str2 = StringUtil.Format(ComputerResources.InvalidParameterForWSMan, ComputerResources.ParameterUsage);
					InvalidOperationException invalidOperationException2 = new InvalidOperationException(str2);
					base.ThrowTerminatingError(new ErrorRecord(invalidOperationException2, "InvalidParameterForWSMan", ErrorCategory.InvalidOperation, null));
				}
				if (!this._isProtocolSpecified && this.WsmanAuthentication != null)
				{
					this.Protocol = "WSMan";
				}
			}
			if ((this._timeoutSpecified || this._waitForSpecified || this._delaySpecified) && !this.Wait)
			{
				InvalidOperationException invalidOperationException3 = new InvalidOperationException(ComputerResources.RestartComputerInvalidParameter);
				base.ThrowTerminatingError(new ErrorRecord(invalidOperationException3, "RestartComputerInvalidParameter", ErrorCategory.InvalidOperation, null));
			}
			if (this.Wait)
			{
				this._activityId = (new Random()).Next();
				if (this._timeout == -1 || this._timeout >= 0x20c49b)
				{
					this._timeoutInMilliseconds = 0x7fffffff;
				}
				else
				{
					this._timeoutInMilliseconds = this._timeout * 0x3e8;
				}
				WaitForServiceTypes waitForServiceType = this._waitFor;
				if (waitForServiceType == WaitForServiceTypes.Wmi || waitForServiceType == WaitForServiceTypes.WinRM)
				{
					return;
				}
				else if (waitForServiceType == WaitForServiceTypes.PowerShell)
				{
					this._powershell = System.Management.Automation.PowerShell.Create();
					this._powershell.AddScript("\r\n$array = @($input)\r\n$result = @{}\r\nforeach ($computerName in $array[1])\r\n{\r\n    $ret = $null\r\n    if ($array[0] -eq $null)\r\n    {\r\n        $ret = Invoke-Command -ComputerName $computerName {$true} -SessionOption (New-PSSessionOption -NoMachineProfile) -ErrorAction SilentlyContinue\r\n    }\r\n    else\r\n    {\r\n        $ret = Invoke-Command -ComputerName $computerName {$true} -SessionOption (New-PSSessionOption -NoMachineProfile) -ErrorAction SilentlyContinue -Credential $array[0]\r\n    }\r\n\r\n    if ($ret -eq $true)\r\n    {\r\n        $result[$computerName] = $true\r\n    }\r\n    else\r\n    {\r\n        $result[$computerName] = $false\r\n    }\r\n}\r\n$result\r\n");
					return;
				}
				InvalidOperationException invalidOperationException4 = new InvalidOperationException(ComputerResources.NoSupportForCombinedServiceType);
				ErrorRecord errorRecord = new ErrorRecord(invalidOperationException4, "NoSupportForCombinedServiceType", ErrorCategory.InvalidOperation, (object)((int)this._waitFor));
				base.ThrowTerminatingError(errorRecord);
			}
		}

		private int CalculateProgressPercentage(string currentStage)
		{
			string str = currentStage;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "VerifyStage")
				{
					if (this._waitFor.Equals(WaitForServiceTypes.Wmi) || this._waitFor.Equals(WaitForServiceTypes.WinRM))
					{
						return 33;
					}
					else
					{
						return 20;
					}
				}
				else
				{
					if (str1 == "WMI")
					{
						if (this._waitFor.Equals(WaitForServiceTypes.Wmi))
						{
							return 66;
						}
						else
						{
							return 40;
						}
					}
					else
					{
						if (str1 == "WinRM")
						{
							if (this._waitFor.Equals(WaitForServiceTypes.WinRM))
							{
								return 66;
							}
							else
							{
								return 60;
							}
						}
						else
						{
							if (str1 == "PowerShell")
							{
								return 80;
							}
						}
					}
				}
			}
			return 0;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this._timer != null)
				{
					this._timer.Dispose();
				}
				this._waitHandler.Dispose();
				this._cancel.Dispose();
				if (this._powershell != null)
				{
					this._powershell.Dispose();
				}
			}
		}

		private void OnTimedEvent(object s, ElapsedEventArgs e)
		{
			bool flag = true;
			bool flag1 = flag;
			this._timeUp = flag;
			this._exit = flag1;
			this._cancel.Cancel();
			this._waitHandler.Set();
			if (this._powershell != null)
			{
				this._powershell.Stop();
				this._powershell.Dispose();
			}
		}

		protected override void ProcessRecord()
		{
			string str;
			object localShutdownPrivilege;
			string str1;
			bool flag;
			string restartMultipleComputersActivity;
			List<string> strs;
			List<string> strs1;
			List<string> strs2;
			this.ValidateComputerNames();
			ConnectionOptions connection = ComputerWMIHelper.GetConnection(this.DcomAuthentication, this.Impersonation, this.Credential);
			object[] objArray = new object[2];
			objArray[0] = 2;
			objArray[1] = 0;
			object[] objArray1 = objArray;
			if (this.Force)
			{
				objArray1[0] = 6;
			}
			if (!base.ParameterSetName.Equals("AsJobSet", StringComparison.OrdinalIgnoreCase))
			{
				if (base.ParameterSetName.Equals("DefaultSet", StringComparison.OrdinalIgnoreCase))
				{
					bool flag1 = this.Protocol.Equals("DCOM", StringComparison.OrdinalIgnoreCase);
					if (this.Wait && this._timeout != 0)
					{
						RestartComputerCommand restartComputerCommand = this;
						if (flag1)
						{
							strs2 = this.SetUpComputerInfoUsingDcom(this._validatedComputerNames, connection);
						}
						else
						{
							strs2 = this.SetUpComputerInfoUsingWsman(this._validatedComputerNames, this._cancel.Token);
						}
						restartComputerCommand._validatedComputerNames = strs2;
					}
					foreach (string _validatedComputerName in this._validatedComputerNames)
					{
						bool flag2 = false;
						if (!_validatedComputerName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
						{
							str = _validatedComputerName;
						}
						else
						{
							str = this._shortLocalMachineName;
							flag2 = true;
							if (flag1)
							{
								connection.Username = null;
								//connection.SecurePassword = null;
							}
						}
						string restartComputerAction = ComputerResources.RestartComputerAction;
						if (flag2)
						{
							localShutdownPrivilege = ComputerResources.LocalShutdownPrivilege;
						}
						else
						{
							localShutdownPrivilege = ComputerResources.RemoteShutdownPrivilege;
						}
						string str2 = StringUtil.Format(restartComputerAction, localShutdownPrivilege);
						if (flag2)
						{
							str1 = StringUtil.Format(ComputerResources.DoubleComputerName, "localhost", str);
						}
						else
						{
							str1 = str;
						}
						string str3 = str1;
						if (!base.ShouldProcess(str3, str2))
						{
							continue;
						}
						if (flag1)
						{
							flag = RestartComputerCommand.RestartOneComputerUsingDcom(this, flag2, str, objArray1, connection);
						}
						else
						{
							flag = RestartComputerCommand.RestartOneComputerUsingWsman(this, flag2, str, objArray1, this.Credential, this.WsmanAuthentication, this._cancel.Token);
						}
						bool flag3 = flag;
						if (!flag3 || !this.Wait || this._timeout == 0)
						{
							continue;
						}
						this._waitOnComputers.Add(_validatedComputerName);
					}
					if (this._waitOnComputers.Count > 0)
					{
						List<string> strs3 = new List<string>(this._waitOnComputers);
						List<string> strs4 = new List<string>();
						List<string> strs5 = new List<string>();
						List<string> strs6 = new List<string>();
						List<string> strs7 = new List<string>();
						bool flag4 = this._waitFor.Equals(WaitForServiceTypes.Wmi);
						bool flag5 = this._waitFor.Equals(WaitForServiceTypes.WinRM);
						bool flag6 = this._waitFor.Equals(WaitForServiceTypes.PowerShell);
						int num = 0;
						int count = 0;
						int num1 = 25;
						bool flag7 = true;
						bool count1 = false;
						this._percent = 0;
						this._status = ComputerResources.WaitForRestartToBegin;
						RestartComputerCommand restartComputerCommand1 = this;
						if (this._waitOnComputers.Count == 1)
						{
							restartMultipleComputersActivity = StringUtil.Format(ComputerResources.RestartSingleComputerActivity, this._waitOnComputers[0]);
						}
						else
						{
							restartMultipleComputersActivity = ComputerResources.RestartMultipleComputersActivity;
						}
						restartComputerCommand1._activity = restartMultipleComputersActivity;
						this._timer = new System.Timers.Timer((double)this._timeoutInMilliseconds);
						this._timer.Elapsed += new ElapsedEventHandler(this.OnTimedEvent);
						this._timer.AutoReset = false;
						this._timer.Enabled = true;
						while (true)
						{
							int num2 = num1 * 4;
							do
							{
								if (num2 <= 0)
								{
									break;
								}
								int num3 = num;
								num = num3 + 1;
								this.WriteProgress(string.Concat(this._indicator[num3 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
								num2--;
								this._waitHandler.Wait(250);
							}
							while (!this._exit);
							if (flag7)
							{
								num1 = this._delay;
								flag7 = false;
								if (this._waitOnComputers.Count > 1)
								{
									this._status = StringUtil.Format(ComputerResources.WaitForMultipleComputers, count, this._waitOnComputers.Count);
									int num4 = num;
									num = num4 + 1;
									this.WriteProgress(string.Concat(this._indicator[num4 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
								}
							}
							if (!this._exit)
							{
								if (strs3.Count > 0)
								{
									if (this._waitOnComputers.Count == 1)
									{
										this._status = ComputerResources.VerifyRebootStage;
										this._percent = this.CalculateProgressPercentage("VerifyStage");
										int num5 = num;
										num = num5 + 1;
										this.WriteProgress(string.Concat(this._indicator[num5 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
									}
									if (flag4 || flag6)
									{
										strs = strs4;
									}
									else
									{
										strs = strs5;
									}
									List<string> strs8 = strs;
									if (flag1)
									{
										strs1 = this.TestRestartStageUsingDcom(strs3, strs8, this._cancel.Token, connection);
									}
									else
									{
										strs1 = this.TestRestartStageUsingWsman(strs3, strs8, this._cancel.Token);
									}
									strs3 = strs1;
								}
								if (!this._exit)
								{
									if (strs4.Count > 0)
									{
										if (!flag1)
										{
											if (this._waitOnComputers.Count == 1)
											{
												this._status = ComputerResources.WaitForWMI;
												this._percent = this.CalculateProgressPercentage("WMI");
												int num6 = num;
												num = num6 + 1;
												this.WriteProgress(string.Concat(this._indicator[num6 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
											}
											strs4 = RestartComputerCommand.TestWmiConnectionUsingWsman(strs4, strs5, this._cancel.Token, this.Credential, this.WsmanAuthentication, this);
										}
										else
										{
											strs5.AddRange(strs4);
											strs4.Clear();
											if (this._waitOnComputers.Count == 1)
											{
												this._status = ComputerResources.WaitForWMI;
												this._percent = this.CalculateProgressPercentage("WMI");
												num2 = num1 * 4;
												while (num2 > 0)
												{
													int num7 = num;
													num = num7 + 1;
													this.WriteProgress(string.Concat(this._indicator[num7 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
													num2--;
													this._waitHandler.Wait(250);
													if (this._exit)
													{
														goto Label0;
													}
												}
											}
										}
									}
								Label0:
									if (!flag4 && !this._exit)
									{
										if (strs5.Count > 0)
										{
											if (!flag1)
											{
												strs6.AddRange(strs5);
												strs5.Clear();
												if (this._waitOnComputers.Count == 1)
												{
													this._status = ComputerResources.WaitForWinRM;
													this._percent = this.CalculateProgressPercentage("WinRM");
													num2 = num1 * 4;
													do
													{
														if (num2 <= 0)
														{
															break;
														}
														int num8 = num;
														num = num8 + 1;
														this.WriteProgress(string.Concat(this._indicator[num8 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
														num2--;
														this._waitHandler.Wait(250);
													}
													while (!this._exit);
												}
											}
											else
											{
												if (this._waitOnComputers.Count == 1)
												{
													this._status = ComputerResources.WaitForWinRM;
													this._percent = this.CalculateProgressPercentage("WinRM");
													int num9 = num;
													num = num9 + 1;
													this.WriteProgress(string.Concat(this._indicator[num9 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
												}
												strs5 = RestartComputerCommand.TestWinrmConnection(strs5, strs6, this._cancel.Token);
											}
										}
										if (!flag5 && !this._exit && strs6.Count > 0)
										{
											if (this._waitOnComputers.Count == 1)
											{
												this._status = ComputerResources.WaitForPowerShell;
												this._percent = this.CalculateProgressPercentage("PowerShell");
												int num10 = num;
												num = num10 + 1;
												this.WriteProgress(string.Concat(this._indicator[num10 % 4], this._activity), this._status, this._percent, ProgressRecordType.Processing);
											}
											strs6 = RestartComputerCommand.TestPowerShell(strs6, strs7, this._powershell, this.Credential);
										}
									}
								}
							}
							if (this._exit)
							{
								break;
							}
							WaitForServiceTypes waitForServiceType = this._waitFor;
							switch (waitForServiceType)
							{
								case WaitForServiceTypes.Wmi:
								{
									count1 = strs5.Count == this._waitOnComputers.Count;
									count = strs5.Count;
									break;
								}
								case WaitForServiceTypes.WinRM:
								{
									count1 = strs6.Count == this._waitOnComputers.Count;
									count = strs6.Count;
									break;
								}
								case WaitForServiceTypes.PowerShell:
								{
									count1 = strs7.Count == this._waitOnComputers.Count;
									count = strs7.Count;
									break;
								}
							}
							if (count1 || this._exit)
							{
								if (!count1)
								{
									break;
								}
								this._status = ComputerResources.RestartComplete;
								this.WriteProgress(string.Concat(this._indicator[num % 4], this._activity), this._status, 100, ProgressRecordType.Completed);
								this._timer.Enabled = false;
								break;
							}
							else
							{
								if (this._waitOnComputers.Count > 1)
								{
									this._status = StringUtil.Format(ComputerResources.WaitForMultipleComputers, count, this._waitOnComputers.Count);
									this._percent = count * 100 / this._waitOnComputers.Count;
								}
							}
						}
						if (this._timeUp)
						{
							if (strs3.Count > 0)
							{
								this.WriteOutTimeoutError(strs3);
							}
							if (strs4.Count > 0)
							{
								this.WriteOutTimeoutError(strs4);
							}
							if (!flag4)
							{
								if (strs5.Count > 0)
								{
									this.WriteOutTimeoutError(strs5);
								}
								if (!flag5)
								{
									if (strs6.Count > 0)
									{
										this.WriteOutTimeoutError(strs6);
									}
								}
								else
								{
									return;
								}
							}
							else
							{
								return;
							}
						}
					}
				}
				return;
			}
			else
			{
				string[] array = this._validatedComputerNames.ToArray();
				string machineNames = ComputerWMIHelper.GetMachineNames(array);
				if (base.ShouldProcess(machineNames))
				{
					InvokeWmiMethod invokeWmiMethod = new InvokeWmiMethod();
					invokeWmiMethod.Path = "Win32_OperatingSystem=@";
					invokeWmiMethod.ComputerName = array;
					invokeWmiMethod.Authentication = this.DcomAuthentication;
					invokeWmiMethod.Impersonation = this.Impersonation;
					invokeWmiMethod.Credential = this.Credential;
					invokeWmiMethod.ThrottleLimit = this.ThrottleLimit;
					invokeWmiMethod.Name = "Win32Shutdown";
					invokeWmiMethod.EnableAllPrivileges = SwitchParameter.Present;
					invokeWmiMethod.ArgumentList = objArray1;
					PSWmiJob pSWmiJob = new PSWmiJob(invokeWmiMethod, array, this.ThrottleLimit, Job.GetCommandTextFromInvocationInfo(base.MyInvocation));
					base.JobRepository.Add(pSWmiJob);
					base.WriteObject(pSWmiJob);
					return;
				}
				else
				{
					return;
				}
			}
		}

		internal static bool RestartOneComputerUsingDcom(PSCmdlet cmdlet, bool isLocalhost, string computerName, object[] flags, ConnectionOptions options)
		{
			bool flag;
			object obj;
			string str;
			string str1;
			bool flag1 = false;
			ManagementObjectSearcher managementObjectSearcher = null;
			Win32Native.TOKEN_PRIVILEGE tOKENPRIVILEGE = new Win32Native.TOKEN_PRIVILEGE();
			try
			{
				try
				{
					if ((!isLocalhost || !ComputerWMIHelper.EnableTokenPrivilege("SeShutdownPrivilege", ref tOKENPRIVILEGE)) && (isLocalhost || !ComputerWMIHelper.EnableTokenPrivilege("SeRemoteShutdownPrivilege", ref tOKENPRIVILEGE)))
					{
						string privilegeNotEnabled = ComputerResources.PrivilegeNotEnabled;
						string str2 = computerName;
						if (isLocalhost)
						{
							obj = "SeShutdownPrivilege";
						}
						else
						{
							obj = "SeRemoteShutdownPrivilege";
						}
						string str3 = StringUtil.Format(privilegeNotEnabled, str2, obj);
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str3), "PrivilegeNotEnabled", ErrorCategory.InvalidOperation, null);
						cmdlet.WriteError(errorRecord);
						flag = false;
						return flag;
					}
					else
					{
						if (isLocalhost)
						{
							str = "localhost";
						}
						else
						{
							str = computerName;
						}
						ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(str, "\\root\\cimv2"), options);
						EnumerationOptions enumerationOption = new EnumerationOptions();
						enumerationOption.UseAmendedQualifiers = true;
						enumerationOption.DirectRead = true;
						EnumerationOptions enumerationOption1 = enumerationOption;
						ObjectQuery objectQuery = new ObjectQuery("select * from Win32_OperatingSystem");
						managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption1);
						foreach (ManagementObject managementObject in managementObjectSearcher.Get())
						{
							object obj1 = managementObject.InvokeMethod("Win32shutdown", flags);
							int num = Convert.ToInt32(obj1.ToString(), CultureInfo.CurrentCulture);
							if (num == 0)
							{
								flag1 = true;
							}
							else
							{
								Win32Exception win32Exception = new Win32Exception(num);
								string str4 = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, win32Exception.Message);
								ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str4), "RestartcomputerFailed", ErrorCategory.OperationStopped, computerName);
								cmdlet.WriteError(errorRecord1);
							}
						}
					}
				}
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					string str5 = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, managementException.Message);
					ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str5), "RestartcomputerFailed", ErrorCategory.OperationStopped, computerName);
					cmdlet.WriteError(errorRecord2);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					string str6 = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, cOMException.Message);
					ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(str6), "RestartcomputerFailed", ErrorCategory.OperationStopped, computerName);
					cmdlet.WriteError(errorRecord3);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					string str7 = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, unauthorizedAccessException.Message);
					ErrorRecord errorRecord4 = new ErrorRecord(new InvalidOperationException(str7), "RestartcomputerFailed", ErrorCategory.OperationStopped, computerName);
					cmdlet.WriteError(errorRecord4);
				}
				return flag1;
			}
			finally
			{
				if (isLocalhost)
				{
					str1 = "SeShutdownPrivilege";
				}
				else
				{
					str1 = "SeRemoteShutdownPrivilege";
				}
				ComputerWMIHelper.RestoreTokenPrivilege(str1, ref tOKENPRIVILEGE);
				if (managementObjectSearcher != null)
				{
					managementObjectSearcher.Dispose();
				}
			}
			return flag;
		}

		internal static bool RestartOneComputerUsingWsman(PSCmdlet cmdlet, bool isLocalhost, string computerName, object[] flags, PSCredential credential, string authentication, CancellationToken token)
		{
			bool flag;
			string str;
			string str1;
			PSCredential pSCredential;
			object obj;
			string str2;
			bool flag1 = false;
			if (isLocalhost)
			{
				str = "localhost";
			}
			else
			{
				str = computerName;
			}
			string str3 = str;
			if (isLocalhost)
			{
				str1 = null;
			}
			else
			{
				str1 = authentication;
			}
			string str4 = str1;
			if (isLocalhost)
			{
				pSCredential = null;
			}
			else
			{
				pSCredential = credential;
			}
			PSCredential pSCredential1 = pSCredential;
			Win32Native.TOKEN_PRIVILEGE tOKENPRIVILEGE = new Win32Native.TOKEN_PRIVILEGE();
			CimOperationOptions cimOperationOption = new CimOperationOptions();
			cimOperationOption.Timeout = TimeSpan.FromMilliseconds(10000);
			cimOperationOption.CancellationToken = new CancellationToken?(token);
			CimOperationOptions cimOperationOption1 = cimOperationOption;
			try
			{
				try
				{
					if ((!isLocalhost || !ComputerWMIHelper.EnableTokenPrivilege("SeShutdownPrivilege", ref tOKENPRIVILEGE)) && (isLocalhost || !ComputerWMIHelper.EnableTokenPrivilege("SeRemoteShutdownPrivilege", ref tOKENPRIVILEGE)))
					{
						string privilegeNotEnabled = ComputerResources.PrivilegeNotEnabled;
						string str5 = computerName;
						if (isLocalhost)
						{
							obj = "SeShutdownPrivilege";
						}
						else
						{
							obj = "SeRemoteShutdownPrivilege";
						}
						string str6 = StringUtil.Format(privilegeNotEnabled, str5, obj);
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str6), "PrivilegeNotEnabled", ErrorCategory.InvalidOperation, null);
						cmdlet.WriteError(errorRecord);
						flag = false;
						return flag;
					}
					else
					{
						CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(str3, pSCredential1, str4, token, cmdlet);
						using (cimSession)
						{
							CimMethodParametersCollection cimMethodParametersCollection = new CimMethodParametersCollection();
							cimMethodParametersCollection.Add(CimMethodParameter.Create("Flags", flags[0], Microsoft.Management.Infrastructure.CimType.SInt32, (CimFlags)((long)0)));
							cimMethodParametersCollection.Add(CimMethodParameter.Create("Reserved", flags[1], Microsoft.Management.Infrastructure.CimType.SInt32, (CimFlags)((long)0)));
							CimMethodResult cimMethodResult = cimSession.InvokeMethod("root/cimv2", "Win32_OperatingSystem", "Win32shutdown", cimMethodParametersCollection, cimOperationOption1);
							int num = Convert.ToInt32(cimMethodResult.ReturnValue.Value, CultureInfo.CurrentCulture);
							if (num == 0)
							{
								flag1 = true;
							}
							else
							{
								Win32Exception win32Exception = new Win32Exception(num);
								string str7 = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, win32Exception.Message);
								ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str7), "RestartcomputerFailed", ErrorCategory.OperationStopped, computerName);
								cmdlet.WriteError(errorRecord1);
							}
						}
					}
				}
				catch (CimException cimException1)
				{
					CimException cimException = cimException1;
					string str8 = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, cimException.Message);
					ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str8), "RestartcomputerFailed", ErrorCategory.OperationStopped, computerName);
					cmdlet.WriteError(errorRecord2);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					string str9 = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, exception.Message);
					ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(str9), "RestartcomputerFailed", ErrorCategory.OperationStopped, computerName);
					cmdlet.WriteError(errorRecord3);
				}
				return flag1;
			}
			finally
			{
				if (isLocalhost)
				{
					str2 = "SeShutdownPrivilege";
				}
				else
				{
					str2 = "SeRemoteShutdownPrivilege";
				}
				ComputerWMIHelper.RestoreTokenPrivilege(str2, ref tOKENPRIVILEGE);
			}
			return flag;
		}

		private List<string> SetUpComputerInfoUsingDcom(IEnumerable<string> computerNames, ConnectionOptions options)
		{
			List<string> strs = new List<string>();
			ObjectQuery objectQuery = new ObjectQuery("Select * From Win32_OperatingSystem");
			EnumerationOptions enumerationOption = new EnumerationOptions();
			enumerationOption.UseAmendedQualifiers = true;
			enumerationOption.DirectRead = true;
			EnumerationOptions enumerationOption1 = enumerationOption;
			ManagementObjectSearcher managementObjectSearcher = null;
			ManagementObjectCollection managementObjectCollections = null;
			foreach (string computerName in computerNames)
			{
				try
				{
					try
					{
						ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(computerName, "\\root\\cimv2"), options);
						managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption1);
						managementObjectCollections = managementObjectSearcher.Get();
						if (managementObjectCollections.Count <= 0)
						{
							string str = StringUtil.Format(ComputerResources.RestartComputerSkipped, computerName, ComputerResources.CannotGetOperatingSystemObject);
							ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "RestartComputerSkipped", ErrorCategory.OperationStopped, computerName);
							base.WriteError(errorRecord);
						}
						else
						{
							foreach (ManagementBaseObject managementBaseObject in managementObjectCollections)
							{
								if (this._computerInfos.ContainsKey(computerName))
								{
									continue;
								}
								RestartComputerCommand.ComputerInfo computerInfo = new RestartComputerCommand.ComputerInfo();
								computerInfo.LastBootUpTime = managementBaseObject.Properties["LastBootUpTime"].Value.ToString();
								computerInfo.RebootComplete = false;
								RestartComputerCommand.ComputerInfo computerInfo1 = computerInfo;
								this._computerInfos.Add(computerName, computerInfo1);
								strs.Add(computerName);
							}
						}
					}
					catch (ManagementException managementException1)
					{
						ManagementException managementException = managementException1;
						string str1 = StringUtil.Format(ComputerResources.RestartComputerSkipped, computerName, managementException.Message);
						ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str1), "RestartComputerSkipped", ErrorCategory.OperationStopped, computerName);
						base.WriteError(errorRecord1);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						string str2 = StringUtil.Format(ComputerResources.RestartComputerSkipped, computerName, cOMException.Message);
						ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str2), "RestartComputerSkipped", ErrorCategory.OperationStopped, computerName);
						base.WriteError(errorRecord2);
					}
					catch (UnauthorizedAccessException unauthorizedAccessException1)
					{
						UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
						string str3 = StringUtil.Format(ComputerResources.RestartComputerSkipped, computerName, unauthorizedAccessException.Message);
						ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(str3), "RestartComputerSkipped", ErrorCategory.OperationStopped, computerName);
						base.WriteError(errorRecord3);
					}
				}
				finally
				{
					if (managementObjectSearcher != null)
					{
						managementObjectSearcher.Dispose();
					}
					if (managementObjectCollections != null)
					{
						managementObjectCollections.Dispose();
					}
				}
			}
			return strs;
		}

		private List<string> SetUpComputerInfoUsingWsman(IEnumerable<string> computerNames, CancellationToken token)
		{
			bool flag = false;
			List<string> strs = new List<string>();
			CimOperationOptions cimOperationOption = new CimOperationOptions();
			cimOperationOption.Timeout = TimeSpan.FromMilliseconds(2000);
			cimOperationOption.CancellationToken = new CancellationToken?(token);
			CimOperationOptions cimOperationOption1 = cimOperationOption;
			foreach (string computerName in computerNames)
			{
				try
				{
					CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(computerName, this.Credential, this.WsmanAuthentication, token, this);
					using (cimSession)
					{
						IEnumerable<CimInstance> cimInstances = cimSession.QueryInstances("root/cimv2", "WQL", "Select * from Win32_OperatingSystem", cimOperationOption1);
						foreach (CimInstance cimInstance in cimInstances)
						{
							flag = true;
							if (this._computerInfos.ContainsKey(computerName))
							{
								continue;
							}
							RestartComputerCommand.ComputerInfo computerInfo = new RestartComputerCommand.ComputerInfo();
							computerInfo.LastBootUpTime = cimInstance.CimInstanceProperties["LastBootUpTime"].Value.ToString();
							computerInfo.RebootComplete = false;
							RestartComputerCommand.ComputerInfo computerInfo1 = computerInfo;
							this._computerInfos.Add(computerName, computerInfo1);
							strs.Add(computerName);
						}
						if (!flag)
						{
							string str = StringUtil.Format(ComputerResources.RestartComputerSkipped, computerName, ComputerResources.CannotGetOperatingSystemObject);
							ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "RestartComputerSkipped", ErrorCategory.OperationStopped, computerName);
							base.WriteError(errorRecord);
						}
					}
				}
				catch (CimException cimException1)
				{
					CimException cimException = cimException1;
					string str1 = StringUtil.Format(ComputerResources.RestartComputerSkipped, computerName, cimException.Message);
					ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str1), "RestartComputerSkipped", ErrorCategory.OperationStopped, computerName);
					base.WriteError(errorRecord1);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					string str2 = StringUtil.Format(ComputerResources.RestartComputerSkipped, computerName, exception.Message);
					ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str2), "RestartComputerSkipped", ErrorCategory.OperationStopped, computerName);
					base.WriteError(errorRecord2);
				}
			}
			return strs;
		}

		protected override void StopProcessing()
		{
			this._exit = true;
			this._cancel.Cancel();
			this._waitHandler.Set();
			if (this._timer != null)
			{
				this._timer.Enabled = false;
			}
			if (this._powershell != null)
			{
				this._powershell.Stop();
				this._powershell.Dispose();
			}
		}

		internal static List<string> TestPowerShell(List<string> computerNames, List<string> nextTestList, System.Management.Automation.PowerShell powershell, PSCredential credential)
		{
			List<string> strs = new List<string>();
			try
			{
				object[] array = new object[2];
				array[0] = credential;
				array[1] = computerNames.ToArray();
				Collection<PSObject> pSObjects = powershell.Invoke(array);
				if (pSObjects.Count != 0)
				{
					object obj = PSObject.Base(pSObjects[0]);
					Hashtable hashtables = obj as Hashtable;
					foreach (string computerName in computerNames)
					{
						if (!LanguagePrimitives.IsTrue(hashtables[computerName]))
						{
							strs.Add(computerName);
						}
						else
						{
							nextTestList.Add(computerName);
						}
					}
				}
				else
				{
					List<string> strs1 = computerNames;
					return strs1;
				}
			}
			catch (PipelineStoppedException pipelineStoppedException)
			{
			}
			catch (ObjectDisposedException objectDisposedException)
			{
			}
			return strs;
		}

		private List<string> TestRestartStageUsingDcom(IEnumerable<string> computerNames, List<string> nextTestList, CancellationToken token, ConnectionOptions options)
		{
			List<string> strs = new List<string>();
			ObjectQuery objectQuery = new ObjectQuery("Select * from Win32_OperatingSystem");
			EnumerationOptions enumerationOption = new EnumerationOptions();
			enumerationOption.UseAmendedQualifiers = true;
			enumerationOption.DirectRead = true;
			EnumerationOptions enumerationOption1 = enumerationOption;
			ManagementObjectSearcher managementObjectSearcher = null;
			ManagementObjectCollection managementObjectCollections = null;
			foreach (string computerName in computerNames)
			{
				try
				{
					try
					{
						if (!token.IsCancellationRequested)
						{
							ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(computerName, "\\root\\cimv2"), options);
							managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption1);
							if (!token.IsCancellationRequested)
							{
								managementObjectCollections = managementObjectSearcher.Get();
								if (managementObjectCollections.Count <= 0)
								{
									strs.Add(computerName);
								}
								else
								{
									foreach (ManagementBaseObject managementBaseObject in managementObjectCollections)
									{
										string str = managementBaseObject.Properties["LastBootUpTime"].Value.ToString();
										string lastBootUpTime = this._computerInfos[computerName].LastBootUpTime;
										if (string.Compare(str, lastBootUpTime, StringComparison.OrdinalIgnoreCase) == 0)
										{
											strs.Add(computerName);
										}
										else
										{
											this._computerInfos[computerName].RebootComplete = true;
											nextTestList.Add(computerName);
										}
									}
								}
							}
							else
							{
								break;
							}
						}
						else
						{
							break;
						}
					}
					catch (ManagementException managementException)
					{
						strs.Add(computerName);
					}
					catch (COMException cOMException)
					{
						strs.Add(computerName);
					}
					catch (UnauthorizedAccessException unauthorizedAccessException)
					{
						strs.Add(computerName);
					}
				}
				finally
				{
					if (managementObjectSearcher != null)
					{
						managementObjectSearcher.Dispose();
					}
					if (managementObjectCollections != null)
					{
						managementObjectCollections.Dispose();
					}
				}
			}
			return strs;
		}

		private List<string> TestRestartStageUsingWsman(IEnumerable<string> computerNames, List<string> nextTestList, CancellationToken token)
		{
			bool flag = false;
			List<string> strs = new List<string>();
			CimOperationOptions cimOperationOption = new CimOperationOptions();
			cimOperationOption.Timeout = TimeSpan.FromMilliseconds(2000);
			cimOperationOption.CancellationToken = new CancellationToken?(token);
			CimOperationOptions cimOperationOption1 = cimOperationOption;
			foreach (string computerName in computerNames)
			{
				try
				{
					if (!token.IsCancellationRequested)
					{
						CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(computerName, this.Credential, this.WsmanAuthentication, token, this);
						using (cimSession)
						{
							IEnumerable<CimInstance> cimInstances = cimSession.QueryInstances("root/cimv2", "WQL", "Select * from Win32_OperatingSystem", cimOperationOption1);
							foreach (CimInstance cimInstance in cimInstances)
							{
								flag = true;
								string str = cimInstance.CimInstanceProperties["LastBootUpTime"].Value.ToString();
								string lastBootUpTime = this._computerInfos[computerName].LastBootUpTime;
								if (string.Compare(str, lastBootUpTime, StringComparison.OrdinalIgnoreCase) == 0)
								{
									strs.Add(computerName);
								}
								else
								{
									this._computerInfos[computerName].RebootComplete = true;
									nextTestList.Add(computerName);
								}
							}
							if (!flag)
							{
								strs.Add(computerName);
							}
						}
					}
					else
					{
						break;
					}
				}
				catch (CimException cimException)
				{
					strs.Add(computerName);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					strs.Add(computerName);
				}
			}
			return strs;
		}

		internal static List<string> TestWinrmConnection(List<string> computerNames, List<string> nextTestList, CancellationToken token)
		{
			List<string> strs = new List<string>();
			IWSManEx wSManClass = (IWSManEx)(new WSManClass());
			int num = 0x8001;
			IWSManConnectionOptionsEx2 wSManConnectionOptionsEx2 = (IWSManConnectionOptionsEx2)wSManClass.CreateConnectionOptions();
			foreach (string computerName in computerNames)
			{
				try
				{
					if (!token.IsCancellationRequested)
					{
						IWSManSession wSManSession = (IWSManSession)wSManClass.CreateSession(computerName, num, wSManConnectionOptionsEx2);
						if (!token.IsCancellationRequested)
						{
							wSManSession.Timeout = 0x5dc;
							wSManSession.Identify(0);
							nextTestList.Add(computerName);
						}
						else
						{
							break;
						}
					}
					else
					{
						break;
					}
				}
				catch (COMException cOMException)
				{
					strs.Add(computerName);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					strs.Add(computerName);
				}
			}
			return strs;
		}

		internal static List<string> TestWmiConnectionUsingWsman(List<string> computerNames, List<string> nextTestList, CancellationToken token, PSCredential credential, string wsmanAuthentication, PSCmdlet cmdlet)
		{
			bool flag = false;
			List<string> strs = new List<string>();
			CimOperationOptions cimOperationOption = new CimOperationOptions();
			cimOperationOption.Timeout = TimeSpan.FromMilliseconds(2000);
			cimOperationOption.CancellationToken = new CancellationToken?(token);
			CimOperationOptions cimOperationOption1 = cimOperationOption;
			foreach (string computerName in computerNames)
			{
				try
				{
					if (!token.IsCancellationRequested)
					{
						CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(computerName, credential, wsmanAuthentication, token, cmdlet);
						using (cimSession)
						{
							IEnumerable<CimInstance> cimInstances = cimSession.QueryInstances("root/cimv2", "WQL", "Select * from Win32_Service Where name = 'Winmgmt'", cimOperationOption1);
							foreach (CimInstance cimInstance in cimInstances)
							{
								flag = true;
								if (!LanguagePrimitives.IsTrue(cimInstance.CimInstanceProperties["Started"].Value))
								{
									strs.Add(computerName);
								}
								else
								{
									nextTestList.Add(computerName);
								}
							}
							if (!flag)
							{
								strs.Add(computerName);
							}
						}
					}
					else
					{
						break;
					}
				}
				catch (CimException cimException)
				{
					strs.Add(computerName);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					strs.Add(computerName);
				}
			}
			return strs;
		}

		private void ValidateComputerNames()
		{
			IPAddress pAddress = null;
			bool flag = false;
			this._validatedComputerNames.Clear();
			string[] strArrays = this._computername;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (str.Equals(".", StringComparison.OrdinalIgnoreCase) || str.Equals("localhost", StringComparison.OrdinalIgnoreCase) || str.Equals(this._shortLocalMachineName, StringComparison.OrdinalIgnoreCase) || str.Equals(this._fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				else
				{
					bool flag1 = false;
					try
					{
						flag1 = IPAddress.TryParse(str, out pAddress);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						CommandProcessorBase.CheckForSevereException(exception);
					}
					try
					{
						string hostName = Dns.GetHostEntry(str).HostName;
						if (hostName.Equals(this._shortLocalMachineName, StringComparison.OrdinalIgnoreCase) || hostName.Equals(this._fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
						{
							flag = true;
						}
						else
						{
							if (!this._uniqueComputerNames.Contains(str))
							{
								this._validatedComputerNames.Add(str);
								this._uniqueComputerNames.Add(str);
							}
						}
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						CommandProcessorBase.CheckForSevereException(exception2);
						if (flag1)
						{
							if (!this._uniqueComputerNames.Contains(str))
							{
								this._validatedComputerNames.Add(str);
								this._uniqueComputerNames.Add(str);
							}
						}
						else
						{
							string str1 = StringUtil.Format(ComputerResources.CannotResolveComputerName, str, exception2.Message);
							ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str1), "AddressResolutionException", ErrorCategory.InvalidArgument, str);
							base.WriteError(errorRecord);
						}
					}
				}
			}
			if (this.Wait && flag)
			{
				InvalidOperationException invalidOperationException = new InvalidOperationException(ComputerResources.CannotWaitLocalComputer);
				base.WriteError(new ErrorRecord(invalidOperationException, "CannotWaitLocalComputer", ErrorCategory.InvalidOperation, null));
				flag = false;
			}
			if (flag)
			{
				this._validatedComputerNames.Add("localhost");
			}
		}

		private void WriteOutTimeoutError(IEnumerable<string> computerNames)
		{
			foreach (string computerName in computerNames)
			{
				string str = StringUtil.Format(ComputerResources.RestartcomputerFailed, computerName, ComputerResources.TimeoutError);
				RestartComputerTimeoutException restartComputerTimeoutException = new RestartComputerTimeoutException(computerName, this.Timeout, str, "RestartComputerTimeout");
				ErrorRecord errorRecord = new ErrorRecord(restartComputerTimeoutException, "RestartComputerTimeout", ErrorCategory.OperationTimeout, computerName);
				base.WriteError(errorRecord);
			}
		}

		private void WriteProgress(string activity, string status, int percent, ProgressRecordType progressRecordType)
		{
			ProgressRecord progressRecord = new ProgressRecord(this._activityId, activity, status);
			progressRecord.PercentComplete = percent;
			progressRecord.RecordType = progressRecordType;
			base.WriteProgress(progressRecord);
		}

		private class ComputerInfo
		{
			internal string LastBootUpTime;

			internal bool RebootComplete;

			public ComputerInfo()
			{
			}
		}
	}
}