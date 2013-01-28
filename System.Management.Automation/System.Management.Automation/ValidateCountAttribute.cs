namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateCountAttribute : ValidateArgumentsAttribute
    {
        private int maxLength;
        private int minLength;

        public ValidateCountAttribute(int minLength, int maxLength)
        {
            if (minLength < 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("minLength", minLength);
            }
            if (maxLength <= 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("maxLength", maxLength);
            }
            if (maxLength < minLength)
            {
                throw new ValidationMetadataException("ValidateRangeMaxLengthSmallerThanMinLength", null, Metadata.ValidateCountMaxLengthSmallerThanMinLength, new object[0]);
            }
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            int count = 0;
            if ((arguments == null) || (arguments == AutomationNull.Value))
            {
                count = 0;
            }
            else
            {
                IList list = arguments as IList;
                if (list != null)
                {
                    count = (int) list.Count;
                }
                else
                {
                    ICollection is2 = arguments as ICollection;
                    if (is2 != null)
                    {
                        count = (int) is2.Count;
                    }
                    else
                    {
                        IEnumerable enumerable = arguments as IEnumerable;
                        if (enumerable != null)
                        {
                            IEnumerator enumerator2 = enumerable.GetEnumerator();
                            while (enumerator2.MoveNext())
                            {
                                count++;
                            }
                        }
                        else
                        {
                            IEnumerator enumerator = arguments as IEnumerator;
                            if (enumerator == null)
                            {
                                throw new ValidationMetadataException("NotAnArrayParameter", null, Metadata.ValidateCountNotInArray, new object[0]);
                            }
                            while (enumerator.MoveNext())
                            {
                                count++;
                            }
                        }
                    }
                }
            }
            if (count < this.minLength)
            {
                throw new ValidationMetadataException("ValidateCountSmallerThanMin", null, Metadata.ValidateCountMinLengthFailure, new object[] { this.minLength, count });
            }
            if (count > this.maxLength)
            {
                throw new ValidationMetadataException("ValidateCountGreaterThanMax", null, Metadata.ValidateCountMaxLengthFailure, new object[] { this.maxLength, count });
            }
        }

        public int MaxLength
        {
            get
            {
                return this.maxLength;
            }
        }

        public int MinLength
        {
            get
            {
                return this.minLength;
            }
        }
    }
}

