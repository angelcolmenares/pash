using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.Management.Faults
{
	internal class AdwsFault
	{
		private MessageFault _fault;

		private object _detail;

		private string _detailText;

		public FaultCode Code
		{
			get
			{
				return this._fault.Code;
			}
		}

		public object Detail
		{
			get
			{
				return this._detail;
			}
		}

		public string DetailText
		{
			get
			{
				return this._detailText;
			}
		}

		public bool HasDetail
		{
			get
			{
				return this._fault.HasDetail;
			}
		}

		public MessageFault Message
		{
			get
			{
				return this._fault;
			}
		}

		public FaultReason Reason
		{
			get
			{
				return this._fault.Reason;
			}
		}

		internal AdwsFault(MessageFault fault)
		{
			this._fault = fault;
		}

		internal AdwsFault(MessageFault fault, object detail) : this(fault)
		{
			this._detail = detail;
		}

		internal AdwsFault(MessageFault fault, string detailText) : this(fault)
		{
			this._detailText = detailText;
		}
	}
}