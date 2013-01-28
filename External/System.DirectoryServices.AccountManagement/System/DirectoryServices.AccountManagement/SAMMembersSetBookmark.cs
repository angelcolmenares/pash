using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;

namespace System.DirectoryServices.AccountManagement
{
	internal class SAMMembersSetBookmark : ResultSetBookmark
	{
		public List<string> groupsToVisit;

		public List<string> groupsVisited;

		public UnsafeNativeMethods.IADsGroup @group;

		public IEnumerator membersEnumerator;

		public DirectoryEntry current;

		public Principal currentFakePrincipal;

		public Principal currentForeign;

		public List<DirectoryEntry> foreignMembers;

		public List<GroupPrincipal> foreignGroups;

		public ResultSet foreignResultSet;

		public bool atBeginning;

		public SAMMembersSetBookmark()
		{
		}
	}
}