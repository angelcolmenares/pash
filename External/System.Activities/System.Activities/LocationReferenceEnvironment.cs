using System;
using System.Collections.Generic;

namespace System.Activities
{
	public abstract class LocationReferenceEnvironment
	{
		public LocationReferenceEnvironment Parent { get; protected set; }
		public abstract Activity Root { get; }
		
		public abstract IEnumerable<LocationReference> GetLocationReferences ();
		public abstract bool IsVisible (LocationReference locationReference);
		public abstract bool TryGetLocationReference (string name, out LocationReference result);
	}
}
