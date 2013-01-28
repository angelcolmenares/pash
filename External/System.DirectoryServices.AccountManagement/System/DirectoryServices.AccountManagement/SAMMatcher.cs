using System;
using System.DirectoryServices;

namespace System.DirectoryServices.AccountManagement
{
	internal abstract class SAMMatcher
	{
		protected SAMMatcher()
		{
		}

		internal abstract bool Matches(DirectoryEntry de);
	}
}