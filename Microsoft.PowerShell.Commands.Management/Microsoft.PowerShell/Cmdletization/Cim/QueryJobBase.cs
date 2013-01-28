using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal abstract class QueryJobBase : CimChildJobBase<CimInstance>
	{
		private CimQuery _cimQuery;

		internal QueryJobBase(CimJobContext jobContext, CimQuery cimQuery) : base(jobContext)
		{
			this._cimQuery = cimQuery;
		}

		internal override CimCustomOptionsDictionary CalculateJobSpecificCustomOptions()
		{
			return CimCustomOptionsDictionary.Create(this._cimQuery.queryOptions);
		}

		public override void OnCompleted()
		{
			base.ExceptionSafeWrapper(() => {
				foreach (ClientSideQuery.NotFoundError notFoundError in this._cimQuery.GenerateNotFoundErrors())
				{
					string str = "CmdletizationQuery_NotFound";
					if (!string.IsNullOrEmpty(notFoundError.PropertyName))
					{
						str = string.Concat(str, "_", notFoundError.PropertyName);
					}
					CimJobException cimJobException = CimJobException.CreateWithFullControl(base.JobContext, notFoundError.ErrorMessageGenerator(this.Description, base.JobContext.ClassName), str, ErrorCategory.ObjectNotFound, null);
					if (!string.IsNullOrEmpty(notFoundError.PropertyName))
					{
						cimJobException.ErrorRecord.SetTargetObject(notFoundError.PropertyValue);
					}
					this.WriteError(cimJobException.ErrorRecord);
				}
			}
			);
			base.OnCompleted();
		}

        public override void OnNext(CimInstance item)
        {
            base.ExceptionSafeWrapper(delegate
            {
                if ((item != null) && this._cimQuery.IsMatchingResult(item))
                {
                    this.WriteObject(item);
                }
            });
        }
	}
}