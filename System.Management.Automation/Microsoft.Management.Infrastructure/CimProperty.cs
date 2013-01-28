namespace Microsoft.Management.Infrastructure
{
    using Microsoft.Management.Infrastructure.Internal;
    using Microsoft.Management.Infrastructure.Internal.Data;
    using System;
    using System.Globalization;

    public abstract class CimProperty
    {
        internal CimProperty()
        {
        }

        internal static object ConvertToNativeLayer(object value, Microsoft.Management.Infrastructure.CimType cimType)
        {
            if (value == null)
            {
                return null;
            }
            object[] objArray = value as object[];
            switch (cimType)
            {
                case Microsoft.Management.Infrastructure.CimType.Boolean:
                    return Convert.ToBoolean(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.UInt8:
                    return Convert.ToByte(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.SInt8:
                    return Convert.ToSByte(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.UInt16:
                    return Convert.ToUInt16(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.SInt16:
                    return Convert.ToInt16(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.UInt32:
                    return Convert.ToUInt32(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.SInt32:
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.UInt64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.SInt64:
                    return Convert.ToInt64(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.Real32:
                    return Convert.ToSingle(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.Real64:
                    return Convert.ToDouble(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.Char16:
                    return Convert.ToChar(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.DateTime:
                    if ((value is TimeSpan) || (value is DateTime))
                    {
                        return value;
                    }
                    return Convert.ToDateTime(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.String:
                    if (value is bool)
                    {
                        return Convert.ToString(value, CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture);
                    }
                    return Convert.ToString(value, CultureInfo.InvariantCulture);

                case Microsoft.Management.Infrastructure.CimType.Reference:
                case Microsoft.Management.Infrastructure.CimType.Instance:
                case Microsoft.Management.Infrastructure.CimType.ReferenceArray:
                case Microsoft.Management.Infrastructure.CimType.InstanceArray:
                    return value;

                case Microsoft.Management.Infrastructure.CimType.BooleanArray:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    bool[] flagArray = new bool[objArray.Length];
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        flagArray[i] = Convert.ToBoolean(objArray[i], CultureInfo.InvariantCulture);
                    }
                    return flagArray;
                }
                case Microsoft.Management.Infrastructure.CimType.UInt8Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    byte[] buffer = new byte[objArray.Length];
                    for (int j = 0; j < objArray.Length; j++)
                    {
                        buffer[j] = Convert.ToByte(objArray[j], CultureInfo.InvariantCulture);
                    }
                    return buffer;
                }
                case Microsoft.Management.Infrastructure.CimType.SInt8Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    sbyte[] numArray6 = new sbyte[objArray.Length];
                    for (int k = 0; k < objArray.Length; k++)
                    {
                        numArray6[k] = Convert.ToSByte(objArray[k], CultureInfo.InvariantCulture);
                    }
                    return numArray6;
                }
                case Microsoft.Management.Infrastructure.CimType.UInt16Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    ushort[] numArray7 = new ushort[objArray.Length];
                    for (int m = 0; m < objArray.Length; m++)
                    {
                        numArray7[m] = Convert.ToUInt16(objArray[m], CultureInfo.InvariantCulture);
                    }
                    return numArray7;
                }
                case Microsoft.Management.Infrastructure.CimType.SInt16Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    short[] numArray3 = new short[objArray.Length];
                    for (int n = 0; n < objArray.Length; n++)
                    {
                        numArray3[n] = Convert.ToInt16(objArray[n], CultureInfo.InvariantCulture);
                    }
                    return numArray3;
                }
                case Microsoft.Management.Infrastructure.CimType.UInt32Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    uint[] numArray8 = new uint[objArray.Length];
                    for (int num11 = 0; num11 < objArray.Length; num11++)
                    {
                        numArray8[num11] = Convert.ToUInt32(objArray[num11], CultureInfo.InvariantCulture);
                    }
                    return numArray8;
                }
                case Microsoft.Management.Infrastructure.CimType.SInt32Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    int[] numArray4 = new int[objArray.Length];
                    for (int num6 = 0; num6 < objArray.Length; num6++)
                    {
                        numArray4[num6] = Convert.ToInt32(objArray[num6], CultureInfo.InvariantCulture);
                    }
                    return numArray4;
                }
                case Microsoft.Management.Infrastructure.CimType.UInt64Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    ulong[] numArray9 = new ulong[objArray.Length];
                    for (int num12 = 0; num12 < objArray.Length; num12++)
                    {
                        numArray9[num12] = Convert.ToUInt64(objArray[num12], CultureInfo.InvariantCulture);
                    }
                    return numArray9;
                }
                case Microsoft.Management.Infrastructure.CimType.SInt64Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    long[] numArray5 = new long[objArray.Length];
                    for (int num7 = 0; num7 < objArray.Length; num7++)
                    {
                        numArray5[num7] = Convert.ToInt64(objArray[num7], CultureInfo.InvariantCulture);
                    }
                    return numArray5;
                }
                case Microsoft.Management.Infrastructure.CimType.Real32Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    float[] numArray = new float[objArray.Length];
                    for (int num3 = 0; num3 < objArray.Length; num3++)
                    {
                        numArray[num3] = Convert.ToSingle(objArray[num3], CultureInfo.InvariantCulture);
                    }
                    return numArray;
                }
                case Microsoft.Management.Infrastructure.CimType.Real64Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    double[] numArray2 = new double[objArray.Length];
                    for (int num4 = 0; num4 < objArray.Length; num4++)
                    {
                        numArray2[num4] = Convert.ToDouble(objArray[num4], CultureInfo.InvariantCulture);
                    }
                    return numArray2;
                }
                case Microsoft.Management.Infrastructure.CimType.Char16Array:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    char[] chArray = new char[objArray.Length];
                    for (int num2 = 0; num2 < objArray.Length; num2++)
                    {
                        chArray[num2] = Convert.ToChar(objArray[num2], CultureInfo.InvariantCulture);
                    }
                    return chArray;
                }
                case Microsoft.Management.Infrastructure.CimType.DateTimeArray:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    object[] objArray2 = new object[objArray.Length];
                    for (int num14 = 0; num14 < objArray.Length; num14++)
                    {
                        if ((objArray[num14] is TimeSpan) || (objArray[num14] is DateTime))
                        {
                            objArray2[num14] = objArray[num14];
                        }
                        else
                        {
                            objArray2[num14] = Convert.ToDateTime(objArray[num14], CultureInfo.InvariantCulture);
                        }
                    }
                    return objArray2;
                }
                case Microsoft.Management.Infrastructure.CimType.StringArray:
                {
                    if (objArray == null)
                    {
                        return value;
                    }
                    string[] strArray = new string[objArray.Length];
                    for (int num9 = 0; num9 < objArray.Length; num9++)
                    {
                        if (objArray[num9] is bool)
                        {
                            strArray[num9] = Convert.ToString(objArray[num9], CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            strArray[num9] = Convert.ToString(objArray[num9], CultureInfo.InvariantCulture);
                        }
                    }
                    return strArray;
                }
            }
            return value;
        }

        public static CimProperty Create(string name, object value, CimFlags flags)
        {
            Microsoft.Management.Infrastructure.CimType cimTypeFromDotNetValueOrThrowAnException = CimConverter.GetCimTypeFromDotNetValueOrThrowAnException(value);
            return Create(name, value, cimTypeFromDotNetValueOrThrowAnException, flags);
        }

        public static CimProperty Create(string name, object value, Microsoft.Management.Infrastructure.CimType type, CimFlags flags)
        {
            return new CimPropertyStandalone(name, value, type, flags);
        }

        public override string ToString()
        {
            return Helpers.ToStringFromNameAndValue(this.Name, this.Value);
        }

        public abstract Microsoft.Management.Infrastructure.CimType CimType { get; }

        public abstract CimFlags Flags { get; }

        public virtual bool IsValueModified
        {
            get
            {
                CimFlags flags = this.Flags;
                bool flag = CimFlags.NotModified == (flags & CimFlags.NotModified);
                return !flag;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract object Value { get; set; }
    }
}

