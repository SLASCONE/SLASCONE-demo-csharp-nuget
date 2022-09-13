namespace Slascone.Provisioning.Sample.NuGet;

public interface IUseCaseFloatingLicense
{
    /// <summary>
    /// Opens a session
    /// </summary>
    /// <param name="sessionDto">Is the object which contains all information to open a session.</param>
    /// <returns>SessionInfo</returns>
    Task<SessionStatusDto> OpenSessionAsync(SessionRequestDto sessionDto);

    /// <summary>
    /// Opens a session
    /// </summary>
    /// <param name="sessionDto">Is the object which contains all information to close a session.</param>
    /// <returns>"Success." or a WarningInfoDto</returns>
    Task<string> CloseSessionAsync(SessionRequestDto sessionDto);

    /// <summary>
    /// Get the license info
    /// </summary>
    /// <returns>LicenseInfo</returns>
    Task<LicenseInfoDto> GetLicenseInfo(ValidateLicenseDto validateLicenseDto);
}

public class UseCaseFloatingLicense : IUseCaseFloatingLicense
{
    private readonly ISlasconeClientV2 _slasconeClientV2;

    public UseCaseFloatingLicense()
    {
        _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
            Helper.IsvId,
            Helper.ProvisioningKey,
            Helper.SignatureValidationMode,
            Helper.SymmetricEncryptionKey,
            Helper.Certificate);
    }

    /// <summary>
    /// Opens a session
    /// </summary>
    /// <param name="sessionDto">Is the object which contains all information to open a session.</param>
    /// <returns>SessionInfo</returns>
    public async Task<SessionStatusDto> OpenSessionAsync(SessionRequestDto sessionDto)
    {
        try
        {
            var response = await _slasconeClientV2.OpenSessionAsync(sessionDto);

            return response;
        }
        catch (ApiException<ActivateLicenseResponseErrors> ex)
        {
            if (ex.Result.Errors == null)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine(ex.Result.Errors);
            throw;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"{ex.StatusCode}: ex.Response");
            throw;
        }
    }

    /// <summary>
    /// Opens a session
    /// </summary>
    /// <param name="sessionDto">Is the object which contains all information to close a session.</param>
    /// <returns>"Success." or a WarningInfoDto</returns>
    public async Task<string> CloseSessionAsync(SessionRequestDto sessionDto)
    {
        try
        {
            var response = await _slasconeClientV2.CloseSessionAsync(sessionDto);

            return response;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"{ex.StatusCode}: ex.Response");
            throw;
        }
    }

    /// <summary>
    /// Get the license info
    /// </summary>
    /// <returns>LicenseInfo</returns>
    public async Task<LicenseInfoDto> GetLicenseInfo(ValidateLicenseDto validateLicenseDto)
    {
        try
        {
            var response = await _slasconeClientV2.GetDeviceInfoAsync(validateLicenseDto);

            return response;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"{ex.StatusCode}: ex.Response");
            throw;
        }
    }
}
