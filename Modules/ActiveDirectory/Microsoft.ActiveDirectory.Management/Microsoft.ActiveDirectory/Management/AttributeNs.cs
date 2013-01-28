using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management
{
	internal class AttributeNs
	{
		private static Dictionary<string, SyntheticAttributeOperation> _SyntheticNsDict;

		static AttributeNs()
		{
			AttributeNs._SyntheticNsDict = new Dictionary<string, SyntheticAttributeOperation>(5);
			AttributeNs._SyntheticNsDict.Add("objectReferenceProperty", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("container-hierarchy-parent", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("relativeDistinguishedName", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("ad:all", SyntheticAttributeOperation.Read | SyntheticAttributeOperation.Write);
			AttributeNs._SyntheticNsDict.Add("distinguishedName", SyntheticAttributeOperation.Read);
		}

		private AttributeNs()
		{
		}

		public static bool IsSynthetic(string attribute, SyntheticAttributeOperation operation)
		{
			SyntheticAttributeOperation syntheticAttributeOperation = 0;
			if (!AttributeNs._SyntheticNsDict.TryGetValue(attribute, out syntheticAttributeOperation))
			{
				return false;
			}
			else
			{
				return operation == (syntheticAttributeOperation & operation);
			}
		}

		public static bool IsSynthetic(string attribute, SyntheticAttributeOperation operation, ref bool hasPrefix)
		{
			hasPrefix = false;
			if (!AttributeNs.IsSynthetic(attribute, operation))
			{
				return false;
			}
			else
			{
				if (string.Equals(attribute, "ad:all", StringComparison.Ordinal))
				{
					hasPrefix = true;
				}
				return true;
			}
		}

		public static string LookupNs(string attribute, SyntheticAttributeOperation operation)
		{
			SyntheticAttributeOperation syntheticAttributeOperation = 0;
			if (!AttributeNs._SyntheticNsDict.TryGetValue(attribute, out syntheticAttributeOperation))
			{
				return "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data";
			}
			else
			{
				if (!string.Equals(attribute, "ad:all", StringComparison.Ordinal))
				{
					if (operation == (syntheticAttributeOperation & operation))
					{
						return "http://schemas.microsoft.com/2008/1/ActiveDirectory";
					}
					else
					{
						return "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data";
					}
				}
				else
				{
					return string.Empty;
				}
			}
		}
	}
}