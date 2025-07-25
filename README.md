# LICENSING & ANALYTICS FOR SOFTWARE AND IoT VENDORS

For more information about this solution, visit
https://slascone.com/ and/or
https://support.slascone.com/

This example uses the official [Slascone.Client NuGet package](https://www.nuget.org/packages/Slascone.Client).

## SLASCONE FEATURES DEMONSTRATED IN THIS SAMPLE

This sample application showcases the following key features of the SLASCONE licensing service:

### License Management

1. **License Activation**
   - Activates a license for a specific device using its unique device ID
   - Demonstrates how to activate a license key for the first time on a specific machine
   - Handles activation responses and potential warnings or errors

2. **License Heartbeat**
   - Sends periodic license verification to the SLASCONE server
   - Retrieves up-to-date license information including features, limitations, and expiration details
   - Caches license information for offline use

3. **Offline License Support**
   - Reads license information when temporarily disconnected from the internet
   - Uses cached license data stored during the last successful heartbeat
   - Ensures software can function during temporary network outages

4. **License Unassignment**
   - Demonstrates how to unassign a license from a device
   - Allows licenses to be transferred to different machines

5. **License File Handling**
   - Validates the digital signature of license files to prevent tampering
   - Reads and displays comprehensive license information from XML files
   - Provides detailed analysis of license validity, features, and limitations

### Analytics Capabilities

1. **Analytical Heartbeat**
   - Gathers general troubleshooting statistics
   - Supports custom fields for gathering application-specific metrics

2. **Feature Usage Tracking**
   - Records which specific features are being used within the application
   - Tracks usage frequency and patterns for specific functionality
   - Provides insights for product development and pricing decisions

3. **Consumption Tracking**
   - Monitors consumption-based licensing metrics (API calls, processed documents, etc.)
   - Supports pay-per-use licensing models
   - Reports consumption against pre-defined limitations

### Floating License Management

1. **Session Management**
   - Opens licensing sessions for floating license scenarios
   - Supports concurrent user licensing models
   - Allows software to be installed on multiple machines but used by a limited number simultaneously

2. **Offline Session Handling**
   - Finds and validates sessions when temporarily disconnected
   - Ensures continuation of work during network interruptions
   - Maintains license compliance even in offline mode

3. **Session Closure**
   - Properly releases floating licenses back to the pool
   - Ensures efficient use of available licenses
   - Prevents license hoarding by inactive installations

### Security Features

1. **Digital Signature Validation**
   - Verifies the authenticity of license files and server responses
   - Prevents tampering with license data
   - Supports both symmetric and asymmetric cryptographic validation

2. **Device Identification**
   - Cross-platform device fingerprinting (Windows, Linux, macOS)
   - Secures licenses to specific hardware
   - Prevents unauthorized license transfers

## CONNECTING TO YOUR SLASCONE ENVIRONMENT

The application connects to the SLASCONE official demo environment. In order to connect to your SLASCONE environment, adjust the values of the file `Settings.cs`:

- `ApiBaseUrl`: Your SLASCONE API endpoint
- `ProvisioningKey`: Your provisioning key for authentication
- `IsvId`: Your ISV (Independent Software Vendor) identifier

## TECHNICAL DETAILS

### Security Considerations

- **API Keys**: The sample uses demo API keys. In production, keep your provisioning keys secure.
- **Signature Validation**: The sample demonstrates both symmetric and asymmetric signature validation.
- **Device Binding**: Licenses are bound to specific devices by their hardware IDs.

### Project Structure

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

## GETTING STARTED

1. Clone this repository
2. Open the solution in Visual Studio 2022
3. Build the project
4. Run the application
5. Explore different licensing scenarios using the interactive menu
6. Review the code to understand how to implement these features in your own applications

For integration into your software product, focus on the relevant sections that match your licensing needs.

## OFFLINE CAPABILITIES AND FREERIDE PERIOD

### Temporary Offline Scenarios

The Slascone.Client NuGet package provides robust support for temporary offline scenarios, which is essential for desktop applications that may not always have internet connectivity. This sample demonstrates how to implement this functionality:

1. **License Caching**: 
   - During a successful license heartbeat, the NuGet package saves the license data locally
   - This cached license information includes all features, limitations, and expiration details
   - The data is protected with digital signatures to prevent tampering

2. **Offline Validation**:
   - When the application cannot connect to the SLASCONE server, it falls back to the cached license data
   - The application verifies the digital signature of the cached data to ensure integrity
   - All license rules (features, limitations, expiration) continue to be enforced based on cached data

3. **Implementation**:
   - The sample stores license data in `license.json` and its signature in `license_signature.txt`
   - For floating licenses, session information is stored in `session.token`
   - The sample demonstrates how to read and validate this information in offline mode

### Freeride Period

Freeride periods provide flexibility when heartbeats fail, allowing users to continue using the software for a specified grace period:

1. **Purpose**:
   - Prevents immediate software lockout when a heartbeat fails
   - Gives users time to resolve connectivity issues
   - Ensures a smoother user experience in environments with intermittent connectivity

2. **Functionality**:
   - If a heartbeat fails, the software continues to work normally during the freeride period
   - The application tracks the time since the last successful heartbeat
   - Once a successful heartbeat occurs, the freeride period is reset
   - If the freeride period expires without a successful heartbeat, license enforcement takes effect

3. **Configuration**:
   - Freeride periods are configured at the license edition level in the SLASCONE portal
   - The freeride duration is specified in days
   - This sample demonstrates how to implement and respect freeride periods
   - The `OfflineLicenseInfoExample` method shows how to display freeride information

4. **Example Scenario**:
   - With a daily heartbeat requirement and a 7-day freeride period
   - If heartbeats fail, the application continues working for 7 days
   - During this time, the application should notify the user about the need to go online
   - If a heartbeat succeeds within those 7 days, normal operation resumes
   - After 7 days without a successful heartbeat, the license becomes invalid

This implementation ensures that temporary network issues or brief periods offline do not disrupt users' work while still maintaining proper license enforcement in the long term.

## Configuration and Storage

### Application Data Folder

The sample application stores license and session files in a dedicated application data folder instead of the current working directory. This provides several benefits:

1. **Persistent Storage**: License and session files remain accessible across application restarts
2. **Centralized Location**: All application data is stored in a single, dedicated location
3. **Security**: Files are stored outside of the application directory, reducing the risk of accidental deletion

#### Default Location

By default, the application data is stored in:
- The current directory of the running app on Linux
- `%ProgramData%\Slascone.Provisioning.Sample.NuGet` on Windows

#### Custom Location

The application data folder is managed by the `SlasconeClientV2` class. To use a custom location for application data, you need to specify it when initializing the client:
_slasconeClientV2.SetAppDataFolder("<custom path>");

#### Stored Files

The following files are managed in the application data folder:

- `license.json`: The cached license information from the last successful heartbeat
- `license_signature.txt`: The digital signature for verifying the license file
- `session.token`: Information about the current floating license session (if applicable); also contains the digital signature

All files are automatically created, updated, and removed as needed during application operation.

