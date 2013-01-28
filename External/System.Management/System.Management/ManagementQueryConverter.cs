using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Management
{
	internal class ManagementQueryConverter : ExpandableObjectConverter
	{
		public ManagementQueryConverter()
		{
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType != typeof(ManagementQuery))
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
				if (value as EventQuery != null && destinationType == typeof(InstanceDescriptor))
				{
					EventQuery eventQuery = (EventQuery)value;
					Type[] typeArray = new Type[1];
					typeArray[0] = typeof(string);
					ConstructorInfo constructor = typeof(EventQuery).GetConstructor(typeArray);
					if (constructor != null)
					{
						object[] queryString = new object[1];
						queryString[0] = eventQuery.QueryString;
						return new InstanceDescriptor(constructor, queryString);
					}
				}
				if (value as ObjectQuery != null && destinationType == typeof(InstanceDescriptor))
				{
					ObjectQuery objectQuery = (ObjectQuery)value;
					Type[] typeArray1 = new Type[1];
					typeArray1[0] = typeof(string);
					ConstructorInfo constructorInfo = typeof(ObjectQuery).GetConstructor(typeArray1);
					if (constructorInfo != null)
					{
						object[] objArray = new object[1];
						objArray[0] = objectQuery.QueryString;
						return new InstanceDescriptor(constructorInfo, objArray);
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