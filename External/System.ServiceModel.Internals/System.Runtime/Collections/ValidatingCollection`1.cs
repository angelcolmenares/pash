using System;
using System.Collections.ObjectModel;
using System.Runtime;

namespace System.Runtime.Collections
{
	internal class ValidatingCollection<T> : Collection<T>
	{
		public Action<T> OnAddValidationCallback
		{
			get;set;
		}

		public Action OnMutateValidationCallback
		{
			get;set;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ValidatingCollection()
		{
		}

		protected override void ClearItems()
		{
			this.OnMutate();
			base.ClearItems();
		}

		protected override void InsertItem(int index, T item)
		{
			this.OnAdd(item);
			base.InsertItem(index, item);
		}

		private void OnAdd(T item)
		{
			if (this.OnAddValidationCallback != null)
			{
				this.OnAddValidationCallback(item);
			}
		}

		private void OnMutate()
		{
			if (this.OnMutateValidationCallback != null)
			{
				this.OnMutateValidationCallback();
			}
		}

		protected override void RemoveItem(int index)
		{
			this.OnMutate();
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, T item)
		{
			this.OnAdd(item);
			this.OnMutate();
			base.SetItem(index, item);
		}
	}
}