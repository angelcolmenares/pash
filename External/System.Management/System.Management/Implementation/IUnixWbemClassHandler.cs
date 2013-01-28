using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.Management
{
	/// <summary>
	/// Unix wbem class handler interface
	/// </summary>
	internal interface IUnixWbemClassHandler
	{
		string PathField { get; }

		IUnixWbemClassHandler New();

		/// <summary>
		/// Get this instance.
		/// </summary>
		IEnumerable<object> Get(string strQuery);
	
		/// <summary>
		/// Get the specified nativeObj.
		/// </summary>
		/// <param name='nativeObj'>
		/// Native object.
		/// </param>
		object Get(object nativeObj);

		/// <summary>
		/// Adds the property.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
		void AddProperty (string key, object obj);

		/// <summary>
		/// Gets the property.
		/// </summary>
		/// <returns>
		/// The property.
		/// </returns>
		/// <param name='key'>
		/// Key.
		/// </param>
		object GetProperty(string key);

		/// <summary>
		/// Invokes the method.
		/// </summary>
		/// <returns>
		/// The method.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		IUnixWbemClassHandler InvokeMethod(string methodName, IUnixWbemClassHandler obj);

		IUnixWbemClassHandler WithProperty(string key, object obj);

		IUnixWbemClassHandler WithMethod(string key, UnixCimMethodInfo methodInfo);

		/// <summary>
		/// Adds the method.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='method'>
		/// Method.
		/// </param>
		void AddMethod (string key, UnixCimMethodInfo method);

		
		UnixWbemQualiferInfo GetQualifier(string name);

		UnixWbemQualiferInfo GetQualifier(int index);

		IEnumerable<string> QualifierNames { get; }

		IDictionary<string, object> Properties { get; }

		IEnumerable<string> PropertyNames { get; }

		IEnumerable<string> SystemPropertyNames { get; }

		IEnumerable<UnixWbemPropertyInfo> PropertyInfos { get; }

		IEnumerable<UnixWbemPropertyInfo> SystemPropertyInfos { get; }

		IEnumerable<string> MethodNames { get; }
		
		IEnumerable<UnixCimMethodInfo> Methods { get; }

		UnixCimMethodInfo NextMethod();
	}
}

