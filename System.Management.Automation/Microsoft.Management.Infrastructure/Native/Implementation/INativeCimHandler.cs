using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.Native
{
	/// <summary>
	/// Native cim handler interface
	/// </summary>
	internal interface INativeCimHandler
	{
		/// <summary>
		/// Invokes the method.
		/// </summary>
		/// <returns>
		/// The method.
		/// </returns>
		/// <param name='namespaceName'>
		/// Namespace name.
		/// </param>
		/// <param name='className'>
		/// Class name.
		/// </param>
		/// <param name='methodName'>
		/// Method name.
		/// </param>
		/// <param name='instance'>
		/// Instance.
		/// </param>
		/// <param name='inSignature'>
		/// In signature.
		/// </param>
		NativeCimInstance InvokeMethod (string namespaceName, string className, string methodName, NativeCimInstance instance, NativeCimInstance inSignature);

		/// <summary>
		/// Queries the instances.
		/// </summary>
		/// <returns>
		/// The instances.
		/// </returns>
		/// <param name='namespaceName'>
		/// Namespace name.
		/// </param>
		/// <param name='queryDialect'>
		/// Query dialect.
		/// </param>
		/// <param name='queryExpression'>
		/// Query expression.
		/// </param>
		/// <param name='keysOnly'>
		/// If set to <c>true</c> keys only.
		/// </param>
		IEnumerable<NativeCimInstance> QueryInstances(NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression, bool keysOnly);

		/// <summary>
		/// Queries the classes.
		/// </summary>
		/// <returns>
		/// The classes.
		/// </returns>
		/// <param name='namespaceName'>
		/// Namespace name.
		/// </param>
		/// <param name='queryDialect'>
		/// Query dialect.
		/// </param>
		/// <param name='queryExpression'>
		/// Query expression.
		/// </param>
		IEnumerable<NativeCimClass> QueryClasses(NativeDestinationOptions options, string namespaceName, string queryDialect, string queryExpression);
	}
}

