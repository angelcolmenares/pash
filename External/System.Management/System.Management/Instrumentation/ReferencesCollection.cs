using System;
using System.Collections.Specialized;

namespace System.Management.Instrumentation
{
	internal class ReferencesCollection
	{
		private StringCollection namespaces;

		private StringCollection assemblies;

		private CodeWriter usingCode;

		public StringCollection Assemblies
		{
			get
			{
				return this.assemblies;
			}
		}

		public StringCollection Namespaces
		{
			get
			{
				return this.namespaces;
			}
		}

		public CodeWriter UsingCode
		{
			get
			{
				return this.usingCode;
			}
		}

		public ReferencesCollection()
		{
			this.namespaces = new StringCollection();
			this.assemblies = new StringCollection();
			this.usingCode = new CodeWriter();
		}

		public void Add(Type type)
		{
			if (!this.namespaces.Contains(type.Namespace))
			{
				this.namespaces.Add(type.Namespace);
				this.usingCode.Line(string.Format("using {0};", type.Namespace));
			}
			if (!this.assemblies.Contains(type.Assembly.Location))
			{
				this.assemblies.Add(type.Assembly.Location);
			}
		}
	}
}