using System.Text;

namespace UrlShortener.Application.Services;

public interface ICodeGenerationService
{
    string GenerateCode(int length = 7);
    long DecodeToId(string code);
    string EncodeFromId(long id);
}
public class Base62CodeGenerationService : ICodeGenerationService
{
    private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int DefaultLength = 7;
    private static readonly Random Random = new();
    public string GenerateCode(int length = DefaultLength)
    {
        var result = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
        {
            result.Append(Base62Chars[Random.Next(Base62Chars.Length)]);
        }
        
        return result.ToString();
    }
    public string EncodeFromId(long id)
    {
        if (id == 0) return Base62Chars[0].ToString();
        var result = new StringBuilder();
        
        while (id > 0)
        {
            result.Insert(0, Base62Chars[(int)(id % 62)]);
            id /= 62;
        }
        
        return result.ToString();
    }
    public long DecodeToId(string code)
    {
        long result = 0;
        long multiplier = 1;
        
        for (int i = code.Length - 1; i >= 0; i--)
        {
            var charIndex = Base62Chars.IndexOf(code[i]);
            if (charIndex == -1)
                throw new ArgumentException($"Invalid character '{code[i]}' in code");
                
            result += charIndex * multiplier;
            multiplier *= 62;
        }
        
        return result;
    }
}