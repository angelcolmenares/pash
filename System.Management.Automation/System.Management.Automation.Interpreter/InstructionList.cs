using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Management.Automation.Interpreter
{
    [DebuggerTypeProxy(typeof(DebugView))]
    internal sealed class InstructionList
    {
        private const int PushIntMinCachedValue = -100;

        private const int PushIntMaxCachedValue = 100;

        private const int CachedObjectCount = 0x100;

        private const int LocalInstrCacheSize = 64;

        private readonly List<Instruction> _instructions;

        private List<object> _objects;

        private int _currentStackDepth;

        private int _maxStackDepth;

        private int _currentContinuationsDepth;

        private int _maxContinuationDepth;

        private int _runtimeLabelCount;

        private List<BranchLabel> _labels;

        private List<KeyValuePair<int, object>> _debugCookies;

        private static Instruction _null;

        private static Instruction _true;

        private static Instruction _false;

        private static Instruction[] _ints;

        private static Instruction[] _loadObjectCached;

        private static Instruction[] _loadLocal;

        private static Instruction[] _loadLocalBoxed;

        private static Instruction[] _loadLocalFromClosure;

        private static Instruction[] _loadLocalFromClosureBoxed;

        private static Instruction[] _assignLocal;

        private static Instruction[] _storeLocal;

        private static Instruction[] _assignLocalBoxed;

        private static Instruction[] _storeLocalBoxed;

        private static Instruction[] _assignLocalToClosure;

        private static Instruction[] _initReference;

        private static Instruction[] _initImmutableRefBox;

        private static Instruction[] _parameterBox;

        private static Instruction[] _parameter;

        private readonly static Dictionary<FieldInfo, Instruction> _loadFields;

        private static Dictionary<Type, Func<CallSiteBinder, Instruction>> _factories;

        private readonly static RuntimeLabel[] EmptyRuntimeLabels;

        public int Count
        {
            get
            {
                return this._instructions.Count;
            }
        }

        public int CurrentContinuationsDepth
        {
            get
            {
                return this._currentContinuationsDepth;
            }
        }

        public int CurrentStackDepth
        {
            get
            {
                return this._currentStackDepth;
            }
        }

        public int MaxStackDepth
        {
            get
            {
                return this._maxStackDepth;
            }
        }

        static InstructionList()
        {
            InstructionList._loadFields = new Dictionary<FieldInfo, Instruction>();
            InstructionList._factories = new Dictionary<Type, Func<CallSiteBinder, Instruction>>();
            RuntimeLabel[] runtimeLabel = new RuntimeLabel[1];
            runtimeLabel[0] = new RuntimeLabel(0x7fffffff, 0, 0);
            InstructionList.EmptyRuntimeLabels = runtimeLabel;
        }

        public InstructionList()
        {
            this._instructions = new List<Instruction>();
        }

        internal static Instruction AssignLocalBoxed(int index)
        {
            if (InstructionList._assignLocalBoxed == null)
            {
                InstructionList._assignLocalBoxed = new Instruction[64];
            }
            if (index >= (int)InstructionList._assignLocalBoxed.Length)
            {
                return new AssignLocalBoxedInstruction(index);
            }
            else
            {
                Instruction instruction = InstructionList._assignLocalBoxed[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    AssignLocalBoxedInstruction assignLocalBoxedInstruction = new AssignLocalBoxedInstruction(index);
                    Instruction instruction2 = assignLocalBoxedInstruction;
                    InstructionList._assignLocalBoxed[index] = assignLocalBoxedInstruction;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        private RuntimeLabel[] BuildRuntimeLabels()
        {
            if (this._runtimeLabelCount != 0)
            {
                RuntimeLabel[] runtimeLabel = new RuntimeLabel[this._runtimeLabelCount + 1];
                foreach (BranchLabel _label in this._labels)
                {
                    if (!_label.HasRuntimeLabel)
                    {
                        continue;
                    }
                    runtimeLabel[_label.LabelIndex] = _label.ToRuntimeLabel();
                }
                runtimeLabel[(int)runtimeLabel.Length - 1] = new RuntimeLabel(0x7fffffff, 0, 0);
                return runtimeLabel;
            }
            else
            {
                return InstructionList.EmptyRuntimeLabels;
            }
        }

        internal static Instruction CreateDynamicInstruction(Type delegateType, CallSiteBinder binder)
        {
            Func<CallSiteBinder, Instruction> func = null;
            Instruction dynamicInstructionN;
            lock (InstructionList._factories)
            {
                if (!InstructionList._factories.TryGetValue(delegateType, out func))
                {
                    if (delegateType.GetMethod("Invoke").ReturnType != typeof(void))
                    {
                        Type dynamicInstructionType = DynamicInstructionN.GetDynamicInstructionType(delegateType);
                        if (dynamicInstructionType != null)
                        {
                            func = (Func<CallSiteBinder, Instruction>)Delegate.CreateDelegate(typeof(Func<CallSiteBinder, Instruction>), dynamicInstructionType.GetMethod("Factory"));
                            InstructionList._factories[delegateType] = func;
                        }
                        else
                        {
                            dynamicInstructionN = new DynamicInstructionN(delegateType, CallSite.Create(delegateType, binder));
                            return dynamicInstructionN;
                        }
                    }
                    else
                    {
                        dynamicInstructionN = new DynamicInstructionN(delegateType, CallSite.Create(delegateType, binder), true);
                        return dynamicInstructionN;
                    }
                }
                return func(binder);
            }
            return dynamicInstructionN;
        }

        public void Emit(Instruction instruction)
        {
            this._instructions.Add(instruction);
            this.UpdateStackDepth(instruction);
        }

        public void EmitAdd(Type type, bool @checked)
        {
            if (!@checked)
            {
                this.Emit(AddInstruction.Create(type));
                return;
            }
            else
            {
                this.Emit(AddOvfInstruction.Create(type));
                return;
            }
        }

        public void EmitAssignLocal(int index)
        {
            if (InstructionList._assignLocal == null)
            {
                InstructionList._assignLocal = new Instruction[64];
            }
            if (index >= (int)InstructionList._assignLocal.Length)
            {
                this.Emit(new AssignLocalInstruction(index));
                return;
            }
            else
            {
                InstructionList instructionList = this;
                Instruction instruction = InstructionList._assignLocal[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    AssignLocalInstruction assignLocalInstruction = new AssignLocalInstruction(index);
                    Instruction instruction2 = assignLocalInstruction;
                    InstructionList._assignLocal[index] = assignLocalInstruction;
                    instruction1 = instruction2;
                }
                instructionList.Emit(instruction1);
                return;
            }
        }

        public void EmitAssignLocalBoxed(int index)
        {
            this.Emit(InstructionList.AssignLocalBoxed(index));
        }

        public void EmitAssignLocalToClosure(int index)
        {
            if (InstructionList._assignLocalToClosure == null)
            {
                InstructionList._assignLocalToClosure = new Instruction[64];
            }
            if (index >= (int)InstructionList._assignLocalToClosure.Length)
            {
                this.Emit(new AssignLocalToClosureInstruction(index));
                return;
            }
            else
            {
                InstructionList instructionList = this;
                Instruction instruction = InstructionList._assignLocalToClosure[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    AssignLocalToClosureInstruction assignLocalToClosureInstruction = new AssignLocalToClosureInstruction(index);
                    Instruction instruction2 = assignLocalToClosureInstruction;
                    InstructionList._assignLocalToClosure[index] = assignLocalToClosureInstruction;
                    instruction1 = instruction2;
                }
                instructionList.Emit(instruction1);
                return;
            }
        }

        private void EmitBranch(OffsetInstruction instruction, BranchLabel label)
        {
            this.Emit(instruction);
            label.AddBranch(this, this.Count - 1);
        }

        public void EmitBranch(BranchLabel label)
        {
            this.EmitBranch(new BranchInstruction(), label);
        }

        public void EmitBranch(BranchLabel label, bool hasResult, bool hasValue)
        {
            this.EmitBranch(new BranchInstruction(hasResult, hasValue), label);
        }

        public void EmitBranchFalse(BranchLabel elseLabel)
        {
            this.EmitBranch(new BranchFalseInstruction(), elseLabel);
        }

        public void EmitBranchTrue(BranchLabel elseLabel)
        {
            this.EmitBranch(new BranchTrueInstruction(), elseLabel);
        }

        public void EmitCall(MethodInfo method)
        {
            this.EmitCall(method, method.GetParameters());
        }

        public void EmitCall(MethodInfo method, ParameterInfo[] parameters)
        {
            this.Emit(CallInstruction.Create(method, parameters));
        }

        public void EmitCoalescingBranch(BranchLabel leftNotNull)
        {
            this.EmitBranch(new CoalescingBranchInstruction(), leftNotNull);
        }

        internal void EmitCreateDelegate(LightDelegateCreator creator)
        {
            this.Emit(new CreateDelegateInstruction(creator));
        }

        public void EmitDefaultValue(Type type)
        {
            this.Emit(InstructionFactory.GetFactory(type).DefaultValue());
        }

        public void EmitDiv(Type type)
        {
            this.Emit(DivInstruction.Create(type));
        }

        public void EmitDup()
        {
            this.Emit(DupInstruction.Instance);
        }

        public void EmitDynamic(Type type, CallSiteBinder binder)
        {
            this.Emit(InstructionList.CreateDynamicInstruction(type, binder));
        }

        public void EmitDynamic<T0, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>.Factory(binder));
        }

        public void EmitDynamic<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>(CallSiteBinder binder)
        {
            this.Emit(DynamicInstruction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet>.Factory(binder));
        }

        public void EmitEnterExceptionHandlerNonVoid()
        {
            this.Emit(EnterExceptionHandlerInstruction.NonVoid);
        }

        public void EmitEnterExceptionHandlerVoid()
        {
            this.Emit(EnterExceptionHandlerInstruction.Void);
        }

        public void EmitEnterFinally(BranchLabel finallyStartLabel)
        {
            this.Emit(EnterFinallyInstruction.Create(this.EnsureLabelIndex(finallyStartLabel)));
        }

        public void EmitEnterTryCatch()
        {
            this.Emit(EnterTryCatchFinallyInstruction.CreateTryCatch());
        }

        public void EmitEnterTryFinally(BranchLabel finallyStartLabel)
        {
            this.Emit(EnterTryCatchFinallyInstruction.CreateTryFinally(this.EnsureLabelIndex(finallyStartLabel)));
        }

        public void EmitEqual(Type type)
        {
            this.Emit(EqualInstruction.Create(type));
        }

        public void EmitGetArrayItem(Type arrayType)
        {
            Type elementType = arrayType.GetElementType();
            if (elementType.IsClass || elementType.IsInterface)
            {
                this.Emit(InstructionFactory<object>.Factory.GetArrayItem());
                return;
            }
            else
            {
                this.Emit(InstructionFactory.GetFactory(elementType).GetArrayItem());
                return;
            }
        }

        public void EmitGoto(BranchLabel label, bool hasResult, bool hasValue)
        {
            this.Emit(GotoInstruction.Create(this.EnsureLabelIndex(label), hasResult, hasValue));
        }

        public void EmitGreaterThan(Type type)
        {
            this.Emit(GreaterThanInstruction.Create(type));
        }

        public void EmitGreaterThanOrEqual(Type type)
        {
            throw new NotSupportedException();
        }

        public void EmitInitializeLocal(int index, Type type)
        {
            object primitiveDefaultValue = ScriptingRuntimeHelpers.GetPrimitiveDefaultValue(type);
            if (primitiveDefaultValue == null)
            {
                if (!type.IsValueType)
                {
                    this.Emit(InstructionList.InitReference(index));
                    return;
                }
                else
                {
                    this.Emit(new InitializeLocalInstruction.MutableValue(index, type));
                    return;
                }
            }
            else
            {
                this.Emit(new InitializeLocalInstruction.ImmutableValue(index, primitiveDefaultValue));
                return;
            }
        }

        internal void EmitInitializeParameter(int index)
        {
            this.Emit(InstructionList.Parameter(index));
        }

        public void EmitLeaveExceptionHandler(bool hasValue, BranchLabel tryExpressionEndLabel)
        {
            this.Emit(LeaveExceptionHandlerInstruction.Create(this.EnsureLabelIndex(tryExpressionEndLabel), hasValue));
        }

        public void EmitLeaveFault(bool hasValue)
        {
            Instruction nonVoid;
            InstructionList instructionList = this;
            if (hasValue)
            {
                nonVoid = LeaveFaultInstruction.NonVoid;
            }
            else
            {
                nonVoid = LeaveFaultInstruction.Void;
            }
            instructionList.Emit(nonVoid);
        }

        public void EmitLeaveFinally()
        {
            this.Emit(LeaveFinallyInstruction.Instance);
        }

        public void EmitLessThan(Type type)
        {
            this.Emit(LessThanInstruction.Create(type));
        }

        public void EmitLessThanOrEqual(Type type)
        {
            throw new NotSupportedException();
        }

        public void EmitLoad(object value)
        {
            this.EmitLoad(value, null);
        }

        public void EmitLoad(bool value)
        {
            if (!value)
            {
                InstructionList instructionList = this;
                Instruction instruction = InstructionList._false;
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    LoadObjectInstruction loadObjectInstruction = new LoadObjectInstruction((object)value);
                    instruction1 = loadObjectInstruction;
                    InstructionList._false = loadObjectInstruction;
                }
                instructionList.Emit(instruction1);
                return;
            }
            else
            {
                InstructionList instructionList1 = this;
                Instruction instruction2 = InstructionList._true;
                Instruction instruction3 = instruction2;
                if (instruction2 == null)
                {
                    LoadObjectInstruction loadObjectInstruction1 = new LoadObjectInstruction((object)value);
                    instruction3 = loadObjectInstruction1;
                    InstructionList._true = loadObjectInstruction1;
                }
                instructionList1.Emit(instruction3);
                return;
            }
        }

        public void EmitLoad(object value, Type type)
        {
            unsafe
            {
                if (value != null)
                {
                    if (type == null || type.IsValueType)
                    {
                        if (!(value is bool))
                        {
                            if (value is int)
                            {
                                int num = (int)value;
                                if (num >= -100 && num <= 100)
                                {
                                    if (InstructionList._ints == null)
                                    {
                                        InstructionList._ints = new Instruction[201];
                                    }
                                    num = num - -100;
                                    InstructionList instructionList = this;
                                    Instruction instruction = InstructionList._ints[num];
                                    Instruction instruction1 = instruction;
                                    if (instruction == null)
                                    {
                                        LoadObjectInstruction loadObjectInstruction = new LoadObjectInstruction(value);
                                        Instruction instruction2 = loadObjectInstruction;
                                        InstructionList._ints[num] = loadObjectInstruction;
                                        instruction1 = instruction2;
                                    }
                                    instructionList.Emit(instruction1);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            this.EmitLoad((bool)value);
                            return;
                        }
                    }
                    if (this._objects == null)
                    {
                        this._objects = new List<object>();
                        if (InstructionList._loadObjectCached == null)
                        {
                            InstructionList._loadObjectCached = new Instruction[0x100];
                        }
                    }
                    if (this._objects.Count >= (int)InstructionList._loadObjectCached.Length)
                    {
                        this.Emit(new LoadObjectInstruction(value));
                        return;
                    }
                    else
                    {
                        int count = (int)this._objects.Count;
                        this._objects.Add(value);
                        InstructionList instructionList1 = this;
                        Instruction instruction3 = InstructionList._loadObjectCached[count];
                        Instruction instruction4 = instruction3;
                        if (instruction3 == null)
                        {
                            LoadCachedObjectInstruction loadCachedObjectInstruction = new LoadCachedObjectInstruction(count);
                            Instruction instruction5 = loadCachedObjectInstruction;
                            InstructionList._loadObjectCached[count] = loadCachedObjectInstruction;
                            instruction4 = instruction5;
                        }
                        instructionList1.Emit(instruction4);
                        return;
                    }
                }
                else
                {
                    InstructionList instructionList2 = this;
                    Instruction instruction6 = InstructionList._null;
                    Instruction instruction7 = instruction6;
                    if (instruction6 == null)
                    {
                        LoadObjectInstruction loadObjectInstruction1 = new LoadObjectInstruction(null);
                        instruction7 = loadObjectInstruction1;
                        InstructionList._null = loadObjectInstruction1;
                    }
                    instructionList2.Emit(instruction7);
                    return;
                }
            }
        }

        public void EmitLoadField(FieldInfo field)
        {
            this.Emit(this.GetLoadField(field));
        }

        public void EmitLoadLocal(int index)
        {
            if (InstructionList._loadLocal == null)
            {
                InstructionList._loadLocal = new Instruction[64];
            }
            if (index >= (int)InstructionList._loadLocal.Length)
            {
                this.Emit(new LoadLocalInstruction(index));
                return;
            }
            else
            {
                InstructionList instructionList = this;
                Instruction instruction = InstructionList._loadLocal[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    LoadLocalInstruction loadLocalInstruction = new LoadLocalInstruction(index);
                    Instruction instruction2 = loadLocalInstruction;
                    InstructionList._loadLocal[index] = loadLocalInstruction;
                    instruction1 = instruction2;
                }
                instructionList.Emit(instruction1);
                return;
            }
        }

        public void EmitLoadLocalBoxed(int index)
        {
            this.Emit(InstructionList.LoadLocalBoxed(index));
        }

        public void EmitLoadLocalFromClosure(int index)
        {
            if (InstructionList._loadLocalFromClosure == null)
            {
                InstructionList._loadLocalFromClosure = new Instruction[64];
            }
            if (index >= (int)InstructionList._loadLocalFromClosure.Length)
            {
                this.Emit(new LoadLocalFromClosureInstruction(index));
                return;
            }
            else
            {
                InstructionList instructionList = this;
                Instruction instruction = InstructionList._loadLocalFromClosure[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    LoadLocalFromClosureInstruction loadLocalFromClosureInstruction = new LoadLocalFromClosureInstruction(index);
                    Instruction instruction2 = loadLocalFromClosureInstruction;
                    InstructionList._loadLocalFromClosure[index] = loadLocalFromClosureInstruction;
                    instruction1 = instruction2;
                }
                instructionList.Emit(instruction1);
                return;
            }
        }

        public void EmitLoadLocalFromClosureBoxed(int index)
        {
            if (InstructionList._loadLocalFromClosureBoxed == null)
            {
                InstructionList._loadLocalFromClosureBoxed = new Instruction[64];
            }
            if (index >= (int)InstructionList._loadLocalFromClosureBoxed.Length)
            {
                this.Emit(new LoadLocalFromClosureBoxedInstruction(index));
                return;
            }
            else
            {
                InstructionList instructionList = this;
                Instruction instruction = InstructionList._loadLocalFromClosureBoxed[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    LoadLocalFromClosureBoxedInstruction loadLocalFromClosureBoxedInstruction = new LoadLocalFromClosureBoxedInstruction(index);
                    Instruction instruction2 = loadLocalFromClosureBoxedInstruction;
                    InstructionList._loadLocalFromClosureBoxed[index] = loadLocalFromClosureBoxedInstruction;
                    instruction1 = instruction2;
                }
                instructionList.Emit(instruction1);
                return;
            }
        }

        public void EmitMul(Type type, bool @checked)
        {
            if (!@checked)
            {
                this.Emit(MulInstruction.Create(type));
                return;
            }
            else
            {
                this.Emit(MulOvfInstruction.Create(type));
                return;
            }
        }

        public void EmitNew(ConstructorInfo constructorInfo)
        {
            this.Emit(new NewInstruction(constructorInfo));
        }

        public void EmitNewArray(Type elementType)
        {
            this.Emit(InstructionFactory.GetFactory(elementType).NewArray());
        }

        public void EmitNewArrayBounds(Type elementType, int rank)
        {
            this.Emit(new NewArrayBoundsInstruction(elementType, rank));
        }

        public void EmitNewArrayInit(Type elementType, int elementCount)
        {
            this.Emit(InstructionFactory.GetFactory(elementType).NewArrayInit(elementCount));
        }

        public void EmitNewRuntimeVariables(int count)
        {
            this.Emit(new RuntimeVariablesInstruction(count));
        }

        public void EmitNot()
        {
            this.Emit(NotInstruction.Instance);
        }

        public void EmitNotEqual(Type type)
        {
            this.Emit(NotEqualInstruction.Create(type));
        }

        public void EmitNumericConvertChecked(TypeCode from, TypeCode to)
        {
            this.Emit(new NumericConvertInstruction.Checked(from, to));
        }

        public void EmitNumericConvertUnchecked(TypeCode from, TypeCode to)
        {
            this.Emit(new NumericConvertInstruction.Unchecked(from, to));
        }

        public void EmitPop()
        {
            this.Emit(PopInstruction.Instance);
        }

        public void EmitRethrow()
        {
            this.Emit(ThrowInstruction.Rethrow);
        }

        public void EmitRethrowVoid()
        {
            this.Emit(ThrowInstruction.VoidRethrow);
        }

        public void EmitSetArrayItem(Type arrayType)
        {
            Type elementType = arrayType.GetElementType();
            if (elementType.IsClass || elementType.IsInterface)
            {
                this.Emit(InstructionFactory<object>.Factory.SetArrayItem());
                return;
            }
            else
            {
                this.Emit(InstructionFactory.GetFactory(elementType).SetArrayItem());
                return;
            }
        }

        public void EmitStoreField(FieldInfo field)
        {
            if (!field.IsStatic)
            {
                this.Emit(new StoreFieldInstruction(field));
                return;
            }
            else
            {
                this.Emit(new StoreStaticFieldInstruction(field));
                return;
            }
        }

        public void EmitStoreLocal(int index)
        {
            if (InstructionList._storeLocal == null)
            {
                InstructionList._storeLocal = new Instruction[64];
            }
            if (index >= (int)InstructionList._storeLocal.Length)
            {
                this.Emit(new StoreLocalInstruction(index));
                return;
            }
            else
            {
                InstructionList instructionList = this;
                Instruction instruction = InstructionList._storeLocal[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    StoreLocalInstruction storeLocalInstruction = new StoreLocalInstruction(index);
                    Instruction instruction2 = storeLocalInstruction;
                    InstructionList._storeLocal[index] = storeLocalInstruction;
                    instruction1 = instruction2;
                }
                instructionList.Emit(instruction1);
                return;
            }
        }

        public void EmitStoreLocalBoxed(int index)
        {
            this.Emit(InstructionList.StoreLocalBoxed(index));
        }

        public void EmitStoreLocalToClosure(int index)
        {
            this.EmitAssignLocalToClosure(index);
            this.EmitPop();
        }

        public void EmitSub(Type type, bool @checked)
        {
            if (!@checked)
            {
                this.Emit(SubInstruction.Create(type));
                return;
            }
            else
            {
                this.Emit(SubOvfInstruction.Create(type));
                return;
            }
        }

        public void EmitSwitch(Dictionary<int, int> cases)
        {
            this.Emit(new SwitchInstruction(cases));
        }

        public void EmitThrow()
        {
            this.Emit(ThrowInstruction.Throw);
        }

        public void EmitThrowVoid()
        {
            this.Emit(ThrowInstruction.VoidThrow);
        }

        public void EmitTypeAs(Type type)
        {
            this.Emit(InstructionFactory.GetFactory(type).TypeAs());
        }

        public void EmitTypeEquals()
        {
            this.Emit(TypeEqualsInstruction.Instance);
        }

        public void EmitTypeIs(Type type)
        {
            this.Emit(InstructionFactory.GetFactory(type).TypeIs());
        }

        private int EnsureLabelIndex(BranchLabel label)
        {
            if (!label.HasRuntimeLabel)
            {
                label.LabelIndex = this._runtimeLabelCount;
                InstructionList instructionList = this;
                instructionList._runtimeLabelCount = instructionList._runtimeLabelCount + 1;
                return label.LabelIndex;
            }
            else
            {
                return label.LabelIndex;
            }
        }

        internal void FixupBranch(int branchIndex, int offset)
        {
            this._instructions[branchIndex] = ((OffsetInstruction)this._instructions[branchIndex]).Fixup(offset);
        }

        internal Instruction GetInstruction(int index)
        {
            return this._instructions[index];
        }

        private Instruction GetLoadField(FieldInfo field)
        {
            Instruction loadFieldInstruction = null;
            Instruction instruction;
            lock (InstructionList._loadFields)
            {
                if (!InstructionList._loadFields.TryGetValue(field, out loadFieldInstruction))
                {
                    if (!field.IsStatic)
                    {
                        loadFieldInstruction = new LoadFieldInstruction(field);
                    }
                    else
                    {
                        loadFieldInstruction = new LoadStaticFieldInstruction(field);
                    }
                    InstructionList._loadFields.Add(field, loadFieldInstruction);
                }
                instruction = loadFieldInstruction;
            }
            return instruction;
        }

        internal static Instruction InitImmutableRefBox(int index)
        {
            if (InstructionList._initImmutableRefBox == null)
            {
                InstructionList._initImmutableRefBox = new Instruction[64];
            }
            if (index >= (int)InstructionList._initImmutableRefBox.Length)
            {
                return new InitializeLocalInstruction.ImmutableBox(index, null);
            }
            else
            {
                Instruction instruction = InstructionList._initImmutableRefBox[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    InitializeLocalInstruction.ImmutableBox immutableBox = new InitializeLocalInstruction.ImmutableBox(index, null);
                    Instruction instruction2 = immutableBox;
                    InstructionList._initImmutableRefBox[index] = immutableBox;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        internal static Instruction InitReference(int index)
        {
            if (InstructionList._initReference == null)
            {
                InstructionList._initReference = new Instruction[64];
            }
            if (index >= (int)InstructionList._initReference.Length)
            {
                return new InitializeLocalInstruction.Reference(index);
            }
            else
            {
                Instruction instruction = InstructionList._initReference[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    InitializeLocalInstruction.Reference reference = new InitializeLocalInstruction.Reference(index);
                    Instruction instruction2 = reference;
                    InstructionList._initReference[index] = reference;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        internal static Instruction LoadLocalBoxed(int index)
        {
            if (InstructionList._loadLocalBoxed == null)
            {
                InstructionList._loadLocalBoxed = new Instruction[64];
            }
            if (index >= (int)InstructionList._loadLocalBoxed.Length)
            {
                return new LoadLocalBoxedInstruction(index);
            }
            else
            {
                Instruction instruction = InstructionList._loadLocalBoxed[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    LoadLocalBoxedInstruction loadLocalBoxedInstruction = new LoadLocalBoxedInstruction(index);
                    Instruction instruction2 = loadLocalBoxedInstruction;
                    InstructionList._loadLocalBoxed[index] = loadLocalBoxedInstruction;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        public BranchLabel MakeLabel()
        {
            if (this._labels == null)
            {
                this._labels = new List<BranchLabel>();
            }
            BranchLabel branchLabel = new BranchLabel();
            this._labels.Add(branchLabel);
            return branchLabel;
        }

        public void MarkLabel(BranchLabel label)
        {
            label.Mark(this);
        }

        public int MarkRuntimeLabel()
        {
            BranchLabel branchLabel = this.MakeLabel();
            this.MarkLabel(branchLabel);
            return this.EnsureLabelIndex(branchLabel);
        }

        internal static Instruction Parameter(int index)
        {
            if (InstructionList._parameter == null)
            {
                InstructionList._parameter = new Instruction[64];
            }
            if (index >= (int)InstructionList._parameter.Length)
            {
                return new InitializeLocalInstruction.Parameter(index);
            }
            else
            {
                Instruction instruction = InstructionList._parameter[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    InitializeLocalInstruction.Parameter parameter = new InitializeLocalInstruction.Parameter(index);
                    Instruction instruction2 = parameter;
                    InstructionList._parameter[index] = parameter;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        internal static Instruction ParameterBox(int index)
        {
            if (InstructionList._parameterBox == null)
            {
                InstructionList._parameterBox = new Instruction[64];
            }
            if (index >= (int)InstructionList._parameterBox.Length)
            {
                return new InitializeLocalInstruction.ParameterBox(index);
            }
            else
            {
                Instruction instruction = InstructionList._parameterBox[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    InitializeLocalInstruction.ParameterBox parameterBox = new InitializeLocalInstruction.ParameterBox(index);
                    Instruction instruction2 = parameterBox;
                    InstructionList._parameterBox[index] = parameterBox;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        [Conditional("DEBUG")]
        public void SetDebugCookie(object cookie)
        {
        }

        internal static Instruction StoreLocalBoxed(int index)
        {
            if (InstructionList._storeLocalBoxed == null)
            {
                InstructionList._storeLocalBoxed = new Instruction[64];
            }
            if (index >= (int)InstructionList._storeLocalBoxed.Length)
            {
                return new StoreLocalBoxedInstruction(index);
            }
            else
            {
                Instruction instruction = InstructionList._storeLocalBoxed[index];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    StoreLocalBoxedInstruction storeLocalBoxedInstruction = new StoreLocalBoxedInstruction(index);
                    Instruction instruction2 = storeLocalBoxedInstruction;
                    InstructionList._storeLocalBoxed[index] = storeLocalBoxedInstruction;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        internal void SwitchToBoxed(int index, int instructionIndex)
        {
            IBoxableInstruction item = this._instructions[instructionIndex] as IBoxableInstruction;
            if (item != null)
            {
                Instruction instruction = item.BoxIfIndexMatches(index);
                if (instruction != null)
                {
                    this._instructions[instructionIndex] = instruction;
                }
            }
        }

        public InstructionArray ToArray()
		{
			object[] array;
			int num = this._maxStackDepth;
			int num1 = this._maxContinuationDepth;
			var _0Array = this._instructions.ToArray();
			if (this._objects != null)
			{
				array = this._objects.ToArray();
			}
			else
			{
				array = null;
			}
			return new InstructionArray(num, num1, _0Array, array, this.BuildRuntimeLabels(), this._debugCookies);
		}

        private void UpdateStackDepth(Instruction instruction)
        {
            InstructionList consumedStack = this;
            consumedStack._currentStackDepth = consumedStack._currentStackDepth - instruction.ConsumedStack;
            InstructionList producedStack = this;
            producedStack._currentStackDepth = producedStack._currentStackDepth + instruction.ProducedStack;
            if (this._currentStackDepth > this._maxStackDepth)
            {
                this._maxStackDepth = this._currentStackDepth;
            }
            InstructionList consumedContinuations = this;
            consumedContinuations._currentContinuationsDepth = consumedContinuations._currentContinuationsDepth - instruction.ConsumedContinuations;
            InstructionList producedContinuations = this;
            producedContinuations._currentContinuationsDepth = producedContinuations._currentContinuationsDepth + instruction.ProducedContinuations;
            if (this._currentContinuationsDepth > this._maxContinuationDepth)
            {
                this._maxContinuationDepth = this._currentContinuationsDepth;
            }
        }

        internal sealed class DebugView
        {
            private readonly InstructionList _list;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public InstructionList.DebugView.InstructionView[] A0
            {
                get
                {
                    return InstructionList.DebugView.GetInstructionViews(this._list._instructions, this._list._objects, (int index) => this._list._labels[index].TargetIndex, this._list._debugCookies);
                }
            }

            public DebugView(InstructionList list)
            {
                this._list = list;
            }

            internal static InstructionList.DebugView.InstructionView[] GetInstructionViews(IList<Instruction> instructions, IList<object> objects, Func<int, int> labelIndexer, IList<KeyValuePair<int, object>> debugCookies)
            {
                object value = null;
                IEnumerable<KeyValuePair<int, object>> keyValuePairs;
                List<InstructionList.DebugView.InstructionView> instructionViews = new List<InstructionList.DebugView.InstructionView>();
                int num = 0;
                int num1 = 0;
                int num2 = 0;
                if (debugCookies != null)
                {
                    keyValuePairs = debugCookies;
                }
                else
                {
                    keyValuePairs = new KeyValuePair<int, object>[0];
                }
                IEnumerator<KeyValuePair<int, object>> enumerator = keyValuePairs.GetEnumerator();
                bool flag = enumerator.MoveNext();
                for (int i = 0; i < instructions.Count; i++)
                {
                    while (flag)
                    {
                        KeyValuePair<int, object> current = enumerator.Current;
                        if (current.Key != i)
                        {
                            break;
                        }
                        KeyValuePair<int, object> keyValuePair = enumerator.Current;
                        value = keyValuePair.Value;
                        flag = enumerator.MoveNext();
                    }
                    int stackBalance = instructions[i].StackBalance;
                    int continuationsBalance = instructions[i].ContinuationsBalance;
                    string debugString = instructions[i].ToDebugString(i, value, labelIndexer, objects);
                    instructionViews.Add(new InstructionList.DebugView.InstructionView(instructions[i], debugString, i, num1, num2));
                    num++;
                    num1 = num1 + stackBalance;
                    num2 = num2 + continuationsBalance;
                }
                return instructionViews.ToArray();
            }

            [DebuggerDisplay("{GetValue(),nq}", Name = "{GetName(),nq}", Type = "{GetDisplayType(), nq}")]
            internal struct InstructionView
            {
                private readonly int _index;

                private readonly int _stackDepth;

                private readonly int _continuationsDepth;

                private readonly string _name;

                private readonly Instruction _instruction;

                public InstructionView(Instruction instruction, string name, int index, int stackDepth, int continuationsDepth)
                {
                    this._instruction = instruction;
                    this._name = name;
                    this._index = index;
                    this._stackDepth = stackDepth;
                    this._continuationsDepth = continuationsDepth;
                }

                internal string GetDisplayType()
                {
                    return string.Concat(this._instruction.ContinuationsBalance, "/", this._instruction.StackBalance);
                }

                internal string GetName()
                {
                    object obj;
                    object obj1;
                    object obj2 = this._index;
                    if (this._continuationsDepth == 0)
                    {
                        obj = "";
                    }
                    else
                    {
                        obj = string.Concat(" C(", this._continuationsDepth, ")");
                    }
                    if (this._stackDepth == 0)
                    {
                        obj1 = "";
                    }
                    else
                    {
                        obj1 = string.Concat(" S(", this._stackDepth, ")");
                    }
                    return string.Concat(obj2, obj, obj1);
                }

                internal string GetValue()
                {
                    return this._name;
                }
            }
        }
    }
}