using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cim;
using Microsoft.PowerShell.Cmdletization;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class ClientSideQuery : QueryBuilder
	{
		private int _numberOfResultsFromMi;

		private int _numberOfMatchingResults;

		private readonly List<ClientSideQuery.CimInstanceFilterBase> _filters;

		private readonly object _myLock;

		public ClientSideQuery()
		{
			this._filters = new List<ClientSideQuery.CimInstanceFilterBase>();
			this._myLock = new object();
		}

		public override void ExcludeByProperty(string propertyName, IEnumerable excludedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this._filters.Add(new ClientSideQuery.CimInstanceExcludeFilter(propertyName, excludedPropertyValues, wildcardsEnabled, behaviorOnNoMatch));
		}

		public override void FilterByAssociatedInstance(object associatedInstance, string associationName, string sourceRole, string resultRole, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this._filters.Add(new ClientSideQuery.CimInstanceAssociationFilter(behaviorOnNoMatch));
		}

		public override void FilterByMaxPropertyValue(string propertyName, object maxPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this._filters.Add(new ClientSideQuery.CimInstanceMaxFilter(propertyName, maxPropertyValue, behaviorOnNoMatch));
		}

		public override void FilterByMinPropertyValue(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this._filters.Add(new ClientSideQuery.CimInstanceMinFilter(propertyName, minPropertyValue, behaviorOnNoMatch));
		}

		public override void FilterByProperty(string propertyName, IEnumerable allowedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
		{
			this._filters.Add(new ClientSideQuery.CimInstanceRegularFilter(propertyName, allowedPropertyValues, wildcardsEnabled, behaviorOnNoMatch));
		}

		internal IEnumerable<ClientSideQuery.NotFoundError> GenerateNotFoundErrors()
		{
			if (this._filters.Count <= 1)
			{
				ClientSideQuery.CimInstanceFilterBase cimInstanceFilterBase = this._filters.SingleOrDefault<ClientSideQuery.CimInstanceFilterBase>();
				if (cimInstanceFilterBase == null)
				{
					return Enumerable.Empty<ClientSideQuery.NotFoundError>();
				}
				else
				{
					return cimInstanceFilterBase.GetNotFoundErrors_IfThisIsTheOnlyFilter();
				}
			}
			else
			{
				if (this._numberOfMatchingResults <= 0)
				{
					List<ClientSideQuery.CimInstanceFilterBase> cimInstanceFilterBases = this._filters;
					if (!cimInstanceFilterBases.All<ClientSideQuery.CimInstanceFilterBase>((ClientSideQuery.CimInstanceFilterBase f) => !f.ShouldReportErrorOnNoMatches_IfMultipleFilters()))
					{
						ClientSideQuery.NotFoundError[] notFoundError = new ClientSideQuery.NotFoundError[1];
						notFoundError[0] = new ClientSideQuery.NotFoundError();
						return notFoundError;
					}
					else
					{
						return Enumerable.Empty<ClientSideQuery.NotFoundError>();
					}
				}
				else
				{
					return Enumerable.Empty<ClientSideQuery.NotFoundError>();
				}
			}
		}

		internal bool IsResultMatchingClientSideQuery(CimInstance result)
		{
			bool flag;
			Func<ClientSideQuery.CimInstanceFilterBase, bool> func = null;
			lock (this._myLock)
			{
				ClientSideQuery clientSideQuery = this;
				clientSideQuery._numberOfResultsFromMi = clientSideQuery._numberOfResultsFromMi + 1;
				List<ClientSideQuery.CimInstanceFilterBase> cimInstanceFilterBases = this._filters;
				if (func == null)
				{
					func = (ClientSideQuery.CimInstanceFilterBase f) => f.IsMatch(result);
				}
				if (!cimInstanceFilterBases.All<ClientSideQuery.CimInstanceFilterBase>(func))
				{
					flag = false;
				}
				else
				{
					ClientSideQuery clientSideQuery1 = this;
					clientSideQuery1._numberOfMatchingResults = clientSideQuery1._numberOfMatchingResults + 1;
					flag = true;
				}
			}
			return flag;
		}

		private class CimInstanceAssociationFilter : ClientSideQuery.CimInstanceFilterBase
		{
			public CimInstanceAssociationFilter(BehaviorOnNoMatch behaviorOnNoMatch)
			{
				if (behaviorOnNoMatch != BehaviorOnNoMatch.Default)
				{
					base.BehaviorOnNoMatch = behaviorOnNoMatch;
					return;
				}
				else
				{
					base.BehaviorOnNoMatch = BehaviorOnNoMatch.ReportErrors;
					return;
				}
			}

			protected override bool IsMatchCore(CimInstance cimInstance)
			{
				return true;
			}
		}

		private class CimInstanceExcludeFilter : ClientSideQuery.CimInstancePropertyBasedFilter
		{
			public CimInstanceExcludeFilter(string propertyName, IEnumerable excludedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
			{
				if (behaviorOnNoMatch != BehaviorOnNoMatch.Default)
				{
					base.BehaviorOnNoMatch = behaviorOnNoMatch;
				}
				else
				{
					base.BehaviorOnNoMatch = BehaviorOnNoMatch.SilentlyContinue;
				}
				foreach (object excludedPropertyValue in excludedPropertyValues)
				{
					base.AddPropertyValueFilter(new ClientSideQuery.PropertyValueExcludeFilter(propertyName, excludedPropertyValue, wildcardsEnabled, behaviorOnNoMatch));
				}
			}
		}

		private abstract class CimInstanceFilterBase
		{
			protected BehaviorOnNoMatch BehaviorOnNoMatch
			{
				get;
				set;
			}

			private bool HadMatches
			{
				get;
				set;
			}

			protected CimInstanceFilterBase()
			{
			}

			public virtual IEnumerable<ClientSideQuery.NotFoundError> GetNotFoundErrors_IfThisIsTheOnlyFilter()
			{
				BehaviorOnNoMatch behaviorOnNoMatch = this.BehaviorOnNoMatch;
				switch (behaviorOnNoMatch)
				{
					case BehaviorOnNoMatch.Default:
					{
						return Enumerable.Empty<ClientSideQuery.NotFoundError>();
					}
					case BehaviorOnNoMatch.ReportErrors:
					{
						if (!this.HadMatches)
						{
							ClientSideQuery.NotFoundError[] notFoundError = new ClientSideQuery.NotFoundError[1];
							notFoundError[0] = new ClientSideQuery.NotFoundError();
							return notFoundError;
						}
						else
						{
							return Enumerable.Empty<ClientSideQuery.NotFoundError>();
						}
					}
					case BehaviorOnNoMatch.SilentlyContinue:
					{
						return Enumerable.Empty<ClientSideQuery.NotFoundError>();
					}
					default:
					{
						return Enumerable.Empty<ClientSideQuery.NotFoundError>();
					}
				}
			}

			public bool IsMatch(CimInstance cimInstance)
			{
				bool flag;
				bool flag1 = this.IsMatchCore(cimInstance);
				ClientSideQuery.CimInstanceFilterBase cimInstanceFilterBase = this;
				if (this.HadMatches)
				{
					flag = true;
				}
				else
				{
					flag = flag1;
				}
				cimInstanceFilterBase.HadMatches = flag;
				return flag1;
			}

			protected abstract bool IsMatchCore(CimInstance cimInstance);

			public virtual bool ShouldReportErrorOnNoMatches_IfMultipleFilters()
			{
				BehaviorOnNoMatch behaviorOnNoMatch = this.BehaviorOnNoMatch;
				switch (behaviorOnNoMatch)
				{
					case BehaviorOnNoMatch.Default:
					{
						return false;
					}
					case BehaviorOnNoMatch.ReportErrors:
					{
						return true;
					}
					case BehaviorOnNoMatch.SilentlyContinue:
					{
						return false;
					}
					default:
					{
						return false;
					}
				}
			}
		}

		private class CimInstanceMaxFilter : ClientSideQuery.CimInstancePropertyBasedFilter
		{
			public CimInstanceMaxFilter(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
			{
				if (behaviorOnNoMatch != BehaviorOnNoMatch.Default)
				{
					base.BehaviorOnNoMatch = behaviorOnNoMatch;
				}
				else
				{
					base.BehaviorOnNoMatch = BehaviorOnNoMatch.SilentlyContinue;
				}
				base.AddPropertyValueFilter(new ClientSideQuery.PropertyValueMaxFilter(propertyName, minPropertyValue, behaviorOnNoMatch));
			}
		}

		private class CimInstanceMinFilter : ClientSideQuery.CimInstancePropertyBasedFilter
		{
			public CimInstanceMinFilter(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
			{
				if (behaviorOnNoMatch != BehaviorOnNoMatch.Default)
				{
					base.BehaviorOnNoMatch = behaviorOnNoMatch;
				}
				else
				{
					base.BehaviorOnNoMatch = BehaviorOnNoMatch.SilentlyContinue;
				}
				base.AddPropertyValueFilter(new ClientSideQuery.PropertyValueMinFilter(propertyName, minPropertyValue, behaviorOnNoMatch));
			}
		}

		private abstract class CimInstancePropertyBasedFilter : ClientSideQuery.CimInstanceFilterBase
		{
			private readonly List<ClientSideQuery.PropertyValueFilter> _propertyValueFilters;

			protected IEnumerable<ClientSideQuery.PropertyValueFilter> PropertyValueFilters
			{
				get
				{
					return this._propertyValueFilters;
				}
			}

			protected CimInstancePropertyBasedFilter()
			{
				this._propertyValueFilters = new List<ClientSideQuery.PropertyValueFilter>();
			}

			protected void AddPropertyValueFilter(ClientSideQuery.PropertyValueFilter propertyValueFilter)
			{
				this._propertyValueFilters.Add(propertyValueFilter);
			}

			protected override bool IsMatchCore(CimInstance cimInstance)
			{
				bool flag = false;
				IEnumerator<ClientSideQuery.PropertyValueFilter> enumerator = this.PropertyValueFilters.GetEnumerator();
				using (enumerator)
				{
					do
					{
					Label0:
						if (!enumerator.MoveNext())
						{
							break;
						}
						ClientSideQuery.PropertyValueFilter current = enumerator.Current;
						if (current.IsMatch(cimInstance))
						{
							flag = true;
						}
						else
						{
							goto Label0;
						}
					}
					while (base.BehaviorOnNoMatch != BehaviorOnNoMatch.SilentlyContinue);
				}
				return flag;
			}
		}

		private class CimInstanceRegularFilter : ClientSideQuery.CimInstancePropertyBasedFilter
		{
			public CimInstanceRegularFilter(string propertyName, IEnumerable allowedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
			{
				HashSet<BehaviorOnNoMatch> behaviorOnNoMatches = new HashSet<BehaviorOnNoMatch>();
				foreach (object allowedPropertyValue in allowedPropertyValues)
				{
					ClientSideQuery.PropertyValueFilter propertyValueRegularFilter = new ClientSideQuery.PropertyValueRegularFilter(propertyName, allowedPropertyValue, wildcardsEnabled, behaviorOnNoMatch);
					base.AddPropertyValueFilter(propertyValueRegularFilter);
					behaviorOnNoMatches.Add(propertyValueRegularFilter.BehaviorOnNoMatch);
				}
				if (behaviorOnNoMatches.Count != 1)
				{
					base.BehaviorOnNoMatch = behaviorOnNoMatch;
					return;
				}
				else
				{
					base.BehaviorOnNoMatch = behaviorOnNoMatches.Single<BehaviorOnNoMatch>();
					return;
				}
			}

			public override IEnumerable<ClientSideQuery.NotFoundError> GetNotFoundErrors_IfThisIsTheOnlyFilter()
			{
				foreach (ClientSideQuery.PropertyValueFilter propertyValueFilter in base.PropertyValueFilters)
				{
					if (propertyValueFilter.BehaviorOnNoMatch != BehaviorOnNoMatch.ReportErrors || propertyValueFilter.HadMatch)
					{
						continue;
					}
					ClientSideQuery.PropertyValueRegularFilter propertyValueRegularFilter = (ClientSideQuery.PropertyValueRegularFilter)propertyValueFilter;
					yield return propertyValueRegularFilter.GetGranularNotFoundError();
				}
			}

			public override bool ShouldReportErrorOnNoMatches_IfMultipleFilters()
			{
				IEnumerable<ClientSideQuery.PropertyValueFilter> propertyValueFilters;
				BehaviorOnNoMatch behaviorOnNoMatch = base.BehaviorOnNoMatch;
				switch (behaviorOnNoMatch)
				{
					case BehaviorOnNoMatch.Default:
					{
						IEnumerable<ClientSideQuery.PropertyValueFilter> propertyValueFilters1 = base.PropertyValueFilters;
						propertyValueFilters = propertyValueFilters1.Where<ClientSideQuery.PropertyValueFilter>((ClientSideQuery.PropertyValueFilter f) => !f.HadMatch);
						break;
					}
					case BehaviorOnNoMatch.ReportErrors:
					{
						return true;
					}
					case BehaviorOnNoMatch.SilentlyContinue:
					{
						return false;
					}
					default:
					{
                        IEnumerable<ClientSideQuery.PropertyValueFilter> propertyValueFilters1 = base.PropertyValueFilters;
                        propertyValueFilters = propertyValueFilters1.Where<ClientSideQuery.PropertyValueFilter>((ClientSideQuery.PropertyValueFilter f) => !f.HadMatch);
                        break;
					}
				}
                return propertyValueFilters.Where<ClientSideQuery.PropertyValueFilter>((ClientSideQuery.PropertyValueFilter f) => f.BehaviorOnNoMatch == BehaviorOnNoMatch.ReportErrors).Any<ClientSideQuery.PropertyValueFilter>();
			}
		}

		internal class NotFoundError
		{
			public Func<string, string, string> ErrorMessageGenerator
			{
				get;
				private set;
			}

			public string PropertyName
			{
				get;
				private set;
			}

			public object PropertyValue
			{
				get;
				private set;
			}

			public NotFoundError()
			{
				this.ErrorMessageGenerator = new Func<string, string, string>(ClientSideQuery.NotFoundError.GetErrorMessageForNotFound);
			}

			public NotFoundError(string propertyName, object propertyValue, bool wildcardsEnabled)
			{
				Func<string, string, string> func = null;
				Func<string, string, string> func1 = null;
				Func<string, string, string> func2 = null;
				this.PropertyName = propertyName;
				this.PropertyValue = propertyValue;
				if (!wildcardsEnabled)
				{
					ClientSideQuery.NotFoundError notFoundError = this;
					if (func2 == null)
					{
						func2 = (string queryDescription, string className) => ClientSideQuery.NotFoundError.GetErrorMessageForNotFound_ForEquality(this.PropertyName, this.PropertyValue, className);
					}
					notFoundError.ErrorMessageGenerator = func2;
					return;
				}
				else
				{
					string str = propertyValue as string;
					if (str == null || !WildcardPattern.ContainsWildcardCharacters(str))
					{
						ClientSideQuery.NotFoundError notFoundError1 = this;
						if (func1 == null)
						{
							func1 = (string queryDescription, string className) => ClientSideQuery.NotFoundError.GetErrorMessageForNotFound_ForEquality(this.PropertyName, this.PropertyValue, className);
						}
						notFoundError1.ErrorMessageGenerator = func1;
						return;
					}
					else
					{
						ClientSideQuery.NotFoundError notFoundError2 = this;
						if (func == null)
						{
							func = (string queryDescription, string className) => ClientSideQuery.NotFoundError.GetErrorMessageForNotFound_ForWildcard(this.PropertyName, this.PropertyValue, className);
						}
						notFoundError2.ErrorMessageGenerator = func;
						return;
					}
				}
			}

			private static string GetErrorMessageForNotFound(string queryDescription, string className)
			{
				object[] objArray = new object[2];
				objArray[0] = queryDescription;
				objArray[1] = className;
				string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_NotFound_ComplexCase, objArray);
				return str;
			}

			private static string GetErrorMessageForNotFound_ForEquality(string propertyName, object propertyValue, string className)
			{
				object[] objArray = new object[3];
				objArray[0] = propertyName;
				objArray[1] = propertyValue;
				objArray[2] = className;
				string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_NotFound_SimpleGranularCase_Equality, objArray);
				return str;
			}

			private static string GetErrorMessageForNotFound_ForWildcard(string propertyName, object propertyValue, string className)
			{
				object[] objArray = new object[3];
				objArray[0] = propertyName;
				objArray[1] = propertyValue;
				objArray[2] = className;
				string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_NotFound_SimpleGranularCase_Wildcard, objArray);
				return str;
			}
		}

		internal class PropertyValueExcludeFilter : ClientSideQuery.PropertyValueRegularFilter
		{
			public PropertyValueExcludeFilter(string propertyName, object expectedPropertyValue, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch) : base(propertyName, expectedPropertyValue, wildcardsEnabled, behaviorOnNoMatch)
			{
			}

			protected override BehaviorOnNoMatch GetDefaultBehaviorWhenNoMatchesFound(object cimTypedExpectedPropertyValue)
			{
				return BehaviorOnNoMatch.SilentlyContinue;
			}

			protected override bool IsMatchingValue(object actualPropertyValue)
			{
				return !base.IsMatchingValue(actualPropertyValue);
			}
		}

		internal abstract class PropertyValueFilter
		{
			private BehaviorOnNoMatch _behaviorOnNoMatch;

			private readonly string propertyName;

			private readonly object _cimTypedExpectedPropertyValue;

			private readonly object _originalExpectedPropertyValue;

			private bool hadMatch;

			public BehaviorOnNoMatch BehaviorOnNoMatch
			{
				get
				{
					if (this._behaviorOnNoMatch == BehaviorOnNoMatch.Default)
					{
						this._behaviorOnNoMatch = this.GetDefaultBehaviorWhenNoMatchesFound(this.CimTypedExpectedPropertyValue);
					}
					return this._behaviorOnNoMatch;
				}
			}

			public object CimTypedExpectedPropertyValue
			{
				get
				{
					return this._cimTypedExpectedPropertyValue;
				}
			}

			public bool HadMatch
			{
				get
				{
					return this.hadMatch;
				}
			}

			public object OriginalExpectedPropertyValue
			{
				get
				{
					return this._originalExpectedPropertyValue;
				}
			}

			public string PropertyName
			{
				get
				{
					return this.propertyName;
				}
			}

			protected PropertyValueFilter(string propertyName, object expectedPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
			{
				this.propertyName = propertyName;
				this._behaviorOnNoMatch = behaviorOnNoMatch;
				this._originalExpectedPropertyValue = expectedPropertyValue;
				this._cimTypedExpectedPropertyValue = CimValueConverter.ConvertFromDotNetToCim(expectedPropertyValue);
			}

			private object ConvertActualValueToExpectedType(object actualPropertyValue, object expectedPropertyValue)
			{
				if (actualPropertyValue as string != null && expectedPropertyValue as string == null)
				{
					actualPropertyValue = LanguagePrimitives.ConvertTo(actualPropertyValue, expectedPropertyValue.GetType(), CultureInfo.InvariantCulture);
				}
				if (ClientSideQuery.PropertyValueFilter.IsSameType(actualPropertyValue, expectedPropertyValue))
				{
					return actualPropertyValue;
				}
				else
				{
					object[] fullName = new object[3];
					fullName[0] = this.propertyName;
					fullName[1] = actualPropertyValue.GetType().FullName;
					fullName[2] = expectedPropertyValue.GetType().FullName;
					string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_MismatchedTypeOfPropertyReturnedByQuery, fullName);
					throw CimJobException.CreateWithoutJobContext(str, "CimJob_PropertyTypeUnexpectedByClientSideQuery", ErrorCategory.InvalidType, null);
				}
			}

			protected abstract BehaviorOnNoMatch GetDefaultBehaviorWhenNoMatchesFound(object cimTypedExpectedPropertyValue);

			public bool IsMatch(CimInstance o)
			{
				bool flag;
				bool flag1;
				if (o != null)
				{
					CimProperty item = o.CimInstanceProperties[this.propertyName];
					if (item != null)
					{
						object value = item.Value;
						if (this._cimTypedExpectedPropertyValue != null)
						{
							value = this.ConvertActualValueToExpectedType(value, this._cimTypedExpectedPropertyValue);
							bool flag2 = this.IsMatchingValue(value);
							ClientSideQuery.PropertyValueFilter propertyValueFilter = this;
							if (this.hadMatch)
							{
								flag = true;
							}
							else
							{
								flag = flag2;
							}
							propertyValueFilter.hadMatch = flag;
							return flag2;
						}
						else
						{
							ClientSideQuery.PropertyValueFilter propertyValueFilter1 = this;
							if (this.hadMatch)
							{
								flag1 = true;
							}
							else
							{
								flag1 = value == null;
							}
							propertyValueFilter1.hadMatch = flag1;
							return value == null;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			protected abstract bool IsMatchingValue(object actualPropertyValue);

			private static bool IsSameType(object actualPropertyValue, object expectedPropertyValue)
			{
				if (actualPropertyValue != null)
				{
					if (expectedPropertyValue != null)
					{
						if (actualPropertyValue.GetType().Equals(typeof(TimeSpan)) || actualPropertyValue.GetType().Equals(typeof(DateTime)))
						{
							if (expectedPropertyValue.GetType().Equals(typeof(TimeSpan)))
							{
								return true;
							}
							else
							{
								return expectedPropertyValue.GetType().Equals(typeof(DateTime));
							}
						}
						else
						{
							return actualPropertyValue.GetType().Equals(expectedPropertyValue.GetType());
						}
					}
					else
					{
						return true;
					}
				}
				else
				{
					return true;
				}
			}
		}

		internal class PropertyValueMaxFilter : ClientSideQuery.PropertyValueFilter
		{
			public PropertyValueMaxFilter(string propertyName, object expectedPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch) : base(propertyName, expectedPropertyValue, behaviorOnNoMatch)
			{
			}

			private static bool ActualValueLessThanOrEqualToExpectedValue(string propertyName, object actualPropertyValue, object expectedPropertyValue)
			{
				bool flag;
				try
				{
					IComparable comparable = actualPropertyValue as IComparable;
					if (comparable != null)
					{
						flag = comparable.CompareTo(expectedPropertyValue) <= 0;
					}
					else
					{
						flag = false;
					}
				}
				catch (ArgumentException argumentException)
				{
					flag = false;
				}
				return flag;
			}

			protected override BehaviorOnNoMatch GetDefaultBehaviorWhenNoMatchesFound(object cimTypedExpectedPropertyValue)
			{
				return BehaviorOnNoMatch.SilentlyContinue;
			}

			protected override bool IsMatchingValue(object actualPropertyValue)
			{
				return ClientSideQuery.PropertyValueMaxFilter.ActualValueLessThanOrEqualToExpectedValue(base.PropertyName, actualPropertyValue, base.CimTypedExpectedPropertyValue);
			}
		}

		internal class PropertyValueMinFilter : ClientSideQuery.PropertyValueFilter
		{
			public PropertyValueMinFilter(string propertyName, object expectedPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch) : base(propertyName, expectedPropertyValue, behaviorOnNoMatch)
			{
			}

			private static bool ActualValueGreaterThanOrEqualToExpectedValue(string propertyName, object actualPropertyValue, object expectedPropertyValue)
			{
				bool flag;
				try
				{
					IComparable comparable = expectedPropertyValue as IComparable;
					if (comparable != null)
					{
						flag = comparable.CompareTo(actualPropertyValue) <= 0;
					}
					else
					{
						flag = false;
					}
				}
				catch (ArgumentException argumentException)
				{
					flag = false;
				}
				return flag;
			}

			protected override BehaviorOnNoMatch GetDefaultBehaviorWhenNoMatchesFound(object cimTypedExpectedPropertyValue)
			{
				return BehaviorOnNoMatch.SilentlyContinue;
			}

			protected override bool IsMatchingValue(object actualPropertyValue)
			{
				return ClientSideQuery.PropertyValueMinFilter.ActualValueGreaterThanOrEqualToExpectedValue(base.PropertyName, actualPropertyValue, base.CimTypedExpectedPropertyValue);
			}
		}

		internal class PropertyValueRegularFilter : ClientSideQuery.PropertyValueFilter
		{
			private readonly bool _wildcardsEnabled;

			public PropertyValueRegularFilter(string propertyName, object expectedPropertyValue, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch) : base(propertyName, expectedPropertyValue, behaviorOnNoMatch)
			{
				this._wildcardsEnabled = wildcardsEnabled;
			}

			protected override BehaviorOnNoMatch GetDefaultBehaviorWhenNoMatchesFound(object cimTypedExpectedPropertyValue)
			{
				if (this._wildcardsEnabled)
				{
					string str = cimTypedExpectedPropertyValue as string;
					if (str == null || !WildcardPattern.ContainsWildcardCharacters(str))
					{
						return BehaviorOnNoMatch.ReportErrors;
					}
					else
					{
						return BehaviorOnNoMatch.SilentlyContinue;
					}
				}
				else
				{
					return BehaviorOnNoMatch.ReportErrors;
				}
			}

			internal ClientSideQuery.NotFoundError GetGranularNotFoundError()
			{
				return new ClientSideQuery.NotFoundError(base.PropertyName, base.OriginalExpectedPropertyValue, this._wildcardsEnabled);
			}

			protected override bool IsMatchingValue(object actualPropertyValue)
			{
				if (!this._wildcardsEnabled)
				{
					return ClientSideQuery.PropertyValueRegularFilter.NonWildcardEqual(base.PropertyName, actualPropertyValue, base.CimTypedExpectedPropertyValue);
				}
				else
				{
					return ClientSideQuery.PropertyValueRegularFilter.WildcardEqual(base.PropertyName, actualPropertyValue, base.CimTypedExpectedPropertyValue);
				}
			}

			private static bool NonWildcardEqual(string propertyName, object actualPropertyValue, object expectedPropertyValue)
			{
				if (!(expectedPropertyValue is char))
				{
					expectedPropertyValue = expectedPropertyValue.ToString();
					actualPropertyValue = actualPropertyValue.ToString();
				}
				string str = expectedPropertyValue as string;
				if (str == null)
				{
					return actualPropertyValue.Equals(expectedPropertyValue);
				}
				else
				{
					string str1 = (string)actualPropertyValue;
					return str1.Equals(str, StringComparison.OrdinalIgnoreCase);
				}
			}

			private static bool WildcardEqual(string propertyName, object actualPropertyValue, object expectedPropertyValue)
			{
				string str = null;
				string str1 = null;
				if (LanguagePrimitives.TryConvertTo<string>(actualPropertyValue, out str))
				{
					if (LanguagePrimitives.TryConvertTo<string>(expectedPropertyValue, out str1))
					{
						return (new WildcardPattern(str1, WildcardOptions.IgnoreCase)).IsMatch(str);
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
		}
	}
}