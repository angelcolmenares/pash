using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Transfer
{
   public class SerializerBodyWriter : BodyWriter
   {
      private readonly object _toSerialize;

      public SerializerBodyWriter(object toSerialize)
         : base(false)
      {
         _toSerialize = toSerialize;
      }

      protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
      {
         if (_toSerialize != null)
         {
            IXmlSerializable serializable = _toSerialize as IXmlSerializable;
            if (serializable != null)
            {
               serializable.WriteXml(writer);
            }
            else
            {
               XmlSerializer xs = new XmlSerializer(_toSerialize.GetType());
               xs.Serialize(writer, _toSerialize);
            }
         }
      }
   }
}