using System.Collections;

namespace System.DirectoryServices.AccountManagement
{
	internal class QbeFilterDescription
	{
		private ArrayList filtersToApply;

		public ArrayList FiltersToApply
		{
			get
			{
				return this.filtersToApply;
			}
		}

		public QbeFilterDescription()
		{
			this.filtersToApply = new ArrayList();
		}
	}
}