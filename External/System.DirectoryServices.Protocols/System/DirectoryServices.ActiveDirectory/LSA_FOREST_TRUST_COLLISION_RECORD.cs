using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class LSA_FOREST_TRUST_COLLISION_RECORD
	{
		public int Index;

		public ForestTrustCollisionType Type;

		public int Flags;

		public LSA_UNICODE_STRING Name;

		public LSA_FOREST_TRUST_COLLISION_RECORD()
		{
		}
	}
}