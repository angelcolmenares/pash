using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADExceptionFilter : IADExceptionFilter
	{
		private List<IADExceptionFilter> _list;

		public List<IADExceptionFilter> List
		{
			get
			{
				return this._list;
			}
		}

		public ADExceptionFilter()
		{
			this._list = new List<IADExceptionFilter>();
		}

		public void Add(IADExceptionFilter filter)
		{
			this.List.Add(filter);
		}

		bool Microsoft.ActiveDirectory.Management.Commands.IADExceptionFilter.FilterException(Exception e, ref bool isTerminating)
		{
			bool flag;
			List<IADExceptionFilter>.Enumerator enumerator = this._list.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					IADExceptionFilter current = enumerator.Current;
					if (!current.FilterException(e, ref isTerminating))
					{
						continue;
					}
					flag = true;
					return flag;
				}
				isTerminating = false;
				return false;
			}
			finally
			{
				enumerator.Dispose();
			}
			return flag;
		}
	}
}