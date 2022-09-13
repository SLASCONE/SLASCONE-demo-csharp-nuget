using Slascone.Client;

namespace Slascone.Provisioning.Sample.NuGet;

public interface IUseCaseActivateLicense
{
    /// <summary>
    /// Activates a License
    /// </summary>
    /// <returns>ProvisioningInfo where LicenseInfoDto or WarningInfoDto is set.</returns>
    Task<ApiResponse<LicenseInfoDto>> ActivateLicenseAsync(ActivateClientDto activateClientDto);

    /// <summary>
    /// Unassign a activated license.
    /// </summary>
    /// <returns>"Successfully deactivated License." or a WarningInfoDto</returns>
    Task<ApiResponse<string>> UnassignAsync(UnassignDto unassignDto);
}

public class UseCaseActivateLicense : IUseCaseActivateLicense
{
    private readonly ISlasconeClientV2 _slasconeClientV2;

    public UseCaseActivateLicense()
    {
        _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
            Helper.IsvId,
            Helper.ProvisioningKey,
            Helper.SignatureValidationMode,
            Helper.SymmetricEncryptionKey,
            Helper.Certificate);
    }

    /// <summary>
    /// Activates a License
    /// </summary>
    /// <returns>ProvisioningInfo where LicenseInfoDto or WarningInfoDto is set.</returns>
    public async Task<ApiResponse<LicenseInfoDto>> ActivateLicenseAsync(ActivateClientDto activateClientDto)
    {
        var  response = new ApiResponse<LicenseInfoDto>();
        try
        {
            response.Result = await _slasconeClientV2.ActivateLicenseAsync(activateClientDto);
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
    /// Unassign a activated license.
    /// </summary>
    /// <returns>"Successfully deactivated License." or a WarningInfoDto</returns>
    public async Task<ApiResponse<string>> UnassignAsync(UnassignDto unassignDto)
    {
        var response = new ApiResponse<string>();
        try
        {
            response.Result = await _slasconeClientV2.UnassignLicenseAsync(unassignDto);        
        }
        catch (ApiException<DeactivateDeviceLicenseResponseError> ex)
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
}