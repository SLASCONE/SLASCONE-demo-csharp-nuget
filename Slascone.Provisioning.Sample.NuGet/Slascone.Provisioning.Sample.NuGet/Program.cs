namespace Slascone.Provisioning.Sample.NuGet;

internal class Program
{
    static async Task Main(string[] args)
    {
        IUseCaseActivateLicense useCaseActivateLicense = new UseCaseActivateLicense();

        var activateClientDto = new ActivateClientDto
        {
            Product_id = Guid.Parse("47df1df5-bbc8-4b1b-a185-58ddfb1d3271"),
            License_key = "0efce21f-c6bb-4a68-b65e-c3cdfdded42c",
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            Client_description = "",
            Client_name = "",
            Software_version = "12.2.8"
        };

        try
        {
            var activatedLicense = await useCaseActivateLicense.ActivateLicenseAsync(activateClientDto);

            if (activatedLicense != null)
            {
                Console.WriteLine("Successfully activated license.");
            }

            // ToDo: Uncomment specific scenario
            //await FloatingLicensingSample(activatedLicense);
            //await HeartbeatSample(activatedLicense);
            //LicenseFileSample("XX/LicenseSample.xml");
        }
        catch (Exception)
        {
            Console.ReadLine();
        }
    }

    private async Task FloatingLicensingSample(LicenseInfoDto activatedLicense)
    {
        IUseCaseHeartbeat useCaseHeartbeat = new UseCaseHeartbeat();
        IUseCaseFloatingLicense useCaseFloatingLicense = new UseCaseFloatingLicense();

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

        var heartbeatResult = await useCaseHeartbeat.AddHeartbeatAsync(heartbeatDto);

        /*If the heartbeat failed, the api server responses with a specific error message which describes the problem.
          You can verify the error messages in Use Cases that thros specific exception.*/

        // After successfully generating a heartbeat the client have to check provisioning mode of the license. Is it floating a session has to be opened.
        if (heartbeatResult != null && heartbeatResult.Provisioning_mode == ProvisioningMode.Floating)
        {
            // ToDo: Fill the variables
            var sessionDto = new SessionRequestDto
            {
                Client_id = Helper.GetWindowsUniqueDeviceId(),
                License_id = Guid.Parse("")
            };

            var openSessionResult = await useCaseFloatingLicense.OpenSessionAsync(sessionDto);

            /*If the heartbeat failed, the api server responses with a specific error message which describes the problem.
             You can verify the error messages in Use Cases that thros specific exception.*/

            if (openSessionResult != null)
            {
                Console.WriteLine("Session active until: " + openSessionResult.Session_valid_until);
            }

            // If the client have finished his work, he has to close the session. Therefore other clients are not blocked anymore and have not to wait until another Client expired. 
            var closeSessionResult = await useCaseFloatingLicense.CloseSessionAsync(sessionDto);

            Console.WriteLine(closeSessionResult);
        }
    }

    private async Task HeartbeatSample(LicenseInfoDto activatedLicense)
    {
        IUseCaseActivateLicense useCaseActivateLicense = new UseCaseActivateLicense();
        IUseCaseHeartbeat useCaseHeartbeat = new UseCaseHeartbeat();

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

        var heartbeatResult = await useCaseHeartbeat.AddHeartbeatAsync(heartbeatDto);

        if (heartbeatResult != null)
        {
            Console.WriteLine("Successfully created heartbeat.");
        }

        /*If the heartbeat failed, the api server responses with a specific error message which describes the problem.
          You can verify the error messages in Use Cases.*/

        // ToDo: Fill the variables
        var analyticalHb = new AnalyticalHeartbeatDto();
        analyticalHb.Analytical_heartbeat = new List<AnalyticalFieldValueDto>();
        analyticalHb.Client_id = Helper.GetWindowsUniqueDeviceId();

        var analyticalHeartbeatResult = await useCaseHeartbeat.AddAnalyticalHeartbeatAsync(analyticalHb);

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

        var usageHeartbeatResult = await useCaseHeartbeat.AddUsageHeartbeatAsync(usageHeartbeat);

        Console.WriteLine(usageHeartbeatResult);

        if (activatedLicense != null)
        {
            // ToDo: Fill the variables
            var unassignDto = new UnassignDto
            {
                Token_key = Guid.Parse("")
            };

            var unassignResult = await useCaseActivateLicense.UnassignAsync(unassignDto);

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

        var consumptionHeartbeatResult = await useCaseHeartbeat.AddConsumptionHeartbeatAsync(consumptionHeartbeat);

        Console.WriteLine(consumptionHeartbeatResult);

        var remainingConsumptions = await useCaseHeartbeat.GetConsumptionStatusAsync(new ValidateConsumptionStatusDto
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

