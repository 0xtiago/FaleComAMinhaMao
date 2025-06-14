using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FCAMM.Core.Services;

public interface ISluggerService
{
    string GenerateSlug(string input);
    string GenerateUniqueSlug(string input, Func<string, Task<bool>> existsFunc);
}

public class SluggerService : ISluggerService
{

    public string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Converter para lowercase
        string slug = input.ToLowerInvariant();

        // Remover acentos
        slug = RemoverAcentos(slug);

        // Substituir espaços e caracteres especiais por hífens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");

        // Remover hífens do início e fim
        slug = slug.Trim('-');

        // Limitar tamanho
        if (slug.Length > 100)
            slug = slug.Substring(0, 100).TrimEnd('-');

        return slug;
    }


    public string GenerateUniqueSlug(string input, Func<string, Task<bool>> existsFunc)
    {
        var baseSlug = GenerateSlug(input);
        var slug = baseSlug;
        var counter = 1;

        // Verificar se o slug já existe e adicionar contador se necessário
        while (existsFunc(slug).Result)
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
    
    private static string RemoverAcentos(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Normalizar string para decompor caracteres acentuados
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            // Manter apenas caracteres que não são acentos
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}