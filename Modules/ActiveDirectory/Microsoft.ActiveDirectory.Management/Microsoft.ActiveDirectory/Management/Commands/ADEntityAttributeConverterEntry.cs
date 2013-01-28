using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADEntityAttributeConverterEntry : AttributeConverterEntry
	{
		private string[] _customObjectProperties;

		internal string[] CustomObjectProperties
		{
			get
			{
				return this._customObjectProperties;
			}
		}

		internal ADEntityAttributeConverterEntry(string extendedAttribute, string directoryAttribute, string customObjectProperty, Type attributeType, bool copyable, TypeAdapterAccess adapterAccessLevel, bool isSingleValue, AttributeSet attributeSet, ToExtendedFormatDelegate toExtended, ToDirectoryFormatDelegate toDirectory, ToSearchFilterDelegate toSearch)
		  	: this(extendedAttribute, new string[] { directoryAttribute }, new string[] { customObjectProperty }, attributeType, copyable, adapterAccessLevel, isSingleValue, attributeSet, toExtended, toDirectory, toSearch)
		{
		}

		internal ADEntityAttributeConverterEntry(string extendedAttribute, string[] directoryAttribute, string[] customObjectProperties, Type attributeType, bool copyable, TypeAdapterAccess adapterAccessLevel, bool isSingleValue, AttributeSet attributeSet, ToExtendedFormatDelegate toExtended, ToDirectoryFormatDelegate toDirectory, ToSearchFilterDelegate toSearch) : base(extendedAttribute, directoryAttribute, attributeType, copyable, adapterAccessLevel, isSingleValue, attributeSet, toExtended, toDirectory, toSearch)
		{
			this._customObjectProperties = customObjectProperties;
		}

		internal override void InvokeToExtendedConverter(ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			this._toExtendedDelegate(base.ExtendedAttribute, this.CustomObjectProperties, userObj, directoryObj, cmdletSessionInfo);
		}
	}
}