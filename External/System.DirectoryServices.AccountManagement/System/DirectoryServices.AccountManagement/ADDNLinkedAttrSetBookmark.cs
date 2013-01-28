using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;

namespace System.DirectoryServices.AccountManagement
{
	internal class ADDNLinkedAttrSetBookmark : ResultSetBookmark
	{
		public Dictionary<string, bool> usersVisited;

		public List<string> groupsToVisit;

		public List<string> groupsVisited;

		public IEnumerable members;

		public IEnumerator membersEnum;

		public Queue<IEnumerable> membersQueue;

		public ADStoreCtx storeCtx;

		public object current;

		public bool returnedPrimaryGroup;

		public List<DirectoryEntry> foreignMembersCurrentGroup;

		public List<DirectoryEntry> fakePrincipalMembers;

		public SidList foreignMembersToReturn;

		public Principal currentForeignPrincipal;

		public DirectoryEntry currentForeignDE;

		public List<GroupPrincipal> foreignGroups;

		public SearchResultCollection queryMembersResults;

		public IEnumerator queryMembersResultEnumerator;

		public SearchResultCollection memberSearchResults;

		public IEnumerator memberSearchResultsEnumerator;

		public bool atBeginning;

		public ExpansionMode expansionMode;

		public Queue<DirectorySearcher> memberSearcherQueue;

		public ADDNLinkedAttrSetBookmark()
		{
		}
	}
}