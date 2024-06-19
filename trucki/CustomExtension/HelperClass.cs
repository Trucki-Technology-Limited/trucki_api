namespace trucki.CustomExtension;

public class HelperClass
{
    public static string GenerateRandomPassword(int length = 6)
    {
        // Define character sets for password generation
        const string lowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        const string uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string symbols = "@£#$";

        // Combine all character sets (adjust as needed)
        string chars = lowercaseLetters + uppercaseLetters + digits + symbols;

        // Create a random number generator
        var random = new Random();

        // Generate a random password of the specified length
        var password = new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return password;
    }
}