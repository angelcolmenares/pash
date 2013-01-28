namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser.Parser
{
	internal enum Tokens
	{
		error = 1,
		EOF = 2,
		IDENTIFIER = 3,
		STRING = 4,
		ISSUE = 5,
		ADD = 6,
		IMPLY = 7,
		TYPE = 8,
		VALUE = 9,
		VALUE_TYPE = 10,
		PROPERTY = 11,
		CLAIM = 12,
		SEMICOLON = 13,
		COLON = 14,
		COMMA = 15,
		DOT = 16,
		O_SQ_BRACKET = 17,
		C_SQ_BRACKET = 18,
		O_BRACKET = 19,
		C_BRACKET = 20,
		INT64_TYPE = 21,
		UINT64_TYPE = 22,
		STRING_TYPE = 23,
		BOOLEAN_TYPE = 24,
		EQ = 25,
		NEQ = 26,
		REGEXP_MATCH = 27,
		REGEXP_NOT_MATCH = 28,
		ASSIGN = 29,
		AND = 30
	}
}