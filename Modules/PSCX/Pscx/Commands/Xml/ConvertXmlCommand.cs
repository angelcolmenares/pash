//---------------------------------------------------------------------
// Author: Keith Hill, jachymko
//
// Description: Class to implement the Convet-Xml cmdlet which applies
//              the supplied XSL tranform to the supplied XML.
//
// Creation Date: Sept 24, 2006
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

using Microsoft.PowerShell.Commands;

namespace Pscx.Commands.Xml
{
    [OutputType(typeof(string))]
    [Cmdlet(VerbsData.Convert, PscxNouns.Xml, DefaultParameterSetName = ParameterSetPath)]
    [RelatedLink(typeof(TestXmlCommand))]
    [RelatedLink(typeof(FormatXmlCommand))]
    [ProviderConstraint(typeof(FileSystemProvider))]
    public class ConvertXmlCommand : XmlCommandBase
    {
        private XslCompiledTransform _xslTransform;
        private SwitchParameter _enableScript;
        private string _xsltPath;
        private ConformanceLevel _conformanceLevel = ConformanceLevel.Auto;

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string XsltPath
        {
            get { return _xsltPath; }
            set { _xsltPath = value; }
        }

        [Parameter]
        public SwitchParameter EnableScript
        {
            get { return _enableScript; }
            set { _enableScript = value; }
        }

        [Parameter]
        [DefaultValue("Auto")]
        public ConformanceLevel ConformanceLevel
        {
            get { return _conformanceLevel; }
            set { _conformanceLevel = value; }
        }

        protected override XmlReaderSettings XmlReaderSettings
        {
            get
            {
                XmlReaderSettings settings = base.XmlReaderSettings;
                settings.ConformanceLevel = _conformanceLevel;
                return settings;
            }
        }

        protected override XmlWriterSettings XmlWriterSettings
        {
            get 
            {
                // Helpful blog post: http://blogs.msdn.com/eriksalt/archive/2005/07/27/OutputSettings.aspx
                XmlWriterSettings settings = _xslTransform.OutputSettings.Clone();
                settings.CloseOutput = true;
                settings.ConformanceLevel = _conformanceLevel;
                return settings; 
            }
        }

        protected virtual XsltSettings XsltSettings
        {
            get { return new XsltSettings(false, _enableScript); }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            try
            {
                string xslt = GetUnresolvedProviderPathFromPSPath(_xsltPath);

                _xslTransform = new XslCompiledTransform();
                _xslTransform.Load(xslt, XsltSettings, XmlUrlResolver);
            }
            catch (XsltException exc)
            {
                ErrorHandler.WriteXsltError(exc);
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception exc)
            {
                ErrorHandler.WriteFileError(_xsltPath, exc);
            }
        }

        protected override void ProcessXmlReader(XmlReader xmlReader, XmlWriter xmlWriter)
        {
            try
            {
                _xslTransform.Transform(xmlReader, xmlWriter);
            }
            catch (Exception exc)
            {
                ErrorHandler.WriteXsltError(exc);
                return;
            }
        }
    }
}