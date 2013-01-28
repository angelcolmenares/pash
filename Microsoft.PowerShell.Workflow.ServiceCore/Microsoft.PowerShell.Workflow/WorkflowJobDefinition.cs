using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.Workflow
{
	internal class WorkflowJobDefinition : JobDefinition
	{
		private readonly string _modulePath;

		private readonly List<string> _dependentWorkflows;

		private readonly string _dependentAssemblyPath;

		private readonly string _xaml;

		internal static IEnumerable<string> EmptyEnumerable;

		internal string DependentAssemblyPath
		{
			get
			{
				return this._dependentAssemblyPath;
			}
		}

		internal List<string> DependentWorkflows
		{
			get
			{
				return this._dependentWorkflows;
			}
		}

		internal bool IsScriptWorkflow
		{
			get;
			set;
		}

		internal string ModulePath
		{
			get
			{
				return this._modulePath;
			}
		}

		internal string Xaml
		{
			get
			{
				return this._xaml;
			}
		}

		static WorkflowJobDefinition()
		{
			WorkflowJobDefinition.EmptyEnumerable = new Collection<string>();
		}

		public WorkflowJobDefinition(Type jobSourceAdapterType, string command, string name) : base(jobSourceAdapterType, command, name)
		{
			this._dependentWorkflows = new List<string>();
			this.IsScriptWorkflow = false;
		}

		internal WorkflowJobDefinition(Type jobSourceAdapterType, string command, string name, string modulePath, IEnumerable<string> dependentWorkflows, string dependentAssemblyPath, string xaml) : this(jobSourceAdapterType, command, name)
		{
			this._modulePath = modulePath;
			this._dependentAssemblyPath = dependentAssemblyPath;
			this._dependentWorkflows.AddRange(dependentWorkflows);
			this._xaml = xaml;
		}

		internal WorkflowJobDefinition(JobDefinition jobDefinition, string modulePath, IEnumerable<string> dependentWorkflows, string dependentAssemblyPath, string xaml) : this(jobDefinition.JobSourceAdapterType, jobDefinition.Command, jobDefinition.Name, modulePath, dependentWorkflows, dependentAssemblyPath, xaml)
		{
			base.InstanceId = jobDefinition.InstanceId;
		}

		internal WorkflowJobDefinition(JobDefinition jobDefinition) : this(jobDefinition, string.Empty, WorkflowJobDefinition.EmptyEnumerable, string.Empty, string.Empty)
		{
		}

		internal static WorkflowJobDefinition AsWorkflowJobDefinition(JobDefinition definition)
		{
			WorkflowJobDefinition workflowJobDefinition = definition as WorkflowJobDefinition;
			WorkflowJobDefinition workflowJobDefinition1 = workflowJobDefinition;
			if (workflowJobDefinition == null)
			{
				WorkflowJobDefinition workflowJobDefinition2 = DefinitionCache.Instance.GetDefinition(definition.InstanceId) as WorkflowJobDefinition;
				workflowJobDefinition1 = workflowJobDefinition2;
				if (workflowJobDefinition2 == null)
				{
					workflowJobDefinition1 = new WorkflowJobDefinition(definition);
				}
			}
			return workflowJobDefinition1;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			throw new NotImplementedException();
		}
	}
}