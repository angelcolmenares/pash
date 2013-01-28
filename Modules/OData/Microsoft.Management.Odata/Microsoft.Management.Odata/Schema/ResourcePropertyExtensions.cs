using Microsoft.Management.Odata.Core;
using Microsoft.Management.Odata.PS;
using System;
using System.Data.Services.Providers;

namespace Microsoft.Management.Odata.Schema
{
	internal static class ResourcePropertyExtensions
	{
		public static PropertyCustomState GetCustomState(this ResourceProperty resourceProperty)
		{
			return resourceProperty.CustomState as PropertyCustomState;
		}

		public static ReferenceCustomState GetReferenceCustomState(this ResourceProperty resourceProperty)
		{
			return resourceProperty.CustomState as ReferenceCustomState;
		}

		public static bool IsKeyProperty(this ResourceProperty resourceProperty)
		{
			if (resourceProperty != null)
			{
				return (resourceProperty.Kind & ResourcePropertyKind.Key) == ResourcePropertyKind.Key;
			}
			else
			{
				return false;
			}
		}

		public static bool IsNavPropertyHasGetReferenceCmdlet(this ResourceProperty resourceProperty, EntityMetadata entityMetadata)
		{
			if (resourceProperty.IsReferenceSetProperty())
			{
				PSEntityMetadata pSEntityMetadatum = entityMetadata as PSEntityMetadata;
				PSEntityMetadata.ReferenceSetCmdlets referenceSetCmdlet = null;
				if (!pSEntityMetadatum.CmdletsForReferenceSets.TryGetValue(resourceProperty.Name, out referenceSetCmdlet) || !referenceSetCmdlet.Cmdlets.ContainsKey(CommandType.GetReference))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool IsReferenceSetProperty(this ResourceProperty resourceProperty)
		{
			if (resourceProperty != null)
			{
				return (resourceProperty.Kind & ResourcePropertyKind.ResourceSetReference) == ResourcePropertyKind.ResourceSetReference;
			}
			else
			{
				return false;
			}
		}
	}
}