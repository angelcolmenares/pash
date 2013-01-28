using Microsoft.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Security.Principal;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADObjectSearcher : IDisposable
	{
		private const string _debugCategory = "ADObjectSearcher";

		private const string RANGE_RETRIEVAL_TAG = ";range=";

		private const int RANGE_RETRIEVAL_END_INDEX_LAST_GROUP = -1;

		private const int RANGE_RETRIEVAL_END_INDEX_UNDEFINED = -2147483648;

		public static string AllProperties;

		private static string DefaultSearchFilterString;

		private static IADOPathNode DefaultSearchFilter;

		private ADObject _adObject;

		private ADSession _adSession;

		private IADSyncOperations _syncOps;

		private ADSessionHandle _sessionHandle;

		private ADTypeConverter _typeConverter;

		private string _searchRoot;

		private ADSearchScope _searchScope;

		private IADOPathNode _filter;

		private List<string> _propertyList;

		private bool _propertyNamesOnly;

		private TimeSpan _timeLimit;

		private int _sizeLimit;

		private bool _schemaTranslationEnabled;

		private int _pageSize;

		private SearchOption? _searchOption;

		private bool _showDeleted;

		private bool _suppressServerRangeRetrievalError;

		private bool _autoRangeRetrieve;

		private bool _showDeactivatedLink;

		private SecurityMasks _sdFlags;

		private string _attributeScopedQuery;

		private SecurityIdentifier _quotaQuerySid;

		private string _inputDN;

		private bool _disposed;

		public string AttributeScopedQuery
		{
			get
			{
				return this._attributeScopedQuery;
			}
			set
			{
				this._attributeScopedQuery = value;
			}
		}

		public bool AutoRangeRetrieve
		{
			get
			{
				return this._autoRangeRetrieve;
			}
			set
			{
				this._autoRangeRetrieve = value;
			}
		}

		public IADOPathNode Filter
		{
			get
			{
				return this._filter;
			}
			set
			{
				if (value != null)
				{
					this._filter = value;
					return;
				}
				else
				{
					DebugLogger.LogWarning("ADObjectSearcher", "Filter: null value");
					throw new ArgumentNullException("value");
				}
			}
		}

		public string InputDN
		{
			get
			{
				return this._inputDN;
			}
			set
			{
				this._inputDN = value;
			}
		}

		public int PageSize
		{
			get
			{
				return this._pageSize;
			}
			set
			{
				if (value >= 0)
				{
					this._pageSize = value;
					return;
				}
				else
				{
					DebugLogger.LogWarning("ADObjectSearcher", "PageSize: negative value");
					throw new ArgumentException(StringResources.NoNegativePageSize, "value");
				}
			}
		}

		public List<string> Properties
		{
			get
			{
				return this._propertyList;
			}
			set
			{
				this._propertyList = value;
			}
		}

		public bool PropertyNamesOnly
		{
			get
			{
				return this._propertyNamesOnly;
			}
			set
			{
				this._propertyNamesOnly = value;
			}
		}

		public SecurityIdentifier QuotaQuerySid
		{
			get
			{
				return this._quotaQuerySid;
			}
			set
			{
				this._quotaQuerySid = value;
			}
		}

		internal bool SchemaTranslation
		{
			get
			{
				return this._schemaTranslationEnabled;
			}
			set
			{
				this._schemaTranslationEnabled = value;
			}
		}

		public ADSearchScope Scope
		{
			get
			{
				return this._searchScope;
			}
			set
			{
				this._searchScope = value;
			}
		}

		public SearchOption? SearchOption
		{
			get
			{
				return this._searchOption;
			}
			set
			{
				this._searchOption = value;
			}
		}

		public string SearchRoot
		{
			get
			{
				return this._searchRoot;
			}
			set
			{
				this._searchRoot = value;
			}
		}

		public SecurityMasks SecurityDescriptorFlags
		{
			get
			{
				return this._sdFlags;
			}
			set
			{
				this._sdFlags = value;
			}
		}

		public bool ShowDeactivatedLink
		{
			get
			{
				return this._showDeactivatedLink;
			}
			set
			{
				this._showDeactivatedLink = value;
			}
		}

		public bool ShowDeleted
		{
			get
			{
				return this._showDeleted;
			}
			set
			{
				this._showDeleted = value;
			}
		}

		public int SizeLimit
		{
			get
			{
				return this._sizeLimit;
			}
			set
			{
				if (value >= 0)
				{
					this._sizeLimit = value;
					return;
				}
				else
				{
					DebugLogger.LogWarning("ADObjectSearcher", "SizeLimit: negative value");
					throw new ArgumentException(StringResources.NoNegativeSizeLimit, "value");
				}
			}
		}

		public bool SuppressServerRangeRetrievalError
		{
			get
			{
				return this._suppressServerRangeRetrievalError;
			}
			set
			{
				this._suppressServerRangeRetrievalError = value;
			}
		}

		public TimeSpan TimeLimit
		{
			get
			{
				return this._timeLimit;
			}
			set
			{
				if (value >= TimeSpan.Zero)
				{
					if (value.TotalSeconds <= 2147483647)
					{
						this._timeLimit = value;
						return;
					}
					else
					{
						DebugLogger.LogWarning("ADObjectSearcher", "TimeLimit: exceeded max");
						throw new ArgumentException(StringResources.ExceedMax, "value");
					}
				}
				else
				{
					DebugLogger.LogWarning("ADObjectSearcher", "TimeLimit: negative value");
					throw new ArgumentException(StringResources.NoNegativeTime, "value");
				}
			}
		}

		static ADObjectSearcher()
		{
			ADObjectSearcher.AllProperties = "*";
			ADObjectSearcher.DefaultSearchFilterString = "(objectClass=*)";
			ADObjectSearcher.DefaultSearchFilter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
		}

		public ADObjectSearcher() : this(null, null)
		{
		}

		public ADObjectSearcher(ADSessionInfo sessionInfo) : this(sessionInfo, null)
		{
		}

		public ADObjectSearcher(ADObject obj) : this(null, obj)
		{
		}

		public ADObjectSearcher(ADSessionInfo sessionInfo, ADObject obj)
		{
			this._searchScope = ADSearchScope.Subtree;
			this._filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
			this._propertyList = new List<string>(ADObject.DefaultProperties);
			this._timeLimit = TimeSpan.Zero;
			this._schemaTranslationEnabled = true;
			this._pageSize = 0x100;
			this._searchOption = null;
			this._autoRangeRetrieve = true;
			this._sdFlags = SecurityMasks.Owner | SecurityMasks.Group | SecurityMasks.Dacl;
			if (sessionInfo == null)
			{
				if (obj == null)
				{
					this._adSession = ADSession.ConstructSession(null);
				}
				else
				{
					this._adSession = ADSession.ConstructSession(obj.SessionInfo);
				}
			}
			else
			{
				this._adSession = ADSession.ConstructSession(sessionInfo);
			}
			if (obj != null)
			{
				this._adObject = obj;
				this._searchRoot = this._adObject.DistinguishedName;
				foreach (string propertyName in this._adObject.PropertyNames)
				{
					this._propertyList.Add(propertyName);
				}
			}
		}

		public void AbandonPagedSearch(ref object pageCookie)
		{
			if (pageCookie != null)
			{
				this.Init();
				ADPageResultRequestControl aDPageResultRequestControl = new ADPageResultRequestControl();
				aDPageResultRequestControl.Cookie = pageCookie;
				ADSearchRequest aDSearchRequest = this.CreateSearchRequest(aDPageResultRequestControl);
				this._syncOps.AbandonSearch(this._sessionHandle, aDSearchRequest);
				pageCookie = null;
				return;
			}
			else
			{
				throw new ArgumentNullException("pageCookie");
			}
		}

		public static bool ContainsRangeRetrievalTag(string fetchedAttrName, out string attrName, out int endIndex)
		{
			int num = fetchedAttrName.IndexOf(";range=", StringComparison.OrdinalIgnoreCase);
			if (num == -1)
			{
				attrName = fetchedAttrName;
				endIndex = -2147483648;
				return false;
			}
			else
			{
				attrName = fetchedAttrName.Substring(0, num);
				int num1 = fetchedAttrName.IndexOf("-", num + ";range=".Length);
				string str = fetchedAttrName.Substring(num1 + 1);
				if (!str.Equals("*"))
				{
					endIndex = int.Parse(fetchedAttrName.Substring(num1 + 1), NumberFormatInfo.InvariantInfo);
				}
				else
				{
					endIndex = -1;
				}
				return true;
			}
		}

		private ADObject CreateRichADObject(ADObject resultEntry)
		{
			ADPropertyValueCollection item;
			ADObject aDObject = new ADObject();
			aDObject.DistinguishedName = resultEntry.DistinguishedName;
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			foreach (string propertyName in resultEntry.PropertyNames)
			{
				if (flag || string.Compare(propertyName, "distinguishedName", StringComparison.OrdinalIgnoreCase) != 0)
				{
					if (flag1 || string.Compare(propertyName, "objectClass", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (flag2 || string.Compare(propertyName, "objectGUID", StringComparison.OrdinalIgnoreCase) != 0)
						{
							if (!this._schemaTranslationEnabled)
							{
								aDObject.Add(propertyName, resultEntry[propertyName]);
							}
							else
							{
								aDObject.Add(propertyName, this._typeConverter.ConvertFromRaw(propertyName, resultEntry[propertyName]));
							}
						}
						else
						{
							item = resultEntry[propertyName];
							if (item.Count <= 0)
							{
								Guid? nullable = null;
								aDObject.ObjectGuid = nullable;
							}
							else
							{
								object objectGuidObj = item[0];
								if (objectGuidObj is string)
								{
									byte[] objGuidBytes = Encoding.Default.GetBytes ((string)objectGuidObj);
									try {
										aDObject.ObjectGuid = new Guid?(new Guid(objGuidBytes));
									}
									catch
									{
										aDObject.ObjectGuid = null;
									}
								}
								else if (objectGuidObj is byte[]) {
									aDObject.ObjectGuid = new Guid?(new Guid((byte[])item[0]));
								}
								else {
									aDObject.ObjectGuid = null;
								}

							}
							flag2 = true;
						}
					}
					else
					{
						item = resultEntry[propertyName];
						if (item.Count <= 0)
						{
							aDObject.ObjectClass = null;
						}
						else
						{
							aDObject.ObjectClass = (string)item[item.Count - 1];
							aDObject.ObjectTypes = item;
						}
						flag1 = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			aDObject.IsSearchResult = true;
			return aDObject;
		}

		private ADSearchRequest CreateSearchRequest(ADPageResultRequestControl pageControl)
		{
			if (this._autoRangeRetrieve)
			{
				if (string.IsNullOrEmpty(this._attributeScopedQuery))
				{
					foreach (string str in this._propertyList)
					{
						if (str.IndexOf(";range=", StringComparison.OrdinalIgnoreCase) <= 0)
						{
							continue;
						}
						throw new ArgumentException(str);
					}
				}
				else
				{
					throw new NotSupportedException();
				}
			}
			bool flag = true;
			ADSearchRequest aDSearchRequest = new ADSearchRequest(this._searchRoot, this._filter.GetLdapFilterString(), (SearchScope)this._searchScope, this._propertyList.ToArray());
			aDSearchRequest.TypesOnly = this._propertyNamesOnly;
			aDSearchRequest.TimeLimit = this._timeLimit;
			aDSearchRequest.SizeLimit = this._sizeLimit;
			if (pageControl == null)
			{
				pageControl = new ADPageResultRequestControl(this._pageSize);
			}
			aDSearchRequest.Controls.Add(pageControl);
			if (this._searchOption.HasValue)
			{
				aDSearchRequest.Controls.Add(new SearchOptionsControl(this._searchOption.Value));
			}
			if (this._showDeleted)
			{
				aDSearchRequest.Controls.Add(new ShowDeletedControl());
			}
			if (this._showDeactivatedLink)
			{
				aDSearchRequest.Controls.Add(new ADShowDeactivatedLinkControl());
			}
			if (this._suppressServerRangeRetrievalError)
			{
				aDSearchRequest.Controls.Add(new ADSupressRangeRetrievalErrorControl());
			}
			if (this._sdFlags != SecurityMasks.None)
			{
				aDSearchRequest.Controls.Add(new SecurityDescriptorFlagControl(this._sdFlags));
			}
			if (this._attributeScopedQuery != null)
			{
				aDSearchRequest.Controls.Add(new AsqRequestControl(this._attributeScopedQuery));
				flag = false;
			}
			if (this._quotaQuerySid != null)
			{
				aDSearchRequest.Controls.Add(new QuotaControl(this._quotaQuerySid));
			}
			if (this._inputDN != null)
			{
				aDSearchRequest.Controls.Add(new ADInputDNControl(this._inputDN));
			}
			aDSearchRequest.ObjectScopedControls = flag;
			return aDSearchRequest;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Uninit();
			}
			this._disposed = true;
		}

		private static void ExtractRangeRetrievalDataFromAttributes(ADObject fetchedObject, ADObject targetObject, HashSet<string> rangeRetrievedObjects, HashSet<string> rangeRetrievedAttributes, ref int rangeRetrievalNextIndex)
		{
			string str = null;
			int num = 0;
			string[] strArrays = new string[fetchedObject.PropertyNames.Count];
			fetchedObject.PropertyNames.CopyTo(strArrays, 0);
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str1 = strArrays1[i];
				if (ADObjectSearcher.ContainsRangeRetrievalTag(str1, out str, out num))
				{
					rangeRetrievedObjects.Add(fetchedObject.DistinguishedName);
					if (num != -1)
					{
						rangeRetrievedAttributes.Add(str);
						if (rangeRetrievalNextIndex == -2147483648)
						{
							rangeRetrievalNextIndex = num + 1;
						}
					}
					//targetObject[str];
					targetObject[str].AddRange(fetchedObject[str1]);
					if (targetObject.Contains(str1))
					{
						targetObject.Remove(str1);
					}
				}
			}
		}

		private static IEnumerable<ADObject> FetchRemainingRangeRetrievalAttributeValues(ADObjectSearcher newSearcher, ADObjectSearcher originalSearcher, HashSet<string> rangeRetrievedObjects, HashSet<string> rangeRetrievedAttributes, int rangeRetrievalNextIndex)
		{
			DebugLogger.LogInfo("ADObjectSearcher", string.Concat("Inside FetchRemainingRangeRetrievalAttributeValues. Fetching next range starting from: ", rangeRetrievalNextIndex));
			newSearcher.AutoRangeRetrieve = false;
			newSearcher.PageSize = originalSearcher.PageSize;
			newSearcher.Scope = originalSearcher.Scope;
			newSearcher.SearchRoot = originalSearcher.SearchRoot;
			newSearcher.SchemaTranslation = originalSearcher.SchemaTranslation;
			newSearcher.ShowDeleted = originalSearcher.ShowDeleted;
			newSearcher.ShowDeactivatedLink = originalSearcher.ShowDeactivatedLink;
			newSearcher.SuppressServerRangeRetrievalError = true;
			List<IADOPathNode> aDOPathNodes = new List<IADOPathNode>();
			foreach (string rangeRetrievedObject in rangeRetrievedObjects)
			{
				aDOPathNodes.Add(ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", rangeRetrievedObject));
			}
			if (aDOPathNodes.Count != 1)
			{
				newSearcher.Filter = ADOPathUtil.CreateOrClause(aDOPathNodes.ToArray());
			}
			else
			{
				newSearcher.Filter = aDOPathNodes[0];
			}
			List<string> strs = new List<string>(rangeRetrievedAttributes.Count);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string rangeRetrievedAttribute in rangeRetrievedAttributes)
			{
				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.Append(rangeRetrievedAttribute).Append(";range=").Append(rangeRetrievalNextIndex).Append("-*");
				strs.Add(stringBuilder.ToString());
			}
			newSearcher.Properties = strs;
			return newSearcher.FindAll();
		}

		~ADObjectSearcher()
		{
			try
			{
				DebugLogger.WriteLine("ADObjectSearcher", "Destructor ADObjectSearcher");
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		public IEnumerable<ADObject> FindAll()
		{
			return new ADObjectSearchResult(this);
		}

		public ADObject FindOne()
		{
			bool flag = false;
			return this.FindOne(out flag);
		}

		public ADObject FindOne(out bool foundMoreThanOneResult)
		{
			foundMoreThanOneResult = false;
			ADObject aDObject = null;
			int num = 0;
			IEnumerable<ADObject> aDObjectSearchResults = new ADObjectSearchResult(this, 2, 2);
			foreach (ADObject aDObjectSearchResult in aDObjectSearchResults)
			{
				if (num <= 0)
				{
					num++;
					aDObject = aDObjectSearchResult;
				}
				else
				{
					foundMoreThanOneResult = true;
					break;
				}
			}
			return aDObject;
		}

		public ADRootDSE GetRootDSE()
		{
			this.Init();
			ADRootDSE rootDSE = this._adSession.RootDSE;
			if (rootDSE == null)
			{
				string[] strArrays = new string[2];
				strArrays[0] = "*";
				strArrays[1] = "msDS-PortLDAP";
				ADSearchRequest aDSearchRequest = new ADSearchRequest("", ADObjectSearcher.DefaultSearchFilter.GetLdapFilterString(), SearchScope.Base, strArrays);
				aDSearchRequest.TimeLimit = this._timeLimit;
				ADSearchResponse aDSearchResponse = this._syncOps.Search(this._sessionHandle, aDSearchRequest);
				if (aDSearchResponse.Entries.Count > 0)
				{
					rootDSE = new ADRootDSE();
					ADObject item = aDSearchResponse.Entries[0];
					foreach (string propertyName in item.PropertyNames)
					{
						rootDSE.Add(propertyName, item[propertyName]);
					}
					this._adSession.RootDSE = rootDSE;
				}
				return rootDSE;
			}
			else
			{
				return rootDSE;
			}
		}

		public ADRootDSE GetRootDSE(ICollection<string> propertyList)
		{
			return this.GetRootDSE(propertyList, false);
		}

		public ADRootDSE GetRootDSE(bool propertyNamesOnly)
		{
			return this.GetRootDSE(null, propertyNamesOnly);
		}

		public ADRootDSE GetRootDSE(ICollection<string> propertyList, bool propertyNamesOnly)
		{
			this.Init();
			ADRootDSE aDRootDSE = null;
			string[] strArrays = null;
			if (propertyList != null)
			{
				strArrays = new string[propertyList.Count];
				propertyList.CopyTo(strArrays, 0);
			}
			ADSearchRequest aDSearchRequest = new ADSearchRequest("", ADObjectSearcher.DefaultSearchFilter.GetLdapFilterString(), SearchScope.Base, strArrays);
			aDSearchRequest.TypesOnly = propertyNamesOnly;
			aDSearchRequest.TimeLimit = this._timeLimit;
			ADSearchResponse aDSearchResponse = this._syncOps.Search(this._sessionHandle, aDSearchRequest);
			if (aDSearchResponse.Entries.Count > 0)
			{
				aDRootDSE = new ADRootDSE();
				ADObject item = aDSearchResponse.Entries[0];
				foreach (string propertyName in item.PropertyNames)
				{
					aDRootDSE.Add(propertyName, item[propertyName]);
				}
			}
			return aDRootDSE;
		}

		private void Init()
		{
			if (!this._disposed)
			{
				if (this._syncOps == null)
				{
					this._sessionHandle = this._adSession.GetSessionHandle();
					this._syncOps = this._adSession.GetSyncOperationsInterface();
					this._typeConverter = new ADTypeConverter(this._adSession.SessionInfo);
				}
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		internal static bool IsDefaultSearchFilter(string filter)
		{
			return string.Compare(filter, ADObjectSearcher.DefaultSearchFilterString, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public List<ADObject> PagedSearch(ref object pageCookie, out bool hasSizeLimitExceeded)
		{
			return this.PagedSearch(ref pageCookie, out hasSizeLimitExceeded, this.PageSize, this.SizeLimit);
		}

		public List<ADObject> PagedSearch(ref object pageCookie, out bool hasSizeLimitExceeded, int pageSize, int sizeLimit)
		{
			hasSizeLimitExceeded = false;
			Dictionary<string, ADObject> strs = null;
			HashSet<string> strs1 = null;
			HashSet<string> strs2 = null;
			int num = -2147483648;
			this.Init();
			if (this._searchRoot != null)
			{
				ADPageResultRequestControl aDPageResultRequestControl = new ADPageResultRequestControl(pageSize);
				if (pageCookie != null)
				{
					aDPageResultRequestControl.Cookie = pageCookie;
				}
				ADSearchRequest aDSearchRequest = this.CreateSearchRequest(aDPageResultRequestControl);
				aDSearchRequest.SizeLimit = sizeLimit;
				ADSearchResponse aDSearchResponse = this._syncOps.Search(this._sessionHandle, aDSearchRequest);
				this.ProcessResponseControls(aDSearchResponse);
				if (aDSearchResponse.ResultCode == ResultCode.SizeLimitExceeded)
				{
					hasSizeLimitExceeded = true;
				}
				List<ADObject> aDObjects = new List<ADObject>(aDSearchResponse.Entries.Count);
				if (this._autoRangeRetrieve)
				{
					strs = new Dictionary<string, ADObject>(StringComparer.OrdinalIgnoreCase);
					strs1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					strs2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				}
				foreach (ADObject entry in aDSearchResponse.Entries)
				{
					ADObject aDObject = this.CreateRichADObject(entry);
					aDObjects.Add(aDObject);
					if (!this._autoRangeRetrieve)
					{
						continue;
					}
					if (!string.IsNullOrEmpty (aDObject.DistinguishedName))
					{
						strs.Add(aDObject.DistinguishedName, aDObject);
						ADObjectSearcher.ExtractRangeRetrievalDataFromAttributes(aDObject, aDObject, strs1, strs2, ref num);
					}
				}
				while (this._autoRangeRetrieve && strs2.Count > 0 && strs1.Count > 0 && num != -2147483648)
				{
					using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(this._adSession.SessionInfo))
					{
						IEnumerable<ADObject> aDObjects1 = ADObjectSearcher.FetchRemainingRangeRetrievalAttributeValues(aDObjectSearcher, this, strs1, strs2, num);
						strs1.Clear();
						strs2.Clear();
						num = -2147483648;
						foreach (ADObject aDObject1 in aDObjects1)
						{
							ADObjectSearcher.ExtractRangeRetrievalDataFromAttributes(aDObject1, strs[aDObject1.DistinguishedName], strs1, strs2, ref num);
						}
					}
				}
				pageCookie = null;
				DirectoryControl[] controls = aDSearchResponse.Controls;
				int num1 = 0;
				while (num1 < (int)controls.Length)
				{
					if (controls[num1] as ADPageResultResponseControl == null)
					{
						num1++;
					}
					else
					{
						pageCookie = ((ADPageResultResponseControl)controls[num1]).Cookie;
						break;
					}
				}
				return aDObjects;
			}
			else
			{
				DebugLogger.LogWarning("ADObjectSearcher", "PagedSearch: SearchRoot is null");
				throw new ArgumentNullException("SearchRoot");
			}
		}

		private void ProcessAsqResponse(ADSearchResponse response)
		{
			if (!string.IsNullOrEmpty(this._attributeScopedQuery))
			{
				DirectoryControl[] controls = response.Controls;
				int num = 0;
				while (num < (int)controls.Length)
				{
					ADAsqResponseControl aDAsqResponseControl = controls[num] as ADAsqResponseControl;
					if (aDAsqResponseControl == null)
					{
						num++;
					}
					else
					{
						if (aDAsqResponseControl.Result == ResultCode.Success)
						{
							break;
						}
						int errorCode = ADStoreAccess.MapResultCodeToErrorCode(aDAsqResponseControl.Result);
						Win32Exception win32Exception = new Win32Exception(errorCode);
						object[] message = new object[3];
						message[0] = this._searchRoot;
						message[1] = this._attributeScopedQuery;
						message[2] = win32Exception.Message;
						string str = string.Format(CultureInfo.CurrentCulture, StringResources.AsqResponseError, message);
						throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, str, null);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void ProcessResponseControls(ADSearchResponse response)
		{
			if (response.Controls != null)
			{
				this.ProcessAsqResponse(response);
				return;
			}
			else
			{
				return;
			}
		}

		private void Uninit()
		{
			if (this._adSession != null)
			{
				this._adSession.Delete();
				this._adSession = null;
			}
		}
	}
}