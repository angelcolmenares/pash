using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Internal.Data;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Globalization;

namespace Microsoft.Management.Infrastructure
{
	public sealed class CimClass : IDisposable
	{
		private CimSystemProperties _systemProperties;

		private ClassHandle _classHandle;

		private bool _disposed;

		public CimReadOnlyKeyedCollection<CimMethodDeclaration> CimClassMethods
		{
			get
			{
				this.AssertNotDisposed();
				return new CimMethodDeclarationCollection(this._classHandle);
			}
		}

		public CimReadOnlyKeyedCollection<CimPropertyDeclaration> CimClassProperties
		{
			get
			{
				this.AssertNotDisposed();
				return new CimClassPropertiesCollection(this._classHandle);
			}
		}

		public CimReadOnlyKeyedCollection<CimQualifier> CimClassQualifiers
		{
			get
			{
				this.AssertNotDisposed();
				return new CimClassQualifierCollection(this._classHandle);
			}
		}

		public CimClass CimSuperClass
		{
			get
			{
				ClassHandle classHandle = null;
				this.AssertNotDisposed();
				MiResult parentClass = ClassMethods.GetParentClass(this._classHandle, out classHandle);
				MiResult miResult = parentClass;
				if (miResult != MiResult.INVALID_SUPERCLASS)
				{
					CimException.ThrowIfMiResultFailure(parentClass);
					return new CimClass(classHandle);
				}
				else
				{
					return null;
				}
			}
		}

		public string CimSuperClassName
		{
			get
			{
				string str = null;
				this.AssertNotDisposed();
				MiResult parentClassName = ClassMethods.GetParentClassName(this._classHandle, out str);
				MiResult miResult = parentClassName;
				if (miResult != MiResult.INVALID_SUPERCLASS)
				{
					CimException.ThrowIfMiResultFailure(parentClassName);
					return str;
				}
				else
				{
					return null;
				}
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
					MiResult serverName = ClassMethods.GetServerName(this._classHandle, out str);
					CimException.ThrowIfMiResultFailure(serverName);
					serverName = ClassMethods.GetClassName(this._classHandle, out str1);
					CimException.ThrowIfMiResultFailure(serverName);
					serverName = ClassMethods.GetNamespace(this._classHandle, out str2);
					CimException.ThrowIfMiResultFailure(serverName);
					cimSystemProperty.UpdateCimSystemProperties(str2, str, str1);
					cimSystemProperty.UpdateSystemPath(CimInstance.GetCimSystemPath(cimSystemProperty, null));
					this._systemProperties = cimSystemProperty;
				}
				return this._systemProperties;
			}
		}

		internal ClassHandle ClassHandle
		{
			get
			{
				this.AssertNotDisposed();
				return this._classHandle;
			}
		}

		internal CimClass (ClassHandle handle)
		{
			this._classHandle = handle;
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
					this._classHandle.Dispose();
					this._classHandle = null;
				}
				this._disposed = true;
				return;
			}
			else
			{
				return;
			}
		}

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return ClassMethods.GetClassHashCode(this.ClassHandle);
		}

		public override string ToString ()
		{
			if (_classHandle == null) {
				return string.Empty;
			}
			object[] @namespace = new object[2];
			@namespace[0] = this.CimSystemProperties.Namespace;
			@namespace[1] = this.CimSystemProperties.ClassName;
			return string.Format(CultureInfo.InvariantCulture, System.Management.Automation.Strings.CimClassToString, @namespace);
		}
	}
}