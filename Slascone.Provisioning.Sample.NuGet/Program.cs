using System.Buffers.Text;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using Slascone.Client;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Xml;
using Microsoft.Identity.Client;

namespace Slascone.Provisioning.Sample.NuGet;

class Program
{
    private readonly ISlasconeClientV2 _slasconeClientV2;

    // ID for product "BI Server"
    private readonly Guid _product_id_bi_server = Guid.Parse("47df1df5-bbc8-4b1b-a185-58ddfb1d3271");
    
    // ID for product "CAD"
	private readonly Guid _product_id_cad = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");

    public Program()
    {
        _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
                Helper.IsvId,
                Helper.ProvisioningKey,
                Helper.SignatureValidationMode,
                Helper.SymmetricEncryptionKey,
                Helper.Certificate);

		string encryptedProvKey64;
        
        using (var aes = Aes.Create())
        {
	        aes.Key = Helper.Key;
	        aes.IV = Helper.IV;

	        var provKeyBytes = Encoding.Unicode.GetBytes(Helper.ProvisioningKey);

			var encryptor = aes.CreateEncryptor();
			var en = Convert.ToBase64String(encryptor.TransformFinalBlock(provKeyBytes, 0, provKeyBytes.Length));

			encryptedProvKey64 = Convert.ToBase64String(aes.EncryptCbc(provKeyBytes, aes.IV));
        }

