using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Workflow
{
	public class PSWorkflowContext
	{
		private Dictionary<string, object> _workflowParameters;

		private Dictionary<string, object> _psWorkflowCommonParameters;

		private Dictionary<string, object> jobMetadata;

		private Dictionary<string, object> privateMetadata;

		public Dictionary<string, object> JobMetadata
		{
			get
			{
				return this.jobMetadata;
			}
			set
			{
				this.jobMetadata = value;
			}
		}

		public Dictionary<string, object> PrivateMetadata
		{
			get
			{
				return this.privateMetadata;
			}
			set
			{
				this.privateMetadata = value;
			}
		}

		public Dictionary<string, object> PSWorkflowCommonParameters
		{
			get
			{
				return this._psWorkflowCommonParameters;
			}
			set
			{
				this._psWorkflowCommonParameters = value;
			}
		}

		public Dictionary<string, object> WorkflowParameters
		{
			get
			{
				return this._workflowParameters;
			}
			set
			{
				this._workflowParameters = value;
			}
		}

		public PSWorkflowContext()
		{
			this._workflowParameters = null;
			this._psWorkflowCommonParameters = null;
			this.jobMetadata = null;
			this.privateMetadata = null;
		}

		public PSWorkflowContext(Dictionary<string, object> workflowParameters, Dictionary<string, object> workflowCommonParameters, Dictionary<string, object> jobMetadata, Dictionary<string, object> privateMetadata)
		{
			this._workflowParameters = workflowParameters;
			this._psWorkflowCommonParameters = workflowCommonParameters;
			this.jobMetadata = jobMetadata;
			this.privateMetadata = privateMetadata;
		}
	}
}