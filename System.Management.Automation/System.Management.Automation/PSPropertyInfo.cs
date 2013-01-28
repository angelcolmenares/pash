namespace System.Management.Automation
{
    using System;

    public abstract class PSPropertyInfo : PSMemberInfo
    {
        protected PSPropertyInfo()
        {
        }

        internal Exception NewGetValueException(Exception e, string errorId)
        {
            return new GetValueInvocationException(errorId, e, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { base.Name, e.Message });
        }

        internal Exception NewSetValueException(Exception e, string errorId)
        {
            return new SetValueInvocationException(errorId, e, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { base.Name, e.Message });
        }

        public abstract bool IsGettable { get; }

        public abstract bool IsSettable { get; }
    }
}

