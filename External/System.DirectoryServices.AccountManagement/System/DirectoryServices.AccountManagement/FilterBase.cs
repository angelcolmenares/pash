using System;

namespace System.DirectoryServices.AccountManagement
{
	internal abstract class FilterBase
	{
		private object @value;

		private object extra;

		public object Extra
		{
			get
			{
				return this.extra;
			}
			set
			{
				this.extra = value;
			}
		}

		public abstract string PropertyName
		{
			get;
		}

		public object Value
		{
			get
			{
				return this.@value;
			}
			set
			{
				this.@value = value;
			}
		}

		protected FilterBase()
		{
		}
	}
}