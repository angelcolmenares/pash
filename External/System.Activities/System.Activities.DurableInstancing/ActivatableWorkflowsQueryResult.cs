using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;

namespace System.Activities.DurableInstancing
{
	public sealed class ActivatableWorkflowsQueryResult : InstanceStoreQueryResult
	{
		private readonly static ReadOnlyDictionaryInternal<XName, object> emptyDictionary;

		public List<IDictionary<XName, object>> ActivationParameters
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			private set;
		}

		static ActivatableWorkflowsQueryResult()
		{
			ActivatableWorkflowsQueryResult.emptyDictionary = new ReadOnlyDictionaryInternal<XName, object>(new Dictionary<XName, object>(0));
		}

		public ActivatableWorkflowsQueryResult()
		{
			this.ActivationParameters = new List<IDictionary<XName, object>>(0);
		}

		public ActivatableWorkflowsQueryResult(IDictionary<XName, object> parameters)
		{
			IDictionary<XName, object> xNames;
			ActivatableWorkflowsQueryResult activatableWorkflowsQueryResult = this;
			List<IDictionary<XName, object>> dictionaries = new List<IDictionary<XName, object>>();
			List<IDictionary<XName, object>> dictionaries1 = dictionaries;
			if (parameters == null)
			{
				xNames = ActivatableWorkflowsQueryResult.emptyDictionary;
			}
			else
			{
				xNames = new ReadOnlyDictionaryInternal<XName, object>(new Dictionary<XName, object>(parameters));
			}
			dictionaries1.Add(xNames);
			activatableWorkflowsQueryResult.ActivationParameters = dictionaries;
		}

		public ActivatableWorkflowsQueryResult(IEnumerable<IDictionary<XName, object>> parameters)
		{
			if (parameters != null)
			{
				ActivatableWorkflowsQueryResult dictionaries = this;
				IEnumerable<IDictionary<XName, object>> dictionaries1 = parameters;
				dictionaries.ActivationParameters = new List<IDictionary<XName, object>>(dictionaries1.Select<IDictionary<XName, object>, ReadOnlyDictionaryInternal<XName, object>>((IDictionary<XName, object> dictionary) => {
					if (dictionary == null)
					{
						return ActivatableWorkflowsQueryResult.emptyDictionary;
					}
					else
					{
						return new ReadOnlyDictionaryInternal<XName, object>(new Dictionary<XName, object>(dictionary));
					}
				}
				));
				return;
			}
			else
			{
				this.ActivationParameters = new List<IDictionary<XName, object>>(0);
				return;
			}
		}
	}
}