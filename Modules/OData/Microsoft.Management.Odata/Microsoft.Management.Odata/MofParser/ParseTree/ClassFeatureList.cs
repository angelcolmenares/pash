using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal class ClassFeatureList : NodeList<ClassFeature>
	{
		private NodeList<PropertyDeclaration> m_properties;

		public ClassDeclaration ContainingClass
		{
			get
			{
				return (ClassDeclaration)base.Parent;
			}
		}

		public NodeList<PropertyDeclaration> Properties
		{
			get
			{
				if (this.m_properties == null)
				{
					this.m_properties = base.GetFilteredList<PropertyDeclaration>();
				}
				return this.m_properties;
			}
		}

		internal ClassFeatureList(ClassFeature[] features) : base(features)
		{
			ClassFeature[] classFeatureArray = features;
			for (int i = 0; i < (int)classFeatureArray.Length; i++)
			{
				ClassFeature classFeature = classFeatureArray[i];
				classFeature.SetParent(this);
			}
		}
	}
}