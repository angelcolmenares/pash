using System;

namespace System.Runtime
{
	internal interface IAsyncEventArgs
	{
		object AsyncState
		{
			get;
		}

		Exception Exception
		{
			get;
		}

	}
}