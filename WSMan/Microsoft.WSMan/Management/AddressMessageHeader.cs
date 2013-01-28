using System;
using System.ServiceModel.Channels;

namespace Microsoft.WSMan.Management
{
	public class AddressMessageHeader : MessageHeader
	{
		private AddressHeader _header;

		public AddressMessageHeader (AddressHeader header)
		{
			_header = header;
		}

		#region implemented abstract members of MessageHeader

		protected override void OnWriteHeaderContents (System.Xml.XmlDictionaryWriter writer, MessageVersion version)
		{
			_header.WriteAddressHeaderContents (writer);
		}

		#endregion

		#region implemented abstract members of MessageHeaderInfo

		public override string Name {
			get {
				return _header.Name;
			}
		}

		public override string Namespace {
			get {
				return _header.Namespace;
			}
		}

		#endregion
	}
}

