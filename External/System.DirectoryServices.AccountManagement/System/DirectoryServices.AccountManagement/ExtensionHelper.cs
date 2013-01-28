using System;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	internal class ExtensionHelper
	{
		private Principal p;

		internal string RdnPrefix
		{
			[SecurityCritical]
			get
			{
				bool hasValue;
				DirectoryRdnPrefixAttribute[] customAttributes = (DirectoryRdnPrefixAttribute[])Attribute.GetCustomAttributes(this.p.GetType(), typeof(DirectoryRdnPrefixAttribute), false);
				if (customAttributes != null)
				{
					string rdnPrefix = null;
					int num = 0;
					while (num < (int)customAttributes.Length)
					{
						ContextType? context = customAttributes[num].Context;
						if (!context.HasValue && rdnPrefix == null)
						{
							rdnPrefix = customAttributes[num].RdnPrefix;
						}
						ContextType contextType = this.p.ContextType;
						ContextType? nullable = customAttributes[num].Context;
						if (contextType != nullable.GetValueOrDefault())
						{
							hasValue = false;
						}
						else
						{
							hasValue = nullable.HasValue;
						}
						if (!hasValue)
						{
							num++;
						}
						else
						{
							return customAttributes[num].RdnPrefix;
						}
					}
					return rdnPrefix;
				}
				else
				{
					return null;
				}
			}
		}

		internal string StructuralObjectClass
		{
			[SecurityCritical]
			get
			{
				bool hasValue;
				DirectoryObjectClassAttribute[] customAttributes = (DirectoryObjectClassAttribute[])Attribute.GetCustomAttributes(this.p.GetType(), typeof(DirectoryObjectClassAttribute), false);
				if (customAttributes != null)
				{
					string objectClass = null;
					int num = 0;
					while (num < (int)customAttributes.Length)
					{
						ContextType? context = customAttributes[num].Context;
						if (!context.HasValue && objectClass == null)
						{
							objectClass = customAttributes[num].ObjectClass;
						}
						ContextType contextType = this.p.ContextType;
						ContextType? nullable = customAttributes[num].Context;
						if (contextType != nullable.GetValueOrDefault())
						{
							hasValue = false;
						}
						else
						{
							hasValue = nullable.HasValue;
						}
						if (!hasValue)
						{
							num++;
						}
						else
						{
							return customAttributes[num].ObjectClass;
						}
					}
					return objectClass;
				}
				else
				{
					return null;
				}
			}
		}

		internal ExtensionHelper(Principal p)
		{
			this.p = p;
		}

		internal static string ReadStructuralObjectClass(Type principalType)
		{
			DirectoryObjectClassAttribute[] customAttributes = (DirectoryObjectClassAttribute[])Attribute.GetCustomAttributes(principalType, typeof(DirectoryObjectClassAttribute), false);
			if (customAttributes != null)
			{
				string objectClass = null;
				for (int i = 0; i < (int)customAttributes.Length; i++)
				{
					ContextType? context = customAttributes[i].Context;
					if (!context.HasValue && objectClass == null)
					{
						objectClass = customAttributes[i].ObjectClass;
					}
				}
				return objectClass;
			}
			else
			{
				return null;
			}
		}
	}
}