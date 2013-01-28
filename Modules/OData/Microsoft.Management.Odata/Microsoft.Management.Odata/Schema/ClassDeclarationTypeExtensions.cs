using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.MofParser.ParseTree;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity.Core;

namespace Microsoft.Management.Odata.Schema
{
	internal static class ClassDeclarationTypeExtensions
	{
		public static SchemaLoader.ClassCategory GetCategory(this ClassDeclaration classDecl)
		{
			bool qualifier = classDecl.GetQualifier("ComplexType") != null;
			bool flag = classDecl.GetQualifier("Association") != null;
			if (!qualifier || !flag)
			{
				if (!qualifier)
				{
					if (!flag)
					{
						return SchemaLoader.ClassCategory.Entity;
					}
					else
					{
						return SchemaLoader.ClassCategory.Association;
					}
				}
				else
				{
					return SchemaLoader.ClassCategory.Complex;
				}
			}
			else
			{
				object[] fullName = new object[1];
				fullName[0] = classDecl.Name.FullName;
				throw new MetadataException(ExceptionHelpers.GetExceptionMessage(Resources.ComplexTypeWithAssociation, fullName));
			}
		}

		public static string GetFullClassName(this MofProduction production)
		{
			ClassDeclaration classDeclaration = production as ClassDeclaration;
			if (classDeclaration != null)
			{
				return classDeclaration.Name.FullName;
			}
			else
			{
				return null;
			}
		}

		public static PropertyDeclaration GetProperty(this ClassDeclaration classDecl, string propertyName, HashSet<MofProduction> mof)
		{
			Func<PropertyDeclaration, bool> func = null;
			Func<MofProduction, bool> func1 = null;
			while (true)
			{
				NodeList<PropertyDeclaration> properties = classDecl.Properties;
				if (func == null)
				{
					func = (PropertyDeclaration it) => string.Equals(it.Name, propertyName, StringComparison.Ordinal);
				}
				PropertyDeclaration propertyDeclaration = properties.FirstOrDefault<PropertyDeclaration>(func);
				if (propertyDeclaration != null)
				{
					return propertyDeclaration;
				}
				if (classDecl.SuperclassName == null)
				{
					break;
				}
				HashSet<MofProduction> mofProductions = mof;
				if (func1 == null)
				{
					func1 = (MofProduction item) => item.GetFullClassName() == classDecl.SuperclassName.FullName;
				}
				classDecl = mofProductions.First<MofProduction>(func1) as ClassDeclaration;
			}
			object[] fullClassName = new object[2];
			fullClassName[0] = propertyName;
			fullClassName[1] = classDecl.GetFullClassName();
			throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.PropertyNotFoundInODataResource, fullClassName));
		}

		public static bool IsClassAndSuperclassesContainsLoop(this ClassDeclaration classDecl, HashSet<MofProduction> mof)
		{
			Func<MofProduction, bool> func = null;
			while (classDecl != null && classDecl.SuperclassName != null)
			{
				HashSet<MofProduction> mofProductions = mof;
				if (func == null)
				{
					func = (MofProduction item) => item.GetFullClassName() == classDecl.SuperclassName.FullName;
				}
				var temp = mofProductions.First<MofProduction>(func) as ClassDeclaration;
				if (classDecl != temp)
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public static bool IsComplexType(this MofProduction mofProduction)
		{
			ClassDeclaration classDeclaration = mofProduction as ClassDeclaration;
			if (classDeclaration == null)
			{
				return false;
			}
			else
			{
				return classDeclaration.GetQualifier("ComplexType") != null;
			}
		}

		public static bool IsEntityType(this MofProduction mofProduction)
		{
			ClassDeclaration classDeclaration = mofProduction as ClassDeclaration;
			if (classDeclaration == null)
			{
				return false;
			}
			else
			{
				return classDeclaration.GetQualifier("ComplexType") == null;
			}
		}
	}
}