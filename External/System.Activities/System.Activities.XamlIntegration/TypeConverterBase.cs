using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xaml;
using System.Xml;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Windows.Markup;

namespace System.Activities.XamlIntegration
{
	public abstract class TypeConverterBase : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context,Type sourceType)
		{
			throw new NotImplementedException ();
		}
		public override bool CanConvertTo (ITypeDescriptorContext context,Type destinationType)
		{
			throw new NotImplementedException ();
		}
		public override object ConvertFrom (ITypeDescriptorContext context,CultureInfo culture,object value)
		{
			throw new NotImplementedException ();
		}
		public override object ConvertTo (ITypeDescriptorContext context,CultureInfo culture,object value,Type destinationType)
		{
			throw new NotImplementedException ();
		}
	}
}
