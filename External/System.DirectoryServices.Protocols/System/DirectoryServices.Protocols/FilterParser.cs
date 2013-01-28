using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace System.DirectoryServices.Protocols
{
	internal class FilterParser
	{
		private const uint mFilterTimeOutInSeconds = 3;

		private const string mAttrRE = "(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*";

		private const string mValueRE = "(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?";

		private const string mExtenAttrRE = "(?<extenattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*";

		private const string mExtenValueRE = "(?<extenvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*";

		private const string mDNAttrRE = "(?<dnattr>\\:dn){0,1}\\s*";

		private const string mMatchRuleOptionalRE = "(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+))){0,1}\\s*";

		private const string mMatchRuleRE = "(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+)))\\s*";

		private const string mExtenRE = "(?<extensible>(((?<extenattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+))){0,1}\\s*)|((?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+)))\\s*))\\:\\=\\s*(?<extenvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*";

		private const string mSubstrAttrRE = "(?<substrattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*";

		private const string mInitialRE = "\\s*(?<initialvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*";

		private const string mFinalRE = "\\s*(?<finalvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*";

		private const string mAnyRE = "(\\*\\s*((?<anyvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\*\\s*)*)";

		private const string mSubstrRE = "(?<substr>(?<substrattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*\\=\\s*\\s*(?<initialvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*(\\*\\s*((?<anyvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\*\\s*)*)\\s*(?<finalvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*)\\s*";

		private const string mSimpleValueRE = "(?<simplevalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*";

		private const string mSimpleAttrRE = "(?<simpleattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*";

		private const string mFiltertypeRE = "(?<filtertype>\\=|\\~\\=|\\>\\=|\\<\\=)\\s*";

		private const string mSimpleRE = "(?<simple>(?<simpleattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<filtertype>\\=|\\~\\=|\\>\\=|\\<\\=)\\s*(?<simplevalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*";

		private const string mPresentRE = "(?<present>(?<presentattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\=\\*)\\s*";

		private const string mItemRE = "(?<item>(?<simple>(?<simpleattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<filtertype>\\=|\\~\\=|\\>\\=|\\<\\=)\\s*(?<simplevalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*|(?<present>(?<presentattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\=\\*)\\s*|(?<substr>(?<substrattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*\\=\\s*\\s*(?<initialvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*(\\*\\s*((?<anyvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\*\\s*)*)\\s*(?<finalvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*)\\s*|(?<extensible>(((?<extenattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+))){0,1}\\s*)|((?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+)))\\s*))\\:\\=\\s*(?<extenvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*)\\s*";

		private const string mFiltercompRE = "(?<filtercomp>\\!|\\&|\\|)\\s*";

		private const string mFilterlistRE = "(?<filterlist>.+)\\s*";

		private const string mFilterRE = "^\\s*\\(\\s*(((?<filtercomp>\\!|\\&|\\|)\\s*(?<filterlist>.+)\\s*)|((?<item>(?<simple>(?<simpleattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<filtertype>\\=|\\~\\=|\\>\\=|\\<\\=)\\s*(?<simplevalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*|(?<present>(?<presentattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\=\\*)\\s*|(?<substr>(?<substrattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*\\=\\s*\\s*(?<initialvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*(\\*\\s*((?<anyvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\*\\s*)*)\\s*(?<finalvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*)\\s*|(?<extensible>(((?<extenattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+))){0,1}\\s*)|((?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+)))\\s*))\\:\\=\\s*(?<extenvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*)\\s*))\\)\\s*$";

		private static Regex mFilter;

		static FilterParser()
		{
			FilterParser.mFilter = new Regex("^\\s*\\(\\s*(((?<filtercomp>\\!|\\&|\\|)\\s*(?<filterlist>.+)\\s*)|((?<item>(?<simple>(?<simpleattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<filtertype>\\=|\\~\\=|\\>\\=|\\<\\=)\\s*(?<simplevalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*|(?<present>(?<presentattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\=\\*)\\s*|(?<substr>(?<substrattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*\\=\\s*\\s*(?<initialvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*(\\*\\s*((?<anyvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\*\\s*)*)\\s*(?<finalvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?){0,1}\\s*)\\s*|(?<extensible>(((?<extenattr>(([0-2](\\.[0-9]+)+)|([a-zA-Z]+([a-zA-Z0-9]|[-])*))(;([a-zA-Z0-9]|[-])+)*)\\s*(?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+))){0,1}\\s*)|((?<dnattr>\\:dn){0,1}\\s*(\\:(?<matchrule>([a-zA-Z][a-zA-Z0-9]*)|([0-9]+(\\.[0-9]+)+)))\\s*))\\:\\=\\s*(?<extenvalue>(([^\\*\\(\\)\\\\])|(\\\\[a-fA-F0-9][a-fA-F0-9]))+?)\\s*)\\s*)\\s*))\\)\\s*$", RegexOptions.ExplicitCapture); //, TimeSpan.FromSeconds(3));
		}

		public FilterParser()
		{
		}

		public static ADFilter ParseFilterString(string filter)
		{
			ADFilter aDFilter = null;
			ADFilter aDFilter1 = null;
			try
			{
				Match match = FilterParser.mFilter.Match(filter);
				if (match.Success)
				{
					ADFilter arrayLists = new ADFilter();
					if (match.Groups["item"].ToString().Length == 0)
					{
						ArrayList arrayLists1 = new ArrayList();
						string str = match.Groups["filterlist"].ToString().Trim();
						while (str.Length > 0)
						{
							if (str[0] == '(')
							{
								int num = 1;
								int num1 = 1;
								bool flag = false;
								while (num < str.Length && !flag)
								{
									if (str[num] == '(')
									{
										num1++;
									}
									if (str[num] == ')')
									{
										if (num1 >= 1)
										{
											if (num1 != 1)
											{
												num1--;
											}
											else
											{
												flag = true;
											}
										}
										else
										{
											aDFilter1 = null;
											return aDFilter1;
										}
									}
									num++;
								}
								if (flag)
								{
									arrayLists1.Add(str.Substring(0, num));
									str = str.Substring(num).TrimStart(new char[0]);
								}
								else
								{
									aDFilter1 = null;
									return aDFilter1;
								}
							}
							else
							{
								aDFilter1 = null;
								return aDFilter1;
							}
						}
						string str1 = match.Groups["filtercomp"].ToString();
						string str2 = str1;
						if (str1 != null)
						{
							if (str2 == "|")
							{
								arrayLists.Type = ADFilter.FilterType.Or;
								arrayLists.Filter.Or = new ArrayList();
								foreach (string arrayList in arrayLists1)
								{
									aDFilter = FilterParser.ParseFilterString(arrayList);
									if (aDFilter != null)
									{
										arrayLists.Filter.Or.Add(aDFilter);
									}
									else
									{
										aDFilter1 = null;
										return aDFilter1;
									}
								}
								if (arrayLists.Filter.Or.Count < 1)
								{
									aDFilter1 = null;
									return aDFilter1;
								}
								else
								{
									goto Label1;
								}
							}
							else
							{
								if (str2 == "&")
								{
									arrayLists.Type = ADFilter.FilterType.And;
									arrayLists.Filter.And = new ArrayList();
									foreach (string arrayList1 in arrayLists1)
									{
										aDFilter = FilterParser.ParseFilterString(arrayList1);
										if (aDFilter != null)
										{
											arrayLists.Filter.And.Add(aDFilter);
										}
										else
										{
											aDFilter1 = null;
											return aDFilter1;
										}
									}
									if (arrayLists.Filter.And.Count < 1)
									{
										aDFilter1 = null;
										return aDFilter1;
									}
									else
									{
										goto Label1;
									}
								}
								else
								{
									if (str2 != "!")
									{
										aDFilter1 = null;
										return aDFilter1;
									}
									arrayLists.Type = ADFilter.FilterType.Not;
									aDFilter = FilterParser.ParseFilterString((string)arrayLists1[0]);
									if (arrayLists1.Count > 1 || aDFilter == null)
									{
										aDFilter1 = null;
										return aDFilter1;
									}
									else
									{
										arrayLists.Filter.Not = aDFilter;
										goto Label1;
									}
								}
							}
						}
						aDFilter1 = null;
						return aDFilter1;
					}
					else
					{
						if (match.Groups["present"].ToString().Length == 0)
						{
							if (match.Groups["simple"].ToString().Length == 0)
							{
								if (match.Groups["substr"].ToString().Length == 0)
								{
									if (match.Groups["extensible"].ToString().Length == 0)
									{
										aDFilter1 = null;
										return aDFilter1;
									}
									else
									{
										arrayLists.Type = ADFilter.FilterType.ExtensibleMatch;
										ADExtenMatchFilter aDExtenMatchFilter = new ADExtenMatchFilter();
										aDExtenMatchFilter.Value = FilterParser.StringFilterValueToADValue(match.Groups["extenvalue"].ToString());
										aDExtenMatchFilter.DNAttributes = match.Groups["dnattr"].ToString().Length != 0;
										aDExtenMatchFilter.Name = match.Groups["extenattr"].ToString();
										aDExtenMatchFilter.MatchingRule = match.Groups["matchrule"].ToString();
										arrayLists.Filter.ExtensibleMatch = aDExtenMatchFilter;
									}
								}
								else
								{
									arrayLists.Type = ADFilter.FilterType.Substrings;
									ADSubstringFilter aDSubstringFilter = new ADSubstringFilter();
									aDSubstringFilter.Initial = FilterParser.StringFilterValueToADValue(match.Groups["initialvalue"].ToString());
									aDSubstringFilter.Final = FilterParser.StringFilterValueToADValue(match.Groups["finalvalue"].ToString());
									if (match.Groups["anyvalue"].ToString().Length != 0)
									{
										foreach (Capture capture in match.Groups["anyvalue"].Captures)
										{
											aDSubstringFilter.Any.Add(FilterParser.StringFilterValueToADValue(capture.ToString()));
										}
									}
									aDSubstringFilter.Name = match.Groups["substrattr"].ToString();
									arrayLists.Filter.Substrings = aDSubstringFilter;
								}
							}
							else
							{
								ADAttribute aDAttribute = new ADAttribute();
								if (match.Groups["simplevalue"].ToString().Length != 0)
								{
									ADValue aDValue = FilterParser.StringFilterValueToADValue(match.Groups["simplevalue"].ToString());
									aDAttribute.Values.Add(aDValue);
								}
								aDAttribute.Name = match.Groups["simpleattr"].ToString();
								string str3 = match.Groups["filtertype"].ToString();
								string str4 = str3;
								if (str3 != null)
								{
									if (str4 == "=")
									{
										arrayLists.Type = ADFilter.FilterType.EqualityMatch;
										arrayLists.Filter.EqualityMatch = aDAttribute;
										goto Label1;
									}
									else
									{
										if (str4 == "~=")
										{
											arrayLists.Type = ADFilter.FilterType.ApproxMatch;
											arrayLists.Filter.ApproxMatch = aDAttribute;
											goto Label1;
										}
										else
										{
											if (str4 == "<=")
											{
												arrayLists.Type = ADFilter.FilterType.LessOrEqual;
												arrayLists.Filter.LessOrEqual = aDAttribute;
												goto Label1;
											}
											else
											{
												if (str4 != ">=")
												{
													aDFilter1 = null;
													return aDFilter1;
												}
												arrayLists.Type = ADFilter.FilterType.GreaterOrEqual;
												arrayLists.Filter.GreaterOrEqual = aDAttribute;
												goto Label1;
											}
										}
									}
								}
								aDFilter1 = null;
								return aDFilter1;
							}
						}
						else
						{
							arrayLists.Type = ADFilter.FilterType.Present;
							arrayLists.Filter.Present = match.Groups["presentattr"].ToString();
						}
					}
				Label1:
					aDFilter1 = arrayLists;
				}
				else
				{
					aDFilter1 = null;
				}
			}
			catch (Exception regexMatchTimeoutException)
			{
				aDFilter1 = null;
			}
			return aDFilter1;
		}

		protected static ADValue StringFilterValueToADValue(string strVal)
		{
			if (strVal == null || strVal.Length == 0)
			{
				return null;
			}
			else
			{
				ADValue aDValue = new ADValue();
				char[] chrArray = new char[1];
				chrArray[0] = '\\';
				string[] strArrays = strVal.Split(chrArray);
				if ((int)strArrays.Length != 1)
				{
					ArrayList arrayLists = new ArrayList((int)strArrays.Length);
					UTF8Encoding uTF8Encoding = new UTF8Encoding();
					aDValue.IsBinary = true;
					aDValue.StringVal = null;
					if (strArrays[0].Length != 0)
					{
						arrayLists.Add(uTF8Encoding.GetBytes(strArrays[0]));
					}
					for (int i = 1; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i].Substring(0, 2);
						byte[] numArray = new byte[1];
						numArray[0] = byte.Parse(str, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
						arrayLists.Add(numArray);
						if (strArrays[i].Length > 2)
						{
							arrayLists.Add(uTF8Encoding.GetBytes(strArrays[i].Substring(2)));
						}
					}
					int length = 0;
					foreach (byte[] arrayList in arrayLists)
					{
						length = length + (int)arrayList.Length;
					}
					aDValue.BinaryVal = new byte[length];
					int num = 0;
					foreach (byte[] arrayList1 in arrayLists)
					{
						arrayList1.CopyTo(aDValue.BinaryVal, num);
						num = num + (int)arrayList1.Length;
					}
				}
				else
				{
					aDValue.IsBinary = false;
					aDValue.StringVal = strVal;
					aDValue.BinaryVal = null;
				}
				return aDValue;
			}
		}
	}
}