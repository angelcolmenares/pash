using System;
using System.Runtime;
using System.Security.Authentication;

namespace System.DirectoryServices.Protocols
{
	public class SecurityPackageContextConnectionInformation
	{
		private SecurityProtocol securityProtocol;

		private CipherAlgorithmType identifier;

		private int strength;

		private HashAlgorithmType hashAlgorithm;

		private int hashStrength;

		private int keyExchangeAlgorithm;

		private int exchangeStrength;

		public CipherAlgorithmType AlgorithmIdentifier
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.identifier;
			}
		}

		public int CipherStrength
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.strength;
			}
		}

		public int ExchangeStrength
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.exchangeStrength;
			}
		}

		public HashAlgorithmType Hash
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.hashAlgorithm;
			}
		}

		public int HashStrength
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.hashStrength;
			}
		}

		public int KeyExchangeAlgorithm
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.keyExchangeAlgorithm;
			}
		}

		public SecurityProtocol Protocol
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.securityProtocol;
			}
		}

		internal SecurityPackageContextConnectionInformation()
		{
		}
	}
}