using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal class XmlAnnotationInfo
	{
		internal bool IsAttribute
		{
			get;
			private set;
		}

		internal CsdlLocation Location
		{
			get;
			private set;
		}

		internal string Name
		{
			get;
			private set;
		}

		internal string NamespaceName
		{
			get;
			private set;
		}

		internal string Value
		{
			get;
			private set;
		}

		internal XmlAnnotationInfo(CsdlLocation location, string namespaceName, string name, string value, bool isAttribute)
		{
			this.Location = location;
			this.NamespaceName = namespaceName;
			this.Name = name;
			this.Value = value;
			this.IsAttribute = isAttribute;
		}
	}
}