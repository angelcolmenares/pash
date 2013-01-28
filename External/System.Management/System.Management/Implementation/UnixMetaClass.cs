using System;

namespace System.Management
{
	public class UnixMetaClass
	{
		public UnixMetaClass (Guid id, string nameSpace, string className, string implementationType)
		{
			Id = id;
			Namespace = nameSpace;
			ClassName = className;
			ImplementationType = implementationType;
		}

		public Guid Id 
		{
			get;
			private set;
		}

		public string Namespace
		{
			get;set;
		}

		public string ClassName
		{
			get; private set;
		}

		public string ImplementationType {
			get; private set;
		}
	}
}

