using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSyntax
	{
		private static ADSyntax[] _syntaxConstants;

		private ADAttributeSyntax _attributeSyntax;

		private string _syntaxOid;

		private static Dictionary<string, ADAttributeSyntax> _syntaxMap;

		private ADAttributeSyntax AttributeSyntax
		{
			get
			{
				return this._attributeSyntax;
			}
		}

		private string SyntaxOid
		{
			get
			{
				return this._syntaxOid;
			}
		}

		static ADSyntax()
		{
			ADSyntax[] aDSyntax = new ADSyntax[89];
			aDSyntax[0] = new ADSyntax(ADAttributeSyntax.CaseIgnoreString, "1.2.840.113556.1.4.1221");
			aDSyntax[1] = new ADSyntax(ADAttributeSyntax.OctetString, "1.2.840.113556.1.4.1222");
			aDSyntax[2] = new ADSyntax(ADAttributeSyntax.CaseExactString, "1.2.840.113556.1.4.1362");
			aDSyntax[3] = new ADSyntax(ADAttributeSyntax.DNWithBinary, "1.2.840.113556.1.4.903");
			aDSyntax[4] = new ADSyntax(ADAttributeSyntax.DNWithString, "1.2.840.113556.1.4.904");
			aDSyntax[5] = new ADSyntax(ADAttributeSyntax.CaseIgnoreString, "1.2.840.113556.1.4.905");
			aDSyntax[6] = new ADSyntax(ADAttributeSyntax.Int64, "1.2.840.113556.1.4.906");
			aDSyntax[7] = new ADSyntax(ADAttributeSyntax.SecurityDescriptor, "1.2.840.113556.1.4.907");
			aDSyntax[8] = new ADSyntax(ADAttributeSyntax.DN, "1.3.6.1.4.1.1466.115.121.1.12");
			aDSyntax[9] = new ADSyntax(ADAttributeSyntax.DirectoryString, "1.3.6.1.4.1.1466.115.121.1.15");
			aDSyntax[10] = new ADSyntax(ADAttributeSyntax.AccessPointDN, "1.3.6.1.4.1.1466.115.121.1.2");
			aDSyntax[11] = new ADSyntax(ADAttributeSyntax.GeneralizedTime, "1.3.6.1.4.1.1466.115.121.1.24");
			aDSyntax[12] = new ADSyntax(ADAttributeSyntax.IA5String, "1.3.6.1.4.1.1466.115.121.1.26");
			aDSyntax[13] = new ADSyntax(ADAttributeSyntax.Int, "1.3.6.1.4.1.1466.115.121.1.27");
			aDSyntax[14] = new ADSyntax(ADAttributeSyntax.NumericString, "1.3.6.1.4.1.1466.115.121.1.36");
			aDSyntax[15] = new ADSyntax(ADAttributeSyntax.Oid, "1.3.6.1.4.1.1466.115.121.1.38");
			aDSyntax[16] = new ADSyntax(ADAttributeSyntax.OctetString, "1.3.6.1.4.1.1466.115.121.1.40");
			aDSyntax[17] = new ADSyntax(ADAttributeSyntax.PresentationAddress, "1.3.6.1.4.1.1466.115.121.1.43");
			aDSyntax[18] = new ADSyntax(ADAttributeSyntax.PrintableString, "1.3.6.1.4.1.1466.115.121.1.44");
			aDSyntax[19] = new ADSyntax(ADAttributeSyntax.OctetString, "1.3.6.1.4.1.1466.115.121.1.5");
			aDSyntax[20] = new ADSyntax(ADAttributeSyntax.UtcTime, "1.3.6.1.4.1.1466.115.121.1.53");
			aDSyntax[21] = new ADSyntax(ADAttributeSyntax.Bool, "1.3.6.1.4.1.1466.115.121.1.7");
			aDSyntax[22] = new ADSyntax(ADAttributeSyntax.OctetString, "OctetString");
			aDSyntax[23] = new ADSyntax(ADAttributeSyntax.Certificate, "1.3.6.1.4.1.1466.115.121.1.8");
			aDSyntax[24] = new ADSyntax(ADAttributeSyntax.CertificateList, "1.3.6.1.4.1.1466.115.121.1.9");
			aDSyntax[25] = new ADSyntax(ADAttributeSyntax.CertificatePair, "1.3.6.1.4.1.1466.115.121.1.10");
			aDSyntax[26] = new ADSyntax(ADAttributeSyntax.CountryString, "1.3.6.1.4.1.1466.115.121.1.11");
			aDSyntax[27] = new ADSyntax(ADAttributeSyntax.DataQualitySyntax, "1.3.6.1.4.1.1466.115.121.1.13");
			aDSyntax[28] = new ADSyntax(ADAttributeSyntax.DeliveryMethod, "1.3.6.1.4.1.1466.115.121.1.14");
			aDSyntax[29] = new ADSyntax(ADAttributeSyntax.DSAQualitySyntax, "1.3.6.1.4.1.1466.115.121.1.19");
			aDSyntax[30] = new ADSyntax(ADAttributeSyntax.EnhancedGuidE, "1.3.6.1.4.1.1466.115.121.1.21");
			aDSyntax[31] = new ADSyntax(ADAttributeSyntax.FacsimileTelephoneNumer, "1.3.6.1.4.1.1466.115.121.1.22");
			aDSyntax[32] = new ADSyntax(ADAttributeSyntax.Fax, "1.3.6.1.4.1.1466.115.121.1.23");
			aDSyntax[33] = new ADSyntax(ADAttributeSyntax.GuidE, "1.3.6.1.4.1.1466.115.121.1.25");
			aDSyntax[34] = new ADSyntax(ADAttributeSyntax.Jpeg, "1.3.6.1.4.1.1466.115.121.1.28");
			aDSyntax[35] = new ADSyntax(ADAttributeSyntax.AttributeTypeDescription, "1.3.6.1.4.1.1466.115.121.1.3");
			aDSyntax[36] = new ADSyntax(ADAttributeSyntax.MailPreference, "1.3.6.1.4.1.1466.115.121.1.32");
			aDSyntax[37] = new ADSyntax(ADAttributeSyntax.ORAddress, "1.3.6.1.4.1.1466.115.121.1.33");
			aDSyntax[38] = new ADSyntax(ADAttributeSyntax.NameAndOptionalUid, "1.3.6.1.4.1.1466.115.121.1.34");
			aDSyntax[39] = new ADSyntax(ADAttributeSyntax.ObjectClassDescription, "1.3.6.1.4.1.1466.115.121.1.37");
			aDSyntax[40] = new ADSyntax(ADAttributeSyntax.OtherMailBox, "1.3.6.1.4.1.1466.115.121.1.39");
			aDSyntax[41] = new ADSyntax(ADAttributeSyntax.Audio, "1.3.6.1.4.1.1466.115.121.1.4");
			aDSyntax[42] = new ADSyntax(ADAttributeSyntax.PostalAddress, "1.3.6.1.4.1.1466.115.121.1.41");
			aDSyntax[43] = new ADSyntax(ADAttributeSyntax.TelephoneNumber, "1.3.6.1.4.1.1466.115.121.1.50");
			aDSyntax[44] = new ADSyntax(ADAttributeSyntax.TeletexTerminalIdentifier, "1.3.6.1.4.1.1466.115.121.1.51");
			aDSyntax[45] = new ADSyntax(ADAttributeSyntax.TelexNumber, "1.3.6.1.4.1.1466.115.121.1.52");
			aDSyntax[46] = new ADSyntax(ADAttributeSyntax.BitString, "1.3.6.1.4.1.1466.115.121.1.6");
			aDSyntax[47] = new ADSyntax(ADAttributeSyntax.AccessPointDN, "AccessPointDN");
			aDSyntax[48] = new ADSyntax(ADAttributeSyntax.AttributeTypeDescription, "AttributeTypeDescription");
			aDSyntax[49] = new ADSyntax(ADAttributeSyntax.Audio, "Audio");
			aDSyntax[50] = new ADSyntax(ADAttributeSyntax.OctetString, "Binary");
			aDSyntax[51] = new ADSyntax(ADAttributeSyntax.BitString, "BitString");
			aDSyntax[52] = new ADSyntax(ADAttributeSyntax.Bool, "Boolean");
			aDSyntax[53] = new ADSyntax(ADAttributeSyntax.CaseExactString, "CaseExactString");
			aDSyntax[54] = new ADSyntax(ADAttributeSyntax.CaseIgnoreString, "CaseIgnoreString");
			aDSyntax[55] = new ADSyntax(ADAttributeSyntax.Certificate, "Certificate");
			aDSyntax[56] = new ADSyntax(ADAttributeSyntax.CertificateList, "CertificateList");
			aDSyntax[57] = new ADSyntax(ADAttributeSyntax.CertificatePair, "CertificatePair");
			aDSyntax[58] = new ADSyntax(ADAttributeSyntax.CountryString, "Country");
			aDSyntax[59] = new ADSyntax(ADAttributeSyntax.DataQualitySyntax, "DataQualitySyntax");
			aDSyntax[60] = new ADSyntax(ADAttributeSyntax.DeliveryMethod, "DeliveryMethod");
			aDSyntax[61] = new ADSyntax(ADAttributeSyntax.DirectoryString, "DirectoryString");
			aDSyntax[62] = new ADSyntax(ADAttributeSyntax.DN, "DN");
			aDSyntax[63] = new ADSyntax(ADAttributeSyntax.DSAQualitySyntax, "DSAQualitySyntax");
			aDSyntax[64] = new ADSyntax(ADAttributeSyntax.EnhancedGuidE, "EnhancedGuide");
			aDSyntax[65] = new ADSyntax(ADAttributeSyntax.FacsimileTelephoneNumer, "FacsimileTelephoneNumber");
			aDSyntax[66] = new ADSyntax(ADAttributeSyntax.Fax, "Fax");
			aDSyntax[67] = new ADSyntax(ADAttributeSyntax.GeneralizedTime, "GeneralizedTime");
			aDSyntax[68] = new ADSyntax(ADAttributeSyntax.GuidE, "Guide");
			aDSyntax[69] = new ADSyntax(ADAttributeSyntax.IA5String, "IA5String");
			aDSyntax[70] = new ADSyntax(ADAttributeSyntax.Int, "INTEGER");
			aDSyntax[71] = new ADSyntax(ADAttributeSyntax.Int64, "INTEGER8");
			aDSyntax[72] = new ADSyntax(ADAttributeSyntax.Jpeg, "JPEG");
			aDSyntax[73] = new ADSyntax(ADAttributeSyntax.MailPreference, "MailPreference");
			aDSyntax[74] = new ADSyntax(ADAttributeSyntax.NameAndOptionalUid, "NameAndOptionalUID");
			aDSyntax[75] = new ADSyntax(ADAttributeSyntax.NumericString, "NumericString");
			aDSyntax[76] = new ADSyntax(ADAttributeSyntax.ObjectClassDescription, "ObjectClassDescription");
			aDSyntax[77] = new ADSyntax(ADAttributeSyntax.SecurityDescriptor, "ObjectSecurityDescriptor");
			aDSyntax[78] = new ADSyntax(ADAttributeSyntax.Oid, "OID");
			aDSyntax[79] = new ADSyntax(ADAttributeSyntax.ORAddress, "ORAddress");
			aDSyntax[80] = new ADSyntax(ADAttributeSyntax.ORName, "ORName");
			aDSyntax[81] = new ADSyntax(ADAttributeSyntax.OtherMailBox, "OtherMailbox");
			aDSyntax[82] = new ADSyntax(ADAttributeSyntax.Password, "Password");
			aDSyntax[83] = new ADSyntax(ADAttributeSyntax.PostalAddress, "PostalAddress");
			aDSyntax[84] = new ADSyntax(ADAttributeSyntax.PresentationAddress, "PresentationAddress");
			aDSyntax[85] = new ADSyntax(ADAttributeSyntax.PrintableString, "PrintableString");
			aDSyntax[86] = new ADSyntax(ADAttributeSyntax.TelephoneNumber, "TelephoneNumber");
			aDSyntax[87] = new ADSyntax(ADAttributeSyntax.TeletexTerminalIdentifier, "TeletexTerminalIdentifier");
			aDSyntax[88] = new ADSyntax(ADAttributeSyntax.TelexNumber, "TelexNumber");
			ADSyntax._syntaxConstants = aDSyntax;
			ADSyntax._syntaxMap = new Dictionary<string, ADAttributeSyntax>((int)ADSyntax._syntaxConstants.Length);
			ADSyntax[] aDSyntaxArray = ADSyntax._syntaxConstants;
			for (int i = 0; i < (int)aDSyntaxArray.Length; i++)
			{
				ADSyntax aDSyntax1 = aDSyntaxArray[i];
				ADSyntax._syntaxMap.Add(aDSyntax1._syntaxOid, aDSyntax1._attributeSyntax);
			}
		}

		private ADSyntax()
		{
		}

		private ADSyntax(ADAttributeSyntax attributeSyntax, string syntaxOID)
		{
			this._attributeSyntax = attributeSyntax;
			this._syntaxOid = syntaxOID;
		}

		public static ADAttributeSyntax OIDToSyntax(string OID)
		{
			ADAttributeSyntax aDAttributeSyntax = ADAttributeSyntax.CaseExactString;
			if (ADSyntax._syntaxMap.TryGetValue(OID, out aDAttributeSyntax))
			{
				return aDAttributeSyntax;
			}
			else
			{
				DebugLogger.LogError("adschema", string.Format("OID {0} not found in mapping table", OID));
				return ADAttributeSyntax.NotFound;
			}
		}
	}
}