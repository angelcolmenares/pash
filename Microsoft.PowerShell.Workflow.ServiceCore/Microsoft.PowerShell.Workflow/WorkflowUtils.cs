using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.PowerShell.Workflow
{
	internal static class WorkflowUtils
	{
		internal static bool CompareAuthentication(AuthenticationMechanism authentication1, AuthenticationMechanism authentication2)
		{
			return authentication1 == authentication2;
		}

		internal static bool CompareCertificateThumbprint(string certificateThumbprint1, string certificateThumbprint2)
		{
			if (certificateThumbprint1 != null || certificateThumbprint2 != null)
			{
				if (!(certificateThumbprint1 == null ^ certificateThumbprint2 == null))
				{
					if (string.Compare(certificateThumbprint1, certificateThumbprint2, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
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

		internal static bool CompareConnectionUri(WSManConnectionInfo connectionInfo1, WSManConnectionInfo connectionInfo2)
		{
			if (string.Compare(connectionInfo2.Scheme, connectionInfo1.Scheme, StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (string.Compare(connectionInfo2.ComputerName, connectionInfo1.ComputerName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (string.Compare(connectionInfo2.AppName, connectionInfo1.AppName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						if (connectionInfo2.Port == connectionInfo1.Port)
						{
							return true;
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
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		internal static bool CompareCredential(PSCredential credential1, PSCredential credential2)
		{
			if (credential1 != null || credential2 != null)
			{
				if (!(credential1 == null ^ credential2 == null))
				{
					if (string.Compare(credential1.UserName, credential2.UserName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						if (WorkflowUtils.ComparePassword(credential1.Password, credential2.Password))
						{
							return true;
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
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		internal static bool CompareOtherWSManSettings(WSManConnectionInfo connectionInfo1, WSManConnectionInfo connectionInfo2)
		{
			bool hasValue;
			bool flag;
			if (connectionInfo1.SkipCACheck == connectionInfo2.SkipCACheck)
			{
				if (connectionInfo1.SkipCNCheck == connectionInfo2.SkipCNCheck)
				{
					if (connectionInfo1.SkipRevocationCheck == connectionInfo2.SkipRevocationCheck)
					{
						if (connectionInfo1.UseCompression == connectionInfo2.UseCompression)
						{
							if (connectionInfo1.UseUTF16 == connectionInfo2.UseUTF16)
							{
								if (connectionInfo1.MaximumConnectionRedirectionCount == connectionInfo2.MaximumConnectionRedirectionCount)
								{
									int? maximumReceivedDataSizePerCommand = connectionInfo1.MaximumReceivedDataSizePerCommand;
									int? nullable = connectionInfo2.MaximumReceivedDataSizePerCommand;
									if (maximumReceivedDataSizePerCommand.GetValueOrDefault() != nullable.GetValueOrDefault())
									{
										hasValue = true;
									}
									else
									{
										hasValue = maximumReceivedDataSizePerCommand.HasValue != nullable.HasValue;
									}
									if (!hasValue)
									{
										int? maximumReceivedObjectSize = connectionInfo1.MaximumReceivedObjectSize;
										int? maximumReceivedObjectSize1 = connectionInfo2.MaximumReceivedObjectSize;
										if (maximumReceivedObjectSize.GetValueOrDefault() != maximumReceivedObjectSize1.GetValueOrDefault())
										{
											flag = true;
										}
										else
										{
											flag = maximumReceivedObjectSize.HasValue != maximumReceivedObjectSize1.HasValue;
										}
										if (!flag)
										{
											if (connectionInfo1.NoEncryption == connectionInfo2.NoEncryption)
											{
												if (connectionInfo1.NoMachineProfile == connectionInfo2.NoMachineProfile)
												{
													if (connectionInfo1.OutputBufferingMode == connectionInfo2.OutputBufferingMode)
													{
														return true;
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
				return false;
			}
		}

		internal static bool ComparePassword(SecureString secureString1, SecureString secureString2)
		{
			bool flag;
			bool flag1;
			if (secureString1 != null || secureString2 != null)
			{
				if (!(secureString1 == null ^ secureString2 == null))
				{
					if (secureString1.Length == secureString2.Length)
					{
						IntPtr zero = IntPtr.Zero;
						IntPtr bSTR = IntPtr.Zero;
						try
						{
							try
							{
								zero = Marshal.SecureStringToBSTR(secureString1);
								bSTR = Marshal.SecureStringToBSTR(secureString2);
								int num = 0;
								bool flag2 = true;
								do
								{
									byte num1 = Marshal.ReadByte(zero, num + 1);
									byte num2 = Marshal.ReadByte(bSTR, num + 1);
									byte num3 = Marshal.ReadByte(zero, num);
									byte num4 = Marshal.ReadByte(bSTR, num);
									num = num + 2;
									if (num1 != num2 || num3 != num4)
									{
										flag = false;
										return flag;
									}
									else
									{
										if (num1 != 0)
										{
											flag1 = true;
										}
										else
										{
											flag1 = num3 != 0;
										}
										flag2 = flag1;
									}
								}
								while (flag2);
								flag = true;
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
								using (traceSource)
								{
									traceSource.WriteMessage("Getting an exception while comparing credentials...");
									traceSource.TraceException(exception);
									flag = false;
								}
							}
						}
						finally
						{
							if (IntPtr.Zero != zero)
							{
								Marshal.ZeroFreeBSTR(zero);
							}
							if (IntPtr.Zero != bSTR)
							{
								Marshal.ZeroFreeBSTR(bSTR);
							}
						}
						return flag;
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

		internal static bool CompareProxySettings(WSManConnectionInfo connectionInfo1, WSManConnectionInfo connectionInfo2)
		{
			if (connectionInfo1.ProxyAccessType == connectionInfo2.ProxyAccessType)
			{
				if (connectionInfo1.ProxyAccessType != ProxyAccessType.None)
				{
					if (connectionInfo1.ProxyAuthentication == connectionInfo2.ProxyAuthentication)
					{
						if (WorkflowUtils.CompareCredential(connectionInfo1.ProxyCredential, connectionInfo2.ProxyCredential))
						{
							return true;
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
			else
			{
				return false;
			}
		}

		internal static bool CompareShellUri(string shellUri1, string shellUri2)
		{
			if (string.Compare(shellUri1, shellUri2, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}