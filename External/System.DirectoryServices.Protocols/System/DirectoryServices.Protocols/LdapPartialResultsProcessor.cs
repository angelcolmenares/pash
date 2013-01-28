using System;
using System.Collections;
using System.Threading;

namespace System.DirectoryServices.Protocols
{
	internal class LdapPartialResultsProcessor
	{
		private ArrayList resultList;

		private ManualResetEvent workThreadWaitHandle;

		private bool workToDo;

		private int currentIndex;

		internal LdapPartialResultsProcessor(ManualResetEvent eventHandle)
		{
			this.resultList = new ArrayList();
			this.workThreadWaitHandle = eventHandle;
		}

		public void Add(LdapPartialAsyncResult asyncResult)
		{
			lock (this)
			{
				this.resultList.Add(asyncResult);
				if (!this.workToDo)
				{
					this.workThreadWaitHandle.Set();
					this.workToDo = true;
				}
			}
		}

		private void AddResult(SearchResponse partialResults, SearchResponse newResult)
		{
			if (newResult != null)
			{
				if (newResult.Entries != null)
				{
					for (int i = 0; i < newResult.Entries.Count; i++)
					{
						partialResults.Entries.Add(newResult.Entries[i]);
					}
				}
				if (newResult.References != null)
				{
					for (int j = 0; j < newResult.References.Count; j++)
					{
						partialResults.References.Add(newResult.References[j]);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		public DirectoryResponse GetCompleteResult(LdapPartialAsyncResult asyncResult)
		{
			DirectoryResponse directoryResponse;
			lock (this)
			{
				if (this.resultList.Contains(asyncResult))
				{
					this.resultList.Remove(asyncResult);
					if (asyncResult.exception == null)
					{
						directoryResponse = asyncResult.response;
					}
					else
					{
						throw asyncResult.exception;
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
				}
			}
			return directoryResponse;
		}

		public PartialResultsCollection GetPartialResults(LdapPartialAsyncResult asyncResult)
		{
			PartialResultsCollection partialResultsCollection;
			lock (this)
			{
				if (this.resultList.Contains(asyncResult))
				{
					if (asyncResult.exception == null)
					{
						PartialResultsCollection partialResultsCollection1 = new PartialResultsCollection();
						if (asyncResult.response != null)
						{
							if (asyncResult.response.Entries != null)
							{
								for (int i = 0; i < asyncResult.response.Entries.Count; i++)
								{
									partialResultsCollection1.Add(asyncResult.response.Entries[i]);
								}
								asyncResult.response.Entries.Clear();
							}
							if (asyncResult.response.References != null)
							{
								for (int j = 0; j < asyncResult.response.References.Count; j++)
								{
									partialResultsCollection1.Add(asyncResult.response.References[j]);
								}
								asyncResult.response.References.Clear();
							}
						}
						partialResultsCollection = partialResultsCollection1;
					}
					else
					{
						this.resultList.Remove(asyncResult);
						throw asyncResult.exception;
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
				}
			}
			return partialResultsCollection;
		}

		private void GetResultsHelper(LdapPartialAsyncResult asyncResult)
		{
			LdapConnection ldapConnection = asyncResult.con;
			ResultAll resultAll = ResultAll.LDAP_MSG_RECEIVED;
			if (asyncResult.resultStatus == ResultsStatus.CompleteResult)
			{
				resultAll = ResultAll.LDAP_MSG_POLLINGALL;
			}
			try
			{
				SearchResponse searchResponse = (SearchResponse)ldapConnection.ConstructResponse(asyncResult.messageID, LdapOperation.LdapSearch, resultAll, asyncResult.requestTimeout, false);
				if (searchResponse != null)
				{
					if (asyncResult.response == null)
					{
						asyncResult.response = searchResponse;
					}
					else
					{
						this.AddResult(asyncResult.response, searchResponse);
					}
					if (searchResponse.searchDone)
					{
						asyncResult.resultStatus = ResultsStatus.Done;
					}
				}
				else
				{
					DateTime now = DateTime.Now;
					if (asyncResult.startTime.Ticks + asyncResult.requestTimeout.Ticks <= now.Ticks)
					{
						throw new LdapException(85, LdapErrorMappings.MapResultCode(85));
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (exception as DirectoryOperationException == null)
				{
					if (exception as LdapException != null)
					{
						LdapException ldapException = (LdapException)exception;
						//TODO: Review: ldapException.ErrorCode;
						if (asyncResult.response != null)
						{
							if (asyncResult.response.Entries != null)
							{
								for (int i = 0; i < asyncResult.response.Entries.Count; i++)
								{
									ldapException.results.Add(asyncResult.response.Entries[i]);
								}
							}
							if (asyncResult.response.References != null)
							{
								for (int j = 0; j < asyncResult.response.References.Count; j++)
								{
									ldapException.results.Add(asyncResult.response.References[j]);
								}
							}
						}
					}
				}
				else
				{
					SearchResponse response = (SearchResponse)((DirectoryOperationException)exception).Response;
					if (asyncResult.response == null)
					{
						asyncResult.response = response;
					}
					else
					{
						this.AddResult(asyncResult.response, response);
					}
					((DirectoryOperationException)exception).response = asyncResult.response;
				}
				asyncResult.exception = exception;
				asyncResult.resultStatus = ResultsStatus.Done;
				Wldap32.ldap_abandon(ldapConnection.ldapHandle, asyncResult.messageID);
			}
		}

		public void NeedCompleteResult(LdapPartialAsyncResult asyncResult)
		{
			lock (this)
			{
				if (!this.resultList.Contains(asyncResult))
				{
					throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
				}
				else
				{
					if (asyncResult.resultStatus == ResultsStatus.PartialResult)
					{
						asyncResult.resultStatus = ResultsStatus.CompleteResult;
					}
				}
			}
		}

		public void Remove(LdapPartialAsyncResult asyncResult)
		{
			lock (this)
			{
				if (this.resultList.Contains(asyncResult))
				{
					this.resultList.Remove(asyncResult);
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
				}
			}
		}

		public void RetrievingSearchResults()
		{
			int num = 0;
			LdapPartialAsyncResult item = null;
			AsyncCallback asyncCallback = null;
			lock (this)
			{
				int count = this.resultList.Count;
				if (count != 0)
				{
					do
					{
						if (this.currentIndex >= count)
						{
							this.currentIndex = 0;
						}
						item = (LdapPartialAsyncResult)this.resultList[this.currentIndex];
						num++;
						LdapPartialResultsProcessor ldapPartialResultsProcessor = this;
						ldapPartialResultsProcessor.currentIndex = ldapPartialResultsProcessor.currentIndex + 1;
						if (item.resultStatus == ResultsStatus.Done)
						{
							continue;
						}
						this.GetResultsHelper(item);
						if (item.resultStatus != ResultsStatus.Done)
						{
							if (item.callback != null && item.partialCallback && item.response != null && (item.response.Entries.Count > 0 || item.response.References.Count > 0))
							{
								asyncCallback = item.callback;
							}
						}
						else
						{
							item.manualResetEvent.Set();
							item.completed = true;
							if (item.callback != null)
							{
								asyncCallback = item.callback;
							}
						}
						if (asyncCallback != null)
						{
							asyncCallback(item);
						}
						return;
					}
					while (num < count);
					this.workToDo = false;
					this.workThreadWaitHandle.Reset();
					return;
				}
				else
				{
					this.workThreadWaitHandle.Reset();
					this.workToDo = false;
					return;
				}
			}
			if (asyncCallback != null)
			{
				asyncCallback(item);
			}
		}
	}
}