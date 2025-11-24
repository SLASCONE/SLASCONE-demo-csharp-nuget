using Slascone.Client;
using Slascone.Client.DeviceInfos;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Slascone.Client.Interfaces;
using System.Diagnostics; // Added for memory/time diagnostics

namespace Slascone.Provisioning.Sample.NuGet;

/// <summary>
/// Main program class demonstrating SLASCONE licensing API integration.
/// Provides a menu-driven interface for exploring various licensing features.
/// </summary>
class Program
{
    private readonly LicensingService _licensingService;

    // This is a sample license key for demonstration purposes only.
    string _license_key = "27180460-29df-4a5a-a0a1-78c85ab6cee0";    // Find your own license key at : https://my.slascone.com/licenses

    /// <summary>
    /// Initializes a new instance of the Program class.
    /// Creates and configures the licensing service.
    /// </summary>
	public Program()
	{
        _licensingService = new LicensingService();
    }

    /// <summary>
    /// Entry point for the application.
    /// Creates a Program instance and starts the main menu.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
	static Task Main(string[] args)
	{
		var pr = new Program();
        return pr.MainMenu(args);
    }

    /// <summary>
    /// Displays the main menu and handles user interactions.
    /// Provides options for license activation, heartbeats, analytics, and offline features.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    async Task MainMenu(string[] args)
    {
        Console.WriteLine("SLASCONE client app example");
		Console.WriteLine("===========================");
		Console.WriteLine();
		Console.WriteLine("Trying to detect Azure or AWS cloud environment or general virtualization ...");
		// You can speed up the application start by skipping the cloud and virtualization detection.
		// Just set the parameter of the GetUniqueDeviceId() function to 'false'
		Console.WriteLine($"Unique client_id for this device: {DeviceInfoService.GetUniqueDeviceId(true)}");
		Console.WriteLine($"Operating system: {DeviceInfoService.GetOperatingSystem()}");

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
            Console.WriteLine("    17: Memory stress test (sequential + parallel)");
            Console.WriteLine("x: Exit demo app");

            Console.Write("> ");
			input = Console.ReadLine();

			switch (input)
			{
				case "1":
					await _licensingService.ActivateLicenseAsync(_license_key);
					break;

				case "2":
					await _licensingService.AddHeartbeatAsync();
                    break;

                case "3":
                    OfflineLicenseInfoExample();
                    break;

                case "4":
                    await _licensingService.UnassignLicenseAsync();
                    break;

                case "5":
					await AnalyticalHeartbeatExample();
					break;

				case "6":
					await UsageHeartbeatExample();
					break;

				case "7":
					await ConsumptionHeartbeatExample();
					break;

                case "8":
                    await _licensingService.OpenSessionAsync();
                    break;

                case "9":
					FindOpenSessionOffline();
					break;

				case "10":
                    await _licensingService.CloseSessionAsync();
					break;

				case "11":
					IsLicenseFileSignatureValid(Path.Combine("..", "..", "..", "Assets", "License-91fad880-90c4-46cb-8d8b-0a12445c6f0e.xml"));
					break;

				case "12":
					OfflineLicenseActivationExample(
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
					Console.Write(DeviceInfoService.GetVirtualizationInfos());
					break;

                case "15":
                    ChainOfTrustExample();
                    break;

                case "16":
                    await _licensingService.LookupLicensesAsync(_license_key);
                    break;

                case "17":
                    LoadTest();
                    break;
            }
		} while (!"x".Equals(input, StringComparison.InvariantCultureIgnoreCase));
	}

    /// <summary>
    /// The Slascone.Client nuget package writes the license information to the local storage
    /// on successful license activation or license heartbeat.
    /// This demonstrates how to read the cached license information from the local storage
    /// and to check if the license is still valid.
    /// This function enables the software to work with licensing information even if the client
    /// lost internet connection temporarily.
    /// </summary>
    private void OfflineLicenseInfoExample()
    {
        var response = SlasconeClientV2.GetOfflineLicense();

        if ((int)HttpStatusCode.OK != response.StatusCode)
        {
            Console.WriteLine(response.Message);
            return;
        }

        var licenseInfo = response.Result;

        LicensePrettyPrinter.PrintLicenseDetails(licenseInfo);

        if (licenseInfo.Created_date_utc.HasValue)
        {
            // Check how old the stored license info is
            var licenseInfoAge = (DateTime.Now - licenseInfo.Created_date_utc.Value).Days;
            Console.WriteLine($"   Offline license info is {licenseInfoAge} days old.");

            if (0 < licenseInfoAge && licenseInfo.Freeride.HasValue)
            {
                Console.Write($"   Freeride period: {licenseInfo.Freeride.Value}; ");
                if (licenseInfoAge <= licenseInfo.Freeride.Value)
                {
                    Console.WriteLine("License is valid because the defined freeride period is adhered to.");
                }
                else
                    Console.WriteLine("License invalid due to freeride period exceeded.");
            }
        }
    }
	
