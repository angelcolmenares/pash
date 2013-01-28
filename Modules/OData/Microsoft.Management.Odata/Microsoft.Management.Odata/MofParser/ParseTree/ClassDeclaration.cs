using Microsoft.Management.Odata.MofParser;
using System;
using System.Text;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal class ClassDeclaration : MofProduction, IQualifierTarget
	{
		private readonly ClassName m_className;

		private readonly AliasIdentifier m_alias;

		private readonly ClassName m_superclassName;

		private readonly QualifierList m_qualifiers;

		private readonly ClassFeatureList m_classFeatures;

		public AliasIdentifier Alias
		{
			get
			{
				return this.m_alias;
			}
		}

		public bool IsAbstract
		{
			get
			{
				return this.m_qualifiers.ContainsAbstractQualifier;
			}
		}

		public NodeList<ClassFeature> Members
		{
			get
			{
				return this.m_classFeatures;
			}
		}

		public ClassName Name
		{
			get
			{
				return this.m_className;
			}
		}

		public NodeList<PropertyDeclaration> Properties
		{
			get
			{
				return this.m_classFeatures.Properties;
			}
		}

		public NodeList<Qualifier> Qualifiers
		{
			get
			{
				return this.m_qualifiers;
			}
		}

		public ClassName SuperclassName
		{
			get
			{
				return this.m_superclassName;
			}
		}

		public override MofProduction.ProductionType Type
		{
			get
			{
				return MofProduction.ProductionType.ClassDeclaration;
			}
		}

		internal ClassDeclaration(DocumentRange location, ClassName className, AliasIdentifier alias, ClassName superclassName, QualifierList qualifiers, ClassFeatureList classFeatures) : base(location)
		{
			this.m_className = className;
			this.m_alias = alias;
			this.m_superclassName = superclassName;
			this.m_qualifiers = qualifiers;
			this.m_classFeatures = classFeatures;
			qualifiers.SetParent(this);
			classFeatures.SetParent(this);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string str = this.m_qualifiers.ToString();
			if (str.Length > 0)
			{
				stringBuilder.AppendLine(str);
			}
			stringBuilder.Append("class ");
			stringBuilder.Append(this.m_className);
			if (this.m_alias != null)
			{
				stringBuilder.Append(" as ");
				stringBuilder.Append(this.m_alias);
			}
			if (this.m_superclassName != null)
			{
				stringBuilder.Append(" : ");
				stringBuilder.Append(this.m_superclassName);
			}
			if (this.m_classFeatures.Count <= 0)
			{
				stringBuilder.Append("{ ");
			}
			else
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("{");
				foreach (ClassFeature mClassFeature in this.m_classFeatures)
				{
					stringBuilder.Append("  ");
					stringBuilder.AppendLine(mClassFeature.ToString());
				}
			}
			stringBuilder.Append("};");
			return stringBuilder.ToString();
		}
	}
}