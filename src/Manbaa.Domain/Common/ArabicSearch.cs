using System.Text;

namespace Manbaa.Domain.Common;

/// <summary>
/// Normalizes Arabic and Latin text so a search matches regardless of hamza forms, diacritics, tatweel, ta marbuta,
/// alef maqsura and Arabic-Indic digits ("احمد" finds "أحمد", "مُحَمَّد" finds "محمد").
/// </summary>
public static class ArabicSearch
{
    private const int MaxWords = 5;

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);

        var lastWasSpace = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsWhiteSpace(character))
            {
                if (!lastWasSpace)
                {
                    builder.Append(' ');
                }

                lastWasSpace = true;

                continue;
            }

            lastWasSpace = false;

            switch (character)
            {
                case >= 'ً' and <= 'ٟ':
                case 'ٰ':
                case 'ـ':
                case '+':
                    break;

                case 'أ' or 'إ' or 'آ' or 'ٱ':
                    builder.Append('ا');

                    break;

                case 'ى' or 'ئ':
                    builder.Append('ي');

                    break;

                case 'ة':
                    builder.Append('ه');

                    break;

                case 'ؤ':
                    builder.Append('و');

                    break;

                case >= '٠' and <= '٩':
                    builder.Append((char)('0' + (character - '٠')));

                    break;

                case >= '۰' and <= '۹':
                    builder.Append((char)('0' + (character - '۰')));

                    break;

                default:
                    builder.Append(character);

                    break;
            }
        }

        return builder.ToString().Trim();
    }

    /// <summary>Joins the parts into one normalized search string. Never empty, so rows without text are not rebuilt forever.</summary>
    public static string Build(params string?[] parts)
    {
        var text = Normalize(string.Join(' ', parts.Where(part => !string.IsNullOrWhiteSpace(part))));

        return text.Length == 0 ? "-" : text;
    }

    /// <summary>The normalized words of a search box value (at most five), or none when it is empty.</summary>
    public static IReadOnlyList<string> Words(string? search) => Normalize(search).Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().Take(MaxWords).ToList();

    /// <summary>For a local phone number typed with a leading zero (01012345678) the same number without it, as stored with the country code (+201012345678).</summary>
    public static string? PhoneVariant(string word)
    {
        if (word.Length < 4 || !word.All(char.IsAsciiDigit) || word[0] != '0')
        {
            return null;
        }

        var trimmed = word.TrimStart('0');

        return trimmed.Length >= 3 ? trimmed : null;
    }
}
