using Slascone.Client;
using System.Xml;

namespace Slascone.Provisioning.Sample.NuGet;

internal class Program
{
    static async Task Main(string[] args)
    {
        ISlasconeClientV2 _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
            Helper.IsvId,
            Helper.ProvisioningKey,
            Helper.SignatureValidationMode,
            Helper.SymmetricEncryptionKey,
            Helper.Certificate);

        var activateClientDto = new ActivateClientDto
        {
            Product_id = Guid.Parse("47df1df5-bbc8-4b1b-a185-58ddfb1d3271"),
            License_key = "0efce21f-c6bb-4a68-b65e-c3cdfdded42c",
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            Client_description = "",
            Client_name = "",
            Software_version = "12.2.8"
        };

        var activatedLicense = await _slasconeClientV2.ActivateLicenseAsync(activateClientDto);

        /*   If the activation failed, the api server responses with a specific error message which describes 
             the problem. Therefore the LicenseInfo object is declared with null. */

        if (activatedLicense.StatusCode == "409")
        {
            Console.WriteLine(activatedLicense.Error.Message);
            /*Example for 
            if (activatedLicense.Error.Id == 2006)
            { 
            }
            */
        }
        else
        {
            Console.WriteLine("Successfully activated license.");
        }

        Console.ReadLine();

