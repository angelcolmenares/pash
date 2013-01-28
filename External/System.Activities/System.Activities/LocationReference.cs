using System;

namespace System.Activities
{
	public abstract class LocationReference
	{
		public string Name { get { throw new NotImplementedException (); } }
		protected abstract string NameCore { get; }
		public Type Type { get { throw new NotImplementedException (); } }
		protected abstract Type TypeCore { get; }
		
		public abstract Location GetLocation (ActivityContext context);
	}
}
