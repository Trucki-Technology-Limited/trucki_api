using System.Text.RegularExpressions;

namespace trucki.Utility
{
    public static class Util
    {
        public static string ValidUserRegistrationPassword(string password)
        {
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");
            //var hasNumber = new Regex(@"[0-9]+");
            //var hasMiniMaxChars = new Regex(@".{8,15}");
            //var hasLowerChar = new Regex(@"[a-z]+");

            if (password.Length < 8)
            {
                return "Password should not be less than or greater than 12 characters";
            }
            else if (!hasUpperChar.IsMatch(password))
            {
                return "Password should contain At least one upper case letter";
            }
            else if (!hasSymbols.IsMatch(password))
            {
                return "Password should contain At least one special case characters";
            }
            else
            {
                return "passed";
            }

        }
    }
}
