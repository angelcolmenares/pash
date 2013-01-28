using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class GlobalCatalog : DomainController
	{
		private ActiveDirectorySchema schema;

		private bool disabled;

		internal GlobalCatalog(DirectoryContext context, string globalCatalogName) : base(context, globalCatalogName)
		{
		}

		internal GlobalCatalog(DirectoryContext context, string globalCatalogName, DirectoryEntryManager directoryEntryMgr) : base(context, globalCatalogName, directoryEntryMgr)
		{
		}

		private void CheckIfDisabled()
		{
			if (!this.disabled)
			{
				return;
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("GCDisabled"));
			}
		}

		public DomainController DisableGlobalCatalog()
		{
			base.CheckIfDisposed();
			this.CheckIfDisabled();
			DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(base.NtdsaObjectName);
			int value = 0;
			try
			{
				if (cachedDirectoryEntry.Properties[PropertyManager.Options].Value != null)
				{
					value = (int)cachedDirectoryEntry.Properties[PropertyManager.Options].Value;
				}
				cachedDirectoryEntry.Properties[PropertyManager.Options].Value = value & -2;
				cachedDirectoryEntry.CommitChanges();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			this.disabled = true;
			return new DomainController(this.context, base.Name);
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override GlobalCatalog EnableGlobalCatalog()
		{
			base.CheckIfDisposed();
			throw new InvalidOperationException(Res.GetString("CannotPerformOnGCObject"));
		}

		public static GlobalCatalogCollection FindAll(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest)
				{
					context = new DirectoryContext(context);
					return GlobalCatalog.FindAllInternal(context, null);
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static GlobalCatalogCollection FindAll(DirectoryContext context, string siteName)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest)
				{
					if (siteName != null)
					{
						context = new DirectoryContext(context);
						return GlobalCatalog.FindAllInternal(context, siteName);
					}
					else
					{
						throw new ArgumentNullException("siteName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		internal static GlobalCatalogCollection FindAllInternal(DirectoryContext context, string siteName)
		{
			ArrayList arrayLists = new ArrayList();
			if (siteName == null || siteName.Length != 0)
			{
				foreach (string replicaList in Utils.GetReplicaList(context, null, siteName, false, false, true))
				{
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(replicaList, DirectoryContextType.DirectoryServer, context);
					arrayLists.Add(new GlobalCatalog(newDirectoryContext, replicaList));
				}
				return new GlobalCatalogCollection(arrayLists);
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
			}
		}

		public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllProperties()
		{
			base.CheckIfDisposed();
			this.CheckIfDisabled();
			if (this.schema == null)
			{
				string str = null;
				try
				{
					str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, this.context);
				this.schema = new ActiveDirectorySchema(this.context, str);
			}
			return this.schema.FindAllProperties(PropertyTypes.InGlobalCatalog);
		}

		public static GlobalCatalog FindOne(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest)
				{
					return GlobalCatalog.FindOneWithCredentialValidation(context, null, (LocatorOptions)((long)0));
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static GlobalCatalog FindOne(DirectoryContext context, string siteName)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest)
				{
					if (siteName != null)
					{
						return GlobalCatalog.FindOneWithCredentialValidation(context, siteName, (LocatorOptions)((long)0));
					}
					else
					{
						throw new ArgumentNullException("siteName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static GlobalCatalog FindOne(DirectoryContext context, LocatorOptions flag)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest)
				{
					return GlobalCatalog.FindOneWithCredentialValidation(context, null, flag);
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static GlobalCatalog FindOne(DirectoryContext context, string siteName, LocatorOptions flag)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest)
				{
					if (siteName != null)
					{
						return GlobalCatalog.FindOneWithCredentialValidation(context, siteName, flag);
					}
					else
					{
						throw new ArgumentNullException("siteName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		internal new static GlobalCatalog FindOneInternal(DirectoryContext context, string forestName, string siteName, LocatorOptions flag)
		{
			DomainControllerInfo domainControllerInfo = null;
			DomainControllerInfo domainControllerInfo1 = null;
			int num = 0;
			if (siteName == null || siteName.Length != 0)
			{
				if (((int)flag & -23554) == 0)
				{
					if (forestName == null)
					{
						int num1 = Locator.DsGetDcNameWrapper(null, DirectoryContext.GetLoggedOnDomain(), null, (long)16, out domainControllerInfo1);
						if (num1 != 0x54b)
						{
							if (num1 == 0)
							{
								forestName = domainControllerInfo1.DnsForestName;
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num);
							}
						}
						else
						{
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(GlobalCatalog), null);
						}
					}
					num = Locator.DsGetDcNameWrapper(null, forestName, siteName, (int)flag | 80, out domainControllerInfo);
					if (num != 0x54b)
					{
						if (num != 0x3ec)
						{
							if (num == 0)
							{
								string str = domainControllerInfo.DomainControllerName.Substring(2);
								DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, context);
								return new GlobalCatalog(newDirectoryContext, str);
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num);
							}
						}
						else
						{
							throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = forestName;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFoundInForest", objArray), typeof(GlobalCatalog), null);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
			}
		}

		internal static GlobalCatalog FindOneWithCredentialValidation(DirectoryContext context, string siteName, LocatorOptions flag)
		{
			bool flag1 = false;
			bool flag2 = false;
			context = new DirectoryContext(context);
			GlobalCatalog globalCatalog = GlobalCatalog.FindOneInternal(context, context.Name, siteName, flag);
			using (globalCatalog)
			{
				if (flag2)
				{
					try
					{
						DomainController.ValidateCredential(globalCatalog, context);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						if (cOMException.ErrorCode != -2147016646)
						{
							throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
						}
						else
						{
							if ((flag & LocatorOptions.ForceRediscovery) != 0)
							{
								object[] name = new object[1];
								name[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFoundInForest", name), typeof(GlobalCatalog), null);
							}
							else
							{
								flag1 = true;
							}
						}
					}
				}
			}
			if (flag1)
			{
				flag2 = false;
				globalCatalog = GlobalCatalog.FindOneInternal(context, context.Name, siteName, flag | LocatorOptions.ForceRediscovery);
				using (globalCatalog)
				{
					if (flag2)
					{
						try
						{
							DomainController.ValidateCredential(globalCatalog, context);
						}
						catch (COMException cOMException3)
						{
							COMException cOMException2 = cOMException3;
							if (cOMException2.ErrorCode != -2147016646)
							{
								throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException2);
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFoundInForest", objArray), typeof(GlobalCatalog), null);
							}
						}
					}
				}
			}
			return globalCatalog;
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override DirectorySearcher GetDirectorySearcher()
		{
			base.CheckIfDisposed();
			this.CheckIfDisabled();
			return this.InternalGetDirectorySearcher();
		}

		public static GlobalCatalog GetGlobalCatalog(DirectoryContext context)
		{
			string propertyValue = null;
			DirectoryEntryManager directoryEntryManager = null;
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.DirectoryServer)
				{
					if (context.isServer())
					{
						context = new DirectoryContext(context);
						try
						{
							directoryEntryManager = new DirectoryEntryManager(context);
							DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
							if (Utils.CheckCapability(directoryEntry, Capability.ActiveDirectory))
							{
								propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.DnsHostName);
								bool flag = bool.Parse((string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.IsGlobalCatalogReady));
								if (!flag)
								{
									object[] name = new object[1];
									name[0] = context.Name;
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", name), typeof(GlobalCatalog), context.Name);
								}
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", objArray), typeof(GlobalCatalog), context.Name);
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
								object[] name1 = new object[1];
								name1[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", name1), typeof(GlobalCatalog), context.Name);
							}
						}
						return new GlobalCatalog(context, propertyValue, directoryEntryManager);
					}
					else
					{
						object[] objArray1 = new object[1];
						objArray1[0] = context.Name;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", objArray1), typeof(GlobalCatalog), context.Name);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeGC"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		private DirectorySearcher InternalGetDirectorySearcher()
		{
			DirectoryEntry directoryEntry = new DirectoryEntry(string.Concat("GC://", base.Name));
			if (!DirectoryContext.ServerBindSupported)
			{
				directoryEntry.AuthenticationType = Utils.DefaultAuthType;
			}
			else
			{
				directoryEntry.AuthenticationType = Utils.DefaultAuthType | AuthenticationTypes.ServerBind;
			}
			directoryEntry.Username = this.context.UserName;
			directoryEntry.Password = this.context.Password;
			return new DirectorySearcher(directoryEntry);
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override bool IsGlobalCatalog()
		{
			base.CheckIfDisposed();
			this.CheckIfDisabled();
			return true;
		}
	}
}