using System;
using System.ComponentModel;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	public class SearchOptionsControl : DirectoryControl
	{
		private SearchOption flag;

		public SearchOption SearchOption
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.flag;
			}
			set
			{
				if (value < SearchOption.DomainScope || value > SearchOption.PhantomRoot)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(SearchOption));
				}
				else
				{
					this.flag = value;
					return;
				}
			}
		}

		public SearchOptionsControl() : base("1.2.840.113556.1.4.1340", null, true, true)
		{
			this.flag = SearchOption.DomainScope;
		}

		public SearchOptionsControl(SearchOption flags) : this()
		{
			this.SearchOption = flags;
		}

		public override byte[] GetValue()
		{
			object[] objArray = new object[1];
			objArray[0] = (int)this.flag;
			this.directoryControlValue = BerConverter.Encode("{i}", objArray);
			return base.GetValue();
		}
	}
}