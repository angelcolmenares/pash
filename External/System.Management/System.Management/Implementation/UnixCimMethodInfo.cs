using System;
using System.Collections.Generic;

namespace System.Management
{
	internal struct UnixCimMethodInfo
	{
		public string Name
		{
			get;
			set;
		}

		public string InSignatureType
		{
			get;set;
		}

		public string OutSignatureType
		{
			get;set;
		}

		public IEnumerable<UnixWbemPropertyInfo> InProperties
		{
			get;set;
		}

		
		public IEnumerable<UnixWbemPropertyInfo> OutProperties
		{
			get;set;
		}

	}
}

