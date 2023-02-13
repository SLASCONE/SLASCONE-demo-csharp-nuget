﻿using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Slascone.Client.DeviceInfos;

namespace Slascone.Provisioning.Sample.NuGet;

public class Helper
{
    static Helper()
    {
    }

    #region Main values - Fill according to your environment

	// ToDo: Exchange the value of the variables to your specific tenant.
	public const string ApiBaseUrl = "https://api.slascone.com";

    // Demo Tenant
    public const string ProvisioningKey = "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D7ashHxFtZOR142LYgKWdNocibDgN75/P58YNvUZafLdaie7eGwI/2gX/XuDPtqDW";
    public static Guid IsvId = Guid.Parse("2af5fe02-6207-4214-946e-b00ac5309f53");

    #endregion

    #region Encryption and Digital Signing

    //https://support.slascone.com/hc/en-us/articles/360016063637-DIGITAL-SIGNATURE-AND-DATA-INTEGRITY

    //0 = none, 1 = symmetric, 2 = assymetric
    //use 0 for initial prototyping, 2 for production
    public const int SignatureValidationMode = 2;
    //Only for symmetric encryption
    public const string SymmetricEncryptionKey = "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D42bmxuQPK5gGEseNNpFRekd/Kf059rff/N4phalkP25zVqH3VZIOlmot4jEeNr0m";
    //Only for assymetric encryption - the public key
    public const string SignaturePubKeyPem =
@"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwpigzm+cZIyw6x253YRD
mroGQyo0rO9qpOdbNAkE/FMSX+At5CQT/Cyr0eZTo2h+MO5gn5a6dwg2SYB/K1Yt
yuiKqnaEUfoPnG51KLrj8hi9LoZyIenfsQnxPz+r8XGCUPeS9MhBEVvT4ba0x9Ew
R+krU87VqfI3KNpFQVdLPaZxN4STTEZaet7nReeNtnnZFYaUt5XeNPB0b0rGfrps
y7drmZz81dlWoRcLrBRpkf6XrOTX4yFxe/3HJ8mpukuvdweUBFoQ0xOHmG9pNQ31
AHGtgLYGjbKcW4xYmpDGl0txfcipAr1zMj7X3oCO9lHcFRnXdzx+TTeJYxQX2XVb
hQIDAQAB
-----END PUBLIC KEY-----";

	#endregion

	private static string UniqueDeviceId;

    /// <summary>
    /// Get a unique device id based on the system
    /// </summary>
    /// <returns>UUID via string</returns>
    public static string GetUniqueDeviceId()
    {
        if (!string.IsNullOrEmpty(UniqueDeviceId))
            return UniqueDeviceId;

        var awsEc2Infos = new AwsEc2Infos() { TimeoutSeconds = 2 };
        var detectAws = new Task<bool>(() => awsEc2Infos.DetectAwsEcs().Result);
        detectAws.Start();
        var azureVmInfos = new AzureVmInfos() { TimeoutSeconds = 2 };
        var detectAzure = new Task<bool>(() => azureVmInfos.DetectAzureVm().Result);
        detectAzure.Start();
        var virtualizationInfos = new VirtualizationInfos();
        var detectVirtualization = new Task<bool>(() => virtualizationInfos.DetectVirtualization().Result);
        detectVirtualization.Start();

        Task.WaitAll(detectAws, detectAzure, detectVirtualization);

        var awsDetected = detectAws.Result;
        var azureDetected = detectAzure.Result;
        var virtualizationDetected = detectVirtualization.Result;

        if (awsDetected)
        {
            return UniqueDeviceId = awsEc2Infos.InstanceId;
        }
        else if (azureDetected)
        {
            return UniqueDeviceId = azureVmInfos.VmId;
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return UniqueDeviceId = WindowsDeviceInfos.ComputerSystemProductId;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var deviceId = string.Concat(LinuxDeviceInfos.MachineId, LinuxDeviceInfos.RootDeviceSerial);

                return UniqueDeviceId = BitConverter.ToString(MD5.HashData(UTF8Encoding.UTF8.GetBytes(deviceId)));
            }

            throw new NotSupportedException("GetUniqueDeviceId() is supported only on Windows and Linux");
        }
    }

    public static string GetOperatingSystem()
    {
	    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
	    {
		    return WindowsDeviceInfos.OperatingSystem;
	    }

	    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
	    {
		    return LinuxDeviceInfos.OSVersion;
	    }

        return RuntimeInformation.OSDescription;
    }

    /// <summary>
    /// Validates the authority by signature with an asymmetric key
    /// </summary>
    /// <returns>True if Signature is valid. False if Signature is invalid.</returns>
    public static bool IsFileSignatureValid(XmlDocument licenseXml)
    {
		using (var rsa = RSA.Create())
		{
			rsa.ImportFromPem(Helper.SignaturePubKeyPem.ToCharArray());
            
			SignedXml signedXml = new SignedXml(licenseXml);
            XmlNodeList nodeList = licenseXml.GetElementsByTagName("Signature");

            signedXml.LoadXml((XmlElement)nodeList[0]);
            if (signedXml.CheckSignature(rsa))
            {
                return true;
            }
            else
            {
                throw new Exception("The signature of the license file is not valid.");
            }
            
        }
    }
}
