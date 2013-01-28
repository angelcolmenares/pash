using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSessionCache
	{
		private static ADSessionCache _cacheObject;

		private static string _debugCategory;

		private static string _defaultServerKey;

		private static Dictionary<string, List<ADSessionCache.CacheEntry>> _sessionMap;

		static ADSessionCache()
		{
			ADSessionCache._cacheObject = new ADSessionCache();
			ADSessionCache._debugCategory = "ADSessionCache";
			Guid guid = Guid.NewGuid();
			ADSessionCache._defaultServerKey = guid.ToString();
			ADSessionCache._sessionMap = new Dictionary<string, List<ADSessionCache.CacheEntry>>(StringComparer.OrdinalIgnoreCase);
		}

		private ADSessionCache()
		{
		}

		public void AddEntry(ADSessionInfo info, ADSession session)
		{
			PSCredential credential;
			string str = this.GenerateKey(info);
			DebugLogger.LogInfo(ADSessionCache._debugCategory, string.Concat("AddEntry: Adding Entry , Key: ", str));
			ADSessionCache.CacheEntry cacheEntry = new ADSessionCache.CacheEntry();
			cacheEntry.Session = session;
			ADSessionCache.CacheEntry cacheEntry1 = cacheEntry;
			if (info == null)
			{
				credential = null;
			}
			else
			{
				credential = info.Credential;
			}
			cacheEntry1.Credential = credential;
			if (!this.AreCredentialsExplicit(info))
			{
				WindowsIdentity current = WindowsIdentity.GetCurrent(true);
				if (current != null)
				{
					cacheEntry.ImpersonatedOwnerSid = current.Owner;
				}
			}
			if (!ADSessionCache._sessionMap.ContainsKey(str))
			{
				ADSessionCache._sessionMap.Add(str, new List<ADSessionCache.CacheEntry>(1));
				ADSessionCache._sessionMap[str].Add(cacheEntry);
				return;
			}
			else
			{
				ADSessionCache._sessionMap[str].Add(cacheEntry);
				return;
			}
		}

		private bool AreCredentialsExplicit(ADSessionInfo info)
		{
			if (info == null)
			{
				return false;
			}
			else
			{
				return ADPasswordUtils.AreCredentialsExplicit(info.Credential);
			}
		}

		public void DeleteEntry(ADSessionInfo info)
		{
			string str = this.GenerateKey(info);
			DebugLogger.LogInfo(ADSessionCache._debugCategory, string.Concat("DeleteEntry: removing entry ", str, " from sessioncache"));
			int num = 0;
			if (this.FindEntryInServerList(str, info, out num) != null)
			{
				ADSessionCache._sessionMap[str].RemoveAt(num);
				if (ADSessionCache._sessionMap[str].Count == 0)
				{
					ADSessionCache._sessionMap.Remove(str);
				}
			}
		}

		private ADSessionCache.CacheEntry FindEntryInServerList(string key, ADSessionInfo info, out int index)
		{
			List<ADSessionCache.CacheEntry> cacheEntries = null;
			ADSessionCache.CacheEntry item = null;
			int num;
			index = -1;
			if (ADSessionCache._sessionMap.TryGetValue(key, out cacheEntries))
			{
				if (DebugLogger.Level >= DebugLogLevel.Info)
				{
					object[] count = new object[4];
					count[0] = "FindEntryInServerList: Found ";
					count[1] = cacheEntries.Count;
					count[2] = " session(s) in cache with key: ";
					count[3] = key;
					DebugLogger.LogInfo(ADSessionCache._debugCategory, string.Concat(count));
				}
				bool flag = this.AreCredentialsExplicit(info);
				WindowsIdentity current = null;
				if (!flag)
				{
					current = WindowsIdentity.GetCurrent(true);
				}
				int i = 0;
				bool flag1 = false;
				for (i = 0; i < cacheEntries.Count; i++)
				{
					item = cacheEntries[i];
					if (item.Session.MatchConnectionOptions(info))
					{
						bool flag2 = ADPasswordUtils.AreCredentialsExplicit(item.Credential);
						if (!flag || !flag2)
						{
							if (!flag && !flag2)
							{
								if (current != null || !(null == item.ImpersonatedOwnerSid))
								{
									if (current.Owner == item.ImpersonatedOwnerSid)
									{
										flag1 = true;
										break;
									}
								}
								else
								{
									flag1 = true;
									break;
								}
							}
						}
						else
						{
							if (ADPasswordUtils.MatchCredentials(item.Credential, info.Credential))
							{
								flag1 = true;
								break;
							}
						}
					}
					else
					{
						if (DebugLogger.Level >= DebugLogLevel.Info)
						{
							DebugLogger.LogInfo(ADSessionCache._debugCategory, string.Concat("FindEntryInServerList: Connection options did NOT match with session ", item.Session.SessionInfo.Server));
						}
					}
				}
				if (!flag1)
				{
					if (DebugLogger.Level >= DebugLogLevel.Info)
					{
						DebugLogger.LogInfo(ADSessionCache._debugCategory, "FindEntryInServerList. Could NOT find a matching cached session entry");
					}
				}
				else
				{
					if (DebugLogger.Level >= DebugLogLevel.Info)
					{
						DebugLogger.LogInfo(ADSessionCache._debugCategory, string.Concat("FindEntryInServerList. Found a cached session entry: ", item.Session.SessionInfo.Server));
					}
				}
				int numPointer = index;
				if (flag1)
				{
					num = i;
				}
				else
				{
					num = -1;
				}
				numPointer = num;
				if (flag1)
				{
					return item;
				}
				else
				{
					return null;
				}
			}
			else
			{
				if (DebugLogger.Level >= DebugLogLevel.Info)
				{
					DebugLogger.LogInfo(ADSessionCache._debugCategory, string.Concat("FindEntryInServerList: Could NOT find a session in cache with key: ", key));
				}
				return null;
			}
		}

		private string GenerateKey(ADSessionInfo info)
		{
			string serverNameOnly = ADSessionCache._defaultServerKey;
			int lDAPPORT = LdapConstants.LDAP_PORT;
			if (info != null)
			{
				if (!string.IsNullOrEmpty(info.ServerNameOnly))
				{
					serverNameOnly = info.ServerNameOnly;
				}
				lDAPPORT = info.EffectivePortNumber;
			}
			return string.Concat(serverNameOnly, ":", lDAPPORT);
		}

		public bool GetEntry(ADSessionInfo info, out ADSession session)
		{
			DebugLogger.LogInfo(ADSessionCache._debugCategory, "GetEntry: Entering");
			string str = this.GenerateKey(info);
			int num = 0;
			if (ADSessionCache._sessionMap.ContainsKey(str))
			{
				ADSessionCache.CacheEntry cacheEntry = this.FindEntryInServerList(str, info, out num);
				if (cacheEntry != null)
				{
					session = cacheEntry.Session;
					return true;
				}
			}
			session = null;
			return false;
		}

		public static ADSessionCache GetObject()
		{
			return ADSessionCache._cacheObject;
		}

		private class CacheEntry
		{
			private PSCredential _credential;

			private SecurityIdentifier _impersonatedOwnerSid;

			private ADSession _session;

			public PSCredential Credential
			{
				get
				{
					return this._credential;
				}
				set
				{
					this._credential = value;
				}
			}

			public SecurityIdentifier ImpersonatedOwnerSid
			{
				get
				{
					return this._impersonatedOwnerSid;
				}
				set
				{
					this._impersonatedOwnerSid = value;
				}
			}

			public ADSession Session
			{
				get
				{
					return this._session;
				}
				set
				{
					this._session = value;
				}
			}

			public CacheEntry()
			{
			}
		}
	}
}