using Slascone.Client;
using Slascone.Client.Interfaces;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Slascone.Provisioning.Wpf.Sample.NuGet.Services;

namespace Slascone.Provisioning.Sample.NuGet
{
    /// <summary>
    /// Provides licensing functionality by interacting with the SLASCONE licensing API.
    /// Handles license activation, heartbeats, analytics, and session management.
    /// </summary>
    internal class LicensingService
    {
        private readonly ISlasconeClientV2 _slasconeClientV2;

        private string _licenseKey;
        private Guid? _tokenId;
        private IDictionary<Guid, string> _limitationMap;
        private Stack<Guid> _sessionIds = new Stack<Guid>();

        #region Construction

        /// <summary>
        /// Initializes a new instance of the LicensingService class.
        /// Configures the SLASCONE client with the appropriate settings for the environment.
        /// </summary>
        public LicensingService()
        {
            _slasconeClientV2 =
                SlasconeClientV2Factory.BuildClient(Settings.ApiBaseUrl, Settings.IsvId, Settings.ProvisioningKey);

            // If you are using Azure AD B2C authentication you can set the bearer token for authorization against the SLASCONE RestAPI.
            // Set the bearer token including the 'Bearer' prefix.
            //_slasconeClientV2 = SlasconeClientV2Factory.BuildClient(Settings.ApiBaseUrl, Settings.IsvId);
            //_slasconeClientV2.SetBearer(Settings.Bearer);

            // If you want to use the AdminKey instead of the ProvisioningKey (e.g., for internal or test purposes).
            //_slasconeClientV2 = SlasconeClientV2Factory.BuildClient(Settings.ApiBaseUrl, Settings.IsvId);
            //_slasconeClientV2.SetBearer(Settings.AdminKey);

#if NET6_0_OR_GREATER

            // Importing a RSA key from a PEM encoded string is available in .NET 6.0 or later
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(Settings.SignaturePubKeyPem.ToCharArray());
                _slasconeClientV2
                    .SetSignaturePublicKey(new PublicKey(rsa))
                    .SetSignatureValidationMode(Settings.SignatureValidationMode);
            }
#else

		// If you are not using .NET 6.0 or later you have to load the public key from a xml string
		_slasconeClientV2.SetSignaturePublicKeyXml(Settings.SignaturePublicKeyXml);
		_slasconeClientV2.SetSignatureValidationMode(Settings.SignatureValidationMode);

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

        #endregion

