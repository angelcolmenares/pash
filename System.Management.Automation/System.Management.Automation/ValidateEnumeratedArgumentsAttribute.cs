namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ValidateEnumeratedArgumentsAttribute : ValidateArgumentsAttribute
    {
        protected ValidateEnumeratedArgumentsAttribute()
        {
        }

        protected sealed override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            if ((arguments == null) || (arguments == AutomationNull.Value))
            {
                throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullOrEmptyCollectionFailure, new object[0]);
            }
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(arguments);
            if (enumerable == null)
            {
                this.ValidateElement(arguments);
            }
            else
            {
                foreach (object obj2 in enumerable)
                {
                    this.ValidateElement(obj2);
                }
            }
        }

        protected abstract void ValidateElement(object element);
    }
}

