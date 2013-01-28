using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Transactions;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Windows.Markup;

namespace System.Activities.Statements
{
	public sealed class Interop : NativeActivity, ICustomTypeDescriptor
	{
		[Browsable (false)]
		public IDictionary<string, Object> ActivityMetaProperties { get { throw new NotImplementedException (); } }
		[Browsable (false)]
		public IDictionary<string, Argument> ActivityProperties { get { throw new NotImplementedException (); } }
		public Type ActivityType { get; set; }

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attributes)
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

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
