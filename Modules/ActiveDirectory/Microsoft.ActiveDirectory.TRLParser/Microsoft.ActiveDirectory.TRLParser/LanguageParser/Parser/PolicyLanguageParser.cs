using Microsoft.ActiveDirectory.TRLParser;
using Microsoft.ActiveDirectory.TRLParser.LanguageParser;
using Microsoft.ActiveDirectory.TRLParser.LanguageParser.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal class PolicyLanguageParser : Parser
	{
		private static Dictionary<int, string> _actionHandlerNames;

		private static Type _thisType;

		private Stack<object> _policyObjects;

		private PolicyRuleSet _policy;

		public PolicyRuleSet Policy
		{
			get
			{
				return this._policy;
			}
		}

		static PolicyLanguageParser()
		{
			PolicyLanguageParser._actionHandlerNames = new Dictionary<int, string>();
			PolicyLanguageParser._thisType = typeof(PolicyLanguageParser);
			string[] names = Enum.GetNames(typeof(GrammarRule));
			for (int i = 0; i < (int)names.Length; i++)
			{
				PolicyLanguageParser._actionHandlerNames[i] = string.Concat("Handle", names[i]);
			}
		}

		public PolicyLanguageParser()
		{
			this._policyObjects = new Stack<object>();
			this._policy = new PolicyRuleSet();
		}

		protected override void DoAction(int action)
		{
			PolicyLanguageParser._thisType.InvokeMember(PolicyLanguageParser._actionHandlerNames[action], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, this, null, null);
		}

		private string GetUnquotedString(string literal)
		{
			string str = literal;
			if (!string.IsNullOrEmpty(str))
			{
				str = str.Remove(0, 1);
				str = str.Remove(str.Length - 1, 1);
			}
			return str;
		}

		private void HandleClaimCopy__CLAIM_ASSIGN_IDENTIFIER()
		{
			DebugLog.PolicyEngineTraceLog.Assert(this.valueStack.Top - 1 >= 0, "there should be more than 0 value in stack", new object[0]);
			string stringValue = this.valueStack.Elements[this.valueStack.Top - 1].StringValue;
			if (!string.IsNullOrEmpty(stringValue))
			{
				CopyClaimIssuanceStatement copyClaimIssuanceStatement = new CopyClaimIssuanceStatement(stringValue);
				this._policyObjects.Push(copyClaimIssuanceStatement);
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "identifier";
				throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
			}
		}

		private void HandleClaimNew__ClaimPropAssignList()
		{
			if (this._policyObjects.Count != 0)
			{
				Collection<ClaimPropertyAssignment> claimPropertyAssignments = this._policyObjects.Peek() as Collection<ClaimPropertyAssignment>;
				if (claimPropertyAssignments != null)
				{
					this._policyObjects.Pop();
					NewClaimIssuanceStatement newClaimIssuanceStatement = new NewClaimIssuanceStatement();
					foreach (ClaimPropertyAssignment claimPropertyAssignment in claimPropertyAssignments)
					{
						newClaimIssuanceStatement.ClaimPropertyAssignments.Add(claimPropertyAssignment);
					}
					this._policyObjects.Push(newClaimIssuanceStatement);
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Collection<ClaimPropertyAssignment>";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimProp__TYPE()
		{
			ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.Type);
			this._policyObjects.Push(claimProperty);
		}

		private void HandleClaimProp__VALUE()
		{
			ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.Value);
			this._policyObjects.Push(claimProperty);
		}

		private void HandleClaimPropAssign__ClaimTypeAssign()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimPropertyAssignment claimPropertyAssignment = this._policyObjects.Peek() as ClaimPropertyAssignment;
				if (claimPropertyAssignment != null)
				{
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "ClaimTypeAssign";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimPropAssign__ClaimValueAssign()
		{
			Collection<ClaimPropertyAssignment> claimPropertyAssignments = this._policyObjects.Peek() as Collection<ClaimPropertyAssignment>;
			if (claimPropertyAssignments != null)
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Collection<ClaimValueAssign>";
				throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
			}
		}

		private void HandleClaimPropAssignList__ClaimPropAssign()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimPropertyAssignment claimPropertyAssignment = this._policyObjects.Peek() as ClaimPropertyAssignment;
				if (claimPropertyAssignment != null)
				{
					this._policyObjects.Pop();
					Collection<ClaimPropertyAssignment> claimPropertyAssignments = new Collection<ClaimPropertyAssignment>();
					claimPropertyAssignments.Add(claimPropertyAssignment);
					this._policyObjects.Push(claimPropertyAssignments);
					return;
				}
				else
				{
					Collection<ClaimPropertyAssignment> claimPropertyAssignments1 = this._policyObjects.Peek() as Collection<ClaimPropertyAssignment>;
					if (claimPropertyAssignments1 != null)
					{
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "ClaimPropertyAssignment or Collection<ClaimPropertyAssignment>";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimPropAssignList__ClaimPropAssignList_COMMA_ClaimPropAssign()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimPropertyAssignment claimPropertyAssignment = this._policyObjects.Peek() as ClaimPropertyAssignment;
				Collection<ClaimPropertyAssignment> claimPropertyAssignments = null;
				if (claimPropertyAssignment == null)
				{
					claimPropertyAssignments = this._policyObjects.Peek() as Collection<ClaimPropertyAssignment>;
					if (claimPropertyAssignments == null)
					{
						object[] objArray = new object[1];
						objArray[0] = "ClaimPropertyAssignment or Collection<ClaimPropertyAssignment>";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				this._policyObjects.Pop();
				Collection<ClaimPropertyAssignment> claimPropertyAssignments1 = this._policyObjects.Peek() as Collection<ClaimPropertyAssignment>;
				if (claimPropertyAssignments1 != null)
				{
					if (claimPropertyAssignment == null)
					{
						if (claimPropertyAssignments == null)
						{
							object[] objArray1 = new object[1];
							objArray1[0] = "ClaimPropertyAssignment or Collection<ClaimPropertyAssignment>";
							throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
						}
						else
						{
							foreach (ClaimPropertyAssignment claimPropertyAssignment1 in claimPropertyAssignments)
							{
								claimPropertyAssignments1.Add(claimPropertyAssignment1);
							}
							return;
						}
					}
					else
					{
						claimPropertyAssignments1.Add(claimPropertyAssignment);
						return;
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = "Collection<ClaimPropertyAssignment>";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray2));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimTypeAssign__TYPE_ASSIGN_expr()
		{
			if (this._policyObjects.Count != 0)
			{
				Expression expression = this._policyObjects.Peek() as Expression;
				if (expression != null)
				{
					this._policyObjects.Pop();
					ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.Type);
					this._policyObjects.Push(new ClaimPropertyAssignment(claimProperty, expression));
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Expression";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimValAssign__VALUE_ASSIGN_Expr()
		{
			if (this._policyObjects.Count != 0)
			{
				Expression expression = this._policyObjects.Peek() as Expression;
				if (expression != null)
				{
					this._policyObjects.Pop();
					ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.Value);
					this._policyObjects.Push(new ClaimPropertyAssignment(claimProperty, expression));
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Expression";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimValTypeAssign__VALUETYPE_ASSIGN_ValueTypeExpr()
		{
			if (this._policyObjects.Count != 0)
			{
				Expression expression = this._policyObjects.Peek() as Expression;
				if (expression != null)
				{
					this._policyObjects.Pop();
					ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.ValueType);
					this._policyObjects.Push(new ClaimPropertyAssignment(claimProperty, expression));
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Expression";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimValueAssign__ClaimValAssign_COMMA_ClaimValTypeAssign()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimPropertyAssignment claimPropertyAssignment = this._policyObjects.Peek() as ClaimPropertyAssignment;
				if (claimPropertyAssignment != null)
				{
					this._policyObjects.Pop();
					Collection<ClaimPropertyAssignment> claimPropertyAssignments = new Collection<ClaimPropertyAssignment>();
					claimPropertyAssignments.Add(claimPropertyAssignment);
					claimPropertyAssignment = this._policyObjects.Peek() as ClaimPropertyAssignment;
					if (claimPropertyAssignment != null)
					{
						this._policyObjects.Pop();
						claimPropertyAssignments.Add(claimPropertyAssignment);
						this._policyObjects.Push(claimPropertyAssignments);
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "ClaimValueTypeAssignment";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "ClaimValueAssignment";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleClaimValueAssign__ClaimValTypeAssign_COMMA_ClaimValAssign()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimPropertyAssignment claimPropertyAssignment = this._policyObjects.Peek() as ClaimPropertyAssignment;
				if (claimPropertyAssignment != null)
				{
					this._policyObjects.Pop();
					Collection<ClaimPropertyAssignment> claimPropertyAssignments = new Collection<ClaimPropertyAssignment>();
					claimPropertyAssignments.Add(claimPropertyAssignment);
					claimPropertyAssignment = this._policyObjects.Peek() as ClaimPropertyAssignment;
					if (claimPropertyAssignment != null)
					{
						this._policyObjects.Pop();
						claimPropertyAssignments.Add(claimPropertyAssignment);
						this._policyObjects.Push(claimPropertyAssignments);
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "ClaimValueAssignment";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "ClaimValueTypeAssignment";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleCond__TypeCond()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimCondition claimCondition = this._policyObjects.Peek() as ClaimCondition;
				if (claimCondition != null)
				{
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "ClaimCondition";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleCond__ValueCond()
		{
			Collection<ClaimCondition> claimConditions = this._policyObjects.Peek() as Collection<ClaimCondition>;
			if (claimConditions != null)
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Collection<ClaimCondition>";
				throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
			}
		}

		private void HandleConditions()
		{
			this._policyObjects.Push(ConditionType.Unconditional);
		}

		private void HandleConditions__SelConditionList()
		{
			this._policyObjects.Push(ConditionType.Selection);
		}

		private void HandleConditions__SelConditionList_AND_SelConditionList()
		{
			if (this._policyObjects.Count != 0)
			{
				Collection<SelectionCondition> selectionConditions = this._policyObjects.Peek() as Collection<SelectionCondition>;
				if (selectionConditions != null)
				{
					this._policyObjects.Pop();
					Collection<SelectionCondition> selectionConditions1 = this._policyObjects.Peek() as Collection<SelectionCondition>;
					if (selectionConditions1 != null)
					{
						foreach (SelectionCondition selectionCondition in selectionConditions)
						{
							selectionConditions1.Add(selectionCondition);
						}
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "Collection<SelectionCondition>";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "Collection<SelectionCondition>";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleCondList__Cond()
		{
			Collection<ClaimCondition> claimConditions;
			if (this._policyObjects.Count != 0)
			{
				ClaimCondition claimCondition = this._policyObjects.Peek() as ClaimCondition;
				if (claimCondition != null)
				{
					claimConditions = new Collection<ClaimCondition>();
					claimConditions.Add(claimCondition);
					this._policyObjects.Pop();
					this._policyObjects.Push(claimConditions);
					return;
				}
				else
				{
					claimConditions = this._policyObjects.Peek() as Collection<ClaimCondition>;
					if (claimConditions != null)
					{
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "ClaimCondition or Collection<ClaimCondition>";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleCondList__CondList_COMMA_Cond()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimCondition claimCondition = this._policyObjects.Peek() as ClaimCondition;
				Collection<ClaimCondition> claimConditions = null;
				if (claimCondition == null)
				{
					claimConditions = this._policyObjects.Peek() as Collection<ClaimCondition>;
					if (claimConditions == null)
					{
						object[] objArray = new object[1];
						objArray[0] = "ClaimCondition or Collection<ClaimCondition>";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				this._policyObjects.Pop();
				Collection<ClaimCondition> claimConditions1 = this._policyObjects.Peek() as Collection<ClaimCondition>;
				if (claimConditions1 != null)
				{
					if (claimCondition == null)
					{
						if (claimConditions == null)
						{
							object[] objArray1 = new object[1];
							objArray1[0] = "ClaimCondition or Collection<ClaimCondition>";
							throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
						}
						else
						{
							foreach (ClaimCondition claimCondition1 in claimConditions)
							{
								claimConditions1.Add(claimCondition1);
							}
							return;
						}
					}
					else
					{
						claimConditions1.Add(claimCondition);
						return;
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = "Collection<ClaimCondition>";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray2));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleCondOper__EQ()
		{
			this._policyObjects.Push(ConditionOperator.EQ);
		}

		private void HandleCondOper__NEQ()
		{
			this._policyObjects.Push(ConditionOperator.NEQ);
		}

		private void HandleCondOper__REGEXPMATCH()
		{
			this._policyObjects.Push(ConditionOperator.REGEXP_MATCH);
		}

		private void HandleCondOper__REGEXPNOTMATCH()
		{
			this._policyObjects.Push(ConditionOperator.REGEXP_NOT_MATCH);
		}

		private void HandleEOF()
		{
		}

		private void HandleExpr__IDENTIFIER_DOT_ClaimProp()
		{
			if (this._policyObjects.Count != 0)
			{
				DebugLog.PolicyEngineTraceLog.Assert(this.valueStack.Top - 3 >= 0, "there should be more than 2 values in stack", new object[0]);
				string stringValue = this.valueStack.Elements[this.valueStack.Top - 3].StringValue;
				if (!string.IsNullOrEmpty(stringValue))
				{
					ClaimProperty claimProperty = this._policyObjects.Peek() as ClaimProperty;
					if (claimProperty != null)
					{
						this._policyObjects.Pop();
						ClaimPropertyAccessExpression claimPropertyAccessExpression = new ClaimPropertyAccessExpression(claimProperty, stringValue);
						this._policyObjects.Push(claimPropertyAccessExpression);
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "ClaimProperty";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "Identifier";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleExpr__Literal()
		{
			if (this._policyObjects.Count != 0)
			{
				string str = this._policyObjects.Peek() as string;
				if (str != null)
				{
					this._policyObjects.Pop();
					this._policyObjects.Push(new StringLiteralExpression(str));
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "StringLiteral";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleExpr__ValueTypeExpr()
		{
			if (this._policyObjects.Count != 0)
			{
				Expression expression = this._policyObjects.Peek() as Expression;
				if (expression != null)
				{
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "ValueTypeExpression";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleIssueParams__ClaimCopy()
		{
		}

		private void HandleIssueParams__ClaimNew()
		{
		}

		private void HandleLiteral__STRING()
		{
			string stringValue = this.yyval.StringValue;
			stringValue = this.GetUnquotedString(stringValue);
			this._policyObjects.Push(stringValue);
		}

		private void HandleOptCondList()
		{
			Collection<ClaimCondition> claimConditions = new Collection<ClaimCondition>();
			this._policyObjects.Push(claimConditions);
		}

		private void HandleOptCondList_CondList()
		{
		}

		private void HandleProgram()
		{
		}

		private void HandleProgram__Rules()
		{
			if (this._policyObjects.Count != 0)
			{
				Collection<Microsoft.ActiveDirectory.TRLParser.LanguageParser.Rule> rules = this._policyObjects.Peek() as Collection<Microsoft.ActiveDirectory.TRLParser.LanguageParser.Rule>;
				if (rules != null)
				{
					foreach (Microsoft.ActiveDirectory.TRLParser.LanguageParser.Rule rule in rules)
					{
						this._policy.Rules.Add(rule);
					}
					this._policyObjects.Pop();
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Collection<Language.Rule>";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleRule__RuleBody()
		{
		}

		private void HandleRuleAction__ISSUE_OBRACKET__IssueParams__CBRACKET()
		{
			this._policyObjects.Push(RuleOutput.EvaluationContextAndOutput);
		}

		private void HandleRuleBody__Conditions_IMPLY_RuleAction_SEMICOLON()
		{
			Microsoft.ActiveDirectory.TRLParser.LanguageParser.Rule unconditionalRule;
			if (this._policyObjects.Count != 0)
			{
				object obj = this._policyObjects.Peek();
				if (obj.GetType() == typeof(RuleOutput))
				{
					RuleOutput ruleOutput = (RuleOutput)obj;
					this._policyObjects.Pop();
					IssuanceStatement issuanceStatement = this._policyObjects.Peek() as IssuanceStatement;
					if (issuanceStatement != null)
					{
						this._policyObjects.Pop();
						obj = this._policyObjects.Peek();
						if (obj.GetType() == typeof(ConditionType))
						{
							ConditionType conditionType = (ConditionType)obj;
							this._policyObjects.Pop();
							ConditionType conditionType1 = conditionType;
							if (conditionType1 == ConditionType.Unconditional)
							{
								unconditionalRule = new UnconditionalRule(issuanceStatement);
							}
							else if (conditionType1 == ConditionType.Selection)
							{
								unconditionalRule = new SelectionRule(issuanceStatement);
								Collection<SelectionCondition> selectionConditions = this._policyObjects.Peek() as Collection<SelectionCondition>;
								foreach (SelectionCondition selectionCondition in selectionConditions)
								{
									((SelectionRule)unconditionalRule).Conditions.Add(selectionCondition);
								}
								this._policyObjects.Pop();
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = (object)conditionType;
								throw new InvalidOperationException(SR.GetString("POLICY0034", objArray));
							}
							unconditionalRule.Output = ruleOutput;
							this._policyObjects.Push(unconditionalRule);
							return;
						}
						else
						{
							object[] objArray1 = new object[1];
							objArray1[0] = "ConditionType";
							throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
						}
					}
					else
					{
						object[] objArray2 = new object[1];
						objArray2[0] = "IssuanceStatement";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray2));
					}
				}
				else
				{
					object[] objArray3 = new object[1];
					objArray3[0] = "RuleOutput";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray3));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleRules__Rule()
		{
			if (this._policyObjects.Count != 0)
			{
				Rule rule = this._policyObjects.Peek() as Rule;
				if (rule != null)
				{
					Collection<Rule> rules = new Collection<Rule>();
					rules.Add(rule);
					this._policyObjects.Pop();
					this._policyObjects.Push(rules);
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Language.Rule";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleRules__Rules_Rule()
		{
			if (this._policyObjects.Count != 0)
			{
				Rule rule = this._policyObjects.Peek() as Rule;
				if (rule != null)
				{
					this._policyObjects.Pop();
					Collection<Rule> rules = this._policyObjects.Peek() as Collection<Rule>;
					if (rules != null)
					{
						rules.Add(rule);
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "Collection<Language.Rule>";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "Language.Rule";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleSelCondition__IDENTIFIER_COLON_SelConditionBody()
		{
			if (this._policyObjects.Count != 0)
			{
				DebugLog.PolicyEngineTraceLog.Assert(this.valueStack.Top - 3 >= 0, "there should be more than 2 values in stack", new object[0]);
				string stringValue = this.valueStack.Elements[this.valueStack.Top - 3].StringValue;
				if (!string.IsNullOrEmpty(stringValue))
				{
					SelectionCondition selectionCondition = this._policyObjects.Peek() as SelectionCondition;
					if (selectionCondition != null)
					{
						selectionCondition.ConditionTag = stringValue;
						return;
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = "SelectionCondition";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "Identifier";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleSelCondition__SelConditionBody()
		{
			if (this._policyObjects.Count != 0)
			{
				SelectionCondition selectionCondition = this._policyObjects.Peek() as SelectionCondition;
				if (selectionCondition != null)
				{
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "SelectionCondition";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleSelConditionBody__OSQBRACKET_OptCondList_CSQBRACKET()
		{
			if (this._policyObjects.Count != 0)
			{
				Collection<ClaimCondition> claimConditions = this._policyObjects.Peek() as Collection<ClaimCondition>;
				if (claimConditions != null)
				{
					this._policyObjects.Pop();
					SelectionCondition selectionCondition = new SelectionCondition();
					foreach (ClaimCondition claimCondition in claimConditions)
					{
						selectionCondition.Conditions.Add(claimCondition);
					}
					this._policyObjects.Push(selectionCondition);
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Collection<ClaimCondition>";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleSelConditionList__SelCondition()
		{
			if (this._policyObjects.Count != 0)
			{
				SelectionCondition selectionCondition = this._policyObjects.Peek() as SelectionCondition;
				if (selectionCondition != null)
				{
					Collection<SelectionCondition> selectionConditions = new Collection<SelectionCondition>();
					selectionConditions.Add(selectionCondition);
					this._policyObjects.Pop();
					this._policyObjects.Push(selectionConditions);
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "SelectionCondition";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleTypeCond__TYPE_CondOper_Expr()
		{
			ClaimCondition claimCondition;
			if (this._policyObjects.Count != 0)
			{
				Expression expression = this._policyObjects.Peek() as Expression;
				if (expression != null)
				{
					this._policyObjects.Pop();
					string str = this._policyObjects.Peek() as string;
					if (expression != null)
					{
						this._policyObjects.Pop();
						ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.Type);
						if (string.CompareOrdinal(str, ConditionOperator.EQ) != 0)
						{
							if (string.CompareOrdinal(str, ConditionOperator.NEQ) != 0)
							{
								if (string.CompareOrdinal(str, ConditionOperator.REGEXP_MATCH) != 0)
								{
									if (string.CompareOrdinal(str, ConditionOperator.REGEXP_NOT_MATCH) != 0)
									{
										object[] objArray = new object[1];
										objArray[0] = str;
										throw new InvalidOperationException(SR.GetString("POLICY0035", objArray));
									}
									else
									{
										claimCondition = ClaimCondition.RegexNotMatch(claimProperty, expression);
									}
								}
								else
								{
									claimCondition = ClaimCondition.RegexMatch(claimProperty, expression);
								}
							}
							else
							{
								claimCondition = ClaimCondition.NotEqual(claimProperty, expression);
							}
						}
						else
						{
							claimCondition = ClaimCondition.Equal(claimProperty, expression);
						}
						this._policyObjects.Push(claimCondition);
						return;
					}
					else
					{
						object[] objArray1 = new object[1];
						objArray1[0] = "ConditionOperator";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = "Expression";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray2));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleValCond__VALUE_CondOper_Expr()
		{
			ClaimCondition claimCondition;
			if (this._policyObjects.Count != 0)
			{
				Expression expression = this._policyObjects.Peek() as Expression;
				if (expression != null)
				{
					this._policyObjects.Pop();
					string str = this._policyObjects.Peek() as string;
					if (expression != null)
					{
						this._policyObjects.Pop();
						ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.Value);
						if (string.CompareOrdinal(str, ConditionOperator.EQ) != 0)
						{
							if (string.CompareOrdinal(str, ConditionOperator.NEQ) != 0)
							{
								if (string.CompareOrdinal(str, ConditionOperator.REGEXP_MATCH) != 0)
								{
									if (string.CompareOrdinal(str, ConditionOperator.REGEXP_NOT_MATCH) != 0)
									{
										object[] objArray = new object[1];
										objArray[0] = str;
										throw new InvalidOperationException(SR.GetString("POLICY0035", objArray));
									}
									else
									{
										claimCondition = ClaimCondition.RegexNotMatch(claimProperty, expression);
									}
								}
								else
								{
									claimCondition = ClaimCondition.RegexMatch(claimProperty, expression);
								}
							}
							else
							{
								claimCondition = ClaimCondition.NotEqual(claimProperty, expression);
							}
						}
						else
						{
							claimCondition = ClaimCondition.Equal(claimProperty, expression);
						}
						this._policyObjects.Push(claimCondition);
						return;
					}
					else
					{
						object[] objArray1 = new object[1];
						objArray1[0] = "ConditionOperator";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = "Expression";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray2));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleValTypeCond__VALUE_TYPE_CondOper_ValueTypeExpr()
		{
			ClaimCondition claimCondition;
			if (this._policyObjects.Count != 0)
			{
				Expression expression = this._policyObjects.Peek() as Expression;
				if (expression != null)
				{
					this._policyObjects.Pop();
					string str = this._policyObjects.Peek() as string;
					if (expression != null)
					{
						this._policyObjects.Pop();
						ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.ValueType);
						if (string.CompareOrdinal(str, ConditionOperator.EQ) != 0)
						{
							if (string.CompareOrdinal(str, ConditionOperator.NEQ) != 0)
							{
								if (string.CompareOrdinal(str, ConditionOperator.REGEXP_MATCH) != 0)
								{
									if (string.CompareOrdinal(str, ConditionOperator.REGEXP_NOT_MATCH) != 0)
									{
										object[] objArray = new object[1];
										objArray[0] = str;
										throw new InvalidOperationException(SR.GetString("POLICY0035", objArray));
									}
									else
									{
										claimCondition = ClaimCondition.RegexNotMatch(claimProperty, expression);
									}
								}
								else
								{
									claimCondition = ClaimCondition.RegexMatch(claimProperty, expression);
								}
							}
							else
							{
								claimCondition = ClaimCondition.NotEqual(claimProperty, expression);
							}
						}
						else
						{
							claimCondition = ClaimCondition.Equal(claimProperty, expression);
						}
						this._policyObjects.Push(claimCondition);
						return;
					}
					else
					{
						object[] objArray1 = new object[1];
						objArray1[0] = "ConditionOperator";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = "Expression";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray2));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleValueCond__ValCond_COMMA_ValTypeCond()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimCondition claimCondition = this._policyObjects.Peek() as ClaimCondition;
				if (claimCondition == null || claimCondition.ClaimProperty.PropertyType != ClaimPropertyType.ValueType)
				{
					object[] objArray = new object[1];
					objArray[0] = "Value Type Condition";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
				else
				{
					Collection<ClaimCondition> claimConditions = new Collection<ClaimCondition>();
					claimConditions.Add(claimCondition);
					this._policyObjects.Pop();
					claimCondition = this._policyObjects.Peek() as ClaimCondition;
					if (claimCondition == null || claimCondition.ClaimProperty.PropertyType != ClaimPropertyType.Value)
					{
						object[] objArray1 = new object[1];
						objArray1[0] = "Value Condition";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
					}
					else
					{
						claimConditions.Add(claimCondition);
						this._policyObjects.Pop();
						this._policyObjects.Push(claimConditions);
						return;
					}
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleValueCond__ValTypeCond_COMMA_ValCond()
		{
			if (this._policyObjects.Count != 0)
			{
				ClaimCondition claimCondition = this._policyObjects.Peek() as ClaimCondition;
				if (claimCondition == null || claimCondition.ClaimProperty.PropertyType != ClaimPropertyType.Value)
				{
					object[] objArray = new object[1];
					objArray[0] = "Value Condition";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
				else
				{
					Collection<ClaimCondition> claimConditions = new Collection<ClaimCondition>();
					claimConditions.Add(claimCondition);
					this._policyObjects.Pop();
					claimCondition = this._policyObjects.Peek() as ClaimCondition;
					if (claimCondition == null || claimCondition.ClaimProperty.PropertyType != ClaimPropertyType.ValueType)
					{
						object[] objArray1 = new object[1];
						objArray1[0] = "Value Type Condition";
						throw new InvalidOperationException(SR.GetString("POLICY0037", objArray1));
					}
					else
					{
						claimConditions.Add(claimCondition);
						this._policyObjects.Pop();
						this._policyObjects.Push(claimConditions);
						return;
					}
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleValueTypeExpr__IDENTIFIER_DOT_VALUETYPE()
		{
			if (this._policyObjects.Count != 0)
			{
				DebugLog.PolicyEngineTraceLog.Assert(this.valueStack.Top - 3 >= 0, "there should be more than 2 values in stack", new object[0]);
				string stringValue = this.valueStack.Elements[this.valueStack.Top - 3].StringValue;
				if (!string.IsNullOrEmpty(stringValue))
				{
					ClaimProperty claimProperty = new ClaimProperty(ClaimPropertyType.ValueType);
					ClaimPropertyAccessExpression claimPropertyAccessExpression = new ClaimPropertyAccessExpression(claimProperty, stringValue);
					this._policyObjects.Push(claimPropertyAccessExpression);
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "Identifier";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleValueTypeExpr__ValueTypeLiteral()
		{
			if (this._policyObjects.Count != 0)
			{
				string str = this._policyObjects.Peek() as string;
				if (str != null)
				{
					this._policyObjects.Pop();
					this._policyObjects.Push(new StringLiteralExpression(str));
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = "ValueTypeLiteral";
					throw new InvalidOperationException(SR.GetString("POLICY0037", objArray));
				}
			}
			else
			{
				throw new InvalidOperationException(SR.GetString("POLICY0038", new object[0]));
			}
		}

		private void HandleValueTypeLiteral__BOOLEAN_TYPE()
		{
			string stringValue = this.yyval.StringValue;
			stringValue = this.GetUnquotedString(stringValue);
			this._policyObjects.Push(stringValue);
		}

		private void HandleValueTypeLiteral__INT64_TYPE()
		{
			string stringValue = this.yyval.StringValue;
			stringValue = this.GetUnquotedString(stringValue);
			this._policyObjects.Push(stringValue);
		}

		private void HandleValueTypeLiteral__STRING_TYPE()
		{
			string stringValue = this.yyval.StringValue;
			stringValue = this.GetUnquotedString(stringValue);
			this._policyObjects.Push(stringValue);
		}

		private void HandleValueTypeLiteral__UINT64_TYPE()
		{
			string stringValue = this.yyval.StringValue;
			stringValue = this.GetUnquotedString(stringValue);
			this._policyObjects.Push(stringValue);
		}
	}
}