        /// <summary>
        /// Activates a license for the current device using the provided license key.
        /// </summary>
        /// <param name="licenseKey">The license key to activate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ActivateLicenseAsync(string licenseKey)
        {
            var activateClientDto = new ActivateClientDto
            {
                Product_id = Settings.ProductId,
                License_key = licenseKey,
                Client_id = DeviceInfoService.GetUniqueDeviceId(),
                Client_description = "",
                Client_name = "From GitHub Nuget Sample",
                Software_version = "25.2.0"
            };

            try
            {
                var result = await ErrorHandlingHelper.Execute(_slasconeClientV2.Provisioning.ActivateLicenseAsync, activateClientDto);

                if (null == result.data)
                {
                    ReportError(result);
                    return;
                }

                var licenseInfoDto = result.data;
                _licenseKey = licenseInfoDto.License_key;
                _tokenId = licenseInfoDto.Token_key;
                _limitationMap = LicensePrettyPrinter.PrintLicenseDetails(licenseInfoDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Sends a heartbeat to the SLASCONE service to validate the license.
        /// Updates license information based on the server response.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddHeartbeatAsync()
        {
            var heartbeatDto = new AddHeartbeatDto
            {
                Product_id = Settings.ProductId,
                Client_id = DeviceInfoService.GetUniqueDeviceId(),
                Software_version = "25.2.0",
                Operating_system = DeviceInfoService.GetOperatingSystem()
            };

            try
            {
                var result
                    = await ErrorHandlingHelper.Execute(_slasconeClientV2.Provisioning.AddHeartbeatAsync, heartbeatDto);

                if (null == result.data)
                {
                    ReportError(result);

                    if (result.error is { Id: 2006 })
                    {
                        // A common error when the license is not activated yet for the current device.
                        // A typical handling could be to ask the user for a license key and call ActivateLicenseAsync.
                        Console.WriteLine("The license has to be activated first.");
                    }

                    return;
                }

                var licenseInfoDto = result.data;
                _licenseKey = licenseInfoDto.License_key;
                _tokenId = licenseInfoDto.Token_key;
                _limitationMap = LicensePrettyPrinter.PrintLicenseDetails(licenseInfoDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Unassigns the license from the current device.
        /// Requires a token key from a previous license activation or heartbeat.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UnassignLicenseAsync()
        {
            if (!_tokenId.HasValue)
            {
                Console.WriteLine("You have to add a license heartbeat first to get a token for this operation.");
                return;
            }

            var unassignDto = new UnassignDto
            {
                Token_key = _tokenId.Value
            };

            try
            {
                var result = await ErrorHandlingHelper.Execute(_slasconeClientV2.Provisioning.UnassignLicenseAsync, unassignDto);
                
                if (null == result.data)
                {
                    ReportError(result);
                    return;
                }

                Console.WriteLine($"UnassignLicense received: {result.data}");

                // Clear the license key, token id, and limitation map after unassigning
                _licenseKey = null;
                _tokenId = null;
                _limitationMap = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Sends an analytical heartbeat to the SLASCONE service.
        /// Used to track analytical field data for the current device.
        /// </summary>
        /// <param name="analyticaFieldId">The ID of the analytical field to update.</param>
        /// <param name="value">The value to report for the analytical field.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAnalyticalHeartbeatAsync(Guid analyticaFieldId, string value)
        {
            var analyticalHeartbeatDto = new AnalyticalHeartbeatDto
            {
                Analytical_heartbeat = new List<AnalyticalFieldValueDto>(),
                Client_id = DeviceInfoService.GetUniqueDeviceId()
            };

            var analyticalField = new AnalyticalFieldValueDto
            {
                Analytical_field_id = analyticaFieldId,
                Value = value
            };
            analyticalHeartbeatDto.Analytical_heartbeat.Add(analyticalField);

            try
            {
                var result = await ErrorHandlingHelper.Execute(_slasconeClientV2.DataGathering.AddAnalyticalHeartbeatAsync, analyticalHeartbeatDto);

                if (null == result.data)
                {
                    ReportError(result);
                    return;
                }

                Console.WriteLine($"Analytical heartbeat received: {result.data}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Sends usage statistics to the SLASCONE service.
        /// Reports usage data for specified features.
        /// </summary>
        /// <param name="usages">A collection of usage field IDs and their corresponding values to report.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddUsageHeartbeatAsync(IEnumerable<(Guid usageFieldId, double value)> usages)
        {
            var usageHeartbeat = new FullUsageHeartbeatDto
            {
                Usage_heartbeat = new List<UsageHeartbeatValueDto>(),
                Client_id = DeviceInfoService.GetUniqueDeviceId()
            };

            foreach (var usage in usages)
            {
                var usageFeatureValue = new UsageHeartbeatValueDto
                {
                    Usage_feature_id = usage.usageFieldId,
                    Value = usage.value
                };
                usageHeartbeat.Usage_heartbeat.Add(usageFeatureValue);
            }

            try
            {
                var result =
                    await ErrorHandlingHelper.Execute((uhb) => _slasconeClientV2.DataGathering.AddUsageHeartbeatAsync(uhb, true), usageHeartbeat);

                if (null == result.data)
                {
                    ReportError(result);
                    return;
                }

                Console.WriteLine($"Usage heartbeat received: {result.data}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Sends consumption data to the SLASCONE service.
        /// Reports the consumption of limited resources to track usage against license limitations.
        /// </summary>
        /// <param name="consumptions">A collection of limitation IDs and their corresponding consumption values.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddConsumptionHeartbeatAsync(IEnumerable<(Guid, decimal)> consumptions)
        {
            var consumptionHeartbeat = new FullConsumptionHeartbeatDto
            {
                Client_id = DeviceInfoService.GetUniqueDeviceId(),
                Consumption_heartbeat = new List<ConsumptionHeartbeatValueDto>()
            };

            foreach (var consumption in consumptions)
            {
                var consumptionFeatureValue = new ConsumptionHeartbeatValueDto
                {
                    Limitation_id = consumption.Item1,
                    Value = consumption.Item2
                };
                consumptionHeartbeat.Consumption_heartbeat.Add(consumptionFeatureValue);
            }

            try
            {
                var result =
                    await ErrorHandlingHelper.Execute(_slasconeClientV2.DataGathering.AddConsumptionHeartbeatAsync, consumptionHeartbeat);

                if (null == result.data)
                {
                    ReportError(result);
                    return;
                }

                foreach (var consumptionDto in result.data)
                {
                    var limitation = _limitationMap.TryGetValue(consumptionDto.Limitation_id, out var limitationName)
                        ? $"'{limitationName}' ({consumptionDto.Limitation_id})"
                        : consumptionDto.Limitation_id.ToString();

                    if (null != consumptionDto.Transaction_id)
                        Console.WriteLine($"Limitation {limitation}: Remaining: {consumptionDto.Remaining}");
                    else
                        Console.WriteLine($"Limitation {limitation}: Limit reached!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Opens a new session with the SLASCONE service.
        /// Used for floating licenses to track concurrent usage.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OpenSessionAsync()
        {
            if (string.IsNullOrEmpty(_licenseKey))
            {
                Console.WriteLine("You have to add a license heartbeat first.");
                return;
            }

            var sessionId = Guid.NewGuid();

            var sessionDto = new SessionRequestDto
            {
                Client_id = DeviceInfoService.GetUniqueDeviceId(),
                License_id = Guid.Parse(_licenseKey),
                Session_id = sessionId
            };

            try
            {
                var result =
                    await ErrorHandlingHelper.Execute(_slasconeClientV2.Provisioning.OpenSessionAsync, sessionDto);

                if (null == result.data)
                {
                    ReportError(result);

                    if (result.error is { Id: 1007 })
                    {
                        // This error indicates that the maximum number of allowed parallel sessions has been reached.
                        // Normally you would inform the user about this and prevent the usage of the software.
                        Console.WriteLine("Maximum of allowed parallel opened sessions exceeded!");
                    }
                    return;
                }

                _sessionIds.Push(sessionId);

                var sessionStatus = result.data;
                Console.WriteLine($"Successfully opened session {sessionId}.");
                Console.WriteLine($"Number of concurrent sessions: {sessionStatus.Max_open_session_count}");
                Console.WriteLine($"Session valid until {sessionStatus.Session_valid_until}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Closes the most recently opened session with the SLASCONE service.
        /// Releases a floating license token for use by other clients.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CloseSessionAsync()
        {
            if (string.IsNullOrEmpty(_licenseKey))
            {
                Console.WriteLine("You have to add a license heartbeat first.");
                return;
            }

            var sessionId = _sessionIds.Any()
                ? _sessionIds.Pop()
                : new Guid();

            var sessionDto = new SessionRequestDto
            {
                Client_id = DeviceInfoService.GetUniqueDeviceId(),
                License_id = Guid.Parse(_licenseKey),
                Session_id = sessionId
            };

            try
            {
                var result = await ErrorHandlingHelper.Execute(_slasconeClientV2.Provisioning.CloseSessionAsync, sessionDto);

                if (null == result.data)
                {
                    ReportError(result);
                    return;
                }

                Console.WriteLine($"Successfully closed session {sessionId}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Looks up licenses associated with the given license key.
        /// Prints details of found licenses to the console.
        /// </summary>
        /// <param name="licenseKey">The license key to look up.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LookupLicensesAsync(string licenseKey)
        {
            var getLicenses = new GetLicensesByLicenseKeyDto
            {
                Product_id = Settings.ProductId,
                License_key = licenseKey
            };

            try
            {
                var result = await ErrorHandlingHelper.Execute(_slasconeClientV2.Provisioning.GetLicensesByLicenseKeyAsync, getLicenses);

                if (null == result.data)
                {
                    ReportError(result);
                    return;
                }

                var licenseDtos = result.data;

                Console.WriteLine($"Found {licenseDtos.Count} license(s) for key '{licenseKey}':");

                foreach (var licenseDto in licenseDtos)
                {
                    LicensePrettyPrinter.PrintLicenseDetails(licenseDto);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        /// <summary>
        /// Gets the SLASCONE client instance used by this service.
        /// </summary>
        public ISlasconeClientV2 SlasconeClientV2
            => _slasconeClientV2;

        /// <summary>
        /// Gets the current limitation map containing limitation IDs and their descriptions.
        /// </summary>
        public IDictionary<Guid, string> LimitationMap
            => _limitationMap;

        /// <summary>
        /// Reports errors from SLASCONE API operations to the console.
        /// </summary>
        /// <typeparam name="T">The type of data expected from the API operation.</typeparam>
        /// <param name="result">The result tuple containing data, error type, error object, and error message.</param>
        /// <param name="caller">The name of the calling method (automatically provided by compiler).</param>
        private static void ReportError<T>((T data, ErrorHandlingHelper.ErrorType errorType, ErrorResultObjects error, string message) result, [CallerMemberName] string caller = "")
        {
            Console.WriteLine($"Error during {caller}:");

            if (null != result.error)
            {
                Console.WriteLine($"Error code: {result.error.Id}");
                Console.WriteLine($"Error description: {result.error.Message}");
            }
            else
            {
                Console.WriteLine($"Error type: {result.errorType.ToString()}");
                Console.WriteLine($"Error message: {result.message}");
            }
        }
    }
}
