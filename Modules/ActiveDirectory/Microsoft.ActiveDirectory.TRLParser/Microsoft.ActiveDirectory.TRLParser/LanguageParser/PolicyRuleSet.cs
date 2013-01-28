using Microsoft.ActiveDirectory.TRLParser;
using Microsoft.ActiveDirectory.TRLParser.LanguageParser.Diagnostics;
using Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser;
using System;
using System.IO;
using System.Text;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class PolicyRuleSet
	{
		private NonNullableCollection<Rule> _rules;

		public NonNullableCollection<Rule> Rules
		{
			get
			{
				return this._rules;
			}
		}

		public PolicyRuleSet()
		{
			this._rules = new NonNullableCollection<Rule>();
		}

		public virtual bool Compare(PolicyRuleSet other)
		{
			if (other != null)
			{
				if (this.Rules.Count == other.Rules.Count)
				{
					int num = 0;
					while (num < this.Rules.Count)
					{
						if (this.Rules[num].Compare(other.Rules[num]))
						{
							num++;
						}
						else
						{
							return false;
						}
					}
					return true;
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

		public PolicyRuleSetStruct GetStruct()
		{
			unsafe
			{
				PolicyRuleSetStruct count = new PolicyRuleSetStruct();
				count.ruleCount = this.Rules.Count;
				count.ruleArray = new RuleStruct[count.ruleCount];
				int num = 0;
				foreach (Rule rule in this.Rules)
				{
					int num1 = num;
					num = num1 + 1;
					count.ruleArray[num1] = rule.GetStruct();
				}
				return count;
			}
		}

		public virtual void Initialize(string policy)
		{
			bool flag;
			this._rules = new NonNullableCollection<Rule>();
			if (!string.IsNullOrEmpty(policy))
			{
				PolicyLanguageParser policyLanguageParser = new PolicyLanguageParser();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (StreamWriter streamWriter = new StreamWriter(memoryStream))
					{
						streamWriter.Write(policy);
						streamWriter.Flush();
						memoryStream.Seek((long)0, SeekOrigin.Begin);
						policyLanguageParser.Scanner = new Scanner(memoryStream);
						try
						{
							flag = policyLanguageParser.Parse();
						}
						catch (PolicyLanguageParserException policyLanguageParserException1)
						{
							PolicyLanguageParserException policyLanguageParserException = policyLanguageParserException1;
							memoryStream.Seek((long)0, SeekOrigin.Begin);
							string empty = string.Empty;
							using (StreamReader streamReader = new StreamReader(memoryStream))
							{
								int num = 1;
								while (true)
								{
									string str = streamReader.ReadLine();
									string str1 = str;
									if (str == null)
									{
										break;
									}
									int num1 = num;
									num = num1 + 1;
									if (num1 == policyLanguageParserException.LineNumber)
									{
										empty = str1;
									}
								}
							}
							object[] newLine = new object[7];
							newLine[0] = Environment.NewLine;
							newLine[1] = policyLanguageParserException.LineNumber;
							newLine[2] = policyLanguageParserException.ColumnNumber;
							newLine[3] = policyLanguageParserException.Text;
							newLine[4] = empty;
							newLine[5] = Environment.NewLine;
							newLine[6] = policyLanguageParserException.ErrMessage;
							throw new PolicyValidationException(SR.GetString("POLICY0002", newLine), policyLanguageParserException);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							if (!ExceptionUtility.IsFatal(exception))
							{
								object[] message = new object[1];
								message[0] = exception.Message;
								throw new PolicyValidationException(SR.GetString("POLICY0003", message), exception);
							}
							else
							{
								throw;
							}
						}
					}
				}
				object[] objArray = new object[1];
				objArray[0] = policy;
				DebugLog.PolicyEngineTraceLog.Assert(flag, "Parser did not successfully parse policy data: '{0}'", objArray);
				object[] objArray1 = new object[1];
				objArray1[0] = policy;
				DebugLog.PolicyEngineTraceLog.Assert(policyLanguageParser.Policy != null, "Parser returned null policy after parsing policy data: '{0}'", objArray1);
				policyLanguageParser.Policy.Validate();
				this._rules = policyLanguageParser.Policy.Rules;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Rule rule in this.Rules)
			{
				stringBuilder.Append(rule.ToString());
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		public virtual void Validate()
		{
			foreach (Rule rule in this.Rules)
			{
				rule.Validate();
			}
		}
	}
}