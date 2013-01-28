using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Internal.Data;
using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Serialization;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Management.Infrastructure
{
	[Serializable]
	public sealed class CimInstance : IDisposable, ICloneable, ISerializable
	{
		private const string serializationId_MiXml = "MI_XML";

		private const string serializationId_CimSessionComputerName = "CSCN";

		private readonly SharedInstanceHandle _myHandle;

		private CimSystemProperties _systemProperties;

		private static bool bRegistryRead;

		private static bool bNotSupportedAPIBehavior;

		private static string tempFileName;

		private readonly static object _logThreadSafetyLock;

		private static StreamWriter _streamWriter;

		private bool _disposed;

		private Guid _CimSessionInstanceID;

		private string _CimSessionComputerName;

		public CimClass CimClass
		{
			get
			{

				ClassHandle classHandle = null;
				this.AssertNotDisposed();
				MiResult @class = InstanceMethods.GetClass(this.InstanceHandle, out classHandle);
				if (classHandle == null || @class != MiResult.OK)
				{
					return null;
				}
				else
				{
					return new CimClass(classHandle);
				}

			}
		}

		public CimKeyedCollection<CimProperty> CimInstanceProperties
		{
			get
			{
				this.AssertNotDisposed();
				return new CimPropertiesCollection(this._myHandle, this);
			}
		}

		public CimSystemProperties CimSystemProperties
		{
			get
			{
				string str = null;
				string str1 = null;
				string str2 = null;
				this.AssertNotDisposed();
				if (this._systemProperties == null)
				{
					CimSystemProperties cimSystemProperty = new CimSystemProperties();
					MiResult serverName = InstanceMethods.GetServerName(this.InstanceHandle, out str);
					CimException.ThrowIfMiResultFailure(serverName);
					serverName = InstanceMethods.GetClassName(this.InstanceHandle, out str1);
					CimException.ThrowIfMiResultFailure(serverName);
					serverName = InstanceMethods.GetNamespace(this.InstanceHandle, out str2);
					CimException.ThrowIfMiResultFailure(serverName);
					cimSystemProperty.UpdateCimSystemProperties(str2, str, str1);
					cimSystemProperty.UpdateSystemPath(CimInstance.GetCimSystemPath(cimSystemProperty, null));
					this._systemProperties = cimSystemProperty;
				}
				return this._systemProperties;
			}
		}

		internal InstanceHandle InstanceHandle
		{
			get
			{
				this.AssertNotDisposed();
				return this._myHandle.Handle;
			}
		}

		static CimInstance()
		{
			CimInstance.bRegistryRead = false;
			CimInstance.bNotSupportedAPIBehavior = false;
			CimInstance.tempFileName = "NotSupportedAPIsCallstack.txt";
			CimInstance._logThreadSafetyLock = new object();
		}

		internal CimInstance(InstanceHandle handle, SharedInstanceHandle parentHandle)
		{
			this._CimSessionInstanceID = Guid.Empty;
			this._myHandle = new SharedInstanceHandle(handle, parentHandle);
		}

		public CimInstance(CimInstance cimInstanceToClone)
		{
			this._CimSessionInstanceID = Guid.Empty;
			if (cimInstanceToClone != null)
			{
				InstanceHandle instanceHandle = cimInstanceToClone.InstanceHandle.Clone();
				this._myHandle = new SharedInstanceHandle(instanceHandle);
				return;
			}
			else
			{
				throw new ArgumentNullException("cimInstanceToClone");
			}
		}

		public CimInstance(string className) : this(className, null)
		{
		}

		public CimInstance(string className, string namespaceName)
		{
			InstanceHandle instanceHandle = null;
			this._CimSessionInstanceID = Guid.Empty;
			if (className != null)
			{
				MiResult miResult = ApplicationMethods.NewInstance(CimApplication.Handle, className, null, out instanceHandle);
				MiResult miResult1 = miResult;
				if (miResult1 != MiResult.INVALID_PARAMETER)
				{
					CimException.ThrowIfMiResultFailure(miResult);
					this._myHandle = new SharedInstanceHandle(instanceHandle);
					if (namespaceName != null)
					{
						miResult = InstanceMethods.SetNamespace(this._myHandle.Handle, namespaceName);
						CimException.ThrowIfMiResultFailure(miResult);
					}
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("className");
				}
			}
			else
			{
				throw new ArgumentNullException("className");
			}
		}

		public CimInstance(CimClass cimClass)
		{
			InstanceHandle instanceHandle = null;
			this._CimSessionInstanceID = Guid.Empty;
			if (cimClass != null)
			{
				MiResult miResult = ApplicationMethods.NewInstance(CimApplication.Handle, cimClass.CimSystemProperties.ClassName, cimClass.ClassHandle, out instanceHandle);
				if (miResult != MiResult.INVALID_PARAMETER)
				{
					CimException.ThrowIfMiResultFailure(miResult);
					this._myHandle = new SharedInstanceHandle(instanceHandle);
					miResult = InstanceMethods.SetNamespace(this._myHandle.Handle, cimClass.CimSystemProperties.Namespace);
					CimException.ThrowIfMiResultFailure(miResult);
					miResult = InstanceMethods.SetServerName(this._myHandle.Handle, cimClass.CimSystemProperties.ServerName);
					CimException.ThrowIfMiResultFailure(miResult);
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("cimClass");
				}
			}
			else
			{
				throw new ArgumentNullException("cimClass");
			}
		}

		private CimInstance(SerializationInfo info, StreamingContext context)
		{
			this._CimSessionInstanceID = Guid.Empty;
			if (info != null)
			{
				string str = info.GetString("MI_XML");
				byte[] bytes = Encoding.Unicode.GetBytes(str);
				CimDeserializer cimDeserializer = CimDeserializer.Create();
				using (cimDeserializer)
				{
					int num = 0;
					InstanceHandle instanceHandle = cimDeserializer.DeserializeInstanceHandle(bytes, ref num, null);
					this._myHandle = new SharedInstanceHandle(instanceHandle);
				}
				this.SetCimSessionComputerName(info.GetString("CSCN"));
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}

		internal void AssertNotDisposed()
		{
			if (!this._disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().FullName);
			}
		}

		internal static object ConvertFromNativeLayer(object value, SharedInstanceHandle sharedParentHandle = null, CimInstance parent = null, bool clone = false)
		{
			InstanceHandle instanceHandle;
			InstanceHandle instanceHandle1;
			InstanceHandle instanceHandle2 = value as InstanceHandle;
			if (instanceHandle2 == null)
			{
				InstanceHandle[] instanceHandleArray = value as InstanceHandle[];
				if (instanceHandleArray == null)
				{
					return value;
				}
				else
				{
					CimInstance[] cimInstanceArray = new CimInstance[(int)instanceHandleArray.Length];
					for (int i = 0; i < (int)instanceHandleArray.Length; i++)
					{
						InstanceHandle instanceHandle3 = instanceHandleArray[i];
						if (instanceHandle3 != null)
						{
							CimInstance[] cimInstance = cimInstanceArray;
							int num = i;
							if (clone)
							{
								instanceHandle = instanceHandle3.Clone();
							}
							else
							{
								instanceHandle = instanceHandle3;
							}
							cimInstance[num] = new CimInstance(instanceHandle, sharedParentHandle);
							if (parent != null)
							{
								cimInstanceArray[i].SetCimSessionComputerName(parent.GetCimSessionComputerName());
								cimInstanceArray[i].SetCimSessionInstanceId(parent.GetCimSessionInstanceId());
							}
						}
						else
						{
							cimInstanceArray[i] = null;
						}
					}
					return cimInstanceArray;
				}
			}
			else
			{
				if (clone)
				{
					instanceHandle1 = instanceHandle2.Clone();
				}
				else
				{
					instanceHandle1 = instanceHandle2;
				}
				CimInstance cimInstance1 = new CimInstance(instanceHandle1, sharedParentHandle);
				if (parent != null)
				{
					cimInstance1.SetCimSessionComputerName(parent.GetCimSessionComputerName());
					cimInstance1.SetCimSessionInstanceId(parent.GetCimSessionInstanceId());
				}
				return cimInstance1;
			}
		}

		internal static object ConvertToNativeLayer(object value, CimType cimType)
		{
			CimInstance cimInstance = value as CimInstance;
			if (cimInstance == null)
			{
				CimInstance[] cimInstanceArray = value as CimInstance[];
				if (cimInstanceArray == null)
				{
					if (cimType != CimType.Unknown)
					{
						return CimProperty.ConvertToNativeLayer(value, cimType);
					}
					else
					{
						return value;
					}
				}
				else
				{
					InstanceHandle[] instanceHandle = new InstanceHandle[(int)cimInstanceArray.Length];
					for (int i = 0; i < (int)cimInstanceArray.Length; i++)
					{
						CimInstance cimInstance1 = cimInstanceArray[i];
						if (cimInstance1 != null)
						{
							instanceHandle[i] = cimInstance1.InstanceHandle;
						}
						else
						{
							instanceHandle[i] = null;
						}
					}
					return instanceHandle;
				}
			}
			else
			{
				return cimInstance.InstanceHandle;
			}
		}

		internal static object ConvertToNativeLayer(object value)
		{
			return CimInstance.ConvertToNativeLayer(value, CimType.Unknown);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
					this._myHandle.Release();
				}
				this._disposed = true;
				return;
			}
			else
			{
				return;
			}
		}

		public string GetCimSessionComputerName()
		{
			return this._CimSessionComputerName;
		}

		public Guid GetCimSessionInstanceId()
		{
			return this._CimSessionInstanceID;
		}

		internal static string GetCimSystemPath(CimSystemProperties sysProperties, IEnumerator cimPropertiesEnumerator)
		{
			return string.Format (@"//{0}/{1}/{2}", sysProperties.ServerName, sysProperties.Namespace, sysProperties.ClassName);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				CimSerializer cimSerializer = CimSerializer.Create();
				using (cimSerializer)
				{
					byte[] numArray = cimSerializer.Serialize(this, InstanceSerializationOptions.IncludeClasses);
					string str = Encoding.Unicode.GetString(numArray);
					info.AddValue("MI_XML", str);
				}
				info.AddValue("CSCN", this.GetCimSessionComputerName());
				return;
			}
			else
			{
				throw new ArgumentNullException("info");
			}
		}

		internal static void NotSupportedAPIBehaviorLog(string propertyName)
		{
			if (!CimInstance.bRegistryRead)
			{
				lock (CimInstance._logThreadSafetyLock)
				{
					if (!CimInstance.bRegistryRead)
					{
						try
						{
							if (System.Management.Automation.OSHelper.IsWindows) {
								object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Management Infrastructure", "NotSupportedAPIBehavior", null);
								CimInstance.bRegistryRead = true;
								if (value != null && value.ToString() == "1")
								{
									string tempPath = Path.GetTempPath();
									object[] ticks = new object[4];
									ticks[0] = tempPath;
									ticks[1] = "\\";
									DateTime now = DateTime.Now;
									ticks[2] = now.Ticks;
									ticks[3] = "-NotSupportedAPIsCallstack.txt";
									CimInstance.tempFileName = string.Concat(ticks);
									CimInstance._streamWriter = File.AppendText(CimInstance.tempFileName);
									CimInstance._streamWriter.AutoFlush = true;
									CimInstance.bNotSupportedAPIBehavior = true;
								}
							}
							else {
								CimInstance.bRegistryRead = true;
								string tempPath = Path.GetTempPath();
								object[] ticks = new object[4];
								ticks[0] = tempPath;
								ticks[1] = "\\";
								DateTime now = DateTime.Now;
								ticks[2] = now.Ticks;
								ticks[3] = "-NotSupportedAPIsCallstack.txt";
								CimInstance.tempFileName = string.Concat(ticks);
								CimInstance._streamWriter = File.AppendText(CimInstance.tempFileName);
								CimInstance._streamWriter.AutoFlush = true;
								CimInstance.bNotSupportedAPIBehavior = true;
							}
						}
						catch (Exception exception)
						{
							CimInstance.bRegistryRead = true;
						}
					}
				}
			}
			if (CimInstance.bNotSupportedAPIBehavior)
			{
				CimInstance._streamWriter.WriteLine(propertyName);
			}
		}

		internal void SetCimSessionComputerName(string computerName)
		{
			this._CimSessionComputerName = computerName;
		}

		internal void SetCimSessionInstanceId(Guid instanceID)
		{
			this._CimSessionInstanceID = instanceID;
		}

		object System.ICloneable.Clone()
		{
			return new CimInstance(this);
		}

		public override string ToString()
		{
			string className;
			CimProperty item = this.CimInstanceProperties["Caption"];
			string value = null;
			if (item != null)
			{
				value = item.Value as string;
			}
			string str = ", ";
			CimKeyedCollection<CimProperty> cimInstanceProperties = this.CimInstanceProperties;
			string str1 = string.Join<CimProperty>(str, cimInstanceProperties.Where<CimProperty>((CimProperty p) => CimFlags.Key == (p.Flags & CimFlags.Key)));
			if (!string.IsNullOrEmpty(str1) || !string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (!string.IsNullOrEmpty(str1))
					{
						object[] objArray = new object[3];
						objArray[0] = this.CimSystemProperties.ClassName;
						objArray[1] = str1;
						objArray[2] = value;
						className = string.Format(CultureInfo.InvariantCulture, System.Management.Automation.Strings.CimInstanceToStringFullData, objArray);
					}
					else
					{
						object[] className1 = new object[2];
						className1[0] = this.CimSystemProperties.ClassName;
						className1[1] = value;
						className = string.Format(CultureInfo.InvariantCulture, System.Management.Automation.Strings.CimInstanceToStringNoKeys, className1);
					}
				}
				else
				{
					object[] objArray1 = new object[2];
					objArray1[0] = this.CimSystemProperties.ClassName;
					objArray1[1] = str1;
					className = string.Format(CultureInfo.InvariantCulture, System.Management.Automation.Strings.CimInstanceToStringNoCaption, objArray1);
				}
			}
			else
			{
				className = this.CimSystemProperties.ClassName;
			}
			return className;
		}
	}
}