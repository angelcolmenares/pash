using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;

namespace System.Management.Classes
{
	[Guid("a3d024ed-a234-4b76-b909-f17a75f28327")]
	internal class Meta_Class : IUnixWbemClassHandler
	{
		private Dictionary<string, object> _prop = new Dictionary<string, object>();
		private readonly Dictionary<string, UnixWbemPropertyInfo> _propInfos;
		private readonly Dictionary<string, UnixWbemPropertyInfo> _systemPropInfos;
		private readonly Dictionary<string, UnixWbemQualiferInfo> _qualifiers;
		private Dictionary<string, UnixCimMethodInfo> _methods;
		private IEnumerator<UnixCimMethodInfo> _methodEnumerator;


		public Meta_Class ()
		{
			_propInfos = new Dictionary<string, UnixWbemPropertyInfo>();
			_systemPropInfos = new Dictionary<string, UnixWbemPropertyInfo>();
			_qualifiers = new Dictionary<string, UnixWbemQualiferInfo>();
			_methods = new Dictionary<string, UnixCimMethodInfo>();
			RegisterProperies();
		}

		public virtual IUnixWbemClassHandler New()
		{
			return (IUnixWbemClassHandler)Activator.CreateInstance (this.GetType ());
		}
		
		protected virtual void RegisterProperies()
		{
			RegisterSystemProperty ("__GENUS", CimType.SInt32, 0);
			RegisterSystemProperty ("__UID", CimType.Object, 0);
			RegisterSystemProperty ("__SERVER", CimType.String, 0);
			RegisterSystemProperty ("__NAMESPACE", CimType.String, 0);
			RegisterSystemProperty ("__CLASS", CimType.String, 0);
			RegisterSystemProperty ("__IMPLEMENTATION_TYPE", CimType.String, 0);
			RegisterSystemProperty ("__DERIVATION", CimType.String, 0);
			RegisterSystemProperty ("__RELATIVE_PATH", CimType.String, 0);
			RegisterSystemProperty ("__PATH", CimType.String, 0);
		}

		protected virtual void RegisterProperty(string name, CimType type, int flavor)
		{
			if (_propInfos.ContainsKey (name))
			_propInfos[name] = new UnixWbemPropertyInfo { Name = name, Type = type, Flavor = flavor};
			else 
				_propInfos.Add (name, new UnixWbemPropertyInfo { Name = name, Type = type, Flavor = flavor});
		}
		
		protected virtual void RegisterSystemProperty(string name, CimType type, int flavor)
		{
			
			if (_propInfos.ContainsKey (name))
			_systemPropInfos[name] = new UnixWbemPropertyInfo { Name = name, Type = type, Flavor = flavor};
			else 
				_systemPropInfos.Add (name, new UnixWbemPropertyInfo { Name = name, Type = type, Flavor = flavor});
		}


		#region IUnixWbemClassHandler implementation

		public System.Collections.Generic.IEnumerable<object> Get (string strQuery)
		{
			var queryable = (IQueryable<UnixMetaClass>)new QueryParser<UnixMetaClass>().Parse (WMIDatabaseFactory.GetMetaClasses ("root/cimv2").AsQueryable (), strQuery);
			return queryable.Select (x => New().Get(x));
		}

		public IUnixWbemClassHandler WithProperty(string key, object obj)
		{
			AddProperty (key, obj);
			return this;
		}
		
		public IUnixWbemClassHandler WithMethod (string key, UnixCimMethodInfo methodInfo)
		{
			AddMethod (key, methodInfo);
			return this;
		}

