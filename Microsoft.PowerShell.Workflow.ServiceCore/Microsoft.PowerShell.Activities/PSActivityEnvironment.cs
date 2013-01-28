using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell.Activities
{
	public sealed class PSActivityEnvironment
	{
		private readonly Collection<string> _modules;

		private readonly Dictionary<string, object> _variables;

		public Collection<string> Modules
		{
			get
			{
				return this._modules;
			}
		}

		public Dictionary<string, object> Variables
		{
			get
			{
				return this._variables;
			}
		}

		public PSActivityEnvironment()
		{
			this._modules = new Collection<string>();
			this._variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}
	}
}