using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Search", "ADAccount", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219343")]
	public class SearchADAccountCmdlet : ADGetCmdletBase<SearchADAccountParameterSet, ADAccountFactory<ADAccount>, ADAccount>
	{
		private const double DEFAULT_ACCOUNTEXPIRING_TIMESPAN_DAYS = 5;

		private const double DEFAULT_ACCOUNTINACTIVE_TIMESPAN_DAYS = 15;

		private static Dictionary<string, double> _accountInactiveTimeSpanCache;

		private static ReaderWriterLockSlim _accountInactiveTimeSpanLock;

		static SearchADAccountCmdlet()
		{
			SearchADAccountCmdlet._accountInactiveTimeSpanCache = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
			SearchADAccountCmdlet._accountInactiveTimeSpanLock = new ReaderWriterLockSlim();
		}

		public SearchADAccountCmdlet()
		{
			base.BeginProcessPipeline.Clear();
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.SearchADAccountBeginCSRoutine));
			base.ProcessRecordPipeline.Clear();
		}

		private bool FilterIsLockedOut(ADAccount output)
		{
			bool? value = (bool?)output.GetValue("LockedOut");
			if (!value.HasValue || !value.Value)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private double GetInactiveTimeSpanDays(CmdletSessionInfo cmdletSessionInfo)
		{
			double num = 0;
			double num1;
			ADRootDSE rootDSE = this.GetRootDSE();
			string defaultNamingContext = rootDSE.DefaultNamingContext;
			if (rootDSE.ServerType != ADServerType.ADDS || defaultNamingContext == null)
			{
				return 15;
			}
			else
			{
				SearchADAccountCmdlet._accountInactiveTimeSpanLock.EnterReadLock();
				try
				{
					if (!SearchADAccountCmdlet._accountInactiveTimeSpanCache.TryGetValue(defaultNamingContext, out num))
					{
						double value = 15;
						ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, defaultNamingContext, ADSearchScope.Base);
						using (aDObjectSearcher)
						{
							aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
							aDObjectSearcher.Properties.Add("msDS-LogonTimeSyncInterval");
							ADObject aDObject = aDObjectSearcher.FindOne();
							if (aDObject != null && aDObject.Contains("msDS-LogonTimeSyncInterval") && aDObject["msDS-LogonTimeSyncInterval"].Count > 0)
							{
								value = (double)((int)aDObject["msDS-LogonTimeSyncInterval"].Value);
							}
						}
						SearchADAccountCmdlet._accountInactiveTimeSpanLock.EnterWriteLock();
						try
						{
							SearchADAccountCmdlet._accountInactiveTimeSpanCache[defaultNamingContext] = value;
						}
						finally
						{
							SearchADAccountCmdlet._accountInactiveTimeSpanLock.ExitWriteLock();
						}
						return value;
					}
					else
					{
						num1 = num;
					}
				}
				finally
				{
					SearchADAccountCmdlet._accountInactiveTimeSpanLock.ExitReadLock();
				}
				return num1;
			}
		}

		private bool SearchADAccountBeginCSRoutine()
		{
			IADOPathNode item;
			DateTime value;
			DateTime dateTime;
			List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			this.ValidateParameters();
			string[] strArrays = new string[6];
			strArrays[0] = "Enabled";
			strArrays[1] = "LockedOut";
			strArrays[2] = "PasswordNeverExpires";
			strArrays[3] = "PasswordExpired";
			strArrays[4] = "AccountExpirationDate";
			strArrays[5] = "LastLogonDate";
			this._cmdletParameters["Properties"] = strArrays;
			base.BuildPropertySet();
			new StringBuilder();
			int num = 0;
			//TODO: Review: this._cmdletParameters.ComputersOnly;
			if (this._cmdletParameters.ComputersOnly)
			{
				aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "computer"));
				num++;
			}
			//TODO: Review: this._cmdletParameters.UsersOnly;
			if (this._cmdletParameters.UsersOnly)
			{
				aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Ne, "objectClass", "computer"));
				num++;
			}
			if (num <= 1)
			{
				double? nullable = null;
				DateTime? nullable1 = null;
				if (this._cmdletParameters.Contains("TimeSpan"))
				{
					//TODO: Review: this._cmdletParameters.TimeSpan;
					TimeSpan timeSpan = this._cmdletParameters.TimeSpan;
					nullable = new double?(timeSpan.TotalDays);
				}
				if (this._cmdletParameters.Contains("DateTime"))
				{
					//TODO: Review: this._cmdletParameters.DateTime;
					nullable1 = new DateTime?(this._cmdletParameters.DateTime);
				}
				if (!this._cmdletParameters.AccountDisabled)
				{
					if (!this._cmdletParameters.PasswordNeverExpires)
					{
						if (!this._cmdletParameters.PasswordExpired)
						{
							if (!this._cmdletParameters.AccountExpiring)
							{
								if (!this._cmdletParameters.AccountExpired)
								{
									if (!this._cmdletParameters.AccountInactive)
									{
										if (this._cmdletParameters.LockedOut)
										{
											aDOPathNodes.Add(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["AccountLockoutTime"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Ge, "AccountLockoutTime", 1), cmdletSessionInfo));
											base.OutputFilterFunction = new ADGetCmdletBase<SearchADAccountParameterSet, ADAccountFactory<ADAccount>, ADAccount>.OutputFilterDelegate(this.FilterIsLockedOut);
										}
									}
									else
									{
										double inactiveTimeSpanDays = this.GetInactiveTimeSpanDays(cmdletSessionInfo);
										if (!nullable1.HasValue)
										{
											if (!nullable.HasValue)
											{
												DateTime now = DateTime.Now;
												dateTime = now.AddDays(-inactiveTimeSpanDays);
											}
											else
											{
												DateTime now1 = DateTime.Now;
												dateTime = now1.AddDays(-(nullable.Value + inactiveTimeSpanDays));
											}
										}
										else
										{
											DateTime value1 = nullable1.Value;
											dateTime = value1.AddDays(-inactiveTimeSpanDays);
										}
										aDOPathNodes.Add(ADOPathUtil.CreateNotClause(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["LastLogonDate"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Ge, "LastLogonDate", dateTime), cmdletSessionInfo)));
									}
								}
								else
								{
									aDOPathNodes.Add(ADOPathUtil.CreateNotClause(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["AccountExpirationDate"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Ge, "AccountExpirationDate", DateTime.Now), cmdletSessionInfo)));
									aDOPathNodes.Add(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["AccountExpirationDate"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Like, "AccountExpirationDate", "*"), cmdletSessionInfo));
								}
							}
							else
							{
								DateTime dateTime1 = DateTime.Now;
								if (!nullable1.HasValue)
								{
									if (!nullable.HasValue)
									{
										value = dateTime1.AddDays(5);
									}
									else
									{
										value = dateTime1.AddDays(nullable.Value);
									}
								}
								else
								{
									value = nullable1.Value;
								}
								aDOPathNodes.Add(ADOPathUtil.CreateNotClause(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["AccountExpirationDate"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Ge, "AccountExpirationDate", value), cmdletSessionInfo)));
								aDOPathNodes.Add(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["AccountExpirationDate"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Ge, "AccountExpirationDate", dateTime1), cmdletSessionInfo));
							}
						}
						else
						{
							aDOPathNodes.Add(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["PasswordExpired"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "PasswordExpired", true), cmdletSessionInfo));
						}
					}
					else
					{
						aDOPathNodes.Add(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["PasswordNeverExpires"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "PasswordNeverExpires", true), cmdletSessionInfo));
					}
				}
				else
				{
					aDOPathNodes.Add(ADAccountFactory<ADAccount>.AttributeTable[cmdletSessionInfo.ConnectedADServerType]["Enabled"].InvokeToSearcherConverter(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "Enabled", false), cmdletSessionInfo));
				}
				if (aDOPathNodes.Count <= 0)
				{
					this.OutputSearchResults(null);
				}
				else
				{
					if (aDOPathNodes.Count != 1)
					{
						item = ADOPathUtil.CreateAndClause(aDOPathNodes.ToArray());
					}
					else
					{
						item = aDOPathNodes[0];
					}
					this.OutputSearchResults(item);
				}
				return true;
			}
			else
			{
				throw new ParameterBindingException(string.Format(StringResources.ParameterRequiredOnlyOne, string.Format("{0} {1}", "ComputersOnly", "UsersOnly")));
			}
		}
	}
}