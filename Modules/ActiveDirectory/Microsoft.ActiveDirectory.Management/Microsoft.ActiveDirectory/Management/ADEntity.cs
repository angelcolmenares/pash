using Microsoft.ActiveDirectory.Management.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.ActiveDirectory.Management
{
	public class ADEntity : ADPropertyCollection
	{
		private static Dictionary<Type, AdapterTableEntry> _adapterTables;

		private ADPropertyCollection _internalProperties;

		private ADSessionInfo _sessionInfo;

		private object _identity;

		private bool _isSearchResult;

		private ADSchemaUtil _schemaUtil;

		internal virtual string IdentifyingString
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		internal virtual object Identity
		{
			get
			{
				return this._identity;
			}
			set
			{
				this._identity = value;
			}
		}

		internal ADPropertyCollection InternalProperties
		{
			get
			{
				if (this._internalProperties == null)
				{
					this._internalProperties = new ADPropertyCollection();
				}
				return this._internalProperties;
			}
		}

		internal bool IsSearchResult
		{
			get
			{
				return this._isSearchResult;
			}
			set
			{
				this._isSearchResult = value;
			}
		}

		internal virtual ADSessionInfo SessionInfo
		{
			get
			{
				return this._sessionInfo;
			}
			set
			{
				this._sessionInfo = value;
				this._schemaUtil = null;
			}
		}

		static ADEntity()
		{
			ADEntity._adapterTables = new Dictionary<Type, AdapterTableEntry>();
		}

		internal ADEntity()
		{
		}

		public ADEntity(string identity)
		{
			this._identity = identity;
		}

		private AttributeConverterEntry GetConverterEntry(string propertyName)
		{
			Type type = base.GetType();
			if (ADEntity._adapterTables == null || !ADEntity._adapterTables.ContainsKey(type) || this._sessionInfo == null || this._sessionInfo.ServerType == ADServerType.Unknown)
			{
				return null;
			}
			else
			{
				return ADEntity._adapterTables[type].GetConverterEntry(propertyName, this._sessionInfo.ServerType);
			}
		}

		internal virtual PropertyInfo GetDotNetProperty(string propertyName)
		{
			Type type = base.GetType();
			return ADEntity._adapterTables[type].GetProperty(propertyName);
		}

		internal virtual Type GetPropertyType(string propertyName)
		{
			AttributeConverterEntry converterEntry = this.GetConverterEntry(propertyName);
			if (converterEntry == null)
			{
				if (this._sessionInfo == null)
				{
					return typeof(object);
				}
				else
				{
					this.InitSchemaUtil(this._sessionInfo);
					return this._schemaUtil.GetAttributeDotNetType(propertyName, typeof(object));
				}
			}
			else
			{
				return converterEntry.AttributeType;
			}
		}

		internal virtual bool HasMethod(string methodName)
		{
			Type type = base.GetType();
			return ADEntity._adapterTables[type].HasMethod(methodName);
		}

		private void InitSchemaUtil(ADSessionInfo sessionInfo)
		{
			if (this._sessionInfo != null && this._schemaUtil == null)
			{
				this._schemaUtil = new ADSchemaUtil(sessionInfo);
			}
		}

		internal virtual bool? IsOfType(string objectType)
		{
			throw new NotImplementedException();
		}

		internal virtual bool PropertyIsReadable(string propertyName)
		{
			AttributeConverterEntry converterEntry = this.GetConverterEntry(propertyName);
			if (converterEntry == null)
			{
				return true;
			}
			else
			{
				TypeAdapterAccess adapterAccessLevel = converterEntry.AdapterAccessLevel;
				if (adapterAccessLevel == TypeAdapterAccess.Read)
				{
					return true;
				}
				else
				{
					return adapterAccessLevel == TypeAdapterAccess.ReadWrite;
				}
			}
		}

		internal virtual bool PropertyIsSingleValue(string propertyName)
		{
			AttributeConverterEntry converterEntry = this.GetConverterEntry(propertyName);
			if (converterEntry == null)
			{
				if (this._sessionInfo == null)
				{
					return false;
				}
				else
				{
					this.InitSchemaUtil(this._sessionInfo);
					return this._schemaUtil.AttributeIsSingleValue(propertyName);
				}
			}
			else
			{
				return converterEntry.IsSingleValue;
			}
		}

		internal virtual bool PropertyIsWritable(string propertyName)
		{
			AttributeConverterEntry converterEntry = this.GetConverterEntry(propertyName);
			if (converterEntry == null)
			{
				if (this._sessionInfo == null)
				{
					return true;
				}
				else
				{
					this.InitSchemaUtil(this._sessionInfo);
					return this._schemaUtil.AttributeIsWritable(propertyName);
				}
			}
			else
			{
				TypeAdapterAccess adapterAccessLevel = converterEntry.AdapterAccessLevel;
				if (adapterAccessLevel == TypeAdapterAccess.Write)
				{
					return true;
				}
				else
				{
					return adapterAccessLevel == TypeAdapterAccess.ReadWrite;
				}
			}
		}

		internal static void RegisterMappingTable(Type t, IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> attributeTable)
		{
			if (!ADEntity._adapterTables.ContainsKey(t))
			{
				ADEntity._adapterTables.Add(t, new AdapterTableEntry(t, attributeTable));
				return;
			}
			else
			{
				ADEntity._adapterTables[t].MergeAdapterTableEntry(attributeTable);
				return;
			}
		}
	}
}