using Microsoft.Management.Odata.MofParser;
using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class ReferenceDeclaration : ClassFeature
	{
		private readonly QualifierList m_qualifiers;

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
				return ClassFeature.FeatureType.Reference;
			}
		}

		internal ReferenceDeclaration(DocumentRange location, string name, ObjectReference reference, object defaultValue, QualifierList qualifiers) : base(location)
		{
			qualifiers.SetParent(this);
			this.m_qualifiers = qualifiers;
		}
	}
}