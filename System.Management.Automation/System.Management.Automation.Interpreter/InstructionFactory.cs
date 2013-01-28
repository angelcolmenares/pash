namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    internal abstract class InstructionFactory
    {
        private static Dictionary<Type, InstructionFactory> _factories;

        protected InstructionFactory()
        {
        }

        protected internal abstract Instruction DefaultValue();
        protected internal abstract Instruction GetArrayItem();
        internal static InstructionFactory GetFactory(Type type)
        {
            if (_factories == null)
            {
                Dictionary<Type, InstructionFactory> dictionary = new Dictionary<Type, InstructionFactory>();
                dictionary.Add(typeof(object), InstructionFactory<object>.Factory);
                dictionary.Add(typeof(bool), InstructionFactory<bool>.Factory);
                dictionary.Add(typeof(byte), InstructionFactory<byte>.Factory);
                dictionary.Add(typeof(sbyte), InstructionFactory<sbyte>.Factory);
                dictionary.Add(typeof(short), InstructionFactory<short>.Factory);
                dictionary.Add(typeof(ushort), InstructionFactory<ushort>.Factory);
                dictionary.Add(typeof(int), InstructionFactory<int>.Factory);
                dictionary.Add(typeof(uint), InstructionFactory<uint>.Factory);
                dictionary.Add(typeof(long), InstructionFactory<long>.Factory);
                dictionary.Add(typeof(ulong), InstructionFactory<ulong>.Factory);
                dictionary.Add(typeof(float), InstructionFactory<float>.Factory);
                dictionary.Add(typeof(double), InstructionFactory<double>.Factory);
                dictionary.Add(typeof(char), InstructionFactory<char>.Factory);
                dictionary.Add(typeof(string), InstructionFactory<string>.Factory);
                dictionary.Add(typeof(BigInteger), InstructionFactory<BigInteger>.Factory);
                _factories = dictionary;
            }
            lock (_factories)
            {
                InstructionFactory factory;
                if (!_factories.TryGetValue(type, out factory))
                {
                    factory = (InstructionFactory) typeof(InstructionFactory<>).MakeGenericType(new Type[] { type }).GetField("Factory").GetValue(null);
                    _factories[type] = factory;
                }
                return factory;
            }
        }

        protected internal abstract Instruction NewArray();
        protected internal abstract Instruction NewArrayInit(int elementCount);
        protected internal abstract Instruction SetArrayItem();
        protected internal abstract Instruction TypeAs();
        protected internal abstract Instruction TypeIs();
    }
}

