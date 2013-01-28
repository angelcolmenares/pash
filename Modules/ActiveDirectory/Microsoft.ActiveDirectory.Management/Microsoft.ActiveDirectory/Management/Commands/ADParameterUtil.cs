using System;
using System.Collections;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ADParameterUtil
	{
		internal static bool ShouldIgnorePipelineValue(object value)
		{
			if (value != null)
			{
				string str = value as string;
				if (str == null || str.Length != 0)
				{
					ICollection collections = value as ICollection;
					if (collections == null || collections.Count != 0)
					{
						Array arrays = value as Array;
						if (arrays != null && arrays.Length == 1)
						{
							str = value as string;
							if (str != null && str.Length == 0)
							{
								return true;
							}
						}
						return false;
					}
					else
					{
						return true;
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}
	}
}