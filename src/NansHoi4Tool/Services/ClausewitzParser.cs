using NansHoi4Tool.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace NansHoi4Tool.Services;

// ── Clausewitz AST ───────────────────────────────────────────────────────────

public class CNode
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public List<CNode> Children { get; set; } = new();
    public bool IsBlock => Children.Count > 0 || Value == null;

    public string GetString(string key, string defaultVal = "")
        => Children.FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value ?? defaultVal;

    public int GetInt(string key, int defaultVal = 0)
        => int.TryParse(GetString(key), out var v) ? v : defaultVal;

    public double GetDouble(string key, double defaultVal = 0)
        => double.TryParse(GetString(key), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : defaultVal;

    public bool GetBool(string key, bool defaultVal = false)
    {
        var s = GetString(key, "");
        if (s == "yes") return true;
        if (s == "no") return false;
        return defaultVal;
    }

    public IEnumerable<CNode> All(string key)
        => Children.Where(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

    public CNode? Block(string key)
        => Children.FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && c.IsBlock);
}

// ── Tokenizer ────────────────────────────────────────────────────────────────

internal enum TokType { Word, String, Equals, LBrace, RBrace, Eof }
internal record Token(TokType Type, string Value);

internal static class Tokenizer
{
    public static List<Token> Tokenize(string text)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            if (c == '#') { while (i < text.Length && text[i] != '\n') i++; continue; }
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (c == '=') { tokens.Add(new Token(TokType.Equals, "=")); i++; continue; }
            if (c == '{') { tokens.Add(new Token(TokType.LBrace, "{")); i++; continue; }
            if (c == '}') { tokens.Add(new Token(TokType.RBrace, "}")); i++; continue; }
            if (c == '"')
            {
                i++;
                var sb = new StringBuilder();
                while (i < text.Length && text[i] != '"') { if (text[i] == '\\') i++; sb.Append(text[i++]); }
                if (i < text.Length) i++;
                tokens.Add(new Token(TokType.String, sb.ToString()));
                continue;
            }
            {
                var sb = new StringBuilder();
                while (i < text.Length && !char.IsWhiteSpace(text[i]) &&
                       text[i] != '=' && text[i] != '{' && text[i] != '}' && text[i] != '#')
                    sb.Append(text[i++]);
                if (sb.Length > 0) tokens.Add(new Token(TokType.Word, sb.ToString()));
            }
        }
        tokens.Add(new Token(TokType.Eof, ""));
        return tokens;
    }
}

// ── Parser ───────────────────────────────────────────────────────────────────

public static class ClausewitzParser
{
    public static CNode Parse(string text)
    {
        var tokens = Tokenizer.Tokenize(text);
        int pos = 0;
        var root = new CNode { Key = "__root__" };
        ParseBlock(tokens, ref pos, root);
        return root;
    }

    private static void ParseBlock(List<Token> tokens, ref int pos, CNode parent)
    {
        while (pos < tokens.Count && tokens[pos].Type != TokType.Eof && tokens[pos].Type != TokType.RBrace)
        {
            if (tokens[pos].Type is TokType.Word or TokType.String)
            {
                string key = tokens[pos].Value;
                pos++;

                if (pos < tokens.Count && tokens[pos].Type == TokType.Equals)
                {
                    pos++;
                    if (pos < tokens.Count && tokens[pos].Type == TokType.LBrace)
                    {
                        pos++;
                        var child = new CNode { Key = key };
                        ParseBlock(tokens, ref pos, child);
                        if (pos < tokens.Count && tokens[pos].Type == TokType.RBrace) pos++;
                        parent.Children.Add(child);
                    }
                    else if (pos < tokens.Count && tokens[pos].Type is TokType.Word or TokType.String)
                    {
                        parent.Children.Add(new CNode { Key = key, Value = tokens[pos].Value });
                        pos++;
                    }
                }
                else if (pos < tokens.Count && tokens[pos].Type == TokType.LBrace)
                {
                    pos++;
                    var child = new CNode { Key = key };
                    ParseBlock(tokens, ref pos, child);
                    if (pos < tokens.Count && tokens[pos].Type == TokType.RBrace) pos++;
                    parent.Children.Add(child);
                }
                else
                {
                    parent.Children.Add(new CNode { Key = key, Value = null });
                }
            }
            else pos++;
        }
    }

    // ── HOI4 YAML localisation (.yml) ────────────────────────────────────────
    // Format: l_english:\n KEY:0 "value"\n
    public static List<(string Key, string Value)> ParseYml(string text)
    {
        var result = new List<(string, string)>();
        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.TrimStart('\uFEFF').Trim();
            if (line.StartsWith("l_") || line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;

            var colonIdx = line.IndexOf(':');
            if (colonIdx < 0) continue;
            var key = line[..colonIdx].Trim();

            var afterColon = line[(colonIdx + 1)..].TrimStart();
            // skip version number e.g. "0 " or just a quoted string
            var match = Regex.Match(afterColon, @"(?:\d+\s+)?""((?:[^""]|"""")*)""");
            if (match.Success)
                result.Add((key, match.Groups[1].Value));
        }
        return result;
    }
}
