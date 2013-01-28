using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DebuggerDisplay("Name ( {Name} )")]
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public abstract class Principal : IDisposable
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string description;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState descriptionChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string displayName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState displayNameChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string samName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState samNameChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string userPrincipalName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState userPrincipalNameChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private SecurityIdentifier sid;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState sidChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Guid? guid;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState guidChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string distinguishedName;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState distinguishedNameChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string structuralObjectClass;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState structuralObjectClassChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string name;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LoadState nameChanged;

		private ExtensionHelper extensionHelper;

		private ExtensionCache extensionCache;

		private LoadState extensionCacheChanged;

		internal bool unpersisted;

		private bool isDeleted;

		private bool loaded;

		internal bool fakePrincipal;

		[SecuritySafeCritical]
		private PrincipalContext ctx;

		private object underlyingObject;

		private object underlyingSearchObject;

		private object discriminant;

		private StoreKey key;

		private bool disposed;

		public PrincipalContext Context
		{
			[SecuritySafeCritical]
			get
			{
				this.CheckDisposedOrDeleted();
				return this.ctx;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal PrincipalContext ContextRaw
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.ctx;
			}
			[SecurityCritical]
			set
			{
				if (value != null)
				{
					value.CheckDisposed();
				}
				this.ctx = value;
			}
		}

		public ContextType ContextType
		{
			[SecurityCritical]
			get
			{
				this.CheckDisposedOrDeleted();
				if (this.ctx != null)
				{
					return this.ctx.ContextType;
				}
				else
				{
					throw new InvalidOperationException(StringResources.PrincipalMustSetContextForProperty);
				}
			}
		}

		public string Description
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<string>(ref this.description, "Principal.Description", ref this.descriptionChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.GetStoreCtxToUse().IsValidProperty(this, "Principal.Description"))
				{
					this.HandleSet<string>(ref this.description, value, ref this.descriptionChanged, "Principal.Description");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		internal object Discriminant
		{
			get
			{
				return this.discriminant;
			}
			set
			{
				this.discriminant = value;
			}
		}

		public string DisplayName
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<string>(ref this.displayName, "Principal.DisplayName", ref this.displayNameChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.GetStoreCtxToUse().IsValidProperty(this, "Principal.DisplayName"))
				{
					this.HandleSet<string>(ref this.displayName, value, ref this.displayNameChanged, "Principal.DisplayName");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		public string DistinguishedName
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<string>(ref this.distinguishedName, "Principal.DistinguishedName", ref this.distinguishedNameChanged);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal ExtensionHelper ExtensionHelper
		{
			get
			{
				if (this.extensionHelper == null)
				{
					this.extensionHelper = new ExtensionHelper(this);
				}
				return this.extensionHelper;
			}
		}

		public Guid? Guid
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<Guid?>(ref this.guid, "Principal.Guid", ref this.guidChanged);
			}
		}

		internal StoreKey Key
		{
			get
			{
				return this.key;
			}
			set
			{
				this.key = value;
			}
		}

		internal bool Loaded
		{
			get
			{
				return this.loaded;
			}
			set
			{
				this.loaded = value;
			}
		}

		public string Name
		{
			[SecurityCritical]
			get
			{
				ContextType contextType;
				if (this.ctx == null)
				{
					contextType = ContextType.Domain;
				}
				else
				{
					contextType = this.ctx.ContextType;
				}
				ContextType contextType1 = contextType;
				if (contextType1 != ContextType.Machine)
				{
					return this.HandleGet<string>(ref this.name, "Principal.Name", ref this.nameChanged);
				}
				else
				{
					return this.HandleGet<string>(ref this.samName, "Principal.SamAccountName", ref this.samNameChanged);
				}
			}
			[SecurityCritical]
			set
			{
				ContextType contextType;
				if (value == null || value.Length == 0)
				{
					object[] objArray = new object[1];
					objArray[0] = "Principal.Name";
					throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullArgument, objArray));
				}
				else
				{
					if (this.GetStoreCtxToUse().IsValidProperty(this, "Principal.Name"))
					{
						if (this.ctx == null)
						{
							contextType = ContextType.Domain;
						}
						else
						{
							contextType = this.ctx.ContextType;
						}
						ContextType contextType1 = contextType;
						if (contextType1 != ContextType.Machine)
						{
							this.HandleSet<string>(ref this.name, value, ref this.nameChanged, "Principal.Name");
							return;
						}
						else
						{
							this.HandleSet<string>(ref this.samName, value, ref this.samNameChanged, "Principal.SamAccountName");
							return;
						}
					}
					else
					{
						throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
					}
				}
			}
		}

		public string SamAccountName
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<string>(ref this.samName, "Principal.SamAccountName", ref this.samNameChanged);
			}
			[SecurityCritical]
			set
			{
				if (value == null || value.Length == 0)
				{
					object[] objArray = new object[1];
					objArray[0] = "Principal.SamAccountName";
					throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidNullArgument, objArray));
				}
				else
				{
					if (this.GetStoreCtxToUse().IsValidProperty(this, "Principal.SamAccountName"))
					{
						this.HandleSet<string>(ref this.samName, value, ref this.samNameChanged, "Principal.SamAccountName");
						return;
					}
					else
					{
						throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
					}
				}
			}
		}

		public SecurityIdentifier Sid
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<SecurityIdentifier>(ref this.sid, "Principal.Sid", ref this.sidChanged);
			}
		}

		public string StructuralObjectClass
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<string>(ref this.structuralObjectClass, "Principal.StructuralObjectClass", ref this.structuralObjectClassChanged);
			}
		}

		internal object UnderlyingObject
		{
			get
			{
				if (this.underlyingObject == null)
				{
					return null;
				}
				else
				{
					return this.underlyingObject;
				}
			}
			set
			{
				this.underlyingObject = value;
			}
		}

		internal object UnderlyingSearchObject
		{
			get
			{
				if (this.underlyingSearchObject == null)
				{
					return null;
				}
				else
				{
					return this.underlyingSearchObject;
				}
			}
			set
			{
				this.underlyingSearchObject = value;
			}
		}

		public string UserPrincipalName
		{
			[SecurityCritical]
			get
			{
				return this.HandleGet<string>(ref this.userPrincipalName, "Principal.UserPrincipalName", ref this.userPrincipalNameChanged);
			}
			[SecurityCritical]
			set
			{
				if (this.GetStoreCtxToUse().IsValidProperty(this, "Principal.UserPrincipalName"))
				{
					this.HandleSet<string>(ref this.userPrincipalName, value, ref this.userPrincipalNameChanged, "Principal.UserPrincipalName");
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.InvalidPropertyForStore);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected Principal()
		{
			this.guid = null;
			this.extensionCache = new ExtensionCache();
		}

		[SecurityCritical]
		internal void AdvancedFilterSet(string attribute, object value, Type objectType, MatchType mt)
		{
			if (attribute != null)
			{
				this.ValidateExtensionObject(value);
				if (value as object[] == null)
				{
					object[] objArray = new object[1];
					objArray[0] = value;
					this.extensionCache.properties[attribute] = new ExtensionCacheValue(objArray, objectType, mt);
				}
				else
				{
					this.extensionCache.properties[attribute] = new ExtensionCacheValue((object[])value, objectType, mt);
				}
				this.extensionCacheChanged = LoadState.Changed;
				return;
			}
			else
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[SecuritySafeCritical]
		protected void CheckDisposedOrDeleted()
		{
			if (!this.disposed)
			{
				if (!this.isDeleted)
				{
					return;
				}
				else
				{
					throw new InvalidOperationException(StringResources.PrincipalDeleted);
				}
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().ToString());
			}
		}

		internal void CheckFakePrincipal()
		{
			if (!this.fakePrincipal)
			{
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalNotSupportedOnFakePrincipal);
			}
		}

		[SecurityCritical]
		public void Delete()
		{
			this.CheckDisposedOrDeleted();
			this.CheckFakePrincipal();
			if (!this.unpersisted)
			{
				this.ctx.QueryCtx.Delete(this);
				this.isDeleted = true;
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalCantDeleteUnpersisted);
			}
		}

		[SecurityCritical]
		public virtual void Dispose()
		{
			if (!this.disposed)
			{
				if (this.UnderlyingObject != null && this.UnderlyingObject as IDisposable != null)
				{
					((IDisposable)this.UnderlyingObject).Dispose();
				}
				if (this.UnderlyingSearchObject != null && this.UnderlyingSearchObject as IDisposable != null)
				{
					((IDisposable)this.UnderlyingSearchObject).Dispose();
				}
				this.disposed = true;
				GC.SuppressFinalize(this);
			}
		}

		public override bool Equals(object o)
		{
			Principal principal = o as Principal;
			if (principal != null)
			{
				if (!object.ReferenceEquals(this, principal))
				{
					if (this.key == null || principal.key == null || !this.key.Equals(principal.key))
					{
						return false;
					}
					else
					{
						return true;
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

		[SecurityCritical]
		protected object[] ExtensionGet(string attribute)
		{
			ExtensionCacheValue extensionCacheValue = null;
			if (attribute != null)
			{
				if (!this.extensionCache.TryGetValue(attribute, out extensionCacheValue))
				{
					if (!this.unpersisted)
					{
						DirectoryEntry underlyingObject = (DirectoryEntry)this.GetUnderlyingObject();
						int count = underlyingObject.Properties[attribute].Count;
						if (count != 0)
						{
							object[] objArray = new object[count];
							underlyingObject.Properties[attribute].CopyTo(objArray, 0);
							return objArray;
						}
						else
						{
							return new object[0];
						}
					}
					else
					{
						return new object[0];
					}
				}
				else
				{
					if (!extensionCacheValue.Filter)
					{
						return extensionCacheValue.Value;
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
		}

		[SecurityCritical]
		protected void ExtensionSet(string attribute, object value)
		{
			if (attribute != null)
			{
				this.ValidateExtensionObject(value);
				if (value as object[] == null)
				{
					object[] objArray = new object[1];
					objArray[0] = value;
					this.extensionCache.properties[attribute] = new ExtensionCacheValue(objArray);
				}
				else
				{
					this.extensionCache.properties[attribute] = new ExtensionCacheValue((object[])value);
				}
				this.extensionCacheChanged = LoadState.Changed;
				return;
			}
			else
			{
				throw new ArgumentException(StringResources.NullArguments);
			}
		}

		[SecurityCritical]
		public static Principal FindByIdentity(PrincipalContext context, string identityValue)
		{
			return Principal.FindByIdentityWithType(context, typeof(Principal), identityValue);
		}

		[SecurityCritical]
		public static Principal FindByIdentity(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			return Principal.FindByIdentityWithType(context, typeof(Principal), identityType, identityValue);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[SecurityCritical]
		protected static Principal FindByIdentityWithType(PrincipalContext context, Type principalType, string identityValue)
		{
			if (context != null)
			{
				if (identityValue != null)
				{
					IdentityType? nullable = null;
					return Principal.FindByIdentityWithTypeHelper(context, principalType, nullable, identityValue, DateTime.UtcNow);
				}
				else
				{
					throw new ArgumentNullException("identityValue");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[SecurityCritical]
		protected static Principal FindByIdentityWithType(PrincipalContext context, Type principalType, IdentityType identityType, string identityValue)
		{
			if (context != null)
			{
				if (identityValue != null)
				{
					if (identityType < IdentityType.SamAccountName || identityType > IdentityType.Guid)
					{
						throw new InvalidEnumArgumentException("identityType", identityType, typeof(IdentityType));
					}
					else
					{
						return Principal.FindByIdentityWithTypeHelper(context, principalType, new IdentityType?(identityType), identityValue, DateTime.UtcNow);
					}
				}
				else
				{
					throw new ArgumentNullException("identityValue");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		[SecurityCritical]
		private static Principal FindByIdentityWithTypeHelper(PrincipalContext context, Type principalType, IdentityType? identityType, string identityValue, DateTime refDate)
		{
			string stringMap;
			StoreCtx queryCtx = context.QueryCtx;
			Type type = principalType;
			if (!identityType.HasValue)
			{
				stringMap = null;
			}
			else
			{
				stringMap = (string)IdentMap.StringMap[identityType.Value, 1];
			}
			Principal principal = queryCtx.FindPrincipalByIdentRef(type, stringMap, identityValue, refDate);
			if (principal == null)
			{
				return null;
			}
			else
			{
				return principal;
			}
		}

		[SecuritySafeCritical]
		internal virtual bool GetChangeStatusForProperty(string propertyName)
		{
			LoadState loadState;
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "Principal.DisplayName")
				{
					loadState = this.displayNameChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.Description")
				{
					loadState = this.descriptionChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.SamAccountName")
				{
					loadState = this.samNameChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.UserPrincipalName")
				{
					loadState = this.userPrincipalNameChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.Sid")
				{
					loadState = this.sidChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.Guid")
				{
					loadState = this.guidChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.DistinguishedName")
				{
					loadState = this.distinguishedNameChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.StructuralObjectClass")
				{
					loadState = this.structuralObjectClassChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.Name")
				{
					loadState = this.nameChanged;
					return loadState == LoadState.Changed;
				}
				else if (str1 == "Principal.ExtensionCache")
				{
					loadState = this.extensionCacheChanged;
					return loadState == LoadState.Changed;
				}
				loadState = LoadState.NotSet;
				return loadState == LoadState.Changed;
			}
			else
			{
				loadState = LoadState.NotSet;
				return loadState == LoadState.Changed;
			}
			return loadState == LoadState.Changed;
		}

		[SecurityCritical]
		public PrincipalSearchResult<Principal> GetGroups()
		{
			return new PrincipalSearchResult<Principal>(this.GetGroupsHelper());
		}

		[SecurityCritical]
		public PrincipalSearchResult<Principal> GetGroups(PrincipalContext contextToQuery)
		{
			if (contextToQuery != null)
			{
				return new PrincipalSearchResult<Principal>(this.GetGroupsHelper(contextToQuery));
			}
			else
			{
				throw new ArgumentNullException("contextToQuery");
			}
		}

		[SecuritySafeCritical]
		private ResultSet GetGroupsHelper()
		{
			this.CheckDisposedOrDeleted();
			if (!this.unpersisted)
			{
				StoreCtx storeCtxToUse = this.GetStoreCtxToUse();
				ResultSet groupsMemberOf = storeCtxToUse.GetGroupsMemberOf(this);
				return groupsMemberOf;
			}
			else
			{
				return new EmptySet();
			}
		}

		[SecuritySafeCritical]
		private ResultSet GetGroupsHelper(PrincipalContext contextToQuery)
		{
			this.CheckDisposedOrDeleted();
			if (this.ctx != null)
			{
				StoreCtx storeCtxToUse = this.GetStoreCtxToUse();
				return contextToQuery.QueryCtx.GetGroupsMemberOf(this, storeCtxToUse);
			}
			else
			{
				throw new InvalidOperationException(StringResources.UserMustSetContextForMethod);
			}
		}

		public override int GetHashCode()
		{
			return this.GetHashCode();
		}

		[SecuritySafeCritical]
		internal StoreCtx GetStoreCtxToUse()
		{
			if (this.ctx != null)
			{
				if (!this.unpersisted)
				{
					return this.ctx.QueryCtx;
				}
				else
				{
					return this.ctx.ContextForType(this.GetType());
				}
			}
			else
			{
				return null;
			}
		}

		[SecurityCritical]
		public object GetUnderlyingObject()
		{
			this.CheckDisposedOrDeleted();
			this.CheckFakePrincipal();
			if (this.UnderlyingObject != null)
			{
				return this.UnderlyingObject;
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalMustPersistFirst);
			}
		}

		[SecurityCritical]
		public Type GetUnderlyingObjectType()
		{
			this.CheckDisposedOrDeleted();
			this.CheckFakePrincipal();
			if (!this.unpersisted)
			{
				return this.ctx.QueryCtx.NativeType(this);
			}
			else
			{
				if (this.ctx == null)
				{
					throw new InvalidOperationException(StringResources.PrincipalMustSetContextForNative);
				}
				else
				{
					return this.ctx.ContextForType(this.GetType()).NativeType(this);
				}
			}
		}

		internal virtual object GetValueForProperty(string propertyName)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "Principal.DisplayName":
					{
						return this.displayName;
					}
					case "Principal.Description":
					{
						return this.description;
					}
					case "Principal.SamAccountName":
					{
						return this.samName;
					}
					case "Principal.UserPrincipalName":
					{
						return this.userPrincipalName;
					}
					case "Principal.Sid":
					{
						return this.sid;
					}
					case "Principal.Guid":
					{
						return this.guid;
					}
					case "Principal.DistinguishedName":
					{
						return this.distinguishedName;
					}
					case "Principal.StructuralObjectClass":
					{
						return this.structuralObjectClass;
					}
					case "Principal.Name":
					{
						return this.name;
					}
					case "Principal.ExtensionCache":
					{
						return this.extensionCache;
					}
				}
			}
			return null;
		}

		[SecurityCritical]
		internal T HandleGet<T>(ref T currentValue, string name, ref LoadState state)
		{
			this.CheckDisposedOrDeleted();
			if ((int)state == 0)
			{
				this.LoadIfNeeded(name);
				state = LoadState.Loaded;
			}
			return currentValue;
		}

		[SecurityCritical]
		internal void HandleSet<T>(ref T currentValue, T newValue, ref LoadState state, string name)
		{
			this.CheckDisposedOrDeleted();
			currentValue = newValue;
			state = LoadState.Changed;
		}

		[SecurityCritical]
		public bool IsMemberOf(GroupPrincipal group)
		{
			this.CheckDisposedOrDeleted();
			if (group != null)
			{
				return group.Members.Contains(this);
			}
			else
			{
				throw new ArgumentNullException("group");
			}
		}

		[SecurityCritical]
		public bool IsMemberOf(PrincipalContext context, IdentityType identityType, string identityValue)
		{
			this.CheckDisposedOrDeleted();
			if (context != null)
			{
				if (identityValue != null)
				{
					GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(context, identityType, identityValue);
					if (groupPrincipal == null)
					{
						throw new NoMatchingPrincipalException(StringResources.NoMatchingGroupExceptionText);
					}
					else
					{
						return this.IsMemberOf(groupPrincipal);
					}
				}
				else
				{
					throw new ArgumentNullException("identityValue");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		[SecurityCritical]
		internal void LoadIfNeeded(string principalPropertyName)
		{
			if (!this.fakePrincipal)
			{
				if (!this.unpersisted)
				{
					this.ctx.QueryCtx.Load(this, principalPropertyName);
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal virtual void LoadValueIntoProperty(string propertyName, object value)
		{
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "Principal.DisplayName")
				{
					this.displayName = (string)value;
					this.displayNameChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.Description")
				{
					this.description = (string)value;
					this.descriptionChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.SamAccountName")
				{
					this.samName = (string)value;
					this.samNameChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.UserPrincipalName")
				{
					this.userPrincipalName = (string)value;
					this.userPrincipalNameChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.Sid")
				{
					SecurityIdentifier securityIdentifier = (SecurityIdentifier)value;
					this.sid = securityIdentifier;
					this.sidChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.Guid")
				{
					Guid guid = (Guid)value;
					this.guid = new Guid?(guid);
					this.guidChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.DistinguishedName")
				{
					this.distinguishedName = (string)value;
					this.distinguishedNameChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.StructuralObjectClass")
				{
					this.structuralObjectClass = (string)value;
					this.structuralObjectClassChanged = LoadState.Loaded;
					return;
				}
				else if (str1 == "Principal.Name")
				{
					this.name = (string)value;
					this.nameChanged = LoadState.Loaded;
					return;
				}
				return;
			}
		}

		internal static Principal MakePrincipal(PrincipalContext ctx, Type principalType)
		{
			Type[] typeArray = new Type[1];
			typeArray[0] = typeof(PrincipalContext);
			ConstructorInfo constructor = principalType.GetConstructor(typeArray);
			if (null != constructor)
			{
				object[] objArray = new object[1];
				objArray[0] = ctx;
				Principal principal = (Principal)constructor.Invoke(objArray);
				if (principal != null)
				{
					principal.unpersisted = false;
					return principal;
				}
				else
				{
					throw new NotSupportedException(StringResources.ExtensionInvalidClassDefinitionConstructor);
				}
			}
			else
			{
				throw new NotSupportedException(StringResources.ExtensionInvalidClassDefinitionConstructor);
			}
		}

		internal virtual void ResetAllChangeStatus()
		{
			LoadState loadState;
			LoadState loadState1;
			LoadState loadState2;
			LoadState loadState3;
			LoadState loadState4;
			LoadState loadState5;
			LoadState loadState6;
			LoadState loadState7;
			LoadState loadState8;
			Principal principal = this;
			if (this.displayNameChanged == LoadState.Changed)
			{
				loadState = LoadState.Loaded;
			}
			else
			{
				loadState = LoadState.NotSet;
			}
			principal.displayNameChanged = loadState;
			Principal principal1 = this;
			if (this.descriptionChanged == LoadState.Changed)
			{
				loadState1 = LoadState.Loaded;
			}
			else
			{
				loadState1 = LoadState.NotSet;
			}
			principal1.descriptionChanged = loadState1;
			Principal principal2 = this;
			if (this.samNameChanged == LoadState.Changed)
			{
				loadState2 = LoadState.Loaded;
			}
			else
			{
				loadState2 = LoadState.NotSet;
			}
			principal2.samNameChanged = loadState2;
			Principal principal3 = this;
			if (this.userPrincipalNameChanged == LoadState.Changed)
			{
				loadState3 = LoadState.Loaded;
			}
			else
			{
				loadState3 = LoadState.NotSet;
			}
			principal3.userPrincipalNameChanged = loadState3;
			Principal principal4 = this;
			if (this.sidChanged == LoadState.Changed)
			{
				loadState4 = LoadState.Loaded;
			}
			else
			{
				loadState4 = LoadState.NotSet;
			}
			principal4.sidChanged = loadState4;
			Principal principal5 = this;
			if (this.guidChanged == LoadState.Changed)
			{
				loadState5 = LoadState.Loaded;
			}
			else
			{
				loadState5 = LoadState.NotSet;
			}
			principal5.guidChanged = loadState5;
			Principal principal6 = this;
			if (this.distinguishedNameChanged == LoadState.Changed)
			{
				loadState6 = LoadState.Loaded;
			}
			else
			{
				loadState6 = LoadState.NotSet;
			}
			principal6.distinguishedNameChanged = loadState6;
			Principal principal7 = this;
			if (this.nameChanged == LoadState.Changed)
			{
				loadState7 = LoadState.Loaded;
			}
			else
			{
				loadState7 = LoadState.NotSet;
			}
			principal7.nameChanged = loadState7;
			Principal principal8 = this;
			if (this.extensionCacheChanged == LoadState.Changed)
			{
				loadState8 = LoadState.Loaded;
			}
			else
			{
				loadState8 = LoadState.NotSet;
			}
			principal8.extensionCacheChanged = loadState8;
		}

		[SecurityCritical]
		public void Save()
		{
			this.CheckDisposedOrDeleted();
			this.CheckFakePrincipal();
			if (this.ctx != null)
			{
				StoreCtx storeCtxToUse = this.GetStoreCtxToUse();
				if (!this.unpersisted)
				{
					storeCtxToUse.Update(this);
					return;
				}
				else
				{
					storeCtxToUse.Insert(this);
					this.unpersisted = false;
					return;
				}
			}
			else
			{
				throw new InvalidOperationException(StringResources.PrincipalMustSetContextForSave);
			}
		}

		[SecurityCritical]
		public void Save(PrincipalContext context)
		{
			this.CheckDisposedOrDeleted();
			this.CheckFakePrincipal();
			if (context.ContextType == ContextType.Machine || this.ctx.ContextType == ContextType.Machine)
			{
				throw new InvalidOperationException(StringResources.SaveToNotSupportedAgainstMachineStore);
			}
			else
			{
				if (context != null)
				{
					if (context != this.ctx)
					{
						if (context.ContextType == this.ctx.ContextType)
						{
							StoreCtx storeCtxToUse = this.GetStoreCtxToUse();
							this.ctx = context;
							StoreCtx storeCtx = this.GetStoreCtxToUse();
							if (!this.unpersisted)
							{
								this.unpersisted = true;
								bool flag = this.nameChanged == LoadState.Changed;
								string str = null;
								if (flag)
								{
									string str1 = this.name;
									this.ctx.QueryCtx.Load(this, "Principal.Name");
									str = this.name;
									this.Name = str1;
								}
								storeCtx.Move(storeCtxToUse, this);
								try
								{
									this.unpersisted = false;
									storeCtx.Update(this);
								}
								catch (SystemException systemException2)
								{
									SystemException systemException = systemException2;
									try
									{
										if (flag)
										{
											this.Name = str;
										}
										storeCtxToUse.Move(storeCtx, this);
									}
									catch (SystemException systemException1)
									{
									}
									if (systemException as COMException == null)
									{
										throw systemException;
									}
									else
									{
										throw ExceptionHelper.GetExceptionFromCOMException((COMException)systemException);
									}
								}
							}
							else
							{
								storeCtx.Insert(this);
								this.unpersisted = false;
							}
							this.ctx.QueryCtx = storeCtx;
							return;
						}
						else
						{
							throw new InvalidOperationException(StringResources.SaveToMustHaveSamecontextType);
						}
					}
					else
					{
						this.Save();
						return;
					}
				}
				else
				{
					throw new InvalidOperationException(StringResources.NullArguments);
				}
			}
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Name;
		}

		private void ValidateExtensionObject(object value)
		{
			if (value as object[] != null)
			{
				if ((int)((object[])value).Length != 0)
				{
					object[] objArray = (object[])value;
					int num = 0;
					while (num < (int)objArray.Length)
					{
						object obj = objArray[num];
						if (obj as ICollection == null)
						{
							num++;
						}
						else
						{
							throw new ArgumentException(StringResources.InvalidExtensionCollectionType);
						}
					}
				}
				else
				{
					throw new ArgumentException(StringResources.InvalidExtensionCollectionType);
				}
			}
			if (value as byte[] == null)
			{
				if (value != null && value as ICollection != null)
				{
					ICollection collections = (ICollection)value;
					if (collections.Count != 0)
					{
						foreach (object obj1 in collections)
						{
							if (obj1 as ICollection == null)
							{
								continue;
							}
							throw new ArgumentException(StringResources.InvalidExtensionCollectionType);
						}
					}
					else
					{
						throw new ArgumentException(StringResources.InvalidExtensionCollectionType);
					}
				}
			}
			else
			{
				if ((int)((byte[])value).Length == 0)
				{
					throw new ArgumentException(StringResources.InvalidExtensionCollectionType);
				}
			}
		}
	}
}