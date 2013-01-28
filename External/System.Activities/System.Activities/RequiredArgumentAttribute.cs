using System;

namespace System.Activities
{
	[AttributeUsage (AttributeTargets.Property)]
	public sealed class RequiredArgumentAttribute : Attribute
	{
		public override object TypeId { get { throw new NotImplementedException (); } }
	}
}
