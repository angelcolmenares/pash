using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class EnumerateAssociatedInstancesJob : QueryJobBase
	{
		private readonly CimInstance associatedObject;

		private readonly string associationName;

		private readonly string resultRole;

		private readonly string sourceRole;

		internal override string Description
		{
			get
			{
				object[] cmdletizationClassName = new object[3];
				cmdletizationClassName[0] = base.JobContext.CmdletizationClassName;
				cmdletizationClassName[1] = base.JobContext.Session.ComputerName;
				cmdletizationClassName[2] = this.associatedObject.ToString();
				return string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_AssociationDescription, cmdletizationClassName);
			}
		}

		internal override string FailSafeDescription
		{
			get
			{
				object[] cmdletizationClassName = new object[2];
				cmdletizationClassName[0] = base.JobContext.CmdletizationClassName;
				cmdletizationClassName[1] = base.JobContext.Session.ComputerName;
				return string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_SafeAssociationDescription, cmdletizationClassName);
			}
		}

		internal EnumerateAssociatedInstancesJob(CimJobContext jobContext, CimQuery cimQuery, CimInstance associatedObject, string associationName, string resultRole, string sourceRole) : base(jobContext, cimQuery)
		{
			this.associatedObject = associatedObject;
			this.associationName = associationName;
			this.resultRole = resultRole;
			this.sourceRole = sourceRole;
		}

		internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
		{
			return CimCustomOptionsDictionary.MergeOptions(base.CalculateJobSpecificCustomOptions(), this.associatedObject);
		}

		internal override IObservable<CimInstance> GetCimOperation()
		{
			base.WriteVerboseStartOfCimOperation();
			IObservable<CimInstance> observable = base.JobContext.Session.EnumerateAssociatedInstancesAsync(base.JobContext.Namespace, this.associatedObject, this.associationName, base.JobContext.ClassNameOrNullIfResourceUriIsUsed, this.sourceRole, this.resultRole, base.CreateOperationOptions());
			return observable;
		}

		internal override string GetProviderVersionExpectedByJob()
		{
			return null;
		}

		internal override void WriteObject(object outputObject)
		{
			if (CimChildJobBase<CimInstance>.IsShowComputerNameMarkerPresent(this.associatedObject))
			{
				PSObject pSObject = PSObject.AsPSObject(outputObject);
				CimChildJobBase<CimInstance>.AddShowComputerNameMarker(pSObject);
			}
			base.WriteObject(outputObject);
		}
	}
}