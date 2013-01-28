using System;

namespace System.Management
{
	internal class WmiDelegateInvoker
	{
		internal object sender;

		internal WmiDelegateInvoker(object sender)
		{
			this.sender = sender;
		}

		internal void FireEventToDelegates(MulticastDelegate md, ManagementEventArgs args)
		{
			try
			{
				if (md != null)
				{
					Delegate[] invocationList = md.GetInvocationList();
					for (int i = 0; i < (int)invocationList.Length; i++)
					{
						Delegate @delegate = invocationList[i];
						try
						{
							object[] objArray = new object[2];
							objArray[0] = this.sender;
							objArray[1] = args;
							@delegate.DynamicInvoke(objArray);
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
		}
	}
}