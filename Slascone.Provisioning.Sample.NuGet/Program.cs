﻿using System.Runtime.InteropServices;
using Slascone.Client;
using System.Xml;
using Slascone.Client.DeviceInfos;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Slascone.Client.Interfaces;

namespace Slascone.Provisioning.Sample.NuGet;

class Program
{
	private readonly ISlasconeClientV2 _slasconeClientV2;

	// CHANGE these values according to your environment 
	private readonly string _license_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0";    // Find your own license key at : https://my.slascone.com/licenses
	private readonly Guid _product_id = Guid.Parse("b18657cc-1f7c-43fa-e3a4-08da6fa41ad3");// Find your own product id key at : https://my.slascone.com/products

    private Guid? _token_id = Guid.Empty;
	private Stack<Guid> _sessionIds = new Stack<Guid>();
	private IDictionary<Guid, string> _limitationNames = new Dictionary<Guid, string>();

	public Program()
	{
		_slasconeClientV2 =
			SlasconeClientV2Factory.BuildClient(Helper.ApiBaseUrl, Helper.IsvId, Helper.ProvisioningKey);

        // If you are using Azure AD B2C authentication you can set the bearer token for authorization against the SLASCONE RestAPI.
        // Set the bearer token including the 'Bearer' prefix.
        //_slasconeClientV2 = SlasconeClientV2Factory.BuildClient(Helper.ApiBaseUrl, Helper.IsvId);
        //_slasconeClientV2.SetBearer(Helper.Bearer);

        // If you want to use the AdminKey instead of the ProvisioningKey (e.g., for internal or test purposes).
        //_slasconeClientV2 = SlasconeClientV2Factory.BuildClient(Helper.ApiBaseUrl, Helper.IsvId);
        //_slasconeClientV2.SetBearer(Helper.AdminKey);


#if NET6_0_OR_GREATER

        // Importing a RSA key from a PEM encoded string is available in .NET 6.0 or later
        using (var rsa = RSA.Create())
		{
			rsa.ImportFromPem(Helper.SignaturePubKeyPem.ToCharArray());
			_slasconeClientV2
				.SetSignaturePublicKey(new PublicKey(rsa))
				.SetSignatureValidationMode(Helper.SignatureValidationMode);
		}
#else

		// If you are not using .NET 6.0 or later you have to load the public key from a xml string
		_slasconeClientV2.SetSignaturePublicKeyXml(Helper.SignaturePublicKeyXml);
		_slasconeClientV2.SetSignatureValidationMode(Helper.SignatureValidationMode);

#endif

		_slasconeClientV2
			.SetCheckHttpsCertificate()
			.SetLastModifiedByHeader("Slascone.Provisioning.Sample.NuGet");

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

		Console.WriteLine("SLASCONE client app example");
		Console.WriteLine("===========================");
		Console.WriteLine();
		Console.WriteLine("Trying to detect Azure or AWS cloud environment or general virtualization ...");
		// You can speed up the application start by skipping the cloud and virtualization detection.
		// Just set the parameter of the GetUniqueDeviceId() function to 'false'
		Console.WriteLine($"Unique client_id for this device: {Helper.GetUniqueDeviceId(true)}");
		Console.WriteLine($"Operating system: {Helper.GetOperatingSystem()}");

		string input;
		do
		{
            Console.WriteLine("-- MAIN");
            Console.WriteLine("    1: Activate license (can be done only once per device)");
            Console.WriteLine("    2: Add license heartbeat (license check)");
            Console.WriteLine("    3: Temporary disconnection: Read local license file (only available after at least one license heartbeat)");
            Console.WriteLine("    4: Unassign license from device (has to be activated again then)");
            Console.WriteLine("-- ANALYTICS");
            Console.WriteLine("    5: Add analytical heartbeat");
            Console.WriteLine("    6: Add usage heartbeat");
            Console.WriteLine("    7: Add consumption heartbeat");
            Console.WriteLine("-- FLOATING");
            Console.WriteLine("    8: Open session");
            Console.WriteLine("    9: Find open session (temporary disconnection)");
            Console.WriteLine("    10: Close session");
            Console.WriteLine("-- OFFLINE ACTIVATION");
            Console.WriteLine("    11: Validate license file (signature check)");
            Console.WriteLine("    12: Validate license file and activation file");
            Console.WriteLine("-- MISC");
            Console.WriteLine("    13: Print client info");
            Console.WriteLine("    14: Print virtualization/cloud environment info");
            Console.WriteLine("    15: Print https chain of trust info");
            Console.WriteLine("    16: Lookup licenses");
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
                    pr.OfflineLicenseInfoExample();
                    break;

                case "4":
                    await pr.UnassignExample();
                    break;

                case "5":
					await pr.AnalyticalHeartbeatExample();
					break;

				case "6":
					await pr.UsageHeartbeatExample();
					break;

				case "7":
					await pr.ConsumptionHeartbeatExample();
					break;

                case "8":
                    await pr.OpenSessionExample();
                    break;

                case "9":
					pr.FindOpenSessionOffline();
					break;

				case "10":
					await pr.CloseSessionExample();
					break;

				case "11":
					pr.IsLicenseFileSignatureValid(Path.Combine("..", "..", "..", "Assets", "License-91fad880-90c4-46cb-8d8b-0a12445c6f0e.xml"));
					break;

				case "12":
					pr.OfflineLicenseActivationExample(
						Path.Combine("..", "..", "..", "Assets", "License-91fad880-90c4-46cb-8d8b-0a12445c6f0e.xml"),
						Path.Combine("..", "..", "..", "Assets", "ActivationFile.xml"));
					break;

				case "13":
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						Console.Write(WindowsDeviceInfos.LogDeviceInfos());
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
						Console.Write(LinuxDeviceInfos.LogDeviceInfos());
					break;

				case "14":
					Console.Write(pr.LogVirtualizationInfos());
					break;

                case "15":
                    pr.ChainOfTrustExample();
                    break;

                case "16":
                    await pr.LookupLicensesExample();
                    break;
            }
		} while (!"x".Equals(input, StringComparison.InvariantCultureIgnoreCase));
	}

	private async Task ActivationExample()
	{
		var activateClientDto = new ActivateClientDto
		{
			Product_id = _product_id,
			License_key = _license_key,
			Client_id = Helper.GetUniqueDeviceId(),
			Client_description = "",
			Client_name = "From GitHub Nuget Sample",
			Software_version = "12.2.8"
		};

		try
		{
			var result = await _slasconeClientV2.Provisioning.ActivateLicenseAsync(activateClientDto);
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
			else if (result.StatusCode == 503 || result.StatusCode == 504)
			{
				// Service is temporarily unavailable
				// Consider implementing a retry logic here to handle temporary service unavailability.
				// See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
				Console.WriteLine("Service is not available. Please try again later.");
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
			Product_id = _product_id,
			Client_id = Helper.GetUniqueDeviceId(),
			Software_version = "22.1",
			Operating_system = Helper.GetOperatingSystem()
		};

		try
		{
			var result = await _slasconeClientV2.Provisioning.AddHeartbeatAsync(heartbeatDto);
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
			else if (result.StatusCode == 503 || result.StatusCode == 504)
			{
				// Service is temporarily unavailable
				// Consider implementing a retry logic here to handle temporary service unavailability.
				// See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
				Console.WriteLine("Service is not available. Please try again later.");
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
			var result = await _slasconeClientV2.DataGathering.AddAnalyticalHeartbeatAsync(analyticalHeartbeatDto);
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
			else if (result.StatusCode == 503 || result.StatusCode == 504)
			{
				// Service is temporarily unavailable
				// Consider implementing a retry logic here to handle temporary service unavailability.
				// See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
				Console.WriteLine("Service is not available. Please try again later.");
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
			var result = await _slasconeClientV2.DataGathering.AddUsageHeartbeatAsync(usageHeartbeat, true);
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
			else if (result.StatusCode == 503 || result.StatusCode == 504)
			{
				// Service is temporarily unavailable
				// Consider implementing a retry logic here to handle temporary service unavailability.
				// See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
				Console.WriteLine("Service is not available. Please try again later.");
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
		var consumptionHeartbeat = new FullConsumptionHeartbeatDto
		{
			Client_id = Helper.GetUniqueDeviceId(),
			Consumption_heartbeat = new List<ConsumptionHeartbeatValueDto>()
		};

		var consumptionHeartbeatValue1 = new ConsumptionHeartbeatValueDto
		{
			Limitation_id = Guid.Parse("00cf2984-d71a-4c66-9f49-08da833189e3"),
			Value = 1
		};
		consumptionHeartbeat.Consumption_heartbeat.Add(consumptionHeartbeatValue1);

		try
		{
			var result = await _slasconeClientV2.DataGathering.AddConsumptionHeartbeatAsync(consumptionHeartbeat);

			if (result.StatusCode == 200)
			{
				foreach (var consumptionDto in result.Result)
				{
					var limitation = _limitationNames.TryGetValue(consumptionDto.Limitation_id, out var limitationName)
						? $"'{limitationName}' ({consumptionDto.Limitation_id})"
						: consumptionDto.Limitation_id.ToString();
					if (null != consumptionDto.Transaction_id)
						Console.WriteLine($"Limitation {limitation}: Remaining: {consumptionDto.Remaining}");
					else
						Console.WriteLine($"Limitation {limitation}: Limit reached!");
				}
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
			else if (result.StatusCode == 503 || result.StatusCode == 504)
			{
				// Service is temporarily unavailable
				// Consider implementing a retry logic here to handle temporary service unavailability.
				// See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
				Console.WriteLine("Service is not available. Please try again later.");
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
			var result = await _slasconeClientV2.Provisioning.UnassignLicenseAsync(unassignDto);
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
			else if (result.StatusCode == 503 || result.StatusCode == 504)
			{
				// Service is temporarily unavailable
				// Consider implementing a retry logic here to handle temporary service unavailability.
				// See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
				Console.WriteLine("Service is not available. Please try again later.");
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
			Product_id = _product_id,
			License_key = _license_key
		};

		try
		{

			var result = await _slasconeClientV2.Provisioning.GetLicensesByLicenseKeyAsync(getLicenses);

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
			else if (result.StatusCode == 503 || result.StatusCode == 504)
			{
				// Service is temporarily unavailable
				// Consider implementing a retry logic here to handle temporary service unavailability.
				// See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
				Console.WriteLine("Service is not available. Please try again later.");
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

	private void FindOpenSessionOffline()
	{
		var validSessionFound = _slasconeClientV2.Session.TryGetSessionStatus(Guid.Parse(_license_key), out var sessionId, out var sessionStatus);

		if (!validSessionFound)
		{
			Console.WriteLine("No valid session found.");

			if (null != sessionStatus) 
				Console.WriteLine($"Session expired since {sessionStatus.Session_valid_until}");

			return;
		}

		Console.WriteLine($"Found valid session with ID '{sessionId}'; session is valid until {sessionStatus.Session_valid_until}.");
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
            var result = await _slasconeClientV2.Provisioning.OpenSessionAsync(sessionDto);
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
            else if (result.StatusCode == 503 || result.StatusCode == 504)
            {
	            // Service is temporarily unavailable
	            // Consider implementing a retry logic here to handle temporary service unavailability.
	            // See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
	            Console.WriteLine("Service is not available. Please try again later.");
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
            var result = await _slasconeClientV2.Provisioning.CloseSessionAsync(sessionDto);
            if (result.StatusCode == 200)
            {
                Console.WriteLine($"Successfully closed session {sessionId}.");
            }
            else if (result.StatusCode == 409)
            {
	            ReportError("CloseSession", result.Error);
            }
            else if (result.StatusCode == 503 || result.StatusCode == 504)
            {
	            // Service is temporarily unavailable
	            // Consider implementing a retry logic here to handle temporary service unavailability.
	            // See: https://support.slascone.com/hc/en-us/articles/360016160398-ERROR-CODES
	            Console.WriteLine("Service is not available. Please try again later.");
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

    private void ChainOfTrustExample()
    {
	    var chainOfTrustInfo = _slasconeClientV2.HttpsChainOfTrust;

	    if (null == chainOfTrustInfo)
	    {
		    Console.WriteLine("No chain of trust information available. You have to call a API method first!");
		    return;
	    }

	    Console.Write(new StringBuilder()
		    .AppendLine("Chain of trust infos:")
		    .Append(string.Concat(chainOfTrustInfo.Select(certInfo =>
				    new StringBuilder().AppendLine((string)$" * {certInfo.Name}")
					    .AppendLine((string)$"    - Subject: {certInfo.Subject}")
					    .AppendLine((string)$"    - Issuer: {certInfo.Issuer}")
					    .AppendLine((string)$"    - Not before: {certInfo.NotBefore}")
					    .AppendLine((string)$"    - Not after: {certInfo.NotAfter}")
					    .AppendLine((string)$"    - Thumbprint: {certInfo.Thumbprint}")
					    .ToString()))
			    .ToString()));
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

	    Console.WriteLine($"   Active features: {string.Join(", ", license.License_features.Where(f => f.Is_active).Select(LicenseFeatureInfo))}");
	    Console.WriteLine($"   Limitations: {string.Join(", ", license.License_limitations.Select(l => $"{l.Limitation_name} = {l.Limit}"))}");
	    _limitationNames = license.License_limitations.ToDictionary(l => l.Limitation_id, l => l.Limitation_name);
    }

	private string LicenseFeatureInfo(LicenseFeatureDto licenseFeature)
	{
		var sb = new StringBuilder(licenseFeature.Feature_name);

		var currentException =
			licenseFeature.Feature_exceptions
				?.Exceptions
				.FirstOrDefault(exc => exc.Start_date_utc <= DateTime.Now.Date && DateTime.Now.Date <= exc.End_date_utc);

		if (null != currentException)
		{
			sb.Append($" (valid until {currentException.End_date_utc})");
		}

		return sb.ToString();
	}

	private void WriteLicenseInfo(LicenseInfoDto licenseInfo)
    {
	    Console.WriteLine($"License infos (Retrieved {licenseInfo.Created_date_utc}):");
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

	    Console.WriteLine($"   Active features: {string.Join(", ", licenseInfo.Features.Where(f => f.Is_active).Select(ProvisioningFeatureInfo))}");
	    Console.WriteLine($"   Limitations: {string.Join(", ", licenseInfo.Limitations.Select(l => $"{l.Name} = {l.Value}"))}");
	    _limitationNames = licenseInfo.Limitations.ToDictionary(l => l.Id, l => l.Name);
    }

	private string ProvisioningFeatureInfo(ProvisioningFeatureDto provisioningFeature)
	{
		var sb = new StringBuilder(provisioningFeature.Name);

		if (null != provisioningFeature.Expiration_date_utc)
		{
			sb.Append($" (valid until {provisioningFeature.Expiration_date_utc})");
		}

		return sb.ToString();
	}

    private bool IsLicenseFileSignatureValid(string licenseFile)
    {
	    var isValid = false;
	    try
	    {
		    isValid = _slasconeClientV2.IsFileSignatureValid(licenseFile);
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

    private void OfflineLicenseActivationExample(string licenseFile, string activationFile)
    {
        var licenseInfo = _slasconeClientV2.ReadLicenseFile(licenseFile);

        var offlineLicensingClientId = "24A43FCC-3674-0B19-A95F-047C160137E5";

        bool isActivated = false;

        if (null != licenseInfo.Client_id)
        {
            // If the license edition has the 'activation upon creation' mode 'client id' the client id is included in the license file.

            // Check inline activation
            if (licenseInfo.Client_id.Equals(offlineLicensingClientId, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Activation client_id in license file is valid");
                isActivated = true;
            }
            else
            {
                Console.WriteLine("Activation client_id in license file is invalid!");
            }
        }
        else
        {
            var activation = _slasconeClientV2.ReadActivationFile(activationFile);

            Console.Write("Validating the signature of the activation file: ");
            var isValid = IsLicenseFileSignatureValid(activationFile);

            if (activation.License_key.Equals(licenseInfo.License_key))
            {
                Console.WriteLine("Valid/Matching license_key");
                isActivated = true;
            }
            else
            {
                Console.WriteLine("Invalid/Not matching license_key");
            }

            // You have to compare the client_id with the client_id of the activation file!
            if (activation.Client_id.Equals(offlineLicensingClientId, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Activation client_id is valid");
                isActivated = true;
            }
            else
            {
                Console.WriteLine("Activation client_id is invalid!");
            }

            if (!isActivated)
            {
                Console.WriteLine("The activation file does not match the license file");
            }
        }

        if (isActivated)
        {
            Console.WriteLine("Successful validation");
            WriteLicenseInfo(licenseInfo);
        }
    }

    private string LogVirtualizationInfos()
    {
	    var sb = new StringBuilder();

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

        var awsEc2Detected = detectAws.Result;
        var azureVmDetected = detectAzure.Result;
        var virtualizationDetected = detectVirtualization.Result;

        if (awsEc2Detected)
		{
			sb.AppendLine("Running on an AWS EC2 instance:");
			sb.AppendLine($"    Instance Id: {awsEc2Infos.InstanceId}");
			sb.AppendLine($"    Instance Type: {awsEc2Infos.InstanceType}");
			sb.AppendLine($"    Instance Region: {awsEc2Infos.Region}");
			sb.AppendLine($"    Instance Version: {awsEc2Infos.Version}");
		}

		if (azureVmDetected)
		{
			sb.AppendLine("Running on an Azure VM.");
			sb.AppendLine($"    Name: {azureVmInfos.Name}");
			sb.AppendLine($"    Vm Id: {azureVmInfos.VmId}");
			sb.AppendLine($"    Resource Id: {azureVmInfos.ResourceId}");
			sb.AppendLine($"    Location: {azureVmInfos.Location}");
			sb.AppendLine($"    Version: {azureVmInfos.Version}");
			sb.AppendLine($"    Provider: {azureVmInfos.Provider}");
			sb.AppendLine($"    Publisher: {azureVmInfos.Publisher}");
			sb.AppendLine($"    Vm size: {azureVmInfos.VmSize}");
			sb.AppendLine($"    License type: {azureVmInfos.LicenseType}");
		}

		if (virtualizationDetected)
		{
			sb.AppendLine($"Virtualization detected: {virtualizationInfos.VirtualizationType}");
		}

		if (!awsEc2Detected && !azureVmDetected && !virtualizationDetected)
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

