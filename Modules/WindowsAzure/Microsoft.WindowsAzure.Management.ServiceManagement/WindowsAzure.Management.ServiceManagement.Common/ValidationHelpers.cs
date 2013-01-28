using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Common
{
	public class ValidationHelpers
	{
		private const int PasswordMinComplexity = 3;

		private const int PasswordMinLength = 8;

		private const int PasswordMaxLength = 123;

		private const int LinuxUserNameMinLength = 1;

		private const int LinuxUserNameMaxLength = 64;

		private const int LinuxPasswordMinLength = 6;

		private const int LinuxPasswordMaxLength = 72;

		private const int WindowsComputerNameMaxLength = 15;

		private readonly static char[] WindowsComputerNameInvalidChars;

		private readonly static Regex NumericRegex;

		private readonly static Regex PasswordHasLowerChar;

		private readonly static Regex PasswordHasUpperChar;

		private readonly static Regex PasswordHasDigitChar;

		private readonly static Regex PasswordHasSpecialChar;

		private readonly static Regex[] PasswordCriteria;

		static ValidationHelpers()
		{
			ValidationHelpers.WindowsComputerNameInvalidChars = "`~!@#$%^&*()=+_[]{}\\|;:.'\",<>/?".ToCharArray();
			ValidationHelpers.NumericRegex = new Regex("^\\d+$", RegexOptions.Compiled | RegexOptions.Singleline);
			ValidationHelpers.PasswordHasLowerChar = new Regex("[a-z]", RegexOptions.Singleline | RegexOptions.CultureInvariant);
			ValidationHelpers.PasswordHasUpperChar = new Regex("[A-Z]", RegexOptions.Singleline | RegexOptions.CultureInvariant);
			ValidationHelpers.PasswordHasDigitChar = new Regex("\\d", RegexOptions.Singleline | RegexOptions.CultureInvariant);
			ValidationHelpers.PasswordHasSpecialChar = new Regex("\\W", RegexOptions.Singleline | RegexOptions.CultureInvariant);
			Regex[] passwordHasLowerChar = new Regex[4];
			passwordHasLowerChar[0] = ValidationHelpers.PasswordHasLowerChar;
			passwordHasLowerChar[1] = ValidationHelpers.PasswordHasUpperChar;
			passwordHasLowerChar[2] = ValidationHelpers.PasswordHasDigitChar;
			passwordHasLowerChar[3] = ValidationHelpers.PasswordHasSpecialChar;
			ValidationHelpers.PasswordCriteria = passwordHasLowerChar;
		}

		public ValidationHelpers()
		{
		}

		public static bool IsLinuxHostNameValid(string hostName)
		{
			if (!string.IsNullOrEmpty(hostName))
			{
				if (hostName.Length <= 64)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool IsLinuxPasswordValid(string password)
		{
			if (password.Length < 6 || password.Length > 72)
			{
				return false;
			}
			else
			{
				int num = ValidationHelpers.PasswordCriteria.Count<Regex>((Regex criteria) => criteria.IsMatch(password));
				if (num >= 3)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool IsWindowsComputerNameValid(string computerName)
		{
			if (!string.IsNullOrEmpty(computerName))
			{
				if (computerName.Length > 15 || computerName.IndexOfAny(ValidationHelpers.WindowsComputerNameInvalidChars) != -1 || ValidationHelpers.NumericRegex.IsMatch(computerName))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool IsWindowsPasswordValid(string password)
		{
			if (password.Length < 8 || password.Length > 123)
			{
				return false;
			}
			else
			{
				int num = ValidationHelpers.PasswordCriteria.Count<Regex>((Regex criteria) => criteria.IsMatch(password));
				if (num >= 3)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}
}