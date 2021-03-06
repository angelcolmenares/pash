/*
	Copyright (c) 2012 Code Owls LLC

	Permission is hereby granted, free of charge, to any person obtaining a copy 
	of this software and associated documentation files (the "Software"), to 
	deal in the Software without restriction, including without limitation the 
	rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
	sell copies of the Software, and to permit persons to whom the Software is 
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in 
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
	IN THE SOFTWARE. 
*/
using System.Xml.Linq;

namespace Mono.Data.PowerShell.Provider
{
    public static class Names
    {
        public static XName Collection = XName.Get("collection", Namespaces.App);
        public static XName Title = XName.Get("title", Namespaces.Atom);
        public static XName Link = XName.Get("link", Namespaces.Atom);
        public static XName Entry = XName.Get("entry", Namespaces.Atom);
        public static XName Id = XName.Get("id", Namespaces.Atom); 
        public static XName TitleAttribute = XName.Get("title"); 
        public static XName HrefAttribute = XName.Get("href");
        public static XName RelAttribute = XName.Get("rel");
        public static XName Properties = XName.Get( "properties", Namespaces.Metadata );
        public static XName XmlBase = XName.Get("base", Namespaces.Xml);
        public static XName Content = XName.Get("content", Namespaces.Atom);
        public static XName SrcAttribute = XName.Get( "src");
        public static XName Category = XName.Get( "category", Namespaces.Atom );
        public static XName TermAttribute = XName.Get( "term" );
        public static XName EntitySet = XName.Get( "EntitySet", Namespaces.Edm);
        public static XName EntitySetMetadata = XName.Get("EntitySet", Namespaces.EdmMetadata);
        public static XName NameAttribute = XName.Get("Name");
        public static XName EntityTypeAttribute = XName.Get("EntityType");
        public static XName EntityType = XName.Get( "EntityType", Namespaces.Edm );
        public static XName EntityTypeMetadata = XName.Get("EntityType", Namespaces.EdmMetadata);
        public static XName Schema = XName.Get( "Schema", Namespaces.Edm );
        public static XName PropertyRef = XName.Get( "PropertyRef", Namespaces.Edm );
        public static XName PropertyRefMetadata = XName.Get("PropertyRef", Namespaces.EdmMetadata);
        public static XName NamespaceAttribute = XName.Get("Namespace");
        public static XName PropertyTypeAttribute = XName.Get("type", Namespaces.Metadata );
        public static XName SchemaMetadata = XName.Get("Schema",Namespaces.EdmMetadata);
    }
}
