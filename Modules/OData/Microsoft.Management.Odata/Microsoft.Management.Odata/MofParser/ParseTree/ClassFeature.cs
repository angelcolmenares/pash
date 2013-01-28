using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal abstract class ClassFeature : ParseTreeNode, IQualifierTarget
	{
		private ClassFeatureList m_parent;

		public ClassDeclaration ContainingClass
		{
			get
			{
				return this.Parent.ContainingClass;
			}
		}

		public ClassFeatureList Parent
		{
			get
			{
				return this.m_parent;
			}
		}

		public abstract NodeList<Qualifier> Qualifiers
		{
			get;
		}

		public abstract ClassFeature.FeatureType Type
		{
			get;
		}

		internal ClassFeature(DocumentRange location) : base(location)
		{
		}

		internal void SetParent(ClassFeatureList parent)
		{
			this.m_parent = parent;
		}

		public enum FeatureType
		{
			None,
			Method,
			Property,
			Reference
		}
	}
}