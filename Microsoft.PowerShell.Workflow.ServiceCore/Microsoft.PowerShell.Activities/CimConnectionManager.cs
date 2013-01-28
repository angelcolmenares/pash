using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell.Workflow;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Timers;

namespace Microsoft.PowerShell.Activities
{
	internal class CimConnectionManager
	{
		internal const int MaxIterations = 6;

		internal const int MaxCimSessionsUpperLimit = 0x1f4;

		internal const int MaxCimSessionsLowerLimit = 1;

		private readonly System.Timers.Timer _cleanupTimer;

		private bool firstTime;

		private Dictionary<string, List<CimConnectionManager.SessionEntry>> availableSessions;

		private object SyncRoot;

		private static CimConnectionManager _globalConnectionManagerInstance;

		private static object gcmLock;

		static CimConnectionManager()
		{
			CimConnectionManager.gcmLock = new object();
		}

		internal CimConnectionManager()
		{
			this._cleanupTimer = new System.Timers.Timer();
			this.firstTime = true;
			this.availableSessions = new Dictionary<string, List<CimConnectionManager.SessionEntry>>();
			this.SyncRoot = new object();
			this._cleanupTimer.Elapsed += new ElapsedEventHandler(this.HandleCleanupTimerElapsed);
			this._cleanupTimer.AutoReset = true;
			this._cleanupTimer.Interval = 20000;
			this._cleanupTimer.Start();
		}

