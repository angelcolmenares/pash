using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionState : IDisposable
	{
		internal static string CimSessionClassName;

		internal static string CimSessionObject;

		internal static string SessionObjectPath;

		internal static string idPropName;

		internal static string instanceidPropName;

		internal static string namePropName;

		internal static string computernamePropName;

		internal static string protocolPropName;

		private uint sessionNameCounter;

		private Dictionary<string, HashSet<CimSessionWrapper>> curCimSessionsByName;

		private Dictionary<string, HashSet<CimSessionWrapper>> curCimSessionsByComputerName;

		private Dictionary<Guid, CimSessionWrapper> curCimSessionsByInstanceId;

		private Dictionary<uint, CimSessionWrapper> curCimSessionsById;

		private Dictionary<CimSession, CimSessionWrapper> curCimSessionWrapper;

		private bool _disposed;

		static CimSessionState()
		{
			CimSessionState.CimSessionClassName = "CimSession";
			CimSessionState.CimSessionObject = "{CimSession Object}";
			CimSessionState.SessionObjectPath = "CimSession id = {0}, name = {2}, ComputerName = {3}, instance id = {1}";
			CimSessionState.idPropName = "Id";
			CimSessionState.instanceidPropName = "InstanceId";
			CimSessionState.namePropName = "Name";
			CimSessionState.computernamePropName = "ComputerName";
			CimSessionState.protocolPropName = "Protocol";
		}

		internal CimSessionState()
		{
			this.sessionNameCounter = 1;
			this.curCimSessionsByName = new Dictionary<string, HashSet<CimSessionWrapper>>(StringComparer.OrdinalIgnoreCase);
			this.curCimSessionsByComputerName = new Dictionary<string, HashSet<CimSessionWrapper>>(StringComparer.OrdinalIgnoreCase);
			this.curCimSessionsByInstanceId = new Dictionary<Guid, CimSessionWrapper>();
			this.curCimSessionsById = new Dictionary<uint, CimSessionWrapper>();
			this.curCimSessionWrapper = new Dictionary<CimSession, CimSessionWrapper>();
		}

		private void AddErrorRecord(ref List<ErrorRecord> errRecords, string propertyName, object propertyValue)
		{
			object[] objArray = new object[2];
			objArray[0] = propertyName;
			objArray[1] = propertyValue;
			errRecords.Add(new ErrorRecord(new CimException(string.Format(CultureInfo.CurrentUICulture, Strings.CouldNotFindCimsessionObject, objArray)), string.Empty, ErrorCategory.ObjectNotFound, null));
		}

		internal PSObject AddObjectToCache(CimSession session, uint sessionId, Guid instanceId, string name, string computerName, ProtocolType protocol)
		{
			HashSet<CimSessionWrapper> cimSessionWrappers = null;
			CimSessionWrapper cimSessionWrapper = new CimSessionWrapper(sessionId, instanceId, name, computerName, session, protocol);
			if (!this.curCimSessionsByComputerName.TryGetValue(computerName, out cimSessionWrappers))
			{
				cimSessionWrappers = new HashSet<CimSessionWrapper>();
				this.curCimSessionsByComputerName.Add(computerName, cimSessionWrappers);
			}
			cimSessionWrappers.Add(cimSessionWrapper);
			if (!this.curCimSessionsByName.TryGetValue(name, out cimSessionWrappers))
			{
				cimSessionWrappers = new HashSet<CimSessionWrapper>();
				this.curCimSessionsByName.Add(name, cimSessionWrappers);
			}
			cimSessionWrappers.Add(cimSessionWrapper);
			this.curCimSessionsByInstanceId.Add(instanceId, cimSessionWrapper);
			this.curCimSessionsById.Add(sessionId, cimSessionWrapper);
			this.curCimSessionWrapper.Add(session, cimSessionWrapper);
			return cimSessionWrapper.GetPSObject();
		}

		public void Cleanup()
		{
			foreach (CimSession key in this.curCimSessionWrapper.Keys)
			{
				key.Dispose();
			}
			this.curCimSessionWrapper.Clear();
			this.curCimSessionsByName.Clear();
			this.curCimSessionsByComputerName.Clear();
			this.curCimSessionsByInstanceId.Clear();
			this.curCimSessionsById.Clear();
			this.sessionNameCounter = 1;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this._disposed && disposing)
			{
				this.Cleanup();
				this._disposed = true;
			}
		}

		internal uint GenerateSessionId()
		{
			CimSessionState cimSessionState = this;
			uint num = cimSessionState.sessionNameCounter;
			uint num1 = num;
			cimSessionState.sessionNameCounter = num + 1;
			return num1;
		}

		internal string GetRemoveSessionObjectTarget(PSObject psObject)
		{
			string empty = string.Empty;
			if (psObject.BaseObject as CimSession != null)
			{
				uint num = 0;
				Guid value = Guid.Empty;
				string str = string.Empty;
				string empty1 = string.Empty;
				if (psObject.Properties[CimSessionState.idPropName].Value is uint)
				{
					num = Convert.ToUInt32(psObject.Properties[CimSessionState.idPropName].Value, null);
				}
				if (psObject.Properties[CimSessionState.instanceidPropName].Value is Guid)
				{
					value = (Guid)psObject.Properties[CimSessionState.instanceidPropName].Value;
				}
				if (psObject.Properties[CimSessionState.namePropName].Value as string != null)
				{
					str = (string)psObject.Properties[CimSessionState.namePropName].Value;
				}
				if (psObject.Properties[CimSessionState.computernamePropName].Value as string != null)
				{
					empty1 = (string)psObject.Properties[CimSessionState.computernamePropName].Value;
				}
				object[] objArray = new object[4];
				objArray[0] = num;
				objArray[1] = value;
				objArray[2] = str;
				objArray[3] = empty1;
				empty = string.Format(CultureInfo.CurrentUICulture, CimSessionState.SessionObjectPath, objArray);
			}
			return empty;
		}

		internal int GetSessionsCount()
		{
			return this.curCimSessionsById.Count;
		}

		internal IEnumerable<PSObject> QuerySession(IEnumerable<uint> ids, out IEnumerable<ErrorRecord> errorRecords)
		{
			HashSet<PSObject> pSObjects = new HashSet<PSObject>();
			HashSet<uint> nums = new HashSet<uint>();
			List<ErrorRecord> errorRecords1 = new List<ErrorRecord>();
			errorRecords = errorRecords1;
			foreach (uint id in ids)
			{
				if (!this.curCimSessionsById.ContainsKey(id))
				{
					this.AddErrorRecord(ref errorRecords1, CimSessionState.idPropName, id);
				}
				else
				{
					if (nums.Contains(id))
					{
						continue;
					}
					nums.Add(id);
					pSObjects.Add(this.curCimSessionsById[id].GetPSObject());
				}
			}
			return pSObjects;
		}

		internal IEnumerable<PSObject> QuerySession(IEnumerable<Guid> instanceIds, out IEnumerable<ErrorRecord> errorRecords)
		{
			HashSet<PSObject> pSObjects = new HashSet<PSObject>();
			HashSet<uint> nums = new HashSet<uint>();
			List<ErrorRecord> errorRecords1 = new List<ErrorRecord>();
			errorRecords = errorRecords1;
			foreach (Guid instanceId in instanceIds)
			{
				if (!this.curCimSessionsByInstanceId.ContainsKey(instanceId))
				{
					this.AddErrorRecord(ref errorRecords1, CimSessionState.instanceidPropName, instanceId);
				}
				else
				{
					CimSessionWrapper item = this.curCimSessionsByInstanceId[instanceId];
					if (nums.Contains(item.SessionId))
					{
						continue;
					}
					nums.Add(item.SessionId);
					pSObjects.Add(item.GetPSObject());
				}
			}
			return pSObjects;
		}

		internal IEnumerable<PSObject> QuerySession(IEnumerable<string> nameArray, out IEnumerable<ErrorRecord> errorRecords)
		{
			HashSet<PSObject> pSObjects = new HashSet<PSObject>();
			HashSet<uint> nums = new HashSet<uint>();
			List<ErrorRecord> errorRecords1 = new List<ErrorRecord>();
			errorRecords = errorRecords1;
			foreach (var str in this.curCimSessionsByName)
			{
				bool count = false;
				WildcardPattern wildcardPattern = new WildcardPattern(str.Key, WildcardOptions.IgnoreCase);
				Dictionary<string, HashSet<CimSessionWrapper>>.Enumerator enumerator = this.curCimSessionsByName.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, HashSet<CimSessionWrapper>> keyValuePair = str;
						if (!wildcardPattern.IsMatch(keyValuePair.Key))
						{
							continue;
						}
						HashSet<CimSessionWrapper> value = keyValuePair.Value;
						count = value.Count > 0;
						HashSet<CimSessionWrapper>.Enumerator enumerator1 = value.GetEnumerator();
						try
						{
							while (enumerator1.MoveNext())
							{
								CimSessionWrapper current = enumerator1.Current;
								if (nums.Contains(current.SessionId))
								{
									continue;
								}
								nums.Add(current.SessionId);
								pSObjects.Add(current.GetPSObject());
							}
						}
						finally
						{
							enumerator1.Dispose();
						}
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				if (count || WildcardPattern.ContainsWildcardCharacters(str.Key))
				{
					continue;
				}
				this.AddErrorRecord(ref errorRecords1, CimSessionState.namePropName, str.Key);
			}
			return pSObjects;
		}

		internal IEnumerable<PSObject> QuerySession(IEnumerable<CimSession> cimsessions, out IEnumerable<ErrorRecord> errorRecords)
		{
			HashSet<PSObject> pSObjects = new HashSet<PSObject>();
			HashSet<uint> nums = new HashSet<uint>();
			List<ErrorRecord> errorRecords1 = new List<ErrorRecord>();
			errorRecords = errorRecords1;
			foreach (CimSession cimsession in cimsessions)
			{
				if (!this.curCimSessionWrapper.ContainsKey(cimsession))
				{
					this.AddErrorRecord(ref errorRecords1, CimSessionState.CimSessionClassName, CimSessionState.CimSessionObject);
				}
				else
				{
					CimSessionWrapper item = this.curCimSessionWrapper[cimsession];
					if (nums.Contains(item.SessionId))
					{
						continue;
					}
					nums.Add(item.SessionId);
					pSObjects.Add(item.GetPSObject());
				}
			}
			return pSObjects;
		}

		internal CimSessionWrapper QuerySession(CimSession cimsession)
		{
			CimSessionWrapper cimSessionWrapper = null;
			this.curCimSessionWrapper.TryGetValue(cimsession, out cimSessionWrapper);
			return cimSessionWrapper;
		}

		internal CimSession QuerySession(Guid cimSessionInstanceId)
		{
			if (!this.curCimSessionsByInstanceId.ContainsKey(cimSessionInstanceId))
			{
				return null;
			}
			else
			{
				CimSessionWrapper item = this.curCimSessionsByInstanceId[cimSessionInstanceId];
				return item.CimSession;
			}
		}

		internal IEnumerable<PSObject> QuerySessionByComputerName(IEnumerable<string> computernameArray, out IEnumerable<ErrorRecord> errorRecords)
		{
			HashSet<PSObject> pSObjects = new HashSet<PSObject>();
			HashSet<uint> nums = new HashSet<uint>();
			List<ErrorRecord> errorRecords1 = new List<ErrorRecord>();
			errorRecords = errorRecords1;
			foreach (string str in computernameArray)
			{
				bool count = false;
				if (this.curCimSessionsByComputerName.ContainsKey(str))
				{
					HashSet<CimSessionWrapper> item = this.curCimSessionsByComputerName[str];
					count = item.Count > 0;
					foreach (CimSessionWrapper cimSessionWrapper in item)
					{
						if (nums.Contains(cimSessionWrapper.SessionId))
						{
							continue;
						}
						nums.Add(cimSessionWrapper.SessionId);
						pSObjects.Add(cimSessionWrapper.GetPSObject());
					}
				}
				if (count)
				{
					continue;
				}
				this.AddErrorRecord(ref errorRecords1, CimSessionState.computernamePropName, str);
			}
			return pSObjects;
		}

		internal void RemoveOneSessionObjectFromCache(PSObject psObject)
		{
			DebugHelper.WriteLogEx();
			if (psObject.BaseObject as CimSession != null)
			{
				this.RemoveOneSessionObjectFromCache(psObject.BaseObject as CimSession);
			}
		}

		internal void RemoveOneSessionObjectFromCache(CimSession session)
		{
			HashSet<CimSessionWrapper> cimSessionWrappers = null;
			DebugHelper.WriteLogEx();
			if (this.curCimSessionWrapper.ContainsKey(session))
			{
				CimSessionWrapper item = this.curCimSessionWrapper[session];
				string name = item.Name;
				string computerName = item.ComputerName;
				object[] sessionId = new object[4];
				sessionId[0] = name;
				sessionId[1] = computerName;
				sessionId[2] = item.SessionId;
				sessionId[3] = item.InstanceId;
				DebugHelper.WriteLog("name {0}, computername {1}, id {2}, instanceId {3}", 1, sessionId);
				if (this.curCimSessionsByComputerName.TryGetValue(computerName, out cimSessionWrappers))
				{
					cimSessionWrappers.Remove(item);
				}
				if (this.curCimSessionsByName.TryGetValue(name, out cimSessionWrappers))
				{
					cimSessionWrappers.Remove(item);
				}
				this.RemoveSessionInternal(session, item);
				return;
			}
			else
			{
				return;
			}
		}

		private void RemoveSessionInternal(CimSession session, CimSessionWrapper wrapper)
		{
			DebugHelper.WriteLogEx();
			this.curCimSessionsByInstanceId.Remove(wrapper.InstanceId);
			this.curCimSessionsById.Remove(wrapper.SessionId);
			this.curCimSessionWrapper.Remove(session);
			session.Dispose();
		}
	}
}