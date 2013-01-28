using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Transactions;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Windows.Markup;

namespace System.Activities.Statements
{
	public abstract class TimerExtension
	{
		public void CancelTimer (Bookmark bookmark)
		{
			OnCancelTimer (bookmark);
		}

		protected abstract void OnCancelTimer (Bookmark bookmark);

		protected abstract void OnRegisterTimer (TimeSpan timeout, Bookmark bookmark);

		public void RegisterTimer (TimeSpan timeout, Bookmark bookmark)
		{
			OnRegisterTimer (timeout, bookmark);
		}
	}
}
