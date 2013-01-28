using System;
using System.Management.Classes;

namespace System.Management
{
	/// <summary>
	/// Unix wbem method creator.
	/// </summary>
	internal static class UnixWbemMethodCreator
	{
		/// <summary>
		/// Creates the signature.
		/// </summary>
		/// <param name='info'>
		/// Info.
		/// </param>
		/// <param name='ppInSignature'>
		/// Pp in signature.
		/// </param>
		/// <param name='ppOutSignature'>
		/// Pp out signature.
		/// </param>
		public static void CreateSignature (UnixCimMethodInfo info, out IWbemClassObject_DoNotMarshal ppInSignature, out IWbemClassObject_DoNotMarshal ppOutSignature)
		{
			Type inType = null;
			Type outType = null; 
			if (string.IsNullOrEmpty (info.InSignatureType)) {
				inType = typeof(UNIX_MethodParameterClass);
			} else {
				inType = Type.GetType (info.InSignatureType, false, true);
			}

			if (string.IsNullOrEmpty (info.OutSignatureType)) {
				outType = typeof(UNIX_MethodParameterClass);
			} else {
				outType = Type.GetType (info.OutSignatureType, false, true);
			}

			var inClass = (UNIX_MethodParameterClass)WMIDatabaseFactory.GetHandler (inType).Get ((object)null);
			var outClass = (UNIX_MethodParameterClass)WMIDatabaseFactory.GetHandler (outType).Get ((object)null);
			
			if (info.InProperties != null) {
				foreach (var property in info.InProperties) {
					inClass.RegisterProperty (property);
				}
			}

			outClass.RegisterProperty (new UnixWbemPropertyInfo { Name = "ReturnValue", Type = CimType.UInt32, Flavor = 0  });

			if (info.OutProperties != null) 
			{
				foreach (var property in info.OutProperties) {
					outClass.RegisterProperty (property);
				}
			}

			ppInSignature = new UnixWbemClassObject(inClass);
			ppOutSignature = new UnixWbemClassObject(outClass);
		}
	}
}

