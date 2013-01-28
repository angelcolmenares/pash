namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Net.Mail;
    using System.Security.Authentication;
    using System.Text;

    [Cmdlet("Send", "MailMessage", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135256")]
    public sealed class SendMailMessage : PSCmdlet
    {
        private string[] attachments;
        private string[] bcc;
        private string body;
        private SwitchParameter bodyashtml;
        private string[] cc;
        private PSCredential credential;
        private DeliveryNotificationOptions deliverynotification;
        private System.Text.Encoding encoding = new ASCIIEncoding();
        private string from;
        private MailMessage mMailMessage = new MailMessage();
        private SmtpClient mSmtpClient;
        private int port;
        private MailPriority priority;
        private string smtpserver;
        private string subject;
        private string[] to;
        private SwitchParameter usessl;

        private void AddAddressesToMailMessage(object address, string param)
        {
            string[] strArray = address as string[];
            foreach (string str in strArray)
            {
                try
                {
                    string str2 = param;
                    if (str2 != null)
                    {
                        if (!(str2 == "to"))
                        {
                            if (str2 == "cc")
                            {
                                goto Label_0062;
                            }
                            if (str2 == "bcc")
                            {
                                goto Label_007A;
                            }
                        }
                        else
                        {
                            this.mMailMessage.To.Add(new MailAddress(str));
                        }
                    }
                    continue;
                Label_0062:
                    this.mMailMessage.CC.Add(new MailAddress(str));
                    continue;
                Label_007A:
                    this.mMailMessage.Bcc.Add(new MailAddress(str));
                }
                catch (FormatException exception)
                {
                    ErrorRecord errorRecord = new ErrorRecord(exception, "FormatException", ErrorCategory.InvalidType, null);
                    base.WriteError(errorRecord);
                }
            }
        }

        protected override void BeginProcessing()
        {
            try
            {
                this.mMailMessage.From = new MailAddress(this.from);
            }
            catch (FormatException exception)
            {
                ErrorRecord errorRecord = new ErrorRecord(exception, "FormatException", ErrorCategory.InvalidType, this.from);
                base.ThrowTerminatingError(errorRecord);
            }
            this.AddAddressesToMailMessage(this.to, "to");
            if (this.bcc != null)
            {
                this.AddAddressesToMailMessage(this.bcc, "bcc");
            }
            if (this.cc != null)
            {
                this.AddAddressesToMailMessage(this.cc, "cc");
            }
            this.mMailMessage.DeliveryNotificationOptions = this.deliverynotification;
            this.mMailMessage.Subject = this.subject;
            this.mMailMessage.Body = this.body;
            this.mMailMessage.SubjectEncoding = this.encoding;
            this.mMailMessage.BodyEncoding = this.encoding;
            this.mMailMessage.IsBodyHtml = (bool) this.bodyashtml;
            this.mMailMessage.Priority = this.priority;
            PSVariable variable = base.SessionState.Internal.GetVariable("PSEmailServer");
            if ((this.smtpserver == null) && (variable != null))
            {
                this.smtpserver = Convert.ToString(variable.Value, CultureInfo.InvariantCulture);
            }
            if (string.IsNullOrEmpty(this.smtpserver))
            {
                ErrorRecord record2 = new ErrorRecord(new InvalidOperationException(SendMailMessageStrings.HostNameValue), null, ErrorCategory.InvalidArgument, null);
                base.ThrowTerminatingError(record2);
            }
            if (this.port == 0)
            {
                this.mSmtpClient = new SmtpClient(this.smtpserver);
            }
            else
            {
                this.mSmtpClient = new SmtpClient(this.smtpserver, this.port);
            }
            if (this.usessl != 0)
            {
                this.mSmtpClient.EnableSsl = true;
            }
            if (this.credential != null)
            {
                this.mSmtpClient.UseDefaultCredentials = false;
                this.mSmtpClient.Credentials = this.credential.GetNetworkCredential();
            }
            else if (this.usessl == 0)
            {
                this.mSmtpClient.UseDefaultCredentials = true;
            }
        }

        protected override void EndProcessing()
        {
            try
            {
                this.mSmtpClient.Send(this.mMailMessage);
            }
            catch (SmtpFailedRecipientsException exception)
            {
                ErrorRecord errorRecord = new ErrorRecord(exception, "SmtpFailedRecipientsException", ErrorCategory.InvalidOperation, this.mSmtpClient);
                base.WriteError(errorRecord);
            }
            catch (SmtpException exception2)
            {
                if (exception2.InnerException != null)
                {
                    ErrorRecord record2 = new ErrorRecord(new SmtpException(exception2.InnerException.Message), "SmtpException", ErrorCategory.InvalidOperation, this.mSmtpClient);
                    base.WriteError(record2);
                }
                else
                {
                    ErrorRecord record3 = new ErrorRecord(exception2, "SmtpException", ErrorCategory.InvalidOperation, this.mSmtpClient);
                    base.WriteError(record3);
                }
            }
            catch (InvalidOperationException exception3)
            {
                ErrorRecord record4 = new ErrorRecord(exception3, "InvalidOperationException", ErrorCategory.InvalidOperation, this.mSmtpClient);
                base.WriteError(record4);
            }
            catch (AuthenticationException exception4)
            {
                ErrorRecord record5 = new ErrorRecord(exception4, "AuthenticationException", ErrorCategory.InvalidOperation, this.mSmtpClient);
                base.WriteError(record5);
            }
            this.mMailMessage.Attachments.Dispose();
        }

        protected override void ProcessRecord()
        {
            if (this.attachments != null)
            {
                string filePath = string.Empty;
                foreach (string str2 in this.attachments)
                {
                    try
                    {
                        filePath = PathUtils.ResolveFilePath(str2, this);
                    }
                    catch (ItemNotFoundException exception)
                    {
                        PathUtils.ReportFileOpenFailure(this, filePath, exception);
                    }
                    Attachment item = new Attachment(filePath);
                    this.mMailMessage.Attachments.Add(item);
                }
            }
        }

        [Alias(new string[] { "PsPath" }), Parameter(ValueFromPipeline=true), ValidateNotNullOrEmpty]
        public string[] Attachments
        {
            get
            {
                return this.attachments;
            }
            set
            {
                this.attachments = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public string[] Bcc
        {
            get
            {
                return this.bcc;
            }
            set
            {
                this.bcc = value;
            }
        }

        [Parameter(Position=2), ValidateNotNullOrEmpty]
        public string Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
            }
        }

        [Alias(new string[] { "BAH" }), Parameter]
        public SwitchParameter BodyAsHtml
        {
            get
            {
                return this.bodyashtml;
            }
            set
            {
                this.bodyashtml = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter]
        public string[] Cc
        {
            get
            {
                return this.cc;
            }
            set
            {
                this.cc = value;
            }
        }

        [Credential, ValidateNotNullOrEmpty, Parameter]
        public PSCredential Credential
        {
            get
            {
                return this.credential;
            }
            set
            {
                this.credential = value;
            }
        }

        [Parameter, Alias(new string[] { "DNO" }), ValidateNotNullOrEmpty]
        public DeliveryNotificationOptions DeliveryNotificationOption
        {
            get
            {
                return this.deliverynotification;
            }
            set
            {
                this.deliverynotification = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter, Alias(new string[] { "BE" }), ArgumentToEncodingNameTransformation]
        public System.Text.Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true)]
        public string From
        {
            get
            {
                return this.from;
            }
            set
            {
                this.from = value;
            }
        }

        [Parameter, ValidateRange(0, 0x7fffffff)]
        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        [Parameter, ValidateNotNullOrEmpty]
        public MailPriority Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                this.priority = value;
            }
        }

        [Parameter(Position=3), ValidateNotNullOrEmpty, Alias(new string[] { "ComputerName" })]
        public string SmtpServer
        {
            get
            {
                return this.smtpserver;
            }
            set
            {
                this.smtpserver = value;
            }
        }

        [ValidateNotNullOrEmpty, Alias(new string[] { "sub" }), Parameter(Mandatory=true, Position=1)]
        public string Subject
        {
            get
            {
                return this.subject;
            }
            set
            {
                this.subject = value;
            }
        }

        [Parameter(Mandatory=true, Position=0), ValidateNotNullOrEmpty]
        public string[] To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        [Parameter]
        public SwitchParameter UseSsl
        {
            get
            {
                return this.usessl;
            }
            set
            {
                this.usessl = value;
            }
        }
    }
}

