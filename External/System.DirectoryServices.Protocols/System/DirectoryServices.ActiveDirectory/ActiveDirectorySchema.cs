using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ActiveDirectorySchema : ActiveDirectoryPartition
	{
		private bool disposed;

		private DirectoryEntry schemaEntry;

		private DirectoryEntry abstractSchemaEntry;

		private DirectoryServer cachedSchemaRoleOwner;

		public DirectoryServer SchemaRoleOwner
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedSchemaRoleOwner == null)
				{
					this.cachedSchemaRoleOwner = this.GetSchemaRoleOwner();
				}
				return this.cachedSchemaRoleOwner;
			}
		}

		internal ActiveDirectorySchema(DirectoryContext context, string distinguishedName) : base(context, distinguishedName)
		{
			this.directoryEntryMgr = new DirectoryEntryManager(context);
			this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(context, distinguishedName);
		}

		internal ActiveDirectorySchema(DirectoryContext context, string distinguishedName, DirectoryEntryManager directoryEntryMgr) : base(context, distinguishedName)
		{
			this.directoryEntryMgr = directoryEntryMgr;
			this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(context, distinguishedName);
		}

		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				try
				{
					if (disposing)
					{
						if (this.schemaEntry != null)
						{
							this.schemaEntry.Dispose();
							this.schemaEntry = null;
						}
						if (this.abstractSchemaEntry != null)
						{
							this.abstractSchemaEntry.Dispose();
							this.abstractSchemaEntry = null;
						}
					}
					this.disposed = true;
				}
				finally
				{
					base.Dispose();
				}
			}
		}

		public ReadOnlyActiveDirectorySchemaClassCollection FindAllClasses()
		{
			base.CheckIfDisposed();
			string[] objectCategory = new string[5];
			objectCategory[0] = "(&(";
			objectCategory[1] = PropertyManager.ObjectCategory;
			objectCategory[2] = "=classSchema)(!(";
			objectCategory[3] = PropertyManager.IsDefunct;
			objectCategory[4] = "=TRUE)))";
			string str = string.Concat(objectCategory);
			return ActiveDirectorySchema.GetAllClasses(this.context, this.schemaEntry, str);
		}

		public ReadOnlyActiveDirectorySchemaClassCollection FindAllClasses(SchemaClassType type)
		{
			base.CheckIfDisposed();
			if (type < SchemaClassType.Type88 || type > SchemaClassType.Auxiliary)
			{
				throw new InvalidEnumArgumentException("type", (int)type, typeof(SchemaClassType));
			}
			else
			{
				object[] objectCategory = new object[9];
				objectCategory[0] = "(&(";
				objectCategory[1] = PropertyManager.ObjectCategory;
				objectCategory[2] = "=classSchema)(";
				objectCategory[3] = PropertyManager.ObjectClassCategory;
				objectCategory[4] = "=";
				objectCategory[5] = (int)type;
				objectCategory[6] = ")(!(";
				objectCategory[7] = PropertyManager.IsDefunct;
				objectCategory[8] = "=TRUE)))";
				string str = string.Concat(objectCategory);
				return ActiveDirectorySchema.GetAllClasses(this.context, this.schemaEntry, str);
			}
		}

		public ReadOnlyActiveDirectorySchemaClassCollection FindAllDefunctClasses()
		{
			base.CheckIfDisposed();
			string[] objectCategory = new string[5];
			objectCategory[0] = "(&(";
			objectCategory[1] = PropertyManager.ObjectCategory;
			objectCategory[2] = "=classSchema)(";
			objectCategory[3] = PropertyManager.IsDefunct;
			objectCategory[4] = "=TRUE))";
			string str = string.Concat(objectCategory);
			return ActiveDirectorySchema.GetAllClasses(this.context, this.schemaEntry, str);
		}

		public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllDefunctProperties()
		{
			base.CheckIfDisposed();
			string[] objectCategory = new string[5];
			objectCategory[0] = "(&(";
			objectCategory[1] = PropertyManager.ObjectCategory;
			objectCategory[2] = "=attributeSchema)(";
			objectCategory[3] = PropertyManager.IsDefunct;
			objectCategory[4] = "=TRUE))";
			string str = string.Concat(objectCategory);
			return ActiveDirectorySchema.GetAllProperties(this.context, this.schemaEntry, str);
		}

		public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllProperties()
		{
			base.CheckIfDisposed();
			string[] objectCategory = new string[5];
			objectCategory[0] = "(&(";
			objectCategory[1] = PropertyManager.ObjectCategory;
			objectCategory[2] = "=attributeSchema)(!(";
			objectCategory[3] = PropertyManager.IsDefunct;
			objectCategory[4] = "=TRUE)))";
			string str = string.Concat(objectCategory);
			return ActiveDirectorySchema.GetAllProperties(this.context, this.schemaEntry, str);
		}

		public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllProperties(PropertyTypes type)
		{
			base.CheckIfDisposed();
			if (((int)type & -7) == 0)
			{
				StringBuilder stringBuilder = new StringBuilder(25);
				stringBuilder.Append("(&(");
				stringBuilder.Append(PropertyManager.ObjectCategory);
				stringBuilder.Append("=attributeSchema)");
				stringBuilder.Append("(!(");
				stringBuilder.Append(PropertyManager.IsDefunct);
				stringBuilder.Append("=TRUE))");
				if ((type & PropertyTypes.Indexed) != 0)
				{
					stringBuilder.Append("(");
					stringBuilder.Append(PropertyManager.SearchFlags);
					stringBuilder.Append(":1.2.840.113556.1.4.804:=");
					stringBuilder.Append(1);
					stringBuilder.Append(")");
				}
				if ((type & PropertyTypes.InGlobalCatalog) != 0)
				{
					stringBuilder.Append("(");
					stringBuilder.Append(PropertyManager.IsMemberOfPartialAttributeSet);
					stringBuilder.Append("=TRUE)");
				}
				stringBuilder.Append(")");
				return ActiveDirectorySchema.GetAllProperties(this.context, this.schemaEntry, stringBuilder.ToString());
			}
			else
			{
				throw new ArgumentException(Res.GetString("InvalidFlags"), "type");
			}
		}

		public ActiveDirectorySchemaClass FindClass(string ldapDisplayName)
		{
			base.CheckIfDisposed();
			return ActiveDirectorySchemaClass.FindByName(this.context, ldapDisplayName);
		}

		public ActiveDirectorySchemaClass FindDefunctClass(string commonName)
		{
			base.CheckIfDisposed();
			if (commonName != null)
			{
				if (commonName.Length != 0)
				{
					Hashtable propertiesFromSchemaContainer = ActiveDirectorySchemaClass.GetPropertiesFromSchemaContainer(this.context, this.schemaEntry, commonName, true);
					ActiveDirectorySchemaClass activeDirectorySchemaClass = new ActiveDirectorySchemaClass(this.context, commonName, propertiesFromSchemaContainer, this.schemaEntry);
					return activeDirectorySchemaClass;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "commonName");
				}
			}
			else
			{
				throw new ArgumentNullException("commonName");
			}
		}

		public ActiveDirectorySchemaProperty FindDefunctProperty(string commonName)
		{
			base.CheckIfDisposed();
			if (commonName != null)
			{
				if (commonName.Length != 0)
				{
					SearchResult propertiesFromSchemaContainer = ActiveDirectorySchemaProperty.GetPropertiesFromSchemaContainer(this.context, this.schemaEntry, commonName, true);
					ActiveDirectorySchemaProperty activeDirectorySchemaProperty = new ActiveDirectorySchemaProperty(this.context, commonName, propertiesFromSchemaContainer, this.schemaEntry);
					return activeDirectorySchemaProperty;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "commonName");
				}
			}
			else
			{
				throw new ArgumentNullException("commonName");
			}
		}

		public ActiveDirectorySchemaProperty FindProperty(string ldapDisplayName)
		{
			base.CheckIfDisposed();
			return ActiveDirectorySchemaProperty.FindByName(this.context, ldapDisplayName);
		}

		internal static ReadOnlyActiveDirectorySchemaClassCollection GetAllClasses(DirectoryContext context, DirectoryEntry schemaEntry, string filter)
		{
			ArrayList arrayLists = new ArrayList();
			string[] ldapDisplayName = new string[3];
			ldapDisplayName[0] = PropertyManager.LdapDisplayName;
			ldapDisplayName[1] = PropertyManager.Cn;
			ldapDisplayName[2] = PropertyManager.IsDefunct;
			ADSearcher aDSearcher = new ADSearcher(schemaEntry, filter, ldapDisplayName, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			using (searchResultCollections)
			{
				try
				{
					searchResultCollections = aDSearcher.FindAll();
					foreach (SearchResult searchResult in searchResultCollections)
					{
						string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.LdapDisplayName);
						DirectoryEntry directoryEntry = searchResult.GetDirectoryEntry();
						directoryEntry.AuthenticationType = Utils.DefaultAuthType;
						directoryEntry.Username = context.UserName;
						directoryEntry.Password = context.Password;
						bool item = false;
						if (searchResult.Properties[PropertyManager.IsDefunct] != null && searchResult.Properties[PropertyManager.IsDefunct].Count > 0)
						{
							item = (bool)searchResult.Properties[PropertyManager.IsDefunct][0];
						}
						if (!item)
						{
							arrayLists.Add(new ActiveDirectorySchemaClass(context, searchResultPropertyValue, directoryEntry, schemaEntry));
						}
						else
						{
							string str = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn);
							arrayLists.Add(new ActiveDirectorySchemaClass(context, str, searchResultPropertyValue, directoryEntry, schemaEntry));
						}
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
				}
			}
			return new ReadOnlyActiveDirectorySchemaClassCollection(arrayLists);
		}

		internal static ReadOnlyActiveDirectorySchemaPropertyCollection GetAllProperties(DirectoryContext context, DirectoryEntry schemaEntry, string filter)
		{
			ArrayList arrayLists = new ArrayList();
			string[] ldapDisplayName = new string[3];
			ldapDisplayName[0] = PropertyManager.LdapDisplayName;
			ldapDisplayName[1] = PropertyManager.Cn;
			ldapDisplayName[2] = PropertyManager.IsDefunct;
			ADSearcher aDSearcher = new ADSearcher(schemaEntry, filter, ldapDisplayName, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			using (searchResultCollections)
			{
				try
				{
					searchResultCollections = aDSearcher.FindAll();
					foreach (SearchResult searchResult in searchResultCollections)
					{
						string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.LdapDisplayName);
						DirectoryEntry directoryEntry = searchResult.GetDirectoryEntry();
						directoryEntry.AuthenticationType = Utils.DefaultAuthType;
						directoryEntry.Username = context.UserName;
						directoryEntry.Password = context.Password;
						bool item = false;
						if (searchResult.Properties[PropertyManager.IsDefunct] != null && searchResult.Properties[PropertyManager.IsDefunct].Count > 0)
						{
							item = (bool)searchResult.Properties[PropertyManager.IsDefunct][0];
						}
						if (!item)
						{
							arrayLists.Add(new ActiveDirectorySchemaProperty(context, searchResultPropertyValue, directoryEntry, schemaEntry));
						}
						else
						{
							string str = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn);
							arrayLists.Add(new ActiveDirectorySchemaProperty(context, str, searchResultPropertyValue, directoryEntry, schemaEntry));
						}
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
				}
			}
			return new ReadOnlyActiveDirectorySchemaPropertyCollection(arrayLists);
		}

		public static ActiveDirectorySchema GetCurrentSchema()
		{
			return ActiveDirectorySchema.GetSchema(new DirectoryContext(DirectoryContextType.Forest));
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override DirectoryEntry GetDirectoryEntry()
		{
			base.CheckIfDisposed();
			return DirectoryEntryManager.GetDirectoryEntry(this.context, base.Name);
		}

		public static ActiveDirectorySchema GetSchema(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest || context.ContextType == DirectoryContextType.ConfigurationSet || context.ContextType == DirectoryContextType.DirectoryServer)
				{
					if (context.Name != null || context.isRootDomain())
					{
						if (context.Name == null || context.isRootDomain() || context.isADAMConfigSet() || context.isServer())
						{
							context = new DirectoryContext(context);
							DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(context);
							string propertyValue = null;
							try
							{
								DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
								if (!context.isServer() || Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectoryOrADAM))
								{
									propertyValue = (string)PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.SchemaNamingContext);
								}
								else
								{
									object[] name = new object[1];
									name[0] = context.Name;
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ServerNotFound", name), typeof(ActiveDirectorySchema), null);
								}
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								int errorCode = cOMException.ErrorCode;
								if (errorCode != -2147016646)
								{
									throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
								}
								else
								{
									if (context.ContextType != DirectoryContextType.Forest)
									{
										if (context.ContextType != DirectoryContextType.ConfigurationSet)
										{
											object[] objArray = new object[1];
											objArray[0] = context.Name;
											throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ServerNotFound", objArray), typeof(ActiveDirectorySchema), null);
										}
										else
										{
											throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ActiveDirectorySchema), context.Name);
										}
									}
									else
									{
										throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(ActiveDirectorySchema), context.Name);
									}
								}
							}
							catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
							{
								if (context.ContextType != DirectoryContextType.ConfigurationSet)
								{
									throw;
								}
								else
								{
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ActiveDirectorySchema), context.Name);
								}
							}
							return new ActiveDirectorySchema(context, propertyValue, directoryEntryManager);
						}
						else
						{
							if (context.ContextType != DirectoryContextType.Forest)
							{
								if (context.ContextType != DirectoryContextType.ConfigurationSet)
								{
									object[] name1 = new object[1];
									name1[0] = context.Name;
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ServerNotFound", name1), typeof(ActiveDirectorySchema), null);
								}
								else
								{
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ActiveDirectorySchema), context.Name);
								}
							}
							else
							{
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(ActiveDirectorySchema), context.Name);
							}
						}
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(ActiveDirectorySchema), null);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		private DirectoryServer GetSchemaRoleOwner()
		{
			DirectoryServer adamInstance;
			DirectoryServer directoryServer;
			try
			{
				this.schemaEntry.RefreshCache();
				if (!this.context.isADAMConfigSet())
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
					if (!Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectory))
					{
						string adamDnsHostNameFromNTDSA = Utils.GetAdamDnsHostNameFromNTDSA(this.context, (string)PropertyManager.GetPropertyValue(this.context, this.schemaEntry, PropertyManager.FsmoRoleOwner));
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(adamDnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, this.context);
						adamInstance = new AdamInstance(newDirectoryContext, adamDnsHostNameFromNTDSA);
					}
					else
					{
						string dnsHostNameFromNTDSA = Utils.GetDnsHostNameFromNTDSA(this.context, (string)PropertyManager.GetPropertyValue(this.context, this.schemaEntry, PropertyManager.FsmoRoleOwner));
						DirectoryContext directoryContext = Utils.GetNewDirectoryContext(dnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, this.context);
						adamInstance = new DomainController(directoryContext, dnsHostNameFromNTDSA);
					}
					directoryServer = adamInstance;
				}
				else
				{
					string str = Utils.GetAdamDnsHostNameFromNTDSA(this.context, (string)PropertyManager.GetPropertyValue(this.context, this.schemaEntry, PropertyManager.FsmoRoleOwner));
					DirectoryContext newDirectoryContext1 = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, this.context);
					directoryServer = new AdamInstance(newDirectoryContext1, str);
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			return directoryServer;
		}

		public void RefreshSchema()
		{
			base.CheckIfDisposed();
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
					directoryEntry.Properties[PropertyManager.SchemaUpdateNow].Value = 1;
					directoryEntry.CommitChanges();
					if (this.abstractSchemaEntry == null)
					{
						this.abstractSchemaEntry = this.directoryEntryMgr.GetCachedDirectoryEntry("Schema");
					}
					this.abstractSchemaEntry.RefreshCache();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
		}
	}
}