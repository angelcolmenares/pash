using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Management
{
	internal class ManagementScopeConverter : ExpandableObjectConverter
	{
		public ManagementScopeConverter()
		{
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType != typeof(ManagementScope))
			{
				return base.CanConvertFrom(context, sourceType);
			}
			else
			{
				return true;
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType != typeof(InstanceDescriptor))
			{
				return base.CanConvertTo(context, destinationType);
			}
			else
			{
				return true;
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != null)
			{
				if (value as ManagementScope != null && destinationType == typeof(InstanceDescriptor))
				{
					ManagementScope managementScope = (ManagementScope)value;
					Type[] typeArray = new Type[1];
					typeArray[0] = typeof(string);
					ConstructorInfo constructor = typeof(ManagementScope).GetConstructor(typeArray);
					if (constructor != null)
					{
						object[] path = new object[1];
						path[0] = managementScope.Path.Path;
						return new InstanceDescriptor(constructor, path);
					}
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
			else
			{
				throw new ArgumentNullException("destinationType");
			}
		}
	}
}