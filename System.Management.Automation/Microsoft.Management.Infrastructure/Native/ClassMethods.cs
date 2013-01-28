using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Management.Infrastructure.Internal;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class ClassMethods
	{
		private ClassMethods()
		{
		}

		internal static unsafe MiResult Clone(ClassHandle ClassHandleToClone, out ClassHandle clonedClassHandle)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			clonedClassHandle = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(ClassHandleToClone);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_Clone((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (_MI_Class*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)_MIClassPointer;
					clonedClassHandle = new ClassHandle(intPtr, true);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			clonedClassHandle = ClassHandleToClone.Clone ();
			return MiResult.OK;
		}

		internal static int GetClassHashCode(ClassHandle handle)
		{
			/*
			int hashCode;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				IntPtr intPtr = (IntPtr)((long)(*((void*)dangerousHandleAccessor.DangerousGetHandle() + (long)8)));
				hashCode = intPtr.GetHashCode();
			}
			dangerousHandleAccessor.Dispose();
			return hashCode;
			*/

			NativeCimClass classObj = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			return classObj.GetHashCode ();
		}

		internal static unsafe MiResult GetClassName(ClassHandle handle, out string className)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			className = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				UInt16* uInt16Pointer = (UInt16*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetClassName((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16Pointer;
					className = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass classObj = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			className = classObj.ClassName;
			return MiResult.OK;
		}

		internal static MiResult GetClassQualifier_Index(ClassHandle handle, string name, out int index)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			uint num1 = 0;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			index = -1;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetClassQualifierSet((void*)dangerousHandleAccessor.DangerousGetHandle(), ref _MIQualifierSet);
				if (_MIResult == 0)
				{
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(name);
					IntPtr intPtr = hGlobalUni;
					try
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifier(ref _MIQualifierSet, (void*)hGlobalUni, (_MI_Type*)(&_MIType), ref num1, ref _MIValue, ref num);
						if (_MIResult == 0)
						{
							index = num;
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			index = 0;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetElement_GetIndex(ClassHandle handle, string name, out int index)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			index = -1;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(name);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElement(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, (long)0, (long)0, ref num);
					if (_MIResult == 0)
					{
						index = num;
					}
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			index = 0;
			return MiResult.OK;
		}

		internal static MiResult GetElementAt_GetFlags(ClassHandle handle, int index, out MiFlags flags)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElementAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, (long)0, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, (long)0, ref num);
				if (_MIResult == 0)
				{
					flags = (MiFlags)num;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			flags = MiFlags.CLASS;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetElementAt_GetName(ClassHandle handle, int index, out string name)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			name = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				UInt16* uInt16Pointer = (UInt16*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElementAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, ref (UInt16*)((long)0), (long)0, (long)0, (_MI_Type*)((long)0), (long)0, (long)0, (long)0);
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16Pointer;
					name = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			NativeCimProperties properties = NativeCimPropertiesHelper.Deserialize (instance.Properties);
			name = properties.ElementAt (index).Name;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetElementAt_GetReferenceClass(ClassHandle handle, int index, out string referenceClass)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			referenceClass = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				UInt16* uInt16* = (UInt16*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElementAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, (long)0, (long)0, (long)0, (_MI_Type*)((long)0), ref (UInt16*)((long)0), (long)0, (long)0);
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16*;
					referenceClass = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			referenceClass = "";
			return MiResult.OK;
		}

		internal static MiResult GetElementAt_GetType (ClassHandle handle, int index, out MiType type)
		{
			/*
			_MI_Result _MIResult;
			_MI_Type _MIType = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElementAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, (long)0, (long)0, (long)0, (_MI_Type*)(&_MIType), (long)0, (long)0, (long)0);
				if (_MIResult == 0)
				{
					type = (MiType)_MIType;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			NativeCimProperties properties = NativeCimPropertiesHelper.Deserialize (instance.Properties);
			var ctype = properties.ElementAt (index).Type;
			if (ctype == CimType.Unknown) {
				Type propType = properties.ElementAt (index).Value.GetType ();
				ctype = CimConverter.GetCimType (propType);
			}
			type = ctype.ToMiType ();
			return MiResult.OK;
		}

		internal static MiResult GetElementAt_GetValue(ClassHandle handle, int index, out object value)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			value = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElementAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, (long)0, ref _MIValue, (long)0, (_MI_Type*)(&_MIType), (long)0, (long)0, ref num);
				if (_MIResult == 0)
				{
					if (0x20000000 == (num & 0x20000000))
					{
						value = null;
					}
					else
					{
						value = InstanceMethods.ConvertFromMiValue((MiType)_MIType, ref _MIValue);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			NativeCimProperties properties = NativeCimPropertiesHelper.Deserialize (instance.Properties);
			value = properties.ElementAt (index).Value;
			return MiResult.OK;
		}

		internal static MiResult GetElementCount(ClassHandle handle, out int count)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			count = 0;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElementCount((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					count = num;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			NativeCimProperties properties = NativeCimPropertiesHelper.Deserialize (instance.Properties);
			count = properties.Count;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethod_GetIndex (ClassHandle handle, string name, out int index)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			index = -1;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(name);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethod(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, ref num);
					if (_MIResult == 0)
					{
						index = num;
					}
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			var methods  = NativeCimMethodsHelper.Deserialize (instance.Methods);
			int i = 0;
			index = -1;
			foreach (var key in methods) {
				if (key.Name.Equals (name, StringComparison.OrdinalIgnoreCase))
				{
					index = i;
					break;
				}
				i++;
			}
			return index == -1 ? MiResult.METHOD_NOT_FOUND : MiResult.OK;
		}

		internal static unsafe MiResult GetMethodAt_GetName(ClassHandle handle, int methodIndex, int parameterIndex, out string name)
		{
			/*
			_MI_Result _MIResult;
			UInt16* uInt16Pointer = null;
			_MI_QualifierSet _MIQualifierSet;
			_MI_Type _MIType = 0;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			name = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterIndex, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						IntPtr intPtr = (IntPtr)uInt16Pointer;
						name = Marshal.PtrToStringUni(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			var methods  = NativeCimMethodsHelper.Deserialize (instance.Methods);
			var method = methods.ElementAt (methodIndex);
			var properties = NativeCimPropertiesHelper.Deserialize (method.InSignature);
			int i = 0;
			name = null;
			foreach (var key in properties) {
				if (i == parameterIndex)
				{
					name = key.Name;
					break;
				}
				i++;
			}
			return name == null ? MiResult.METHOD_NOT_FOUND : MiResult.OK;
		}

		internal static unsafe MiResult GetMethodAt_GetReferenceClass(ClassHandle handle, int methodIndex, int parameterIndex, out string referenceClass)
		{
			/*
			_MI_Result _MIResult;
			UInt16* uInt16* = null;
			_MI_QualifierSet _MIQualifierSet;
			_MI_Type _MIType = 0;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			referenceClass = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterIndex, ref uInt16Pointer, (_MI_Type*)(&_MIType), ref uInt16*, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						IntPtr intPtr = (IntPtr)uInt16*;
						referenceClass = Marshal.PtrToStringUni(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			referenceClass = "";
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodAt_GetType (ClassHandle handle, int methodIndex, int parameterIndex, out MiType type)
		{
			/*
			_MI_Result _MIResult;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterIndex, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						type = (MiType)_MIType;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			var methods = NativeCimMethodsHelper.Deserialize (instance.Methods);
			var method = methods.ElementAt (methodIndex);
			var properties = NativeCimPropertiesHelper.Deserialize (method.InSignature);
			type = MiType.Instance;
			int i = 0;
			foreach (var property in properties) {
				if (i == parameterIndex)
				{
					type = property.Type.ToMiType();
					break;
				}
			}
			return MiResult.OK;
		}

		internal static MiResult GetMethodCount(ClassHandle handle, out int methodCount)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			methodCount = 0;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodCount((void*)dangerousHandleAccessor.DangerousGetHandle(), ref num);
				if (_MIResult == 0)
				{
					methodCount = num;
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			var methods = NativeCimMethodsHelper.Deserialize (instance.Methods);
			methodCount = methods.Count;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodElement_GetIndex (ClassHandle handle, int methodIndex, string name, out int index)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			_MI_Type _MIType = 0;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			index = -1;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(name);
					IntPtr intPtr = hGlobalUni;
					try
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameter(ref _MIParameterSet, (void*)hGlobalUni, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet, ref num);
						if (_MIResult == 0)
						{
							index = num;
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			index = -1;
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			var methods = NativeCimMethodsHelper.Deserialize (instance.Methods);
			var method = methods.ElementAt (methodIndex);
			var properties = NativeCimPropertiesHelper.Deserialize (method.InSignature);
			int i = 0;
			foreach (var property in properties) {
				if (name.Equals (property.Name, StringComparison.OrdinalIgnoreCase))
				{
					index = i;
					break;
				}
				i++;
			}
			return index == -1 ? MiResult.METHOD_NOT_FOUND : MiResult.OK;
		}

		internal static unsafe MiResult GetMethodElementAt_GetName(ClassHandle handle, int index, out string name)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			name = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				UInt16* uInt16Pointer = (UInt16*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, ref (UInt16*)((long)0), (long)0, (long)0);
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16Pointer;
					name = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/

			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			var methods  = NativeCimMethodsHelper.Deserialize (instance.Methods);
			name = methods.ElementAt (index).Name;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodElementAt_GetType(ClassHandle handle, int index, out MiType type)
		{
			/*
			_MI_Result _MIResult;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, ref uInt16Pointer, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetMethodReturnType(ref _MIParameterSet, (_MI_Type*)(&_MIType), ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						type = (MiType)_MIType;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/

			type = MiType.UInt32;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodGetQualifierElement_GetIndex(ClassHandle handle, int methodIndex, int parameterIndex, string name, out int index)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			uint num1 = 0;
			_MI_QualifierSet _MIQualifierSet;
			_MI_Type _MIType = 0;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			index = -1;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterIndex, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						IntPtr hGlobalUni = Marshal.StringToHGlobalUni(name);
						IntPtr intPtr = hGlobalUni;
						try
						{
							_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifier(ref _MIQualifierSet, (void*)hGlobalUni, (_MI_Type*)(&_MIType), ref num1, ref _MIValue, ref num);
							if (_MIResult == 0)
							{
								index = num;
							}
						}
						finally
						{
							if (intPtr != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(intPtr);
							}
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			index = 0;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodParameterGetQualifierElementAt_GetFlags(ClassHandle handle, int methodIndex, int parameterName, int index, out MiFlags flags)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			_MI_QualifierSet _MIQualifierSet;
			_MI_Type _MIType = 0;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterName, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
						if (_MIResult == 0)
						{
							flags = (MiFlags)num;
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			flags = MiFlags.ANY;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodParameterGetQualifierElementAt_GetName(ClassHandle handle, int methodIndex, int parameterName, int index, out string name)
		{
			/*
			_MI_Result _MIResult;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			_MI_Type _MIType = 0;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			name = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterName, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						UInt16* uInt16Pointer2 = (UInt16*)((long)0);
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
						if (_MIResult == 0)
						{
							IntPtr intPtr = (IntPtr)uInt16Pointer2;
							name = Marshal.PtrToStringUni(intPtr);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			name = "";
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodParameterGetQualifierElementAt_GetType(ClassHandle handle, int methodIndex, int parameterName, int index, out MiType type)
		{
			/*
			_MI_Result _MIResult;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterName, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
						if (_MIResult == 0)
						{
							type = (MiType)_MIType;
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			type = MiType.Instance;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodParameterGetQualifierElementAt_GetValue(ClassHandle handle, int methodIndex, int parameterName, int index, out object value)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			value = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterName, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
						if (_MIResult == 0)
						{
							if (0x20000000 == (num & 0x20000000))
							{
								value = null;
							}
							else
							{
								value = InstanceMethods.ConvertFromMiValue((MiType)_MIType, ref _MIValue);
							}
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			value = null;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodParametersCount(ClassHandle handle, int index, out int parameterCount)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			parameterCount = 0;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, ref uInt16Pointer, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterCount(ref _MIParameterSet, ref num);
					if (_MIResult == 0)
					{
						parameterCount = num;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass>(handle.DangerousGetHandle ());
			var methods = NativeCimMethodsHelper.Deserialize (instance.Methods);
			var method = methods.ElementAt (index);
			var properties = NativeCimPropertiesHelper.Deserialize (method.InSignature);
			parameterCount = properties.Count;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodParametersGetQualifiersCount(ClassHandle handle, int index, int parameterIndex, out int parameterCount)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			_MI_Type _MIType = 0;
			UInt16* uInt16Pointer = null;
			_MI_ParameterSet _MIParameterSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			parameterCount = 0;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), index, ref uInt16Pointer1, (long)0, ref _MIParameterSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_ParameterSet_GetParameterAt(ref _MIParameterSet, parameterIndex, ref uInt16Pointer, (_MI_Type*)(&_MIType), (long)0, ref _MIQualifierSet);
					if (_MIResult == 0)
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierCount(ref _MIQualifierSet, ref num);
						if (_MIResult == 0)
						{
							parameterCount = num;
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			parameterCount = 0;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodQualifierCount(ClassHandle handle, int methodIndex, out int parameterCount)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			parameterCount = 0;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer, ref _MIQualifierSet, (long)0);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierCount(ref _MIQualifierSet, ref num);
					if (_MIResult == 0)
					{
						parameterCount = num;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			parameterCount = 0;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodQualifierElement_GetIndex(ClassHandle handle, int methodIndex, string name, out int index)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			uint num1 = 0;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			index = -1;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer, ref _MIQualifierSet, (long)0);
				if (_MIResult == 0)
				{
					IntPtr hGlobalUni = Marshal.StringToHGlobalUni(name);
					IntPtr intPtr = hGlobalUni;
					try
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifier(ref _MIQualifierSet, (void*)hGlobalUni, (_MI_Type*)(&_MIType), ref num1, ref _MIValue, ref num);
						if (_MIResult == 0)
						{
							index = num;
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			index  = 0;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodQualifierElementAt_GetFlags(ClassHandle handle, int methodIndex, int qualifierIndex, out MiFlags flags)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer, ref _MIQualifierSet, (long)0);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, qualifierIndex, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						flags = (MiFlags)num;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			flags = MiFlags.ANY;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodQualifierElementAt_GetName(ClassHandle handle, int methodIndex, int qualifierIndex, out string name)
		{
			/*
			_MI_Result _MIResult;
			UInt16* uInt16Pointer = null;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer1 = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			name = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer1, ref _MIQualifierSet, (long)0);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, qualifierIndex, ref uInt16Pointer, (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						IntPtr intPtr = (IntPtr)uInt16Pointer;
						name = Marshal.PtrToStringUni(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			name = "";
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodQualifierElementAt_GetType(ClassHandle handle, int methodIndex, int qualifierIndex, out MiType type)
		{
			/*
			_MI_Result _MIResult;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer, ref _MIQualifierSet, (long)0);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, qualifierIndex, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						type = (MiType)_MIType;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			type = MiType.Instance;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetMethodQualifierElementAt_GetValue(ClassHandle handle, int methodIndex, int qualifierIndex, out object value)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			_MI_QualifierSet _MIQualifierSet;
			UInt16* uInt16Pointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			value = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetMethodAt((void*)dangerousHandleAccessor.DangerousGetHandle(), methodIndex, ref uInt16Pointer, ref _MIQualifierSet, (long)0);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, qualifierIndex, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						if (0x20000000 == (num & 0x20000000))
						{
							value = null;
						}
						else
						{
							value = InstanceMethods.ConvertFromMiValue((MiType)_MIType, ref _MIValue);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			value = null;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetNamespace(ClassHandle handle, out string nameSpace)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			nameSpace = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				UInt16* uInt16Pointer = (UInt16*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetNameSpace((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16Pointer;
					nameSpace = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			nameSpace = instance.Namespace;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetParentClass (ClassHandle handle, out ClassHandle superClass)
		{
			/*
			_MI_Result _MIResult;
			_MI_Class* _MIClassPointer = null;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			superClass = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetParentClass((void*)dangerousHandleAccessor.DangerousGetHandle(), ref _MIClassPointer);
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)_MIClassPointer;
					superClass = new ClassHandle(intPtr, true);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			if (handle == null) {
				superClass = null;
			}
			else {
				NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
				if (instance.SystemProperties == null) {
					superClass = null;
				} else {
					var properties = NativeCimPropertiesHelper.Deserialize (instance.SystemProperties);
					var derivations = properties.FirstOrDefault (x => x.Name == "__DERIVATION");
					if (derivations == null)
						superClass = null;
					else {
						string[] classes = derivations.Value as string[];
						if (classes == null || classes.Length == 0)
							superClass = null;
						else {
							string className = classes [0];
							var options = CimNativeApi.GetDestinationOptions (new SessionHandle (instance.SessionHandle));
							var superNativeClass = CimNativeApi.QueryClasses (options, instance.Namespace, CimNativeApi.WQLNamespace, "SELECT * FROM Meta_Class WHERE ClassName = '" + className + "'").FirstOrDefault ();
							superNativeClass.SessionHandle = instance.SessionHandle;
							superClass = new ClassHandle ((IntPtr)CimNativeApi.MarshalledObject.Create<NativeCimClass> (superNativeClass), true);
						}
					}
				}
			}
			return superClass == null ? MiResult.INVALID_SUPERCLASS : MiResult.OK;
		}

		internal static unsafe MiResult GetParentClassName(ClassHandle handle, out string className)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			className = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				UInt16* uInt16Pointer = (UInt16*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetParentClassName((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16Pointer;
					className = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/

			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			var properties = NativeCimPropertiesHelper.Deserialize (instance.SystemProperties);
			var derivations = properties.FirstOrDefault (x => x.Name == "__DERIVATION");
			if (derivations == null)
				className = "";
			else {
				string[] classes = derivations.Value as string[];
				if (classes == null)
					className = "";
				else {
					className = classes[0];
				}
			}
			return MiResult.OK;
		}

		internal static unsafe MiResult GetPropertyQualifier_Count(ClassHandle handle, string name, out int count)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			count = 0;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(name);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElement(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, ref _MIQualifierSet, (long)0, (long)0);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierCount(ref _MIQualifierSet, ref num);
					if (_MIResult == 0)
					{
						count = num;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			count = 0;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetPropertyQualifier_Index(ClassHandle handle, string propertyName, string name, out int index)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			uint num1 = 0;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			index = -1;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(propertyName);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElement(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, ref _MIQualifierSet, (long)0, (long)0);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				if (_MIResult == 0)
				{
					IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(name);
					IntPtr intPtr1 = hGlobalUni1;
					try
					{
						_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifier(ref _MIQualifierSet, (void*)hGlobalUni1, (_MI_Type*)(&_MIType), ref num1, ref _MIValue, ref num);
						if (_MIResult == 0)
						{
							index = num;
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr1);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			index = 0;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetPropertyQualifierElementAt_GetFlags(ClassHandle handle, int index, string propertyName, out MiFlags flags)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(propertyName);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElement(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, ref _MIQualifierSet, (long)0, (long)0);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						flags = (MiFlags)num;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			flags = MiFlags.ANY;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetPropertyQualifierElementAt_GetName(ClassHandle handle, int index, string propertyName, out string name)
		{
			/*
			_MI_Result _MIResult;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			name = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(propertyName);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElement(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, ref _MIQualifierSet, (long)0, (long)0);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				if (_MIResult == 0)
				{
					UInt16* uInt16Pointer = (UInt16*)((long)0);
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						IntPtr intPtr1 = (IntPtr)uInt16Pointer;
						name = Marshal.PtrToStringUni(intPtr1);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			name = "";
			return MiResult.OK;
		}

		internal static unsafe MiResult GetPropertyQualifierElementAt_GetType(ClassHandle handle, int index, string propertyName, out MiType type)
		{
			/*
			_MI_Result _MIResult;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(propertyName);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElement(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, ref _MIQualifierSet, (long)0, (long)0);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						type = (MiType)_MIType;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			type = MiType.Instance;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetPropertyQualifierElementAt_GetValue(ClassHandle handle, int index, string propertyName, out object value)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			value = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MI_Class* _MIClassPointer = (_MI_Class*)((void*)dangerousHandleAccessor.DangerousGetHandle());
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(propertyName);
				IntPtr intPtr = hGlobalUni;
				_MIResult = 0;
				try
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetElement(_MIClassPointer, (void*)hGlobalUni, (long)0, (long)0, (_MI_Type*)((long)0), (long)0, ref _MIQualifierSet, (long)0, (long)0);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						if (0x20000000 == (num & 0x20000000))
						{
							value = null;
						}
						else
						{
							value = InstanceMethods.ConvertFromMiValue((MiType)_MIType, ref _MIValue);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			value = null;
			return MiResult.OK;
		}

		internal static MiResult GetQualifier_Count(ClassHandle handle, out int qualifierCount)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			qualifierCount = 0;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetClassQualifierSet((void*)dangerousHandleAccessor.DangerousGetHandle(), ref _MIQualifierSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierCount(ref _MIQualifierSet, ref num);
					if (_MIResult == 0)
					{
						qualifierCount = num;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/

			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			var qualifiers = NativeCimQualifiersHelper.Deserialize (instance.Qualifiers);
			qualifierCount = qualifiers.Count;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetQualifierElementAt_GetFlags(ClassHandle handle, int index, out MiFlags flags)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Value _MIValue;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetClassQualifierSet((void*)dangerousHandleAccessor.DangerousGetHandle(), ref _MIQualifierSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						flags = (MiFlags)num;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			flags = MiFlags.NULLFLAG;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetQualifierElementAt_GetName(ClassHandle handle, int index, out string name)
		{
			/*
			_MI_Result _MIResult;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			name = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetClassQualifierSet((void*)dangerousHandleAccessor.DangerousGetHandle(), ref _MIQualifierSet);
				if (_MIResult == 0)
				{
					UInt16* uInt16Pointer = (UInt16*)((long)0);
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						IntPtr intPtr = (IntPtr)uInt16Pointer;
						name = Marshal.PtrToStringUni(intPtr);
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			var qualifiers = NativeCimQualifiersHelper.Deserialize (instance.Qualifiers);
			name = qualifiers.ElementAt (index).Name;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetQualifierElementAt_GetType(ClassHandle handle, int index, out MiType type)
		{
			/*
			_MI_Result _MIResult;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			uint num = 0;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetClassQualifierSet((void*)dangerousHandleAccessor.DangerousGetHandle(), ref _MIQualifierSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						type = (MiType)_MIType;
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			var qualifiers = NativeCimQualifiersHelper.Deserialize (instance.Qualifiers);
			type = CimConverter.GetCimType (qualifiers.ElementAt (index).Value.GetType ()).ToMiType ();
			return MiResult.OK;
		}

		internal static unsafe MiResult GetQualifierElementAt_GetValue(ClassHandle handle, int index, out object value)
		{
			/*
			_MI_Result _MIResult;
			uint num = 0;
			_MI_Type _MIType = 0;
			_MI_Value _MIValue;
			_MI_QualifierSet _MIQualifierSet;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			value = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetClassQualifierSet((void*)dangerousHandleAccessor.DangerousGetHandle(), ref _MIQualifierSet);
				if (_MIResult == 0)
				{
					_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_QualifierSet_GetQualifierAt(ref _MIQualifierSet, index, ref (UInt16*)((long)0), (_MI_Type*)(&_MIType), ref num, ref _MIValue);
					if (_MIResult == 0)
					{
						if (0x20000000 == (num & 0x20000000))
						{
							value = null;
						}
						else
						{
							value = InstanceMethods.ConvertFromMiValue((MiType)_MIType, ref _MIValue);
						}
					}
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			var qualifiers = NativeCimQualifiersHelper.Deserialize (instance.Qualifiers);
			value = qualifiers.ElementAt (index).Value;
			return MiResult.OK;
		}

		internal static unsafe MiResult GetServerName(ClassHandle handle, out string serverName)
		{
			/*
			_MI_Result _MIResult;
			DangerousHandleAccessor dangerousHandleAccessor = null;
			serverName = null;
			DangerousHandleAccessor dangerousHandleAccessor = new DangerousHandleAccessor(handle);
			try
			{
				dangerousHandleAccessor = dangerousHandleAccessor;
				UInt16* uInt16Pointer = (UInt16*)((long)0);
				_MIResult = (_MI_Result)<Module>.?A0x37ea71e9.MI_Class_GetServerName((void*)dangerousHandleAccessor.DangerousGetHandle(), ref (UInt16*)((long)0));
				if (_MIResult == 0)
				{
					IntPtr intPtr = (IntPtr)uInt16Pointer;
					serverName = Marshal.PtrToStringUni(intPtr);
				}
			}
			dangerousHandleAccessor.Dispose();
			return (MiResult)_MIResult;
			*/
			NativeCimClass instance = CimNativeApi.MarshalledObject.FromPointer<NativeCimClass> (handle.DangerousGetHandle ());
			serverName = instance.ServerName;
			return MiResult.OK;
		}
	}
}