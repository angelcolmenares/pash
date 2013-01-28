using Microsoft.Management.Odata.MofParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	internal sealed class MofSpecificationSet
	{
		private readonly MofSpecification[] m_specifications;

		public readonly Dictionary<string, ClassDeclaration> ClassDeclarations;

		public int ClassDeclarationCount
		{
			get
			{
				return this.ClassDeclarations.Count;
			}
		}

		public MofSpecificationSet(MofSpecification[] specifications)
		{
			this.ClassDeclarations = new Dictionary<string, ClassDeclaration>(StringComparer.OrdinalIgnoreCase);
			if (specifications != null)
			{
				this.m_specifications = specifications;
				MofSpecification[] mofSpecificationArray = specifications;
				for (int i = 0; i < (int)mofSpecificationArray.Length; i++)
				{
					MofSpecification mofSpecification = mofSpecificationArray[i];
					MofProduction[] productions = mofSpecification.Productions;
					int num = 0;
					while (num < (int)productions.Length)
					{
						MofProduction mofProduction = productions[num];
						MofProduction.ProductionType type = mofProduction.Type;
						switch (type)
						{
							case MofProduction.ProductionType.ClassDeclaration:
							{
								ClassDeclaration classDeclaration = (ClassDeclaration)mofProduction;
								try
								{
									this.ClassDeclarations.Add(classDeclaration.Name.FullName, classDeclaration);
									num++;
									continue;
								}
								catch (ArgumentException argumentException1)
								{
									ArgumentException argumentException = argumentException1;
									argumentException.ToString();
									throw new ParseFailureException(string.Concat("class ", classDeclaration.Name.FullName, " is already defined"), classDeclaration.Location);
								}
							}
							case MofProduction.ProductionType.CompilerDirective:
							case MofProduction.ProductionType.InstanceDeclaration:
							{
								num++;
								continue;
							}
						}
						throw new InvalidOperationException();
					}
				}
				return;
			}
			else
			{
				throw new ArgumentException();
			}
		}

		private void AccumulateClosureOfAssociationClass(HashSet<MofProduction> accumulator, IEnumerable<Qualifier> qualifiers)
		{
			foreach (Qualifier qualifier in qualifiers)
			{
				if (!string.Equals(qualifier.Name, "AssociationClass", StringComparison.Ordinal))
				{
					continue;
				}
				this.GetClosureOfClass(accumulator, qualifier.Parameter as string);
			}
		}

		private void AccumulateClosureOfDataType(HashSet<MofProduction> accumulator, DataType dataType)
		{
			DataTypeType type = dataType.Type;
			switch (type)
			{
				case DataTypeType.ObjectReference:
				{
					ObjectReference objectReference = (ObjectReference)dataType;
					this.GetClosureOfClass(accumulator, objectReference.Name.FullName);
					return;
				}
				case DataTypeType.ObjectReferenceArray:
				{
					ArrayType arrayType = (ArrayType)dataType;
					this.AccumulateClosureOfDataType(accumulator, arrayType.ElementType);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private void AccumulateClosureOfEmbeddedType(HashSet<MofProduction> accumulator, IEnumerable<Qualifier> qualifiers)
		{
			foreach (Qualifier qualifier in qualifiers)
			{
				if (!string.Equals(qualifier.Name, "EmbeddedInstance", StringComparison.Ordinal))
				{
					continue;
				}
				this.GetClosureOfClass(accumulator, qualifier.Parameter as string);
			}
		}

		private void AccumulateQualifiers(HashSet<MofProduction> accumulator, IEnumerable<Qualifier> qualifiers)
		{
		}

		public MofProduction[] GetClosure(string[] classNames)
		{
			if (classNames != null)
			{
				HashSet<MofProduction> mofProductions = new HashSet<MofProduction>();
				string[] strArrays = classNames;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					this.GetClosureOfClass(mofProductions, str);
				}
				return mofProductions.ToArray<MofProduction>();
			}
			else
			{
				throw new ArgumentException();
			}
		}

		public void GetClosureOfClass(HashSet<MofProduction> accumulator, string className)
		{
			ClassDeclaration classDeclaration = null;
			if (accumulator != null)
			{
				if (this.ClassDeclarations.TryGetValue(className, out classDeclaration) && accumulator.Add(classDeclaration))
				{
					ClassName superclassName = classDeclaration.SuperclassName;
					if (superclassName != null)
					{
						this.GetClosureOfClass(accumulator, superclassName.FullName);
					}
					foreach (ClassName derivedClass in this.GetDerivedClasses(classDeclaration))
					{
						this.GetClosureOfClass(accumulator, derivedClass.FullName);
					}
					this.AccumulateQualifiers(accumulator, classDeclaration.Qualifiers);
					foreach (PropertyDeclaration property in classDeclaration.Properties)
					{
						this.AccumulateQualifiers(accumulator, property.Qualifiers);
						this.AccumulateClosureOfDataType(accumulator, property.DataType);
						this.AccumulateClosureOfEmbeddedType(accumulator, property.Qualifiers);
						this.AccumulateClosureOfAssociationClass(accumulator, property.Qualifiers);
					}
				}
				return;
			}
			else
			{
				throw new ArgumentException();
			}
		}

		private HashSet<ClassName> GetDerivedClasses(ClassDeclaration classDecl)
		{
			IEnumerable<ClassDeclaration> classDeclarations = this.ClassDeclarations.Values.Where<ClassDeclaration>((ClassDeclaration item) => item.SuperclassName == classDecl.Name);
			return new HashSet<ClassName>(classDeclarations.Select<ClassDeclaration, ClassName>((ClassDeclaration item) => item.Name));
		}
	}
}