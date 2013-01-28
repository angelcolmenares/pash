namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    internal sealed class TypeMatch
    {
        private int _bestMatchIndex;
        private TypeMatchItem _bestMatchItem;
        private TypeInfoDataBase _db;
        private MshExpressionFactory _expressionFactory;
        private List<MshExpressionResult> _failedResultsList;
        private Collection<string> _typeNameHierarchy;
        private bool _useInheritance;
        private static PSTraceSource activeTracer = null;
        private const int BestMatchIndexPerfect = 0;
        private const int BestMatchIndexUndefined = -1;
        [TraceSource("TypeMatch", "F&O TypeMatch")]
        private static readonly PSTraceSource classTracer = PSTraceSource.GetTracer("TypeMatch", "F&O TypeMatch");

        internal TypeMatch(MshExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
        {
            this._failedResultsList = new List<MshExpressionResult>();
            this._bestMatchIndex = -1;
            this._expressionFactory = expressionFactory;
            this._db = db;
            this._typeNameHierarchy = typeNames;
            this._useInheritance = true;
        }

        internal TypeMatch(MshExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames, bool useInheritance)
        {
            this._failedResultsList = new List<MshExpressionResult>();
            this._bestMatchIndex = -1;
            this._expressionFactory = expressionFactory;
            this._db = db;
            this._typeNameHierarchy = typeNames;
            this._useInheritance = useInheritance;
        }

        private int ComputeBestMatch(AppliesTo appliesTo, PSObject currentObject)
        {
            int num = -1;
            foreach (TypeOrGroupReference reference in appliesTo.referenceList)
            {
                MshExpression ex = null;
                if (reference.conditionToken != null)
                {
                    ex = this._expressionFactory.CreateFromExpressionToken(reference.conditionToken);
                }
                int num2 = -1;
                TypeReference reference2 = reference as TypeReference;
                if (reference2 != null)
                {
                    num2 = this.MatchTypeIndex(reference2.name, currentObject, ex);
                }
                else
                {
                    TypeGroupReference reference3 = reference as TypeGroupReference;
                    TypeGroupDefinition tgd = DisplayDataQuery.FindGroupDefinition(this._db, reference3.name);
                    if (tgd != null)
                    {
                        num2 = this.ComputeBestMatchInGroup(tgd, currentObject, ex);
                    }
                }
                if (num2 == 0)
                {
                    return num2;
                }
                if ((num == -1) || (num < num2))
                {
                    num = num2;
                }
            }
            return num;
        }

        private int ComputeBestMatchInGroup(TypeGroupDefinition tgd, PSObject currentObject, MshExpression ex)
        {
            int num = -1;
            int num2 = 0;
            foreach (TypeReference reference in tgd.typeReferenceList)
            {
                int num3 = this.MatchTypeIndex(reference.name, currentObject, ex);
                if (num3 == 0)
                {
                    return num3;
                }
                if ((num == -1) || (num < num3))
                {
                    num = num3;
                }
                num2++;
            }
            return num;
        }

        private bool MatchCondition(PSObject currentObject, MshExpression ex)
        {
            MshExpressionResult result;
            if (ex == null)
            {
                return true;
            }
            bool flag = DisplayCondition.Evaluate(currentObject, ex, out result);
            if ((result != null) && (result.Exception != null))
            {
                this._failedResultsList.Add(result);
            }
            return flag;
        }

        private int MatchTypeIndex(string typeName, PSObject currentObject, MshExpression ex)
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                int num = 0;
                foreach (string str in this._typeNameHierarchy)
                {
                    if (string.Equals(str, typeName, StringComparison.OrdinalIgnoreCase) && this.MatchCondition(currentObject, ex))
                    {
                        return num;
                    }
                    if ((num == 0) && !this._useInheritance)
                    {
                        break;
                    }
                    num++;
                }
            }
            return -1;
        }

        internal bool PerfectMatch(TypeMatchItem item)
        {
            int num = this.ComputeBestMatch(item.AppliesTo, item.CurrentObject);
            if (num == -1)
            {
                return false;
            }
            if ((this._bestMatchIndex == -1) || (num < this._bestMatchIndex))
            {
                this._bestMatchIndex = num;
                this._bestMatchItem = item;
            }
            return (this._bestMatchIndex == 0);
        }

        internal static void ResetTracer()
        {
            activeTracer = classTracer;
        }

        internal static void SetTracer(PSTraceSource t)
        {
            activeTracer = t;
        }

        private static PSTraceSource ActiveTracer
        {
            get
            {
                return (activeTracer ?? classTracer);
            }
        }

        internal object BestMatch
        {
            get
            {
                if (this._bestMatchItem == null)
                {
                    return null;
                }
                return this._bestMatchItem.Item;
            }
        }
    }
}

