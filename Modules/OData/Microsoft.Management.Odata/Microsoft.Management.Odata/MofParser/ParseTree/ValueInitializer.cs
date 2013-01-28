using System;
using System.Text;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class ValueInitializer
	{
		private readonly string m_propertyName;

		private readonly QualifierList m_qualifiers;

		private readonly object m_initializer;

		private object m_parent;

		public string Name
		{
			get
			{
				return this.m_propertyName;
			}
		}

		public object Parent
		{
			get
			{
				return this.m_parent;
			}
		}

		public NodeList<Qualifier> Qualifiers
		{
			get
			{
				return this.m_qualifiers;
			}
		}

		public object Value
		{
			get
			{
				return this.m_initializer;
			}
		}

		internal ValueInitializer(string propertyName, QualifierList qualifiers, object initializer)
		{
			this.m_propertyName = propertyName;
			this.m_qualifiers = qualifiers;
			this.m_initializer = initializer;
			this.m_qualifiers.SetParent(this);
		}

		internal void SetParent(object parent)
		{
			this.m_parent = parent;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string str = this.m_qualifiers.ToString();
			if (str.Length > 0)
			{
				stringBuilder.Append(str);
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(this.m_propertyName);
			if (this.m_initializer != null)
			{
				stringBuilder.Append(" = ");
				stringBuilder.Append(MofDataType.QuoteAndEscapeIfString(this.m_initializer));
			}
			stringBuilder.Append(";");
			return stringBuilder.ToString();
		}
	}
}