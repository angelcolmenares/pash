using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace System.Management
{
	internal static class WMIDatabaseFactory
	{
		private static XmlDocument _xmlDoc;
		private static readonly object _lock = new object();
		private const string DefaultPath = "/etc/wmi.db.xml";


		public static void Invalidate ()
		{
			lock (_lock) {
				_xmlDoc = null;
			}
		}

		private static XmlDocument Document
		{
			get {
				if (_xmlDoc == null)
				{
					if (File.Exists(DefaultPath))
					{
						lock (_lock) {
							_xmlDoc = new XmlDocument();
							_xmlDoc.Load (DefaultPath);
						}
					}
					else 
					{
						string dirPath = new FileInfo(Assembly.GetEntryAssembly ().Location).Directory.FullName;
						string path = Path.Combine(dirPath, "wmi.db.xml");
						if (File.Exists(path))
						{
							lock (_lock) {
								_xmlDoc = new XmlDocument();
								_xmlDoc.Load (path);
							}
						}
					}
				}
				return _xmlDoc;
			}
		}

		static WMIDatabaseFactory()
		{

		}

		internal static void EnsureOpen ()
		{

		}

		internal static void Open() {

		}

		internal static void Close() {


		}
		/*
		internal static IDictionary<string, UnixMetaClass> GetMetaClasses ()
		{
			var meta = new Dictionary<string, UnixMetaClass> ();
			using (var cmd = new SqliteCommand("SELECT * FROM META_CLASS", cn)) {
				using (var reader = cmd.ExecuteReader (System.Data.CommandBehavior.Default)) {
					while ( reader.Read ())
					{
						var className = reader.GetString(reader.GetOrdinal ("__CLASS"));
						meta.Add (className, new UnixMetaClass(reader.GetGuid (reader.GetOrdinal ("UID")), className, reader.GetString (reader.GetOrdinal ("__" +
							"PATH_FIELD"))));
					}
				}
			}
			return meta;
		}

		public static IDictionary<string, IEnumerable<string>> GetMetaDerivations ()
		{
			var meta = new Dictionary<string, IEnumerable<string>> ();
			using (var cmd = new SqliteCommand("SELECT * FROM META_DERIVATION", cn)) {
				using (var reader = cmd.ExecuteReader (System.Data.CommandBehavior.Default)) {
					while ( reader.Read ())
					{
						var className = reader.GetString(reader.GetOrdinal ("__CLASS"));
						if (meta.ContainsKey (className))
						{
							((List<string>)meta[className]).Add (reader.GetString (reader.GetOrdinal ("__DERIVATION")));
						}
						else {
							meta.Add (className, new List<string>(new string[] { reader.GetString (reader.GetOrdinal ("__DERIVATION")) }));
						}
					}
				}
			}
			return meta;
		}
       */


		internal static IEnumerable<UnixMetaClass> GetMetaClasses (string nameSpace)
		{
			var meta = new List<UnixMetaClass> ();
			var doc = Document;
			if (doc != null) {
				var nodes = doc.SelectNodes (string.Format("//*/Namespace[@Name='{0}']/Class", nameSpace));
				foreach(XmlNode node in nodes)
				{
					string className = node.Attributes["Name"].Value;
					if (className.Equals ("meta_class", StringComparison.OrdinalIgnoreCase)) continue;
					meta.Add (new UnixMetaClass(new Guid(node.Attributes["Id"].Value), nameSpace, className, node.Attributes["Type"].Value));
				}
			}

			return meta;
		}

		private static string GetClassNameFromQuery(string query)
		{
			int num = query.IndexOf(" from ", StringComparison.OrdinalIgnoreCase);
			string str = query.Substring(num + " from ".Length);
			char[] chrArray = new char[1];
			chrArray[0] = ' ';
			string str1 = str.Split(chrArray)[0];
			return str1;
		}
		 
		public static IEnumerable<IWbemClassObject_DoNotMarshal> Get (string nameSpace, string strQuery)
		{
			strQuery = Regex.Replace(strQuery, "Win32", "UNIX", RegexOptions.IgnoreCase);
			strQuery = Regex.Replace(strQuery, "unix_", "UNIX_", RegexOptions.IgnoreCase);
			var tableName = GetClassNameFromQuery (strQuery);

			var doc = Document;
			if (doc != null) {

				XmlNode node = doc.SelectSingleNode (string.Format ("//*/Namespace[@Name='{0}']/Class[@Name='{1}']", nameSpace, tableName));
				if (node != null)
				{
					/* Look for static object MOF reference */

					/* This is a dynamic object */
					Type type = Type.GetType (node.Attributes["Type"].Value, false, true);
					if (type != null)
					{
						if (!type.IsClass)
						{
							throw new InvalidDataException("Invalid class");
						}
						else if (type.IsAbstract)
						{
							//Find First Declarative Type in Derivation of {type}
							var fallbackNamespaceNode = node.Attributes["FallbackNamespace"];
							var fallbackClassNode = node.Attributes["FallbackClass"];
							if (fallbackClassNode != null)
							{
								string fallbackNamespace = nameSpace;
								if (fallbackNamespaceNode != null)
								{
									fallbackNamespace = fallbackNamespaceNode.Value;
									if (fallbackNamespace.Equals (nameSpace, StringComparison.OrdinalIgnoreCase) && fallbackNamespaceNode.Value.Equals (tableName, StringComparison.OrdinalIgnoreCase))
									{
										throw new InvalidDataException("Circular Dependency failed on Abstract CMI Class");
									}
									return Get (nameSpace, fallbackNamespaceNode.Value);
								}
								else {
									// Launch Declarative Type Discovery
									throw new InvalidDataException("Invalid class");
								}
							}
						}
						else
						{
							IUnixWbemClassHandler handler = GetHandler (type);
							return handler.Get(strQuery).Select (x => new UnixWbemClassObject((IUnixWbemClassHandler)x));
						}
					}
				}
			}

			return new IWbemClassObject_DoNotMarshal[0];
		}

		public static IUnixWbemClassHandler GetHandler (Type type)
		{
			if (type.IsAbstract) {
				MetaImplementationAttribute att = type.GetCustomAttribute<MetaImplementationAttribute>(true);
				if (att != null)
				{
					IUnixWbemClassHandler metaHandler = (IUnixWbemClassHandler)Activator.CreateInstance (att.ImplementationType, true);
					return metaHandler;
				}
			}
			IUnixWbemClassHandler handler = (IUnixWbemClassHandler)Activator.CreateInstance (type, true);
			return handler;
		}

	}
}