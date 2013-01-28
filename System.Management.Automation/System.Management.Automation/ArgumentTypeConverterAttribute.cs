namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;

    internal sealed class ArgumentTypeConverterAttribute : ArgumentTransformationAttribute
    {
        private Type[] _convertTypes;

        internal ArgumentTypeConverterAttribute(params Type[] types)
        {
            this._convertTypes = types;
        }

        private static void CheckBoolValue(object value, Type boolType)
        {
            if (value != null)
            {
                Type type = value.GetType();
                if (type == typeof(PSObject))
                {
                    type = ((PSObject) value).BaseObject.GetType();
                }
                if (!LanguagePrimitives.IsNumeric(Type.GetTypeCode(type)) && !LanguagePrimitives.IsBoolOrSwitchParameterType(type))
                {
                    ThrowPSInvalidBooleanArgumentCastException(type, boolType);
                }
            }
            else if (!(boolType.IsGenericType && (boolType.GetGenericTypeDefinition() == typeof(Nullable<>))) && LanguagePrimitives.IsBooleanType(boolType))
            {
                ThrowPSInvalidBooleanArgumentCastException(null, boolType);
            }
        }

        internal static void ThrowPSInvalidBooleanArgumentCastException(Type resultType, Type convertType)
        {
            throw new PSInvalidCastException("InvalidCastExceptionUnsupportedParameterType", null, ExtendedTypeSystem.InvalidCastExceptionForBooleanArgumentValue, new object[] { resultType, convertType });
        }

        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            return this.Transform(engineIntrinsics, inputData, false, false);
        }

        internal object Transform(EngineIntrinsics engineIntrinsics, object inputData, bool bindingParameters, bool bindingScriptCmdlet)
        {
            if (this._convertTypes == null)
            {
                return inputData;
            }
            object obj2 = inputData;
            try
            {
                for (int i = 0; i < this._convertTypes.Length; i++)
                {
                    if (bindingParameters)
                    {
                        if (this._convertTypes[i].Equals(typeof(PSReference)))
                        {
                            object baseObject;
                            PSObject obj4 = obj2 as PSObject;
                            if (obj4 != null)
                            {
                                baseObject = obj4.BaseObject;
                            }
                            else
                            {
                                baseObject = obj2;
                            }
                            if (!(baseObject is PSReference))
                            {
                                throw new PSInvalidCastException("InvalidCastExceptionReferenceTypeExpected", null, ExtendedTypeSystem.ReferenceTypeExpected, new object[0]);
                            }
                        }
                        else
                        {
                            object obj5;
                            PSObject obj6 = obj2 as PSObject;
                            if (obj6 != null)
                            {
                                obj5 = obj6.BaseObject;
                            }
                            else
                            {
                                obj5 = obj2;
                            }
                            PSReference reference2 = obj5 as PSReference;
                            if (reference2 != null)
                            {
                                obj2 = reference2.Value;
                            }
                            if (bindingScriptCmdlet && (this._convertTypes[i] == typeof(string)))
                            {
                                obj5 = PSObject.Base(obj2);
                                if ((obj5 != null) && obj5.GetType().IsArray)
                                {
                                    throw new PSInvalidCastException("InvalidCastFromAnyTypeToString", null, ExtendedTypeSystem.InvalidCastCannotRetrieveString, new object[0]);
                                }
                            }
                        }
                    }
                    if (LanguagePrimitives.IsBoolOrSwitchParameterType(this._convertTypes[i]))
                    {
                        CheckBoolValue(obj2, this._convertTypes[i]);
                    }
                    if (bindingScriptCmdlet)
                    {
                        ParameterCollectionTypeInformation information = new ParameterCollectionTypeInformation(this._convertTypes[i]);
                        if ((information.ParameterCollectionType != ParameterCollectionType.NotCollection) && LanguagePrimitives.IsBoolOrSwitchParameterType(information.ElementType))
                        {
                            IList iList = ParameterBinderBase.GetIList(obj2);
                            if (iList != null)
                            {
                                foreach (object obj7 in iList)
                                {
                                    CheckBoolValue(obj7, information.ElementType);
                                }
                            }
                            else
                            {
                                CheckBoolValue(obj2, information.ElementType);
                            }
                        }
                    }
                    obj2 = LanguagePrimitives.ConvertTo(obj2, this._convertTypes[i], CultureInfo.InvariantCulture);
                }
            }
            catch (PSInvalidCastException exception)
            {
                throw new ArgumentTransformationMetadataException(exception.Message, exception);
            }
            return obj2;
        }

        internal Type TargetType
        {
            get
            {
                if (this._convertTypes != null)
                {
                    return this._convertTypes.LastOrDefault<Type>();
                }
                return null;
            }
        }
    }
}

