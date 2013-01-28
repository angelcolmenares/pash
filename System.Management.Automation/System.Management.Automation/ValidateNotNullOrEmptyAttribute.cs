namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateNotNullOrEmptyAttribute : ValidateArgumentsAttribute
    {
        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            IEnumerable enumerable = null;
            IEnumerator enumerator = null;
            string str = null;
            if ((arguments == null) || (arguments == AutomationNull.Value))
            {
                throw new ValidationMetadataException("ArgumentIsNull", null, Metadata.ValidateNotNullOrEmptyFailure, new object[0]);
            }
            str = arguments as string;
            if (str != null)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullOrEmptyFailure, new object[0]);
                }
            }
            else
            {
                enumerable = arguments as IEnumerable;
                if (enumerable != null)
                {
                    int num = 0;
                    foreach (object obj2 in enumerable)
                    {
                        num++;
                        if ((obj2 == null) || (obj2 == AutomationNull.Value))
                        {
                            throw new ValidationMetadataException("ArgumentIsNull", null, Metadata.ValidateNotNullOrEmptyCollectionFailure, new object[0]);
                        }
                        string str2 = obj2 as string;
                        if ((str2 != null) && string.IsNullOrEmpty(str2))
                        {
                            throw new ValidationMetadataException("ArgumentCollectionContainsEmpty", null, Metadata.ValidateNotNullOrEmptyFailure, new object[0]);
                        }
                    }
                    if (num == 0)
                    {
                        throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullOrEmptyCollectionFailure, new object[0]);
                    }
                }
                else
                {
                    enumerator = arguments as IEnumerator;
                    if (enumerator != null)
                    {
                        int num2 = 0;
                        while (enumerator.MoveNext())
                        {
                            num2++;
                            if ((enumerator.Current == null) || (enumerator.Current == AutomationNull.Value))
                            {
                                throw new ValidationMetadataException("ArgumentIsNull", null, Metadata.ValidateNotNullOrEmptyCollectionFailure, new object[0]);
                            }
                        }
                        if (num2 == 0)
                        {
                            throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullOrEmptyCollectionFailure, new object[0]);
                        }
                    }
                }
            }
        }
    }
}

