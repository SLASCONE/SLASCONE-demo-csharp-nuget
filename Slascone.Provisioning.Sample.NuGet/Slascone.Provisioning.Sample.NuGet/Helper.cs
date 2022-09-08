﻿namespace Slascone.Provisioning.Sample.NuGet;

public class Helper
{
    static Helper()
    {

    }

    #region Main values - Fill according to your environment

    // ToDo: Exchange the value of the variables to your specific tenant.
    public const string ApiBaseUrl = "https://api.slascone.com";
    public const string ProvisioningKey = "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D7ashHxFtZOR142LYgKWdNocibDgN75/P58YNvUZafLdaie7eGwI/2gX/XuDPtqDW";
    public const string IsvId = "2af5fe02-6207-4214-946e-b00ac5309f53";

    #endregion

    #region Encryption and Digital Signing

    //https://support.slascone.com/hc/en-us/articles/360016063637-DIGITAL-SIGNATURE-AND-DATA-INTEGRITY

    //0 = none, 1 = symmetric, 2 = assymetric
    public const int SignatureValidationMode = 0;
    //Only for symmetric encryption
    public const string SymmetricEncryptionKey = "";
    //Only for assymetric encryption - The path to the certificate.
    public const string Certificate = "signature_pub_key.pfx";

    #endregion

    /// <summary>
    /// Get a unique device id based on the system
    /// </summary>
    /// <returns>UUID via string</returns>
    public static string GetWindowsUniqueDeviceId()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct"))
        {
            var shares = searcher.Get();
            var props = shares.Cast<ManagementObject>().First().Properties;
            var uuid = props["UUID"].Value as string;

            return uuid;
        }
    }

    public static string GetOperatingSystem()
    {
        return RuntimeInformation.OSDescription;
    }
}
