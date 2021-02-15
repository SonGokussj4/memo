using System.Linq;
using System.Text;
using System.Globalization;

// namespace here
public static class Utility
{
    public static string RemoveDiacritics(this string str)
    {
        if (null == str) return null;
        var chars =
            from c in str.Normalize(NormalizationForm.FormD).ToCharArray()
            // from c in str.ToLower().Normalize(NormalizationForm.FormD).ToCharArray()
            let uc = CharUnicodeInfo.GetUnicodeCategory(c)
            where uc != UnicodeCategory.NonSpacingMark
            select c;

        var cleanStr = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);

        return cleanStr;
    }

    // or, alternatively
    public static string RemoveDiacritics2(this string str)
    {
        if (null == str) return null;
        var chars = str
            // .ToLower()
            .Normalize(NormalizationForm.FormD)
            .ToCharArray()
            .Where(c=> CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(chars).Normalize(NormalizationForm.FormC);
    }

    // or from most popular SO answer
    //
    // public static string RemoveDiacritics3(this string text)
    // {
    //     var normalizedString = text.Normalize(NormalizationForm.FormD);
    //     var stringBuilder = new StringBuilder();

    //     foreach (var c in normalizedString)
    //     {
    //         var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
    //         if (unicodeCategory != UnicodeCategory.NonSpacingMark)
    //         {
    //             stringBuilder.Append(c);
    //         }
    //     }

    //     return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    // }
}
