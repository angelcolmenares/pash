namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ArgumentTransformationAttribute : CmdletMetadataAttribute
    {
        protected ArgumentTransformationAttribute()
        {
        }

        public abstract object Transform(EngineIntrinsics engineIntrinsics, object inputData);
    }
}