        // ToDo: Uncomment specific scenario
        //await FloatingLicensingSample(activatedLicense);
        //await HeartbeatSample(activatedLicense);
        //LicenseFileSample("XX/LicenseSample.xml");
    }

    private async Task FloatingLicensingSample(LicenseInfoDto activatedLicense)
    {
        ISlasconeClientV2 _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
            Helper.IsvId,
            Helper.ProvisioningKey,
            Helper.SignatureValidationMode,
            Helper.SymmetricEncryptionKey,
            Helper.Certificate);

        // ToDo: Fill the variables
        var heartbeatDto = new AddHeartbeatDto
        {

            Product_id = Guid.Parse(""),
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            Software_version = "",
            //GroupId = "",
            //HeartbeatTypeId = Guid.Parse(""),
            //TokenKey = "",
            Operating_system = Helper.GetOperatingSystem()
        };

        var heartbeatResult = await _slasconeClientV2.AddHeartbeatAsync(heartbeatDto);

        /* After successfully generating a heartbeat the client have to check provisioning mode of the license.
           Is it floating a session has to be opened. */
        if (heartbeatResult != null && heartbeatResult.Result?.Provisioning_mode == ProvisioningMode.Floating)
        {
            // ToDo: Fill the variables
            var sessionDto = new SessionRequestDto
            {
                Client_id = Helper.GetWindowsUniqueDeviceId(),
                License_id = Guid.Parse("")
            };

            var openSessionResult = await _slasconeClientV2.OpenSessionAsync(sessionDto);

            //If the floating limit is reached the api server responses with an Error.
            if (openSessionResult.StatusCode == "409")
            {
                Console.WriteLine(openSessionResult.Error?.Message);
            }
            else
            {
                Console.WriteLine("Session active until: " + openSessionResult.Result?.Session_valid_until);
            }

            /* If the client have finished his work, he has to close the session.
               Therefore other clients are not blocked anymore and have not to wait until another Client expired. */
            var closeSessionResult = await _slasconeClientV2.CloseSessionAsync(sessionDto);

            Console.WriteLine(closeSessionResult);
        }
    }

    private async Task HeartbeatSample(LicenseInfoDto activatedLicense)
    {
        ISlasconeClientV2 _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
            Helper.IsvId,
            Helper.ProvisioningKey,
            Helper.SignatureValidationMode,
            Helper.SymmetricEncryptionKey,
            Helper.Certificate);

        // ToDo: Fill the variables
        var heartbeatDto = new AddHeartbeatDto
        {
            Token_key = Guid.Parse(""),
            Product_id = Guid.Parse(""),
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            Software_version = "",
            //GroupId = "",
            //HeartbeatTypeId = Guid.Parse(""),
            Operating_system = Helper.GetOperatingSystem()
        };

        var heartbeatResult = await _slasconeClientV2.AddHeartbeatAsync(heartbeatDto);

        /* If the heartbeat failed, the api server responses with a specific error message which describes the problem.
           Therefore the LicenseInfo object is declared with null. */
        if (heartbeatResult.StatusCode == "409")
        {
            Console.WriteLine(heartbeatResult.Error?.Message);
        }
        else
        {
            Console.WriteLine("Successfully created heartbeat.");
        }

        // ToDo: Fill the variables
        var analyticalHb = new AnalyticalHeartbeatDto();
        analyticalHb.Analytical_heartbeat = new List<AnalyticalFieldValueDto>();
        analyticalHb.Client_id = Helper.GetWindowsUniqueDeviceId();

        var analyticalHeartbeatResult = await _slasconeClientV2.AddAnalyticalHeartbeatAsync(analyticalHb);

        Console.WriteLine(analyticalHeartbeatResult);

        // ToDo: Fill the variables
        var usageHeartbeat = new FullUsageHeartbeatDto();
        usageHeartbeat.Usage_heartbeat = new List<UsageHeartbeatValueDto>();
        usageHeartbeat.Client_id = Helper.GetWindowsUniqueDeviceId();

        var usageFeatureValue1 = new UsageHeartbeatValueDto();
        usageFeatureValue1.Usage_feature_id = Guid.Parse("");
        usageFeatureValue1.Value = 0;

        var usageFeatureValue2 = new UsageHeartbeatValueDto();
        usageFeatureValue2.Usage_feature_id = Guid.Parse("");
        usageFeatureValue2.Value = 0;
        usageHeartbeat.Usage_heartbeat.Add(usageFeatureValue1);
        usageHeartbeat.Usage_heartbeat.Add(usageFeatureValue2);

        var usageHeartbeatResult = await _slasconeClientV2.AddUsageHeartbeatAsync(usageHeartbeat);

        Console.WriteLine(usageHeartbeatResult);

        if (activatedLicense != null)
        {
            // ToDo: Fill the variables
            var unassignDto = new UnassignDto
            {
                Token_key = Guid.Parse("")
            };

            var unassignResult = await _slasconeClientV2.UnassignLicenseAsync(unassignDto);

            Console.WriteLine(unassignResult);
        }

        // ToDo: Fill the variables
        var consumptionHeartbeat = new FullConsumptionHeartbeatDto();
        consumptionHeartbeat.Client_id = Helper.GetWindowsUniqueDeviceId();
        consumptionHeartbeat.Consumption_heartbeat = new List<ConsumptionHeartbeatValueDto>();
        consumptionHeartbeat.Token_key = Guid.Parse("");

        var consumptionHeartbeatValue1 = new ConsumptionHeartbeatValueDto();
        consumptionHeartbeatValue1.Limitation_id = Guid.Parse("");
        consumptionHeartbeatValue1.Value = 1;
        consumptionHeartbeatValue1.Limitation_id = Guid.Parse("");
        consumptionHeartbeat.Consumption_heartbeat.Add(consumptionHeartbeatValue1);

        var consumptionHeartbeatResult = await _slasconeClientV2.AddConsumptionHeartbeatAsync(consumptionHeartbeat);

        Console.WriteLine(consumptionHeartbeatResult);

        var remainingConsumptions = await _slasconeClientV2.GetConsumptionStatusAsync(new ValidateConsumptionStatusDto
        { Limitation_id = Guid.Parse(""), Client_id = Helper.GetWindowsUniqueDeviceId() });
    }

    private static bool LicenseFileSample(string licenseFile)
    {
        //Signature Validation of Offline License XML
        XmlDocument xmlDoc = new XmlDocument();
        //Load an XML file into the XmlDocument object.
        xmlDoc.PreserveWhitespace = true;
        xmlDoc.Load(licenseFile);
        return Helper.IsFileSignatureValid(xmlDoc);
    }
}

