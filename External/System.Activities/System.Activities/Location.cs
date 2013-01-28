using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[DataContract]
	public abstract class Location
	{
		public abstract Type LocationType { get; }
		public object Value { get; set; }
		protected abstract object ValueCore { get; set; }
	}

	[DataContract]
	public class Location<T> : Location
	{
		public override Type LocationType {
			get { return typeof (T); }
		}
		public new virtual object Value { get; set; }
		protected override object ValueCore { get; set; }
	}
}