    /// <summary>
    /// Demonstrates sending analytical data to the SLASCONE service.
    /// Collects user input and sends it as an analytical heartbeat.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task AnalyticalHeartbeatExample()
	{
        Console.Write("Value for analytical field: ");
        var value = Console.ReadLine();

        await _licensingService.AddAnalyticalHeartbeatAsync(Guid.Parse("2754aca1-4d1a-4af3-9387-08da9ac54c6d"), value);
    }

    /// <summary>
    /// Demonstrates tracking usage statistics with the SLASCONE service.
    /// Collects user input for predefined usage features and sends it to the server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UsageHeartbeatExample()
    {
        var usageHeartbeat = new FullUsageHeartbeatDto
        {
            Usage_heartbeat = new List<UsageHeartbeatValueDto>(),
            Client_id = DeviceInfoService.GetUniqueDeviceId()
        };

        var usageFeatureIds = new[]
        {
            Guid.Parse("66099049-0472-467c-6ea6-08da9ac57d7c"),
            Guid.Parse("e82619b1-f403-4e0d-5389-08da9e17dd73")
        };

        var usages = new List<(Guid, double)>();
		foreach (var usageFeatureId in usageFeatureIds)
		{
			Console.Write($"Value for usage feature {usageFeatureId}: ");
			var input = Console.ReadLine();
			double.TryParse(input, out var value);
			usages.Add((usageFeatureId, value));
        }

        await _licensingService.AddUsageHeartbeatAsync(usages);
    }

    /// <summary>
    /// Demonstrates tracking consumption of limited resources with the SLASCONE service.
    /// Collects user input for each limitation defined in the license and sends it to the server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
	private async Task ConsumptionHeartbeatExample()
	{
        if (null == _licensingService.LimitationMap || !_licensingService.LimitationMap.Any())
        {
            Console.WriteLine("There are no limitations.");
            Console.WriteLine(
                "You have to add a license heartbeat first to get license information or the license doesn't provide any limitation to enter consumptions for.");
            return;
        }

        var consumptions = new List<(Guid, decimal)>();
        foreach (var kvp in _licensingService.LimitationMap)
        {
			Console.Write($"Value for limitation '{kvp.Value}': ");
			var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || !decimal.TryParse(input, out var value))
            {
                Console.WriteLine("Invalid input! Must be a number.");
                continue;
            }

            consumptions.Add((kvp.Key, value));
        }

