using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class AttributeConverterEntry : MappingTableEntry
	{
		protected ToExtendedFormatDelegate _toExtendedDelegate;

		protected ToDirectoryFormatDelegate _toDirectoryDelegate;

		protected ToSearchFilterDelegate _toSearchDelegate;

		protected string[] _directoryAttributes;

		protected AttributeSet _attributeSet;

		protected bool _copyable;

		protected TypeAdapterAccess _adapterAccessLevel;

		protected bool _isSingleValue;

		protected Type _attributeType;

		internal TypeAdapterAccess AdapterAccessLevel
		{
			get
			{
				return this._adapterAccessLevel;
			}
		}

		internal AttributeSet AttributeSet
		{
			get
			{
				return this._attributeSet;
			}
		}

		internal Type AttributeType
		{
			get
			{
				return this._attributeType;
			}
		}

		internal bool Copyable
		{
			get
			{
				return this._copyable;
			}
		}

		internal string[] DirectoryAttributes
		{
			get
			{
				return this._directoryAttributes;
			}
		}

		internal bool IsDirectoryConverterDefined
		{
			get
			{
				return this._toDirectoryDelegate != null;
			}
		}

		internal bool IsExtendedConverterDefined
		{
			get
			{
				return this._toExtendedDelegate != null;
			}
		}

		internal bool IsSearchConverterDefined
		{
			get
			{
				return this._toSearchDelegate != null;
			}
		}

		internal bool IsSingleValue
		{
			get
			{
				return this._isSingleValue;
			}
		}

		internal AttributeConverterEntry(string extendedAttribute, string directoryAttribute, Type attributeType, bool copyable, TypeAdapterAccess adapterAccessLevel, bool isSingleValue, AttributeSet attributeSet, ToExtendedFormatDelegate toExtended, ToDirectoryFormatDelegate toDirectory, ToSearchFilterDelegate toSearch)
			: this(extendedAttribute, new string [] { directoryAttribute }, attributeType, copyable, adapterAccessLevel, isSingleValue, attributeSet, toExtended, toDirectory, toSearch)
		{

		}

		internal AttributeConverterEntry(string extendedAttribute, string[] directoryAttribute, Type attributeType, bool copyable, TypeAdapterAccess adapterAccessLevel, bool isSingleValue, AttributeSet attributeSet, ToExtendedFormatDelegate toExtended, ToDirectoryFormatDelegate toDirectory, ToSearchFilterDelegate toSearch) : base(extendedAttribute)
		{
			this._toExtendedDelegate = toExtended;
			this._toDirectoryDelegate = toDirectory;
			this._directoryAttributes = directoryAttribute;
			this._attributeSet = attributeSet;
			this._toSearchDelegate = toSearch;
			this._copyable = copyable;
			this._adapterAccessLevel = adapterAccessLevel;
			this._isSingleValue = isSingleValue;
			this._attributeType = attributeType;
		}

		private AttributeConverterEntry() : base(null)
		{
		}

		internal virtual void InvokeToDirectoryConverter(ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			this._toDirectoryDelegate(base.ExtendedAttribute, this.DirectoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal virtual void InvokeToExtendedConverter(ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			this._toExtendedDelegate(base.ExtendedAttribute, this.DirectoryAttributes, userObj, directoryObj, cmdletSessionInfo);
		}

		internal virtual IADOPathNode InvokeToSearcherConverter(IADOPathNode filter, CmdletSessionInfo cmdletSessionInfo)
		{
			return this._toSearchDelegate(base.ExtendedAttribute, this.DirectoryAttributes, filter, cmdletSessionInfo);
		}
	}
}