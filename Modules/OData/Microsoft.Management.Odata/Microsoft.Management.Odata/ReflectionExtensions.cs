using System;
using System.Reflection;

namespace Microsoft.Management.Odata
{
	internal static class ReflectionExtensions
	{
		public static object GetValue(this MemberInfo member, object instance)
		{
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Field)
			{
				return ((FieldInfo)member).GetValue(instance);
			}
			else
			{
				if (memberType != MemberTypes.Property)
				{
					throw new InvalidOperationException();
				}
				else
				{
					return ((PropertyInfo)member).GetValue(instance, null);
				}
			}
		}
	}
}