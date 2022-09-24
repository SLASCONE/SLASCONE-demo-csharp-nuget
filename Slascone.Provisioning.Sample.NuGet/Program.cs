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
    
        await ActivationExample(pr._slasconeClientV2);
        await HeartbeatExample(pr._slasconeClientV2);
        await AnalyticalHeartbeatExample(pr._slasconeClientV2);
        await UsageHeartbeatExample(pr._slasconeClientV2);
        await ConsumptionHeartbeatExample(pr._slasconeClientV2);
        await OpenSessionExample(pr._slasconeClientV2);
        await CloseSessionExample(pr._slasconeClientV2);
        IsLicenseFileSignatureValid( @"../../../Assets/OfflineLicenseFile.xml");
    }

    private static async Task ActivationExample(ISlasconeClientV2 _slasconeClientV2)
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
    private static async Task HeartbeatExample(ISlasconeClientV2 _slasconeClientV2)
    {
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

    private static async Task AnalyticalHeartbeatExample(ISlasconeClientV2 _slasconeClientV2)
    {
        var analyticalHeartbeatDto = new AnalyticalHeartbeatDto();
        analyticalHeartbeatDto.Analytical_heartbeat = new List<AnalyticalFieldValueDto>();
        analyticalHeartbeatDto.Client_id = Helper.GetWindowsUniqueDeviceId();
        var analyticalField = new AnalyticalFieldValueDto 
        { 
            Analytical_field_id = Guid.Parse("2754aca1-4d1a-4af3-9387-08da9ac54c6d"),
            Value = "SQL Server 2019"
        };        
        analyticalHeartbeatDto.Analytical_heartbeat.Add(analyticalField);

        try
        {
            var result = await _slasconeClientV2.AddAnalyticalHeartbeatAsync(analyticalHeartbeatDto);
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

    private static async Task UsageHeartbeatExample(ISlasconeClientV2 _slasconeClientV2)
    {
        var usageHeartbeat = new FullUsageHeartbeatDto();
        usageHeartbeat.Usage_heartbeat = new List<UsageHeartbeatValueDto>();
        usageHeartbeat.Client_id = Helper.GetWindowsUniqueDeviceId();

        var usageFeatureValue1 = new UsageHeartbeatValueDto();
        usageFeatureValue1.Usage_feature_id = Guid.Parse("66099049-0472-467c-6ea6-08da9ac57d7c");
        usageFeatureValue1.Value = 2;

        var usageFeatureValue2 = new UsageHeartbeatValueDto();
        usageFeatureValue2.Usage_feature_id = Guid.Parse("e82619b1-f403-4e0d-5389-08da9e17dd73");
        usageFeatureValue2.Value = 5;
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

    private static async Task ConsumptionHeartbeatExample(ISlasconeClientV2 _slasconeClientV2)
    {
        var consumptionHeartbeat = new FullConsumptionHeartbeatDto();
        consumptionHeartbeat.Client_id = Helper.GetWindowsUniqueDeviceId();
        consumptionHeartbeat.Consumption_heartbeat = new List<ConsumptionHeartbeatValueDto>();
        //consumptionHeartbeat.Token_key = Guid.Parse("");

        var consumptionHeartbeatValue1 = new ConsumptionHeartbeatValueDto();
        consumptionHeartbeatValue1.Limitation_id = Guid.Parse("00cf2984-d71a-4c66-9f49-08da833189e3");
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


    private static async Task OpenSessionExample(ISlasconeClientV2 _slasconeClientV2)
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


    private static async Task CloseSessionExample(ISlasconeClientV2 _slasconeClientV2)
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

    private static bool IsLicenseFileSignatureValid(string licenseFile)
    {
        //Signature Validation of Offline License XML
        XmlDocument xmlDoc = new XmlDocument();
        //Load an XML file into the XmlDocument object.
        xmlDoc.PreserveWhitespace = true;
        xmlDoc.Load(licenseFile);
        bool isValid = false;
        try
        {
             isValid = Helper.IsFileSignatureValid(xmlDoc);
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.ToString());
        }
        Console.WriteLine("Successfully validated the file's signature.");
        return isValid;

    }
}

