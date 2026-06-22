using Slascone.Client;
using System.Text;
using System.Text.RegularExpressions;
using Slascone.Client.Interfaces;

namespace Slascone.Provisioning.Sample.NuGet
{
    internal class ValidityCheck
    {
        private static string dateFormat = "yyyy-MM-dd HH:mm";

        internal static bool CheckValidity(LicenseDto license)
        {
            // Start date and expiration date information
            Console.WriteLine("\nLicense Validity Status:");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"\n===> License is {(license.Is_valid ? "valid" : "not valid")} <===\n");

            // Date information and license validity
            if (license.Is_valid && DateValidity.IsValid == license.Date_validity)
            {
                // Check if it's a "9999" perpetual license
                if (license.Expiration_date_utc.Value.Year >= 9999)
                {
                    Console.WriteLine("This is a perpetual license.");
                }
                else
                {
                    long valid = (license.Expiration_date_utc.Value - DateTime.UtcNow).Days;
                    Console.WriteLine($"License is valid for another {valid} day(s) until {license.Expiration_date_utc.Value.ToString(dateFormat)}.");
                }
            }
            else
            {
                switch (license.Date_validity)
                {
                    case DateValidity.IsNotValidYet:
                        Console.WriteLine(
                            $"License is not valid yet.{(license.Start_date_utc.HasValue ? $" (Start Date: {license.Start_date_utc.Value.ToString(dateFormat)}" : "")}");
                        break;

                    case DateValidity.IsExpired:
                        Console.WriteLine(license.Expiration_date_utc.HasValue
                            ? $"License has expired.since {license.Expiration_date_utc.Value.ToString(dateFormat)}."
                            : "License has expired.");
                        break;
                }
            }

            if (!license.Is_valid && !license.Is_active)
            {
                Console.WriteLine("License is deactivated.");
            }

            // Software version information
            var isSoftwareVersionCompliant = true;
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
                    if (!string.IsNullOrEmpty(swLimitation.Description))
                    {
                        Console.WriteLine($"Description: {swLimitation.Description}");
                    }
                }

                if (!string.IsNullOrEmpty(license.Prioritized_software_release))
                {
                    isSoftwareVersionCompliant = Compare(swLimitation.Software_release, license.Prioritized_software_release) <= 0;
                }

                if (isSoftwareVersionCompliant)
                    Console.WriteLine("Software version is compliant.");
                else
                    Console.WriteLine($"\n===> Software version is not compliant. <===\n");
            }

            return license.Is_valid && isSoftwareVersionCompliant;
        }

        internal static bool CheckValidity(LicenseInfoDto licenseInfo)
        {
            // License validity status
            Console.WriteLine("\nLicense Validity Status:");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"\n===> License is {(licenseInfo.Is_license_valid ? "valid" : "not valid")} <===\n");

            // Date information and license validity
            if (licenseInfo.Is_license_valid && DateValidity.IsValid == licenseInfo.Date_validity)
            {
                // Check if it's a "9999" perpetual license
                if (licenseInfo.Expiration_date_utc.Value.Year >= 9999)
                {
                    Console.WriteLine("This is a perpetual license.");
                }
                else
                {
                    long valid = (licenseInfo.Expiration_date_utc.Value - DateTime.UtcNow).Days;
                    Console.WriteLine($"License is valid for another {valid} day(s) until {licenseInfo.Expiration_date_utc.Value.ToString(dateFormat)}.");
                }
            }
            else
            {
                switch (licenseInfo.Date_validity)
                {
                    case DateValidity.IsNotValidYet:
                        Console.WriteLine(
                            $"License is not valid yet.{(licenseInfo.Start_date_utc.HasValue ? $" (Start Date: {licenseInfo.Start_date_utc.Value.ToString(dateFormat)}" : "")}");
                        break;

                    case DateValidity.IsExpired:
                        Console.WriteLine(licenseInfo.Expiration_date_utc.HasValue
                            ? $"License has expired.since {licenseInfo.Expiration_date_utc.Value.ToString(dateFormat)}."
                            : "License has expired.");
                        break;
                }
            }

            if (!licenseInfo.Is_license_valid && !licenseInfo.Is_license_active)
            {
                Console.WriteLine("License is deactivated.");
            }

            // Software version information
            var swLimitation = licenseInfo.Software_release_limitation;
            if (swLimitation != null)
            {
                Console.WriteLine("\nSoftware Version Information:");
                Console.WriteLine("----------------------------");
                if (licenseInfo.Is_software_version_valid)
                    Console.WriteLine("Software version is compliant");
                else
                    Console.WriteLine("\n===> Software version is not compliant <===\n");
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

            return licenseInfo.Is_license_valid && licenseInfo.Is_software_version_valid;
        }

        private static int Compare(string releaseLimitation, string softwareVersion)
        {
            var releaseLimitationParts = releaseLimitation.Split('.');
            var softwareVersionParts = Regex.Replace(softwareVersion, "[a-z-A-Z]", "").Split('.');

            if (softwareVersionParts.Length < releaseLimitationParts.Length)
            {
                // softwareVersion must have at least as many parts as releaseLimitation
                throw new ArgumentException("Release has too few parts.");
            }

            for (int i = 0; i < releaseLimitationParts.Length; i++)
            {
                var limitationPart = releaseLimitationParts[i];
                var versionPart = softwareVersionParts[i];

                if (int.TryParse(limitationPart, out var limitationPartInt) &&
                    int.TryParse(versionPart, out var versionPartInt))
                {
                    if (limitationPartInt < versionPartInt)
                    {
                        return -1;
                    }

                    if (limitationPartInt > versionPartInt)
                    {
                        return 1;
                    }
                }
                else
                {
                    throw new ArgumentException("Version format error.");
                }
            }

            // If softwareVersion has more parts than releaseLimitation, the rest is ignored and both are considered equal
            return 0;
        }

    }
}
