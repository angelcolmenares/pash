namespace System.Management.Automation
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public sealed class CredentialAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            PSCredential credential = null;
            string userName = null;
            bool flag = false;
            if (((engineIntrinsics == null) || (engineIntrinsics.Host == null)) || (engineIntrinsics.Host.UI == null))
            {
                throw PSTraceSource.NewArgumentNullException("engineIntrinsics");
            }
            if (inputData == null)
            {
                flag = true;
            }
            else
            {
                credential = inputData is string ? null : LanguagePrimitives.FromObjectAs<PSCredential>(inputData);
                if (credential == null)
                {
                    flag = true;
                    userName = LanguagePrimitives.FromObjectAs<string>(inputData);
                    if (userName == null)
                    {
                        throw new PSArgumentException("userName");
                    }
                }
            }
            if (flag)
            {
                string caption = null;
                string message = null;
                caption = CredentialAttributeStrings.CredentialAttribute_Prompt_Caption;
                message = CredentialAttributeStrings.CredentialAttribute_Prompt;
                credential = engineIntrinsics.Host.UI.PromptForCredential(caption, message, userName, "");
            }
            return credential;
        }
    }
}

