using System;
using Microsoft.WSMan.Enumeration;

namespace Microsoft.WSMan.Cim
{
	public class CimEnumerationRequestHandler : IEnumerationRequestHandler
	{
		public bool IsLocal
		{
			get;set;
		}

		public CimEnumerationRequestHandler ()
		{

		}

		#region IEnumerationRequestHandler implementation

		public System.Collections.Generic.IEnumerable<object> Enumerate (IEnumerationContext context)
		{
			var filter = context.Filter.Value as CimEnumerationFilter;
			if (filter != null) {
				using (CimEnumerator enumerator = new CimEnumerator(IsLocal))
				{
					return enumerator.Get (OperationContextProxy.Current.UserName, "", filter.Namespace, filter.Filter);
				}
			}
			throw new NotImplementedException ();
		}

		public int EstimateRemainingItemsCount (IEnumerationContext context)
		{
			var filter = context.Filter.Value as CimEnumerationFilter;
			if (filter != null) {
				using (CimEnumerator enumerator = new CimEnumerator(IsLocal))
				{
					return enumerator.GetCount(OperationContextProxy.Current.UserName, "", filter.Namespace, filter.Filter);
				}
			}
			return 0;
		}

		#endregion
	}
}

