using Slascone.Client;

namespace Slascone.Provisioning.Sample.NuGet;

public interface IUseCaseFloatingLicense
{
    /// <summary>
    /// Opens a session
    /// </summary>
    /// <param name="sessionDto">Is the object which contains all information to open a session.</param>
    /// <returns>SessionInfo</returns>
    Task<ApiResponse<SessionStatusDto>> OpenSessionAsync(SessionRequestDto sessionDto);

    /// <summary>
    /// Opens a session
    /// </summary>
    /// <param name="sessionDto">Is the object which contains all information to close a session.</param>
    /// <returns>"Success." or a WarningInfoDto</returns>
    Task<ApiResponse<string>> CloseSessionAsync(SessionRequestDto sessionDto);

    /// <summary>
    /// Get the license info
    /// </summary>
    /// <returns>LicenseInfo</returns>
    Task<ApiResponse<LicenseInfoDto>> GetLicenseInfo(ValidateLicenseDto validateLicenseDto);
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
    public async Task<ApiResponse<SessionStatusDto>> OpenSessionAsync(SessionRequestDto sessionDto)
    {
        var response = new ApiResponse<SessionStatusDto>();
        try
        {
            response.Result = await _slasconeClientV2.OpenSessionAsync(sessionDto);     
        }
        catch (ApiException<ActivateLicenseResponseErrors> ex)
        {
            response.Errors = ex.Result.Errors;
            response.StatusCode = ex.StatusCode.ToString();
            response.Message = ex.Message;
        }
        catch (ApiException ex)
        {
            response.StatusCode = ex.StatusCode.ToString();
            response.Message = ex.Message;
        }

        return response;
    }

    /// <summary>
    /// Opens a session
    /// </summary>
    /// <param name="sessionDto">Is the object which contains all information to close a session.</param>
    /// <returns>"Success." or a WarningInfoDto</returns>
    public async Task<ApiResponse<string>> CloseSessionAsync(SessionRequestDto sessionDto)
    {
        var response = new ApiResponse<string>();
        try
        {
            response.Result = await _slasconeClientV2.CloseSessionAsync(sessionDto);         
        }
        catch (ApiException ex)
        {
            response.StatusCode = ex.StatusCode.ToString();
            response.Message = ex.Message;
        }

        return response;
    }

    /// <summary>
    /// Get the license info
    /// </summary>
    /// <returns>LicenseInfo</returns>
    public async Task<ApiResponse<LicenseInfoDto>> GetLicenseInfo(ValidateLicenseDto validateLicenseDto)
    {
        var response = new ApiResponse<LicenseInfoDto>();
        try
        {
            response.Result = await _slasconeClientV2.GetDeviceInfoAsync(validateLicenseDto);           
        }
        catch (ApiException ex)
        {
            response.StatusCode = ex.StatusCode.ToString();
            response.Message = ex.Message;
        }

        return response;
    }
}
