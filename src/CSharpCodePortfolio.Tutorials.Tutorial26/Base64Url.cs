using System.Text;
using System.Text.Json;

namespace CSharpCodePortfolio.Tutorials.Tutorial26;

internal static class Base64Url
{
    public static string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static byte[] Decode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch
        {
            0 => string.Empty,
            2 => "==",
            3 => "=",
            _ => throw new FormatException("Base64Url inválido.")
        };

        return Convert.FromBase64String(padded);
    }

    public static string EncodeJson(IReadOnlyDictionary<string, string> value)
    {
        return Encode(JsonSerializer.SerializeToUtf8Bytes(value));
    }

    public static IReadOnlyDictionary<string, string> DecodeJson(string value)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(Decode(value))
            ?? new Dictionary<string, string>();
    }

    public static string EncodeText(string value)
    {
        return Encode(Encoding.UTF8.GetBytes(value));
    }
}
