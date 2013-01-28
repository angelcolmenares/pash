using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class PragmaNamespace : CompilerDirective
	{
		private readonly string m_namespaceName;

		public string Namespace
		{
			get
			{
				return this.m_namespaceName;
			}
		}

		internal PragmaNamespace(DocumentRange range, string namespaceName) : base(range, "Namespace")
		{
			this.m_namespaceName = namespaceName;
		}

		public override string ToString()
		{
			return string.Concat("#pragma Namespace(", MofDataType.QuoteAndEscapeString(this.Namespace), ")");
		}
	}
}