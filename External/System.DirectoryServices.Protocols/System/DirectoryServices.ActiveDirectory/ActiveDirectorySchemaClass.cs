using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ActiveDirectorySchemaClass : IDisposable
	{
		private DirectoryEntry classEntry;

		private DirectoryEntry schemaEntry;

		private DirectoryEntry abstractClassEntry;

		private NativeComInterfaces.IAdsClass iadsClass;

		private DirectoryContext context;

		internal bool isBound;

		private bool disposed;

		private ActiveDirectorySchema schema;

		private bool propertiesFromSchemaContainerInitialized;

		private bool isDefunctOnServer;

		private Hashtable propertyValuesFromServer;

		private string ldapDisplayName;

		private string commonName;

		private string oid;

		private string description;

		private bool descriptionInitialized;

		private bool isDefunct;

		private ActiveDirectorySchemaClassCollection possibleSuperiors;

		private ActiveDirectorySchemaClassCollection auxiliaryClasses;

		private ReadOnlyActiveDirectorySchemaClassCollection possibleInferiors;

		private ActiveDirectorySchemaPropertyCollection mandatoryProperties;

		private ActiveDirectorySchemaPropertyCollection optionalProperties;

		private ActiveDirectorySchemaClass subClassOf;

		private SchemaClassType type;

		private bool typeInitialized;

		private byte[] schemaGuidBinaryForm;

		private string defaultSDSddlForm;

		private bool defaultSDSddlFormInitialized;

		public ActiveDirectorySchemaClassCollection AuxiliaryClasses
		{
			get
			{
				this.CheckIfDisposed();
				if (this.auxiliaryClasses == null)
				{
					if (!this.isBound)
					{
						this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, false, PropertyManager.AuxiliaryClass, new ArrayList());
					}
					else
					{
						if (this.isDefunctOnServer)
						{
							string[] auxiliaryClass = new string[2];
							auxiliaryClass[0] = PropertyManager.AuxiliaryClass;
							auxiliaryClass[1] = PropertyManager.SystemAuxiliaryClass;
							this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.AuxiliaryClass, this.GetClasses(this.GetPropertyValuesRecursively(auxiliaryClass)));
						}
						else
						{
							ArrayList arrayLists = new ArrayList();
							bool flag = false;
							object auxDerivedFrom = null;
							try
							{
								auxDerivedFrom = this.iadsClass.AuxDerivedFrom;
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								if (cOMException.ErrorCode != -2147463155)
								{
									throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
								}
								else
								{
									flag = true;
								}
							}
							if (flag)
							{
								this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.AuxiliaryClass, new ArrayList());
							}
							else
							{
								if (auxDerivedFrom as ICollection == null)
								{
									arrayLists.Add((string)auxDerivedFrom);
								}
								else
								{
									arrayLists.AddRange((ICollection)auxDerivedFrom);
								}
								this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.AuxiliaryClass, arrayLists, true);
							}
						}
					}
				}
				return this.auxiliaryClasses;
			}
		}

		public string CommonName
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && this.commonName == null)
				{
					this.commonName = (string)this.GetValueFromCache(PropertyManager.Cn, true);
				}
				return this.commonName;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.SetProperty(PropertyManager.Cn, value);
				}
				this.commonName = value;
			}
		}

		public ActiveDirectorySecurity DefaultObjectSecurityDescriptor
		{
			get
			{
				this.CheckIfDisposed();
				ActiveDirectorySecurity activeDirectorySecurity = null;
				if (this.isBound && !this.defaultSDSddlFormInitialized)
				{
					this.defaultSDSddlForm = (string)this.GetValueFromCache(PropertyManager.DefaultSecurityDescriptor, false);
					this.defaultSDSddlFormInitialized = true;
				}
				if (this.defaultSDSddlForm != null)
				{
					activeDirectorySecurity = new ActiveDirectorySecurity();
					activeDirectorySecurity.SetSecurityDescriptorSddlForm(this.defaultSDSddlForm);
				}
				return activeDirectorySecurity;
			}
			set
			{
				string securityDescriptorSddlForm;
				object obj;
				this.CheckIfDisposed();
				if (this.isBound)
				{
					ActiveDirectorySchemaClass activeDirectorySchemaClass = this;
					string defaultSecurityDescriptor = PropertyManager.DefaultSecurityDescriptor;
					if (value == null)
					{
						obj = null;
					}
					else
					{
						obj = value.GetSecurityDescriptorSddlForm(AccessControlSections.All);
					}
					activeDirectorySchemaClass.SetProperty(defaultSecurityDescriptor, obj);
				}
				ActiveDirectorySchemaClass activeDirectorySchemaClass1 = this;
				if (value == null)
				{
					securityDescriptorSddlForm = null;
				}
				else
				{
					securityDescriptorSddlForm = value.GetSecurityDescriptorSddlForm(AccessControlSections.All);
				}
				activeDirectorySchemaClass1.defaultSDSddlForm = securityDescriptorSddlForm;
			}
		}

		public string Description
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && !this.descriptionInitialized)
				{
					this.description = (string)this.GetValueFromCache(PropertyManager.Description, false);
					this.descriptionInitialized = true;
				}
				return this.description;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.SetProperty(PropertyManager.Description, value);
				}
				this.description = value;
			}
		}

		public bool IsDefunct
		{
			get
			{
				this.CheckIfDisposed();
				return this.isDefunct;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.SetProperty(PropertyManager.IsDefunct, value);
				}
				this.isDefunct = value;
			}
		}

		public ActiveDirectorySchemaPropertyCollection MandatoryProperties
		{
			get
			{
				this.CheckIfDisposed();
				if (this.mandatoryProperties == null)
				{
					if (!this.isBound)
					{
						this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, false, PropertyManager.MustContain, new ArrayList());
					}
					else
					{
						if (this.isDefunctOnServer)
						{
							string[] systemMustContain = new string[2];
							systemMustContain[0] = PropertyManager.SystemMustContain;
							systemMustContain[1] = PropertyManager.MustContain;
							this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MustContain, this.GetProperties(this.GetPropertyValuesRecursively(systemMustContain)));
						}
						else
						{
							ArrayList arrayLists = new ArrayList();
							bool flag = false;
							object mandatoryProperties = null;
							try
							{
								mandatoryProperties = this.iadsClass.MandatoryProperties;
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								if (cOMException.ErrorCode != -2147463155)
								{
									throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
								}
								else
								{
									flag = true;
								}
							}
							if (flag)
							{
								this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MustContain, new ArrayList());
							}
							else
							{
								if (mandatoryProperties as ICollection == null)
								{
									arrayLists.Add((string)mandatoryProperties);
								}
								else
								{
									arrayLists.AddRange((ICollection)mandatoryProperties);
								}
								this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MustContain, arrayLists, true);
							}
						}
					}
				}
				return this.mandatoryProperties;
			}
		}

		public string Name
		{
			get
			{
				this.CheckIfDisposed();
				return this.ldapDisplayName;
			}
		}

		public string Oid
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && this.oid == null)
				{
					if (this.isDefunctOnServer)
					{
						this.oid = (string)this.GetValueFromCache(PropertyManager.GovernsID, true);
					}
					else
					{
						try
						{
							this.oid = this.iadsClass.OID;
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
					}
				}
				return this.oid;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.SetProperty(PropertyManager.GovernsID, value);
				}
				this.oid = value;
			}
		}

		public ActiveDirectorySchemaPropertyCollection OptionalProperties
		{
			get
			{
				this.CheckIfDisposed();
				if (this.optionalProperties == null)
				{
					if (!this.isBound)
					{
						this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, false, PropertyManager.MayContain, new ArrayList());
					}
					else
					{
						if (this.isDefunctOnServer)
						{
							string[] systemMayContain = new string[2];
							systemMayContain[0] = PropertyManager.SystemMayContain;
							systemMayContain[1] = PropertyManager.MayContain;
							ArrayList arrayLists = new ArrayList();
							foreach (string propertyValuesRecursively in this.GetPropertyValuesRecursively(systemMayContain))
							{
								if (this.MandatoryProperties.Contains(propertyValuesRecursively))
								{
									continue;
								}
								arrayLists.Add(propertyValuesRecursively);
							}
							this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MayContain, this.GetProperties(arrayLists));
						}
						else
						{
							ArrayList arrayLists1 = new ArrayList();
							bool flag = false;
							object optionalProperties = null;
							try
							{
								optionalProperties = this.iadsClass.OptionalProperties;
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								if (cOMException.ErrorCode != -2147463155)
								{
									throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
								}
								else
								{
									flag = true;
								}
							}
							if (flag)
							{
								this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MayContain, new ArrayList());
							}
							else
							{
								if (optionalProperties as ICollection == null)
								{
									arrayLists1.Add((string)optionalProperties);
								}
								else
								{
									arrayLists1.AddRange((ICollection)optionalProperties);
								}
								this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MayContain, arrayLists1, true);
							}
						}
					}
				}
				return this.optionalProperties;
			}
		}

		public ReadOnlyActiveDirectorySchemaClassCollection PossibleInferiors
		{
			get
			{
				this.CheckIfDisposed();
				if (this.possibleInferiors == null)
				{
					if (!this.isBound)
					{
						this.possibleInferiors = new ReadOnlyActiveDirectorySchemaClassCollection(new ArrayList());
					}
					else
					{
						this.possibleInferiors = new ReadOnlyActiveDirectorySchemaClassCollection(this.GetClasses(this.GetValuesFromCache(PropertyManager.PossibleInferiors)));
					}
				}
				return this.possibleInferiors;
			}
		}

		public ActiveDirectorySchemaClassCollection PossibleSuperiors
		{
			get
			{
				this.CheckIfDisposed();
				if (this.possibleSuperiors == null)
				{
					if (!this.isBound)
					{
						this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, false, PropertyManager.PossibleSuperiors, new ArrayList());
					}
					else
					{
						if (this.isDefunctOnServer)
						{
							ArrayList arrayLists = new ArrayList();
							arrayLists.AddRange(this.GetValuesFromCache(PropertyManager.PossibleSuperiors));
							arrayLists.AddRange(this.GetValuesFromCache(PropertyManager.SystemPossibleSuperiors));
							this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.PossibleSuperiors, this.GetClasses(arrayLists));
						}
						else
						{
							ArrayList arrayLists1 = new ArrayList();
							bool flag = false;
							object possibleSuperiors = null;
							try
							{
								possibleSuperiors = this.iadsClass.PossibleSuperiors;
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								if (cOMException.ErrorCode != -2147463155)
								{
									throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
								}
								else
								{
									flag = true;
								}
							}
							if (flag)
							{
								this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.PossibleSuperiors, new ArrayList());
							}
							else
							{
								if (possibleSuperiors as ICollection == null)
								{
									arrayLists1.Add((string)possibleSuperiors);
								}
								else
								{
									arrayLists1.AddRange((ICollection)possibleSuperiors);
								}
								this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.PossibleSuperiors, arrayLists1, true);
							}
						}
					}
				}
				return this.possibleSuperiors;
			}
		}

		public Guid SchemaGuid
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && this.schemaGuidBinaryForm == null)
				{
					this.schemaGuidBinaryForm = (byte[])this.GetValueFromCache(PropertyManager.SchemaIDGuid, true);
				}
				return new Guid(this.schemaGuidBinaryForm);
			}
			set
			{
				byte[] byteArray;
				object obj;
				this.CheckIfDisposed();
				if (this.isBound)
				{
					ActiveDirectorySchemaClass activeDirectorySchemaClass = this;
					string schemaIDGuid = PropertyManager.SchemaIDGuid;
					if (value.Equals(Guid.Empty))
					{
						obj = null;
					}
					else
					{
						obj = value.ToByteArray();
					}
					activeDirectorySchemaClass.SetProperty(schemaIDGuid, obj);
				}
				ActiveDirectorySchemaClass activeDirectorySchemaClass1 = this;
				if (value.Equals(Guid.Empty))
				{
					byteArray = null;
				}
				else
				{
					byteArray = value.ToByteArray();
				}
				activeDirectorySchemaClass1.schemaGuidBinaryForm = byteArray;
			}
		}

		public ActiveDirectorySchemaClass SubClassOf
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && this.subClassOf == null)
				{
					this.subClassOf = new ActiveDirectorySchemaClass(this.context, (string)this.GetValueFromCache(PropertyManager.SubClassOf, true), (DirectoryEntry)null, this.schemaEntry);
				}
				return this.subClassOf;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.SetProperty(PropertyManager.SubClassOf, value);
				}
				this.subClassOf = value;
			}
		}

		public SchemaClassType Type
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && !this.typeInitialized)
				{
					this.type = (SchemaClassType)((int)this.GetValueFromCache(PropertyManager.ObjectClassCategory, true));
					this.typeInitialized = true;
				}
				return this.type;
			}
			set
			{
				this.CheckIfDisposed();
				if (value < SchemaClassType.Type88 || value > SchemaClassType.Auxiliary)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(SchemaClassType));
				}
				else
				{
					if (this.isBound)
					{
						this.SetProperty(PropertyManager.ObjectClassCategory, value);
					}
					this.type = value;
					return;
				}
			}
		}

		public ActiveDirectorySchemaClass(DirectoryContext context, string ldapDisplayName)
		{
			this.type = SchemaClassType.Structural;
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isADAMConfigSet() || context.isServer())
					{
						if (ldapDisplayName != null)
						{
							if (ldapDisplayName.Length != 0)
							{
								this.context = new DirectoryContext(context);
								this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.SchemaNamingContext);
								//TODO: REVIEW: URGENT!!:this.schemaEntry.Bind(true);
								this.ldapDisplayName = ldapDisplayName;
								this.commonName = ldapDisplayName;
								this.isBound = false;
								return;
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "ldapDisplayName");
							}
						}
						else
						{
							throw new ArgumentNullException("ldapDisplayName");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		internal ActiveDirectorySchemaClass(DirectoryContext context, string ldapDisplayName, DirectoryEntry classEntry, DirectoryEntry schemaEntry)
		{
			this.type = SchemaClassType.Structural;
			this.context = context;
			this.ldapDisplayName = ldapDisplayName;
			this.classEntry = classEntry;
			this.schemaEntry = schemaEntry;
			this.isDefunctOnServer = false;
			this.isDefunct = this.isDefunctOnServer;
			try
			{
				this.abstractClassEntry = DirectoryEntryManager.GetDirectoryEntryInternal(context, string.Concat("LDAP://", context.GetServerName(), "/schema/", ldapDisplayName));
				this.iadsClass = (NativeComInterfaces.IAdsClass)this.abstractClassEntry.NativeObject;
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				if (cOMException.ErrorCode != -2147463168)
				{
					throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
				}
				else
				{
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaClass), ldapDisplayName);
				}
			}
			catch (InvalidCastException invalidCastException)
			{
				throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaClass), ldapDisplayName);
			}
			catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
			{
				object[] name = new object[1];
				name[0] = context.Name;
				throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", name));
			}
			this.isBound = true;
		}

		internal ActiveDirectorySchemaClass(DirectoryContext context, string commonName, Hashtable propertyValuesFromServer, DirectoryEntry schemaEntry)
		{
			this.type = SchemaClassType.Structural;
			this.context = context;
			this.schemaEntry = schemaEntry;
			this.propertyValuesFromServer = propertyValuesFromServer;
			this.propertiesFromSchemaContainerInitialized = true;
			this.classEntry = this.GetSchemaClassDirectoryEntry();
			this.commonName = commonName;
			this.ldapDisplayName = (string)this.GetValueFromCache(PropertyManager.LdapDisplayName, true);
			this.isDefunctOnServer = true;
			this.isDefunct = this.isDefunctOnServer;
			this.isBound = true;
		}

		internal ActiveDirectorySchemaClass(DirectoryContext context, string commonName, string ldapDisplayName, DirectoryEntry classEntry, DirectoryEntry schemaEntry)
		{
			this.type = SchemaClassType.Structural;
			this.context = context;
			this.schemaEntry = schemaEntry;
			this.classEntry = classEntry;
			this.commonName = commonName;
			this.ldapDisplayName = ldapDisplayName;
			this.isDefunctOnServer = true;
			this.isDefunct = this.isDefunctOnServer;
			this.isBound = true;
		}

		private void CheckIfDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.schemaEntry != null)
					{
						this.schemaEntry.Dispose();
						this.schemaEntry = null;
					}
					if (this.classEntry != null)
					{
						this.classEntry.Dispose();
						this.classEntry = null;
					}
					if (this.abstractClassEntry != null)
					{
						this.abstractClassEntry.Dispose();
						this.abstractClassEntry = null;
					}
					if (this.schema != null)
					{
						this.schema.Dispose();
					}
				}
				this.disposed = true;
			}
		}

		public static ActiveDirectorySchemaClass FindByName(DirectoryContext context, string ldapDisplayName)
		{
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isServer() || context.isADAMConfigSet())
					{
						if (ldapDisplayName != null)
						{
							if (ldapDisplayName.Length != 0)
							{
								context = new DirectoryContext(context);
								ActiveDirectorySchemaClass activeDirectorySchemaClass = new ActiveDirectorySchemaClass(context, ldapDisplayName, (DirectoryEntry)null, (DirectoryEntry)null);
								return activeDirectorySchemaClass;
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "ldapDisplayName");
							}
						}
						else
						{
							throw new ArgumentNullException("ldapDisplayName");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public ReadOnlyActiveDirectorySchemaPropertyCollection GetAllProperties()
		{
			this.CheckIfDisposed();
			ArrayList arrayLists = new ArrayList();
			arrayLists.AddRange(this.MandatoryProperties);
			arrayLists.AddRange(this.OptionalProperties);
			return new ReadOnlyActiveDirectorySchemaPropertyCollection(arrayLists);
		}

		private ArrayList GetClasses(ICollection ldapDisplayNames)
		{
			ArrayList arrayLists;
			ArrayList arrayLists1 = new ArrayList();
			SearchResultCollection searchResultCollections = null;
			using (searchResultCollections)
			{
				try
				{
					if (ldapDisplayNames.Count >= 1)
					{
						if (this.schemaEntry == null)
						{
							this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
						}
						StringBuilder stringBuilder = new StringBuilder(100);
						if (ldapDisplayNames.Count > 1)
						{
							stringBuilder.Append("(|");
						}
						foreach (string ldapDisplayName in ldapDisplayNames)
						{
							stringBuilder.Append("(");
							stringBuilder.Append(PropertyManager.LdapDisplayName);
							stringBuilder.Append("=");
							stringBuilder.Append(Utils.GetEscapedFilterValue(ldapDisplayName));
							stringBuilder.Append(")");
						}
						if (ldapDisplayNames.Count > 1)
						{
							stringBuilder.Append(")");
						}
						string[] objectCategory = new string[7];
						objectCategory[0] = "(&(";
						objectCategory[1] = PropertyManager.ObjectCategory;
						objectCategory[2] = "=classSchema)";
						objectCategory[3] = stringBuilder.ToString();
						objectCategory[4] = "(!(";
						objectCategory[5] = PropertyManager.IsDefunct;
						objectCategory[6] = "=TRUE)))";
						string str = string.Concat(objectCategory);
						string[] strArrays = new string[1];
						strArrays[0] = PropertyManager.LdapDisplayName;
						ADSearcher aDSearcher = new ADSearcher(this.schemaEntry, str, strArrays, SearchScope.OneLevel);
						searchResultCollections = aDSearcher.FindAll();
						foreach (SearchResult searchResult in searchResultCollections)
						{
							string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.LdapDisplayName);
							DirectoryEntry directoryEntry = searchResult.GetDirectoryEntry();
							directoryEntry.AuthenticationType = Utils.DefaultAuthType;
							directoryEntry.Username = this.context.UserName;
							directoryEntry.Password = this.context.Password;
							ActiveDirectorySchemaClass activeDirectorySchemaClass = new ActiveDirectorySchemaClass(this.context, searchResultPropertyValue, directoryEntry, this.schemaEntry);
							arrayLists1.Add(activeDirectorySchemaClass);
						}
					}
					else
					{
						arrayLists = arrayLists1;
						return arrayLists;
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				return arrayLists1;
			}
			return arrayLists;
		}

		public DirectoryEntry GetDirectoryEntry()
		{
			this.CheckIfDisposed();
			if (this.isBound)
			{
				this.GetSchemaClassDirectoryEntry();
				return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.classEntry.Path);
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("CannotGetObject"));
			}
		}

		private ArrayList GetProperties(ICollection ldapDisplayNames)
		{
			ArrayList arrayLists;
			ArrayList arrayLists1 = new ArrayList();
			SearchResultCollection searchResultCollections = null;
			using (searchResultCollections)
			{
				try
				{
					if (ldapDisplayNames.Count >= 1)
					{
						if (this.schemaEntry == null)
						{
							this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
						}
						StringBuilder stringBuilder = new StringBuilder(100);
						if (ldapDisplayNames.Count > 1)
						{
							stringBuilder.Append("(|");
						}
						foreach (string ldapDisplayName in ldapDisplayNames)
						{
							stringBuilder.Append("(");
							stringBuilder.Append(PropertyManager.LdapDisplayName);
							stringBuilder.Append("=");
							stringBuilder.Append(Utils.GetEscapedFilterValue(ldapDisplayName));
							stringBuilder.Append(")");
						}
						if (ldapDisplayNames.Count > 1)
						{
							stringBuilder.Append(")");
						}
						string[] objectCategory = new string[7];
						objectCategory[0] = "(&(";
						objectCategory[1] = PropertyManager.ObjectCategory;
						objectCategory[2] = "=attributeSchema)";
						objectCategory[3] = stringBuilder.ToString();
						objectCategory[4] = "(!(";
						objectCategory[5] = PropertyManager.IsDefunct;
						objectCategory[6] = "=TRUE)))";
						string str = string.Concat(objectCategory);
						string[] strArrays = new string[1];
						strArrays[0] = PropertyManager.LdapDisplayName;
						ADSearcher aDSearcher = new ADSearcher(this.schemaEntry, str, strArrays, SearchScope.OneLevel);
						searchResultCollections = aDSearcher.FindAll();
						foreach (SearchResult searchResult in searchResultCollections)
						{
							string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.LdapDisplayName);
							DirectoryEntry directoryEntry = searchResult.GetDirectoryEntry();
							directoryEntry.AuthenticationType = Utils.DefaultAuthType;
							directoryEntry.Username = this.context.UserName;
							directoryEntry.Password = this.context.Password;
							ActiveDirectorySchemaProperty activeDirectorySchemaProperty = new ActiveDirectorySchemaProperty(this.context, searchResultPropertyValue, directoryEntry, this.schemaEntry);
							arrayLists1.Add(activeDirectorySchemaProperty);
						}
					}
					else
					{
						arrayLists = arrayLists1;
						return arrayLists;
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				return arrayLists1;
			}
			return arrayLists;
		}

		internal static Hashtable GetPropertiesFromSchemaContainer(DirectoryContext context, DirectoryEntry schemaEntry, string name, bool isDefunctOnServer)
		{
			Hashtable valuesWithRangeRetrieval = null;
			StringBuilder stringBuilder = new StringBuilder(15);
			stringBuilder.Append("(&(");
			stringBuilder.Append(PropertyManager.ObjectCategory);
			stringBuilder.Append("=classSchema)");
			stringBuilder.Append("(");
			if (isDefunctOnServer)
			{
				stringBuilder.Append(PropertyManager.Cn);
			}
			else
			{
				stringBuilder.Append(PropertyManager.LdapDisplayName);
			}
			stringBuilder.Append("=");
			stringBuilder.Append(Utils.GetEscapedFilterValue(name));
			stringBuilder.Append(")");
			if (isDefunctOnServer)
			{
				stringBuilder.Append("(");
			}
			else
			{
				stringBuilder.Append("(!(");
			}
			stringBuilder.Append(PropertyManager.IsDefunct);
			if (isDefunctOnServer)
			{
				stringBuilder.Append("=TRUE))");
			}
			else
			{
				stringBuilder.Append("=TRUE)))");
			}
			ArrayList arrayLists = new ArrayList();
			ArrayList arrayLists1 = new ArrayList();
			arrayLists1.Add(PropertyManager.DistinguishedName);
			arrayLists1.Add(PropertyManager.Cn);
			arrayLists1.Add(PropertyManager.Description);
			arrayLists1.Add(PropertyManager.PossibleInferiors);
			arrayLists1.Add(PropertyManager.SubClassOf);
			arrayLists1.Add(PropertyManager.ObjectClassCategory);
			arrayLists1.Add(PropertyManager.SchemaIDGuid);
			arrayLists1.Add(PropertyManager.DefaultSecurityDescriptor);
			arrayLists.Add(PropertyManager.AuxiliaryClass);
			arrayLists.Add(PropertyManager.SystemAuxiliaryClass);
			arrayLists.Add(PropertyManager.MustContain);
			arrayLists.Add(PropertyManager.SystemMustContain);
			arrayLists.Add(PropertyManager.MayContain);
			arrayLists.Add(PropertyManager.SystemMayContain);
			if (isDefunctOnServer)
			{
				arrayLists1.Add(PropertyManager.LdapDisplayName);
				arrayLists1.Add(PropertyManager.GovernsID);
				arrayLists.Add(PropertyManager.SystemPossibleSuperiors);
				arrayLists.Add(PropertyManager.PossibleSuperiors);
			}
			try
			{
				valuesWithRangeRetrieval = Utils.GetValuesWithRangeRetrieval(schemaEntry, stringBuilder.ToString(), arrayLists, arrayLists1, SearchScope.OneLevel);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				if (cOMException.ErrorCode != -2147016656)
				{
					throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
				}
				else
				{
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaClass), name);
				}
			}
			return valuesWithRangeRetrieval;
		}

		private ArrayList GetPropertyValuesRecursively(string[] propertyNames)
		{
			ActiveDirectorySchemaClass activeDirectorySchemaClass = null;
			ActiveDirectorySchemaClass activeDirectorySchemaClass1 = null;
			ArrayList arrayLists = new ArrayList();
			try
			{
				if (Utils.Compare(this.SubClassOf.Name, this.Name) != 0)
				{
					foreach (string propertyValuesRecursively in this.SubClassOf.GetPropertyValuesRecursively(propertyNames))
					{
						if (arrayLists.Contains(propertyValuesRecursively))
						{
							continue;
						}
						arrayLists.Add(propertyValuesRecursively);
					}
				}
				foreach (string str in activeDirectorySchemaClass.GetPropertyValuesRecursively(propertyNames))
				{
					activeDirectorySchemaClass = new ActiveDirectorySchemaClass(this.context, str, (DirectoryEntry)null, (DirectoryEntry)null);
					IEnumerator enumerator = activeDirectorySchemaClass.GetPropertyValuesRecursively(propertyNames).GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string str1 = (string)str;
							if (arrayLists.Contains(str1))
							{
								continue;
							}
							arrayLists.Add(str1);
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
				foreach (string propertyValuesRecursively1 in activeDirectorySchemaClass1.GetPropertyValuesRecursively(propertyNames))
				{
					activeDirectorySchemaClass1 = new ActiveDirectorySchemaClass(this.context, propertyValuesRecursively1, (DirectoryEntry)null, (DirectoryEntry)null);
					IEnumerator enumerator1 = activeDirectorySchemaClass1.GetPropertyValuesRecursively(propertyNames).GetEnumerator();
					try
					{
						while (enumerator1.MoveNext())
						{
							string str2 = (string)propertyValuesRecursively1;
							if (arrayLists.Contains(str2))
							{
								continue;
							}
							arrayLists.Add(str2);
						}
					}
					finally
					{
						IDisposable disposable1 = enumerator1 as IDisposable;
						if (disposable1 != null)
						{
							disposable1.Dispose();
						}
					}
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			string[] strArrays = propertyNames;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str3 = strArrays[i];
				foreach (string valuesFromCache in this.GetValuesFromCache(str3))
				{
					if (arrayLists.Contains(valuesFromCache))
					{
						continue;
					}
					arrayLists.Add(valuesFromCache);
				}
			}
			return arrayLists;
		}

		internal DirectoryEntry GetSchemaClassDirectoryEntry()
		{
			if (this.classEntry == null)
			{
				this.InitializePropertiesFromSchemaContainer();
				this.classEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, (string)this.GetValueFromCache(PropertyManager.DistinguishedName, true));
			}
			return this.classEntry;
		}

		private object GetValueFromCache(string propertyName, bool mustExist)
		{
			object item = null;
			this.InitializePropertiesFromSchemaContainer();
			ArrayList arrayLists = (ArrayList)this.propertyValuesFromServer[propertyName.ToLower(CultureInfo.InvariantCulture)];
			if (arrayLists.Count >= 1 || !mustExist)
			{
				if (arrayLists.Count > 0)
				{
					item = arrayLists[0];
				}
				return item;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = propertyName;
				throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", objArray));
			}
		}

		private ICollection GetValuesFromCache(string propertyName)
		{
			this.InitializePropertiesFromSchemaContainer();
			ArrayList item = (ArrayList)this.propertyValuesFromServer[propertyName.ToLower(CultureInfo.InvariantCulture)];
			return item;
		}

		private void InitializePropertiesFromSchemaContainer()
		{
			string str;
			if (!this.propertiesFromSchemaContainerInitialized)
			{
				if (this.schemaEntry == null)
				{
					this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
				}
				ActiveDirectorySchemaClass propertiesFromSchemaContainer = this;
				DirectoryContext directoryContext = this.context;
				DirectoryEntry directoryEntry = this.schemaEntry;
				if (this.isDefunctOnServer)
				{
					str = this.commonName;
				}
				else
				{
					str = this.ldapDisplayName;
				}
				propertiesFromSchemaContainer.propertyValuesFromServer = ActiveDirectorySchemaClass.GetPropertiesFromSchemaContainer(directoryContext, directoryEntry, str, this.isDefunctOnServer);
				this.propertiesFromSchemaContainerInitialized = true;
			}
		}

		public void Save()
		{
			this.CheckIfDisposed();
			if (!this.isBound)
			{
				try
				{
					if (this.schemaEntry == null)
					{
						this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
					}
					string escapedPath = string.Concat("CN=", this.commonName);
					escapedPath = Utils.GetEscapedPath(escapedPath);
					this.classEntry = this.schemaEntry.Children.Add(escapedPath, "classSchema");
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
				{
					object[] name = new object[1];
					name[0] = this.context.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", name));
				}
				this.SetProperty(PropertyManager.LdapDisplayName, this.ldapDisplayName);
				this.SetProperty(PropertyManager.GovernsID, this.oid);
				this.SetProperty(PropertyManager.Description, this.description);
				if (this.possibleSuperiors != null)
				{
					this.classEntry.Properties[PropertyManager.PossibleSuperiors].AddRange(this.possibleSuperiors.GetMultiValuedProperty());
				}
				if (this.mandatoryProperties != null)
				{
					this.classEntry.Properties[PropertyManager.MustContain].AddRange(this.mandatoryProperties.GetMultiValuedProperty());
				}
				if (this.optionalProperties != null)
				{
					this.classEntry.Properties[PropertyManager.MayContain].AddRange(this.optionalProperties.GetMultiValuedProperty());
				}
				if (this.subClassOf == null)
				{
					this.SetProperty(PropertyManager.SubClassOf, "top");
				}
				else
				{
					this.SetProperty(PropertyManager.SubClassOf, this.subClassOf.Name);
				}
				this.SetProperty(PropertyManager.ObjectClassCategory, this.type);
				if (this.schemaGuidBinaryForm != null)
				{
					this.SetProperty(PropertyManager.SchemaIDGuid, this.schemaGuidBinaryForm);
				}
				if (this.defaultSDSddlForm != null)
				{
					this.SetProperty(PropertyManager.DefaultSecurityDescriptor, this.defaultSDSddlForm);
				}
			}
			try
			{
				this.classEntry.CommitChanges();
				if (this.schema == null)
				{
					ActiveDirectorySchema schema = ActiveDirectorySchema.GetSchema(this.context);
					bool flag = false;
					DirectoryServer schemaRoleOwner = null;
					try
					{
						schemaRoleOwner = schema.SchemaRoleOwner;
						if (Utils.Compare(schemaRoleOwner.Name, this.context.GetServerName()) == 0)
						{
							flag = true;
							this.schema = schema;
						}
						else
						{
							DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(schemaRoleOwner.Name, DirectoryContextType.DirectoryServer, this.context);
							this.schema = ActiveDirectorySchema.GetSchema(newDirectoryContext);
						}
					}
					finally
					{
						if (schemaRoleOwner != null)
						{
							schemaRoleOwner.Dispose();
						}
						if (!flag)
						{
							schema.Dispose();
						}
					}
				}
				this.schema.RefreshSchema();
			}
			catch (COMException cOMException3)
			{
				COMException cOMException2 = cOMException3;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
			}
			this.isDefunctOnServer = this.isDefunct;
			this.commonName = null;
			this.oid = null;
			this.description = null;
			this.descriptionInitialized = false;
			this.possibleSuperiors = null;
			this.auxiliaryClasses = null;
			this.possibleInferiors = null;
			this.mandatoryProperties = null;
			this.optionalProperties = null;
			this.subClassOf = null;
			this.typeInitialized = false;
			this.schemaGuidBinaryForm = null;
			this.defaultSDSddlForm = null;
			this.defaultSDSddlFormInitialized = false;
			this.propertiesFromSchemaContainerInitialized = false;
			this.isBound = true;
		}

		private void SetProperty(string propertyName, object value)
		{
			this.GetSchemaClassDirectoryEntry();
			try
			{
				if (value != null)
				{
					this.classEntry.Properties[propertyName].Value = value;
				}
				else
				{
					if (this.classEntry.Properties.Contains(propertyName))
					{
						this.classEntry.Properties[propertyName].Clear();
					}
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Name;
		}
	}
}