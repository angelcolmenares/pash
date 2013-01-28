namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateNotNullAttribute : ValidateArgumentsAttribute
    {
        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            IEnumerable enumerable = null;
            IEnumerator enumerator = null;
            if ((arguments == null) || (arguments == AutomationNull.Value))
            {
                throw new ValidationMetadataException("ArgumentIsNull", null, Metadata.ValidateNotNullFailure, new object[0]);
            }
            enumerable = arguments as IEnumerable;
            if (enumerable != null)
            {
                foreach (object obj2 in enumerable)
                {
                    if ((obj2 == null) || (obj2 == AutomationNull.Value))
                    {
                        throw new ValidationMetadataException("ArgumentIsNull", null, Metadata.ValidateNotNullCollectionFailure, new object[0]);
                    }
                }
            }
            else
            {
                enumerator = arguments as IEnumerator;
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        if ((enumerator.Current == null) || (enumerator.Current == AutomationNull.Value))
                        {
                            throw new ValidationMetadataException("ArgumentIsNull", null, Metadata.ValidateNotNullCollectionFailure, new object[0]);
                        }
                    }
                }
            }
        }
    }
}

