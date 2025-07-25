namespace Slascone.Provisioning.Sample.NuGet;

public class Settings
{
    #region Main values - Fill according to your environment

    // CHANGE these values according to your environment at: https://my.slascone.com/info
    
    //Use this to connect to the Argus Demo 
    public const string ApiBaseUrl = "https://api.slascone.com";

    // Use this instead to connect to the SaaS environment
    // public const string ApiBaseUrl = "https://api365.slascone.com";

    public const string ProvisioningKey = "NfEpJ2DFfgczdYqOjvmlgP2O/4VlqmRHXNE9xDXbqZcOwXTbH3TFeBAKKbEzga7D7ashHxFtZOR142LYgKWdNocibDgN75/P58YNvUZafLdaie7eGwI/2gX/XuDPtqDW";
    public const string AdminKey = "";
    public const string Bearer = "";

    public static Guid IsvId = Guid.Parse("2af5fe02-6207-4214-946e-b00ac5309f53");
    public static Guid ProductId = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");  // Find your own product id key at : https://my.slascone.com/products

    #endregion

    #region Encryption and Digital Signing

    // https://support.slascone.com/hc/en-us/articles/360016063637-DIGITAL-SIGNATURE-AND-DATA-INTEGRITY
    // 0 = none, 1 = symmetric, 2 = asymmetric
    // use 0 for initial prototyping, 2 for production
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
}
