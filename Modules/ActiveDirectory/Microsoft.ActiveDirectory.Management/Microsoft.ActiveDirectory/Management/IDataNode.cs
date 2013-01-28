using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IDataNode
	{
		object DataObject
		{
			get;
		}

		bool? EncodeAsteriskChar
		{
			get;
			set;
		}

	}
}