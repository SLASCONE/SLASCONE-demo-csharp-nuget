using System.Runtime.InteropServices;
using Slascone.Client;
using System.Xml;
using Slascone.Client.DeviceInfos;
using System.Reflection;
using System.Text;

namespace Slascone.Provisioning.Sample.NuGet;

class Program
{
	private readonly ISlasconeClientV2 _slasconeClientV2;

	// ID for license
	private readonly string _license_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0";

	// ID for product "BI Server"
	private readonly Guid _product_id_bi_server = Guid.Parse("47df1df5-bbc8-4b1b-a185-58ddfb1d3271");

	// ID for product "CAD"
	private readonly Guid _product_id_cad = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");

	private Guid? _token_id = Guid.Empty;
	private Stack<Guid> _sessionIds = new Stack<Guid>();

	public Program()
	{
		_slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
				Helper.IsvId,
				Helper.ProvisioningKey,
				Helper.SignatureValidationMode,
				Helper.SymmetricEncryptionKey,
				Helper.Certificate);

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var appDataFolder =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Assembly.GetExecutingAssembly().GetName().Name);
			_slasconeClientV2.SetAppDataFolder(appDataFolder);
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			_slasconeClientV2.SetAppDataFolder(Environment.CurrentDirectory);
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
		if (2 == Helper.SignatureValidationMode)
			Console.WriteLine(Helper.LogCertificate());

		string input;
		do
		{
			Console.WriteLine();
			Console.WriteLine("1: Activate license (can be done only once per device)");
			Console.WriteLine("2: Add license heart beat");
			Console.WriteLine("3: Add analytical heart beat");
			Console.WriteLine("4: Add usage heart beat");
			Console.WriteLine("5: Add consumption heart beat");
			Console.WriteLine("6: Unassign license from device (has to be activated again then)");
			Console.WriteLine("7: Lookup licenses");
			Console.WriteLine("8: Open session");
			Console.WriteLine("9: Close session");
			Console.WriteLine("10: Read offline license info (only available after at least one license heart beat)");
			Console.WriteLine("11: Validate license file");
			Console.WriteLine("12: Print device infos");
			Console.WriteLine("13: Print virtualization/cloud environment infos");
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
					await pr.ConsumptionHeartbeatExample();
					break;

				case "6":
					await pr.UnassignExample();
					break;

				case "7":
					await pr.LookupLicensesExample();
					break;

				case "8":
					await pr.OpenSessionExample();
					break;

				case "9":
					await pr.CloseSessionExample();
					break;

				case "10":
					pr.OfflineLicenseInfoExample();
					break;

				case "11":
					IsLicenseFileSignatureValid(@"../../../Assets/OfflineLicenseFile.xml");
					break;

				case "12":
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						Console.Write(WindowsDeviceInfos.LogDeviceInfos());
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
						Console.Write(LinuxDeviceInfos.LogDeviceInfos());
					break;

				case "13":
					Console.Write(await pr.LogVirtualizationInfos());
					break;
			}
		} while (!"x".Equals(input, StringComparison.InvariantCultureIgnoreCase));

	}

	private async Task ActivationExample()
	{
		var activateClientDto = new ActivateClientDto
		{
			Product_id = _product_id_cad,
			License_key = _license_key,
			Client_id = Helper.GetUniqueDeviceId(),
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
				WriteLicenseInfo(result.Result);
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
			Product_id = _product_id_cad,
			Client_id = Helper.GetUniqueDeviceId(),
			Software_version = "22.1",
			Operating_system = Helper.GetOperatingSystem()
		};

		try
		{
			var result = await _slasconeClientV2.AddHeartbeatAsync(heartbeatDto);
			if (result.StatusCode == 200)
			{
				Console.WriteLine("Successfully created heartbeat.");
				WriteLicenseInfo(result.Result);
				_token_id = result.Result.Token_key;
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
				ReportError("ComsumptionHeaerbeat", result.Error);
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

	private async Task UnassignExample()
	{
		if (!_token_id.HasValue)
		{
			Console.WriteLine("You have to add a license heartbeat first to get a token for this operation.");
			return;
		}

		var unassignDto = new UnassignDto
		{
			Token_key = _token_id.Value
		};

		try
		{
			var result = await _slasconeClientV2.UnassignLicenseAsync(unassignDto);
			if (result.StatusCode == 200)
			{
				Console.WriteLine("Successfully unaasigned device from license.");
			}
			else if (result.StatusCode == 409)
			{
				ReportError("UnassignLicense", result.Error);
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
		var getLicenses = new GetLicensesByLicenseKeyDto
		{
			Product_id = _product_id_cad,
			License_key = _license_key
		};

		try
		{

			var result = await _slasconeClientV2.GetLicensesByLicenseKeyAsync(getLicenses);

			if (200 == result.StatusCode)
			{
				foreach (var license in result.Result)
				{
					WriteLicenseInfo(license);
				}
			}
			else if (409 == result.StatusCode)
			{
				ReportError("LookupLicenses", result.Error);
			}
			else
			{
				Console.WriteLine(result.Message);
			}
		}
		catch (Exception exception)
		{
			Console.WriteLine(exception.Message);
		}
	}

	private async Task OpenSessionExample()
	{
		var sessionId = Guid.NewGuid();

        var sessionDto = new SessionRequestDto
        {
			Client_id = Helper.GetUniqueDeviceId(),
			License_id = Guid.Parse(_license_key),
			Session_id = sessionId
        };

        try
        {
            var result = await _slasconeClientV2.OpenSessionAsync(sessionDto);
            if (result.StatusCode == 200)
            {
				_sessionIds.Push(sessionId);
                Console.WriteLine($"Successfully opened session {sessionId}.");
                Console.WriteLine($"Session valid until {result.Result.Session_valid_until}.");
            }
            else if (result.StatusCode == 409)
            {
	            ReportError("OpenSession", result.Error);
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
	    var sessionId = _sessionIds.Any()
		    ? _sessionIds.Pop()
		    : new Guid();

        var sessionDto = new SessionRequestDto
        {
            Client_id = Helper.GetUniqueDeviceId(),
            License_id = Guid.Parse(_license_key),
			Session_id = sessionId
        };

        try
        {
            var result = await _slasconeClientV2.CloseSessionAsync(sessionDto);
            if (result.StatusCode == 200)
            {
                Console.WriteLine($"Successfully closed session {sessionId}.");
            }
            else if (result.StatusCode == 409)
            {
	            ReportError("CloseSession", result.Error);
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

    private void OfflineLicenseInfoExample()
    {
	    var response = _slasconeClientV2.GetOfflineLicense();

	    if (response.StatusCode == 200)
	    {
		    var licenseInfo = response.Result;

		    WriteLicenseInfo(licenseInfo);

		    if (licenseInfo.Created_date_utc.HasValue)
		    {
			    // Check how old the stored license info is
			    var licenseInfoAge = (DateTime.Now - licenseInfo.Created_date_utc.Value).Days;
			    Console.WriteLine($"   Offline license info is {licenseInfoAge} days old.");

				if (0 < licenseInfoAge && licenseInfo.Freeride.HasValue)
			    {
				    Console.Write($"   Freeride period: {licenseInfo.Freeride.Value}; ");
				    if (licenseInfoAge <= licenseInfo.Freeride.Value)
					    Console.WriteLine("License is valid because the defined freeride period is adhered to.");
				    else
					    Console.WriteLine("License invalid due to freeride period exceeded.");
			    }
		    }
		}
		else if (response.StatusCode == 400)
	    {
		    Console.WriteLine(response.Message);
	    }
    }

	private void WriteLicenseInfo(LicenseDto license)
    {
	    Console.WriteLine("License infos:");
	    Console.WriteLine($"   Company name: {license.Customer.Company_name}");

	    // Handle license info
	    //  o Active and expired state (i.e. valid state)
	    //  o Active features and limitations
	    Console.WriteLine($"   License is {(license.Is_valid ? "valid" : "not valid")} (IsActive: {license.Is_active}; IsExpired: {license.Is_expired})");

	    if (license.Is_expired)
	    {
		    var expiration = (DateTime.Now - license.Expiration_date_utc.Value).Days;
		    Console.WriteLine($"   License is expired since {expiration} day(s).");
	    }

	    Console.WriteLine($"   Active features: {string.Join(", ", license.License_features.Where(f => f.Is_active).Select(f => f.Feature_name))}");
	    Console.WriteLine($"   Limitations: {string.Join(", ", license.License_limitations.Select(l => $"{l.Limitation_name} = {l.Limit}"))}");
    }

	private void WriteLicenseInfo(LicenseInfoDto licenseInfo)
    {
	    Console.WriteLine($"License infos (Retrieved {licenseInfo.Created_date_utc.Value}):");
	    Console.WriteLine($"   Company name: {licenseInfo.Customer.Company_name}");

	    // Handle license info
	    //  o Active and expired state (i.e. valid state)
	    //  o Active features and limitations
	    Console.WriteLine($"   License is {(licenseInfo.Is_license_valid ? "valid" : "not valid")} (IsActive: {licenseInfo.Is_license_active}; IsExpired: {licenseInfo.Is_license_expired})");

	    if (licenseInfo.Is_license_expired)
	    {
		    var expiration = (DateTime.Now - licenseInfo.Expiration_date_utc.Value).Days;
		    Console.WriteLine($"   License is expired since {expiration} day(s).");

		    // Check freeride
		    if (licenseInfo.Freeride.HasValue && expiration < licenseInfo.Freeride.Value)
		    {
			    Console.WriteLine($"   Freeride granted for {licenseInfo.Freeride.Value - expiration} day(s).");
		    }
	    }

	    Console.WriteLine($"   Active features: {string.Join(", ", licenseInfo.Features.Where(f => f.Is_active).Select(f => f.Name))}");
	    Console.WriteLine($"   Limitations: {string.Join(", ", licenseInfo.Limitations.Select(l => $"{l.Name} = {l.Value}"))}");
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
        if (isValid)
        {
            Console.WriteLine("Successfully validated the file's signature.");
        }
        else
        {
            Console.WriteLine("Invalid file signature.");
        }
        return isValid;
    }

    private async Task<string> LogVirtualizationInfos()
    {
	    var sb = new StringBuilder();

	    var awsEc2Infos = new AwsEc2Infos();

		var awsEc2Detected = await awsEc2Infos.DetectAwsEcs();

		if (awsEc2Detected)
		{
			sb.AppendLine("Running on an AWS EC2 instance:");
			sb.AppendLine($"    Instance Id: {awsEc2Infos.InstanceId}");
			sb.AppendLine($"    Instance Type: {awsEc2Infos.InstanceType}");
			sb.AppendLine($"    Instance Region: {awsEc2Infos.Region}");
			sb.AppendLine($"    Instance Version: {awsEc2Infos.Version}");
		}

		var azureVmInfos = new AzureVmInfos();
		var azureVmDetected = await azureVmInfos.DetectAzureVm();

		if (azureVmDetected)
		{
			Console.WriteLine("Running on an Azure VM.");
			Console.WriteLine($"    Name: {azureVmInfos.Name}");
			Console.WriteLine($"    Vm Id: {azureVmInfos.VmId}");
			Console.WriteLine($"    Resource Id: {azureVmInfos.ResourceId}");
			Console.WriteLine($"    Location: {azureVmInfos.Location}");
			Console.WriteLine($"    Version: {azureVmInfos.Version}");
			Console.WriteLine($"    Provider: {azureVmInfos.Provider}");
			Console.WriteLine($"    Publisher: {azureVmInfos.Publisher}");
			Console.WriteLine($"    Vm size: {azureVmInfos.VmSize}");
			Console.WriteLine($"    License type: {azureVmInfos.LicenseType}");
		}

		if (!awsEc2Detected && !azureVmDetected)
		{
			sb.AppendLine("No virtualization or cloud environment detected.");
		}

	    return sb.ToString();
    }

	private static void ReportError(string action, ErrorResultObjects error)
    {
	    if (null == error)
		    return;

	    Console.WriteLine($"{action} received an error: {error.Message} (Id: {error.Id})");
    }
}

