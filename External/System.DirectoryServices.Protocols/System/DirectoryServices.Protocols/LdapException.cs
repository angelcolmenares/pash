using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.DirectoryServices.Protocols
{
	[Serializable]
	public class LdapException : DirectoryException, ISerializable
	{
		private int errorCode;

		private string serverErrorMessage;

		internal PartialResultsCollection results;

		public int ErrorCode
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorCode;
			}
		}

		public PartialResultsCollection PartialResults
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.results;
			}
		}

		public string ServerErrorMessage
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.serverErrorMessage;
			}
		}

		protected LdapException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.results = new PartialResultsCollection();
		}

		public LdapException()
		{
			this.results = new PartialResultsCollection();
		}

		public LdapException(string message) : base(message)
		{
			this.results = new PartialResultsCollection();
		}

		public LdapException(string message, Exception inner) : base(message, inner)
		{
			this.results = new PartialResultsCollection();
		}

		public LdapException(int errorCode) : base(Res.GetString("DefaultLdapError"))
		{
			this.results = new PartialResultsCollection();
			this.errorCode = errorCode;
		}

		public LdapException(int errorCode, string message) : base(message)
		{
			this.results = new PartialResultsCollection();
			this.errorCode = errorCode;
		}

		public LdapException(int errorCode, string message, string serverErrorMessage) : base(message)
		{
			this.results = new PartialResultsCollection();
			this.errorCode = errorCode;
			this.serverErrorMessage = serverErrorMessage;
		}

		public LdapException(int errorCode, string message, Exception inner) : base(message, inner)
		{
			this.results = new PartialResultsCollection();
			this.errorCode = errorCode;
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData(serializationInfo, streamingContext);
		}
	}
}