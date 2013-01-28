using System;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PromptForChoiceMessage : PromptMessageBase
	{
		public string Caption
		{
			get;
			private set;
		}

		[CLSCompliant(false)]
		public Collection<ChoiceDescription> Choices
		{
			get;
			private set;
		}

		public int DefaultChoice
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		internal PromptForChoiceMessage(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice) : base((ClientMessageType)102)
		{
			this.Caption = caption;
			this.Message = message;
			this.Choices = choices;
			this.DefaultChoice = defaultChoice;
		}
	}
}