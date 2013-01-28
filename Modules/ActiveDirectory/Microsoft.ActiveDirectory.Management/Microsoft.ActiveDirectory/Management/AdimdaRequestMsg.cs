using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal abstract class AdimdaRequestMsg : AdwsRequestMsg
	{
		protected AdimdaRequestMsg(string instance) : base(instance)
		{
			this.AddHeaders();
		}

		protected AdimdaRequestMsg(string instance, string objectReferenceProperty) : base(instance, objectReferenceProperty)
		{
			this.AddHeaders();
		}

		private void AddHeaders()
		{
			this.Headers.Add(MessageHeader.CreateHeader("IdentityManagementOperation", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess", new object(), true));
		}

		protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
		{
			base.OnWriteStartEnvelope(writer);
			writer.WriteXmlnsAttribute("da", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
		}
	}
}