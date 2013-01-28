using Microsoft.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management
{
	internal abstract class DelegatePipeline
	{
		private List<Delegate> _delegatePipeline;

		private Type _delegateType;

		protected abstract string DelegateMethodSuffix
		{
			get;
		}

		protected DelegatePipeline(Type typeOfDelegate)
		{
			this._delegatePipeline = new List<Delegate>();
			this._delegateType = typeOfDelegate;
		}

		protected void CheckDelegatePipelineNotEmpty()
		{
			if (this._delegatePipeline.Count != 0)
			{
				return;
			}
			else
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.DelegatePipelineEmptyError, new object[0]));
			}
		}

		public void Clear()
		{
			this._delegatePipeline.Clear();
		}

		public Delegate GetDelegate()
		{
			Delegate @delegate = null;
			foreach (Delegate delegate1 in this._delegatePipeline)
			{
				@delegate = Delegate.Combine(@delegate, delegate1);
			}
			return @delegate;
		}

		private int GetDelegateIndex(Delegate referenceDelegate)
		{
			int num;
			int num1 = 0;
			List<Delegate>.Enumerator enumerator = this._delegatePipeline.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Delegate current = enumerator.Current;
					if (current.Target != referenceDelegate.Target || !(current.Method == referenceDelegate.Method))
					{
						num1++;
					}
					else
					{
						num = num1;
						return num;
					}
				}
				object[] target = new object[2];
				target[0] = referenceDelegate.Target;
				target[1] = referenceDelegate.Method;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.DelegatePipelineReferenceDelegateNotFoundError, target));
			}
			finally
			{
				enumerator.Dispose();
			}
			return num;
		}

		private void Insert(Delegate referenceDelegate, Delegate newDelegate, DelegatePipeline.InsertPosition position)
		{
			int delegateIndex;
			this.CheckDelegatePipelineNotEmpty();
			if (position != DelegatePipeline.InsertPosition.After)
			{
				delegateIndex = this.GetDelegateIndex(referenceDelegate);
			}
			else
			{
				delegateIndex = this.GetDelegateIndex(referenceDelegate) + 1;
			}
			this._delegatePipeline.Insert(delegateIndex, newDelegate);
		}

		protected void InsertAfter(Delegate referenceDelegate, Delegate newDelegate)
		{
			this.ValidateNewDelegate(newDelegate);
			this.Insert(referenceDelegate, newDelegate, DelegatePipeline.InsertPosition.After);
		}

		protected void InsertAtEnd(Delegate newDelegate)
		{
			this.ValidateNewDelegate(newDelegate);
			this._delegatePipeline.Add(newDelegate);
		}

		protected void InsertAtStart(Delegate newDelegate)
		{
			this.ValidateNewDelegate(newDelegate);
			this._delegatePipeline.Insert(0, newDelegate);
		}

		protected void InsertBefore(Delegate referenceDelegate, Delegate newDelegate)
		{
			this.ValidateNewDelegate(newDelegate);
			this.Insert(referenceDelegate, newDelegate, DelegatePipeline.InsertPosition.Before);
		}

		protected void Remove(Delegate existingDelegate)
		{
			this.CheckDelegatePipelineNotEmpty();
			int delegateIndex = this.GetDelegateIndex(existingDelegate);
			this._delegatePipeline.RemoveAt(delegateIndex);
		}

		protected void Replace(Delegate existingDelegate, Delegate newDelegate)
		{
			this.ValidateNewDelegate(newDelegate);
			this.InsertAfter(existingDelegate, newDelegate);
			this.Remove(existingDelegate);
		}

		private void ValidateNewDelegate(Delegate newDelegate)
		{
			if (newDelegate != null)
			{
				if (newDelegate.GetType() == this._delegateType)
				{
					if ((int)newDelegate.GetInvocationList().Length <= 1)
					{
						return;
					}
					else
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.DelegatePipelineMulticastDelegatesNotAllowedError, new object[0]));
					}
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = this._delegateType;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.DelegatePipelineUnsupportedTypeError, objArray), "newDelegate");
				}
			}
			else
			{
				throw new ArgumentNullException("newDelegate");
			}
		}

		private enum InsertPosition
		{
			Before,
			After
		}
	}
}