		await _licensingService.AddConsumptionHeartbeatAsync(consumptions);
    }

    /// <summary>
    /// Demonstrates retrieving offline session information.
    /// Checks if there's an active session stored locally and displays its status.
    /// The Slascone.Client nuget package writes the session information to the local storage if a session is opened successfully.
    /// </summary>
	private void FindOpenSessionOffline()
	{
		var validSessionFound = SlasconeClientV2.Session.TryGetSessionStatus(Guid.Parse(_license_key), out var sessionId, out var sessionStatus);

		if (!validSessionFound)
		{
			Console.WriteLine("No valid session found.");

			if (null != sessionStatus) 
				Console.WriteLine($"Session expired since {sessionStatus.Session_valid_until}");

			return;
		}

		Console.WriteLine($"Found valid session with ID '{sessionId}'; session is valid until {sessionStatus.Session_valid_until}.");
	}

    /// <summary>
    /// Demonstrates retrieving HTTPS certificate chain of trust information.
    /// Displays details about the SSL/TLS certificates used by the SLASCONE service.
    /// </summary>
    private void ChainOfTrustExample()
    {
	    var chainOfTrustInfo = SlasconeClientV2.HttpsChainOfTrust;

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

    /// <summary>
    /// Validates the digital signature of a license file.
    /// Uses the SLASCONE client to verify that the license file has not been tampered with.
    /// </summary>
    /// <param name="licenseFile">The path to the license file to validate.</param>
    /// <returns>True if the signature is valid, false otherwise.</returns>
    private bool IsLicenseFileSignatureValid(string licenseFile)
    {
	    var isValid = false;
	    try
	    {
            isValid = SlasconeClientV2.IsFileSignatureValid(licenseFile);
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

    /// <summary>
    /// Demonstrates offline license activation with a license file and an activation file.
    /// Validates both files and checks if the license is properly activated for the current client.
    /// </summary>
    /// <param name="licenseFile">The path to the license file.</param>
    /// <param name="activationFile">The path to the activation file.</param>
    private void OfflineLicenseActivationExample(string licenseFile, string activationFile)
    {
        var licenseInfo = SlasconeClientV2.ReadLicenseFile(licenseFile);

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
            var activation = SlasconeClientV2.ReadActivationFile(activationFile);

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
            LicensePrettyPrinter.PrintLicenseDetails(licenseInfo);
        }
    }

    /// <summary>
    /// Memory stress test for licensing operations (sequential + parallel).
    /// Creates fresh LicensingService instances to mimic stateless request processing.
    /// Monitors allocations, working set and GC collections.
    /// </summary>
    private void LoadTest()
    {
        var licenseFilePath = Path.Combine("..", "..", "..", "Assets", "License-91fad880-90c4-46cb-8d8b-0a12445c6f0e.xml");
        const int iterations = 10000; // sequential iterations

        Console.WriteLine("=== Sequential stress test ===");
        Console.WriteLine($"License file: {licenseFilePath}");
        Console.WriteLine($"Iterations: {iterations}");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var proc = Process.GetCurrentProcess();
        long initialAllocated = GC.GetTotalAllocatedBytes(true);
        long initialWorkingSet = proc.WorkingSet64;
        long initialPrivate = proc.PrivateMemorySize64;
        int gen0Before = GC.CollectionCount(0);
        int gen1Before = GC.CollectionCount(1);
        int gen2Before = GC.CollectionCount(2);

        var sw = Stopwatch.StartNew();
        long peakWorkingSet = initialWorkingSet;

        for (int i = 1; i <= iterations; i++)
        {
            var licensingService = new LicensingService();
            licensingService.SlasconeClientV2.ReadLicenseFile(licenseFilePath);
            licensingService.SlasconeClientV2.IsFileSignatureValid(licenseFilePath);

            // Periodic reporting
            if (0 == i % 1000)
            {
                long allocated = GC.GetTotalAllocatedBytes(false);
                peakWorkingSet = Math.Max(peakWorkingSet, proc.WorkingSet64);
                Console.WriteLine($"Iter {i,5}: AllocatedΔ={FormatBytes(allocated - initialAllocated),12} WS={FormatBytes(proc.WorkingSet64),12} Private={FormatBytes(proc.PrivateMemorySize64),12}");
                // Optional GC to see if memory is released
                //GC.Collect();
            }
        }

        sw.Stop();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long finalAllocated = GC.GetTotalAllocatedBytes(true);
        long finalWorkingSet = proc.WorkingSet64;
        long finalPrivate = proc.PrivateMemorySize64;
        int gen0After = GC.CollectionCount(0);
        int gen1After = GC.CollectionCount(1);
        int gen2After = GC.CollectionCount(2);

        Console.WriteLine("--- Sequential summary ---");
        Console.WriteLine($"Time: {sw.Elapsed}");
        Console.WriteLine($"Allocated totalΔ: {FormatBytes(finalAllocated - initialAllocated)}");
        Console.WriteLine($"WorkingSetΔ: {FormatBytes(finalWorkingSet - initialWorkingSet)} (Peak: {FormatBytes(peakWorkingSet)})");
        Console.WriteLine($"PrivateMemΔ: {FormatBytes(finalPrivate - initialPrivate)}");
        Console.WriteLine($"Gen0 collections: {gen0After - gen0Before}, Gen1: {gen1After - gen1Before}, Gen2: {gen2After - gen2Before}");
        Console.WriteLine($"Avg allocated per iteration (approx): {FormatBytes((finalAllocated - initialAllocated) / iterations)}");

        // Parallel test
        Console.WriteLine();
        Console.WriteLine("=== Parallel stress test ===");
        const int parallelIterations = 5000; // reduce slightly to avoid excessive pressure
        int degree = Environment.ProcessorCount;
        Console.WriteLine($"Iterations: {parallelIterations}  DegreeOfParallelism: {degree}");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long beforeParallelAllocated = GC.GetTotalAllocatedBytes(true);
        long beforeParallelWS = proc.WorkingSet64;
        var swPar = Stopwatch.StartNew();

        Parallel.For(0, parallelIterations, new ParallelOptions { MaxDegreeOfParallelism = degree }, i =>
        {
            var licensingService = new LicensingService();
            licensingService.SlasconeClientV2.ReadLicenseFile(licenseFilePath);
            licensingService.SlasconeClientV2.IsFileSignatureValid(licenseFilePath);
        });

        swPar.Stop();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long afterParallelAllocated = GC.GetTotalAllocatedBytes(true);
        long afterParallelWS = proc.WorkingSet64;

        Console.WriteLine("--- Parallel summary ---");
        Console.WriteLine($"Time: {swPar.Elapsed}");
        Console.WriteLine($"Allocated totalΔ: {FormatBytes(afterParallelAllocated - beforeParallelAllocated)}");
        Console.WriteLine($"WorkingSetΔ: {FormatBytes(afterParallelWS - beforeParallelWS)}");
        Console.WriteLine($"Avg allocated per iteration (approx): {FormatBytes((afterParallelAllocated - beforeParallelAllocated) / parallelIterations)}");
    }
    
    private static string FormatBytes(long bytes)
        => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F2} KB",
            < 1024L * 1024 * 1024 => $"{bytes / 1024.0 / 1024.0:F2} MB",
            _ => $"{bytes / 1024.0 / 1024.0 / 1024.0:F2} GB"
        };
    
    public ISlasconeClientV2 SlasconeClientV2
        => _licensingService.SlasconeClientV2;
}

