namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public enum ClientMessageType
	{
		CommandCompleted = 0,
		Exit = 100,
		Prompt = 101,
		PromptForChoice = 102,
		PromptForCredential = 103,
		ReadLine = 104,
		ReadLineAsSecureString = 105,
		Write = 106,
		WriteLine = 107,
		WriteProgress = 108,
		ClearBuffer = 109,
		SetBackgroundColor = 110,
		SetBufferSize = 111,
		SetForegroundColor = 112,
		SetWindowSize = 113,
		SetWindowTitle = 114,
		SessionTerminated = 115
	}
}