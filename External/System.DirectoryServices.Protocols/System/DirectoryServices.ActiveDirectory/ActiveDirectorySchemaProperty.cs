using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ActiveDirectorySchemaProperty : IDisposable
	{
		private DirectoryEntry schemaEntry;

		private DirectoryEntry propertyEntry;

		private DirectoryEntry abstractPropertyEntry;

		private NativeComInterfaces.IAdsProperty iadsProperty;

		private DirectoryContext context;

		internal bool isBound;

		private bool disposed;

		private ActiveDirectorySchema schema;

		private bool propertiesFromSchemaContainerInitialized;

		private bool isDefunctOnServer;

		private SearchResult propertyValuesFromServer;

		private string ldapDisplayName;

		private string commonName;

		private string oid;

		private ActiveDirectorySyntax syntax;

		private bool syntaxInitialized;

		private string description;

		private bool descriptionInitialized;

		private bool isSingleValued;

		private bool isSingleValuedInitialized;

		private bool isInGlobalCatalog;

		private bool isInGlobalCatalogInitialized;

		private int? rangeLower;

		private bool rangeLowerInitialized;

		private int? rangeUpper;

		private bool rangeUpperInitialized;

		private bool isDefunct;

		private SearchFlags searchFlags;

		private bool searchFlagsInitialized;

		private ActiveDirectorySchemaProperty linkedProperty;

		private bool linkedPropertyInitialized;

		private int? linkId;

		private bool linkIdInitialized;

		private byte[] schemaGuidBinaryForm;

		private static OMObjectClass dnOMObjectClass;

		private static OMObjectClass dNWithStringOMObjectClass;

		private static OMObjectClass dNWithBinaryOMObjectClass;

		private static OMObjectClass replicaLinkOMObjectClass;

		private static OMObjectClass presentationAddressOMObjectClass;

		private static OMObjectClass accessPointDnOMObjectClass;

		private static OMObjectClass oRNameOMObjectClass;

		private static int SyntaxesCount;

		private static Syntax[] syntaxes;

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
				if (value == null || value.Length != 0)
				{
					if (this.isBound)
					{
						this.SetProperty(PropertyManager.Cn, value);
					}
					this.commonName = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "value");
				}
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
				if (value == null || value.Length != 0)
				{
					if (this.isBound)
					{
						this.SetProperty(PropertyManager.Description, value);
					}
					this.description = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "value");
				}
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

		public bool IsInAnr
		{
			get
			{
				this.CheckIfDisposed();
				return this.IsSetInSearchFlags(SearchFlags.IsInAnr);
			}
			set
			{
				this.CheckIfDisposed();
				if (!value)
				{
					this.ResetBitInSearchFlags(SearchFlags.IsInAnr);
					return;
				}
				else
				{
					this.SetBitInSearchFlags(SearchFlags.IsInAnr);
					return;
				}
			}
		}

		public bool IsIndexed
		{
			get
			{
				this.CheckIfDisposed();
				return this.IsSetInSearchFlags(SearchFlags.IsIndexed);
			}
			set
			{
				this.CheckIfDisposed();
				if (!value)
				{
					this.ResetBitInSearchFlags(SearchFlags.IsIndexed);
					return;
				}
				else
				{
					this.SetBitInSearchFlags(SearchFlags.IsIndexed);
					return;
				}
			}
		}

		public bool IsIndexedOverContainer
		{
			get
			{
				this.CheckIfDisposed();
				return this.IsSetInSearchFlags(SearchFlags.IsIndexedOverContainer);
			}
			set
			{
				this.CheckIfDisposed();
				if (!value)
				{
					this.ResetBitInSearchFlags(SearchFlags.IsIndexedOverContainer);
					return;
				}
				else
				{
					this.SetBitInSearchFlags(SearchFlags.IsIndexedOverContainer);
					return;
				}
			}
		}

		public bool IsInGlobalCatalog
		{
			get
			{
				bool flag;
				this.CheckIfDisposed();
				if (this.isBound && !this.isInGlobalCatalogInitialized)
				{
					object valueFromCache = this.GetValueFromCache(PropertyManager.IsMemberOfPartialAttributeSet, false);
					ActiveDirectorySchemaProperty activeDirectorySchemaProperty = this;
					if (valueFromCache != null)
					{
						flag = (bool)valueFromCache;
					}
					else
					{
						flag = false;
					}
					activeDirectorySchemaProperty.isInGlobalCatalog = flag;
					this.isInGlobalCatalogInitialized = true;
				}
				return this.isInGlobalCatalog;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.GetSchemaPropertyDirectoryEntry();
					this.propertyEntry.Properties[PropertyManager.IsMemberOfPartialAttributeSet].Value = value;
				}
				this.isInGlobalCatalog = value;
			}
		}

		public bool IsOnTombstonedObject
		{
			get
			{
				this.CheckIfDisposed();
				return this.IsSetInSearchFlags(SearchFlags.IsOnTombstonedObject);
			}
			set
			{
				this.CheckIfDisposed();
				if (!value)
				{
					this.ResetBitInSearchFlags(SearchFlags.IsOnTombstonedObject);
					return;
				}
				else
				{
					this.SetBitInSearchFlags(SearchFlags.IsOnTombstonedObject);
					return;
				}
			}
		}

		public bool IsSingleValued
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && !this.isSingleValuedInitialized)
				{
					if (this.isDefunctOnServer)
					{
						this.isSingleValued = (bool)this.GetValueFromCache(PropertyManager.IsSingleValued, true);
					}
					else
					{
						try
						{
							this.isSingleValued = !this.iadsProperty.MultiValued;
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
					}
					this.isSingleValuedInitialized = true;
				}
				return this.isSingleValued;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.GetSchemaPropertyDirectoryEntry();
					this.propertyEntry.Properties[PropertyManager.IsSingleValued].Value = value;
				}
				this.isSingleValued = value;
			}
		}

		public bool IsTupleIndexed
		{
			get
			{
				this.CheckIfDisposed();
				return this.IsSetInSearchFlags(SearchFlags.IsTupleIndexed);
			}
			set
			{
				this.CheckIfDisposed();
				if (!value)
				{
					this.ResetBitInSearchFlags(SearchFlags.IsTupleIndexed);
					return;
				}
				else
				{
					this.SetBitInSearchFlags(SearchFlags.IsTupleIndexed);
					return;
				}
			}
		}

		public ActiveDirectorySchemaProperty Link
		{
			get
			{
				int num;
				this.CheckIfDisposed();
				if (this.isBound && !this.linkedPropertyInitialized)
				{
					object valueFromCache = this.GetValueFromCache(PropertyManager.LinkID, false);
					if (valueFromCache != null)
					{
						num = (int)valueFromCache;
					}
					else
					{
						num = -1;
					}
					int num1 = num;
					if (num1 != -1)
					{
						int num2 = num1 - 2 * num1 % 2 + 1;
						try
						{
							if (this.schemaEntry == null)
							{
								this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
							}
							object[] objectCategory = new object[7];
							objectCategory[0] = "(&(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=attributeSchema)(";
							objectCategory[3] = PropertyManager.LinkID;
							objectCategory[4] = "=";
							objectCategory[5] = num2;
							objectCategory[6] = "))";
							string str = string.Concat(objectCategory);
							ReadOnlyActiveDirectorySchemaPropertyCollection allProperties = ActiveDirectorySchema.GetAllProperties(this.context, this.schemaEntry, str);
							if (allProperties.Count == 1)
							{
								this.linkedProperty = allProperties[0];
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = num2;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("LinkedPropertyNotFound", objArray), typeof(ActiveDirectorySchemaProperty), null);
							}
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
					}
					this.linkedPropertyInitialized = true;
				}
				return this.linkedProperty;
			}
		}

		public int? LinkId
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && !this.linkIdInitialized)
				{
					object valueFromCache = this.GetValueFromCache(PropertyManager.LinkID, false);
					if (valueFromCache != null)
					{
						this.linkId = new int?((int)valueFromCache);
					}
					else
					{
						this.linkId = null;
					}
					this.linkIdInitialized = true;
				}
				return this.linkId;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.GetSchemaPropertyDirectoryEntry();
					if (value.HasValue)
					{
						this.propertyEntry.Properties[PropertyManager.LinkID].Value = value.Value;
					}
					else
					{
						if (this.propertyEntry.Properties.Contains(PropertyManager.LinkID))
						{
							this.propertyEntry.Properties[PropertyManager.LinkID].Clear();
						}
					}
				}
				this.linkId = value;
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
						this.oid = (string)this.GetValueFromCache(PropertyManager.AttributeID, true);
					}
					else
					{
						try
						{
							this.oid = this.iadsProperty.OID;
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
				if (value == null || value.Length != 0)
				{
					if (this.isBound)
					{
						this.SetProperty(PropertyManager.AttributeID, value);
					}
					this.oid = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "value");
				}
			}
		}

		public int? RangeLower
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && !this.rangeLowerInitialized)
				{
					object valueFromCache = this.GetValueFromCache(PropertyManager.RangeLower, false);
					if (valueFromCache != null)
					{
						this.rangeLower = new int?((int)valueFromCache);
					}
					else
					{
						this.rangeLower = null;
					}
					this.rangeLowerInitialized = true;
				}
				return this.rangeLower;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.GetSchemaPropertyDirectoryEntry();
					if (value.HasValue)
					{
						this.propertyEntry.Properties[PropertyManager.RangeLower].Value = value.Value;
					}
					else
					{
						if (this.propertyEntry.Properties.Contains(PropertyManager.RangeLower))
						{
							this.propertyEntry.Properties[PropertyManager.RangeLower].Clear();
						}
					}
				}
				this.rangeLower = value;
			}
		}

		public int? RangeUpper
		{
			get
			{
				this.CheckIfDisposed();
				if (this.isBound && !this.rangeUpperInitialized)
				{
					object valueFromCache = this.GetValueFromCache(PropertyManager.RangeUpper, false);
					if (valueFromCache != null)
					{
						this.rangeUpper = new int?((int)valueFromCache);
					}
					else
					{
						this.rangeUpper = null;
					}
					this.rangeUpperInitialized = true;
				}
				return this.rangeUpper;
			}
			set
			{
				this.CheckIfDisposed();
				if (this.isBound)
				{
					this.GetSchemaPropertyDirectoryEntry();
					if (value.HasValue)
					{
						this.propertyEntry.Properties[PropertyManager.RangeUpper].Value = value.Value;
					}
					else
					{
						if (this.propertyEntry.Properties.Contains(PropertyManager.RangeUpper))
						{
							this.propertyEntry.Properties[PropertyManager.RangeUpper].Clear();
						}
					}
				}
				this.rangeUpper = value;
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
					ActiveDirectorySchemaProperty activeDirectorySchemaProperty = this;
					string schemaIDGuid = PropertyManager.SchemaIDGuid;
					if (value.Equals(Guid.Empty))
					{
						obj = null;
					}
					else
					{
						obj = value.ToByteArray();
					}
					activeDirectorySchemaProperty.SetProperty(schemaIDGuid, obj);
				}
				ActiveDirectorySchemaProperty activeDirectorySchemaProperty1 = this;
				if (value.Equals(Guid.Empty))
				{
					byteArray = null;
				}
				else
				{
					byteArray = value.ToByteArray();
				}
				activeDirectorySchemaProperty1.schemaGuidBinaryForm = byteArray;
			}
		}

		public ActiveDirectorySyntax Syntax
		{
			get
			{
				OMObjectClass oMObjectClass;
				this.CheckIfDisposed();
				if (this.isBound && !this.syntaxInitialized)
				{
					byte[] valueFromCache = (byte[])this.GetValueFromCache(PropertyManager.OMObjectClass, false);
					if (valueFromCache != null)
					{
						oMObjectClass = new OMObjectClass(valueFromCache);
					}
					else
					{
						oMObjectClass = null;
					}
					OMObjectClass oMObjectClass1 = oMObjectClass;
					this.syntax = this.MapSyntax((string)this.GetValueFromCache(PropertyManager.AttributeSyntax, true), (int)this.GetValueFromCache(PropertyManager.OMSyntax, true), oMObjectClass1);
					this.syntaxInitialized = true;
				}
				return this.syntax;
			}
			set
			{
				this.CheckIfDisposed();
				if (value < ActiveDirectorySyntax.CaseExactString || value > ActiveDirectorySyntax.ReplicaLink)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ActiveDirectorySyntax));
				}
				else
				{
					if (this.isBound)
					{
						this.SetSyntax(value);
					}
					this.syntax = value;
					return;
				}
			}
		}

		static ActiveDirectorySchemaProperty()
		{
			byte[] numArray = new byte[] { 43, 12, 2, 135, 115, 28, 0, 133, 74 };
			ActiveDirectorySchemaProperty.dnOMObjectClass = new OMObjectClass(numArray);
			byte[] numArray1 = new byte[] { 42, 134, 72, 134, 247, 20, 1, 1, 1, 12 };
			ActiveDirectorySchemaProperty.dNWithStringOMObjectClass = new OMObjectClass(numArray1);
			byte[] numArray2 = new byte[] { 42, 134, 72, 134, 247, 20, 1, 1, 1, 11 };
			ActiveDirectorySchemaProperty.dNWithBinaryOMObjectClass = new OMObjectClass(numArray2);
			byte[] numArray3 = new byte[] { 42, 134, 72, 134, 247, 20, 1, 1, 1, 6 };
			ActiveDirectorySchemaProperty.replicaLinkOMObjectClass = new OMObjectClass(numArray3);
			byte[] numArray4 = new byte[] { 43, 12, 2, 135, 115, 28, 0, 133, 92 };
			ActiveDirectorySchemaProperty.presentationAddressOMObjectClass = new OMObjectClass(numArray4);
			byte[] numArray5 = new byte[] { 43, 12, 2, 135, 115, 28, 0, 133, 62 };
			ActiveDirectorySchemaProperty.accessPointDnOMObjectClass = new OMObjectClass(numArray5);
			byte[] numArray6 = new byte[] { 86, 6, 1, 2, 5, 11, 29 };
			ActiveDirectorySchemaProperty.oRNameOMObjectClass = new OMObjectClass(numArray6);
			ActiveDirectorySchemaProperty.SyntaxesCount = 23;
			Syntax[] syntax = new Syntax[23];
			syntax[0] = new Syntax("2.5.5.3", 27, null);
			syntax[1] = new Syntax("2.5.5.4", 20, null);
			syntax[2] = new Syntax("2.5.5.6", 18, null);
			syntax[3] = new Syntax("2.5.5.12", 64, null);
			syntax[4] = new Syntax("2.5.5.10", 4, null);
			syntax[5] = new Syntax("2.5.5.15", 66, null);
			syntax[6] = new Syntax("2.5.5.9", 2, null);
			syntax[7] = new Syntax("2.5.5.16", 65, null);
			syntax[8] = new Syntax("2.5.5.8", 1, null);
			syntax[9] = new Syntax("2.5.5.2", 6, null);
			syntax[10] = new Syntax("2.5.5.11", 24, null);
			syntax[11] = new Syntax("2.5.5.11", 23, null);
			syntax[12] = new Syntax("2.5.5.1", 127, ActiveDirectorySchemaProperty.dnOMObjectClass);
			syntax[13] = new Syntax("2.5.5.7", 127, ActiveDirectorySchemaProperty.dNWithBinaryOMObjectClass);
			syntax[14] = new Syntax("2.5.5.14", 127, ActiveDirectorySchemaProperty.dNWithStringOMObjectClass);
			syntax[15] = new Syntax("2.5.5.9", 10, null);
			syntax[16] = new Syntax("2.5.5.5", 22, null);
			syntax[17] = new Syntax("2.5.5.5", 19, null);
			syntax[18] = new Syntax("2.5.5.17", 4, null);
			syntax[19] = new Syntax("2.5.5.14", 127, ActiveDirectorySchemaProperty.accessPointDnOMObjectClass);
			syntax[20] = new Syntax("2.5.5.7", 127, ActiveDirectorySchemaProperty.oRNameOMObjectClass);
			syntax[21] = new Syntax("2.5.5.13", 127, ActiveDirectorySchemaProperty.presentationAddressOMObjectClass);
			syntax[22] = new Syntax("2.5.5.10", 127, ActiveDirectorySchemaProperty.replicaLinkOMObjectClass);
			ActiveDirectorySchemaProperty.syntaxes = syntax;
		}

		public ActiveDirectorySchemaProperty(DirectoryContext context, string ldapDisplayName)
		{
			this.syntax = ActiveDirectorySyntax.CaseIgnoreString | ActiveDirectorySyntax.NumericString | ActiveDirectorySyntax.DirectoryString | ActiveDirectorySyntax.OctetString | ActiveDirectorySyntax.SecurityDescriptor | ActiveDirectorySyntax.Int | ActiveDirectorySyntax.Int64 | ActiveDirectorySyntax.Bool | ActiveDirectorySyntax.Oid | ActiveDirectorySyntax.GeneralizedTime | ActiveDirectorySyntax.UtcTime | ActiveDirectorySyntax.DN | ActiveDirectorySyntax.DNWithBinary | ActiveDirectorySyntax.DNWithString | ActiveDirectorySyntax.Enumeration | ActiveDirectorySyntax.IA5String | ActiveDirectorySyntax.PrintableString | ActiveDirectorySyntax.Sid | ActiveDirectorySyntax.AccessPointDN | ActiveDirectorySyntax.ORName | ActiveDirectorySyntax.PresentationAddress | ActiveDirectorySyntax.ReplicaLink;
			this.rangeLower = null;
			this.rangeUpper = null;
			this.linkId = null;
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

		internal ActiveDirectorySchemaProperty(DirectoryContext context, string ldapDisplayName, DirectoryEntry propertyEntry, DirectoryEntry schemaEntry)
		{
			this.syntax = ActiveDirectorySyntax.CaseIgnoreString | ActiveDirectorySyntax.NumericString | ActiveDirectorySyntax.DirectoryString | ActiveDirectorySyntax.OctetString | ActiveDirectorySyntax.SecurityDescriptor | ActiveDirectorySyntax.Int | ActiveDirectorySyntax.Int64 | ActiveDirectorySyntax.Bool | ActiveDirectorySyntax.Oid | ActiveDirectorySyntax.GeneralizedTime | ActiveDirectorySyntax.UtcTime | ActiveDirectorySyntax.DN | ActiveDirectorySyntax.DNWithBinary | ActiveDirectorySyntax.DNWithString | ActiveDirectorySyntax.Enumeration | ActiveDirectorySyntax.IA5String | ActiveDirectorySyntax.PrintableString | ActiveDirectorySyntax.Sid | ActiveDirectorySyntax.AccessPointDN | ActiveDirectorySyntax.ORName | ActiveDirectorySyntax.PresentationAddress | ActiveDirectorySyntax.ReplicaLink;
			this.rangeLower = null;
			this.rangeUpper = null;
			this.linkId = null;
			this.context = context;
			this.ldapDisplayName = ldapDisplayName;
			this.propertyEntry = propertyEntry;
			this.isDefunctOnServer = false;
			this.isDefunct = this.isDefunctOnServer;
			try
			{
				this.abstractPropertyEntry = DirectoryEntryManager.GetDirectoryEntryInternal(context, string.Concat("LDAP://", context.GetServerName(), "/schema/", ldapDisplayName));
				this.iadsProperty = (NativeComInterfaces.IAdsProperty)this.abstractPropertyEntry.NativeObject;
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
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), ldapDisplayName);
				}
			}
			catch (InvalidCastException invalidCastException)
			{
				throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), ldapDisplayName);
			}
			catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
			{
				object[] name = new object[1];
				name[0] = context.Name;
				throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", name));
			}
			this.isBound = true;
		}

		internal ActiveDirectorySchemaProperty(DirectoryContext context, string commonName, SearchResult propertyValuesFromServer, DirectoryEntry schemaEntry)
		{
			this.syntax = ActiveDirectorySyntax.CaseIgnoreString | ActiveDirectorySyntax.NumericString | ActiveDirectorySyntax.DirectoryString | ActiveDirectorySyntax.OctetString | ActiveDirectorySyntax.SecurityDescriptor | ActiveDirectorySyntax.Int | ActiveDirectorySyntax.Int64 | ActiveDirectorySyntax.Bool | ActiveDirectorySyntax.Oid | ActiveDirectorySyntax.GeneralizedTime | ActiveDirectorySyntax.UtcTime | ActiveDirectorySyntax.DN | ActiveDirectorySyntax.DNWithBinary | ActiveDirectorySyntax.DNWithString | ActiveDirectorySyntax.Enumeration | ActiveDirectorySyntax.IA5String | ActiveDirectorySyntax.PrintableString | ActiveDirectorySyntax.Sid | ActiveDirectorySyntax.AccessPointDN | ActiveDirectorySyntax.ORName | ActiveDirectorySyntax.PresentationAddress | ActiveDirectorySyntax.ReplicaLink;
			this.rangeLower = null;
			this.rangeUpper = null;
			this.linkId = null;
			this.context = context;
			this.schemaEntry = schemaEntry;
			this.propertyValuesFromServer = propertyValuesFromServer;
			this.propertiesFromSchemaContainerInitialized = true;
			this.propertyEntry = this.GetSchemaPropertyDirectoryEntry();
			this.commonName = commonName;
			this.ldapDisplayName = (string)this.GetValueFromCache(PropertyManager.LdapDisplayName, true);
			this.isDefunctOnServer = true;
			this.isDefunct = this.isDefunctOnServer;
			this.isBound = true;
		}

		internal ActiveDirectorySchemaProperty(DirectoryContext context, string commonName, string ldapDisplayName, DirectoryEntry propertyEntry, DirectoryEntry schemaEntry)
		{
			this.syntax = ActiveDirectorySyntax.CaseIgnoreString | ActiveDirectorySyntax.NumericString | ActiveDirectorySyntax.DirectoryString | ActiveDirectorySyntax.OctetString | ActiveDirectorySyntax.SecurityDescriptor | ActiveDirectorySyntax.Int | ActiveDirectorySyntax.Int64 | ActiveDirectorySyntax.Bool | ActiveDirectorySyntax.Oid | ActiveDirectorySyntax.GeneralizedTime | ActiveDirectorySyntax.UtcTime | ActiveDirectorySyntax.DN | ActiveDirectorySyntax.DNWithBinary | ActiveDirectorySyntax.DNWithString | ActiveDirectorySyntax.Enumeration | ActiveDirectorySyntax.IA5String | ActiveDirectorySyntax.PrintableString | ActiveDirectorySyntax.Sid | ActiveDirectorySyntax.AccessPointDN | ActiveDirectorySyntax.ORName | ActiveDirectorySyntax.PresentationAddress | ActiveDirectorySyntax.ReplicaLink;
			this.rangeLower = null;
			this.rangeUpper = null;
			this.linkId = null;
			this.context = context;
			this.schemaEntry = schemaEntry;
			this.propertyEntry = propertyEntry;
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
					if (this.propertyEntry != null)
					{
						this.propertyEntry.Dispose();
						this.propertyEntry = null;
					}
					if (this.abstractPropertyEntry != null)
					{
						this.abstractPropertyEntry.Dispose();
						this.abstractPropertyEntry = null;
					}
					if (this.schema != null)
					{
						this.schema.Dispose();
					}
				}
				this.disposed = true;
			}
		}

		public static ActiveDirectorySchemaProperty FindByName(DirectoryContext context, string ldapDisplayName)
		{
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
								context = new DirectoryContext(context);
								ActiveDirectorySchemaProperty activeDirectorySchemaProperty = new ActiveDirectorySchemaProperty(context, ldapDisplayName, (DirectoryEntry)null, (DirectoryEntry)null);
								return activeDirectorySchemaProperty;
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

		public DirectoryEntry GetDirectoryEntry()
		{
			this.CheckIfDisposed();
			if (this.isBound)
			{
				this.GetSchemaPropertyDirectoryEntry();
				return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.propertyEntry.Path);
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("CannotGetObject"));
			}
		}

		internal static SearchResult GetPropertiesFromSchemaContainer(DirectoryContext context, DirectoryEntry schemaEntry, string name, bool isDefunctOnServer)
		{
			string[] distinguishedName;
			SearchResult searchResult = null;
			StringBuilder stringBuilder = new StringBuilder(15);
			stringBuilder.Append("(&(");
			stringBuilder.Append(PropertyManager.ObjectCategory);
			stringBuilder.Append("=attributeSchema)");
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
			if (isDefunctOnServer)
			{
				distinguishedName = new string[15];
				distinguishedName[0] = PropertyManager.DistinguishedName;
				distinguishedName[1] = PropertyManager.Cn;
				distinguishedName[2] = PropertyManager.AttributeSyntax;
				distinguishedName[3] = PropertyManager.OMSyntax;
				distinguishedName[4] = PropertyManager.OMObjectClass;
				distinguishedName[5] = PropertyManager.Description;
				distinguishedName[6] = PropertyManager.SearchFlags;
				distinguishedName[7] = PropertyManager.IsMemberOfPartialAttributeSet;
				distinguishedName[8] = PropertyManager.LinkID;
				distinguishedName[9] = PropertyManager.SchemaIDGuid;
				distinguishedName[10] = PropertyManager.AttributeID;
				distinguishedName[11] = PropertyManager.IsSingleValued;
				distinguishedName[12] = PropertyManager.RangeLower;
				distinguishedName[13] = PropertyManager.RangeUpper;
				distinguishedName[14] = PropertyManager.LdapDisplayName;
			}
			else
			{
				distinguishedName = new string[12];
				distinguishedName[0] = PropertyManager.DistinguishedName;
				distinguishedName[1] = PropertyManager.Cn;
				distinguishedName[2] = PropertyManager.AttributeSyntax;
				distinguishedName[3] = PropertyManager.OMSyntax;
				distinguishedName[4] = PropertyManager.OMObjectClass;
				distinguishedName[5] = PropertyManager.Description;
				distinguishedName[6] = PropertyManager.SearchFlags;
				distinguishedName[7] = PropertyManager.IsMemberOfPartialAttributeSet;
				distinguishedName[8] = PropertyManager.LinkID;
				distinguishedName[9] = PropertyManager.SchemaIDGuid;
				distinguishedName[10] = PropertyManager.RangeLower;
				distinguishedName[11] = PropertyManager.RangeUpper;
			}
			ADSearcher aDSearcher = new ADSearcher(schemaEntry, stringBuilder.ToString(), distinguishedName, SearchScope.OneLevel, false, false);
			try
			{
				searchResult = aDSearcher.FindOne();
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
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), name);
				}
			}
			if (searchResult != null)
			{
				return searchResult;
			}
			else
			{
				throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), name);
			}
		}

		internal DirectoryEntry GetSchemaPropertyDirectoryEntry()
		{
			if (this.propertyEntry == null)
			{
				this.InitializePropertiesFromSchemaContainer();
				this.propertyEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, (string)this.GetValueFromCache(PropertyManager.DistinguishedName, true));
			}
			return this.propertyEntry;
		}

		private object GetValueFromCache(string propertyName, bool mustExist)
		{
			object item = null;
			this.InitializePropertiesFromSchemaContainer();
			try
			{
				ResultPropertyValueCollection resultPropertyValueCollection = this.propertyValuesFromServer.Properties[propertyName];
				if (resultPropertyValueCollection == null || resultPropertyValueCollection.Count < 1)
				{
					if (mustExist)
					{
						object[] objArray = new object[1];
						objArray[0] = propertyName;
						throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", objArray));
					}
				}
				else
				{
					item = resultPropertyValueCollection[0];
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
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
				ActiveDirectorySchemaProperty propertiesFromSchemaContainer = this;
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
				propertiesFromSchemaContainer.propertyValuesFromServer = ActiveDirectorySchemaProperty.GetPropertiesFromSchemaContainer(directoryContext, directoryEntry, str, this.isDefunctOnServer);
				this.propertiesFromSchemaContainerInitialized = true;
			}
		}

		private void InitializeSearchFlags()
		{
			if (this.isBound && !this.searchFlagsInitialized)
			{
				object valueFromCache = this.GetValueFromCache(PropertyManager.SearchFlags, false);
				if (valueFromCache != null)
				{
					this.searchFlags = (SearchFlags)((int)valueFromCache);
				}
				this.searchFlagsInitialized = true;
			}
		}

		private bool IsSetInSearchFlags(SearchFlags searchFlagBit)
		{
			this.InitializeSearchFlags();
			return (this.searchFlags & searchFlagBit) != SearchFlags.None;
		}

		private ActiveDirectorySyntax MapSyntax(string syntaxId, int oMID, OMObjectClass oMObjectClass)
		{
			int num = 0;
			while (num < ActiveDirectorySchemaProperty.SyntaxesCount)
			{
				if (!ActiveDirectorySchemaProperty.syntaxes[num].Equals(new Syntax(syntaxId, oMID, oMObjectClass)))
				{
					num++;
				}
				else
				{
					return (ActiveDirectorySyntax)num;
				}
			}
			object[] objArray = new object[1];
			objArray[0] = this.ldapDisplayName;
			throw new ActiveDirectoryOperationException(Res.GetString("UnknownSyntax", objArray));
		}

		private void ResetBitInSearchFlags(SearchFlags searchFlagBit)
		{
			this.InitializeSearchFlags();
			this.searchFlags = this.searchFlags & ~searchFlagBit;
			if (this.isBound)
			{
				this.GetSchemaPropertyDirectoryEntry();
				this.propertyEntry.Properties[PropertyManager.SearchFlags].Value = (int)this.searchFlags;
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
					this.propertyEntry = this.schemaEntry.Children.Add(escapedPath, "attributeSchema");
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
				this.SetProperty(PropertyManager.AttributeID, this.oid);
				if (this.syntax != (ActiveDirectorySyntax.CaseIgnoreString | ActiveDirectorySyntax.NumericString | ActiveDirectorySyntax.DirectoryString | ActiveDirectorySyntax.OctetString | ActiveDirectorySyntax.SecurityDescriptor | ActiveDirectorySyntax.Int | ActiveDirectorySyntax.Int64 | ActiveDirectorySyntax.Bool | ActiveDirectorySyntax.Oid | ActiveDirectorySyntax.GeneralizedTime | ActiveDirectorySyntax.UtcTime | ActiveDirectorySyntax.DN | ActiveDirectorySyntax.DNWithBinary | ActiveDirectorySyntax.DNWithString | ActiveDirectorySyntax.Enumeration | ActiveDirectorySyntax.IA5String | ActiveDirectorySyntax.PrintableString | ActiveDirectorySyntax.Sid | ActiveDirectorySyntax.AccessPointDN | ActiveDirectorySyntax.ORName | ActiveDirectorySyntax.PresentationAddress | ActiveDirectorySyntax.ReplicaLink))
				{
					this.SetSyntax(this.syntax);
				}
				this.SetProperty(PropertyManager.Description, this.description);
				this.propertyEntry.Properties[PropertyManager.IsSingleValued].Value = this.isSingleValued;
				this.propertyEntry.Properties[PropertyManager.IsMemberOfPartialAttributeSet].Value = this.isInGlobalCatalog;
				this.propertyEntry.Properties[PropertyManager.IsDefunct].Value = this.isDefunct;
				if (this.rangeLower.HasValue)
				{
					this.propertyEntry.Properties[PropertyManager.RangeLower].Value = this.rangeLower.Value;
				}
				if (this.rangeUpper.HasValue)
				{
					this.propertyEntry.Properties[PropertyManager.RangeUpper].Value = this.rangeUpper.Value;
				}
				if (this.searchFlags != SearchFlags.None)
				{
					this.propertyEntry.Properties[PropertyManager.SearchFlags].Value = (int)this.searchFlags;
				}
				if (this.linkId.HasValue)
				{
					this.propertyEntry.Properties[PropertyManager.LinkID].Value = this.linkId.Value;
				}
				if (this.schemaGuidBinaryForm != null)
				{
					this.SetProperty(PropertyManager.SchemaIDGuid, this.schemaGuidBinaryForm);
				}
			}
			try
			{
				this.propertyEntry.CommitChanges();
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
			this.syntaxInitialized = false;
			this.descriptionInitialized = false;
			this.isSingleValuedInitialized = false;
			this.isInGlobalCatalogInitialized = false;
			this.rangeLowerInitialized = false;
			this.rangeUpperInitialized = false;
			this.searchFlagsInitialized = false;
			this.linkedPropertyInitialized = false;
			this.linkIdInitialized = false;
			this.schemaGuidBinaryForm = null;
			this.propertiesFromSchemaContainerInitialized = false;
			this.isBound = true;
		}

		private void SetBitInSearchFlags(SearchFlags searchFlagBit)
		{
			this.InitializeSearchFlags();
			this.searchFlags = this.searchFlags | searchFlagBit;
			if (this.isBound)
			{
				this.GetSchemaPropertyDirectoryEntry();
				this.propertyEntry.Properties[PropertyManager.SearchFlags].Value = (int)this.searchFlags;
			}
		}

		private void SetProperty(string propertyName, object value)
		{
			this.GetSchemaPropertyDirectoryEntry();
			if (value != null)
			{
				this.propertyEntry.Properties[propertyName].Value = value;
			}
			else
			{
				if (this.propertyEntry.Properties.Contains(propertyName))
				{
					this.propertyEntry.Properties[propertyName].Clear();
					return;
				}
			}
		}

		private void SetSyntax(ActiveDirectorySyntax syntax)
		{
			if (syntax < ActiveDirectorySyntax.CaseExactString || syntax > (ActiveDirectorySyntax)(ActiveDirectorySchemaProperty.SyntaxesCount - 1))
			{
				throw new InvalidEnumArgumentException("syntax", (int)syntax, typeof(ActiveDirectorySyntax));
			}
			else
			{
				this.GetSchemaPropertyDirectoryEntry();
				this.propertyEntry.Properties[PropertyManager.AttributeSyntax].Value = ActiveDirectorySchemaProperty.syntaxes[(int)syntax].attributeSyntax;
				this.propertyEntry.Properties[PropertyManager.OMSyntax].Value = ActiveDirectorySchemaProperty.syntaxes[(int)syntax].oMSyntax;
				OMObjectClass oMObjectClass = ActiveDirectorySchemaProperty.syntaxes[(int)syntax].oMObjectClass;
				if (oMObjectClass != null)
				{
					this.propertyEntry.Properties[PropertyManager.OMObjectClass].Value = oMObjectClass.Data;
				}
				return;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Name;
		}
	}
}