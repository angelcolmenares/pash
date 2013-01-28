using System;
using System.Runtime;
using System.Runtime.Serialization;

namespace System.DirectoryServices.Protocols
{
	[Serializable]
	public class BerConversionException : DirectoryException
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected BerConversionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public BerConversionException() : base(Res.GetString("BerConversionError"))
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public BerConversionException(string message) : base(message)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public BerConversionException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}