using Slascone.Client;
using System.Xml;

namespace Slascone.Provisioning.Sample.NuGet;

 class Program
{
    public  ISlasconeClientV2 _slasconeClientV2;
    public Program() {

        _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
                Helper.IsvId,
                Helper.ProvisioningKey,
                Helper.SignatureValidationMode,
                Helper.SymmetricEncryptionKey,
                Helper.Certificate);
    }
    static async Task Main(string[] args)
    {
        var pr = new Program();

        // ToDo: Uncomment the specific scenario you want to test
       
        await ActivationSample(pr._slasconeClientV2);
        await HeartbeatSample(pr._slasconeClientV2);
        //await AnalyticalHeartbeatSample(pr._slasconeClientV2);
        //await UsageHeartbeatSample(pr._slasconeClientV2);
        //await ConsumptionHeartbeatSample(pr._slasconeClientV2);
        await OpenSession(pr._slasconeClientV2);
        //await CloseSession(pr._slasconeClientV2);
        //LicenseFileSample("XX/LicenseSample.xml");
    }

    private static async Task ActivationSample(ISlasconeClientV2 _slasconeClientV2)
    {
        var activateClientDto = new ActivateClientDto
        {
            Product_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3"),
            License_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0",
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            Client_description = "",
            Client_name = "",
            Software_version = "12.2.8"
        };

        try
        {
            var result = await _slasconeClientV2.ActivateLicenseAsync(activateClientDto);
            /*   If the activation failed, the api server responses with a specific error message which describes 
                 the problem. Therefore the LicenseInfo object is declared with null. */

            if (result.StatusCode == 200)
            {
                Console.WriteLine("Successfull activation.");
            }
            else if (result.StatusCode == 409)
            {
                Console.WriteLine(result.Error.Message);
                /*Example for 
                if (result.Error.Id == 2006)
                { 
                }
                */
            }
            else
            {
                Console.WriteLine(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private static async Task HeartbeatSample(ISlasconeClientV2 _slasconeClientV2)
    {
 
        // ToDo: Fill the variables
        var heartbeatDto = new AddHeartbeatDto
        {
            //Token_key = Guid.Parse(""),
            Product_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3"),
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            Software_version = "22.1",
            //GroupId = "",
            //HeartbeatTypeId = Guid.Parse(""),
            Operating_system = Helper.GetOperatingSystem()
        };

        try
        {
            var result = await _slasconeClientV2.AddHeartbeatAsync(heartbeatDto);  
            if (result.StatusCode == 200)
            {
                Console.WriteLine("Successfully created heartbeat.");
            }
            else if (result.StatusCode == 409)
            {
                Console.WriteLine(result.Error.Message);
                /*Example for 
                if (result.Error.Id == 2006)
                { 
                }
                */
            }
            else
            {
                Console.WriteLine(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    private async Task AnalyticalHeartbeatSample(ISlasconeClientV2 _slasconeClientV2)
    {

        // ToDo: Fill the variables
        var analyticalHb = new AnalyticalHeartbeatDto();
        analyticalHb.Analytical_heartbeat = new List<AnalyticalFieldValueDto>();
        analyticalHb.Client_id = Helper.GetWindowsUniqueDeviceId();

        try
        {
            var result = await _slasconeClientV2.AddAnalyticalHeartbeatAsync(analyticalHb);
            if (result.StatusCode == 200)
            {
                Console.WriteLine("Successfully created analytical heartbeat.");
            }
            else if (result.StatusCode == 409)
            {
                Console.WriteLine(result.Error.Message);
                /*Example for 
                if (result.Error.Id == 2006)
                { 
                }
                */
            }
            else
            {
                Console.WriteLine(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task UsageHeartbeatSample(ISlasconeClientV2 _slasconeClientV2)
    {


        // ToDo: Fill the variables
        var usageHeartbeat = new FullUsageHeartbeatDto();
        usageHeartbeat.Usage_heartbeat = new List<UsageHeartbeatValueDto>();
        usageHeartbeat.Client_id = Helper.GetWindowsUniqueDeviceId();

        var usageFeatureValue1 = new UsageHeartbeatValueDto();
        usageFeatureValue1.Usage_feature_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");
        usageFeatureValue1.Value = 0;

        var usageFeatureValue2 = new UsageHeartbeatValueDto();
        usageFeatureValue2.Usage_feature_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");
        usageFeatureValue2.Value = 0;
        usageHeartbeat.Usage_heartbeat.Add(usageFeatureValue1);
        usageHeartbeat.Usage_heartbeat.Add(usageFeatureValue2);

        try
        {
            var result = await _slasconeClientV2.AddUsageHeartbeatAsync(usageHeartbeat);
            if (result.StatusCode == 200)
            {
                Console.WriteLine("Successfully created usage heartbeat.");
            }
            else if (result.StatusCode == 409)
            {
                Console.WriteLine(result.Error.Message);
                /*Example for 
                if (result.Error.Id == 2006)
                { 
                }
                */
            }
            else
            {
                Console.WriteLine(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task ConsumptionHeartbeatSample(ISlasconeClientV2 _slasconeClientV2)
    {

        // ToDo: Fill the variables
        var consumptionHeartbeat = new FullConsumptionHeartbeatDto();
        consumptionHeartbeat.Client_id = Helper.GetWindowsUniqueDeviceId();
        consumptionHeartbeat.Consumption_heartbeat = new List<ConsumptionHeartbeatValueDto>();
        //consumptionHeartbeat.Token_key = Guid.Parse("");

        var consumptionHeartbeatValue1 = new ConsumptionHeartbeatValueDto();
        consumptionHeartbeatValue1.Limitation_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");
        consumptionHeartbeatValue1.Value = 1;
        consumptionHeartbeat.Consumption_heartbeat.Add(consumptionHeartbeatValue1);

        try
        {
            var result = await _slasconeClientV2.AddConsumptionHeartbeatAsync(consumptionHeartbeat);

            if (result.StatusCode == 200)
            {
                Console.WriteLine("Successfully created consumption heartbeat.");
            }
            else if (result.StatusCode == 409)
            {
                Console.WriteLine(result.Error.Message);
                /*Example for 
                if (result.Error.Id == 2006)
                { 
                }
                */
            }
            else
            {
                Console.WriteLine(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    private static async Task OpenSession(ISlasconeClientV2 _slasconeClientV2)
    {
        var sessionDto = new SessionRequestDto
        {
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            License_id = Guid.Parse("27180460-29df-4a5a-a0a1-78c85ab6cee0")
        };

        try
        {
            var result = await _slasconeClientV2.OpenSessionAsync(sessionDto);
            if (result.StatusCode == 200)
            {
                Console.WriteLine("Successfully opened session.");
            }
            else if (result.StatusCode == 409)
            {
                Console.WriteLine(result.Error.Message);
                /*Example for 
                if (result.Error.Id == 2006)
                { 
                }
                */
            }
            else
            {
                Console.WriteLine(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }


    private static async Task CloseSession(ISlasconeClientV2 _slasconeClientV2)
    {
        var sessionDto = new SessionRequestDto
        {
            Client_id = Helper.GetWindowsUniqueDeviceId(),
            License_id = Guid.Parse("27180460-29df-4a5a-a0a1-78c85ab6cee0")
        };

        try
        {
            var result = await _slasconeClientV2.CloseSessionAsync(sessionDto);
            if (result.StatusCode == 200)
            {
                Console.WriteLine("Successfully closed session.");
            }
            else if (result.StatusCode == 409)
            {
                Console.WriteLine(result.Error.Message);
                /*Example for 
                if (result.Error.Id == 2006)
                { 
                }
                */
            }
            else
            {
                Console.WriteLine(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

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

