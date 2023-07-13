using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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

    // CHANGE these values according to your environment at: https://my.slascone.com/info
    
    //Use this to connect to the Argus Demo 
    public const string ApiBaseUrl = "https://api.slascone.com";

    //Use this instead to connect to the SaaS environment
    //public const string ApiBaseUrl = "https://api365.slascone.com";
    public const string ProvisioningKey = "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D7ashHxFtZOR142LYgKWdNocibDgN75/P58YNvUZafLdaie7eGwI/2gX/XuDPtqDW";
    public const string AdminKey = "";
    public const string Bearer = "";

    public static Guid IsvId = Guid.Parse("2af5fe02-6207-4214-946e-b00ac5309f53");

    #endregion

    #region Encryption and Digital Signing
    //https://support.slascone.com/hc/en-us/articles/360016063637-DIGITAL-SIGNATURE-AND-DATA-INTEGRITY
    //0 = none, 1 = symmetric, 2 = assymetric
    //use 0 for initial prototyping, 2 for production
    public const int SignatureValidationMode = 2;

    // CHANGE these values according to your environment at: https://my.slascone.com/administration/signature
    // You can work either with pem OR with xml
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

    public const string SignaturePublicKeyXml =
@"<RSAKeyValue>
  <Modulus>wpigzm+cZIyw6x253YRDmroGQyo0rO9qpOdbNAkE/FMSX+At5CQT/Cyr0eZTo2h+MO5gn5a6dwg2SYB/K1YtyuiKqnaEUfoPnG51KLrj8hi9LoZyIenfsQnxPz+r8XGCUPeS9MhBEVvT4ba0x9EwR+krU87VqfI3KNpFQVdLPaZxN4STTEZaet7nReeNtnnZFYaUt5XeNPB0b0rGfrpsy7drmZz81dlWoRcLrBRpkf6XrOTX4yFxe/3HJ8mpukuvdweUBFoQ0xOHmG9pNQ31AHGtgLYGjbKcW4xYmpDGl0txfcipAr1zMj7X3oCO9lHcFRnXdzx+TTeJYxQX2XVbhQ==</Modulus>
  <Exponent>AQAB</Exponent>
</RSAKeyValue>";

    #endregion

	private static string UniqueDeviceId;

    /// <summary>
    /// Get a unique device id based on the system
    /// </summary>
    /// <returns>UUID via string</returns>
    public static string GetUniqueDeviceId(bool detectCloudAndVirtualization = false)
    {
        if (!string.IsNullOrEmpty(UniqueDeviceId))
            return UniqueDeviceId;

		if (detectCloudAndVirtualization)
		{
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
			if (azureDetected)
			{
				return UniqueDeviceId = azureVmInfos.VmId;
			}
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			try
			{
				UniqueDeviceId = WindowsDeviceInfos.ComputerSystemProductId;
			}
			catch (ManagementException managementException)
			{
				// WindowsDeviceInfos.ComputerSystemProductId uses a WMI query to get the machine ID
				// If a problem occurs executing the WMI query a device id has to be created in an alternative way
				UniqueDeviceId = $"{Guid.NewGuid()}-fallback";
			}

			return UniqueDeviceId;
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			var deviceId = LinuxDeviceInfos.DockerEnvExists
							   ? LinuxDeviceInfos.Hostname
							   : string.Concat(LinuxDeviceInfos.MachineId, LinuxDeviceInfos.RootDeviceSerial);

			return UniqueDeviceId = BitConverter.ToString(MD5.HashData(UTF8Encoding.UTF8.GetBytes(deviceId)));
		}

		throw new NotSupportedException("GetUniqueDeviceId() is supported only on Windows and Linux");
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
