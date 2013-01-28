namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ValidateArgumentsAttribute : CmdletMetadataAttribute
    {
        protected ValidateArgumentsAttribute()
        {
        }

        internal void InternalValidate(object o, EngineIntrinsics engineIntrinsics)
        {
            this.Validate(o, engineIntrinsics);
        }

        protected abstract void Validate(object arguments, EngineIntrinsics engineIntrinsics);
    }
}

