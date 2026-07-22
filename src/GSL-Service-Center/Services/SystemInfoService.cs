using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace GSL_Rettungsring.Services;

internal sealed class SystemInfoService
{
    public string GetSystemInfo()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Computername: {Environment.MachineName}");
        builder.AppendLine($"Benutzer: {Environment.UserDomainName}\\{Environment.UserName}");
        builder.AppendLine($"Betriebssystem: {GetOperatingSystemName()}");
        builder.AppendLine($"Architektur: {RuntimeInformation.OSArchitecture}");
        builder.AppendLine($".NET: {RuntimeInformation.FrameworkDescription}");
        builder.AppendLine($"IP-Adressen: {GetIpAddresses()}");
        builder.AppendLine($"MAC-Adressen: {GetMacAddresses()}");

        return builder.ToString();
    }

    private static string GetOperatingSystemName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var productName = GetValue(key, "ProductName");
            var displayVersion = GetValue(key, "DisplayVersion");
            var currentBuild = GetValue(key, "CurrentBuild");
            var ubr = GetValue(key, "UBR");

            if (string.IsNullOrWhiteSpace(productName))
            {
                return RuntimeInformation.OSDescription;
            }

            if (IsWindows11Build(currentBuild) &&
                productName.StartsWith("Windows 10", StringComparison.OrdinalIgnoreCase))
            {
                productName = "Windows 11" + productName["Windows 10".Length..];
            }

            var result = productName;

            if (!string.IsNullOrWhiteSpace(displayVersion))
            {
                result += $" {displayVersion}";
            }

            if (!string.IsNullOrWhiteSpace(currentBuild))
            {
                result += $" Build {currentBuild}";

                if (!string.IsNullOrWhiteSpace(ubr))
                {
                    result += $".{ubr}";
                }
            }

            return result;
        }
        catch
        {
            return RuntimeInformation.OSDescription;
        }
    }

    private static bool IsWindows11Build(string build)
    {
        return int.TryParse(build, out var buildNumber) && buildNumber >= 22000;
    }

    private static string GetValue(RegistryKey? key, string name)
    {
        return key?.GetValue(name)?.ToString()?.Trim() ?? string.Empty;
    }

    private static string GetIpAddresses()
    {
        try
        {
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
                .SelectMany(adapter => adapter.GetIPProperties().UnicastAddresses)
                .Where(address => address.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(address => address.Address.ToString())
                .Distinct()
                .ToArray();

            return addresses.Length > 0 ? string.Join(", ", addresses) : "Keine";
        }
        catch
        {
            return "Unbekannt";
        }
    }

    private static string GetMacAddresses()
    {
        try
        {
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
                .Select(adapter => adapter.GetPhysicalAddress()?.ToString())
                .Where(address => !string.IsNullOrWhiteSpace(address))
                .Distinct()
                .ToArray();

            return addresses.Length > 0 ? string.Join(", ", addresses) : "Keine";
        }
        catch
        {
            return "Unbekannt";
        }
    }
}