        using (var aes = Aes.Create())
        {
	        aes.Key = Helper.Key;
	        aes.IV = Helper.IV;

	        var decryptedProvKey =
		        Encoding.Unicode.GetString(aes.DecryptCbc(Convert.FromBase64String(Assets.apikey.pk), aes.IV));

	        var areEqual = decryptedProvKey == Helper.ProvisioningKey;
        }
    }

	static async Task Main(string[] args)
    {
	    var pr = new Program();

	    Console.WriteLine("Slascone client app example");
        Console.WriteLine("===========================");
        Console.WriteLine();
        Console.WriteLine($"Unique Client-Id for this device: {Helper.GetUniqueDeviceId()}");
        Console.WriteLine($"Operating system: {Helper.GetOperatingSystem()}");

        //PublicClientApplicationBuilder
        //IPublicClientApplication app = new IPublicClientApplication();
        
        string input;
        do
        {
	        Console.WriteLine();
	        Console.WriteLine("1: Activate license (can be done only once per device)");
	        Console.WriteLine("2: Add license heart beat");
	        Console.WriteLine("3: Add analytical heart beat");
	        Console.WriteLine("4: Add usage heart beat");
	        Console.WriteLine("5: Lookup licenses");
	        Console.WriteLine("6: Get all licenses of a customer");
	        Console.WriteLine("7: Print device infos");
	        Console.WriteLine("x: Exit demo app");

            Console.Write("> ");
	        input = Console.ReadLine();

	        switch (input)
	        {
		        case "1":
			        await pr.ActivationExample();
			        break;

		        case "2":
			        await pr.HeartbeatExample();
			        break;

		        case "3":
			        await pr.AnalyticalHeartbeatExample();
			        break;

		        case "4":
			        await pr.UsageHeartbeatExample();
			        break;

                case "5":
	                await pr.LookupLicensesExample();
	                break;

                case "6":
	                await pr.LicensesOfCustomerExample();
	                break;

                case "7":
	                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						Console.Write(Slascone.Client.DeviceInfos.WindowsDeviceInfos.LogDeviceInfos());
	                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		                Console.Write(Slascone.Client.DeviceInfos.LinuxDeviceInfos.LogDeviceInfos());
	                break;
			}
		} while (!"x".Equals(input, StringComparison.InvariantCultureIgnoreCase));

        //await pr.ActivationExample();
        //await pr.HeartbeatExample();
        //await pr.AnalyticalHeartbeatExample();
        //await pr.UsageHeartbeatExample();
        //await pr.ConsumptionHeartbeatExample();
        //await pr.LookupLicenseExample();
        //await pr.OpenSessionExample();
        //await pr.CloseSessionExample();

        //IsLicenseFileSignatureValid(@"../../../Assets/OfflineLicenseFile.xml");
    }

	private async Task ActivationExample()
    {
        var activateClientDto = new ActivateClientDto
        {
            Product_id = _product_id_cad,
            License_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0",
            Client_id = Helper.GetUniqueDeviceId(),
            //Client_id = Guid.NewGuid().ToString(),
            Client_description = "",
            Client_name = "From GitHub Nuget Sample",
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
	            ReportError("ActivateLicense", result.Error);
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

    private async Task HeartbeatExample()
    {
        var heartbeatDto = new AddHeartbeatDto
        {
            //Token_key = Guid.Parse(""),
            Product_id = _product_id_cad,
            Client_id = Helper.GetUniqueDeviceId(),
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
                ReportError("AddHeartbeat", result.Error);
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

    private async Task AnalyticalHeartbeatExample()
    {
        var analyticalHeartbeatDto = new AnalyticalHeartbeatDto
        {
	        Analytical_heartbeat = new List<AnalyticalFieldValueDto>(),
	        Client_id = Helper.GetUniqueDeviceId()
        };

        Console.Write("Value for analytical field: ");
        var value = Console.ReadLine();

		var analyticalField = new AnalyticalFieldValueDto
        {
            Analytical_field_id = Guid.Parse("2754aca1-4d1a-4af3-9387-08da9ac54c6d"),
            Value = value
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
                ReportError("AddAnalyticalHeartbeat", result.Error);
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

    private async Task UsageHeartbeatExample()
    {
        var usageHeartbeat = new FullUsageHeartbeatDto
        {
	        Usage_heartbeat = new List<UsageHeartbeatValueDto>(),
	        Client_id = Helper.GetUniqueDeviceId()
        };

        Console.Write("Value for usage feature 1: ");
        var input = Console.ReadLine();
        double.TryParse(input, out var value);
        
		var usageFeatureValue1 = new UsageHeartbeatValueDto
        {
	        Usage_feature_id = Guid.Parse("66099049-0472-467c-6ea6-08da9ac57d7c"),
	        Value = value
        };
        usageHeartbeat.Usage_heartbeat.Add(usageFeatureValue1);

        Console.Write("Value for usage feature 2: ");
        input = Console.ReadLine();
        double.TryParse(input, out value);

		var usageFeatureValue2 = new UsageHeartbeatValueDto
        {
	        Usage_feature_id = Guid.Parse("e82619b1-f403-4e0d-5389-08da9e17dd73"),
	        Value = value
        };
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
                ReportError("AddUsageHeartbeat", result.Error);
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

    private async Task ConsumptionHeartbeatExample()
    {
        var consumptionHeartbeat = new FullConsumptionHeartbeatDto();
        consumptionHeartbeat.Client_id = Helper.GetUniqueDeviceId();
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

    private async Task LookupLicensesExample()
    {
		var lookup = new LookupDto
	    {
		    Product_id = _product_id_cad,
		    Customer_number = "87012",
			Client_id = Helper.GetUniqueDeviceId()
	    };

	    var result = await _slasconeClientV2.LookupLicensesAsync(lookup);

	    var getLicenses = new GetLicensesByLicenseKeyDto
	    {
		    Product_id = _product_id_cad,
		    License_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0"
	    };
	    var getlicensesresult = await _slasconeClientV2.GetLicensesByLicenseKeyAsync(getLicenses);

	    var licenses = getlicensesresult?.Result;
    }

    private async Task LicensesOfCustomerExample()
    {
	    var lookupDto = new LookupDto()
	    {
		    Product_id = _product_id_cad,
		    //Client_id = Helper.GetUniqueDeviceId(),
		    Client_id = Guid.NewGuid().ToString(),
		    Customer_number = "87012"
	    };

        var result = await _slasconeClientV2.LookupLicensesAsync(lookupDto);
    }

    private async Task OpenSessionExample()
    {
        var sessionDto = new SessionRequestDto
        {
            Client_id = Helper.GetUniqueDeviceId(),
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

    private async Task CloseSessionExample()
    {
        var sessionDto = new SessionRequestDto
        {
            Client_id = Helper.GetUniqueDeviceId(),
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

    private static void ReportError(string action, ErrorResultObjects error)
    {
	    Console.WriteLine($"{action} received an error: {error.Message} (Id: {error.Id})");
    }
}

