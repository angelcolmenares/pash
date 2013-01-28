using System;

namespace Microsoft.WSMan.Management
{
	public class WSManResourceLocator : IWSManResourceLocator
	{
		public WSManResourceLocator ()
		{

		}

		#region IWSManResourceLocator implementation

		public void AddOption (string OptionName, object OptionValue, int mustComply)
		{
			throw new NotImplementedException ();
		}

		public void AddSelector (string resourceSelName, object selValue)
		{
			throw new NotImplementedException ();
		}

		public void ClearOptions ()
		{
			throw new NotImplementedException ();
		}

		public void ClearSelectors ()
		{
			throw new NotImplementedException ();
		}

		public string Error {
			get;
			private set;
		}

		public string FragmentDialect {
			get;
			set;
		}

		public string FragmentPath {
			get;
			set;
		}

		public int MustUnderstandOptions {
			get;
			set;
		}

		public string resourceUri {
			get;
			set;
		}

		#endregion
	}
}

