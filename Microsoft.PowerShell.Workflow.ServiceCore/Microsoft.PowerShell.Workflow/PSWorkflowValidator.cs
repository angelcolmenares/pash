using Microsoft.PowerShell.Activities;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Activities.Validation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Tracing;
using System.Reflection;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	public class PSWorkflowValidator
	{
		private readonly static string Facility;

		private readonly static PowerShellTraceSource Tracer;

		private readonly static Tracer _structuredTracer;

		private readonly ConcurrentDictionary<Guid, PSWorkflowValidationResults> _validationCache;

		private readonly static List<string> AllowedSystemActivities;

		private static HashSet<string> PowerShellActivitiesAssemblies;

		internal static bool TestMode;

		internal static long ObjectCounter;

		internal PSWorkflowConfigurationProvider Configuration
		{
			get;
			private set;
		}

		static PSWorkflowValidator()
		{
			PSWorkflowValidator.Facility = "WorkflowValidation : ";
			PSWorkflowValidator.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			PSWorkflowValidator._structuredTracer = new Tracer();
			List<string> strs = new List<string>();
			strs.Add("DynamicActivity");
			strs.Add("DoWhile");
			strs.Add("ForEach`1");
			strs.Add("If");
			strs.Add("Parallel");
			strs.Add("ParallelForEach`1");
			strs.Add("Sequence");
			strs.Add("Switch`1");
			strs.Add("While");
			strs.Add("Assign");
			strs.Add("Assign`1");
			strs.Add("Delay");
			strs.Add("InvokeMethod");
			strs.Add("TerminateWorkflow");
			strs.Add("WriteLine");
			strs.Add("Rethrow");
			strs.Add("Throw");
			strs.Add("TryCatch");
			strs.Add("Literal`1");
			strs.Add("VisualBasicValue`1");
			strs.Add("VisualBasicReference`1");
			strs.Add("LocationReferenceValue`1");
			strs.Add("VariableValue`1");
			strs.Add("VariableReference`1");
			strs.Add("LocationReferenceReference`1");
			strs.Add("LambdaValue`1");
			strs.Add("Flowchart");
			strs.Add("FlowDecision");
			strs.Add("FlowSwitch`1");
			strs.Add("AddToCollection`1");
			strs.Add("ExistsInCollection`1");
			strs.Add("RemoveFromCollection`1");
			strs.Add("ClearCollection`1");
			PSWorkflowValidator.AllowedSystemActivities = strs;
			HashSet<string> strs1 = new HashSet<string>();
			strs1.Add("microsoft.powershell.activities");
			strs1.Add("microsoft.powershell.core.activities");
			strs1.Add("microsoft.powershell.diagnostics.activities");
			strs1.Add("microsoft.powershell.management.activities");
			strs1.Add("microsoft.powershell.security.activities");
			strs1.Add("microsoft.powershell.utility.activities");
			strs1.Add("microsoft.wsman.management.activities");
			PSWorkflowValidator.PowerShellActivitiesAssemblies = strs1;
			PSWorkflowValidator.TestMode = false;
			PSWorkflowValidator.ObjectCounter = (long)0;
		}

		public PSWorkflowValidator(PSWorkflowConfigurationProvider configuration)
		{
			this._validationCache = new ConcurrentDictionary<Guid, PSWorkflowValidationResults>();
			if (configuration != null)
			{
				if (PSWorkflowValidator.TestMode)
				{
					Interlocked.Increment(ref PSWorkflowValidator.ObjectCounter);
				}
				this.Configuration = configuration;
				return;
			}
			else
			{
				throw new ArgumentNullException("configuration");
			}
		}

		private bool CheckIfSuspendable(Activity activity)
		{
			bool flag;
			if (!string.Equals(activity.GetType().ToString(), "Microsoft.PowerShell.Activities.PSPersist", StringComparison.OrdinalIgnoreCase))
			{
				if (!string.Equals(activity.GetType().ToString(), "Microsoft.PowerShell.Activities.Suspend", StringComparison.OrdinalIgnoreCase))
				{
					PSActivity pSActivity = activity as PSActivity;
					if (pSActivity == null || pSActivity.PSPersist == null || pSActivity.PSPersist.Expression == null)
					{
						Sequence sequence = activity as Sequence;
						if (sequence != null && sequence.Variables != null && sequence.Variables.Count > 0)
						{
							foreach (Variable variable in sequence.Variables)
							{
								if (!string.Equals(variable.Name, "PSPersistPreference", StringComparison.OrdinalIgnoreCase))
								{
									continue;
								}
								flag = true;
								return flag;
							}
						}
						Parallel parallel = activity as Parallel;
						if (parallel != null && parallel.Variables != null && parallel.Variables.Count > 0)
						{
							foreach (Variable variable1 in parallel.Variables)
							{
								if (!string.Equals(variable1.Name, "PSPersistPreference", StringComparison.OrdinalIgnoreCase))
								{
									continue;
								}
								flag = true;
								return flag;
							}
						}
						return false;
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
			else
			{
				return true;
			}
		}

		internal bool IsActivityAllowed(Activity activity, string runtimeAssembly)
		{
			return this.ValidateActivity(activity, runtimeAssembly, null);
		}

		private static bool IsMatched(string allowedActivity, string match)
		{
			if (WildcardPattern.ContainsWildcardCharacters(allowedActivity))
			{
				return (new WildcardPattern(allowedActivity, WildcardOptions.IgnoreCase)).IsMatch(match);
			}
			else
			{
				return string.Equals(allowedActivity, match, StringComparison.OrdinalIgnoreCase);
			}
		}

		internal void ProcessValidationResults(ValidationResults results)
		{
			if (results != null)
			{
				if (results.Errors == null || results.Errors.Count != 0)
				{
					string empty = string.Empty;
					foreach (ValidationError error in results.Errors)
					{
						empty = string.Concat(empty, error.Message);
						empty = string.Concat(empty, "\n");
					}
					ValidationException validationException = new ValidationException(empty);
					PSWorkflowValidator.Tracer.TraceException(validationException);
					PSWorkflowValidator._structuredTracer.WorkflowValidationError(Guid.Empty);
					throw validationException;
				}
				else
				{
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("results");
			}
		}

		private Constraint ValidateActivitiesConstraint(string runtimeAssembly, PSWorkflowValidationResults validationResults)
		{
			var activityToValidate = new DelegateInArgument<Activity>();
			DelegateInArgument<ValidationContext> delegateInArgument1 = new DelegateInArgument<ValidationContext>();
			Constraint<Activity> constraint = new Constraint<Activity>();
			ActivityAction<Activity, ValidationContext> activityAction = new ActivityAction<Activity, ValidationContext>();
			activityAction.Argument1 = activityToValidate;
			activityAction.Argument2 = delegateInArgument1;
			AssertValidation assertValidation = new AssertValidation();
			assertValidation.IsWarning = false;
			ParameterExpression parameterExpression = Expression.Parameter(typeof(ActivityContext), "env");
			Expression[] expressionArray = new Expression[3];
			Expression[] expressionArray1 = new Expression[1];
			expressionArray1[0] = parameterExpression;
			expressionArray[0] = Expression.Call(Expression.Field(Expression.Constant(this), FieldInfo.GetFieldFromHandle(activityToValidate)), (MethodInfo)MethodBase.GetMethodFromHandle(Get, DelegateInArgument<Activity>), expressionArray1);
			expressionArray[1] = Expression.Field(Expression.Constant(this), FieldInfo.GetFieldFromHandle(runtimeAssembly));
			expressionArray[2] = Expression.Field(Expression.Constant(this), FieldInfo.GetFieldFromHandle(validationResults));
			ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
			parameterExpressionArray[0] = parameterExpression;
			assertValidation.Assertion = new InArgument<bool>(Expression.Lambda<Func<ActivityContext, bool>>(Expression.Call(Expression.Constant(this, typeof(PSWorkflowValidator)), (MethodInfo)MethodBase.GetMethodFromHandle(ValidateActivity), expressionArray), parameterExpressionArray));
			ParameterExpression parameterExpression1 = Expression.Parameter(typeof(ActivityContext), "env");
			Expression[] expressionArray2 = new Expression[3];
			expressionArray2[0] = Expression.Property(null, typeof(CultureInfo).GetProperty ("CurrentCulture", BindingFlags.Static)); //TODO: REIVEW: (MethodInfo)MethodBase.GetMethodFromHandle(CultureInfo.get_CurrentCulture));
			expressionArray2[1] = Expression.Property(null, typeof(Resources).GetProperty ("InvalidActivity", BindingFlags.Static)); //TODO: REVIEW: (MethodInfo)MethodBase.GetMethodFromHandle(Resources.get_InvalidActivity));
			Expression[] expressionArray3 = new Expression[1];
			Expression[] expressionArray4 = new Expression[1];
			expressionArray4[0] = parameterExpression1;
			expressionArray3[0] = Expression.Property(Expression.Call(Expression.Call(Expression.Field(Expression.Constant(this), FieldInfo.GetFieldFromHandle(PSWorkflowValidator.PSWorkflowValidator.activityToValidate)), (MethodInfo)MethodBase.GetMethodFromHandle(Get, DelegateInArgument<Activity>), expressionArray4), (MethodInfo)MethodBase.GetMethodFromHandle(GetType), new Expression[0]), (MethodInfo)MethodBase.GetMethodFromHandle(get_FullName));
			expressionArray2[2] = Expression.NewArrayInit(typeof(object), expressionArray3);
			ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
			parameterExpressionArray1[0] = parameterExpression1;
			assertValidation.Message = new InArgument<string>(Expression.Lambda<Func<ActivityContext, string>>(Expression.Call(null, (MethodInfo)MethodBase.GetMethodFromHandle(string.Format), expressionArray2), parameterExpressionArray1));
			activityAction.Handler = assertValidation;
			constraint.Body = activityAction;
			return constraint;
		}

		private bool ValidateActivity(Activity activity, string runtimeAssembly, PSWorkflowValidationResults validationResult)
		{
			bool flag;
			if (validationResult != null && !validationResult.IsWorkflowSuspendable)
			{
				validationResult.IsWorkflowSuspendable = this.CheckIfSuspendable(activity);
			}
			Type type = activity.GetType();
			if (Validation.CustomHandler == null || !Validation.CustomHandler(activity))
			{
				if (!string.Equals(type.Assembly.FullName, "System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase))
				{
					if (!string.Equals(runtimeAssembly, type.Assembly.GetName().Name, StringComparison.OrdinalIgnoreCase))
					{
						if (!this.Configuration.PSDefaultActivitiesAreAllowed || !PSWorkflowValidator.PowerShellActivitiesAssemblies.Contains(type.Assembly.GetName().Name.ToLowerInvariant()))
						{
							IEnumerable<string> allowedActivity = this.Configuration.AllowedActivity;
							IEnumerable<string> strs = allowedActivity;
							if (allowedActivity == null)
							{
								strs = (IEnumerable<string>)(new string[0]);
							}
							foreach (string str in strs)
							{
								if (string.Equals(str, "PSDefaultActivities", StringComparison.OrdinalIgnoreCase) || activity == null || !PSWorkflowValidator.IsMatched(str, type.Name) && !PSWorkflowValidator.IsMatched(str, type.FullName) && !PSWorkflowValidator.IsMatched(str, string.Concat(type.Assembly.GetName().Name, "\\", type.Name)) && !PSWorkflowValidator.IsMatched(str, string.Concat(type.Assembly.GetName().Name, "\\", type.FullName)) && !PSWorkflowValidator.IsMatched(str, string.Concat(type.Assembly.GetName().FullName, "\\", type.Name)) && !PSWorkflowValidator.IsMatched(str, string.Concat(type.Assembly.GetName().FullName, "\\", type.FullName)))
								{
									continue;
								}
								flag = true;
								break;
							}
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = PSWorkflowValidator.AllowedSystemActivities.Contains(type.Name);
				}
			}
			else
			{
				flag = true;
			}
			string displayName = activity.DisplayName;
			if (string.IsNullOrEmpty(displayName))
			{
				displayName = this.GetType().Name;
			}
			if (!flag)
			{
				PSWorkflowValidator._structuredTracer.WorkflowActivityValidationFailed(Guid.Empty, displayName, type.FullName);
			}
			else
			{
				PSWorkflowValidator._structuredTracer.WorkflowActivityValidated(Guid.Empty, displayName, type.FullName);
			}
			return flag;
		}

		public ValidationResults ValidateWorkflow(Activity workflow, string runtimeAssembly)
		{
			PSWorkflowValidationResults pSWorkflowValidationResult = new PSWorkflowValidationResults();
			this.ValidateWorkflowInternal(workflow, runtimeAssembly, pSWorkflowValidationResult);
			return pSWorkflowValidationResult.Results;
		}

		internal PSWorkflowValidationResults ValidateWorkflow(Guid referenceId, Activity workflow, string runtimeAssembly)
		{
			PSWorkflowValidationResults pSWorkflowValidationResult = null;
			if (!this._validationCache.ContainsKey(referenceId))
			{
				pSWorkflowValidationResult = new PSWorkflowValidationResults();
				this.ValidateWorkflowInternal(workflow, runtimeAssembly, pSWorkflowValidationResult);
				if (this._validationCache.Keys.Count == this.Configuration.ValidationCacheLimit)
				{
					this._validationCache.Clear();
				}
				this._validationCache.TryAdd(referenceId, pSWorkflowValidationResult);
			}
			else
			{
				this._validationCache.TryGetValue(referenceId, out pSWorkflowValidationResult);
			}
			return pSWorkflowValidationResult;
		}

		private void ValidateWorkflowInternal(Activity workflow, string runtimeAssembly, PSWorkflowValidationResults validationResults)
		{
			PSWorkflowValidator.Tracer.WriteMessage(string.Concat(PSWorkflowValidator.Facility, "Validating a workflow."));
			PSWorkflowValidator._structuredTracer.WorkflowValidationStarted(Guid.Empty);
			ValidationSettings validationSetting = new ValidationSettings();
			List<Constraint> constraints = new List<Constraint>();
			constraints.Add(this.ValidateActivitiesConstraint(runtimeAssembly, validationResults));
			validationSetting.AdditionalConstraints.Add(typeof(Activity), constraints);
			ValidationSettings validationSetting1 = validationSetting;
			try
			{
				validationResults.Results = ActivityValidationServices.Validate(workflow, validationSetting1);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				PSWorkflowValidator.Tracer.TraceException(exception);
				new ValidationException(Resources.ErrorWhileValidatingWorkflow, exception);
				throw exception;
			}
			PSWorkflowValidator._structuredTracer.WorkflowValidationFinished(Guid.Empty);
		}
	}
}