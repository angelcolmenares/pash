using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal interface IADCmdletMessageWriter
	{
		void WriteCommandDetail(string text);

		void WriteDebug(string text);

		void WriteProgress(ProgressRecord progressRecord);

		void WriteVerbose(string text);

		void WriteWarningBuffered(string text);

		void WriteWarningImmediate(string text);
	}
}