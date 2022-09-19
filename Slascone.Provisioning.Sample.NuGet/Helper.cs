﻿using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Management;

namespace Slascone.Provisioning.Sample.NuGet;

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

    /// <summary>
    /// Validates the authority by signature with an asymmetric key
    /// </summary>
    /// <returns>True if Signature is valid. False if Signature is invalid.</returns>
    public static bool IsFileSignatureValid(XmlDocument licenseXml)
    {
        byte[] rawData = ReadFile(Certificate);
        // Load the certificate into an X509Certificate object.
        var signatureKeyCert = new X509Certificate2(rawData);

        using (RSA rsa = signatureKeyCert.GetRSAPublicKey())
        {
            SignedXml signedXml = new SignedXml(licenseXml);
            XmlNodeList nodeList = licenseXml.GetElementsByTagName("Signature");

            signedXml.LoadXml((XmlElement)nodeList[0]);
            if (signedXml.CheckSignature(rsa))
            {
                return true;
            }
            return false;
        }
    }


    /// <summary>
    /// Read a File
    /// </summary>
    /// <param name="fileName">file name</param>
    /// <returns></returns>
    private static byte[] ReadFile(string fileName)
    {
        FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        int size = (int)f.Length;
        byte[] data = new byte[size];
        size = f.Read(data, 0, size);
        f.Close();
        return data;
    }
}