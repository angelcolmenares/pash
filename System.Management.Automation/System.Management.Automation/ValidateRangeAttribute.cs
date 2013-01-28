namespace System.Management.Automation
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ValidateRangeAttribute : ValidateEnumeratedArgumentsAttribute
    {
        private IComparable maxComparable;
        private object maxRange;
        private IComparable minComparable;
        private object minRange;
        private Type promotedType;

        public ValidateRangeAttribute(object minRange, object maxRange)
        {
            if (minRange == null)
            {
                throw PSTraceSource.NewArgumentNullException("minRange");
            }
            if (maxRange == null)
            {
                throw PSTraceSource.NewArgumentNullException("maxRange");
            }
            if (!maxRange.GetType().Equals(minRange.GetType()))
            {
                object obj2;
                bool flag = true;
                this.promotedType = GetCommonType(minRange.GetType(), maxRange.GetType());
                if ((this.promotedType != null) && LanguagePrimitives.TryConvertTo(minRange, this.promotedType, out obj2))
                {
                    minRange = obj2;
                    if (LanguagePrimitives.TryConvertTo(maxRange, this.promotedType, out obj2))
                    {
                        maxRange = obj2;
                        flag = false;
                    }
                }
                if (flag)
                {
                    throw new ValidationMetadataException("MinRangeNotTheSameTypeOfMaxRange", null, Metadata.ValidateRangeMinRangeMaxRangeType, new object[] { minRange.GetType().Name, maxRange.GetType().Name });
                }
            }
            else
            {
                this.promotedType = minRange.GetType();
            }
            this.minComparable = minRange as IComparable;
            if (this.minComparable == null)
            {
                throw new ValidationMetadataException("MinRangeNotIComparable", null, Metadata.ValidateRangeNotIComparable, new object[0]);
            }
            this.maxComparable = maxRange as IComparable;
            if (this.minComparable.CompareTo(maxRange) > 0)
            {
                throw new ValidationMetadataException("MaxRangeSmallerThanMinRange", null, Metadata.ValidateRangeMaxRangeSmallerThanMinRange, new object[0]);
            }
            this.minRange = minRange;
            this.maxRange = maxRange;
        }

        private static Type GetCommonType(Type minType, Type maxType)
        {
            Type type = null;
            TypeCode typeCode = LanguagePrimitives.GetTypeCode(minType);
            TypeCode code2 = LanguagePrimitives.GetTypeCode(maxType);
            TypeCode code3 = (typeCode >= code2) ? typeCode : code2;
            if (code3 <= TypeCode.Int32)
            {
                return typeof(int);
            }
            if (code3 <= TypeCode.UInt32)
            {
                return ((LanguagePrimitives.IsSignedInteger(typeCode) || LanguagePrimitives.IsSignedInteger(code2)) ? typeof(double) : typeof(int));
            }
            if (code3 <= TypeCode.Int64)
            {
                return typeof(long);
            }
            if (code3 <= TypeCode.UInt64)
            {
                return ((LanguagePrimitives.IsSignedInteger(typeCode) || LanguagePrimitives.IsSignedInteger(code2)) ? typeof(double) : typeof(ulong));
            }
            if (code3 == TypeCode.Decimal)
            {
                return typeof(decimal);
            }
            if ((code3 != TypeCode.Single) && (code3 != TypeCode.Double))
            {
                return type;
            }
            return typeof(double);
        }

        protected override void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ValidationMetadataException("ArgumentIsEmpty", null, Metadata.ValidateNotNullFailure, new object[0]);
            }
            if (element is PSObject)
            {
                element = ((PSObject) element).BaseObject;
            }
            if (!element.GetType().Equals(this.promotedType))
            {
                object obj2;
                if (!LanguagePrimitives.TryConvertTo(element, this.promotedType, out obj2))
                {
                    throw new ValidationMetadataException("ValidationRangeElementType", null, Metadata.ValidateRangeElementType, new object[] { element.GetType().Name, this.minRange.GetType().Name });
                }
                element = obj2;
            }
            if (this.minComparable.CompareTo(element) > 0)
            {
                throw new ValidationMetadataException("ValidateRangeTooSmall", null, Metadata.ValidateRangeSmallerThanMinRangeFailure, new object[] { element.ToString(), this.minRange.ToString() });
            }
            if (this.maxComparable.CompareTo(element) < 0)
            {
                throw new ValidationMetadataException("ValidateRangeTooBig", null, Metadata.ValidateRangeGreaterThanMaxRangeFailure, new object[] { element.ToString(), this.maxRange.ToString() });
            }
        }

        public object MaxRange
        {
            get
            {
                return this.maxRange;
            }
        }

        public object MinRange
        {
            get
            {
                return this.minRange;
            }
        }
    }
}

