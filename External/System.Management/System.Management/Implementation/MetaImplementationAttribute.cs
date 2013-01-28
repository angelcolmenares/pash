using System;

namespace System.Management
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MetaImplementationAttribute : Attribute
	{
		public MetaImplementationAttribute (Type type)
		{
			ImplementationType = type;
		}

		public Type ImplementationType {
			get;
			private set;
		}
	}
}

