using Slascone.Client;
using Slascone.Client.Xml;

namespace Slascone.Provisioning.Sample.NuGet
{
    internal class LicensePrettyPrinter
    {
        /// <summary>
        /// Prints the details of a LicenseDto object to the console.
        /// Displays license information, customer details, features, limitations, and variables.
        /// </summary>
        /// <param name="license">The license object to print</param>
        public static IDictionary<Guid, string> PrintLicenseDetails(LicenseDto license)
        {
            if (license == null)
            {
                Console.WriteLine("No license information available.");
                return null;
            }

            Console.WriteLine($"License details (Created: {license.Created_date_utc?.ToString("yyyy-MM-dd") ?? "N/A"}):");

            // Display the main properties of the license
            Console.WriteLine("\nLicense Information:");
            Console.WriteLine("-------------------");
            Console.WriteLine($"License ID: {license.Id}");
            Console.WriteLine($"License Name: {license.Name ?? ""}");
            if (!string.IsNullOrEmpty(license.Description))
            {
                Console.WriteLine($"Description: {license.Description}");
            }
            if (!string.IsNullOrEmpty(license.Legacy_license_key))
            {
                Console.WriteLine($"Legacy License Key: {license.Legacy_license_key}");
            }
            Console.WriteLine($"Client ID: {license.Client_id ?? "N/A"}");

            // Token information
            Console.WriteLine("\nToken Information:");
            Console.WriteLine("------------------");
            Console.WriteLine($"Token Limit: {license.Token_limit}");
            if (license.Goodwill_token_limit.HasValue)
            {
                Console.WriteLine($"Goodwill Token Limit: {license.Goodwill_token_limit.Value}");
            }
            if (license.Floating_token_limit.HasValue)
            {
                Console.WriteLine($"Floating Token Limit: {license.Floating_token_limit.Value}");
            }
            if (license.User_limit.HasValue)
            {
                Console.WriteLine($"User Limit: {license.User_limit.Value}");
            }

            // Customer information
            var customer = license.Customer;
            if (customer != null)
            {
                Console.WriteLine("\nCustomer Information:");
                Console.WriteLine("---------------------");
                Console.WriteLine($"Customer ID: {license.Customer_id}"); // Use license.Customer_id instead of customer.Customer_id
                if (!string.IsNullOrEmpty(customer.Company_name))
                {
                    Console.WriteLine($"Company Name: {customer.Company_name}");
                }
                if (!string.IsNullOrEmpty(customer.Customer_number))
                {
                    Console.WriteLine($"Customer Number: {customer.Customer_number}");
                }
            }

            // Product information
            var product = license.Product;
            Console.WriteLine("\nProduct Information:");
            Console.WriteLine("--------------------");
            Console.WriteLine($"Product ID: {license.Product_id}");
            if (product != null && !string.IsNullOrEmpty(product.Name))
            {
                Console.WriteLine($"Product Name: {product.Name}");
            }

            // Template information
            var template = license.Template;
            Console.WriteLine($"Template ID: {license.Template_id}");
            if (template != null && !string.IsNullOrEmpty(template.Name))
            {
                Console.WriteLine($"Template Name: {template.Name}");
                Console.WriteLine($"Provisioning mode / client type: {template.Provisioning_mode.ToString()} / {template.Client_type.ToString()}");
            }

            // License type information
            if (license.License_type_id.HasValue)
            {
                Console.WriteLine($"License Type ID: {license.License_type_id.Value}");
                if (license.License_type != null && !string.IsNullOrEmpty(license.License_type.Name))
                {
                    Console.WriteLine($"License Type: {license.License_type.Name}");
                }
            }

            // License details
            Console.WriteLine("\nLicense Details:");
            Console.WriteLine("----------------");
            Console.WriteLine($"Is Temporary: {license.Is_temporary}");

            // Date information and license validity
            string dateFormat = "yyyy-MM-dd HH:mm";
            Console.WriteLine("\nLicense Dates:");
            Console.WriteLine("--------------");
            if (license.Created_date_utc.HasValue)
            {
                Console.WriteLine($"Created Date: {license.Created_date_utc.Value.ToString(dateFormat)}");
            }
            if (license.Modified_date_utc.HasValue)
            {
                Console.WriteLine($"Modified Date: {license.Modified_date_utc.Value.ToString(dateFormat)}");
                Console.WriteLine($"Last Modified By: {license.Last_modified_by ?? "N/A"}");
            }

            // Expiration information
            Console.WriteLine("\nLicense Validity Status:");
            Console.WriteLine("-----------------------");
            
            bool isExpired = false;
            if (license.Expiration_date_utc.HasValue)
            {
                Console.WriteLine($"Expiration Date: {license.Expiration_date_utc.Value.ToString(dateFormat)}");
                
                // Check if perpetual (year 9999)
                if (license.Expiration_date_utc.Value.Year >= 9999)
                {
                    Console.WriteLine("This is a perpetual license.");
                }
                else
                {
                    // Calculate remaining days
                    long daysRemaining = (license.Expiration_date_utc.Value - DateTime.UtcNow).Days;
                    isExpired = daysRemaining < 0;
                    
                    if (isExpired)
                    {
                        long expiredDays = Math.Abs(daysRemaining);
                        Console.WriteLine($"License is expired since {expiredDays} day(s).");
                    }
                    else
                    {
                        Console.WriteLine($"License is valid for another {daysRemaining} day(s) until {license.Expiration_date_utc.Value.ToString(dateFormat)}.");
                    }
                }
            }
            else
            {
                Console.WriteLine("License has no expiration date.");
            }

            // Software version information
            var swLimitation = license.Software_release_limitation;
            if (swLimitation != null || !string.IsNullOrEmpty(license.Prioritized_software_release))
            {
                Console.WriteLine("\nSoftware Version Information:");
                Console.WriteLine("----------------------------");
                
                if (!string.IsNullOrEmpty(license.Prioritized_software_release))
                {
                    Console.WriteLine($"Prioritized Software Release: {license.Prioritized_software_release}");
                }
                
                if (swLimitation != null)
                {
                    Console.WriteLine($"Software Release Limitation ID: {license.Software_release_limitation_id ?? Guid.Empty}");
                    
                    if (!string.IsNullOrEmpty(swLimitation.Software_release))
                    {
                        Console.WriteLine($"Software Release: {swLimitation.Software_release}");
                    }
                    
                    if (!string.IsNullOrEmpty(swLimitation.Description))
                    {
                        Console.WriteLine($"Description: {swLimitation.Description}");
                    }
                }
            }

            // License Status
            Console.WriteLine($"\nLicense Status: {(isExpired ? "Expired" : "Active")}");

            // Features
            if (license.License_features != null && license.License_features.Count > 0)
            {
                Console.WriteLine("\nFeatures:");
                foreach (var feature in license.License_features)
                {
                    Console.WriteLine($"- {feature.Feature_name ?? ""} (Active: {feature.Is_active})");
                    
                    if (feature.Feature_exceptions != null && 
                        feature.Feature_exceptions.Exceptions != null && 
                        feature.Feature_exceptions.Exceptions.Count > 0)
                    {
                        var currentException = feature.Feature_exceptions.Exceptions
                            .FirstOrDefault(exc => exc.Start_date_utc <= DateTime.Now.Date && 
                                                  DateTime.Now.Date <= exc.End_date_utc);
                        
                        if (currentException != null)
                        {
                            Console.WriteLine($"  Valid until: {currentException.End_date_utc.ToString(dateFormat)}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("\nNo features available in this license.");
            }

            // Limitations
            var limitationMap =
                license.License_limitations?.ToDictionary(
                    l => l.Limitation_id,
                    l => $"{l.Limitation_name ?? ""} (max: {l.Limit})")
                ?? new Dictionary<Guid, string>();

            if (license.License_limitations != null && license.License_limitations.Count > 0)
            {
                Console.WriteLine("\nLimitations:");
                foreach (var limitation in license.License_limitations)
                {
                    Console.WriteLine($"- {limitation.Limitation_name ?? ""}: {limitation.Limit}");
                }
            }
            else
            {
                Console.WriteLine("\nNo limitations available in this license.");
            }

            // Constrained variables
            if (license.License_constrained_variables != null && license.License_constrained_variables.Count > 0)
            {
                Console.WriteLine("\nConstrained Variables:");
                foreach (var variable in license.License_constrained_variables)
                {
                    // We'll try both possibilities for the property name to handle whatever is available
                    string valueStr;
                    try
                    {
                        // Try to access a property that's most likely to exist based on naming conventions
                        var propertyInfo = variable.GetType().GetProperty("Value");
                        valueStr = propertyInfo?.GetValue(variable)?.ToString() ?? "";
                    }
                    catch
                    {
                        // If that fails, just output the variable name without the value
                        valueStr = "N/A";
                    }
                    
                    Console.WriteLine($"- {variable.Variable_name ?? ""}: {valueStr}");
                }
            }
            else
            {
                Console.WriteLine("\nNo constrained variables available in this license.");
            }

            // Variables
            if (license.License_variables != null && license.License_variables.Count > 0)
            {
                Console.WriteLine("\nVariables:");
                foreach (var variable in license.License_variables)
                {
                    Console.WriteLine($"- {variable.Variable_name ?? ""}: {variable.Value ?? ""}");
                }
            }
            else
            {
                Console.WriteLine("\nNo variables available in this license.");
            }

            // User information
            if (license.License_users != null && license.License_users.Count > 0)
            {
                Console.WriteLine("\nLicense Users:");
                Console.WriteLine($"Number of users: {license.License_users.Count}");
            }

            // User groups information
            if (license.License_users_groups != null && license.License_users_groups.Count > 0)
            {
                Console.WriteLine("\nLicense User Groups:");
                Console.WriteLine($"Number of user groups: {license.License_users_groups.Count}");
            }

            // Mail logs information
            if (license.Mail_logs != null && license.Mail_logs.Count > 0)
            {
                Console.WriteLine("\nMail Logs:");
                Console.WriteLine($"Number of mail logs: {license.Mail_logs.Count}");
            }

            return limitationMap;
        }

        /// <summary>
        /// Prints the details of a LicenseXmlDto object to the console.
        /// Displays license information, customer details, features, limitations, and variables.
        /// </summary>
        /// <param name="licenseXml">The license XML object to print</param>
        public static void PrintLicenseDetails(LicenseXmlDto licenseXml)
        {
            if (licenseXml == null)
            {
                Console.WriteLine("No license information available.");
                return;
            }

            // Display the main properties of the license
            Console.WriteLine("\nLicense Information:");
            Console.WriteLine("-------------------");
            Console.WriteLine($"License Name: {licenseXml.LicenseName}");
            Console.WriteLine($"License Key: {licenseXml.LicenseKey}");
            Console.WriteLine($"Legacy License Key: {licenseXml.LegacyLicenseKey}");

            // Customer information
            var customer = licenseXml.Customer;
            if (customer != null)
            {
                Console.WriteLine("\nCustomer Information:");
                Console.WriteLine("---------------------");
                Console.WriteLine($"Customer ID: {customer.CustomerId}");
                if (!string.IsNullOrEmpty(customer.CompanyName))
                {
                    Console.WriteLine($"Company Name: {customer.CompanyName}");
                }
                if (!string.IsNullOrEmpty(customer.CompanyName))
                {
                    Console.WriteLine($"Customer Name: {customer.CompanyName}");
                }
                if (!string.IsNullOrEmpty(customer.CustomerNumber))
                {
                    Console.WriteLine($"Customer Number: {customer.CustomerNumber}");
                }
            }

            // Product information
            Console.WriteLine("\nProduct Information:");
            Console.WriteLine("--------------------");
            Console.WriteLine($"Product Name: {licenseXml.ProductName}");
            Console.WriteLine($"Product ID: {licenseXml.ProductId}");
            Console.WriteLine($"Template Name: {licenseXml.TemplateName}");

            // License details
            Console.WriteLine("\nLicense Details:");
            Console.WriteLine("----------------");
            Console.WriteLine($"Provisioning Mode: {licenseXml.ProvisioningMode}");
            Console.WriteLine($"Is Temporary: {licenseXml.IsTemporary}");

            // Date information and license validity
            string dateFormat = "yyyy-MM-dd";
            Console.WriteLine("\nLicense Validity:");
            Console.WriteLine("-------------------");
            if (licenseXml.ModifiedDateUtc.HasValue)
            {
                Console.WriteLine($"Modified Date: {licenseXml.ModifiedDateUtc.Value.ToString(dateFormat)}");
            }

            bool isExpired = false;
            long daysRemaining = 0;

            if (licenseXml.ExpirationDateUtc.HasValue &&
                licenseXml.ExpirationDateUtc.Value.Year < 9999)
            {
                Console.WriteLine($"Expiration Date: {licenseXml.ExpirationDateUtc.Value.ToString(dateFormat)}");

                // Calculate remaining days
                daysRemaining = (licenseXml.ExpirationDateUtc.Value - DateTime.UtcNow).Days;
                isExpired = daysRemaining < 0;

                if (isExpired)
                {
                    long expiredDays = Math.Abs(daysRemaining);
                    Console.WriteLine($"License is expired since {expiredDays} day(s).");

                    // Check freeride
                    if (licenseXml.FreeRide.HasValue)
                    {
                        int freeRideDays = licenseXml.FreeRide.Value;
                        if (expiredDays < freeRideDays)
                        {
                            Console.WriteLine($"Freeride granted for {freeRideDays} day(s).");
                            Console.WriteLine($"License is still usable during freeride period (expires in {freeRideDays - expiredDays} day(s)).");
                        }
                        else
                        {
                            Console.WriteLine("Freeride period has expired. License is no longer valid.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"License is valid for another {daysRemaining} day(s) until {licenseXml.ExpirationDateUtc.Value.ToString(dateFormat)}.");

                    // Show freeride information
                    if (licenseXml.FreeRide.HasValue)
                    {
                        Console.WriteLine($"Freeride Period: {licenseXml.FreeRide.Value} day(s) after last heartbeat.");
                    }
                }
            }
            else
            {
                Console.WriteLine("License has no expiration date (perpetual license).");
            }

            // License Status
            Console.WriteLine($"\nLicense Status: {(isExpired ? "Expired" : "Active")}");

            // Features with formatted output
            if (licenseXml.Features != null && licenseXml.Features.Count > 0)
            {
                Console.WriteLine("\nFeatures:");
                foreach (var feature in licenseXml.Features)
                {
                    Console.WriteLine($"- {feature.Name} (Active: {feature.IsActive})");
                }
            }
            else
            {
                Console.WriteLine("\nNo features available in this license.");
            }

            // Limitations
            if (licenseXml.Limitations != null && licenseXml.Limitations.Count > 0)
            {
                Console.WriteLine("\nLimitations:");
                foreach (var limitation in licenseXml.Limitations)
                {
                    Console.WriteLine($"- {limitation.Name}: {limitation.Value}");
                }
            }
            else
            {
                Console.WriteLine("\nNo limitations available in this license.");
            }

            // Variables
            if (licenseXml.Variables != null && licenseXml.Variables.Count > 0)
            {
                Console.WriteLine("\nVariables:");
                foreach (var variable in licenseXml.Variables)
                {
                    Console.WriteLine($"- {variable.Name}: {variable.Value}");
                }
            }
            else
            {
                Console.WriteLine("\nNo variables available in this license.");
            }

            Console.WriteLine("\nLicense file successfully read and validated!");
        }

        /// <summary>
        /// Prints the details of a LicenseInfoDto object to the console.
        /// Displays license status, features, limitations, and expiration information.
        /// </summary>
        /// <param name="licenseInfo">The LicenseInfoDto object to print</param>
        /// <returns>A dictionary of limitations for further use in the application</returns>
        public static Dictionary<Guid, string> PrintLicenseDetails(LicenseInfoDto licenseInfo)
        {
            if (licenseInfo == null)
            {
                Console.WriteLine("No license information available.");
                return new Dictionary<Guid, string>();
            }

            Console.WriteLine($"License infos (Retrieved {licenseInfo.Created_date_utc}):");

            // Display the main properties of the license
            Console.WriteLine("\nLicense Information:");
            Console.WriteLine("-------------------");
            Console.WriteLine($"License Name: {licenseInfo.License_name ?? ""}");
            Console.WriteLine($"License Key: {licenseInfo.License_key ?? ""}");

            if (!string.IsNullOrEmpty(licenseInfo.Legacy_license_key))
            {
                Console.WriteLine($"Legacy License Key: {licenseInfo.Legacy_license_key}");
            }

            if (licenseInfo.Token_key.HasValue)
            {
                Console.WriteLine($"Token Key: {licenseInfo.Token_key.Value}");
            }

            // Customer information
            var customer = licenseInfo.Customer;
            if (customer != null)
            {
                Console.WriteLine("\nCustomer Information:");
                Console.WriteLine("---------------------");
                Console.WriteLine($"Customer ID: {customer.Customer_id.ToString() ?? ""}");
                Console.WriteLine($"Company Name: {customer.Company_name ?? ""}");

                if (!string.IsNullOrEmpty(customer.Customer_number))
                {
                    Console.WriteLine($"Customer Number: {customer.Customer_number}");
                }
            }

            // Product information
            Console.WriteLine("\nProduct Information:");
            Console.WriteLine("--------------------");
            Console.WriteLine($"Product Name: {licenseInfo.Product_name ?? ""}");
            Console.WriteLine($"Template Name: {licenseInfo.Template_name ?? ""}");
            Console.WriteLine($"Provisioning mode / client type: {licenseInfo.Provisioning_mode.ToString()} / {licenseInfo.Client_type.ToString()}");

            // License details
            Console.WriteLine("\nLicense Details:");
            Console.WriteLine("----------------");
            Console.WriteLine($"Provisioning Mode: {licenseInfo.Provisioning_mode}");
            Console.WriteLine($"Is Temporary: {licenseInfo.Is_temporary}");
            Console.WriteLine($"Heartbeat Period: {licenseInfo.Heartbeat_period ?? 0} days");

            if (licenseInfo.Session_period.HasValue && licenseInfo.Session_period.Value > 0)
            {
                Console.WriteLine($"Session Period: {licenseInfo.Session_period.Value} days");
            }

            // License validity status
            Console.WriteLine("\nLicense Validity Status:");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"License is {(licenseInfo.Is_license_valid ? "valid" : "not valid")} " +
                              $"(IsActive: {licenseInfo.Is_license_active}; IsExpired: {licenseInfo.Is_license_expired})");

            // Date information and license validity
            string dateFormat = "yyyy-MM-dd HH:mm";
            if (licenseInfo.Created_date_utc.HasValue)
            {
                Console.WriteLine($"Created Date: {licenseInfo.Created_date_utc.Value.ToString(dateFormat)}");
            }

            if (licenseInfo.Expiration_date_utc.HasValue)
            {
                Console.WriteLine($"Expiration Date: {licenseInfo.Expiration_date_utc.Value.ToString(dateFormat)}");

                // Check if it's a "9999" perpetual license
                if (licenseInfo.Expiration_date_utc.Value.Year >= 9999)
                {
                    Console.WriteLine("This is a perpetual license.");
                }
                else if (licenseInfo.Is_license_expired)
                {
                    long expiration = (DateTime.UtcNow - licenseInfo.Expiration_date_utc.Value).Days;
                    Console.WriteLine($"License is expired since {expiration} day(s).");

                    // Check freeride
                    if (licenseInfo.Freeride.HasValue && licenseInfo.Freeride.Value > 0)
                    {
                        if (expiration < licenseInfo.Freeride.Value)
                        {
                            Console.WriteLine($"Freeride granted for {licenseInfo.Freeride.Value} day(s).");
                            Console.WriteLine($"License is still usable during freeride period (expires in {licenseInfo.Freeride.Value - expiration} day(s)).");
                        }
                        else
                        {
                            Console.WriteLine("Freeride period has expired. License is no longer valid.");
                        }
                    }
                }
                else
                {
                    long valid = (licenseInfo.Expiration_date_utc.Value - DateTime.UtcNow).Days;
                    Console.WriteLine($"License is valid for another {valid} day(s) until {licenseInfo.Expiration_date_utc.Value.ToString(dateFormat)}.");

                    // Show freeride information
                    if (licenseInfo.Freeride.HasValue && licenseInfo.Freeride.Value > 0)
                    {
                        Console.WriteLine($"Freeride Period: {licenseInfo.Freeride.Value} day(s) after expiration");
                    }
                }
            }
            else
            {
                Console.WriteLine("License has no expiration date.");
            }

            // Software version information
            var swLimitation = licenseInfo.Software_release_limitation;
            if (swLimitation != null)
            {
                Console.WriteLine("\nSoftware Version Information:");
                Console.WriteLine("----------------------------");
                Console.WriteLine($"Is Software Version Valid: {licenseInfo.Is_software_version_valid}");
                Console.WriteLine($"Enforce Software Upgrade: {licenseInfo.Enforce_software_version_upgrade}");

                if (!string.IsNullOrEmpty(swLimitation.Software_release))
                {
                    Console.WriteLine($"Software Release: {swLimitation.Software_release}");
                }

                if (!string.IsNullOrEmpty(swLimitation.Description))
                {
                    Console.WriteLine($"Description: {swLimitation.Description}");
                }
            }

            // Enumerate features
            if (licenseInfo.Features != null && licenseInfo.Features.Count > 0)
            {
                Console.WriteLine("\nFeatures:");
                foreach (var feature in licenseInfo.Features)
                {
                    Console.WriteLine($"- {feature.Name ?? ""} (Active: {feature.Is_active})");

                    if (feature.Expiration_date_utc.HasValue)
                    {
                        Console.WriteLine($"  Expires: {feature.Expiration_date_utc.Value.ToString(dateFormat)}");
                    }
                }
            }
            else
            {
                Console.WriteLine("\nNo features available in this license.");
            }

            // Enumerate limitations
            if (licenseInfo.Limitations != null && licenseInfo.Limitations.Count > 0)
            {
                Console.WriteLine("\nLimitations:");
                foreach (var limitation in licenseInfo.Limitations)
                {
                    Console.WriteLine($"- {limitation.Name ?? ""}: {limitation.Value}");
                }
            }
            else
            {
                Console.WriteLine("\nNo limitations available in this license.");
            }

            // Enumerate variables if present
            if (licenseInfo.Variables != null && licenseInfo.Variables.Count > 0)
            {
                Console.WriteLine("\nVariables:");
                foreach (var variable in licenseInfo.Variables)
                {
                    Console.WriteLine($"- {variable.Name ?? ""}: {variable.Value ?? ""}");
                }
            }
            else
            {
                Console.WriteLine("\nNo variables available in this license.");
            }

            // User information if present
            if (licenseInfo.License_users != null && licenseInfo.License_users.Count > 0)
            {
                Console.WriteLine("\nLicense Users:");
                Console.WriteLine($"Number of users: {licenseInfo.License_users.Count}");
            }

            Console.WriteLine("\nLicense information successfully validated!");

            // Create a dictionary of limitations similar to the Java implementation
            var limitationMap =
                licenseInfo.Limitations?.ToDictionary(
                    l => l.Id, 
                    l => $"{l.Name} (max: {l.Value})")
                ?? new Dictionary<Guid, string>();

            return limitationMap;
        }
    }
}