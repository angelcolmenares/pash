using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Transactions;
using System.Windows.Markup;
using System.Xaml;
using System.Xml.Linq;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.Tracking;
using System.Activities.Validation;
using System.Activities.XamlIntegration;

namespace System.Activities
{
	[ContentProperty ("Implementation")]
	public sealed class DynamicActivity : Activity, ICustomTypeDescriptor
	{
		public Collection<Attribute> Attributes { get { throw new NotImplementedException (); } }

		public new Collection<Constraint> Constraints { get { throw new NotImplementedException (); } }

		[XamlDeferLoad (typeof(FuncDeferringLoader), typeof(Activity))]
		[AmbientAttribute]
		[Browsable (false)]
		public new Func<Activity> Implementation { get; set; }

		public string Name { get; set; }

		[Browsable (false)]
		public KeyedCollection<string, DynamicActivityProperty> Properties { get { throw new NotImplementedException (); } }

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attribute)
		{
			throw new NotImplementedException ();
		}
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			throw new NotImplementedException ();
		}
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			throw new NotImplementedException ();
		}
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException ();
		}
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException ();
		}
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException ();
		}
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}
	}
	
	[ContentProperty ("Implementation")]
	public sealed class DynamicActivity<TResult> : Activity<TResult>, ICustomTypeDescriptor
	{
		public Collection<Attribute> Attributes { get { throw new NotImplementedException (); } }

		public new Collection<Constraint> Constraints { get { throw new NotImplementedException (); } }

		[XamlDeferLoad (typeof(FuncDeferringLoader), typeof(Activity))]
		[AmbientAttribute]
		[Browsable (false)]
		public new Func<Activity> Implementation { get; set; }

		public string Name { get; set; }

		[Browsable (false)]
		public KeyedCollection<string, DynamicActivityProperty> Properties { get { throw new NotImplementedException (); } }

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attribute)
		{
			throw new NotImplementedException ();
		}
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			throw new NotImplementedException ();
		}
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			throw new NotImplementedException ();
		}
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException ();
		}
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException ();
		}
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException ();
		}
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}
	}
}
