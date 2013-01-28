using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal abstract class CIMDateTime
	{
		public abstract bool IsInterval
		{
			get;
		}

		public abstract bool IsTimestamp
		{
			get;
		}

		internal CIMDateTime()
		{
		}
	}
}