using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class Qualifier : ParseTreeNode
	{
		private readonly string m_name;

		private readonly object m_parameter;

		private readonly Flavor m_flavors;

		private object m_parent;

		public Flavor Flavors
		{
			get
			{
				return this.m_flavors;
			}
		}

		public string Name
		{
			get
			{
				return this.m_name;
			}
		}

		public object Parameter
		{
			get
			{
				return this.m_parameter;
			}
		}

		public object Parent
		{
			get
			{
				return this.m_parent;
			}
		}

		internal Qualifier(DocumentRange location, string name, object parameter, Flavor flavors) : base(location)
		{
			this.m_name = name;
			this.m_parameter = parameter;
			this.m_flavors = flavors;
		}

		internal void SetParent(object parent)
		{
			this.m_parent = parent;
		}

		public override string ToString()
		{
			string str = "";
			if (this.m_parameter == null)
			{
				return string.Format("{0}{1}", this.m_name, str);
			}
			else
			{
				return string.Format("{0}({1}){2}", this.m_name, MofDataType.QuoteAndEscapeIfString(this.m_parameter), str);
			}
		}
	}
}