using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell;
using Microsoft.PowerShell.Commands;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation.Internal;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace System.Management.Automation
{
	public static class CompletionCompleters
	{
		private const int MAX_PREFERRED_LENGTH = -1;

		private const int NERR_Success = 0;

		private const int ERROR_MORE_DATA = 234;

		private const int STYPE_DISKTREE = 0;

		private const int STYPE_MASK = 0xff;

		internal readonly static List<string> PseudoWorkflowCommands;

		private static ConcurrentDictionary<string, IEnumerable<string>> cimNamespaceToClassNames;

		private readonly static string[] VariableScopes;

		private readonly static char[] CharactersRequiringQuotes;

		private readonly static Lazy<SortedSet<string>> _specialVariablesCache;

		private static CompletionCompleters.TypeCompletionMapping[][] typeCache;

		static CompletionCompleters()
		{
			List<string> strs = new List<string>();
			strs.Add("Checkpoint-Workflow");
			strs.Add("Suspend-Workflow");
			strs.Add("InlineScript");
			CompletionCompleters.PseudoWorkflowCommands = strs;
			CompletionCompleters.cimNamespaceToClassNames = new ConcurrentDictionary<string, IEnumerable<string>>();
			string[] strArrays = new string[4];
			strArrays[0] = "Global:";
			strArrays[1] = "Local:";
			strArrays[2] = "Script:";
			strArrays[3] = "Private:";
			CompletionCompleters.VariableScopes = strArrays;
			char[] chrArray = new char[] { '-', '\u0060', '&', '@', '\'', '#', '{', '}', '(', ')', '$', ',', ';', '|', '<', '>', ' ', '.', '\\', '/', '\t', '\u005E' };
			CompletionCompleters.CharactersRequiringQuotes = chrArray;
			CompletionCompleters._specialVariablesCache = new Lazy<SortedSet<string>>(new Func<SortedSet<string>>(CompletionCompleters.BuildSpecialVariablesCache));
			AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CompletionCompleters.UpdateTypeCacheOnAssemblyLoad);
		}

		internal static PowerShell AddCommandWithPreferenceSetting(PowerShell powershell, string command)
		{
			powershell.AddCommand(command).AddParameter("ErrorAction", ActionPreference.Ignore).AddParameter("WarningAction", ActionPreference.Ignore).AddParameter("Verbose", false).AddParameter("Debug", false);
			return powershell;
		}

		private static void AddInferredMember(object member, WildcardPattern memberNamePattern, List<CompletionResult> results, bool skipMethods)
		{
			CompletionResultType completionResultType;
			string str2;
			Func<string> func = null;
			Func<string> func1 = null;
			Func<string> func2 = null;
			Func<string> func3 = null;
			Func<string> func4 = null;
			string name = null;
			bool flag = false;
			Func<string> func5 = null;
			PropertyInfo propertyInfo = member as PropertyInfo;
			if (propertyInfo != null)
			{
				name = propertyInfo.Name;
				if (func == null)
				{
					func = () => {
						string str;
						string str1;
						string[] strArrays = new string[7];
						strArrays[0] = ToStringCodeMethods.Type(propertyInfo.PropertyType, false);
						strArrays[1] = " ";
						strArrays[2] = name;
						strArrays[3] = " { ";
						string[] strArrays1 = strArrays;
						int num = 4;
						if (propertyInfo.GetGetMethod() != null)
						{
							str = "get; ";
						}
						else
						{
							str = "";
						}
						strArrays1[num] = str;
						string[] strArrays2 = strArrays;
						int num1 = 5;
						if (propertyInfo.GetSetMethod() != null)
						{
							str1 = "set; ";
						}
						else
						{
							str1 = "";
						}
						strArrays2[num1] = str1;
						strArrays[6] = "}";
						return string.Concat(strArrays);
					}
					;
				}
				func5 = func;
			}
			FieldInfo fieldInfo = member as FieldInfo;
			if (fieldInfo != null)
			{
				name = fieldInfo.Name;
				if (func1 == null)
				{
					func1 = () => string.Concat(ToStringCodeMethods.Type(fieldInfo.FieldType, false), " ", name);
				}
				func5 = func1;
			}
			DotNetAdapter.MethodCacheEntry methodCacheEntry = member as DotNetAdapter.MethodCacheEntry;
			if (methodCacheEntry != null)
			{
				name = methodCacheEntry[0].method.Name;
				flag = true;
				if (func2 == null)
				{
					func2 = () => string.Join("\n", (IEnumerable<string>) (from m in methodCacheEntry.methodInformationStructures select m.methodDefinition));
				}
				func5 = func2;
			}
			PSMemberInfo pSMemberInfo = member as PSMemberInfo;
			if (pSMemberInfo != null)
			{
				name = pSMemberInfo.Name;
				flag = member is PSMethodInfo;
				if (func3 == null)
				{
					func3 = () => pSMemberInfo.ToString();
				}
				func5 = func3;
			}
			CimPropertyDeclaration cimPropertyDeclaration = member as CimPropertyDeclaration;
			if (cimPropertyDeclaration != null)
			{
				name = cimPropertyDeclaration.Name;
				flag = false;
				if (func4 == null)
				{
					func4 = () => CompletionCompleters.GetCimPropertyToString(cimPropertyDeclaration);
				}
				func5 = func4;
			}
			if (name == null || !memberNamePattern.IsMatch(name) || skipMethods && flag)
			{
				return;
			}
			else
			{
				if (flag)
				{
					completionResultType = CompletionResultType.Method;
				}
				else
				{
					completionResultType = CompletionResultType.Property;
				}
				CompletionResultType completionResultType1 = completionResultType;
				if (flag)
				{
					str2 = string.Concat(name, "(");
				}
				else
				{
					str2 = name;
				}
				string str3 = str2;
				results.Add(new CompletionResult(str3, name, completionResultType1, func5()));
				return;
			}
		}

		private static void AddUniqueVariable(HashSet<string> hashedResults, List<CompletionResult> results, string completionText, string listItemText, string tooltip)
		{
			if (!hashedResults.Contains(completionText))
			{
				hashedResults.Add(completionText);
				results.Add(new CompletionResult(completionText, listItemText, CompletionResultType.Variable, tooltip));
			}
		}

		private static SortedSet<string> BuildSpecialVariablesCache()
		{
			SortedSet<string> strs = new SortedSet<string>();
			FieldInfo[] fields = typeof(SpecialVariables).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
			for (int i = 0; i < (int)fields.Length; i++)
			{
				FieldInfo fieldInfo = fields[i];
				if (fieldInfo.FieldType.Equals(typeof(string)))
				{
					strs.Add((string)fieldInfo.GetValue(null));
				}
			}
			return strs;
		}

		private static bool CheckFileExtension(string path, HashSet<string> extension)
		{
			if (extension == null || extension.Count == 0)
			{
				return true;
			}
			else
			{
				string str = Path.GetExtension(path);
				if (str == null)
				{
					return true;
				}
				else
				{
					return extension.Contains(str);
				}
			}
		}

		internal static string CombineVariableWithPartialPath(VariableExpressionAst variableAst, string extraText, ExecutionContext executionContext)
		{
			string str;
			string empty;
			VariablePath variablePath = variableAst.VariablePath;
			if (variablePath.IsVariable || variablePath.DriveName.Equals("env", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					object variableValue = VariableOps.GetVariableValue(variablePath, executionContext, variableAst);
					if (variableValue == null)
					{
						empty = string.Empty;
					}
					else
					{
						empty = variableValue as string;
					}
					string str1 = empty;
					if (str1 == null)
					{
						object obj = PSObject.Base(variableValue);
						if (obj as string != null || obj.GetType().IsPrimitive)
						{
							str1 = LanguagePrimitives.ConvertTo<string>(variableValue);
						}
					}
					if (str1 == null)
					{
						return null;
					}
					else
					{
						str = string.Concat(str1, extraText);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					return null;
				}
				return str;
			}
			return null;
		}

		public static IEnumerable<CompletionResult> CompleteCommand(string commandName)
		{
			return CompletionCompleters.CompleteCommand(commandName, null, CommandTypes.All);
		}

		public static IEnumerable<CompletionResult> CompleteCommand(string commandName, string moduleName, CommandTypes commandTypes = CommandTypes.All)
		{
			Runspace defaultRunspace = Runspace.DefaultRunspace;
			if (defaultRunspace != null)
			{
				CompletionExecutionHelper completionExecutionHelper = new CompletionExecutionHelper(PowerShell.Create(RunspaceMode.CurrentRunspace));
				CompletionContext completionContext = new CompletionContext();
				completionContext.WordToComplete = commandName;
				completionContext.Helper = completionExecutionHelper;
				return CompletionCompleters.CompleteCommand(completionContext, moduleName, commandTypes);
			}
			else
			{
				return CommandCompletion.EmptyCompletionResult;
			}
		}

		internal static List<CompletionResult> CompleteCommand(CompletionContext context)
		{
			return CompletionCompleters.CompleteCommand(context, null, CommandTypes.All);
		}

		private static List<CompletionResult> CompleteCommand(CompletionContext context, string moduleName, CommandTypes types = CommandTypes.All)
		{
			Exception exception = null;
			Exception exception1 = null;
			bool flag = CompletionCompleters.IsAmpersandNeeded(context, false);
			string wordToComplete = context.WordToComplete;
			string str = CompletionCompleters.HandleDoubleAndSingleQuote(ref wordToComplete);
			wordToComplete = string.Concat(wordToComplete, "*");
			List<CompletionResult> completionResults = null;
			char[] chrArray = new char[] { '/', '\\', ':' };
			if (wordToComplete.IndexOfAny(chrArray) != -1)
			{
				int num = wordToComplete.IndexOf(':');
				int num1 = wordToComplete.IndexOf('\\');
				if (num1 > 0 && (num1 < num || num == -1))
				{
					moduleName = wordToComplete.Substring(0, num1);
					wordToComplete = wordToComplete.Substring(num1 + 1);
					PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Command").AddParameter("All").AddParameter("Name", wordToComplete).AddParameter("Module", moduleName);
					if (!types.Equals(CommandTypes.All))
					{
						currentPowerShell.AddParameter("CommandType", types);
					}
					Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception1, null);
					if (pSObjects == null || pSObjects.Count <= 1)
					{
						completionResults = CompletionCompleters.MakeCommandsUnique(pSObjects, true, flag, str, context);
					}
					else
					{
						Collection<PSObject> pSObjects1 = pSObjects;
						IOrderedEnumerable<PSObject> pSObjects2 = pSObjects1.OrderBy<PSObject, PSObject>((PSObject a) => a, new CompletionCompleters.CommandNameComparer());
						completionResults = CompletionCompleters.MakeCommandsUnique(pSObjects2, true, flag, str, context);
					}
				}
			}
			else
			{
				Ast parent = null;
				if (context.RelatedAsts != null && context.RelatedAsts.Count > 0)
				{
					parent = context.RelatedAsts.Last<Ast>();
				}
				PowerShell powerShell = context.Helper.CurrentPowerShell;
				CompletionCompleters.AddCommandWithPreferenceSetting(powerShell, "Get-Command").AddParameter("All").AddParameter("Name", wordToComplete);
				if (moduleName != null)
				{
					powerShell.AddParameter("Module", moduleName);
				}
				if (!types.Equals(CommandTypes.All))
				{
					powerShell.AddParameter("CommandType", types);
				}
				Collection<PSObject> pSObjects3 = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (parent != null)
				{
					pSObjects3 = CompletionCompleters.CompleteWorkflowCommand(wordToComplete, parent, pSObjects3);
				}
				if (pSObjects3 == null || pSObjects3.Count <= 1)
				{
					completionResults = CompletionCompleters.MakeCommandsUnique(pSObjects3, false, flag, str, context);
				}
				else
				{
					Collection<PSObject> pSObjects4 = pSObjects3;
					IOrderedEnumerable<PSObject> pSObjects5 = pSObjects4.OrderBy<PSObject, PSObject>((PSObject a) => a, new CompletionCompleters.CommandNameComparer());
					completionResults = CompletionCompleters.MakeCommandsUnique(pSObjects5, false, flag, str, context);
				}
				if (parent != null)
				{
					CompletionCompleters.FindFunctionsVisitor findFunctionsVisitor = new CompletionCompleters.FindFunctionsVisitor();
					while (parent.Parent != null)
					{
						parent = parent.Parent;
					}
					parent.Visit(findFunctionsVisitor);
					WildcardPattern wildcardPattern = new WildcardPattern(wordToComplete, WildcardOptions.IgnoreCase);
					foreach (FunctionDefinitionAst functionDefinition in findFunctionsVisitor.FunctionDefinitions)
					{
						Func<CompletionResult, bool> func = null;
						if (!wildcardPattern.IsMatch(functionDefinition.Name))
						{
							continue;
						}
						List<CompletionResult> completionResults1 = completionResults;
						if (func == null)
						{
							func = (CompletionResult cr) => cr.CompletionText.Equals(functionDefinition.Name, StringComparison.OrdinalIgnoreCase);
						}
						if (completionResults1.Where<CompletionResult>(func).Any<CompletionResult>())
						{
							continue;
						}
						completionResults.Insert(0, CompletionCompleters.GetCommandNameCompletionResult(functionDefinition.Name, functionDefinition, flag, str));
					}
				}
			}
			return completionResults;
		}

		internal static List<CompletionResult> CompleteCommandArgument(CompletionContext context)
		{
			CompletionCompleters.ArgumentLocation argumentLocation;
			CommandElementAst commandElementAst = null;
			bool flag;
			int num = 0;
			int num1 = 0;
			string commandName;
			bool flag1;
			List<CompletionResult> completionResults;
			CompletionContext completionContext;
			List<CompletionResult> completionResults1;
			char[] chrArray;
			string[] fileName;
			object[] wordToComplete;
			string value;
			ExpressionAst expressionAst;
			CommandAst parent = null;
			List<CompletionResult> argumentCompletionResultsWithFailedPseudoBinding = new List<CompletionResult>();
			MemberExpressionAst memberExpressionAst = null;
			Ast ast = context.RelatedAsts.Last<Ast>();
			ExpressionAst argument = ast as ExpressionAst;
			if (argument == null)
			{
				CommandParameterAst commandParameterAst = ast as CommandParameterAst;
				if (commandParameterAst == null)
				{
					parent = ast as CommandAst;
				}
				else
				{
					parent = commandParameterAst.Parent as CommandAst;
				}
			}
			else
			{
				if (argument.Parent as CommandAst == null)
				{
					if (argument.Parent as ArrayLiteralAst == null || argument.Parent.Parent as CommandAst == null)
					{
						if (argument.Parent as ArrayLiteralAst == null || argument.Parent.Parent as CommandParameterAst == null)
						{
							if (argument.Parent as CommandParameterAst != null && argument.Parent.Parent as CommandAst != null)
							{
								parent = (CommandAst)argument.Parent.Parent;
								if (argument as ErrorExpressionAst == null || !argument.Extent.Text.EndsWith(",", StringComparison.Ordinal))
								{
									if (context.WordToComplete == string.Empty)
									{
										argument = null;
									}
								}
								else
								{
									context.WordToComplete = string.Empty;
								}
							}
						}
						else
						{
							parent = (CommandAst)argument.Parent.Parent.Parent;
							if (context.WordToComplete != string.Empty)
							{
								argument = (ExpressionAst)argument.Parent;
							}
							else
							{
								argument = null;
							}
						}
					}
					else
					{
						parent = (CommandAst)argument.Parent.Parent;
						if (parent.CommandElements.Count == 1 || context.WordToComplete == string.Empty)
						{
							argument = null;
						}
						else
						{
							argument = (ExpressionAst)argument.Parent;
						}
					}
				}
				else
				{
					parent = (CommandAst)argument.Parent;
					if (argument as ErrorExpressionAst == null || !argument.Extent.Text.EndsWith(",", StringComparison.Ordinal))
					{
						if (parent.CommandElements.Count == 1 || context.WordToComplete == string.Empty)
						{
							argument = null;
						}
						else
						{
							if (parent.CommandElements.Count > 2)
							{
								int count = parent.CommandElements.Count;
								int num2 = 1;
								while (num2 < count && parent.CommandElements[num2] != argument)
								{
									num2++;
								}
								CommandElementAst item = null;
								if (num2 > 1)
								{
									item = parent.CommandElements[num2 - 1];
									memberExpressionAst = item as MemberExpressionAst;
								}
								StringConstantExpressionAst stringConstantExpressionAst = argument as StringConstantExpressionAst;
								if (stringConstantExpressionAst != null && item != null && stringConstantExpressionAst.StringConstantType == StringConstantType.BareWord && item.Extent.EndLineNumber == stringConstantExpressionAst.Extent.StartLineNumber && item.Extent.EndColumnNumber == stringConstantExpressionAst.Extent.StartColumnNumber)
								{
									chrArray = new char[2];
									chrArray[0] = '/';
									chrArray[1] = '\\';
									if (stringConstantExpressionAst.Value.IndexOfAny(chrArray) == 0)
									{
										StringConstantExpressionAst stringConstantExpressionAst1 = item as StringConstantExpressionAst;
										ExpandableStringExpressionAst expandableStringExpressionAst = item as ExpandableStringExpressionAst;
										ArrayLiteralAst arrayLiteralAst = item as ArrayLiteralAst;
										CommandParameterAst commandParameterAst1 = item as CommandParameterAst;
										if (stringConstantExpressionAst1 != null || expandableStringExpressionAst != null)
										{
											string str = CompletionCompleters.ConcatenateStringPathArguments(item, stringConstantExpressionAst.Value, context);
											if (stringConstantExpressionAst1 != null)
											{
												expressionAst = stringConstantExpressionAst1;
											}
											else
											{
												expressionAst = expandableStringExpressionAst;
											}
											argument = expressionAst;
											context.ReplacementIndex = ((InternalScriptPosition)item.Extent.StartScriptPosition).Offset;
											CompletionContext replacementLength = context;
											replacementLength.ReplacementLength = replacementLength.ReplacementLength + ((InternalScriptPosition)item.Extent.EndScriptPosition).Offset - context.ReplacementIndex;
											context.WordToComplete = str;
										}
										else
										{
											if (arrayLiteralAst == null)
											{
												if (commandParameterAst1 != null)
												{
													string str1 = CompletionCompleters.ConcatenateStringPathArguments(commandParameterAst1.Argument, stringConstantExpressionAst.Value, context);
													if (str1 == null)
													{
														ArrayLiteralAst argument1 = commandParameterAst1.Argument as ArrayLiteralAst;
														if (argument1 != null)
														{
															ExpressionAst expressionAst1 = argument1.Elements.LastOrDefault<ExpressionAst>();
															str1 = CompletionCompleters.ConcatenateStringPathArguments(expressionAst1, stringConstantExpressionAst.Value, context);
															if (str1 != null)
															{
																argument = argument1;
																context.ReplacementIndex = ((InternalScriptPosition)expressionAst1.Extent.StartScriptPosition).Offset;
																CompletionContext replacementLength1 = context;
																replacementLength1.ReplacementLength = replacementLength1.ReplacementLength + ((InternalScriptPosition)expressionAst1.Extent.EndScriptPosition).Offset - context.ReplacementIndex;
																context.WordToComplete = str1;
															}
														}
													}
													else
													{
														argument = commandParameterAst1.Argument;
														context.ReplacementIndex = ((InternalScriptPosition)commandParameterAst1.Argument.Extent.StartScriptPosition).Offset;
														CompletionContext completionContext1 = context;
														completionContext1.ReplacementLength = completionContext1.ReplacementLength + ((InternalScriptPosition)commandParameterAst1.Argument.Extent.EndScriptPosition).Offset - context.ReplacementIndex;
														context.WordToComplete = str1;
													}
												}
											}
											else
											{
												ExpressionAst expressionAst2 = arrayLiteralAst.Elements.LastOrDefault<ExpressionAst>();
												string str2 = CompletionCompleters.ConcatenateStringPathArguments(expressionAst2, stringConstantExpressionAst.Value, context);
												if (str2 != null)
												{
													argument = arrayLiteralAst;
													context.ReplacementIndex = ((InternalScriptPosition)expressionAst2.Extent.StartScriptPosition).Offset;
													CompletionContext replacementLength2 = context;
													replacementLength2.ReplacementLength = replacementLength2.ReplacementLength + ((InternalScriptPosition)expressionAst2.Extent.EndScriptPosition).Offset - context.ReplacementIndex;
													context.WordToComplete = str2;
												}
											}
										}
									}
								}
							}
						}
					}
					else
					{
						context.WordToComplete = string.Empty;
					}
				}
			}
			if (parent != null)
			{
				PseudoBindingInfo pseudoBindingInfo = (new PseudoParameterBinder()).DoPseudoParameterBinding(parent, null, null, true);
				if (pseudoBindingInfo != null)
				{
					if (pseudoBindingInfo.AllParsedArguments == null || pseudoBindingInfo.AllParsedArguments.Count <= 0)
					{
						int num3 = 0;
						if (argument == null)
						{
							Token tokenAtCursor = context.TokenAtCursor;
							Token tokenBeforeCursor = tokenAtCursor;
							if (tokenAtCursor == null)
							{
								tokenBeforeCursor = context.TokenBeforeCursor;
							}
							Token token = tokenBeforeCursor;
							foreach (CommandElementAst commandElement in parent.CommandElements)
							{
								if (commandElement.Extent.StartOffset > token.Extent.EndOffset)
								{
									break;
								}
								commandElementAst = commandElement;
								num3++;
							}
						}
						else
						{
							foreach (CommandElementAst commandElement1 in parent.CommandElements)
							{
								if (commandElement1.GetHashCode() == argument.GetHashCode())
								{
									break;
								}
								commandElementAst = commandElement1;
								num3++;
							}
						}
						if (num3 != 1)
						{
							if (commandElementAst as CommandParameterAst != null && ((CommandParameterAst)commandElementAst).Argument == null)
							{
								string parameterName = ((CommandParameterAst)commandElementAst).ParameterName;
								WildcardPattern wildcardPattern = new WildcardPattern(string.Concat(parameterName, "*"), WildcardOptions.IgnoreCase);
								List<MergedCompiledCommandParameter>.Enumerator enumerator = pseudoBindingInfo.UnboundParameters.GetEnumerator();
								try
								{
									do
									{
										if (!enumerator.MoveNext())
										{
											break;
										}
										MergedCompiledCommandParameter current = enumerator.Current;
										if (!wildcardPattern.IsMatch(current.Parameter.Name))
										{
											flag = false;
											foreach (string alias in current.Parameter.Aliases)
											{
												if (!wildcardPattern.IsMatch(alias))
												{
													continue;
												}
												flag = true;
												CompletionCompleters.ProcessParameter(pseudoBindingInfo.CommandName, parent, context, argumentCompletionResultsWithFailedPseudoBinding, current, null);
												break;
											}
										}
										else
										{
											CompletionCompleters.ProcessParameter(pseudoBindingInfo.CommandName, parent, context, argumentCompletionResultsWithFailedPseudoBinding, current, null);
											break;
										}
									}
									while (!flag);
								}
								finally
								{
									enumerator.Dispose();
								}
							}
						}
						else
						{
							CompletionCompleters.CompletePositionalArgument(pseudoBindingInfo.CommandName, parent, context, argumentCompletionResultsWithFailedPseudoBinding, pseudoBindingInfo.UnboundParameters, pseudoBindingInfo.DefaultParameterSetFlag, 0, 0, null);
						}
					}
					else
					{
						bool flag2 = false;
						if (argument != null)
						{
							flag2 = true;
							StringConstantExpressionAst stringConstantExpressionAst2 = argument as StringConstantExpressionAst;
							if (stringConstantExpressionAst2 != null && stringConstantExpressionAst2.Value.Trim().Equals("-", StringComparison.OrdinalIgnoreCase))
							{
								flag2 = false;
							}
						}
						if (!flag2)
						{
							Collection<AstParameterArgumentPair> allParsedArguments = pseudoBindingInfo.AllParsedArguments;
							Token tokenAtCursor1 = context.TokenAtCursor;
							Token tokenBeforeCursor1 = tokenAtCursor1;
							if (tokenAtCursor1 == null)
							{
								tokenBeforeCursor1 = context.TokenBeforeCursor;
							}
							argumentLocation = CompletionCompleters.FindTargetArgumentLocation(allParsedArguments, tokenBeforeCursor1);
						}
						else
						{
							argumentLocation = CompletionCompleters.FindTargetArgumentLocation(pseudoBindingInfo.AllParsedArguments, argument);
						}
						context.PseudoBindingInfo = pseudoBindingInfo;
						PseudoBindingInfoType infoType = pseudoBindingInfo.InfoType;
						switch (infoType)
						{
							case PseudoBindingInfoType.PseudoBindingFail:
							{
								argumentCompletionResultsWithFailedPseudoBinding = CompletionCompleters.GetArgumentCompletionResultsWithFailedPseudoBinding(context, argumentLocation, parent);
								break;
							}
							case PseudoBindingInfoType.PseudoBindingSucceed:
							{
								argumentCompletionResultsWithFailedPseudoBinding = CompletionCompleters.GetArgumentCompletionResultsWithSuccessfulPseudoBinding(context, argumentLocation, parent);
								break;
							}
						}
					}
				}
				bool flag3 = false;
				if (argumentCompletionResultsWithFailedPseudoBinding.Count > 0 && argumentCompletionResultsWithFailedPseudoBinding[argumentCompletionResultsWithFailedPseudoBinding.Count - 1].Equals(CompletionResult.Null))
				{
					argumentCompletionResultsWithFailedPseudoBinding.RemoveAt(argumentCompletionResultsWithFailedPseudoBinding.Count - 1);
					flag3 = true;
					if (argumentCompletionResultsWithFailedPseudoBinding.Count > 0)
					{
						return argumentCompletionResultsWithFailedPseudoBinding;
					}
				}
				if (argument != null || flag3 || parent.CommandElements.Count != 1 || parent.InvocationOperator == TokenKind.Unknown || !(context.WordToComplete != string.Empty))
				{
					if (argument as StringConstantExpressionAst != null)
					{
						StringConstantExpressionAst stringConstantExpressionAst3 = (StringConstantExpressionAst)argument;
						Match match = Regex.Match(stringConstantExpressionAst3.Value, "^(\\[[\\w\\d\\.]+\\]::[\\w\\d\\*]*)$");
						if (match.Success)
						{
							string value1 = match.Groups[1].Value;
							Tuple<Ast, Token[], IScriptPosition> parsedInput = CommandCompletion.MapStringInputToParsedInput(value1, value1.Length);
							CompletionAnalysis completionAnalysi = new CompletionAnalysis(parsedInput.Item1, parsedInput.Item2, parsedInput.Item3, context.Options);
							List<CompletionResult> results = completionAnalysi.GetResults(context.Helper.CurrentPowerShell, out num, out num1);
							if (results != null && results.Count > 0)
							{
								string str3 = string.Concat(TokenKind.LParen.Text(), value1.Substring(0, num));
								foreach (CompletionResult result in results)
								{
									string str4 = string.Concat(str3, result.CompletionText);
									if (result.ResultType.Equals(CompletionResultType.Property))
									{
										str4 = string.Concat(str4, TokenKind.RParen.Text());
									}
									argumentCompletionResultsWithFailedPseudoBinding.Add(new CompletionResult(str4, result.ListItemText, result.ResultType, result.ToolTip));
								}
								return argumentCompletionResultsWithFailedPseudoBinding;
							}
						}
						if (stringConstantExpressionAst3.Value.IndexOf('*') != -1 && memberExpressionAst != null && memberExpressionAst.Extent.EndLineNumber == stringConstantExpressionAst3.Extent.StartLineNumber && memberExpressionAst.Extent.EndColumnNumber == stringConstantExpressionAst3.Extent.StartColumnNumber)
						{
							if (stringConstantExpressionAst3.Value.EndsWith("*", StringComparison.Ordinal))
							{
								value = stringConstantExpressionAst3.Value;
							}
							else
							{
								value = string.Concat(stringConstantExpressionAst3.Value, "*");
							}
							string str5 = value;
							ExpressionAst expression = memberExpressionAst.Expression;
							if (!CompletionCompleters.IsSplattedVariable(expression))
							{
								StringConstantExpressionAst member = memberExpressionAst.Member as StringConstantExpressionAst;
								if (member != null)
								{
									str5 = string.Concat(member.Value, str5);
								}
								CompletionCompleters.CompleteMemberHelper(false, str5, expression, context, argumentCompletionResultsWithFailedPseudoBinding);
								if (argumentCompletionResultsWithFailedPseudoBinding.Count > 0)
								{
									context.ReplacementIndex = ((InternalScriptPosition)memberExpressionAst.Expression.Extent.EndScriptPosition).Offset + 1;
									if (member != null)
									{
										CompletionContext completionContext2 = context;
										completionContext2.ReplacementLength = completionContext2.ReplacementLength + member.Value.Length;
									}
									return argumentCompletionResultsWithFailedPseudoBinding;
								}
							}
							else
							{
								return argumentCompletionResultsWithFailedPseudoBinding;
							}
						}
						string value2 = stringConstantExpressionAst3.Value;
						if (parent.InvocationOperator != TokenKind.Unknown)
						{
							chrArray = new char[2];
							chrArray[0] = '\\';
							chrArray[1] = '/';
							if (value2.IndexOfAny(chrArray) != 0 || parent.CommandElements.Count != 2 || parent.CommandElements[0] as StringConstantExpressionAst == null || parent.CommandElements[0].Extent.EndLineNumber != argument.Extent.StartLineNumber || parent.CommandElements[0].Extent.EndColumnNumber != argument.Extent.StartColumnNumber)
							{
								if (!flag3)
								{
									commandName = parent.GetCommandName();
									fileName = new string[3];
									fileName[0] = commandName;
									fileName[1] = Path.GetFileName(commandName);
									fileName[2] = Path.GetFileNameWithoutExtension(commandName);
									wordToComplete = new object[2];
									wordToComplete[0] = context.WordToComplete;
									wordToComplete[1] = parent;
									if (!CompletionCompleters.TryCustomArgumentCompletion("NativeArgumentCompleters", fileName, wordToComplete, context, argumentCompletionResultsWithFailedPseudoBinding))
									{
										flag1 = false;
										if (pseudoBindingInfo == null)
										{
											flag1 = CompletionCompleters.TurnOnLiteralPathOption(context);
										}
										try
										{
											argumentCompletionResultsWithFailedPseudoBinding = new List<CompletionResult>(CompletionCompleters.CompleteFilename(context));
										}
										finally
										{
											if (flag1)
											{
												context.Options.Remove("LiteralPaths");
											}
										}
										if (context.WordToComplete != string.Empty && context.WordToComplete.IndexOf('-') != -1)
										{
											completionContext = new CompletionContext();
											completionContext.WordToComplete = context.WordToComplete;
											completionContext.Helper = context.Helper;
											completionResults = CompletionCompleters.CompleteCommand(completionContext);
											if (completionResults != null)
											{
												argumentCompletionResultsWithFailedPseudoBinding.AddRange(completionResults);
											}
										}
									}
									else
									{
										argumentCompletionResultsWithFailedPseudoBinding.Remove(CompletionResult.Null);
										return argumentCompletionResultsWithFailedPseudoBinding;
									}
								}
								return argumentCompletionResultsWithFailedPseudoBinding;
							}
							if (pseudoBindingInfo == null)
							{
								StringConstantExpressionAst item1 = (StringConstantExpressionAst)parent.CommandElements[0];
								value2 = string.Concat(item1.Value, value2);
								context.ReplacementIndex = ((InternalScriptPosition)item1.Extent.StartScriptPosition).Offset;
								CompletionContext replacementLength3 = context;
								replacementLength3.ReplacementLength = replacementLength3.ReplacementLength + ((InternalScriptPosition)item1.Extent.EndScriptPosition).Offset - context.ReplacementIndex;
								context.WordToComplete = value2;
								bool flag4 = CompletionCompleters.TurnOnLiteralPathOption(context);
								try
								{
									completionResults1 = new List<CompletionResult>(CompletionCompleters.CompleteFilename(context));
									return completionResults1;
								}
								finally
								{
									if (flag4)
									{
										context.Options.Remove("LiteralPaths");
									}
								}
							}
							else
							{
								return argumentCompletionResultsWithFailedPseudoBinding;
							}
						}
					}
					if (!flag3)
					{
						commandName = parent.GetCommandName();
						fileName = new string[3];
						fileName[0] = commandName;
						fileName[1] = Path.GetFileName(commandName);
						fileName[2] = Path.GetFileNameWithoutExtension(commandName);
						wordToComplete = new object[2];
						wordToComplete[0] = context.WordToComplete;
						wordToComplete[1] = parent;
						if (!CompletionCompleters.TryCustomArgumentCompletion("NativeArgumentCompleters", fileName, wordToComplete, context, argumentCompletionResultsWithFailedPseudoBinding))
						{
							flag1 = false;
							if (pseudoBindingInfo == null)
							{
								flag1 = CompletionCompleters.TurnOnLiteralPathOption(context);
							}
							try
							{
								argumentCompletionResultsWithFailedPseudoBinding = new List<CompletionResult>(CompletionCompleters.CompleteFilename(context));
							}
							finally
							{
								if (flag1)
								{
									context.Options.Remove("LiteralPaths");
								}
							}
							if (context.WordToComplete != string.Empty && context.WordToComplete.IndexOf('-') != -1)
							{
								completionContext = new CompletionContext();
								completionContext.WordToComplete = context.WordToComplete;
								completionContext.Helper = context.Helper;
								completionResults = CompletionCompleters.CompleteCommand(completionContext);
								if (completionResults != null)
								{
									argumentCompletionResultsWithFailedPseudoBinding.AddRange(completionResults);
								}
							}
						}
						else
						{
							argumentCompletionResultsWithFailedPseudoBinding.Remove(CompletionResult.Null);
							return argumentCompletionResultsWithFailedPseudoBinding;
						}
					}
					return argumentCompletionResultsWithFailedPseudoBinding;
				}
				else
				{
					bool flag5 = false;
					bool flag6 = CompletionCompleters.TurnOnLiteralPathOption(context);
					if (context.WordToComplete.IndexOf('-') != -1)
					{
						flag5 = true;
					}
					try
					{
						List<CompletionResult> completionResults2 = new List<CompletionResult>(CompletionCompleters.CompleteFilename(context));
						if (flag5)
						{
							List<CompletionResult> completionResults3 = CompletionCompleters.CompleteCommand(context);
							if (completionResults3 != null && completionResults3.Count > 0)
							{
								completionResults2.AddRange(completionResults3);
							}
						}
						completionResults1 = completionResults2;
					}
					finally
					{
						if (flag6)
						{
							context.Options.Remove("LiteralPaths");
						}
					}
				}
				return completionResults1;
			}
			else
			{
				return argumentCompletionResultsWithFailedPseudoBinding;
			}
		}

		internal static List<CompletionResult> CompleteCommandParameter(CompletionContext context)
		{
            CommandParameterAst item = null;
			List<CompletionResult> completionResults;
			string empty = null;
			bool flag = false;
			CommandAst parent = null;
			List<CompletionResult> parameterCompletionResults = new List<CompletionResult>();
			for (int i = context.RelatedAsts.Count - 1; i >= 0; i--)
			{
				item = context.RelatedAsts[i] as CommandParameterAst;
				if (item != null)
				{
					break;
				}
			}
			if (item == null)
			{
				StringConstantExpressionAst stringConstantExpressionAst = context.RelatedAsts[context.RelatedAsts.Count - 1] as StringConstantExpressionAst;
				if (stringConstantExpressionAst != null)
				{
					if (stringConstantExpressionAst.Value.Trim().Equals("-", StringComparison.OrdinalIgnoreCase))
					{
						parent = (CommandAst)stringConstantExpressionAst.Parent;
						empty = string.Empty;
					}
					else
					{
						return parameterCompletionResults;
					}
				}
				else
				{
					return parameterCompletionResults;
				}
			}
			else
			{
				parent = (CommandAst)item.Parent;
				empty = item.ParameterName;
				flag = context.WordToComplete.EndsWith(":", StringComparison.Ordinal);
			}
			PseudoBindingInfo pseudoBindingInfo = (new PseudoParameterBinder()).DoPseudoParameterBinding(parent, null, item, false);
			if (pseudoBindingInfo != null)
			{
				PseudoBindingInfoType infoType = pseudoBindingInfo.InfoType;
				switch (infoType)
				{
					case PseudoBindingInfoType.PseudoBindingFail:
					{
						parameterCompletionResults = CompletionCompleters.GetParameterCompletionResults(empty, 0, pseudoBindingInfo.UnboundParameters, flag);
						break;
					}
					case PseudoBindingInfoType.PseudoBindingSucceed:
					{
						parameterCompletionResults = CompletionCompleters.GetParameterCompletionResults(empty, pseudoBindingInfo, item, flag);
						break;
					}
				}
				if (parameterCompletionResults.Count == 0)
				{
					if (pseudoBindingInfo.CommandName.Equals("Set-Location", StringComparison.OrdinalIgnoreCase))
					{
						completionResults = new List<CompletionResult>(CompletionCompleters.CompleteFilename(context, true, null));
					}
					else
					{
						completionResults = new List<CompletionResult>(CompletionCompleters.CompleteFilename(context));
					}
					parameterCompletionResults = completionResults;
				}
				return parameterCompletionResults;
			}
			else
			{
				return parameterCompletionResults;
			}
		}

		internal static List<CompletionResult> CompleteComment(CompletionContext context)
		{
			Collection<PSObject> pSObjects;
			Exception exception = null;
			int num = 0;
			List<CompletionResult> completionResults = new List<CompletionResult>();
			Match match = Regex.Match(context.WordToComplete, "^#([\\w\\-]*)$");
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!Regex.IsMatch(value, "^[0-9]+$") || !LanguagePrimitives.TryConvertTo<int>(value, out num))
				{
					value = string.Concat("*", value, "*");
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-History");
					pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
					WildcardPattern wildcardPattern = new WildcardPattern(value, WildcardOptions.IgnoreCase);
					if (pSObjects != null)
					{
						for (int i = pSObjects.Count - 1; i >= 0; i--)
						{
							PSObject item = pSObjects[i];
							HistoryInfo historyInfo = PSObject.Base(item) as HistoryInfo;
							if (historyInfo != null)
							{
								string commandLine = historyInfo.CommandLine;
								if (!string.IsNullOrEmpty(commandLine) && wildcardPattern.IsMatch(commandLine))
								{
									completionResults.Add(new CompletionResult(commandLine, commandLine, CompletionResultType.History, commandLine));
								}
							}
						}
					}
					return completionResults;
				}
				else
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-History").AddParameter("Id", num);
					pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
					if (pSObjects != null && pSObjects.Count == 1)
					{
						HistoryInfo historyInfo1 = PSObject.Base(pSObjects[0]) as HistoryInfo;
						if (historyInfo1 != null)
						{
							string str = historyInfo1.CommandLine;
							if (!string.IsNullOrEmpty(str))
							{
								completionResults.Add(new CompletionResult(str, str, CompletionResultType.History, str));
							}
						}
					}
					return completionResults;
				}
			}
			else
			{
				return completionResults;
			}
		}

		public static IEnumerable<CompletionResult> CompleteFilename(string fileName)
		{
			Runspace defaultRunspace = Runspace.DefaultRunspace;
			if (defaultRunspace != null)
			{
				CompletionExecutionHelper completionExecutionHelper = new CompletionExecutionHelper(PowerShell.Create(RunspaceMode.CurrentRunspace));
				CompletionContext completionContext = new CompletionContext();
				completionContext.WordToComplete = fileName;
				completionContext.Helper = completionExecutionHelper;
				return CompletionCompleters.CompleteFilename(completionContext);
			}
			else
			{
				return CommandCompletion.EmptyCompletionResult;
			}
		}

		internal static IEnumerable<CompletionResult> CompleteFilename(CompletionContext context)
		{
			return CompletionCompleters.CompleteFilename(context, false, null);
		}

		private static IEnumerable<CompletionResult> CompleteFilename(CompletionContext context, bool containerOnly, HashSet<string> extension)
		{
			string str = null;
			Exception exception = null;
			ProviderInfo provider = null;
            string str1 = null;
            string str2 = null;
			char[] chrArray;
			bool flag;
			bool flag1;
			dynamic obj;
			CompletionResultType completionResultType;
			CompletionResultType completionResultType1;
			string str3;
			string str4;
			dynamic obj1;
			string str5;
			bool flag2;
			string wordToComplete = context.WordToComplete;
			string str6 = CompletionCompleters.HandleDoubleAndSingleQuote(ref wordToComplete);
			List<CompletionResult> completionResults = new List<CompletionResult>();
			Match match = Regex.Match(wordToComplete, "^\\\\\\\\([^\\\\]+)\\\\([^\\\\]*)$");
			if (!match.Success)
			{
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				ExecutionContext contextFromTLS = currentPowerShell.GetContextFromTLS();
				if (string.IsNullOrWhiteSpace(wordToComplete))
				{
					flag = true;
				}
				else
				{
					chrArray = new char[2];
					chrArray[0] = '\\';
					chrArray[1] = '/';
					if (wordToComplete.IndexOfAny(chrArray) == 0 || Regex.Match(wordToComplete, "^~[\\\\/]+.*").Success)
					{
						flag = false;
					}
					else
					{
						flag = !contextFromTLS.LocationGlobber.IsAbsolutePath(wordToComplete, out str);
					}
				}
				bool flag3 = flag;
				bool option = context.GetOption("RelativePaths", flag3);
				bool option1 = context.GetOption("LiteralPaths", false);
				if (option1 && LocationGlobber.StringContainsGlobCharacters(wordToComplete))
				{
					chrArray = new char[2];
					chrArray[0] = '*';
					chrArray[1] = '?';
					wordToComplete = WildcardPattern.Escape(wordToComplete, chrArray);
				}
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Resolve-Path").AddParameter("Path", string.Concat(wordToComplete, "*"));
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					bool flag4 = false;
					bool flag5 = CompletionCompleters.ProviderSpecified(wordToComplete);
					if (pSObjects.Count <= 0)
					{
						try
						{
							if (!flag3)
							{
								contextFromTLS.LocationGlobber.GetProviderPath(wordToComplete, out provider);
							}
							else
							{
								provider = contextFromTLS.EngineSessionState.CurrentDrive.Provider;
							}
							if (provider == null)
							{
								flag1 = false;
							}
							else
							{
								flag1 = provider.Name.Equals("FileSystem", StringComparison.OrdinalIgnoreCase);
							}
							flag4 = flag1;
						}
						catch (Exception exception2)
						{
							Exception exception1 = exception2;
							CommandProcessorBase.CheckForSevereException(exception1);
						}
					}
					else
					{
						dynamic item = pSObjects[0];
						ProviderInfo providerInfo = item.Provider as ProviderInfo;
						if (providerInfo == null)
						{
							flag2 = false;
						}
						else
						{
							flag2 = providerInfo.Name.Equals("FileSystem", StringComparison.OrdinalIgnoreCase);
						}
						flag4 = flag2;
					}
					if (flag4)
					{
						bool flag6 = false;
						if (pSObjects.Count > 0 && !LocationGlobber.StringContainsGlobCharacters(wordToComplete))
						{
							string fileName = null;
							if (flag5)
							{
								str5 = wordToComplete.Substring(wordToComplete.IndexOf(':') + 2);
							}
							else
							{
								str5 = wordToComplete;
							}
							string str7 = str5;
							try
							{
								fileName = Path.GetFileName(str7);
							}
							catch (Exception exception4)
							{
								Exception exception3 = exception4;
								CommandProcessorBase.CheckForSevereException(exception3);
							}
							HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
							foreach (dynamic obj2 in pSObjects)
							{
								str1 = (string)obj2.ProviderPath;
								if (!string.IsNullOrEmpty(str1))
								{
									if (strs.Contains(str1))
									{
										continue;
									}
									strs.Add(str1);
								}
								else
								{
									fileName = null;
									break;
								}
							}
							if (fileName != null)
							{
								fileName = string.Concat(fileName, "*");
								string directoryName = Path.GetDirectoryName(str1);
								if (!string.IsNullOrEmpty(directoryName))
								{
									string[] fileSystemEntries = null;
									try
									{
										fileSystemEntries = Directory.GetFileSystemEntries(directoryName, fileName);
									}
									catch (Exception exception6)
									{
										Exception exception5 = exception6;
										CommandProcessorBase.CheckForSevereException(exception5);
									}
									if (fileSystemEntries != null)
									{
										flag6 = true;
										if ((int)fileSystemEntries.Length > strs.Count)
										{
											string[] strArrays = fileSystemEntries;
											for (int i = 0; i < (int)strArrays.Length; i++)
											{
												string str8 = strArrays[i];
												if (!strs.Contains(str8))
												{
													FileInfo fileInfo = new FileInfo(str8);
													if ((fileInfo.Attributes & FileAttributes.Hidden) != 0)
													{
														PSObject pSObject = PSObject.AsPSObject(str8);
														pSObjects.Add(pSObject);
													}
												}
											}
										}
									}
								}
							}
						}
						if (!flag6)
						{
							CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-ChildItem").AddParameter("Path", string.Concat(wordToComplete, "*")).AddParameter("Hidden", true);
							Collection<PSObject> pSObjects1 = context.Helper.ExecuteCurrentPowerShell(out exception, null);
							if (pSObjects1 != null && pSObjects1.Count > 0)
							{
								foreach (PSObject pSObject1 in pSObjects1)
								{
									pSObjects.Add(pSObject1);
								}
							}
						}
					}
					Collection<PSObject> pSObjects2 = pSObjects;
					IOrderedEnumerable<PSObject> pSObjects3 = pSObjects2.OrderBy<PSObject, PSObject>((PSObject a) => a, new CompletionCompleters.ItemPathComparer());
					foreach (PSObject pSObject2 in pSObjects3)
					{
						object obj3 = PSObject.Base(pSObject2);
						string path = null;
						string providerPath = null;
						PathInfo pathInfo = obj3 as PathInfo;
						if (pathInfo == null)
						{
							if (obj3 as FileSystemInfo == null)
							{
								string str9 = obj3 as string;
								if (str9 != null)
								{
									providerPath = str9;
									if (flag5)
									{
										str4 = string.Concat("FileSystem::", str9);
									}
									else
									{
										str4 = providerPath;
									}
									path = str4;
								}
							}
							else
							{
								dynamic obj4 = pSObject2;
								providerPath = (string)obj4.FullName;
								if (flag5)
								{
									obj1 = obj4.PSPath;
								}
								else
								{
									obj1 = providerPath;
								}
								path = (string)obj1;
							}
						}
						else
						{
							path = pathInfo.Path;
							providerPath = pathInfo.ProviderPath;
						}
						if (path == null || flag4 && providerPath == null)
						{
							continue;
						}
						if (!option)
						{
							str2 = path;
						}
						else
						{
							try
							{
								SessionStateInternal engineSessionState = contextFromTLS.EngineSessionState;
								str2 = engineSessionState.NormalizeRelativePath(path, engineSessionState.CurrentLocation.ProviderPath);
								if (!str2.StartsWith("..\\", StringComparison.Ordinal))
								{
									str2 = string.Concat(".\\", str2);
								}
							}
							catch (Exception exception8)
							{
								Exception exception7 = exception8;
								CommandProcessorBase.CheckForSevereException(exception7);
								continue;
							}
						}
						if (CompletionCompleters.ProviderSpecified(str2) && !flag5)
						{
							int num = str2.IndexOf(':');
							str2 = str2.Substring(num + 2);
						}
						if (!CompletionCompleters.CompletionRequiresQuotes(str2, !option1))
						{
							if (str6 != string.Empty)
							{
								str2 = string.Concat(str6, str2, str6);
							}
						}
						else
						{
							if (str6 == string.Empty)
							{
								str3 = "'";
							}
							else
							{
								str3 = str6;
							}
							string str10 = str3;
							if (str10 != "'")
							{
								str2 = str2.Replace("`", "``");
								str2 = str2.Replace("$", "`$");
							}
							else
							{
								str2 = str2.Replace("'", "''");
							}
							if (!option1)
							{
								if (str10 != "'")
								{
									str2 = str2.Replace("[", "``[");
									str2 = str2.Replace("]", "``]");
								}
								else
								{
									str2 = str2.Replace("[", "`[");
									str2 = str2.Replace("]", "`]");
								}
							}
							str2 = string.Concat(str10, str2, str10);
						}
						if (!flag4)
						{
							CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Item").AddParameter("LiteralPath", path);
							Collection<PSObject> pSObjects4 = context.Helper.ExecuteCurrentPowerShell(out exception, null);
							if (pSObjects4 == null || pSObjects4.Count != 1)
							{
								completionResults.Add(new CompletionResult(str2));
							}
							else
							{
								dynamic item1 = pSObjects4[0];
                                dynamic obj5 = LanguagePrimitives.ConvertTo<bool>(item1.PSIsContainer);
								bool flag7 = containerOnly;
								if (!flag7)
								{
									obj = flag7;
								}
								else
								{
									bool flag8 = flag7;
									obj = flag8 & (dynamic)(!obj5);
								}
								if (obj)
								{
									continue;
								}
								PowerShell powerShell = CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Convert-Path");
								string str11 = "LiteralPath";
								powerShell.AddParameter(str11, item1.PSPath);
								Collection<PSObject> pSObjects5 = context.Helper.ExecuteCurrentPowerShell(out exception, null);
								string str12 = null;
								string str13 = (string)item1.PSChildName;
								if (pSObjects5 != null && pSObjects5.Count == 1)
								{
									str12 = PSObject.Base(pSObjects5[0]) as string;
								}
								if (string.IsNullOrEmpty(str13))
								{
									str13 = (string)item1.Name;
								}
								List<CompletionResult> completionResults1 = completionResults;
								string str14 = str2;
								string str15 = str13;
								if (obj5)
								{
									completionResultType = CompletionResultType.ProviderContainer;
								}
								else
								{
									completionResultType = CompletionResultType.ProviderItem;
								}
								string str16 = str12;
								string str17 = str16;
								if (str16 == null)
								{
									str17 = path;
								}
								completionResults1.Add(new CompletionResult(str14, str15, completionResultType, str17));
							}
						}
						else
						{
							bool flag9 = Directory.Exists(providerPath);
							if (containerOnly && !flag9 || !containerOnly && !flag9 && !CompletionCompleters.CheckFileExtension(providerPath, extension))
							{
								continue;
							}
							string str18 = providerPath;
							string fileName1 = Path.GetFileName(providerPath);
							List<CompletionResult> completionResults2 = completionResults;
							string str19 = str2;
							string str20 = fileName1;
							if (flag9)
							{
								completionResultType1 = CompletionResultType.ProviderContainer;
							}
							else
							{
								completionResultType1 = CompletionResultType.ProviderItem;
							}
							completionResults2.Add(new CompletionResult(str19, str20, completionResultType1, str18));
						}
					}
				}
			}
			else
			{
				string value = match.Groups[1].Value;
				WildcardPattern wildcardPattern = new WildcardPattern(string.Concat(match.Groups[2].Value, "*"), WildcardOptions.IgnoreCase);
				bool option2 = context.GetOption("IgnoreHiddenShares", false);
				List<string> fileShares = CompletionCompleters.GetFileShares(value, option2);
				foreach (string fileShare in fileShares)
				{
					if (!wildcardPattern.IsMatch(fileShare))
					{
						continue;
					}
					string str21 = string.Concat("\\\\", value, "\\", fileShare);
					if (str6 != string.Empty)
					{
						str21 = string.Concat(str6, str21, str6);
					}
					completionResults.Add(new CompletionResult(str21, str21, CompletionResultType.ProviderContainer, str21));
				}
			}
			return completionResults;
		}

		internal static List<CompletionResult> CompleteHashtableKey(CompletionContext completionContext, HashtableAst hashtableAst)
		{
			string key = null;
			ConvertExpressionAst parent = hashtableAst.Parent as ConvertExpressionAst;
			if (parent == null)
			{
				Ast ast = hashtableAst.Parent;
				if (ast as ArrayLiteralAst != null)
				{
					ast = ast.Parent;
				}
				if (ast as CommandParameterAst != null)
				{
					ast = ast.Parent;
				}
				CommandAst commandAst = ast as CommandAst;
				if (commandAst != null)
				{
					PseudoBindingInfo pseudoBindingInfo = (new PseudoParameterBinder()).DoPseudoParameterBinding(commandAst, null, null, true);
					foreach (KeyValuePair<string, AstParameterArgumentPair> boundArgument in pseudoBindingInfo.BoundArguments)
					{
						AstPair value = boundArgument.Value as AstPair;
						if (value == null)
						{
							AstArrayPair astArrayPair = boundArgument.Value as AstArrayPair;
							if (astArrayPair == null || !astArrayPair.Argument.Contains<ExpressionAst>(hashtableAst))
							{
								continue;
							}
							key = boundArgument.Key;
							break;
						}
						else
						{
							if (value.Argument != hashtableAst)
							{
								continue;
							}
							key = boundArgument.Key;
							break;
						}
					}
					if (key != null)
					{
						if (!key.Equals("GroupBy", StringComparison.OrdinalIgnoreCase))
						{
							if (key.Equals("Property", StringComparison.OrdinalIgnoreCase))
							{
								string commandName = pseudoBindingInfo.CommandName;
								string str = commandName;
								if (commandName != null)
								{
									switch (str)
									{
										case "New-Object":
										{
											IEnumerable<PSTypeName> inferredType = commandAst.GetInferredType(completionContext);
											List<CompletionResult> completionResults = new List<CompletionResult>();
											CompletionCompleters.CompleteMemberByInferredType(completionContext, inferredType, completionResults, string.Concat(completionContext.WordToComplete, "*"), true);
											return completionResults;
										}
										case "Sort-Object":
										{
											string[] strArrays = new string[3];
											strArrays[0] = "Expression";
											strArrays[1] = "Ascending";
											strArrays[2] = "Descending";
											return CompletionCompleters.GetSpecialHashTableKeyMembers(strArrays);
										}
										case "Group-Object":
										{
											string[] strArrays1 = new string[1];
											strArrays1[0] = "Expression";
											return CompletionCompleters.GetSpecialHashTableKeyMembers(strArrays1);
										}
										case "Format-Table":
										{
											string[] strArrays2 = new string[5];
											strArrays2[0] = "Expression";
											strArrays2[1] = "FormatString";
											strArrays2[2] = "Label";
											strArrays2[3] = "Width";
											strArrays2[4] = "Alignment";
											return CompletionCompleters.GetSpecialHashTableKeyMembers(strArrays2);
										}
										case "Format-List":
										{
											string[] strArrays3 = new string[3];
											strArrays3[0] = "Expression";
											strArrays3[1] = "FormatString";
											strArrays3[2] = "Label";
											return CompletionCompleters.GetSpecialHashTableKeyMembers(strArrays3);
										}
										case "Format-Wide":
										{
											string[] strArrays4 = new string[2];
											strArrays4[0] = "Expression";
											strArrays4[1] = "FormatString";
											return CompletionCompleters.GetSpecialHashTableKeyMembers(strArrays4);
										}
										case "Format-Custom":
										{
											string[] strArrays5 = new string[2];
											strArrays5[0] = "Expression";
											strArrays5[1] = "Depth";
											return CompletionCompleters.GetSpecialHashTableKeyMembers(strArrays5);
										}
									}
								}
							}
						}
						else
						{
							string commandName1 = pseudoBindingInfo.CommandName;
							string str1 = commandName1;
							if (commandName1 == null || !(str1 == "Format-Table") && !(str1 == "Format-List") && !(str1 == "Format-Wide") && !(str1 == "Format-Custom"))
							{
								return null;
							}
							else
							{
								string[] strArrays6 = new string[3];
								strArrays6[0] = "Expression";
								strArrays6[1] = "FormatString";
								strArrays6[2] = "Label";
								return CompletionCompleters.GetSpecialHashTableKeyMembers(strArrays6);
							}
						}
					}
				}
				return null;
			}
			else
			{
				List<CompletionResult> completionResults1 = new List<CompletionResult>();
				CompletionCompleters.CompleteMemberByInferredType(completionContext, parent.GetInferredType(completionContext), completionResults1, string.Concat(completionContext.WordToComplete, "*"), true);
				return completionResults1;
			}
		}

		internal static List<CompletionResult> CompleteHelpTopics(CompletionContext context)
		{
			List<CompletionResult> completionResults = new List<CompletionResult>();
			string str = string.Concat(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID), "\\", Thread.CurrentThread.CurrentCulture.Name);
			string str1 = string.Concat(context.WordToComplete, "*");
			WildcardPattern wildcardPattern = new WildcardPattern("about_*.help.txt", WildcardOptions.IgnoreCase);
			string[] files = null;
			try
			{
				files = Directory.GetFiles(str, str1);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				CommandProcessorBase.CheckForSevereException(exception);
			}
			if (files != null)
			{
				string[] strArrays = files;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str2 = strArrays[i];
					if (str2 != null)
					{
						try
						{
							string fileName = Path.GetFileName(str2);
							if (fileName != null && wildcardPattern.IsMatch(fileName))
							{
								string str3 = fileName.Substring(0, fileName.Length - 9);
								completionResults.Add(new CompletionResult(str3));
							}
						}
						catch (Exception exception3)
						{
							Exception exception2 = exception3;
							CommandProcessorBase.CheckForSevereException(exception2);
						}
					}
				}
			}
			return completionResults;
		}

		internal static List<CompletionResult> CompleteMember(CompletionContext context, bool @static)
		{
			List<CompletionResult> completionResults = new List<CompletionResult>();
			Ast ast = context.RelatedAsts.Last<Ast>();
			MemberExpressionAst memberExpressionAst = ast as MemberExpressionAst;
			Ast member = null;
			ExpressionAst expression = null;
			if (memberExpressionAst == null)
			{
				member = ast;
			}
			else
			{
				if (context.TokenAtCursor.Extent.StartOffset >= memberExpressionAst.Member.Extent.StartOffset)
				{
					member = memberExpressionAst.Member;
				}
				expression = memberExpressionAst.Expression;
			}
			StringConstantExpressionAst stringConstantExpressionAst = member as StringConstantExpressionAst;
			string str = "*";
			if (stringConstantExpressionAst == null)
			{
				if (ast as ErrorExpressionAst == null && expression == null)
				{
					return completionResults;
				}
			}
			else
			{
				if (!stringConstantExpressionAst.Value.Equals(".", StringComparison.OrdinalIgnoreCase) && !stringConstantExpressionAst.Value.Equals("::", StringComparison.OrdinalIgnoreCase))
				{
					str = string.Concat(stringConstantExpressionAst.Value, "*");
				}
			}
			CommandAst parent = ast.Parent as CommandAst;
			if (parent == null)
			{
				if (ast.Parent as MemberExpressionAst == null)
				{
					if (ast.Parent as BinaryExpressionAst != null && context.TokenAtCursor.Kind.Equals(TokenKind.Multiply))
					{
						MemberExpressionAst left = ((BinaryExpressionAst)ast.Parent).Left as MemberExpressionAst;
						if (left != null)
						{
							expression = left.Expression;
							if (left.Member as StringConstantExpressionAst != null)
							{
								str = string.Concat(((StringConstantExpressionAst)left.Member).Value, "*");
							}
						}
					}
				}
				else
				{
					MemberExpressionAst parent1 = (MemberExpressionAst)ast.Parent;
					expression = parent1.Expression;
				}
			}
			else
			{
				int count = parent.CommandElements.Count - 1;
				while (count >= 0 && parent.CommandElements[count] != ast)
				{
					count--;
				}
				CommandElementAst item = parent.CommandElements[count - 1];
				IScriptExtent extent = item.Extent;
				IScriptExtent scriptExtent = ast.Extent;
				if (extent.EndLineNumber == scriptExtent.StartLineNumber && extent.EndColumnNumber == scriptExtent.StartColumnNumber)
				{
					expression = item as ExpressionAst;
				}
			}
			if (expression != null)
			{
				if (!CompletionCompleters.IsSplattedVariable(expression))
				{
					CompletionCompleters.CompleteMemberHelper(@static, str, expression, context, completionResults);
					if (completionResults.Count == 0 && !@static)
					{
						IEnumerable<PSTypeName> inferredType = expression.GetInferredType(context);
						CompletionCompleters.CompleteMemberByInferredType(context, inferredType, completionResults, str, false);
					}
					return completionResults;
				}
				else
				{
					return completionResults;
				}
			}
			else
			{
				return completionResults;
			}
		}

		private static void CompleteMemberByInferredType(CompletionContext context, IEnumerable<PSTypeName> inferredTypes, List<CompletionResult> results, string memberName, bool skipMethods)
		{
			IEnumerable<object> membersByInferredType = null;
			Exception exception = null;
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			WildcardPattern wildcardPattern = new WildcardPattern(memberName, WildcardOptions.IgnoreCase);
			foreach (PSTypeName pSTypeName in membersByInferredType)
			{
				if (strs.Contains(pSTypeName.Name))
				{
					continue;
				}
				strs.Add(pSTypeName.Name);
				membersByInferredType = CompletionCompleters.GetMembersByInferredType(pSTypeName, false, context);
				IEnumerator<object> enumerator = membersByInferredType.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						object obj = pSTypeName;
						CompletionCompleters.AddInferredMember(obj, wildcardPattern, results, skipMethods);
					}
				}
			}
			if (results.Count > 0)
			{
				string[] strArrays = new string[2];
				strArrays[0] = "ResultType";
				strArrays[1] = "ListItemText";
				CompletionCompleters.AddCommandWithPreferenceSetting(context.Helper.CurrentPowerShell, "Sort-Object").AddParameter("Property", strArrays).AddParameter("Unique");
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, results);
				results.Clear();
				List<CompletionResult> completionResults = results;
				Collection<PSObject> pSObjects1 = pSObjects;
				completionResults.AddRange(pSObjects1.Select<PSObject, CompletionResult>((PSObject psobj) => PSObject.Base(psobj) as CompletionResult));
			}
		}

		internal static void CompleteMemberHelper(bool @static, string memberName, ExpressionAst targetExpr, CompletionContext context, List<CompletionResult> results)
		{
			object obj = null;
			IEnumerable members;
			Exception exception = null;
			CompletionResultType completionResultType;
			bool isSpecial;
			if (SafeExprEvaluator.TrySafeEval(targetExpr, context.ExecutionContext, out obj) && obj != null)
			{
				if (targetExpr as ArrayExpressionAst != null && obj as object[] == null)
				{
					object[] objArray = new object[1];
					objArray[0] = obj;
					obj = objArray;
				}
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Where-Object").AddParameter("Property", "Name").AddParameter("Like").AddParameter("Value", memberName);
				object[] objArray1 = new object[2];
				objArray1[0] = "MemberType";
				objArray1[1] = "Name";
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Sort-Object").AddParameter("Property", objArray1);
				if (!@static)
				{
					members = PSObject.AsPSObject(obj).Members;
				}
				else
				{
					Type type = PSObject.Base(obj) as Type;
					if (type != null)
					{
						members = PSObject.dotNetStaticAdapter.BaseGetMembers<PSMemberInfo>(type);
					}
					else
					{
						return;
					}
				}
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, members);
				foreach (PSObject pSObject in pSObjects)
				{
					PSMemberInfo pSMemberInfo = (PSMemberInfo)PSObject.Base(pSObject);
					if (pSMemberInfo.IsHidden)
					{
						continue;
					}
					string name = pSMemberInfo.Name;
					if (CompletionCompleters.CompletionRequiresQuotes(name, false))
					{
						name = name.Replace("'", "''");
						name = string.Concat("'", name, "'");
					}
					bool flag = pSMemberInfo is PSMethodInfo;
					if (flag)
					{
						if (pSMemberInfo as PSMethod == null)
						{
							isSpecial = false;
						}
						else
						{
							isSpecial = ((PSMethod)pSMemberInfo).IsSpecial;
						}
						bool flag1 = isSpecial;
						if (flag1)
						{
							continue;
						}
						name = string.Concat(name, (char)40);
					}
					string str = pSMemberInfo.ToString();
					if (str.IndexOf("),", StringComparison.OrdinalIgnoreCase) != -1)
					{
						string[] strArrays = new string[1];
						strArrays[0] = "),";
						string[] strArrays1 = str.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
						StringBuilder stringBuilder = new StringBuilder();
						string[] strArrays2 = strArrays1;
						for (int i = 0; i < (int)strArrays2.Length; i++)
						{
							string str1 = strArrays2[i];
							stringBuilder.Append(string.Concat(str1.Trim(), ")\r\n"));
						}
						stringBuilder.Remove(stringBuilder.Length - 3, 3);
						str = stringBuilder.ToString();
					}
					List<CompletionResult> completionResults = results;
					string str2 = name;
					string name1 = pSMemberInfo.Name;
					if (flag)
					{
						completionResultType = CompletionResultType.Method;
					}
					else
					{
						completionResultType = CompletionResultType.Property;
					}
					completionResults.Add(new CompletionResult(str2, name1, completionResultType, str));
				}
				IDictionary dictionaries = PSObject.Base(obj) as IDictionary;
				if (dictionaries != null)
				{
					WildcardPattern wildcardPattern = new WildcardPattern(memberName, WildcardOptions.IgnoreCase);
					foreach (DictionaryEntry dictionaryEntry in dictionaries)
					{
						string key = dictionaryEntry.Key as string;
						if (key == null || !wildcardPattern.IsMatch(key))
						{
							continue;
						}
						if (CompletionCompleters.CompletionRequiresQuotes(key, false))
						{
							key = key.Replace("'", "''");
							key = string.Concat("'", key, "'");
						}
						results.Add(new CompletionResult(key, key, CompletionResultType.Property, key));
					}
				}
			}
		}

		private static List<CompletionResult> CompleteModuleName(CompletionContext context, bool loadedModulesOnly)
		{
			Exception exception = null;
			string str;
			string wordToComplete = context.WordToComplete;
			string empty = wordToComplete;
			if (wordToComplete == null)
			{
				empty = string.Empty;
			}
			string str1 = empty;
			List<CompletionResult> completionResults = new List<CompletionResult>();
			string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref str1);
			if (!str1.EndsWith("*", StringComparison.Ordinal))
			{
				str1 = string.Concat(str1, "*");
			}
			PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
			CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Module").AddParameter("Name", str1);
			if (!loadedModulesOnly)
			{
				currentPowerShell.AddParameter("ListAvailable", true);
			}
			Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
			if (pSObjects != null)
			{
				foreach (dynamic obj in pSObjects)
				{
					dynamic obj1 = obj.Name.ToString();
					dynamic obj2 = obj1;
					string str3 = "Description: ";
					dynamic obj3 = str3 + obj.Description.ToString() + "\r\nModuleType: ";
					dynamic obj4 = obj3 + obj.ModuleType.ToString() + "\r\nPath: ";
					dynamic obj5 = obj4 + obj.Path.ToString();
					if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
					{
						obj1 = str2 + obj1 + str2;
					}
					else
					{
						if (str2 == string.Empty)
						{
							str = "'";
						}
						else
						{
							str = str2;
						}
						string str4 = str;
						if (str4 == "'")
						{
							obj1 = obj1.Replace("'", "''");
						}
						obj1 = str4 + obj1 + str4;
					}
					List<CompletionResult> completionResults1 = completionResults;
                    completionResults1.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj5));
				}
			}
			return completionResults;
		}

		public static List<CompletionResult> CompleteOperator(string wordToComplete)
		{
			if (wordToComplete.StartsWith("-", StringComparison.Ordinal))
			{
				wordToComplete = wordToComplete.Substring(1);
			}
			IEnumerable<string> strs = Tokenizer._operatorText.Where<string>((string op) => op.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase));
			IOrderedEnumerable<string> strs1 = strs.OrderBy<string, string>((string op) => op);
			return strs1.Select<string, CompletionResult>((string op) => new CompletionResult(string.Concat("-", op), op, CompletionResultType.ParameterName, CompletionCompleters.GetOperatorDescription(op))).ToList<CompletionResult>();
		}

        private static void CompletePositionalArgument(string commandName, CommandAst commandAst, CompletionContext context, List<CompletionResult> result, IEnumerable<MergedCompiledCommandParameter> parameters, int defaultParameterSetFlag, int validParameterSetFlags, int position, Dictionary<string, AstParameterArgumentPair> boundArguments = null)
        {
            bool flag = false;
            bool flag2 = (defaultParameterSetFlag != 0) && ((defaultParameterSetFlag & validParameterSetFlags) != 0);
            MergedCompiledCommandParameter parameter = null;
            foreach (MergedCompiledCommandParameter parameter2 in parameters)
            {
                if (!(((parameter2.Parameter.ParameterSetFlags & validParameterSetFlags) != 0) || parameter2.Parameter.IsInAllSets))
                {
                    continue;
                }
                foreach (ParameterSetSpecificMetadata metadata in parameter2.Parameter.GetMatchingParameterSetData(validParameterSetFlags))
                {
                    if (!metadata.ValueFromRemainingArguments)
                    {
                        int num = metadata.Position;
                        if ((num != -2147483648) && (num == position))
                        {
                            if (flag2)
                            {
                                if (metadata.ParameterSetFlag == defaultParameterSetFlag)
                                {
                                    ProcessParameter(commandName, commandAst, context, result, parameter2, boundArguments);
                                    flag = result.Any<CompletionResult>();
                                    break;
                                }
                                if (parameter == null)
                                {
                                    parameter = parameter2;
                                }
                            }
                            else
                            {
                                flag = true;
                                ProcessParameter(commandName, commandAst, context, result, parameter2, boundArguments);
                                break;
                            }
                        }
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            if (!flag && (parameter != null))
            {
                flag = true;
                ProcessParameter(commandName, commandAst, context, result, parameter, boundArguments);
            }
            if (!flag)
            {
                foreach (MergedCompiledCommandParameter parameter3 in parameters)
                {
                    if (((parameter3.Parameter.ParameterSetFlags & validParameterSetFlags) != 0) || parameter3.Parameter.IsInAllSets)
                    {
                        foreach (ParameterSetSpecificMetadata metadata2 in parameter3.Parameter.GetMatchingParameterSetData(validParameterSetFlags))
                        {
                            if (metadata2.ValueFromRemainingArguments)
                            {
                                ProcessParameter(commandName, commandAst, context, result, parameter3, boundArguments);
                                break;
                            }
                        }
                    }
                }
            }
        }

		internal static List<CompletionResult> CompleteStatementFlags(TokenKind kind, string wordToComplete)
		{
			string str2;
			string str3;
			TokenKind tokenKind = kind;
			if (tokenKind != TokenKind.Switch)
			{
				return null;
			}
			else
			{
				wordToComplete = wordToComplete.Substring(1);
				bool flag = wordToComplete.EndsWith(":", StringComparison.Ordinal);
				if (flag)
				{
					str2 = wordToComplete.Remove(wordToComplete.Length - 1);
				}
				else
				{
					str2 = wordToComplete;
				}
				wordToComplete = str2;
				string str4 = LanguagePrimitives.EnumSingleTypeConverter.EnumValues(typeof(SwitchFlags));
				string listSeparator = ExtendedTypeSystem.ListSeparator;
				string[] strArrays = new string[1];
				strArrays[0] = listSeparator;
				string[] strArrays1 = str4.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
				WildcardPattern wildcardPattern = new WildcardPattern(string.Concat(wordToComplete, "*"), WildcardOptions.IgnoreCase);
				List<string> strs = new List<string>();
				List<CompletionResult> completionResults = new List<CompletionResult>();
				CompletionResult completionResult = null;
				string[] strArrays2 = strArrays1;
				for (int i = 0; i < (int)strArrays2.Length; i++)
				{
					string str5 = strArrays2[i];
					if (!str5.Equals(SwitchFlags.None.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						if (!wordToComplete.Equals(str5, StringComparison.OrdinalIgnoreCase))
						{
							if (wildcardPattern.IsMatch(str5))
							{
								strs.Add(str5);
							}
						}
						else
						{
							if (flag)
							{
								str3 = string.Concat("-", str5, ":");
							}
							else
							{
								str3 = string.Concat("-", str5);
							}
							string str6 = str3;
							completionResult = new CompletionResult(str6, str5, CompletionResultType.ParameterName, str5);
						}
					}
				}
				if (completionResult != null)
				{
					completionResults.Add(completionResult);
				}
				strs.Sort();
				List<CompletionResult> completionResults1 = completionResults;
				var collection = strs.Select((string entry) => {
					string str;
					string str1 = entry;
					if (flag)
					{
						str = string.Concat("-", entry, ":");
					}
					else
					{
						str = string.Concat("-", entry);
					}
					return new { entry = str1, completionText = str };
				}
				);
				completionResults1.AddRange(collection.Select((argument0) => new CompletionResult(argument0.completionText, argument0.entry, CompletionResultType.ParameterName, argument0.entry)));
				return completionResults;
			}
		}

		public static IEnumerable<CompletionResult> CompleteType(string typeName)
		{
			PowerShell powerShell;
			if (Runspace.DefaultRunspace == null)
			{
				powerShell = PowerShell.Create();
			}
			else
			{
				powerShell = PowerShell.Create(RunspaceMode.CurrentRunspace);
			}
			PowerShell powerShell1 = powerShell;
			CompletionExecutionHelper completionExecutionHelper = new CompletionExecutionHelper(powerShell1);
			CompletionContext completionContext = new CompletionContext();
			completionContext.WordToComplete = typeName;
			completionContext.Helper = completionExecutionHelper;
			return CompletionCompleters.CompleteType(completionContext, "", "");
		}

        internal static List<CompletionResult> CompleteType(CompletionContext context, string prefix = "", string suffix = "")
        {
            WildcardPattern pattern;
            TypeCompletionMapping[][] mappingArray = typeCache ?? InitializeTypeCache();
            List<CompletionResult> list = new List<CompletionResult>();
            string wordToComplete = context.WordToComplete;
            int index = (from c in wordToComplete
                         where c == '.'
                         select c).Count<char>();
            if ((index < mappingArray.Length) && (mappingArray[index] != null))
            {
                pattern = new WildcardPattern(wordToComplete + "*", WildcardOptions.IgnoreCase);
                foreach (TypeCompletionMapping mapping in from e in mappingArray[index]
                                                          where pattern.IsMatch(e.Key)
                                                          select e)
                {
                    foreach (TypeCompletionBase base2 in mapping.Completions)
                    {
                        list.Add(base2.GetCompletionResult(mapping.Key, prefix, suffix));
                    }
                }
                list.Sort((Comparison<CompletionResult>)((c1, c2) => string.Compare(c1.ListItemText, c2.ListItemText, StringComparison.OrdinalIgnoreCase)));
            }
            return list;
        }

		public static IEnumerable<CompletionResult> CompleteVariable(string variableName)
		{
			Runspace defaultRunspace = Runspace.DefaultRunspace;
			if (defaultRunspace != null)
			{
				CompletionExecutionHelper completionExecutionHelper = new CompletionExecutionHelper(PowerShell.Create(RunspaceMode.CurrentRunspace));
				CompletionContext completionContext = new CompletionContext();
				completionContext.WordToComplete = variableName;
				completionContext.Helper = completionExecutionHelper;
				return CompletionCompleters.CompleteVariable(completionContext);
			}
			else
			{
				return CommandCompletion.EmptyCompletionResult;
			}
		}

		internal static List<CompletionResult> CompleteVariable(CompletionContext context)
		{
			string str;
			string str1;
			Exception exception = null;
			string str2;
			string str3;
			string str4;
			string description;
			string str5;
			string str6;
			string str7;
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<CompletionResult> completionResults = new List<CompletionResult>();
			string wordToComplete = context.WordToComplete;
			int num = wordToComplete.IndexOf(':');
			string str8 = "$";
			Ast ast = context.RelatedAsts.Last<Ast>();
			VariableExpressionAst variableExpressionAst = ast as VariableExpressionAst;
			if (variableExpressionAst != null && variableExpressionAst.Splatted)
			{
				str8 = "@";
			}
			WildcardPattern wildcardPattern = new WildcardPattern(string.Concat(wordToComplete, "*"), WildcardOptions.IgnoreCase);
			if (ast != null)
			{
				Ast parent = ast.Parent;
				CompletionCompleters.FindVariablesVisitor findVariablesVisitor = new CompletionCompleters.FindVariablesVisitor();
				findVariablesVisitor.CompletionVariableAst = ast;
				CompletionCompleters.FindVariablesVisitor findVariablesVisitor1 = findVariablesVisitor;
				while (parent != null)
				{
					if (parent as IParameterMetadataProvider != null)
					{
						findVariablesVisitor1.Top = parent;
						parent.Visit(findVariablesVisitor1);
					}
					parent = parent.Parent;
				}
				foreach (VariableExpressionAst variable in findVariablesVisitor1.Variables)
				{
					string userPath = variable.VariablePath.UserPath;
					if (!wildcardPattern.IsMatch(userPath))
					{
						continue;
					}
					if (userPath.IndexOfAny(CompletionCompleters.CharactersRequiringQuotes) == -1)
					{
						str7 = string.Concat(str8, userPath);
					}
					else
					{
						str7 = string.Concat(str8, "{", userPath, "}");
					}
					string str9 = str7;
					string text = userPath;
					Ast parent1 = variable.Parent;
					while (parent1 != null)
					{
						ParameterAst parameterAst = parent1 as ParameterAst;
						if (parameterAst == null)
						{
							AssignmentStatementAst assignmentStatementAst = parent1.Parent as AssignmentStatementAst;
							if (assignmentStatementAst == null)
							{
								parent1 = parent1.Parent;
							}
							else
							{
								if (assignmentStatementAst.Left != parent1)
								{
									break;
								}
								text = parent1.Extent.Text;
								break;
							}
						}
						else
						{
							TypeConstraintAst typeConstraintAst = parameterAst.Attributes.OfType<TypeConstraintAst>().FirstOrDefault<TypeConstraintAst>();
							if (typeConstraintAst == null)
							{
								break;
							}
							text = StringUtil.Format("{0}${1}", typeConstraintAst.Extent.Text, userPath);
							break;
						}
					}
					CompletionCompleters.AddUniqueVariable(strs, completionResults, str9, userPath, text);
				}
			}
			if (num != -1)
			{
				str1 = wordToComplete.Substring(0, num + 1);
				if (!CompletionCompleters.VariableScopes.Contains<string>(str1, StringComparer.OrdinalIgnoreCase))
				{
					str = string.Concat(wordToComplete, "*");
				}
				else
				{
					str = string.Concat("variable:", wordToComplete.Substring(num + 1), "*");
				}
			}
			else
			{
				str = string.Concat("variable:", wordToComplete, "*");
				str1 = "";
			}
			PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
			CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Item").AddParameter("Path", str);
			CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Sort-Object").AddParameter("Property", "Name");
			Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
			if (pSObjects != null)
			{
				foreach (dynamic obj in pSObjects)
				{
					string str10 = obj.Name as string;
					if (string.IsNullOrEmpty(str10))
					{
						continue;
					}
					string str11 = str10;
					PSVariable pSVariable = PSObject.Base(obj) as PSVariable;
					if (pSVariable != null)
					{
						object value = pSVariable.Value;
						if (value != null)
						{
							str11 = StringUtil.Format("[{0}]${1}", ToStringCodeMethods.Type(value.GetType(), true), str10);
						}
					}
					if (str10.IndexOfAny(CompletionCompleters.CharactersRequiringQuotes) == -1)
					{
						str6 = string.Concat(str8, str1, str10);
					}
					else
					{
						string[] strArrays = new string[5];
						strArrays[0] = str8;
						strArrays[1] = "{";
						strArrays[2] = str1;
						strArrays[3] = str10;
						strArrays[4] = "}";
						str6 = string.Concat(strArrays);
					}
					string str12 = str6;
					CompletionCompleters.AddUniqueVariable(strs, completionResults, str12, str10, str11);
				}
			}
			if (num == -1 && "env".StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
			{
				currentPowerShell = context.Helper.CurrentPowerShell;
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Item").AddParameter("Path", "env:*");
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Sort-Object").AddParameter("Property", "Key");
				pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					foreach (dynamic obj1 in pSObjects)
					{
						string str13 = obj1.Name as string;
						if (string.IsNullOrEmpty(str13))
						{
							continue;
						}
						str13 = string.Concat("env:", str13);
						if (str13.IndexOfAny(CompletionCompleters.CharactersRequiringQuotes) == -1)
						{
							str5 = string.Concat(str8, str13);
						}
						else
						{
							str5 = string.Concat(str8, "{", str13, "}");
						}
						string str14 = str5;
						CompletionCompleters.AddUniqueVariable(strs, completionResults, str14, str13, string.Concat("[string]", str13));
					}
				}
			}
			foreach (string value1 in CompletionCompleters._specialVariablesCache.Value)
			{
				if (!wildcardPattern.IsMatch(value1))
				{
					continue;
				}
				if (value1.IndexOfAny(CompletionCompleters.CharactersRequiringQuotes) == -1)
				{
					str2 = string.Concat(str8, value1);
				}
				else
				{
					str2 = string.Concat(str8, "{", value1, "}");
				}
				string str15 = str2;
				CompletionCompleters.AddUniqueVariable(strs, completionResults, str15, value1, value1);
			}
			if (num == -1)
			{
				str = string.Concat(wordToComplete, "*");
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-PSDrive").AddParameter("Name", str);
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Sort-Object").AddParameter("Property", "Name");
				pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					foreach (PSObject pSObject in pSObjects)
					{
						PSDriveInfo pSDriveInfo = PSObject.Base(pSObject) as PSDriveInfo;
						if (pSDriveInfo == null)
						{
							continue;
						}
						string name = pSDriveInfo.Name;
						if (name == null || string.IsNullOrWhiteSpace(name) || name.Length <= 1)
						{
							continue;
						}
						if (name.IndexOfAny(CompletionCompleters.CharactersRequiringQuotes) == -1)
						{
							str4 = string.Concat(str8, name, ":");
						}
						else
						{
							str4 = string.Concat(str8, "{", name, ":}");
						}
						string str16 = str4;
						if (string.IsNullOrEmpty(pSDriveInfo.Description))
						{
							description = name;
						}
						else
						{
							description = pSDriveInfo.Description;
						}
						string str17 = description;
						CompletionCompleters.AddUniqueVariable(strs, completionResults, str16, name, str17);
					}
				}
				WildcardPattern wildcardPattern1 = new WildcardPattern(str, WildcardOptions.IgnoreCase);
				string[] variableScopes = CompletionCompleters.VariableScopes;
				for (int i = 0; i < (int)variableScopes.Length; i++)
				{
					string str18 = variableScopes[i];
					if (wildcardPattern1.IsMatch(str18))
					{
						if (str18.IndexOfAny(CompletionCompleters.CharactersRequiringQuotes) == -1)
						{
							str3 = string.Concat(str8, str18);
						}
						else
						{
							str3 = string.Concat(str8, "{", str18, "}");
						}
						string str19 = str3;
						CompletionCompleters.AddUniqueVariable(strs, completionResults, str19, str18, str18);
					}
				}
			}
			return completionResults;
		}

		private static Collection<PSObject> CompleteWorkflowCommand(string command, Ast lastAst, Collection<PSObject> commandInfos)
		{
			if (lastAst.IsInWorkflow())
			{
				Collection<PSObject> pSObjects = commandInfos;
				Collection<PSObject> pSObjects1 = pSObjects;
				if (pSObjects == null)
				{
					pSObjects1 = new Collection<PSObject>();
				}
				commandInfos = pSObjects1;
				WildcardPattern wildcardPattern = new WildcardPattern(command, WildcardOptions.IgnoreCase);
				foreach (string pseudoWorkflowCommand in CompletionCompleters.PseudoWorkflowCommands)
				{
					if (!wildcardPattern.IsMatch(pseudoWorkflowCommand))
					{
						continue;
					}
					commandInfos.Add(PSObject.AsPSObject(pseudoWorkflowCommand));
				}
				return commandInfos;
			}
			else
			{
				return commandInfos;
			}
		}

		private static bool CompletionRequiresQuotes(string completion, bool escape)
		{
			Token[] tokenArray = null;
			ParseError[] parseErrorArray = null;
			char[] chrArray;
			bool length;
			Parser.ParseInput(completion, out tokenArray, out parseErrorArray);
			if (escape)
			{
				char[] chrArray1 = new char[] { Convert.ToChar(_PrivateImplementationDetails_FC3E3B2A_E7A8_4C71_9D4E_1786F9734191_.__method0x6004bda_1) };
				chrArray = chrArray1;
			}
			else
			{
				char[] chrArray2 = new char[2];
				chrArray2[0] = '$';
				chrArray2[1] = '\u0060';
				chrArray = chrArray2;
			}
			char[] chrArray3 = chrArray;
			if ((int)parseErrorArray.Length != 0)
			{
				length = true;
			}
			else
			{
				length = (int)tokenArray.Length != 2;
			}
			bool flag = length;
			if (!flag && tokenArray[0] as StringToken != null || (int)tokenArray.Length == 2 && (tokenArray[0].TokenFlags & TokenFlags.Keyword) != TokenFlags.None)
			{
				flag = false;
				string text = tokenArray[0].Text;
				if (text.IndexOfAny(chrArray3) != -1)
				{
					flag = true;
				}
			}
			return flag;
		}

		internal static string ConcatenateStringPathArguments(CommandElementAst stringAst, string partialPath, CompletionContext completionContext)
		{
			StringConstantExpressionAst stringConstantExpressionAst = stringAst as StringConstantExpressionAst;
			if (stringConstantExpressionAst == null)
			{
				ExpandableStringExpressionAst expandableStringExpressionAst = stringAst as ExpandableStringExpressionAst;
				string str = null;
				if (expandableStringExpressionAst == null || !CompletionCompleters.IsPathSafelyExpandable(expandableStringExpressionAst, partialPath, completionContext.ExecutionContext, out str))
				{
					return null;
				}
				else
				{
					return str;
				}
			}
			else
			{
				string empty = string.Empty;
				StringConstantType stringConstantType = stringConstantExpressionAst.StringConstantType;
				switch (stringConstantType)
				{
					case StringConstantType.SingleQuoted:
					{
						empty = "'";
						return string.Concat(empty, stringConstantExpressionAst.Value, partialPath, empty);
					}
					case StringConstantType.SingleQuotedHereString:
					{
						return string.Concat(empty, stringConstantExpressionAst.Value, partialPath, empty);
					}
					case StringConstantType.DoubleQuoted:
					{
						empty = "\"";
						return string.Concat(empty, stringConstantExpressionAst.Value, partialPath, empty);
					}
					default:
					{
						return string.Concat(empty, stringConstantExpressionAst.Value, partialPath, empty);
					}
				}
			}
		}

		private static CompletionCompleters.ArgumentLocation FindTargetArgumentLocation(Collection<AstParameterArgumentPair> parsedArguments, Token token)
		{
			CompletionCompleters.ArgumentLocation argumentLocation;
			int num = 0;
			AstParameterArgumentPair astParameterArgumentPair = null;
			IEnumerator<AstParameterArgumentPair> enumerator = parsedArguments.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					AstParameterArgumentPair current = enumerator.Current;
					AstParameterArgumentType parameterArgumentType = current.ParameterArgumentType;
					if (parameterArgumentType == AstParameterArgumentType.AstPair)
					{
						AstPair astPair = (AstPair)current;
						if (!astPair.ParameterSpecified)
						{
							if (astPair.Argument.Extent.StartOffset <= token.Extent.StartOffset)
							{
								num++;
							}
							else
							{
								argumentLocation = CompletionCompleters.GenerateArgumentLocation(astParameterArgumentPair, num);
								return argumentLocation;
							}
						}
						else
						{
							if (astPair.Parameter.Extent.StartOffset <= token.Extent.StartOffset)
							{
								if (!astPair.ParameterContainsArgument && astPair.Argument.Extent.StartOffset > token.Extent.StartOffset)
								{
									CompletionCompleters.ArgumentLocation argumentLocation1 = new CompletionCompleters.ArgumentLocation();
									argumentLocation1.Argument = astPair;
									argumentLocation1.IsPositional = false;
									argumentLocation1.Position = -1;
									argumentLocation = argumentLocation1;
									return argumentLocation;
								}
							}
							else
							{
								argumentLocation = CompletionCompleters.GenerateArgumentLocation(astParameterArgumentPair, num);
								return argumentLocation;
							}
						}
						astParameterArgumentPair = astPair;
						continue;
					}
					else if (parameterArgumentType == AstParameterArgumentType.Switch || parameterArgumentType == AstParameterArgumentType.Fake)
					{
						if (current.Parameter.Extent.StartOffset <= token.Extent.StartOffset)
						{
							astParameterArgumentPair = current;
							continue;
						}
						else
						{
							argumentLocation = CompletionCompleters.GenerateArgumentLocation(astParameterArgumentPair, num);
							return argumentLocation;
						}
					}
					else if (parameterArgumentType == AstParameterArgumentType.AstArray || parameterArgumentType == AstParameterArgumentType.PipeObject)
					{
						continue;
					}
				}
				return CompletionCompleters.GenerateArgumentLocation(astParameterArgumentPair, num);
			}
			return argumentLocation;
		}

		private static CompletionCompleters.ArgumentLocation FindTargetArgumentLocation(Collection<AstParameterArgumentPair> parsedArguments, ExpressionAst expAst)
		{
			CompletionCompleters.ArgumentLocation argumentLocation;
			CompletionCompleters.ArgumentLocation argumentLocation1;
			int num = 0;
			IEnumerator<AstParameterArgumentPair> enumerator = parsedArguments.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					AstParameterArgumentPair current = enumerator.Current;
					AstParameterArgumentType parameterArgumentType = current.ParameterArgumentType;
					if (parameterArgumentType == AstParameterArgumentType.AstPair)
					{
						AstPair astPair = (AstPair)current;
						if (astPair.ArgumentIsCommandParameterAst)
						{
							continue;
						}
						if (!astPair.ParameterContainsArgument || astPair.Argument != expAst)
						{
							if (astPair.Argument.GetHashCode() != expAst.GetHashCode())
							{
								if (astPair.ParameterSpecified)
								{
									continue;
								}
								num++;
								continue;
							}
							else
							{
								if (astPair.ParameterSpecified)
								{
									CompletionCompleters.ArgumentLocation argumentLocation2 = new CompletionCompleters.ArgumentLocation();
									argumentLocation2.IsPositional = false;
									argumentLocation2.Position = -1;
									argumentLocation2.Argument = astPair;
									argumentLocation1 = argumentLocation2;
								}
								else
								{
									CompletionCompleters.ArgumentLocation argumentLocation3 = new CompletionCompleters.ArgumentLocation();
									argumentLocation3.IsPositional = true;
									argumentLocation3.Position = num;
									argumentLocation3.Argument = astPair;
									argumentLocation1 = argumentLocation3;
								}
								argumentLocation = argumentLocation1;
								return argumentLocation;
							}
						}
						else
						{
							CompletionCompleters.ArgumentLocation argumentLocation4 = new CompletionCompleters.ArgumentLocation();
							argumentLocation4.IsPositional = false;
							argumentLocation4.Position = -1;
							argumentLocation4.Argument = astPair;
							argumentLocation = argumentLocation4;
							return argumentLocation;
						}
					}
					else if (parameterArgumentType == AstParameterArgumentType.Switch || parameterArgumentType == AstParameterArgumentType.Fake || parameterArgumentType == AstParameterArgumentType.AstArray || parameterArgumentType == AstParameterArgumentType.PipeObject)
					{
						continue;
					}
				}
				return null;
			}
			return argumentLocation;
		}

		private static AstPair FindTargetPositionalArgument(Collection<AstParameterArgumentPair> parsedArguments, int position, out AstPair lastPositionalArgument)
		{
			AstPair astPair;
			int num = 0;
			lastPositionalArgument = null;
			IEnumerator<AstParameterArgumentPair> enumerator = parsedArguments.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					AstParameterArgumentPair current = enumerator.Current;
					if (current.ParameterSpecified || num != position)
					{
						if (current.ParameterSpecified)
						{
							continue;
						}
						num++;
						lastPositionalArgument = (AstPair)current;
					}
					else
					{
						astPair = (AstPair)current;
						return astPair;
					}
				}
				return null;
			}
			return astPair;
		}

		private static CompletionCompleters.ArgumentLocation GenerateArgumentLocation(AstParameterArgumentPair prev, int position)
		{
			if (prev != null)
			{
				AstParameterArgumentType parameterArgumentType = prev.ParameterArgumentType;
				switch (parameterArgumentType)
				{
					case AstParameterArgumentType.AstPair:
					case AstParameterArgumentType.Switch:
					{
						if (prev.ParameterSpecified)
						{
							if (prev.Parameter.Extent.Text.EndsWith(":", StringComparison.Ordinal))
							{
								CompletionCompleters.ArgumentLocation argumentLocation = new CompletionCompleters.ArgumentLocation();
								argumentLocation.Argument = prev;
								argumentLocation.IsPositional = false;
								argumentLocation.Position = -1;
								return argumentLocation;
							}
							else
							{
								CompletionCompleters.ArgumentLocation argumentLocation1 = new CompletionCompleters.ArgumentLocation();
								argumentLocation1.Argument = null;
								argumentLocation1.IsPositional = true;
								argumentLocation1.Position = position;
								return argumentLocation1;
							}
						}
						else
						{
							CompletionCompleters.ArgumentLocation argumentLocation2 = new CompletionCompleters.ArgumentLocation();
							argumentLocation2.Argument = null;
							argumentLocation2.IsPositional = true;
							argumentLocation2.Position = position;
							return argumentLocation2;
						}
					}
					case AstParameterArgumentType.Fake:
					{
						CompletionCompleters.ArgumentLocation argumentLocation3 = new CompletionCompleters.ArgumentLocation();
						argumentLocation3.Argument = prev;
						argumentLocation3.IsPositional = false;
						argumentLocation3.Position = -1;
						return argumentLocation3;
					}
				}
				return null;
			}
			else
			{
				CompletionCompleters.ArgumentLocation argumentLocation4 = new CompletionCompleters.ArgumentLocation();
				argumentLocation4.Argument = null;
				argumentLocation4.IsPositional = true;
				argumentLocation4.Position = 0;
				return argumentLocation4;
			}
		}

		private static List<CompletionResult> GetArgumentCompletionResultsWithFailedPseudoBinding(CompletionContext context, CompletionCompleters.ArgumentLocation argLocation, CommandAst commandAst)
		{
			bool flag;
			List<CompletionResult> completionResults = new List<CompletionResult>();
			PseudoBindingInfo pseudoBindingInfo = context.PseudoBindingInfo;
			if (!argLocation.IsPositional)
			{
				string parameterName = argLocation.Argument.ParameterName;
				WildcardPattern wildcardPattern = new WildcardPattern(string.Concat(parameterName, "*"), WildcardOptions.IgnoreCase);
				List<MergedCompiledCommandParameter>.Enumerator enumerator = pseudoBindingInfo.UnboundParameters.GetEnumerator();
				try
				{
					do
					{
						if (!enumerator.MoveNext())
						{
							break;
						}
						MergedCompiledCommandParameter current = enumerator.Current;
						if (!wildcardPattern.IsMatch(current.Parameter.Name))
						{
							flag = false;
							foreach (string alias in current.Parameter.Aliases)
							{
								if (!wildcardPattern.IsMatch(alias))
								{
									continue;
								}
								flag = true;
								CompletionCompleters.ProcessParameter(pseudoBindingInfo.CommandName, commandAst, context, completionResults, current, null);
								break;
							}
						}
						else
						{
							CompletionCompleters.ProcessParameter(pseudoBindingInfo.CommandName, commandAst, context, completionResults, current, null);
							break;
						}
					}
					while (!flag);
				}
				finally
				{
					enumerator.Dispose();
				}
			}
			else
			{
				CompletionCompleters.CompletePositionalArgument(pseudoBindingInfo.CommandName, commandAst, context, completionResults, pseudoBindingInfo.UnboundParameters, pseudoBindingInfo.DefaultParameterSetFlag, 0, argLocation.Position, null);
			}
			return completionResults;
		}

		private static List<CompletionResult> GetArgumentCompletionResultsWithSuccessfulPseudoBinding(CompletionContext context, CompletionCompleters.ArgumentLocation argLocation, CommandAst commandAst)
		{
			PseudoBindingInfo pseudoBindingInfo = context.PseudoBindingInfo;
			List<CompletionResult> completionResults = new List<CompletionResult>();
			if (argLocation.IsPositional && argLocation.Argument == null)
			{
				Func<ExpressionAst, bool> func = null;
                AstPair LambdaVar243 = null;
				AstParameterArgumentPair astParameterArgumentPair = CompletionCompleters.FindTargetPositionalArgument(pseudoBindingInfo.AllParsedArguments, argLocation.Position, out LambdaVar243);
				if (astParameterArgumentPair == null)
				{
					if (LambdaVar243 != null)
					{
						bool flag = false;
						Collection<string> strs = new Collection<string>();
						foreach (KeyValuePair<string, AstParameterArgumentPair> boundArgument in pseudoBindingInfo.BoundArguments)
						{
							if (boundArgument.Value.ParameterSpecified)
							{
								if (!boundArgument.Value.ParameterArgumentType.Equals(AstParameterArgumentType.AstArray))
								{
									continue;
								}
								AstArrayPair value = (AstArrayPair)boundArgument.Value;
								ExpressionAst[] argument = value.Argument;
								if (func == null)
								{
									func = (ExpressionAst exp) => exp.GetHashCode() == LambdaVar243.Argument.GetHashCode();
								}
								if (!argument.Any<ExpressionAst>(func))
								{
									continue;
								}
								strs.Add(boundArgument.Key);
							}
							else
							{
								AstPair astPair = (AstPair)boundArgument.Value;
								if (astPair.Argument.GetHashCode() != LambdaVar243.Argument.GetHashCode())
								{
									continue;
								}
								flag = true;
								break;
							}
						}
						if (strs.Count <= 0)
						{
							if (!flag)
							{
								return completionResults;
							}
						}
						else
						{
							foreach (string str in strs)
							{
								MergedCompiledCommandParameter item = pseudoBindingInfo.BoundParameters[str];
								CompletionCompleters.ProcessParameter(pseudoBindingInfo.CommandName, commandAst, context, completionResults, item, pseudoBindingInfo.BoundArguments);
							}
							return completionResults;
						}
					}
					CompletionCompleters.CompletePositionalArgument(pseudoBindingInfo.CommandName, commandAst, context, completionResults, pseudoBindingInfo.UnboundParameters, pseudoBindingInfo.DefaultParameterSetFlag, pseudoBindingInfo.ValidParameterSetsFlags, argLocation.Position, pseudoBindingInfo.BoundArguments);
					return completionResults;
				}
				else
				{
					argLocation.Argument = astParameterArgumentPair;
				}
			}
			if (argLocation.Argument != null)
			{
				Collection<string> strs1 = new Collection<string>();
				foreach (KeyValuePair<string, AstParameterArgumentPair> keyValuePair in pseudoBindingInfo.BoundArguments)
				{
					if (keyValuePair.Value.ParameterArgumentType.Equals(AstParameterArgumentType.PipeObject))
					{
						continue;
					}
					if (!keyValuePair.Value.ParameterArgumentType.Equals(AstParameterArgumentType.AstArray) || argLocation.Argument.ParameterSpecified)
					{
						if (keyValuePair.Value.GetHashCode() != argLocation.Argument.GetHashCode())
						{
							continue;
						}
						strs1.Add(keyValuePair.Key);
					}
					else
					{
						AstArrayPair astArrayPair = (AstArrayPair)keyValuePair.Value;
						AstPair argument1 = (AstPair)argLocation.Argument;
						if (!astArrayPair.Argument.Any<ExpressionAst>((ExpressionAst exp) => (exp.GetHashCode() == argument1.Argument.GetHashCode())))
						{
							continue;
						}
						strs1.Add(keyValuePair.Key);
					}
				}
				if (strs1.Count > 0)
				{
					foreach (string str1 in strs1)
					{
						MergedCompiledCommandParameter mergedCompiledCommandParameter = pseudoBindingInfo.BoundParameters[str1];
						CompletionCompleters.ProcessParameter(pseudoBindingInfo.CommandName, commandAst, context, completionResults, mergedCompiledCommandParameter, pseudoBindingInfo.BoundArguments);
					}
				}
			}
			return completionResults;
		}

		private static Hashtable GetBoundArgumentsAsHashtable(CompletionContext context)
		{
			object obj = null;
			ExpressionAst argument;
			Hashtable hashtables = new Hashtable(StringComparer.OrdinalIgnoreCase);
			if (context.PseudoBindingInfo != null)
			{
				Dictionary<string, AstParameterArgumentPair> boundArguments = context.PseudoBindingInfo.BoundArguments;
				if (boundArguments != null)
				{
					foreach (KeyValuePair<string, AstParameterArgumentPair> boundArgument in boundArguments)
					{
						AstPair value = boundArgument.Value as AstPair;
						if (value == null)
						{
							SwitchPair switchPair = boundArgument.Value as SwitchPair;
							if (switchPair == null)
							{
								continue;
							}
							hashtables[boundArgument.Key] = switchPair.Argument;
						}
						else
						{
							CommandParameterAst commandParameterAst = value.Argument as CommandParameterAst;
							if (commandParameterAst != null)
							{
								argument = commandParameterAst.Argument;
							}
							else
							{
								argument = value.Argument as ExpressionAst;
							}
							ExpressionAst expressionAst = argument;
							if (expressionAst == null || !SafeExprEvaluator.TrySafeEval(expressionAst, context.ExecutionContext, out obj))
							{
								continue;
							}
							hashtables[boundArgument.Key] = obj;
						}
					}
				}
			}
			return hashtables;
		}

		private static string GetCimPropertyToString(CimPropertyDeclaration cimProperty)
		{
			string str;
			string str1;
			Microsoft.Management.Infrastructure.CimType cimType = cimProperty.CimType;
			switch (cimType)
			{
				case Microsoft.Management.Infrastructure.CimType.DateTime:
				case Microsoft.Management.Infrastructure.CimType.Reference:
				case Microsoft.Management.Infrastructure.CimType.Instance:
				{
					str = string.Concat("CimInstance#", cimProperty.CimType.ToString());
					break;
				}
				case Microsoft.Management.Infrastructure.CimType.String:
				{
					str = ToStringCodeMethods.Type(CimConverter.GetDotNetType(cimProperty.CimType), false);
					break;
				}
				default:
				{
					if (cimType == Microsoft.Management.Infrastructure.CimType.DateTimeArray || cimType == Microsoft.Management.Infrastructure.CimType.ReferenceArray || cimType == Microsoft.Management.Infrastructure.CimType.InstanceArray)
					{
                        str = string.Concat("CimInstance#", cimProperty.CimType.ToString());
					}
					else if (cimType == Microsoft.Management.Infrastructure.CimType.StringArray)
					{
                        str = ToStringCodeMethods.Type(CimConverter.GetDotNetType(cimProperty.CimType), false);
					}
                    str = ToStringCodeMethods.Type(CimConverter.GetDotNetType(cimProperty.CimType), false);
                    break;
				}
			}
			bool flags = CimFlags.ReadOnly == (cimProperty.Flags & CimFlags.ReadOnly);
			string[] name = new string[5];
			name[0] = str;
			name[1] = " ";
			name[2] = cimProperty.Name;
			name[3] = " { get; ";
			string[] strArrays = name;
			int num = 4;
			if (flags)
			{
				str1 = "}";
			}
			else
			{
				str1 = "set; }";
			}
			strArrays[num] = str1;
			return string.Concat(name);
		}

		internal static CompletionResult GetCommandNameCompletionResult(string name, object command, bool addAmpersandIfNecessary, string quote)
		{
			bool flag;
			string str;
			bool flag1;
			bool flag2;
			string str1;
			string syntax = name;
			string str2 = name;
			CommandInfo commandInfo = command as CommandInfo;
			if (commandInfo != null)
			{
				try
				{
					str2 = commandInfo.Name;
					syntax = commandInfo.Syntax;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
				}
			}
			if (string.IsNullOrEmpty(syntax))
			{
				str = name;
			}
			else
			{
				str = syntax;
			}
			syntax = str;
			if (!CompletionCompleters.CompletionRequiresQuotes(name, false))
			{
				if (!(quote == string.Empty) || !addAmpersandIfNecessary || !Tokenizer.IsKeyword(name))
				{
					flag1 = false;
				}
				else
				{
					flag1 = !name.Equals("InlineScript", StringComparison.OrdinalIgnoreCase);
				}
				flag = flag1;
				name = string.Concat(quote, name, quote);
			}
			else
			{
				if (quote != string.Empty)
				{
					flag2 = false;
				}
				else
				{
					flag2 = addAmpersandIfNecessary;
				}
				flag = flag2;
				if (quote == string.Empty)
				{
					str1 = "'";
				}
				else
				{
					str1 = quote;
				}
				string str3 = str1;
				if (str3 != "'")
				{
					name = name.Replace("`", "``");
					name = name.Replace("$", "`$");
				}
				else
				{
					name = name.Replace("'", "''");
				}
				name = string.Concat(str3, name, str3);
			}
			if (flag && name != "foreach")
			{
				name = string.Concat("& ", name);
			}
			return new CompletionResult(name, str2, CompletionResultType.Command, syntax);
		}

		private static Type GetEffectiveParameterType(Type type)
		{
			Type underlyingType = Nullable.GetUnderlyingType(type);
			Type type1 = underlyingType;
			Type type2 = type1;
			if (type1 == null)
			{
				type2 = type;
			}
			return type2;
		}

		internal static List<string> GetFileShares(string machine, bool ignoreHidden)
		{
			IntPtr intPtr;
			int num = 0;
			int num1 = 0;
			int num2 = 0;
			int num3 = CompletionCompleters.NetShareEnum(machine, 1, out intPtr, -1, out num, out num1, ref num2);
			List<string> strs = new List<string>();
			if (num3 == 0 || num3 == 234)
			{
				for (int i = 0; i < num; i++)
				{
					IntPtr intPtr1 = (IntPtr)((long)intPtr + (long)(Marshal.SizeOf(typeof(CompletionCompleters.SHARE_INFO_1)) * i));
					CompletionCompleters.SHARE_INFO_1 structure = (CompletionCompleters.SHARE_INFO_1)Marshal.PtrToStructure(intPtr1, typeof(CompletionCompleters.SHARE_INFO_1));
					if ((structure.type & 0xff) == 0 && (!ignoreHidden || !structure.netname.EndsWith("$", StringComparison.Ordinal)))
					{
						strs.Add(structure.netname);
					}
				}
			}
			return strs;
		}

		internal static IEnumerable<object> GetMembersByInferredType(PSTypeName typename, bool @static, CompletionContext context)
		{
			IEnumerable<Type> types;
			Exception exception = null;
			List<object> objs = new List<object>();
			if (typename.Type == null)
			{
				if (!@static)
				{
					string[] name = new string[1];
					name[0] = typename.Name;
					ConsolidatedString consolidatedString = new ConsolidatedString(name);
					objs.AddRange(context.ExecutionContext.TypeTable.GetMembers<PSMemberInfo>(consolidatedString));
				}
				Match match = Regex.Match(typename.Name, "(?<NetTypeName>.*)#(?<CimNamespace>.*)[/\\\\](?<CimClassName>.*)");
				if (match.Success && match.Groups["NetTypeName"].Value.Equals(typeof(CimInstance).FullName, StringComparison.OrdinalIgnoreCase))
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(context.Helper.CurrentPowerShell, "Get-CimClass").AddParameter("Namespace", match.Groups["CimNamespace"].Value).AddParameter("Class", match.Groups["CimClassName"].Value);
					Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
					foreach (CimClass cimClass in pSObjects.Select<PSObject, object>(new Func<PSObject, object>(PSObject.Base)).OfType<CimClass>())
					{
						objs.AddRange(cimClass.CimClassProperties);
					}
				}
			}
			else
			{
				if (!typename.Type.IsArray)
				{
					Type[] interfaces = typename.Type.GetInterfaces();
					types = (IEnumerable<Type>)interfaces.Where<Type>((Type t) => {
						if (!t.IsGenericType)
						{
							return false;
						}
						else
						{
							return t.GetGenericTypeDefinition() == typeof(IEnumerable<>);
						}
					}
					);
				}
				else
				{
					Type[] elementType = new Type[1];
					elementType[0] = typename.Type.GetElementType();
					types = elementType;
				}
				foreach (Type type in types.Prepend<Type>(typename.Type))
				{
					if (!@static)
					{
						ConsolidatedString internedTypeNameHierarchy = DotNetAdapter.GetInternedTypeNameHierarchy(type);
						objs.AddRange(context.ExecutionContext.TypeTable.GetMembers<PSMemberInfo>(internedTypeNameHierarchy));
					}
					IEnumerable<object> propertiesAndMethods = PSObject.dotNetInstanceAdapter.GetPropertiesAndMethods(type, @static);
					objs.AddRange(propertiesAndMethods);
				}
			}
			return objs;
		}

		private static string GetOperatorDescription(string op)
		{
			return string.Concat("-", op);
		}

		private static List<CompletionResult> GetParameterCompletionResults(string parameterName, PseudoBindingInfo bindingInfo, CommandParameterAst parameterAst, bool withColon)
		{
			List<CompletionResult> completionResults;
			string empty;
			Func<CommandParameterAst, bool> func = null;
			Func<CommandParameterAst, bool> func1 = null;
			Func<AstParameterArgumentPair, bool> func2 = null;
			List<CompletionResult> parameterCompletionResults = new List<CompletionResult>();
			if (parameterName != string.Empty)
			{
				if (bindingInfo.ParametersNotFound.Count > 0)
				{
					Collection<CommandParameterAst> parametersNotFound = bindingInfo.ParametersNotFound;
					if (func == null)
					{
						func = (CommandParameterAst pAst) => parameterAst.GetHashCode() == pAst.GetHashCode();
					}
					if (parametersNotFound.Any<CommandParameterAst>(func))
					{
						return parameterCompletionResults;
					}
				}
				if (bindingInfo.AmbiguousParameters.Count <= 0)
				{
					if (bindingInfo.DuplicateParameters.Count <= 0)
					{
						string key = null;
						Dictionary<string, AstParameterArgumentPair>.Enumerator enumerator = bindingInfo.BoundArguments.GetEnumerator();
						try
						{
							do
							{
								if (!enumerator.MoveNext())
								{
									break;
								}
								KeyValuePair<string, AstParameterArgumentPair> current = enumerator.Current;
								AstParameterArgumentType parameterArgumentType = current.Value.ParameterArgumentType;
								if (parameterArgumentType == AstParameterArgumentType.AstPair)
								{
									AstPair value = (AstPair)current.Value;
									if (!value.ParameterSpecified || value.Parameter.GetHashCode() != parameterAst.GetHashCode())
									{
										if (!value.ArgumentIsCommandParameterAst || value.Argument.GetHashCode() != parameterAst.GetHashCode())
										{
											continue;
										}
										completionResults = parameterCompletionResults;
										return completionResults;
									}
									else
									{
										key = current.Key;
										continue;
									}
								}
								else if (parameterArgumentType == AstParameterArgumentType.Switch)
								{
									SwitchPair switchPair = (SwitchPair)current.Value;
									if (!switchPair.ParameterSpecified || switchPair.Parameter.GetHashCode() != parameterAst.GetHashCode())
									{
										continue;
									}
									key = current.Key;
									continue;
								}
								else if (parameterArgumentType == AstParameterArgumentType.Fake)
								{
									FakePair fakePair = (FakePair)current.Value;
									if (!fakePair.ParameterSpecified || fakePair.Parameter.GetHashCode() != parameterAst.GetHashCode())
									{
										continue;
									}
									key = current.Key;
									continue;
								}
								else if (parameterArgumentType == AstParameterArgumentType.AstArray || parameterArgumentType == AstParameterArgumentType.PipeObject)
								{
									continue;
								}
							}
							while (key == null);
                            MergedCompiledCommandParameter parameter = bindingInfo.BoundParameters[key];
                            WildcardPattern pattern = new WildcardPattern(parameterName + "*", WildcardOptions.IgnoreCase);
                            string parameterType = "[" + ToStringCodeMethods.Type(parameter.Parameter.Type, true) + "] ";
                            string colonSuffix = withColon ? ":" : string.Empty;
                            if (pattern.IsMatch(key))
                            {
                                string completionText = "-" + key + colonSuffix;
                                string toolTip = parameterType + key;
                                parameterCompletionResults.Add(new CompletionResult(completionText, key, CompletionResultType.ParameterName, toolTip));
                            }
                            parameterCompletionResults.AddRange(from alias in parameter.Parameter.Aliases
                                          where pattern.IsMatch(alias)
                                          select new CompletionResult("-" + alias + colonSuffix, alias, CompletionResultType.ParameterName, parameterType + alias));
                            completionResults = parameterCompletionResults;
						}
						finally
						{
							enumerator.Dispose();
						}
						return completionResults;
					}
					else
					{
						Collection<AstParameterArgumentPair> duplicateParameters = bindingInfo.DuplicateParameters;
						if (func2 == null)
						{
							func2 = (AstParameterArgumentPair pAst) => parameterAst.GetHashCode() == pAst.Parameter.GetHashCode();
						}
						if (duplicateParameters.Any<AstParameterArgumentPair>(func2))
						{
							parameterCompletionResults = CompletionCompleters.GetParameterCompletionResults(parameterName, bindingInfo.ValidParameterSetsFlags, bindingInfo.BoundParameters.Values, withColon);
						}
						return parameterCompletionResults;
					}
				}
				else
				{
					Collection<CommandParameterAst> ambiguousParameters = bindingInfo.AmbiguousParameters;
					if (func1 == null)
					{
						func1 = (CommandParameterAst pAst) => parameterAst.GetHashCode() == pAst.GetHashCode();
					}
					if (ambiguousParameters.Any<CommandParameterAst>(func1))
					{
						parameterCompletionResults = CompletionCompleters.GetParameterCompletionResults(parameterName, bindingInfo.ValidParameterSetsFlags, bindingInfo.UnboundParameters, withColon);
					}
					return parameterCompletionResults;
				}
			}
			else
			{
				parameterCompletionResults = CompletionCompleters.GetParameterCompletionResults(parameterName, bindingInfo.ValidParameterSetsFlags, bindingInfo.UnboundParameters, withColon);
				return parameterCompletionResults;
			}
		}

		private static List<CompletionResult> GetParameterCompletionResults(string parameterName, int validParameterSetFlags, IEnumerable<MergedCompiledCommandParameter> parameters, bool withColon)
		{
			string empty;
			bool isInAllSets;
			List<CompletionResult> completionResults;
			Func<string, bool> func = null;
			List<CompletionResult> completionResults1 = new List<CompletionResult>();
			List<CompletionResult> completionResults2 = new List<CompletionResult>();
			WildcardPattern wildcardPattern = new WildcardPattern(string.Concat(parameterName, "*"), WildcardOptions.IgnoreCase);
			if (withColon)
			{
				empty = ":";
			}
			else
			{
				empty = string.Empty;
			}
			var colonSuffix = empty;
			foreach (MergedCompiledCommandParameter parameter in parameters)
			{
				Func<string, CompletionResult> func1 = null;
				if ((parameter.Parameter.ParameterSetFlags & validParameterSetFlags) != 0)
				{
					isInAllSets = true;
				}
				else
				{
					isInAllSets = parameter.Parameter.IsInAllSets;
				}
				bool flag = isInAllSets;
				if (!flag)
				{
					continue;
				}
				string name = parameter.Parameter.Name;
				string str = string.Concat("[", ToStringCodeMethods.Type(parameter.Parameter.Type, true), "] ");
				bool flag1 = CommonParameters.CommonCommandParameters.Contains<string>(name, StringComparer.OrdinalIgnoreCase);
				if (flag1)
				{
					completionResults = completionResults2;
				}
				else
				{
					completionResults = completionResults1;
				}
				List<CompletionResult> completionResults3 = completionResults;
				if (wildcardPattern.IsMatch(name))
				{
                    string str1 = string.Concat("-", name, empty);
					string str2 = string.Concat(str, name);
					completionResults3.Add(new CompletionResult(str1, name, CompletionResultType.ParameterName, str2));
				}
				if (parameterName == string.Empty)
				{
					continue;
				}
				List<CompletionResult> completionResults4 = completionResults3;
				ReadOnlyCollection<string> aliases = parameter.Parameter.Aliases;
				if (func == null)
				{
					func = (string alias) => wildcardPattern.IsMatch(alias);
				}
				IEnumerable<string> strs = aliases.Where<string>(func);
				if (func1 == null)
				{
                    func1 = (string alias) => new CompletionResult(string.Concat("-", alias, empty), alias, CompletionResultType.ParameterName, string.Concat(str, alias));
				}
				completionResults4.AddRange(strs.Select<string, CompletionResult>(func1));
			}
			completionResults1.AddRange(completionResults2);
			return completionResults1;
		}

		private static List<CompletionResult> GetSpecialHashTableKeyMembers(string[] keys)
		{
			string[] strArrays = keys;
			return strArrays.Select<string, CompletionResult>((string key) => new CompletionResult(key, key, CompletionResultType.Property, key)).ToList<CompletionResult>();
		}

		internal static string HandleDoubleAndSingleQuote(ref string wordToComplete)
		{
			string str;
			string str1;
			string str2;
			string empty = string.Empty;
			if (!string.IsNullOrEmpty(wordToComplete) && (wordToComplete[0].IsSingleQuote() || wordToComplete[0].IsDoubleQuote()))
			{
				char chr = wordToComplete[0];
				int length = wordToComplete.Length;
				if (length != 1)
				{
					if (length > 1)
					{
						if ((!wordToComplete[(length - 1)].IsDoubleQuote() || !chr.IsDoubleQuote()) && (!wordToComplete[(length - 1)].IsSingleQuote() || !chr.IsSingleQuote()))
						{
							if (!wordToComplete[(length - 1)].IsDoubleQuote() && !wordToComplete[(length - 1)].IsSingleQuote())
							{
								wordToComplete = wordToComplete.Substring(1);
								if (chr.IsSingleQuote())
								{
									str = "'";
								}
								else
								{
									str = "\"";
								}
								empty = str;
							}
						}
						else
						{
							wordToComplete = wordToComplete.Substring(1, length - 2);
							if (chr.IsSingleQuote())
							{
								str1 = "'";
							}
							else
							{
								str1 = "\"";
							}
							empty = str1;
						}
					}
				}
				else
				{
					wordToComplete = string.Empty;
					if (chr.IsSingleQuote())
					{
						str2 = "'";
					}
					else
					{
						str2 = "\"";
					}
					empty = str2;
				}
			}
			return empty;
		}

		private static CompletionCompleters.TypeCompletionMapping[][] InitializeTypeCache()
		{
			CompletionCompleters.TypeCompletionMapping typeCompletionMapping = null;
			CompletionCompleters.TypeCompletion typeCompletion;
			Type type = null;
			Dictionary<string, CompletionCompleters.TypeCompletionMapping> strs = new Dictionary<string, CompletionCompleters.TypeCompletionMapping>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, Type> get in TypeAccelerators.Get)
			{
				CompletionCompleters.TypeCompletionMapping typeCompletionMapping1 = new CompletionCompleters.TypeCompletionMapping();
				typeCompletionMapping1.Key = get.Key;
				CompletionCompleters.TypeCompletion value = new CompletionCompleters.TypeCompletion();
				value.Type = get.Value;
				typeCompletionMapping1.Completions.Add(value);
				strs.Add(get.Key, typeCompletionMapping1);
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			IEnumerable<Type> types = assemblies.SelectMany<Assembly, Type>((Assembly assembly) => assembly.GetTypes().Where<Type>(new Func<Type, bool>(LanguagePrimitives.IsPublic)));
			foreach (Type type1 in types)
			{
				string @namespace = type1.Namespace;
				if (!string.IsNullOrEmpty(@namespace))
				{
					if (strs.TryGetValue(@namespace, out typeCompletionMapping))
					{
						if (!typeCompletionMapping.Completions.OfType<CompletionCompleters.NamespaceCompletion>().Any<CompletionCompleters.NamespaceCompletion>())
						{
							CompletionCompleters.NamespaceCompletion namespaceCompletion = new CompletionCompleters.NamespaceCompletion();
							namespaceCompletion.Namespace = @namespace;
							typeCompletionMapping.Completions.Add(namespaceCompletion);
						}
					}
					else
					{
						CompletionCompleters.TypeCompletionMapping typeCompletionMapping2 = new CompletionCompleters.TypeCompletionMapping();
						typeCompletionMapping2.Key = @namespace;
						CompletionCompleters.NamespaceCompletion namespaceCompletion1 = new CompletionCompleters.NamespaceCompletion();
						namespaceCompletion1.Namespace = @namespace;
						typeCompletionMapping2.Completions.Add(namespaceCompletion1);
						typeCompletionMapping = typeCompletionMapping2;
						strs.Add(@namespace, typeCompletionMapping);
					}
				}
				string fullName = type1.FullName;
				if (string.IsNullOrEmpty(fullName))
				{
					continue;
				}
				string name = type1.Name;
				if (!type1.IsGenericTypeDefinition)
				{
					CompletionCompleters.TypeCompletion typeCompletion1 = new CompletionCompleters.TypeCompletion();
					typeCompletion1.Type = type1;
					typeCompletion = typeCompletion1;
				}
				else
				{
					if (type1.IsNested)
					{
						continue;
					}
					int num = fullName.LastIndexOf('\u0060');
					if (num != -1)
					{
						fullName = fullName.Substring(0, num);
						name = name.Substring(0, name.LastIndexOf('\u0060'));
					}
					CompletionCompleters.GenericTypeCompletion genericTypeCompletion = new CompletionCompleters.GenericTypeCompletion();
					genericTypeCompletion.Type = type1;
					typeCompletion = genericTypeCompletion;
				}
				if (!strs.TryGetValue(fullName, out typeCompletionMapping))
				{
					CompletionCompleters.TypeCompletionMapping typeCompletionMapping3 = new CompletionCompleters.TypeCompletionMapping();
					typeCompletionMapping3.Key = fullName;
					typeCompletionMapping3.Completions.Add(typeCompletion);
					typeCompletionMapping = typeCompletionMapping3;
					strs.Add(fullName, typeCompletionMapping);
				}
				if (TypeAccelerators.Get.TryGetValue(name, out type))
				{
					continue;
				}
				if (!strs.TryGetValue(name, out typeCompletionMapping))
				{
					CompletionCompleters.TypeCompletionMapping typeCompletionMapping4 = new CompletionCompleters.TypeCompletionMapping();
					typeCompletionMapping4.Key = name;
					typeCompletionMapping = typeCompletionMapping4;
					strs.Add(name, typeCompletionMapping);
				}
				typeCompletionMapping.Completions.Add(typeCompletion);
			}
			var values = strs.Values;
			IEnumerable<IGrouping<int, CompletionCompleters.TypeCompletionMapping>> groupings = values.GroupBy<CompletionCompleters.TypeCompletionMapping, int>((CompletionCompleters.TypeCompletionMapping t) => {
				string key = t.Key;
				return key.Where<char>((char c) => c == '.').Count<char>();
			}
			);
			IGrouping<int, CompletionCompleters.TypeCompletionMapping>[] array = groupings.OrderBy<IGrouping<int, CompletionCompleters.TypeCompletionMapping>, int>((IGrouping<int, CompletionCompleters.TypeCompletionMapping> g) => g.Key).ToArray<IGrouping<int, CompletionCompleters.TypeCompletionMapping>>();
			CompletionCompleters.TypeCompletionMapping[][] typeCompletionMappingArray = new CompletionCompleters.TypeCompletionMapping[array.Last<IGrouping<int, CompletionCompleters.TypeCompletionMapping>>().Key + 1][];
			IGrouping<int, CompletionCompleters.TypeCompletionMapping>[] groupingArrays = array;
			for (int i = 0; i < (int)groupingArrays.Length; i++)
			{
				IGrouping<int, CompletionCompleters.TypeCompletionMapping> nums = groupingArrays[i];
				typeCompletionMappingArray[nums.Key] = nums.ToArray<CompletionCompleters.TypeCompletionMapping>();
			}
			Interlocked.Exchange<CompletionCompleters.TypeCompletionMapping[][]>(ref CompletionCompleters.typeCache, typeCompletionMappingArray);
			return typeCompletionMappingArray;
		}

		internal static bool IsAmpersandNeeded(CompletionContext context, bool defaultChoice)
		{
			if (context.RelatedAsts != null && !string.IsNullOrEmpty(context.WordToComplete))
			{
				Ast ast = context.RelatedAsts.Last<Ast>();
				CommandAst parent = ast.Parent as CommandAst;
				if (parent != null && parent.CommandElements.Count == 1 && (!defaultChoice && parent.InvocationOperator == TokenKind.Unknown || defaultChoice && parent.InvocationOperator != TokenKind.Unknown))
				{
					defaultChoice = !defaultChoice;
				}
			}
			return defaultChoice;
		}

		internal static bool IsPathSafelyExpandable(ExpandableStringExpressionAst expandableStringAst, string extraText, ExecutionContext executionContext, out string expandedString)
		{
			bool flag;
			string empty;
			expandedString = null;
			StringConstantType stringConstantType = expandableStringAst.StringConstantType;
			if (stringConstantType != StringConstantType.DoubleQuotedHereString)
			{
				List<string> strs = new List<string>();
				IEnumerator<ExpressionAst> enumerator = expandableStringAst.NestedExpressions.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						ExpressionAst current = enumerator.Current;
						VariableExpressionAst variableExpressionAst = current as VariableExpressionAst;
						if (variableExpressionAst != null)
						{
							string str = CompletionCompleters.CombineVariableWithPartialPath(variableExpressionAst, null, executionContext);
							if (str == null)
							{
								flag = false;
								return flag;
							}
							else
							{
								strs.Add(str);
							}
						}
						else
						{
							flag = false;
							return flag;
						}
					}
					string str1 = string.Format(CultureInfo.InvariantCulture, expandableStringAst.FormatExpression, strs.ToArray());
					if (stringConstantType == StringConstantType.DoubleQuoted)
					{
						empty = "\"";
					}
					else
					{
						empty = string.Empty;
					}
					string str2 = empty;
					expandedString = string.Concat(str2, str1, extraText, str2);
					return true;
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		internal static bool IsSplattedVariable(Ast targetExpr)
		{
			if (targetExpr as VariableExpressionAst == null || !((VariableExpressionAst)targetExpr).Splatted)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		internal static List<CompletionResult> MakeCommandsUnique(IEnumerable<PSObject> commandInfoPsObjs, bool includeModulePrefix, bool addAmpersandIfNecessary, string quote, CompletionContext context)
		{
			string name;
			object obj = null;
			List<CompletionResult> completionResults = new List<CompletionResult>();
			if (commandInfoPsObjs == null || !commandInfoPsObjs.Any<PSObject>())
			{
				return completionResults;
			}
			else
			{
				Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				foreach (PSObject commandInfoPsObj in commandInfoPsObjs)
				{
					object obj1 = PSObject.Base(commandInfoPsObj);
					CommandInfo commandInfo = obj1 as CommandInfo;
					if (commandInfo == null)
					{
						name = obj1 as string;
						if (name == null)
						{
							continue;
						}
					}
					else
					{
						if (commandInfo.Visibility == SessionStateEntryVisibility.Private)
						{
							continue;
						}
						name = commandInfo.Name;
						if (includeModulePrefix && !string.IsNullOrEmpty(commandInfo.ModuleName) && (string.IsNullOrEmpty(commandInfo.Prefix) || !ModuleCmdletBase.IsPrefixedCommand(commandInfo)))
						{
							name = string.Concat(commandInfo.ModuleName, "\\", commandInfo.Name);
						}
					}
					if (strs.TryGetValue(name, out obj))
					{
						List<object> objs = obj as List<object>;
						if (objs == null)
						{
							List<object> objs1 = new List<object>();
							objs1.Add(obj);
							objs1.Add(obj1);
							objs = objs1;
							strs[name] = objs;
						}
						else
						{
							objs.Add(obj1);
						}
					}
					else
					{
						strs.Add(name, obj1);
					}
				}
				List<CompletionResult> completionResults1 = null;
				foreach (KeyValuePair<string, object> str in strs)
				{
					List<object> value = str.Value as List<object>;
					if (value == null)
					{
						string key = str.Key;
						if (!includeModulePrefix)
						{
							CommandInfo value1 = str.Value as CommandInfo;
							if (value1 != null && !string.IsNullOrEmpty(value1.Prefix) && !ModuleCmdletBase.IsPrefixedCommand(value1))
							{
								key = string.Concat(value1.ModuleName, "\\", key);
							}
						}
						completionResults.Add(CompletionCompleters.GetCommandNameCompletionResult(key, str.Value, addAmpersandIfNecessary, quote));
					}
					else
					{
						if (completionResults1 == null)
						{
							completionResults1 = new List<CompletionResult>();
						}
						string key1 = str.Key;
						if (!includeModulePrefix)
						{
							CommandInfo item = value[0] as CommandInfo;
							if (item != null && !string.IsNullOrEmpty(item.Prefix) && !ModuleCmdletBase.IsPrefixedCommand(item))
							{
								key1 = string.Concat(item.ModuleName, "\\", key1);
							}
						}
						completionResults.Add(CompletionCompleters.GetCommandNameCompletionResult(key1, value[0], addAmpersandIfNecessary, quote));
						for (int i = 1; i < value.Count; i++)
						{
							CommandInfo item1 = value[i] as CommandInfo;
							if (item1 != null)
							{
								if (item1.CommandType != CommandTypes.Application)
								{
									if (!string.IsNullOrEmpty(item1.ModuleName))
									{
										string str1 = string.Concat(item1.ModuleName, "\\", item1.Name);
										completionResults1.Add(CompletionCompleters.GetCommandNameCompletionResult(str1, item1, addAmpersandIfNecessary, quote));
									}
								}
								else
								{
									completionResults1.Add(CompletionCompleters.GetCommandNameCompletionResult(item1.Definition, item1, addAmpersandIfNecessary, quote));
								}
							}
						}
					}
				}
				if (completionResults1 != null && completionResults1.Count > 0)
				{
					completionResults.AddRange(completionResults1);
				}
				return completionResults;
			}
		}

		private static void NativeCommandArgumentCompletion(string commandName, string parameter, List<CompletionResult> result, CommandAst commandAst, CompletionContext context, Dictionary<string, AstParameterArgumentPair> boundArguments = null)
		{
			bool flag;
			if (!string.IsNullOrEmpty(commandName))
			{
				string[] strArrays = new string[2];
				strArrays[0] = string.Concat(commandName, ":", parameter);
				strArrays[1] = parameter;
				object[] wordToComplete = new object[5];
				wordToComplete[0] = commandName;
				wordToComplete[1] = parameter;
				wordToComplete[2] = context.WordToComplete;
				wordToComplete[3] = commandAst;
				wordToComplete[4] = CompletionCompleters.GetBoundArgumentsAsHashtable(context);
				if (!CompletionCompleters.TryCustomArgumentCompletion("CustomArgumentCompleters", strArrays, wordToComplete, context, result))
				{
					string str = commandName;
					string str1 = str;
					if (str != null)
					{
						if (str1 == "Get-Command")
						{
							if (!parameter.Equals("Module", StringComparison.OrdinalIgnoreCase))
							{
								if (!parameter.Equals("Name", StringComparison.OrdinalIgnoreCase))
								{
									if (!parameter.Equals("ParameterType", StringComparison.OrdinalIgnoreCase))
									{
										return;
									}
									CompletionCompleters.NativeCompletionTypeName(context, result);
									return;
								}
								else
								{
									IList<string> strs = CompletionCompleters.NativeCommandArgumentCompletion_ExtractSecondaryArgument(boundArguments, "Module");
									if (strs.Count <= 0)
									{
										CompletionCompleters.NativeCompletionGetCommand(context.WordToComplete, null, parameter, result, context);
										return;
									}
									else
									{
										IEnumerator<string> enumerator = strs.GetEnumerator();
										using (enumerator)
										{
											while (enumerator.MoveNext())
											{
												string current = enumerator.Current;
												CompletionCompleters.NativeCompletionGetCommand(context.WordToComplete, current, parameter, result, context);
											}
											return;
										}
									}
								}
							}
							else
							{
								CompletionCompleters.NativeCompletionGetCommand(context.WordToComplete, null, parameter, result, context);
								return;
							}
						}
						else if (str1 == "Show-Command")
						{
							CompletionCompleters.NativeCompletionGetHelpCommand(context.WordToComplete, parameter, false, result, context);
							return;
						}
						else if (str1 == "help" || str1 == "Get-Help")
						{
							CompletionCompleters.NativeCompletionGetHelpCommand(context.WordToComplete, parameter, true, result, context);
							return;
						}
						else if (str1 == "Invoke-Expression")
						{
							if (!parameter.Equals("Command", StringComparison.OrdinalIgnoreCase))
							{
								return;
							}
							CompletionContext completionContext = new CompletionContext();
							completionContext.WordToComplete = context.WordToComplete;
							completionContext.Helper = context.Helper;
							List<CompletionResult> completionResults = CompletionCompleters.CompleteCommand(completionContext);
							if (completionResults == null)
							{
								return;
							}
							result.AddRange(completionResults);
							return;
						}
						else if (str1 == "Clear-EventLog" || str1 == "Get-EventLog" || str1 == "Limit-EventLog" || str1 == "Remove-EventLog" || str1 == "Write-EventLog")
						{
							CompletionCompleters.NativeCompletionEventLogCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Get-Job" || str1 == "Receive-Job" || str1 == "Remove-Job" || str1 == "Stop-Job" || str1 == "Wait-Job" || str1 == "Suspend-Job" || str1 == "Resume-Job")
						{
							CompletionCompleters.NativeCompletionJobCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Disable-ScheduledJob" || str1 == "Enable-ScheduledJob" || str1 == "Get-ScheduledJob" || str1 == "Unregister-ScheduledJob")
						{
							CompletionCompleters.NativeCompletionScheduledJobCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Get-Module")
						{
							if (boundArguments == null)
							{
								flag = true;
							}
							else
							{
								flag = !boundArguments.ContainsKey("ListAvailable");
							}
							bool flag1 = flag;
							CompletionCompleters.NativeCompletionModuleCommands(context.WordToComplete, parameter, flag1, false, result, context);
							return;
						}
						else if (str1 == "Remove-Module")
						{
							CompletionCompleters.NativeCompletionModuleCommands(context.WordToComplete, parameter, true, false, result, context);
							return;
						}
						else if (str1 == "Import-Module")
						{
							CompletionCompleters.NativeCompletionModuleCommands(context.WordToComplete, parameter, false, true, result, context);
							return;
						}
						else if (str1 == "Debug-Process" || str1 == "Get-Process" || str1 == "Stop-Process" || str1 == "Wait-Process")
						{
							CompletionCompleters.NativeCompletionProcessCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Get-PSDrive" || str1 == "Remove-PSDrive")
						{
							if (!parameter.Equals("PSProvider", StringComparison.OrdinalIgnoreCase))
							{
								if (!parameter.Equals("Name", StringComparison.OrdinalIgnoreCase))
								{
									return;
								}
								IList<string> strs1 = CompletionCompleters.NativeCommandArgumentCompletion_ExtractSecondaryArgument(boundArguments, "PSProvider");
								if (strs1.Count <= 0)
								{
									CompletionCompleters.NativeCompletionDriveCommands(context.WordToComplete, null, parameter, result, context);
									return;
								}
								else
								{
									IEnumerator<string> enumerator1 = strs1.GetEnumerator();
									using (enumerator1)
									{
										while (enumerator1.MoveNext())
										{
											string current1 = enumerator1.Current;
											CompletionCompleters.NativeCompletionDriveCommands(context.WordToComplete, current1, parameter, result, context);
										}
										return;
									}
								}
							}
							else
							{
								CompletionCompleters.NativeCompletionProviderCommands(context.WordToComplete, parameter, result, context);
								return;
							}
						}
						else if (str1 == "New-PSDrive")
						{
							CompletionCompleters.NativeCompletionProviderCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Get-PSProvider")
						{
							CompletionCompleters.NativeCompletionProviderCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Get-Service" || str1 == "Start-Service" || str1 == "Restart-Service" || str1 == "Resume-Service" || str1 == "Set-Service" || str1 == "Stop-Service" || str1 == "Suspend-Service")
						{
							CompletionCompleters.NativeCompletionServiceCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Clear-Variable" || str1 == "Get-Variable" || str1 == "Remove-Variable" || str1 == "Set-Variable")
						{
							CompletionCompleters.NativeCompletionVariableCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Get-Alias")
						{
							CompletionCompleters.NativeCompletionAliasCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Get-TraceSource" || str1 == "Set-TraceSource" || str1 == "Trace-Command")
						{
							CompletionCompleters.NativeCompletionTraceSourceCommands(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Push-Location" || str1 == "Set-Location")
						{
							CompletionCompleters.NativeCompletionSetLocationCommand(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "Move-Item" || str1 == "Copy-Item")
						{
							CompletionCompleters.NativeCompletionCopyMoveItemCommand(context.WordToComplete, parameter, result, context);
							return;
						}
						else if (str1 == "ForEach-Object")
						{
							if (!parameter.Equals("MemberName", StringComparison.OrdinalIgnoreCase))
							{
								return;
							}
							CompletionCompleters.NativeCompletionMemberName(context.WordToComplete, result, commandAst, context);
							return;
						}
						else if (str1 == "Group-Object" || str1 == "Measure-Object" || str1 == "Select-Object" || str1 == "Sort-Object" || str1 == "Where-Object" || str1 == "Format-Custom" || str1 == "Format-List" || str1 == "Format-Table" || str1 == "Format-Wide")
						{
							if (!parameter.Equals("Property", StringComparison.OrdinalIgnoreCase))
							{
								return;
							}
							CompletionCompleters.NativeCompletionMemberName(context.WordToComplete, result, commandAst, context);
							return;
						}
						else if (str1 == "New-Object")
						{
							if (!parameter.Equals("TypeName", StringComparison.OrdinalIgnoreCase))
							{
								return;
							}
							CompletionCompleters.NativeCompletionTypeName(context, result);
							return;
						}
						else if (str1 == "Get-CimClass" || str1 == "Get-CimInstance" || str1 == "Invoke-CimMethod" || str1 == "New-CimInstance" || str1 == "Register-CimIndicationEvent")
						{
							CompletionCompleters.NativeCompletionCimCommands(parameter, boundArguments, result, context);
							return;
						}
						CompletionCompleters.NativeCompletionPathArgument(context.WordToComplete, parameter, result, context);
						return;
					}
					else
					{
						CompletionCompleters.NativeCompletionPathArgument(context.WordToComplete, parameter, result, context);
						return;
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static IList<string> NativeCommandArgumentCompletion_ExtractSecondaryArgument(Dictionary<string, AstParameterArgumentPair> boundArguments, string parameterName)
		{
			List<string> strs = new List<string>();
			if (boundArguments != null)
			{
				if (boundArguments.ContainsKey(parameterName))
				{
					AstParameterArgumentPair item = boundArguments[parameterName];
					AstParameterArgumentType parameterArgumentType = item.ParameterArgumentType;
					if (parameterArgumentType == AstParameterArgumentType.AstPair)
					{
						AstPair astPair = (AstPair)item;
						if (astPair.Argument as StringConstantExpressionAst == null)
						{
							if (astPair.Argument as ArrayLiteralAst != null)
							{
								ArrayLiteralAst argument = (ArrayLiteralAst)astPair.Argument;
								foreach (ExpressionAst element in argument.Elements)
								{
									StringConstantExpressionAst stringConstantExpressionAst = element as StringConstantExpressionAst;
									if (stringConstantExpressionAst == null)
									{
										strs.Clear();
										break;
									}
									else
									{
										strs.Add(stringConstantExpressionAst.Value);
									}
								}
							}
						}
						else
						{
							StringConstantExpressionAst argument1 = (StringConstantExpressionAst)astPair.Argument;
							strs.Add(argument1.Value);
						}
					}
					else
					{
						if (parameterArgumentType == AstParameterArgumentType.AstArray)
						{
							AstArrayPair astArrayPair = (AstArrayPair)item;
							ExpressionAst[] expressionAstArray = astArrayPair.Argument;
							ExpressionAst[] expressionAstArray1 = expressionAstArray;
							int num = 0;
							while (num < (int)expressionAstArray1.Length)
							{
								ExpressionAst expressionAst = expressionAstArray1[num];
								StringConstantExpressionAst stringConstantExpressionAst1 = expressionAst as StringConstantExpressionAst;
								if (stringConstantExpressionAst1 == null)
								{
									strs.Clear();
									break;
								}
								else
								{
									strs.Add(stringConstantExpressionAst1.Value);
									num++;
								}
							}
						}
					}
					return strs;
				}
				else
				{
					return strs;
				}
			}
			else
			{
				return strs;
			}
		}

		private static void NativeCompletionAliasCommands(string commandName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("Definition", StringComparison.OrdinalIgnoreCase) && !paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				if (!paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
				{
					CompletionContext completionContext = new CompletionContext();
					completionContext.WordToComplete = commandName;
					completionContext.Helper = context.Helper;
					List<CompletionResult> completionResults = CompletionCompleters.CompleteCommand(completionContext, null, CommandTypes.Function | CommandTypes.Cmdlet | CommandTypes.ExternalScript | CommandTypes.Workflow);
					if (completionResults != null && completionResults.Count<CompletionResult>() > 0)
					{
						result.AddRange(completionResults);
					}
					CompletionContext helper = new CompletionContext();
					helper.WordToComplete = commandName;
					helper.Helper = context.Helper;
					List<CompletionResult> completionResults1 = new List<CompletionResult>(CompletionCompleters.CompleteFilename(helper));
					if (completionResults1.Count > 0)
					{
						result.AddRange(completionResults1);
					}
				}
				else
				{
					string str1 = commandName;
					string empty = str1;
					if (str1 == null)
					{
						empty = string.Empty;
					}
					commandName = empty;
					string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref commandName);
					PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
					if (!commandName.EndsWith("*", StringComparison.Ordinal))
					{
						commandName = string.Concat(commandName, "*");
					}
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Alias").AddParameter("Name", commandName);
					Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
					if (pSObjects != null)
					{
						foreach (dynamic obj in pSObjects)
						{
							dynamic obj1 = obj.Name;
							dynamic obj2 = obj1;
							if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
							{
								obj1 = str2 + obj1 + str2;
							}
							else
							{
								if (str2 == string.Empty)
								{
									str = "'";
								}
								else
								{
									str = str2;
								}
								string str3 = str;
								if (str3 == "'")
								{
									obj1 = obj1.Replace("'", "''");
								}
								obj1 = str3 + obj1 + str3;
							}
							List<CompletionResult> completionResults2 = result;
							completionResults2.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
						}
					}
				}
				result.Add(CompletionResult.Null);
				return;
			}
		}

        private static void NativeCompletionCimClassName(string pseudoBoundNamespace, List<CompletionResult> result, CompletionContext context)
        {
            string targetNamespace = pseudoBoundNamespace ?? "root/cimv2";
            List<string> first = new List<string>();
            List<string> second = new List<string>();
            IEnumerable<string> orAdd = cimNamespaceToClassNames.GetOrAdd(targetNamespace, new Func<string, IEnumerable<string>>(CompletionCompleters.NativeCompletionCimClassName_GetClassNames));
            WildcardPattern pattern = new WildcardPattern(context.WordToComplete + "*", WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
            foreach (string str in orAdd)
            {
                if (context.Helper.CancelTabCompletion)
                {
                    break;
                }
                if (pattern.IsMatch(str))
                {
                    if ((str.Length > 0) && (str[0] == '_'))
                    {
                        second.Add(str);
                    }
                    else
                    {
                        first.Add(str);
                    }
                }
            }
            first.Sort(StringComparer.OrdinalIgnoreCase);
            second.Sort(StringComparer.OrdinalIgnoreCase);
            result.AddRange(from className in first.Concat<string>(second) select new CompletionResult(className, className, CompletionResultType.Type, targetNamespace + ":" + className));
            result.Add(CompletionResult.Null);
        }

        private static IEnumerable<string> NativeCompletionCimClassName_GetClassNames(string targetNamespace)
        {
            List<string> list = new List<string>();
            using (CimSession session = CimSession.Create(null))
            {
                CimOperationOptions options2 = new CimOperationOptions
                {
                    ClassNamesOnly = true
                };
                using (CimOperationOptions options = options2)
                {
                    foreach (CimClass class2 in session.EnumerateClasses(targetNamespace, (string)null, options))
                    {
                        using (class2)
                        {
                            string className = class2.CimSystemProperties.ClassName;
                            list.Add(className);
                        }
                    }
                    return list;
                }
            }
        }

		private static void NativeCompletionCimCommands(string parameter, Dictionary<string, AstParameterArgumentPair> boundArguments, List<CompletionResult> result, CompletionContext context)
		{
			if (boundArguments == null || !boundArguments.ContainsKey("CimSession") && !boundArguments.ContainsKey("ComputerName"))
			{
				if (!parameter.Equals("ClassName", StringComparison.OrdinalIgnoreCase))
				{
					if (parameter.Equals("Namespace", StringComparison.OrdinalIgnoreCase))
					{
						CompletionCompleters.NativeCompletionCimNamespace(result, context);
					}
					return;
				}
				else
				{
					string str = CompletionCompleters.NativeCommandArgumentCompletion_ExtractSecondaryArgument(boundArguments, "Namespace").FirstOrDefault<string>();
					CompletionCompleters.NativeCompletionCimClassName(str, result, context);
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionCimNamespace(List<CompletionResult> result, CompletionContext context)
		{
			string str = "root";
			string str1 = "";
			if (!string.IsNullOrEmpty(context.WordToComplete))
			{
				char[] chrArray = new char[2];
				chrArray[0] = '\\';
				chrArray[1] = '/';
				int num = context.WordToComplete.LastIndexOfAny(chrArray);
				if (num != -1)
				{
					str = context.WordToComplete.Substring(0, num);
					str1 = context.WordToComplete.Substring(num + 1);
				}
			}
			List<CompletionResult> completionResults = new List<CompletionResult>();
			WildcardPattern wildcardPattern = new WildcardPattern(string.Concat(str1, "*"), WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
			CimSession cimSession = CimSession.Create(null);
			using (cimSession)
			{
				foreach (CimInstance cimInstance in cimSession.EnumerateInstances(str, "__Namespace"))
				{
					using (cimInstance)
					{
						if (!context.Helper.CancelTabCompletion)
						{
							CimProperty item = cimInstance.CimInstanceProperties["Name"];
							if (item != null)
							{
								string value = item.Value as string;
								if (value != null)
								{
									if (wildcardPattern.IsMatch(value))
									{
										completionResults.Add(new CompletionResult(string.Concat(str, "/", value), value, CompletionResultType.Namespace, string.Concat(str, "/", value)));
									}
								}
							}
						}
						else
						{
							break;
						}
					}
				}
			}
			List<CompletionResult> completionResults1 = result;
			List<CompletionResult> completionResults2 = completionResults;
			completionResults1.AddRange(completionResults2.OrderBy<CompletionResult, string>((CompletionResult x) => x.ListItemText, StringComparer.OrdinalIgnoreCase));
			result.Add(CompletionResult.Null);
		}

		private static void NativeCompletionCopyMoveItemCommand(string pathName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			if (!string.IsNullOrEmpty(paramName))
			{
				if (paramName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase) || paramName.Equals("Path", StringComparison.OrdinalIgnoreCase))
				{
					CompletionCompleters.NativeCompletionPathArgument(pathName, paramName, result, context);
					return;
				}
				else
				{
					if (paramName.Equals("Destination", StringComparison.OrdinalIgnoreCase))
					{
						CompletionCompleters.RemoveLastNullCompletionResult(result);
						CompletionContext completionContext = context;
						string str = pathName;
						string empty = str;
						if (str == null)
						{
							empty = string.Empty;
						}
						completionContext.WordToComplete = empty;
						bool flag = CompletionCompleters.TurnOnLiteralPathOption(context);
						try
						{
							IEnumerable<CompletionResult> completionResults = CompletionCompleters.CompleteFilename(context);
							if (completionResults != null)
							{
								result.AddRange(completionResults);
							}
						}
						finally
						{
							if (flag)
							{
								context.Options.Remove("LiteralPaths");
							}
						}
						result.Add(CompletionResult.Null);
					}
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionDriveCommands(string wordToComplete, string psProvider, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				string str1 = wordToComplete;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				wordToComplete = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref wordToComplete);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!wordToComplete.EndsWith("*", StringComparison.Ordinal))
				{
					wordToComplete = string.Concat(wordToComplete, "*");
				}
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-PSDrive").AddParameter("Name", wordToComplete);
				if (psProvider != null)
				{
					currentPowerShell.AddParameter("PSProvider", psProvider);
				}
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					foreach (dynamic obj in pSObjects)
					{
						dynamic obj1 = obj.Name;
						dynamic obj2 = obj1;
						if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
						{
							obj1 = str2 + obj1 + str2;
						}
						else
						{
							if (str2 == string.Empty)
							{
								str = "'";
							}
							else
							{
								str = str2;
							}
							string str3 = str;
							if (str3 == "'")
							{
								obj1 = obj1.Replace("'", "''");
							}
							obj1 = str3 + obj1 + str3;
						}
						List<CompletionResult> completionResults = result;
						completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
					}
				}
				result.Add(CompletionResult.Null);
				return;
			}
		}

		private static void NativeCompletionEventLogCommands(string logName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (!string.IsNullOrEmpty(paramName) && paramName.Equals("LogName", StringComparison.OrdinalIgnoreCase))
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				string str1 = logName;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				logName = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref logName);
				if (!logName.EndsWith("*", StringComparison.Ordinal))
				{
					logName = string.Concat(logName, "*");
				}
				WildcardPattern wildcardPattern = new WildcardPattern(logName, WildcardOptions.IgnoreCase);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-EventLog").AddParameter("LogName", "*");
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					foreach (dynamic obj in pSObjects)
					{
						dynamic obj1 = obj.Log.ToString();
						dynamic obj2 = obj1;
						if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
						{
							obj1 = str2 + obj1 + str2;
						}
						else
						{
							if (str2 == string.Empty)
							{
								str = "'";
							}
							else
							{
								str = str2;
							}
							string str3 = str;
							if (str3 == "'")
							{
								obj1 = obj1.Replace("'", "''");
							}
							obj1 = str3 + obj1 + str3;
						}
						if (!wildcardPattern.IsMatch(obj2))
						{
							continue;
						}
						List<CompletionResult> completionResults = result;
                        completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
					}
				}
				result.Add(CompletionResult.Null);
			}
		}

		private static void NativeCompletionGetCommand(string commandName, string moduleName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(paramName) && paramName.Equals("Module", StringComparison.OrdinalIgnoreCase))
				{
					CompletionCompleters.RemoveLastNullCompletionResult(result);
					HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					CompletionContext completionContext = new CompletionContext();
					completionContext.WordToComplete = commandName;
					completionContext.Helper = context.Helper;
					List<CompletionResult> completionResults = CompletionCompleters.CompleteModuleName(completionContext, true);
					if (completionResults != null)
					{
						foreach (CompletionResult completionResult in completionResults)
						{
							if (strs.Contains(completionResult.ToolTip))
							{
								continue;
							}
							strs.Add(completionResult.ToolTip);
							result.Add(completionResult);
						}
					}
					CompletionContext helper = new CompletionContext();
					helper.WordToComplete = commandName;
					helper.Helper = context.Helper;
					completionResults = CompletionCompleters.CompleteModuleName(helper, false);
					if (completionResults != null)
					{
						foreach (CompletionResult completionResult1 in completionResults)
						{
							if (strs.Contains(completionResult1.ToolTip))
							{
								continue;
							}
							strs.Add(completionResult1.ToolTip);
							result.Add(completionResult1);
						}
					}
					result.Add(CompletionResult.Null);
				}
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				CompletionContext completionContext1 = new CompletionContext();
				completionContext1.WordToComplete = commandName;
				completionContext1.Helper = context.Helper;
				List<CompletionResult> completionResults1 = CompletionCompleters.CompleteCommand(completionContext1, moduleName, CommandTypes.All);
				if (completionResults1 != null)
				{
					result.AddRange(completionResults1);
				}
				if (moduleName == null)
				{
					HashSet<string> strs1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					strs1.Add(".ps1");
					HashSet<string> strs2 = strs1;
					CompletionContext helper1 = new CompletionContext();
					helper1.WordToComplete = commandName;
					helper1.Helper = context.Helper;
					List<CompletionResult> completionResults2 = new List<CompletionResult>(CompletionCompleters.CompleteFilename(helper1, false, strs2));
					if (completionResults2.Count > 0)
					{
						result.AddRange(completionResults2);
					}
				}
				result.Add(CompletionResult.Null);
				return;
			}
		}

		private static void NativeCompletionGetHelpCommand(string commandName, string paramName, bool isHelpRelated, List<CompletionResult> result, CompletionContext context)
		{
			if (!string.IsNullOrEmpty(paramName) && paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				CompletionContext completionContext = new CompletionContext();
				completionContext.WordToComplete = commandName;
				completionContext.Helper = context.Helper;
				List<CompletionResult> completionResults = CompletionCompleters.CompleteCommand(completionContext, null, CommandTypes.Alias | CommandTypes.Function | CommandTypes.Cmdlet | CommandTypes.ExternalScript | CommandTypes.Workflow);
				if (completionResults != null)
				{
					result.AddRange(completionResults);
				}
				HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				strs.Add(".ps1");
				HashSet<string> strs1 = strs;
				CompletionContext helper = new CompletionContext();
				helper.WordToComplete = commandName;
				helper.Helper = context.Helper;
				List<CompletionResult> completionResults1 = new List<CompletionResult>(CompletionCompleters.CompleteFilename(helper, false, strs1));
				if (completionResults1.Count > 0)
				{
					result.AddRange(completionResults1);
				}
				if (isHelpRelated)
				{
					CompletionContext completionContext1 = new CompletionContext();
					completionContext1.WordToComplete = commandName;
					completionContext1.Helper = context.Helper;
					List<CompletionResult> completionResults2 = CompletionCompleters.CompleteHelpTopics(completionContext1);
					if (completionResults2 != null)
					{
						result.AddRange(completionResults2);
					}
				}
				result.Add(CompletionResult.Null);
			}
		}

		private static void NativeCompletionJobCommands(string wordToComplete, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (!string.IsNullOrEmpty(paramName))
			{
				string str1 = wordToComplete;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				wordToComplete = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref wordToComplete);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!wordToComplete.EndsWith("*", StringComparison.Ordinal))
				{
					wordToComplete = string.Concat(wordToComplete, "*");
				}
				WildcardPattern wildcardPattern = new WildcardPattern(wordToComplete, WildcardOptions.IgnoreCase);
				if (!paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Job").AddParameter("IncludeChildJob", true);
				}
				else
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Job").AddParameter("Name", wordToComplete);
				}
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					if (!paramName.Equals("Id", StringComparison.OrdinalIgnoreCase))
					{
						if (!paramName.Equals("InstanceId", StringComparison.OrdinalIgnoreCase))
						{
							if (paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
							{
								CompletionCompleters.RemoveLastNullCompletionResult(result);
								foreach (dynamic obj in pSObjects)
								{
									dynamic obj1 = obj.Name;
									dynamic obj2 = obj1;
									if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
									{
										obj1 = str2 + obj1 + str2;
									}
									else
									{
										if (str2 == string.Empty)
										{
											str = "'";
										}
										else
										{
											str = str2;
										}
										string str3 = str;
										if (str3 == "'")
										{
											obj1 = obj1.Replace("'", "''");
										}
										obj1 = str3 + obj1 + str3;
									}
									List<CompletionResult> completionResults = result;
                                    completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
								}
								result.Add(CompletionResult.Null);
							}
							return;
						}
						else
						{
							CompletionCompleters.RemoveLastNullCompletionResult(result);
							foreach (dynamic obj3 in pSObjects)
							{
								dynamic obj4 = obj3.InstanceId.ToString();
								if (!wildcardPattern.IsMatch(obj4))
								{
									continue;
								}
								dynamic obj5 = obj4;
								obj4 = str2 + obj4 + str2;
								List<CompletionResult> completionResults1 = result;
                                completionResults1.Add(new CompletionResult(obj4, obj5, (CompletionResultType)8, obj5));
							}
							result.Add(CompletionResult.Null);
							return;
						}
					}
					else
					{
						CompletionCompleters.RemoveLastNullCompletionResult(result);
						foreach (dynamic obj6 in pSObjects)
						{
							dynamic obj7 = obj6.Id.ToString();
							if (!wildcardPattern.IsMatch(obj7))
							{
								continue;
							}
							dynamic obj8 = obj7;
							obj7 = str2 + obj7 + str2;
							List<CompletionResult> completionResults2 = result;
                            completionResults2.Add(new CompletionResult(obj7, obj8, (CompletionResultType)8, obj8));
						}
						result.Add(CompletionResult.Null);
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionMemberName(string wordToComplete, List<CompletionResult> result, CommandAst commandAst, CompletionContext context)
		{
			AstParameterArgumentPair astParameterArgumentPair = null;
			PipelineAst parent = commandAst.Parent as PipelineAst;
			if (parent != null)
			{
				int num = 0;
				while (num < parent.PipelineElements.Count && parent.PipelineElements[num] != commandAst)
				{
					num++;
				}
				IEnumerable<PSTypeName> inferredType = null;
				if (num != 0)
				{
					inferredType = parent.PipelineElements[num - 1].GetInferredType(context);
				}
				else
				{
					if (!context.PseudoBindingInfo.BoundArguments.TryGetValue("InputObject", out astParameterArgumentPair) || !astParameterArgumentPair.ArgumentSpecified)
					{
						return;
					}
					else
					{
						AstPair astPair = astParameterArgumentPair as AstPair;
						if (astPair == null || astPair.Argument == null)
						{
							return;
						}
						else
						{
							inferredType = astPair.Argument.GetInferredType(context);
						}
					}
				}
				CompletionCompleters.CompleteMemberByInferredType(context, inferredType, result, string.Concat(wordToComplete, "*"), true);
				result.Add(CompletionResult.Null);
				return;
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionModuleCommands(string assemblyOrModuleName, string paramName, bool loadedModulesOnly, bool isImportModule, List<CompletionResult> result, CompletionContext context)
		{
			if (!string.IsNullOrEmpty(paramName))
			{
				if (!paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
				{
					if (paramName.Equals("Assembly", StringComparison.OrdinalIgnoreCase))
					{
						CompletionCompleters.RemoveLastNullCompletionResult(result);
						HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						strs.Add(".dll");
						HashSet<string> strs1 = strs;
						CompletionContext completionContext = new CompletionContext();
						completionContext.WordToComplete = assemblyOrModuleName;
						completionContext.Helper = context.Helper;
						List<CompletionResult> completionResults = new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext, false, strs1));
						if (completionResults.Count > 0)
						{
							result.AddRange(completionResults);
						}
						result.Add(CompletionResult.Null);
					}
					return;
				}
				else
				{
					CompletionCompleters.RemoveLastNullCompletionResult(result);
					if (isImportModule)
					{
						HashSet<string> strs2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						strs2.Add(".ps1");
						strs2.Add(".psm1");
						strs2.Add(".psd1");
						strs2.Add(".dll");
						strs2.Add(".cdxml");
						strs2.Add(".xaml");
						HashSet<string> strs3 = strs2;
						CompletionContext helper = new CompletionContext();
						helper.WordToComplete = assemblyOrModuleName;
						helper.Helper = context.Helper;
						List<CompletionResult> completionResults1 = new List<CompletionResult>(CompletionCompleters.CompleteFilename(helper, false, strs3));
						if (completionResults1.Count > 0)
						{
							result.AddRange(completionResults1);
						}
						char[] chrArray = new char[] { '\\', '/', ':' };
						if (assemblyOrModuleName.IndexOfAny(chrArray) != -1)
						{
							return;
						}
					}
					CompletionContext completionContext1 = new CompletionContext();
					completionContext1.WordToComplete = assemblyOrModuleName;
					completionContext1.Helper = context.Helper;
					List<CompletionResult> completionResults2 = CompletionCompleters.CompleteModuleName(completionContext1, loadedModulesOnly);
					if (completionResults2 != null && completionResults2.Count > 0)
					{
						result.AddRange(completionResults2);
					}
					result.Add(CompletionResult.Null);
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionPathArgument(string pathName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase) && !paramName.Equals("Path", StringComparison.OrdinalIgnoreCase) && !paramName.Equals("FilePath", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				CompletionContext completionContext = context;
				string str = pathName;
				string empty = str;
				if (str == null)
				{
					empty = string.Empty;
				}
				completionContext.WordToComplete = empty;
				bool flag = false;
				if (paramName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase))
				{
					flag = CompletionCompleters.TurnOnLiteralPathOption(context);
				}
				try
				{
					IEnumerable<CompletionResult> completionResults = CompletionCompleters.CompleteFilename(context);
					if (completionResults != null)
					{
						result.AddRange(completionResults);
					}
				}
				finally
				{
					if (flag)
					{
						context.Options.Remove("LiteralPaths");
					}
				}
				result.Add(CompletionResult.Null);
				return;
			}
		}

		private static void NativeCompletionProcessCommands(string wordToComplete, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (!string.IsNullOrEmpty(paramName))
			{
				string str1 = wordToComplete;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				wordToComplete = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref wordToComplete);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!wordToComplete.EndsWith("*", StringComparison.Ordinal))
				{
					wordToComplete = string.Concat(wordToComplete, "*");
				}
				if (!paramName.Equals("Id", StringComparison.OrdinalIgnoreCase))
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Process").AddParameter("Name", wordToComplete);
				}
				else
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Process");
				}
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					if (!paramName.Equals("Id", StringComparison.OrdinalIgnoreCase))
					{
						if (paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
						{
							CompletionCompleters.RemoveLastNullCompletionResult(result);
							HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
							foreach (dynamic obj in pSObjects)
							{
								dynamic obj1 = obj.Name;
								dynamic obj2 = obj1;
								if (strs.Contains(obj1))
								{
									continue;
								}
								strs.Add(obj1);
								if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
								{
									obj1 = str2 + obj1 + str2;
								}
								else
								{
									if (str2 == string.Empty)
									{
										str = "'";
									}
									else
									{
										str = str2;
									}
									string str3 = str;
									if (str3 == "'")
									{
										obj1 = obj1.Replace("'", "''");
									}
									obj1 = str3 + obj1 + str3;
								}
								List<CompletionResult> completionResults = result;
                                completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
							}
							result.Add(CompletionResult.Null);
						}
						return;
					}
					else
					{
						CompletionCompleters.RemoveLastNullCompletionResult(result);
						WildcardPattern wildcardPattern = new WildcardPattern(wordToComplete, WildcardOptions.IgnoreCase);
						foreach (dynamic obj3 in pSObjects)
						{
							dynamic obj4 = obj3.Id.ToString();
							if (!wildcardPattern.IsMatch(obj4))
							{
								continue;
							}
							dynamic obj5 = obj4;
							obj4 = str2 + obj4 + str2;
							List<CompletionResult> completionResults1 = result;
							completionResults1.Add(new CompletionResult(obj4, obj5, (CompletionResultType)8, obj5));
						}
						result.Add(CompletionResult.Null);
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionProviderCommands(string providerName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("PSProvider", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				string str1 = providerName;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				providerName = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref providerName);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!providerName.EndsWith("*", StringComparison.Ordinal))
				{
					providerName = string.Concat(providerName, "*");
				}
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-PSProvider").AddParameter("PSProvider", providerName);
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					foreach (dynamic obj in pSObjects)
					{
						dynamic obj1 = obj.Name;
						dynamic obj2 = obj1;
						if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
						{
							obj1 = str2 + obj1 + str2;
						}
						else
						{
							if (str2 == string.Empty)
							{
								str = "'";
							}
							else
							{
								str = str2;
							}
							string str3 = str;
							if (str3 == "'")
							{
								obj1 = obj1.Replace("'", "''");
							}
							obj1 = str3 + obj1 + str3;
						}
						List<CompletionResult> completionResults = result;
                        completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
					}
					result.Add(CompletionResult.Null);
					return;
				}
				else
				{
					return;
				}
			}
		}

		private static void NativeCompletionScheduledJobCommands(string wordToComplete, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (!string.IsNullOrEmpty(paramName))
			{
				string str1 = wordToComplete;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				wordToComplete = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref wordToComplete);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!wordToComplete.EndsWith("*", StringComparison.Ordinal))
				{
					wordToComplete = string.Concat(wordToComplete, "*");
				}
				WildcardPattern wildcardPattern = new WildcardPattern(wordToComplete, WildcardOptions.IgnoreCase);
				if (!paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-ScheduledJob");
				}
				else
				{
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-ScheduledJob").AddParameter("Name", wordToComplete);
				}
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					if (!paramName.Equals("Id", StringComparison.OrdinalIgnoreCase))
					{
						if (paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
						{
							CompletionCompleters.RemoveLastNullCompletionResult(result);
							foreach (dynamic obj in pSObjects)
							{
								dynamic obj1 = obj.Name;
								dynamic obj2 = obj1;
								if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
								{
									obj1 = str2 + obj1 + str2;
								}
								else
								{
									if (str2 == string.Empty)
									{
										str = "'";
									}
									else
									{
										str = str2;
									}
									string str3 = str;
									if (str3 == "'")
									{
										obj1 = obj1.Replace("'", "''");
									}
									obj1 = str3 + obj1 + str3;
								}
								List<CompletionResult> completionResults = result;
                                completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
							}
							result.Add(CompletionResult.Null);
						}
						return;
					}
					else
					{
						CompletionCompleters.RemoveLastNullCompletionResult(result);
						foreach (dynamic obj3 in pSObjects)
						{
							dynamic obj4 = obj3.Id.ToString();
							if (!wildcardPattern.IsMatch(obj4))
							{
								continue;
							}
							dynamic obj5 = obj4;
							obj4 = str2 + obj4 + str2;
							List<CompletionResult> completionResults1 = result;
                            completionResults1.Add(new CompletionResult(obj4, obj5, (CompletionResultType)8, obj5));
						}
						result.Add(CompletionResult.Null);
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionServiceCommands(string wordToComplete, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			string str1;
			if (!string.IsNullOrEmpty(paramName))
			{
				string str2 = wordToComplete;
				string empty = str2;
				if (str2 == null)
				{
					empty = string.Empty;
				}
				wordToComplete = empty;
				string str3 = CompletionCompleters.HandleDoubleAndSingleQuote(ref wordToComplete);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!wordToComplete.EndsWith("*", StringComparison.Ordinal))
				{
					wordToComplete = string.Concat(wordToComplete, "*");
				}
				if (!paramName.Equals("DisplayName", StringComparison.OrdinalIgnoreCase))
				{
					if (paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
					{
						CompletionCompleters.RemoveLastNullCompletionResult(result);
						CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Service").AddParameter("Name", wordToComplete);
						Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
						if (pSObjects != null)
						{
							foreach (dynamic obj in pSObjects)
							{
								dynamic obj1 = obj.Name;
								dynamic obj2 = obj1;
								if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
								{
									obj1 = str3 + obj1 + str3;
								}
								else
								{
									if (str3 == string.Empty)
									{
										str = "'";
									}
									else
									{
										str = str3;
									}
									string str4 = str;
									if (str4 == "'")
									{
										obj1 = obj1.Replace("'", "''");
									}
									obj1 = str4 + obj1 + str4;
								}
								List<CompletionResult> completionResults = result;
                                completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
							}
						}
						result.Add(CompletionResult.Null);
					}
					return;
				}
				else
				{
					CompletionCompleters.RemoveLastNullCompletionResult(result);
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Service").AddParameter("DisplayName", wordToComplete);
					CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Sort-Object").AddParameter("Property", "DisplayName");
					Collection<PSObject> pSObjects1 = context.Helper.ExecuteCurrentPowerShell(out exception, null);
					if (pSObjects1 != null)
					{
						foreach (dynamic obj3 in pSObjects1)
						{
							dynamic obj4 = obj3.DisplayName;
							dynamic obj5 = obj4;
							if (!CompletionCompleters.CompletionRequiresQuotes(obj4, false))
							{
								obj4 = str3 + obj4 + str3;
							}
							else
							{
								if (str3 == string.Empty)
								{
									str1 = "'";
								}
								else
								{
									str1 = str3;
								}
								string str5 = str1;
								if (str5 == "'")
								{
									obj4 = obj4.Replace("'", "''");
								}
								obj4 = str5 + obj4 + str5;
							}
							List<CompletionResult> completionResults1 = result;
                            completionResults1.Add(new CompletionResult(obj4, obj5, (CompletionResultType)8, obj5));
						}
					}
					result.Add(CompletionResult.Null);
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void NativeCompletionSetLocationCommand(string dirName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("Path", StringComparison.OrdinalIgnoreCase) && !paramName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				CompletionContext completionContext = context;
				string str = dirName;
				string empty = str;
				if (str == null)
				{
					empty = string.Empty;
				}
				completionContext.WordToComplete = empty;
				bool flag = false;
				if (paramName.Equals("LiteralPath", StringComparison.OrdinalIgnoreCase))
				{
					flag = CompletionCompleters.TurnOnLiteralPathOption(context);
				}
				try
				{
					IEnumerable<CompletionResult> completionResults = CompletionCompleters.CompleteFilename(context, true, null);
					if (completionResults != null)
					{
						result.AddRange(completionResults);
					}
				}
				finally
				{
					if (flag)
					{
						context.Options.Remove("LiteralPaths");
					}
				}
				result.Add(CompletionResult.Null);
				return;
			}
		}

		private static void NativeCompletionTraceSourceCommands(string traceSourceName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			string str;
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				string str1 = traceSourceName;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				traceSourceName = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref traceSourceName);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!traceSourceName.EndsWith("*", StringComparison.Ordinal))
				{
					traceSourceName = string.Concat(traceSourceName, "*");
				}
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-TraceSource").AddParameter("Name", traceSourceName);
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					foreach (dynamic obj in pSObjects)
					{
						dynamic obj1 = obj.Name;
						dynamic obj2 = obj1;
						if (!CompletionCompleters.CompletionRequiresQuotes(obj1, false))
						{
							obj1 = str2 + obj1 + str2;
						}
						else
						{
							if (str2 == string.Empty)
							{
								str = "'";
							}
							else
							{
								str = str2;
							}
							string str3 = str;
							if (str3 == "'")
							{
								obj1 = obj1.Replace("'", "''");
							}
							obj1 = str3 + obj1 + str3;
						}
						List<CompletionResult> completionResults = result;
                        completionResults.Add(new CompletionResult(obj1, obj2, (CompletionResultType)8, obj2));
					}
					result.Add(CompletionResult.Null);
					return;
				}
				else
				{
					return;
				}
			}
		}

		private static void NativeCompletionTypeName(CompletionContext context, List<CompletionResult> result)
		{
			bool flag;
			int num;
			bool flag1;
			int num1;
			string wordToComplete = context.WordToComplete;
			if (wordToComplete.Length <= 0)
			{
				flag = false;
			}
			else
			{
				if (wordToComplete[0].IsSingleQuote())
				{
					flag = true;
				}
				else
				{
					flag = wordToComplete[0].IsDoubleQuote();
				}
			}
			bool flag2 = flag;
			string str = "";
			string str1 = "";
			if (flag2)
			{
				string str2 = wordToComplete.Substring(0, 1);
				str1 = str2;
				str = str2;
				if (wordToComplete.Length <= 1)
				{
					flag1 = false;
				}
				else
				{
					flag1 = wordToComplete[wordToComplete.Length - 1] == wordToComplete[0];
				}
				bool flag3 = flag1;
				string str3 = wordToComplete;
				int num2 = 1;
				int length = wordToComplete.Length;
				if (flag3)
				{
					num1 = 2;
				}
				else
				{
					num1 = 1;
				}
				wordToComplete = str3.Substring(num2, length - num1);
			}
			if (wordToComplete.IndexOf('[') != -1)
			{
				InternalScriptPosition cursorPosition = (InternalScriptPosition)context.CursorPosition;
				InternalScriptPosition internalScriptPosition = cursorPosition;
				int offset = cursorPosition.Offset - context.TokenAtCursor.Extent.StartOffset;
				if (flag2)
				{
					num = 1;
				}
				else
				{
					num = 0;
				}
				cursorPosition = internalScriptPosition.CloneWithNewOffset(offset - num);
				ITypeName typeName = Parser.ScanType(wordToComplete, true);
				TypeName complete = CompletionAnalysis.FindTypeNameToComplete(typeName, cursorPosition);
				if (complete != null)
				{
					int num3 = 0;
					int num4 = 0;
					string str4 = wordToComplete;
					for (int i = 0; i < str4.Length; i++)
					{
						char chr = str4[i];
						if (chr != '[')
						{
							if (chr == ']')
							{
								num4++;
							}
						}
						else
						{
							num3++;
						}
					}
					wordToComplete = complete.FullName;
					string text = typeName.Extent.Text;
					if (!flag2)
					{
						string str5 = "'";
						str1 = str5;
						str = str5;
					}
					if (num4 < num3)
					{
						str1 = str1.Insert(0, new string(']', num3 - num4));
					}
					if (!flag2 || num4 != num3)
					{
						str = string.Concat(str, text.Substring(0, complete.Extent.StartOffset));
						str1 = str1.Insert(0, text.Substring(complete.Extent.EndOffset));
					}
					else
					{
						context.ReplacementIndex = complete.Extent.StartOffset + context.TokenAtCursor.Extent.StartOffset + 1;
						context.ReplacementLength = wordToComplete.Length;
						string str6 = "";
						str1 = str6;
						str = str6;
					}
				}
				else
				{
					return;
				}
			}
			context.WordToComplete = wordToComplete;
			List<CompletionResult> completionResults = CompletionCompleters.CompleteType(context, str, str1);
			if (completionResults != null)
			{
				result.AddRange(completionResults);
			}
			result.Add(CompletionResult.Null);
		}

		private static void NativeCompletionVariableCommands(string variableName, string paramName, List<CompletionResult> result, CompletionContext context)
		{
			Exception exception = null;
			dynamic obj;
			string str;
			if (string.IsNullOrEmpty(paramName) || !paramName.Equals("Name", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else
			{
				CompletionCompleters.RemoveLastNullCompletionResult(result);
				string str1 = variableName;
				string empty = str1;
				if (str1 == null)
				{
					empty = string.Empty;
				}
				variableName = empty;
				string str2 = CompletionCompleters.HandleDoubleAndSingleQuote(ref variableName);
				PowerShell currentPowerShell = context.Helper.CurrentPowerShell;
				if (!variableName.EndsWith("*", StringComparison.Ordinal))
				{
					variableName = string.Concat(variableName, "*");
				}
				CompletionCompleters.AddCommandWithPreferenceSetting(currentPowerShell, "Get-Variable").AddParameter("Name", variableName);
				Collection<PSObject> pSObjects = context.Helper.ExecuteCurrentPowerShell(out exception, null);
				if (pSObjects != null)
				{
					foreach (dynamic obj1 in pSObjects)
					{
						string str3 = str2;
						dynamic obj2 = obj1.Name;
						dynamic obj3 = obj2;
						char[] chrArray = new char[2];
						chrArray[0] = '?';
						chrArray[1] = '*';
						if (obj2.IndexOfAny(chrArray) != -1)
						{
							str3 = "'";
							obj2 = obj2.Replace("?", "`?");
							obj2 = obj2.Replace("*", "`*");
						}
						dynamic obj4 = !obj2.Equals("$", 4);
						if (!obj4)
						{
							obj = obj4;
						}
						else
						{
							dynamic obj5 = obj4;
							obj = obj5 & CompletionCompleters.CompletionRequiresQuotes(obj2, false);
						}
						if (!obj)
						{
							obj2 = str3 + obj2 + str3;
						}
						else
						{
							if (str3 == string.Empty)
							{
								str = "'";
							}
							else
							{
								str = str3;
							}
							string str4 = str;
							if (str4 == "'")
							{
								obj2 = obj2.Replace("'", "''");
							}
							obj2 = str4 + obj2 + str4;
						}
						List<CompletionResult> completionResults = result;
                        completionResults.Add(new CompletionResult(obj2, obj3, (CompletionResultType)8, obj3));
					}
					result.Add(CompletionResult.Null);
					return;
				}
				else
				{
					return;
				}
			}
		}

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int NetShareEnum(string serverName, int level, out IntPtr bufptr, int prefMaxLen, out int entriesRead, out int totalEntries, ref int resumeHandle);

        private static void ProcessParameter(string commandName, CommandAst commandAst, CompletionContext context, List<CompletionResult> result, MergedCompiledCommandParameter parameter, Dictionary<string, AstParameterArgumentPair> boundArguments = null)
        {
            CompletionResult item = null;
            Type effectiveParameterType = GetEffectiveParameterType(parameter.Parameter.Type);
            if (effectiveParameterType.IsArray)
            {
                effectiveParameterType = effectiveParameterType.GetElementType();
            }
            if (effectiveParameterType.IsEnum)
            {
                RemoveLastNullCompletionResult(result);
                string str = LanguagePrimitives.EnumSingleTypeConverter.EnumValues(effectiveParameterType);
                string listSeparator = ExtendedTypeSystem.ListSeparator;
                string[] strArray = str.Split(new string[] { listSeparator }, StringSplitOptions.RemoveEmptyEntries);
                string wordToComplete = context.WordToComplete;
                string quote = HandleDoubleAndSingleQuote(ref wordToComplete);
                WildcardPattern pattern = new WildcardPattern(wordToComplete + "*", WildcardOptions.IgnoreCase);
                List<string> list = new List<string>();
                foreach (string str4 in strArray)
                {
                    if (wordToComplete.Equals(str4, StringComparison.OrdinalIgnoreCase))
                    {
                        string completionText = (quote == string.Empty) ? str4 : (quote + str4 + quote);
                        item = new CompletionResult(completionText, str4, CompletionResultType.ParameterValue, str4);
                    }
                    else if (pattern.IsMatch(str4))
                    {
                        list.Add(str4);
                    }
                }
                if (item != null)
                {
                    result.Add(item);
                }
                list.Sort();
                result.AddRange(from entry in list
                                let completionText = (quote == string.Empty) ? (entry) : ((quote + entry + quote))
                                select new CompletionResult(completionText, entry, CompletionResultType.ParameterValue, entry));
                result.Add(CompletionResult.Null);
            }
            else if (effectiveParameterType.Equals(typeof(SwitchParameter)))
            {
                RemoveLastNullCompletionResult(result);
                if ((context.WordToComplete == string.Empty) || context.WordToComplete.Equals("$", StringComparison.Ordinal))
                {
                    result.Add(new CompletionResult("$true", "$true", CompletionResultType.ParameterValue, "$true"));
                    result.Add(new CompletionResult("$false", "$false", CompletionResultType.ParameterValue, "$false"));
                }
                result.Add(CompletionResult.Null);
            }
            else
            {
                foreach (ValidateArgumentsAttribute attribute in parameter.Parameter.ValidationAttributes)
                {
                    if (attribute is ValidateSetAttribute)
                    {
                        RemoveLastNullCompletionResult(result);
                        ValidateSetAttribute attribute2 = (ValidateSetAttribute)attribute;
                        string str6 = context.WordToComplete;
                        string quote = HandleDoubleAndSingleQuote(ref str6);
                        WildcardPattern pattern2 = new WildcardPattern(str6 + "*", WildcardOptions.IgnoreCase);
                        List<string> list2 = new List<string>();
                        foreach (string str7 in attribute2.ValidValues)
                        {
                            if (str6.Equals(str7, StringComparison.OrdinalIgnoreCase))
                            {
                                string str8 = (quote == string.Empty) ? str7 : (quote + str7 + quote);
                                item = new CompletionResult(str8, str7, CompletionResultType.ParameterValue, str7);
                            }
                            else if (pattern2.IsMatch(str7))
                            {
                                list2.Add(str7);
                            }
                        }
                        if (item != null)
                        {
                            result.Add(item);
                        }
                        list2.Sort();
                        result.AddRange(from entry in list2
                                        let completionText = (quote == string.Empty) ? (entry) : ((quote + entry + quote))
                                        select new CompletionResult(completionText, entry, CompletionResultType.ParameterValue, entry));
                        result.Add(CompletionResult.Null);
                        return;
                    }
                }
                NativeCommandArgumentCompletion(commandName, parameter.Parameter.Name, result, commandAst, context, boundArguments);
            }
        }
		private static bool ProviderSpecified(string path)
		{
			int num = path.IndexOf(':');
			if (num == -1 || num + 1 >= path.Length)
			{
				return false;
			}
			else
			{
				return path[num + 1] == ':';
			}
		}

		private static void RemoveLastNullCompletionResult(List<CompletionResult> result)
		{
			if (result.Count > 0 && result[result.Count - 1].Equals(CompletionResult.Null))
			{
				result.RemoveAt(result.Count - 1);
			}
		}

		private static bool TryCustomArgumentCompletion(string optionKey, IEnumerable<string> keys, object[] argumentsToCompleter, CompletionContext context, List<CompletionResult> result)
		{
			Hashtable options = context.Options;
			if (options == null || !options.ContainsKey(optionKey))
			{
				return false;
			}
			else
			{
				Hashtable item = options[optionKey] as Hashtable;
				if (item != null)
				{
					ScriptBlock scriptBlock = null;
					IEnumerator<string> enumerator = keys.GetEnumerator();
					using (enumerator)
					{
						do
						{
						Label0:
							if (!enumerator.MoveNext())
							{
								break;
							}
							string current = enumerator.Current;
							if (item.ContainsKey(current))
							{
								scriptBlock = item[current] as ScriptBlock;
							}
							else
							{
								goto Label0;
							}
						}
						while (scriptBlock == null);
					}
					if (scriptBlock != null)
					{
						Collection<PSObject> pSObjects = null;
						try
						{
							pSObjects = scriptBlock.Invoke(argumentsToCompleter);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							CommandProcessorBase.CheckForSevereException(exception);
						}
						if (pSObjects == null || !pSObjects.Any<PSObject>())
						{
							return false;
						}
						else
						{
							foreach (PSObject pSObject in pSObjects)
							{
								CompletionResult baseObject = pSObject.BaseObject as CompletionResult;
								if (baseObject == null)
								{
									string str = pSObject.ToString();
									result.Add(new CompletionResult(str));
								}
								else
								{
									result.Add(baseObject);
								}
							}
							result.Add(CompletionResult.Null);
							return true;
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
		}

		private static bool TurnOnLiteralPathOption(CompletionContext completionContext)
		{
			bool flag = false;
			if (completionContext.Options != null)
			{
				if (!completionContext.Options.ContainsKey("LiteralPaths"))
				{
					completionContext.Options.Add("LiteralPaths", true);
					flag = true;
				}
			}
			else
			{
				Hashtable hashtables = new Hashtable();
				hashtables.Add("LiteralPaths", true);
				completionContext.Options = hashtables;
				flag = true;
			}
			return flag;
		}

		private static void UpdateTypeCacheOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			Interlocked.Exchange<CompletionCompleters.TypeCompletionMapping[][]>(ref CompletionCompleters.typeCache, null);
		}

		private sealed class ArgumentLocation
		{
			internal AstParameterArgumentPair Argument
			{
				get;
				set;
			}

			internal bool IsPositional
			{
				get;
				set;
			}

			internal int Position
			{
				get;
				set;
			}

			public ArgumentLocation()
			{
			}
		}

		private class CommandNameComparer : IComparer<PSObject>
		{
			public CommandNameComparer()
			{
			}

			public int Compare(PSObject x, PSObject y)
			{
				string name;
				string str;
				object obj = PSObject.Base(x);
				object obj1 = PSObject.Base(y);
				CommandInfo commandInfo = obj as CommandInfo;
				if (commandInfo != null)
				{
					name = commandInfo.Name;
				}
				else
				{
					name = obj as string;
				}
				string str1 = name;
				CommandInfo commandInfo1 = obj1 as CommandInfo;
				if (commandInfo1 != null)
				{
					str = commandInfo1.Name;
				}
				else
				{
					str = obj1 as string;
				}
				string str2 = str;
				if (str1 != null)
				{
				}
				return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase);
			}
		}

		private class FindFunctionsVisitor : AstVisitor
		{
			internal readonly List<FunctionDefinitionAst> FunctionDefinitions;

			public FindFunctionsVisitor()
			{
				this.FunctionDefinitions = new List<FunctionDefinitionAst>();
			}

			public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
			{
				this.FunctionDefinitions.Add(functionDefinitionAst);
				return AstVisitAction.Continue;
			}
		}

		private class FindVariablesVisitor : AstVisitor
		{
			internal Ast Top;

			internal Ast CompletionVariableAst;

			internal readonly List<VariableExpressionAst> Variables;

			public FindVariablesVisitor()
			{
				this.Variables = new List<VariableExpressionAst>();
			}

			public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
			{
				if (functionDefinitionAst != this.Top)
				{
					return AstVisitAction.SkipChildren;
				}
				else
				{
					return AstVisitAction.Continue;
				}
			}

			public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
			{
				if (scriptBlockAst != this.Top)
				{
					return AstVisitAction.SkipChildren;
				}
				else
				{
					return AstVisitAction.Continue;
				}
			}

			public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
			{
				if (scriptBlockExpressionAst != this.Top)
				{
					return AstVisitAction.SkipChildren;
				}
				else
				{
					return AstVisitAction.Continue;
				}
			}

			public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
			{
				if (variableExpressionAst != this.CompletionVariableAst)
				{
					this.Variables.Add(variableExpressionAst);
				}
				return AstVisitAction.Continue;
			}
		}

		private class GenericTypeCompletion : CompletionCompleters.TypeCompletion
		{
			public GenericTypeCompletion()
			{
			}

			internal override CompletionResult GetCompletionResult(string keyMatched, string prefix, string suffix)
			{
				string fullName = this.Type.FullName;
				int num = fullName.LastIndexOf('\u0060');
				if (num != -1)
				{
					fullName = fullName.Substring(0, num);
				}
				string name = this.Type.Name;
				num = name.LastIndexOf('\u0060');
				if (num != -1)
				{
					name = name.Substring(0, num);
				}
				string str = string.Concat(name, "<>");
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.GetTooltipPrefix());
				if (!string.IsNullOrEmpty(this.Type.Namespace))
				{
					stringBuilder.Append(this.Type.Namespace);
					stringBuilder.Append('.');
				}
				stringBuilder.Append(name);
				stringBuilder.Append('[');
				Type[] genericArguments = this.Type.GetGenericArguments();
				for (int i = 0; i < (int)genericArguments.Length; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(genericArguments[i].Name);
				}
				stringBuilder.Append(']');
				return new CompletionResult(string.Concat(prefix, fullName, suffix), str, CompletionResultType.Type, stringBuilder.ToString());
			}
		}

		private class ItemPathComparer : IComparer<PSObject>
		{
			public ItemPathComparer()
			{
			}

			public int Compare(PSObject x, PSObject y)
			{
				PathInfo pathInfo = PSObject.Base(x) as PathInfo;
				FileSystemInfo fileSystemInfo = PSObject.Base(x) as FileSystemInfo;
				string str = PSObject.Base(x) as string;
				PathInfo pathInfo1 = PSObject.Base(y) as PathInfo;
				FileSystemInfo fileSystemInfo1 = PSObject.Base(y) as FileSystemInfo;
				string str1 = PSObject.Base(y) as string;
				string fullName = null;
				string providerPath = null;
				if (pathInfo == null)
				{
					if (fileSystemInfo == null)
					{
						if (str != null)
						{
							fullName = str;
						}
					}
					else
					{
						fullName = fileSystemInfo.FullName;
					}
				}
				else
				{
					fullName = pathInfo.ProviderPath;
				}
				if (pathInfo1 == null)
				{
					if (fileSystemInfo1 == null)
					{
						if (str1 != null)
						{
							providerPath = str1;
						}
					}
					else
					{
						providerPath = fileSystemInfo1.FullName;
					}
				}
				else
				{
					providerPath = pathInfo1.ProviderPath;
				}
				if (!string.IsNullOrEmpty(fullName))
				{
					string.IsNullOrEmpty(providerPath);
				}
				return string.Compare(fullName, providerPath, StringComparison.CurrentCultureIgnoreCase);
			}
		}

		private class NamespaceCompletion : CompletionCompleters.TypeCompletionBase
		{
			internal string Namespace;

			public NamespaceCompletion()
			{
			}

			internal override CompletionResult GetCompletionResult(string keyMatched, string prefix, string suffix)
			{
				string @namespace = this.Namespace;
				int num = @namespace.LastIndexOf('.');
				if (num != -1)
				{
					@namespace = @namespace.Substring(num + 1);
				}
				return new CompletionResult(string.Concat(prefix, this.Namespace, suffix), @namespace, CompletionResultType.Namespace, string.Concat("Namespace ", this.Namespace));
			}
		}

		private struct SHARE_INFO_1
		{
			public string netname;

			public int type;

			public string remark;

		}

		private class TypeCompletion : CompletionCompleters.TypeCompletionBase
		{
			internal Type Type;

			public TypeCompletion()
			{
			}

			internal override CompletionResult GetCompletionResult(string keyMatched, string prefix, string suffix)
			{
				string fullName = ToStringCodeMethods.Type(this.Type, false);
				if (keyMatched.IndexOf('.') != -1 && fullName.IndexOf('.') == -1)
				{
					fullName = this.Type.FullName;
				}
				string name = this.Type.Name;
				string str = string.Concat(this.GetTooltipPrefix(), this.Type.FullName);
				return new CompletionResult(string.Concat(prefix, fullName, suffix), name, CompletionResultType.Type, str);
			}

			protected string GetTooltipPrefix()
			{
				if (!typeof(Delegate).IsAssignableFrom(this.Type))
				{
					if (!this.Type.IsInterface)
					{
						if (!this.Type.IsClass)
						{
							if (!this.Type.IsEnum)
							{
								if (!typeof(ValueType).IsAssignableFrom(this.Type))
								{
									return "";
								}
								else
								{
									return "Struct ";
								}
							}
							else
							{
								return "Enum ";
							}
						}
						else
						{
							return "Class ";
						}
					}
					else
					{
						return "Interface ";
					}
				}
				else
				{
					return "Delegate ";
				}
			}
		}

		private abstract class TypeCompletionBase
		{
			protected TypeCompletionBase()
			{
			}

			internal abstract CompletionResult GetCompletionResult(string keyMatched, string prefix, string suffix);
		}

		private class TypeCompletionMapping
		{
			internal string Key;

			internal List<CompletionCompleters.TypeCompletionBase> Completions;

			public TypeCompletionMapping()
			{
				this.Completions = new List<CompletionCompleters.TypeCompletionBase>();
			}
		}
	}
}