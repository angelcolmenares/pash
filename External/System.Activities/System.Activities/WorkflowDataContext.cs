using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Activities
{
	public sealed class WorkflowDataContext : CustomTypeDescriptor, INotifyPropertyChanged, IDisposable
	{
		internal WorkflowDataContext ()
		{
			throw new NotImplementedException ();
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}
}