		private static bool CompareSessionOptions(CimConnectionManager.SessionEntry sessionEntry, CimSessionOptions options2, PSCredential credential2, string certificateThumbprint, AuthenticationMechanism authenticationMechanism, bool useSsl, uint port, PSSessionOption pssessionOption)
		{
			TimeSpan timeout = sessionEntry.SessionOptions.Timeout;
			if (timeout.Equals(options2.Timeout))
			{
				if (string.Equals(sessionEntry.SessionOptions.Culture.ToString(), options2.Culture.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					if (string.Equals(sessionEntry.SessionOptions.UICulture.ToString(), options2.UICulture.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						if (string.Equals(sessionEntry.CertificateThumbprint, certificateThumbprint, StringComparison.OrdinalIgnoreCase))
						{
							if (sessionEntry.AuthenticationMechanism == authenticationMechanism)
							{
								if (WorkflowUtils.CompareCredential(sessionEntry.Credential, credential2))
								{
									if (sessionEntry.UseSsl == useSsl)
									{
										if (sessionEntry.Port == port)
										{
											if (!(pssessionOption == null ^ sessionEntry.PSSessionOption == null))
											{
												if (pssessionOption != null && sessionEntry.PSSessionOption != null)
												{
													if (sessionEntry.PSSessionOption.ProxyAccessType == pssessionOption.ProxyAccessType)
													{
														if (sessionEntry.PSSessionOption.ProxyAuthentication == pssessionOption.ProxyAuthentication)
														{
															if (WorkflowUtils.CompareCredential(sessionEntry.PSSessionOption.ProxyCredential, pssessionOption.ProxyCredential))
															{
																if (sessionEntry.PSSessionOption.SkipCACheck == pssessionOption.SkipCACheck)
																{
																	if (sessionEntry.PSSessionOption.SkipCNCheck == pssessionOption.SkipCNCheck)
																	{
																		if (sessionEntry.PSSessionOption.SkipRevocationCheck == pssessionOption.SkipRevocationCheck)
																		{
																			if (sessionEntry.PSSessionOption.NoEncryption == pssessionOption.NoEncryption)
																			{
																				if (sessionEntry.PSSessionOption.UseUTF16 != pssessionOption.UseUTF16)
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

		public static CimConnectionManager GetGlobalCimConnectionManager()
		{
			CimConnectionManager cimConnectionManager;
			lock (CimConnectionManager.gcmLock)
			{
				if (CimConnectionManager._globalConnectionManagerInstance == null)
				{
					CimConnectionManager._globalConnectionManagerInstance = new CimConnectionManager();
				}
				cimConnectionManager = CimConnectionManager._globalConnectionManagerInstance;
			}
			return cimConnectionManager;
		}

		internal CimSession GetSession(string computerName, PSCredential credential, string certificateThumbprint, AuthenticationMechanism authenticationMechanism, CimSessionOptions sessionOptions, bool useSsl, uint port, PSSessionOption pssessionOption)
		{
			CimSession session;
			lock (this.SyncRoot)
			{
				if (this.availableSessions.ContainsKey(computerName))
				{
					List<CimConnectionManager.SessionEntry> item = this.availableSessions[computerName];
					if (item.Count > 0)
					{
						int num = 0;
						while (num < item.Count)
						{
							CimConnectionManager.SessionEntry sessionEntry = item[num];
							if ((sessionEntry.SessionOptions != null || sessionOptions != null) && !CimConnectionManager.CompareSessionOptions(sessionEntry, sessionOptions, credential, certificateThumbprint, authenticationMechanism, useSsl, port, pssessionOption))
							{
								num++;
							}
							else
							{
								sessionEntry.AddReference();
								session = sessionEntry.Session;
								return session;
							}
						}
					}
				}
				CimConnectionManager.SessionEntry sessionEntry1 = new CimConnectionManager.SessionEntry(computerName, credential, certificateThumbprint, authenticationMechanism, sessionOptions, useSsl, port, pssessionOption);
				sessionEntry1.IterationsRemaining = 6;
				sessionEntry1.AddReference();
				if (!this.availableSessions.ContainsKey(computerName))
				{
					this.availableSessions.Add(computerName, new List<CimConnectionManager.SessionEntry>());
				}
				this.availableSessions[computerName].Add(sessionEntry1);
				session = sessionEntry1.Session;
			}
			return session;
		}

		private void HandleCleanupTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (!this.firstTime)
			{
				lock (this.SyncRoot)
				{
					List<string> strs = new List<string>();
					foreach (KeyValuePair<string, List<CimConnectionManager.SessionEntry>> availableSession in this.availableSessions)
					{
						List<CimConnectionManager.SessionEntry> value = availableSession.Value;
						if (value.Count != 0)
						{
							for (int i = value.Count - 1; i >= 0; i--)
							{
								if (value[i].GetReferenceCount <= 0)
								{
									CimConnectionManager.SessionEntry item = value[i];
									int iterationsRemaining = item.IterationsRemaining - 1;
									int num = iterationsRemaining;
									item.IterationsRemaining = iterationsRemaining;
									if (num <= 0)
									{
										value[i].Session.Close();
										value.RemoveAt(i);
									}
								}
							}
						}
						else
						{
							strs.Add(availableSession.Key);
						}
					}
				}
				return;
			}
			else
			{
				this.firstTime = false;
				return;
			}
		}

		internal void ReleaseSession(string computerName, CimSession session)
		{
			lock (this.SyncRoot)
			{
				if (this.availableSessions.ContainsKey(computerName))
				{
					foreach (CimConnectionManager.SessionEntry item in this.availableSessions[computerName])
					{
						if (item.Session != session)
						{
							continue;
						}
						item.RemoveReference();
					}
				}
			}
		}

		private class SessionEntry
		{
			public int IterationsRemaining;

			public CimSessionOptions SessionOptions;

			public CimSession Session;

			private int _numberOfUses;

			private PSCredential _credential;

			private bool _useSsl;

			private uint _port;

			private PSSessionOption _psSessionOption;

			private string _certificateThumbprint;

			private AuthenticationMechanism _authenticationMechanism;

			public AuthenticationMechanism AuthenticationMechanism
			{
				get
				{
					return this._authenticationMechanism;
				}
			}

			public string CertificateThumbprint
			{
				get
				{
					return this._certificateThumbprint;
				}
			}

			public PSCredential Credential
			{
				get
				{
					return this._credential;
				}
			}

			public int GetReferenceCount
			{
				get
				{
					return this._numberOfUses;
				}
			}

			public uint Port
			{
				get
				{
					return this._port;
				}
			}

			public PSSessionOption PSSessionOption
			{
				get
				{
					return this._psSessionOption;
				}
			}

			public bool UseSsl
			{
				get
				{
					return this._useSsl;
				}
			}

			public SessionEntry(string computerName, PSCredential credential, string certificateThumbprint, AuthenticationMechanism authenticationMechanism, CimSessionOptions sessionOptions, bool useSsl, uint port, PSSessionOption pssessionOption)
			{
				this.IterationsRemaining = 6;
				this.SessionOptions = sessionOptions;
				this._credential = credential;
				this._certificateThumbprint = certificateThumbprint;
				this._authenticationMechanism = authenticationMechanism;
				this._useSsl = useSsl;
				this._port = port;
				this._psSessionOption = pssessionOption;
				this.Session = CimSession.Create(computerName, sessionOptions);
			}

			public void AddReference()
			{
				Interlocked.Add(ref this._numberOfUses, 1);
			}

			public void RemoveReference()
			{
				Interlocked.Decrement(ref this._numberOfUses);
			}
		}
	}
}