using System.Security.Cryptography;

using System.Text;

using System.Text.RegularExpressions;

namespace Manbaa.Application.Common;

public static partial class PhoneHelper
{
    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex E164Pattern();

    public static string? Normalize(string? input, string phoneCode)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var cleaned = new string(input.Where(character => char.IsDigit(character) || character == '+').ToArray());

        var codeDigits = phoneCode.TrimStart('+');

        if (cleaned.StartsWith("00"))
        {
            cleaned = "+" + cleaned[2..];
        }

        else if (!cleaned.StartsWith('+'))
        {
            if (cleaned.StartsWith('0'))
            {
                cleaned = "+" + codeDigits + cleaned[1..];
            }

            else if (cleaned.StartsWith(codeDigits) && cleaned.Length >= codeDigits.Length + 8)
            {
                cleaned = "+" + cleaned;
            }

            else
            {
                cleaned = "+" + codeDigits + cleaned;
            }
        }

        return E164Pattern().IsMatch(cleaned) ? cleaned : null;
    }

    public static bool LooksSuspicious(string e164)
    {
        var digits = e164.TrimStart('+');

        return digits.Length >= 8 && digits.Skip(digits.Length - 8).Distinct().Count() <= 2;
    }

    public static byte[] Hash(string e164) => SHA256.HashData(Encoding.UTF8.GetBytes(e164));

    public static string DigitsOnly(string e164) => e164.TrimStart('+');
}
