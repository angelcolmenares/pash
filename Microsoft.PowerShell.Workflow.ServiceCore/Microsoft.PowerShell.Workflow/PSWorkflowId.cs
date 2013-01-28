using System;

namespace Microsoft.PowerShell.Workflow
{
	public sealed class PSWorkflowId
	{
		private Guid _guid;

		public Guid Guid
		{
			get
			{
				return this._guid;
			}
		}

		public PSWorkflowId()
		{
			this._guid = new Guid();
		}

		public PSWorkflowId(Guid value)
		{
			this._guid = value;
		}

		public static PSWorkflowId NewWorkflowGuid()
		{
			return new PSWorkflowId(Guid.NewGuid());
		}
	}
}