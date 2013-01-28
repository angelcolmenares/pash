using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal class NodeList<TNodeType> : IEnumerable<TNodeType>, IEnumerable
	{
		private readonly TNodeType[] m_nodes;

		private object m_parent;

		public int Count
		{
			get
			{
				return (int)this.m_nodes.Length;
			}
		}

		public TNodeType this[int i]
		{
			get
			{
				return this.m_nodes[i];
			}
		}

		public object Parent
		{
			get
			{
				return this.m_parent;
			}
		}

		internal NodeList(TNodeType[] nodes)
		{
			this.m_nodes = nodes;
		}

		public IEnumerator<TNodeType> GetEnumerator()
		{
			return this.m_nodes.AsEnumerable<TNodeType>().GetEnumerator();
		}

		internal NodeList<TSubtype> GetFilteredList<TSubtype>()
		where TSubtype : TNodeType
		{
			NodeList<TSubtype> tSubtypes = new NodeList<TSubtype>(this.Where<TNodeType>(new Func<TNodeType, bool>( (x) => { return true; }/* TODO: NodeList<TNodeType>.<GetFilteredList>b__0<TSubtype> */)).Cast<TSubtype>().ToArray<TSubtype>());
			tSubtypes.m_parent = this.m_parent;
			return tSubtypes;
		}

		internal void SetParent(object parent)
		{
			this.m_parent = parent;
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.m_nodes.GetEnumerator();
		}
	}
}