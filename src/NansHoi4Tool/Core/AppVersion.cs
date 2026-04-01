using System.Reflection;

namespace NansHoi4Tool.Core;

/// <summary>Centralised version helpers — reads from AssemblyInformationalVersion.</summary>
public static class AppVersion
{
    private static string? _cached;

    public static string Current => _cached ??=
        Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "0.0.2";

    public static string Display => $"v{Current}";

    public static string Full => $"Nan's Hoi4 Tool  {Display}";
}
