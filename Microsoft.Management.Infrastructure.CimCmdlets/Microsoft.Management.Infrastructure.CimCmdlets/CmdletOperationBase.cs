using System;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CmdletOperationBase
	{
		private readonly Cmdlet cmdlet;

		public CmdletOperationBase(Cmdlet cmdlet)
		{
			ValidationHelper.ValidateNoNullArgument(cmdlet, "cmdlet");
			this.cmdlet = cmdlet;
		}

		public virtual bool ShouldContinue(string query, string caption)
		{
			return this.cmdlet.ShouldContinue(query, caption);
		}

		public virtual bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
		{
			return this.cmdlet.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
		}

		public virtual bool ShouldProcess(string target)
		{
			return this.cmdlet.ShouldProcess(target);
		}

		public virtual bool ShouldProcess(string target, string action)
		{
			return this.cmdlet.ShouldProcess(target, action);
		}

		public virtual bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
		{
			return this.cmdlet.ShouldProcess(verboseDescription, verboseWarning, caption);
		}

		public virtual bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
		{
			return this.cmdlet.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
		}

		public virtual void ThrowTerminatingError(ErrorRecord errorRecord)
		{
			this.cmdlet.ThrowTerminatingError(errorRecord);
		}

		internal void ThrowTerminatingError(Exception exception, string operation)
		{
			ErrorRecord errorRecord = new ErrorRecord(exception, operation, ErrorCategory.InvalidOperation, this);
			this.cmdlet.ThrowTerminatingError(errorRecord);
		}

		public virtual void WriteCommandDetail(string text)
		{
			this.cmdlet.WriteCommandDetail(text);
		}

		public virtual void WriteDebug(string text)
		{
			this.cmdlet.WriteDebug(text);
		}

		public virtual void WriteError(ErrorRecord errorRecord)
		{
			this.cmdlet.WriteError(errorRecord);
		}

		public virtual void WriteObject(object sendToPipeline, XOperationContextBase context)
		{
			this.cmdlet.WriteObject(sendToPipeline);
		}

		public virtual void WriteObject(object sendToPipeline, bool enumerateCollection, XOperationContextBase context)
		{
			this.cmdlet.WriteObject(sendToPipeline, enumerateCollection);
		}

		public virtual void WriteProgress(ProgressRecord progressRecord)
		{
			this.cmdlet.WriteProgress(progressRecord);
		}

		public virtual void WriteVerbose(string text)
		{
			this.cmdlet.WriteVerbose(text);
		}

		public virtual void WriteWarning(string text)
		{
			this.cmdlet.WriteWarning(text);
		}
	}
}