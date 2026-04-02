# SLASCONE C# Licensing & Analytics Sample

This console sample shows how to integrate a software licensing platform via API, including license management, entitlement management, usage analytics, floating licenses, and offline licensing.
Going beyond simple API connectivity, it is designed as a production-oriented integration template for real-world licensing scenarios, including temporary offline operation, local license caching, and resilient handling of transient technical failures such as 5xx responses.

It includes examples for the most important licensing, analytics, and resilience workflows: The sample is intentionally built around the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client/), which provides a higher-level integration layer for common licensing workflows such as heartbeat handling, offline license access, and local caching.

* license activation and heartbeat
* floating session management
* license file validation (offline activation)
* analytical, usage, and consumption heartbeats
* offline fallback using cached license data
* response and file integrity validation
* error handling and retry logic

For more information, see the [SLASCONE website](https://slascone.com/), the [Help Center](https://support.slascone.com/), and the [API Test Center](https://api365.slascone.com/swagger).

## Quick Start

```bash
# Build the project
dotnet build

# Run the interactive sample
dotnet run --project Slascone.Provisioning.Sample.NuGet
```

The application starts with an interactive menu. By default, it connects to a SLASCONE demo environment so you can explore the licensing and analytics scenarios immediately.

## Table of Contents

* [Quick Start](#quick-start)
* [What This Sample Demonstrates](#what-this-sample-demonstrates)
* [Connecting to Your SLASCONE Environment](#connecting-to-your-slascone-environment)
* [Typical Licensing Flow](#typical-licensing-flow)
* [Offline Licensing and Freeride Period](#offline-licensing-and-freeride-period)
* [Configuration and Storage](#configuration-and-storage)
* [Error Handling and Retry Logic](#error-handling-and-retry-logic)
* [Technical Details](#technical-details)
* [Project Structure](#project-structure)
* [SLASCONE NuGet Client](#slascone-nuget-client)
* [Further Reading](#further-reading)

## What This Sample Demonstrates

This sample application showcases the following key features of the SLASCONE licensing service.

### License Management & Entitlements

**License Activation (online)**
   * Activates a license for a specific device using its unique device ID
   * Demonstrates how to activate a license key for the first time on a specific machine
   * Handles activation responses and potential warnings or errors
     
**License Heartbeat**
   * Sends periodic license verification requests to the SLASCONE server
   * Retrieves up-to-date license information including features, limitations, and expiration details
   * Caches license information for offline use

   ```csharp
   // Build the heartbeat request with the current device and product info
   var heartbeatDto = new AddHeartbeatDto
   {
       Product_id = Settings.ProductId,
       Client_id = DeviceInfoService.GetUniqueDeviceId(),
       Software_version = Settings.SoftwareVersion,
       Operating_system = DeviceInfoService.GetOperatingSystem()
   };

   // Execute with automatic retry logic for transient failures
   var result = await ErrorHandlingHelper.Execute(
       _slasconeClientV2.Provisioning.AddHeartbeatAsync, heartbeatDto);

   if (result.data == null)
   {
       // On network/technical errors, fall back to cached license data
       // On functional errors (e.g., 2006 = unknown client), handle accordingly
   }

   // On success, the client automatically caches the license
   // response (license.json + license_signature.txt) for offline use
   LicenseInfoDto licenseInfo = result.data;
   ```
     
**License Activation (offline)**
   * Validates the digital signature of license files to prevent tampering
   * Reads and displays comprehensive license information from XML files
   * Provides detailed analysis of license validity, features, and limitations
     
**Offline License Support**
   * Reads license information when temporarily disconnected from the internet
   * Uses cached license data stored during the last successful heartbeat
   * Ensures the software can continue to function during temporary network outages

   ```csharp
   // Read and validate the cached license when the server is unreachable
   var licenseInfo = _slasconeClientV2.GetOfflineLicense();

   if (licenseInfo != null)
   {
       // The signature was verified — the cached data has not been tampered with
       // Use licenseInfo to enforce features, limitations, and expiration as usual
   }
   else
   {
       // No valid cached license available (missing, expired, or tampered)
   }
   ```
     
**License Unassignment**
   * Demonstrates how to unassign a license from a device
   * Allows a license to be transferred to a different machine

### Analytics Capabilities

**Analytical Heartbeat**
   * Gathers general troubleshooting statistics
   * Supports custom fields for application-specific metrics

**Feature Usage Tracking**
   * Records which specific features are being used within the application
   * Tracks usage frequency and patterns for specific functionality
   * Provides insights for product development and pricing decisions

**Consumption Tracking**
   * Monitors consumption-based licensing metrics such as API calls or processed documents
   * Supports pay-per-use licensing models
   * Reports consumption against pre-defined limitations

### Floating License Management

**Session Management**
   * Opens licensing sessions for floating license scenarios
   * Supports concurrent user licensing models
   * Allows software to be installed on multiple machines while limiting concurrent use

**Offline Session Handling**
   * Reads and validates locally stored session data when temporarily disconnected
   * Ensures continuation of work during network interruptions
   * Maintains license compliance even in offline mode

**Session Closure**
   * Properly releases floating licenses back to the pool
   * Ensures efficient use of available licenses
   * Prevents license hoarding by inactive installations

### Security Features

**Device Identification**
   * Cross-platform device fingerprinting for Windows, Linux, and macOS
   * Secures licenses to specific hardware
   * Helps prevent unauthorized license transfers

**Digital Signature Validation**
   * Verifies the authenticity of license files and server responses
   * Prevents tampering with license data
   * Supports both symmetric and asymmetric cryptographic validation
     
**Replay Protection**
   * Uses a nonce-based challenge-response flow

## Connecting to Your SLASCONE Environment

By default, the application is configured to connect to a SLASCONE demo environment.

To connect it to your own SLASCONE environment, adjust the values in `Settings.cs`:

* `ApiBaseUrl`: your API endpoint
* `ProvisioningKey`: your provisioning API key
* `IsvId`: your ISV identifier
* `ProductId`: your product identifier

You can find these values as explained [here](https://support.slascone.com/hc/en-us/articles/360016153358#common-parameters). For meaningful testing and evaluation, your SLASCONE environment should have at least one active license.

> ⚠️ **Security Warning**: Keep provisioning keys secure and do not embed production secrets in publicly accessible repositories. More about secrets, see the [Secrets](https://support.slascone.com/hc/en-us/articles/7702036319261#secrets) section in the SLASCONE Help Center.

## Typical Licensing Flow

A typical integration follows this sequence:

1. Identify the current client or device.
2. Activate the license for that client.
3. Send regular heartbeats to retrieve the latest license state.
4. Store the last valid license information locally.
5. Fall back to the cached license state during temporary connectivity issues.
6. Open and close sessions when using floating licenses.
7. Send analytical, usage, or consumption heartbeats as required by your product.
8. Unassign the license when needed.

This sample demonstrates each of these steps in a compact console application so you can adapt the relevant parts to your own product.

## Offline Licensing and Freeride Period

The SLASCONE licensing system provides robust support for (temporary) offline scenarios, which is especially important for desktop applications and intermittently connected systems. For more background on temporary and permanent offline scenarios, see the [Offline & Connectivity](https://support.slascone.com/hc/en-us/sections/10214124833693) section in the SLASCONE Help Center.

This sample demonstrates how to implement this behavior.

1. **License Caching**

   * During a successful license heartbeat, the application saves the license data locally
   * The cached license information includes features, limitations, and expiration details
   * The data is protected with digital signatures to prevent tampering
2. **Offline Validation**

   * When the application cannot connect to the SLASCONE server, it falls back to the cached license data
   * The application verifies the digital signature of the cached data before using it
   * All relevant license rules such as features, limitations, and expiration continue to be enforced based on the cached state
3. **Implementation**

   * The sample stores license data in `license.json` and its signature in `license_signature.txt`
   * For floating licenses, session information is stored in `session.token`
   * The sample demonstrates how to read and validate this information in offline mode

### Freeride Period

The freeride period provides flexibility when heartbeats fail, allowing users to continue using the software for a defined grace period.

1. **Purpose**

   * Prevents immediate software lockout when a heartbeat fails
   * Gives users time to resolve connectivity issues
   * Ensures a smoother user experience in environments with intermittent connectivity
2. **Functionality**

   * If a heartbeat fails, the software continues to work normally during the freeride period
   * The application tracks the time since the last successful heartbeat
   * Once a successful heartbeat occurs, the freeride period is reset
   * If the freeride period expires without a successful heartbeat, license enforcement takes effect
3. **Configuration**

   * Freeride periods are configured at the license edition level in the SLASCONE portal
   * The freeride duration is typically specified in days
   * This sample demonstrates how to implement and respect freeride periods
   * The `checkAndReadOfflineLicenseExample` method shows how to display freeride information
4. **Example Scenario**

   * With a daily heartbeat requirement and a 7-day freeride period
   * If heartbeats fail, the application continues working for 7 days
   * During this time, the application should notify the user about the need to go online
   * If a heartbeat succeeds within those 7 days, normal operation resumes
   * After 7 days without a successful heartbeat, the license becomes invalid

This approach ensures that temporary network issues or brief periods offline do not disrupt users' work while still maintaining proper license enforcement in the long term.

## Configuration and Storage

For detailed guidance on what should be stored locally, why it matters, and how cached license state supports offline and freeride scenarios, see [What to Store Locally in Your Client](https://support.slascone.com/hc/en-us/articles/7702036319261).

### Application Data Folder

The sample application stores license and session files in a dedicated application data folder instead of the current working directory.

This provides several benefits:

1. **Persistent Storage**

   * License and session files remain accessible across application restarts
2. **Centralized Location**

   * All application data is stored in a single, dedicated location
3. **Security**

   * Files are stored outside of the application directory, reducing the risk of accidental deletion

#### Default Location

By default, the application data is stored in:
- The current directory of the running app on Linux
- `%ProgramData%\Slascone.Provisioning.Sample.NuGet` on Windows

#### Custom Location


The application data folder is managed by the `SlasconeClientV2` class. To use a custom location for application data, specify it when initializing the client:

```csharp
_slasconeClientV2.SetAppDataFolder("<custom path>");
```

#### Stored Files

The following files are managed in the application data folder:

- `license.json`: The cached license information from the last successful heartbeat
- `license_signature.txt`: The digital signature for verifying the license file
- `session.token`: Information about the current floating license session (if applicable); also contains the digital signature

All files are automatically created, updated, and removed as needed during application operation.

## Error Handling and Retry Logic

For detailed information about SLASCONE API error codes, refer to the [SLASCONE error handling documentation](https://support.slascone.com/hc/en-us/articles/360016160398).
This sample application demonstrates how to handle SLASCONE API errors and implement retry logic using the `ErrorHandlingHelper` class.

All API calls are routed through this helper, which provides:

* consistent error classification
* automatic retries for transient failures
* a unified response wrapper

### Error Categories

The `ErrorHandlingHelper` classifies API errors into three categories.

1. **Functional Errors (HTTP 409)**

   * Represent business logic conflicts returned by the SLASCONE API
   * Examples include attempting to activate an already activated license, unknown client IDs, or exceeded license limits
   * The response body is automatically parsed into an `ErrorResultObjects` instance with a specific error code and message
   * These errors are never retried because they require the caller to address the underlying business logic issue
2. **Technical Errors (HTTP 4xx and 5xx)**

   * Represent server-side or request issues such as internal server errors, bad gateways, or service unavailability
   * Transient HTTP errors such as `408`, `429`, `502`, `503`, `504`, and `507` are automatically retried
   * Non-transient errors such as `401`, `403`, or `404` are returned immediately without retry
3. **Network Errors**

   * Represent connectivity issues such as socket timeouts, connection refusals, DNS resolution failures, or SSL errors
   * Transient network exceptions such as `HttpRequestException` are automatically retried
   * Non-transient network exceptions are returned immediately

### Retry Logic

The `ErrorHandlingHelper` implements automatic retry logic for transient errors.

1. **Retry Count**

   * By default, the helper performs a maximum of one automatic retry via `MaxRetryCount`
   * This follows the SLASCONE recommendation of a moderate retry policy
2. **Wait Time**

   * The default wait time between retries is 15 seconds via `RetryWaitTime`
3. **Retry-After Header**

   * For HTTP errors that include a `Retry-After` response header, commonly with `429` or `503`, the helper uses the server-specified wait time instead of the default
   * This helps align the client with rate limiting and server availability signals
4. **Non-Transient Errors**

   * Errors that are not classified as transient, such as `404` or a functional `409` conflict, are returned immediately without any retry attempt

### Handling API Responses

All API calls wrapped by `ErrorHandlingHelper.Execute()` return a tuple `(TOut data, ErrorType errorType, ErrorResultObjects error, string message)`, which encapsulates either a successful result or error details.

1. **Success Check**

   * Check `result.data` to determine whether the API call succeeded or failed
2. **Success Path**

   * Use `result.data` to access the API response data such as `LicenseInfoDto` or `SessionStatus`
3. **Error Inspection**

   * When an error occurs, use the following properties:

     * `result.errorType` returns the error category: `Functional`, `Technical`, or `Network`
     * `result.message` returns a formatted error description
     * `result.error` returns the parsed `ErrorResultObjects` for functional errors
4. **Usage Example**

```csharp
var result = await ErrorHandlingHelper.Execute(
    _slasconeClientV2.Provisioning.ActivateLicenseAsync,
    activateClientDto);

if (ErrorHandlingHelper.ErrorType.None != result.errorType)
{
    Console.WriteLine($"Error Type: {result.errorType}");
    Console.WriteLine($"Message: {result.message}");

    if (ErrorHandlingHelper.ErrorType.Functional == result.errorType && result.error != null)
    {
        // Handle specific business logic error codes
        int errorId = result.error.Id;
    }

    return;
}

LicenseInfoDto licenseInfo = result.data;
```

### Recommended Error Handling Strategy

Based on the SLASCONE error handling guidelines, consider the following strategies when integrating SLASCONE licensing into your application.

1. **Always Handle HTTP 409 Explicitly**

   * These are business logic responses, not unexpected errors
   * Check the specific error code from `result.error.Id` and handle each case according to your application's needs
   * Refer to the endpoint-specific documentation in the [SLASCONE API](https://api365.slascone.com/swagger) for possible conflict scenarios
2. **Fallback for Transient Failures**

   * The built-in retry logic handles the first retry automatically
   * If retries are exhausted, implement a fallback strategy such as using cached license data from the last successful heartbeat
3. **Heartbeat Failure Resilience**

   * When a license heartbeat fails after retries, fall back to the locally cached license data
   * The freeride period provides a grace period during which the application can continue operating
   * Reserve freeride logic for true offline scenarios rather than generic server-side errors
4. **Session Open Resilience**

   * For floating license session open failures caused by transient errors, consider a resilience strategy that preserves usability while maintaining long-term license compliance once connectivity is restored

## Technical Details

### Environment Requirements

* **.NET Version**: .NET 8 or newer is required. The project was developed and tested with .NET 8.
* **Network**: Internet connectivity is required for initial license activation and for online heartbeat operations

### Dependencies Overview

This application relies on several key libraries:

* **[Slascone.Client](https://www.nuget.org/packages/Slascone.Client)** for REST API communication with the SLASCONE server, including JSON serialization and offline license management
* **System.Management** for WMI-based device identification on Windows
* **System.Security.Cryptography.Pkcs** and **System.Security.Cryptography.Xml** for digital signature validation of license files and server responses

### Cross-Platform Compatibility

The application includes platform-specific code to obtain device IDs on:

* **Windows**: uses WMI to query the system UUID
* **Linux**: reads from `/etc/machine-id` or generates a UUID from the hostname
* **macOS**: uses `ioreg` to obtain the system hardware UUID

### Security Considerations

* **API Keys**: The sample uses demo API keys. In production, keep provisioning keys secure.
* **Signature Validation**: The sample demonstrates both symmetric and asymmetric signature validation.
* **Device Binding**: Licenses are bound to specific devices by their hardware IDs.

## Project Structure

```plaintext
SLASCONE-demo-csharp-nuget/               # Root project
├── Slascone.Provisioning.Sample.NuGet/   # Main application module
│   ├── DeviceInfoService.cs              # Device identification service
│   ├── ErrorHandlingHelper.cs            # Error handling utilities
│   ├── LicensePrettyPrinter.cs           # License information display
│   ├── LicensingService.cs               # Main licensing service
│   ├── Program.cs                        # Main program with interactive menu
│   ├── Settings.cs                       # Configuration settings
│   ├── Assets/                           # License file examples
│   └── Slascone.Provisioning.Sample.NuGet.csproj # Project file
└── README.md                             # This documentation file
```

## SLASCONE NuGet Client
## SLASCONE NuGet Client

This sample is built on the [SLASCONE NuGet Client](https://www.nuget.org/packages/Slascone.Client) rather than direct API calls. The client provides a higher-level integration layer for common licensing workflows and supports features such as offline access, local persistence, and resilient request handling.

## Further Reading

* [API Test Center](https://api365.slascone.com/swagger)
* [What and How to Save in Your Client](https://support.slascone.com/hc/en-us/articles/7702036319261)
* [Digital Signature and Data Integrity](https://support.slascone.com/hc/en-us/articles/360016063637)
* [Error Handling](https://support.slascone.com/hc/en-us/articles/360016160398)
* [Product Analytics](https://support.slascone.com/hc/en-us/articles/360016055537)
* [Consumption-Based Limitations](https://support.slascone.com/hc/en-us/articles/360016057197)

