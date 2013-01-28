namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    public sealed class ValidateScriptAttribute : ValidateEnumeratedArgumentsAttribute
    {
        private System.Management.Automation.ScriptBlock _scriptBlock;

        public ValidateScriptAttribute(System.Management.Automation.ScriptBlock scriptBlock)
        {
            if (scriptBlock == null)
            {
                throw PSTraceSource.NewArgumentException("scriptBlock");
            }
            this._scriptBlock = scriptBlock;
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullFailure, new object[0]);
            }
            if (!LanguagePrimitives.IsTrue(this._scriptBlock.DoInvokeReturnAsIs(true, System.Management.Automation.ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, LanguagePrimitives.AsPSObjectOrNull(element), AutomationNull.Value, AutomationNull.Value, new object[0])))
            {
                throw new ValidationMetadataException("ValidateScriptFailure", null, Metadata.ValidateScriptFailure, new object[] { element, this._scriptBlock });
            }
        }

        public System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return this._scriptBlock;
            }
        }
    }
}

