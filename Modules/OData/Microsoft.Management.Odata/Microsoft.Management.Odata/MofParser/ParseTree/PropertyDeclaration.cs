using Microsoft.Management.Odata.MofParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class PropertyDeclaration : ClassFeature
	{
		private readonly string m_name;

		private readonly DataType m_DataType;

		private readonly object m_defaultValue;

		private readonly QualifierList m_qualifiers;

		public DataType DataType
		{
			get
			{
				return this.m_DataType;
			}
		}

		public object DefaultValue
		{
			get
			{
				return this.m_defaultValue;
			}
		}

		public string Name
		{
			get
			{
				return this.m_name;
			}
		}

		public static IEqualityComparer<PropertyDeclaration> NameEqualityComparer
		{
			get
			{
				return PropertyDeclaration.PropertyDeclarationComparer.Instance;
			}
		}

		public override NodeList<Qualifier> Qualifiers
		{
			get
			{
				return this.m_qualifiers;
			}
		}

		public override ClassFeature.FeatureType Type
		{
			get
			{
				return ClassFeature.FeatureType.Property;
			}
		}

		internal PropertyDeclaration(DocumentRange location, string name, DataType dataType, object defaultValue, QualifierList qualifiers) : base(location)
		{
			this.m_name = name;
			this.m_DataType = dataType;
			this.m_defaultValue = defaultValue;
			this.m_qualifiers = qualifiers;
			qualifiers.SetParent(this);
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
			ArrayType dataType = this.DataType as ArrayType;
			if (dataType != null)
			{
				stringBuilder.Append(dataType.ElementType);
			}
			else
			{
				stringBuilder.Append(this.DataType);
			}
			stringBuilder.Append(" ");
			stringBuilder.Append(this.Name);
			if (dataType != null)
			{
				stringBuilder.Append("[");
				int? length = dataType.Length;
				if (length.HasValue)
				{
					stringBuilder.Append(dataType.Length);
				}
				stringBuilder.Append("]");
			}
			if (this.m_defaultValue != null)
			{
				stringBuilder.Append(" = ");
				stringBuilder.Append(MofDataType.QuoteAndEscapeIfString(this.m_defaultValue));
			}
			stringBuilder.Append(";");
			return stringBuilder.ToString();
		}

		private sealed class PropertyDeclarationComparer : IEqualityComparer<PropertyDeclaration>
		{
			public static PropertyDeclaration.PropertyDeclarationComparer Instance;

			static PropertyDeclarationComparer()
			{
				PropertyDeclaration.PropertyDeclarationComparer.Instance = new PropertyDeclaration.PropertyDeclarationComparer();
			}

			public PropertyDeclarationComparer()
			{
			}

			public bool Equals(PropertyDeclaration x, PropertyDeclaration y)
			{
				return x.Name == y.Name;
			}

			public int GetHashCode(PropertyDeclaration obj)
			{
				return obj.Name.GetHashCode();
			}
		}
	}
}