		public object Get (object nativeObj)
		{
			var obj = nativeObj as UnixMetaClass;
			Type targetType = Type.GetType (obj.ImplementationType, false, true);
			if (targetType != null) 
			{
				IUnixWbemClassHandler targetHandler = WMIDatabaseFactory.GetHandler (targetType);
				foreach (var p in targetHandler.PropertyInfos)
				{				
					RegisterProperty (p.Name, p.Type, p.Flavor);
					_prop.Add (p.Name, null);
				}

				foreach(var qualifierName in targetHandler.QualifierNames)
				{
					_qualifiers.Add (qualifierName, targetHandler.GetQualifier (qualifierName));
				}

				foreach(var method in targetHandler.Methods)
				{
					AddMethod (method.Name, method);
				}
			}
			if (obj != null) 
			{
				_prop.Add ("__GENUS", 1);
				_prop.Add ("__SERVER", System.Net.Dns.GetHostName ().ToLower ());
				_prop.Add ("__NAMESPACE", obj.Namespace);
				_prop.Add ("__UID", obj.Id);
				_prop.Add ("__CLASS", obj.ClassName);
				_prop.Add ("__IMPLEMENTATION_TYPE", obj.ImplementationType);
				_prop.Add ("__DERIVATION", GetDerivations (targetType));
				_prop.Add ("__PATH", string.Format ("//{0}/{1}/{2}/{3}=\"{4}\"", _prop ["__SERVER"], _prop ["__NAMESPACE"], "META_CLASS", PathField, _prop [PathField]));
				_prop.Add ("__RELATIVE_PATH", string.Format ("{0}/{1}=\"{2}\"", _prop ["__CLASS"], PathField, _prop [PathField]));
			}

			return this;
		}

		private string[] GetDerivations(Type targetType)
		{
			Type type = targetType;
			var list = new List<string>();
			do 
			{
				list.Add (type.BaseType.Name);
				type = type.BaseType;
			}
			while(type.BaseType != null && type.BaseType != typeof(object));
			
			return list.ToArray ();
		}
		
		/// <summary>
		/// Adds the property.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
		public virtual void AddProperty (string key, object obj)
		{
			if (_prop.ContainsKey (key)) {
				_prop [key] = obj;
			} else {
				_prop.Add (key, obj);
			}
		}

		public void AddMethod (string key, UnixCimMethodInfo method)
		{
			if (_methods.ContainsKey (key)) {
				_methods [key] = method;
			} else {
				_methods.Add (key, method);
			}
		}

		/// <summary>
		/// Gets the property.
		/// </summary>
		/// <returns>
		/// The property.
		/// </returns>
		/// <param name='key'>
		/// Key.
		/// </param>
		public virtual object GetProperty (string key)
		{
			var realKey = _prop.Keys.FirstOrDefault (x => x.Equals (key, StringComparison.OrdinalIgnoreCase));
			if (!string.IsNullOrEmpty (realKey))
			{
				return _prop[realKey];
			}
			return  null;
		}
		
		/// <summary>
		/// Invokes the method.
		/// </summary>
		/// <returns>
		/// The method.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		public virtual IUnixWbemClassHandler InvokeMethod (string methodName, IUnixWbemClassHandler obj)
		{
			return null;
		}

		public virtual IDictionary<string, object> Properties { get { return _prop; } }

		public string PathField {
			get { return "__CLASS"; }
		}
		
		public UnixWbemQualiferInfo GetQualifier (string name)
		{
			return _qualifiers[name];
		}
		
		public UnixWbemQualiferInfo GetQualifier (int index)
		{
			return _qualifiers.ElementAt (index).Value;
		}

		public IEnumerable<UnixWbemQualiferInfo> GetQualifiers ()
		{
			return _qualifiers.Values;
		}
		
		public virtual IEnumerable<string> QualifierNames { get { return _qualifiers.Keys; } }

		public virtual IEnumerable<string> PropertyNames { get { return _propInfos.Keys; } }
		
		public virtual IEnumerable<UnixWbemPropertyInfo> PropertyInfos { get { return _propInfos.Values; } }
		
		public virtual IEnumerable<UnixWbemPropertyInfo> SystemPropertyInfos { get { return _systemPropInfos.Values; } }
		
		public virtual IEnumerable<string> SystemPropertyNames { get { return _systemPropInfos.Keys; } }

		public IEnumerable<string> MethodNames { get { return _methods.Keys; } }
		
		public IEnumerable<UnixCimMethodInfo> Methods { get { return _methods.Values; } }
	
		public UnixCimMethodInfo NextMethod ()
		{
			if (_methodEnumerator == null)
				_methodEnumerator = Methods.GetEnumerator ();
			if (_methodEnumerator.MoveNext ()) {
				return _methodEnumerator.Current;
			} else {
				_methodEnumerator = null;
			}
			return default(UnixCimMethodInfo);
		}


		#endregion
	}
}

