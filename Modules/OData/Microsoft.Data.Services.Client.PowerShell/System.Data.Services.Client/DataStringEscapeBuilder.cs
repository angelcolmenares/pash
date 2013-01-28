namespace System.Data.Services.Client
{
    using System;
    using System.Text;

    internal class DataStringEscapeBuilder
    {
        private int index;
        private readonly string input;
        private readonly StringBuilder output = new StringBuilder();
        private StringBuilder quotedDataBuilder;
        private const string SensitiveCharacters = "+";

        private DataStringEscapeBuilder(string dataString)
        {
            this.input = dataString;
        }

        private string Build()
        {
            this.index = 0;
            while (this.index < this.input.Length)
            {
                char quoteStart = this.input[this.index];
                switch (quoteStart)
                {
                    case '\'':
                    case '"':
                        this.ReadQuotedString(quoteStart);
                        break;

                    default:
                        if ("+".IndexOf(quoteStart) >= 0)
                        {
                            this.output.Append(Uri.EscapeDataString(quoteStart.ToString()));
                        }
                        else
                        {
                            this.output.Append(quoteStart);
                        }
                        break;
                }
                this.index++;
            }
            return this.output.ToString();
        }

        internal static string EscapeDataString(string input)
        {
            DataStringEscapeBuilder builder = new DataStringEscapeBuilder(input);
            return builder.Build();
        }

        private void ReadQuotedString(char quoteStart)
        {
            if (this.quotedDataBuilder == null)
            {
                this.quotedDataBuilder = new StringBuilder();
            }
            this.output.Append(quoteStart);
            while (++this.index < this.input.Length)
            {
                if (this.input[this.index] == quoteStart)
                {
                    this.output.Append(Uri.EscapeDataString(this.quotedDataBuilder.ToString()));
                    this.output.Append(quoteStart);
                    this.quotedDataBuilder.Clear();
                    break;
                }
                this.quotedDataBuilder.Append(this.input[this.index]);
            }
            if (this.quotedDataBuilder.Length > 0)
            {
                this.output.Append(Uri.EscapeDataString(this.quotedDataBuilder.ToString()));
                this.quotedDataBuilder.Clear();
            }
        }
    }
}

