using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class AliasIdentifier
	{
		private readonly string m_aliasName;

		public string Name
		{
			get
			{
				return this.m_aliasName;
			}
		}

		internal AliasIdentifier(string aliasName)
		{
			this.m_aliasName = aliasName;
		}

		public override string ToString()
		{
			return string.Concat("$", this.m_aliasName);
		}
	}
}