namespace Slascone.Provisioning.Sample.NuGet;

public interface IUseCaseActivateLicense
{
    /// <summary>
    /// Activates a License
    /// </summary>
    /// <returns>ProvisioningInfo where LicenseInfoDto or WarningInfoDto is set.</returns>
    Task<LicenseInfoDto> ActivateLicenseAsync(ActivateClientDto activateClientDto);

    /// <summary>
    /// Unassign a activated license.
    /// </summary>
    /// <returns>"Successfully deactivated License." or a WarningInfoDto</returns>
    Task<string> UnassignAsync(UnassignDto unassignDto);
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
    public async Task<LicenseInfoDto> ActivateLicenseAsync(ActivateClientDto activateClientDto)
    {
        try
        {
            var response = await _slasconeClientV2.ActivateLicenseAsync(activateClientDto);

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
    /// Unassign a activated license.
    /// </summary>
    /// <returns>"Successfully deactivated License." or a WarningInfoDto</returns>
    public async Task<string> UnassignAsync(UnassignDto unassignDto)
    {
        try
        {
            var response = await _slasconeClientV2.UnassignLicenseAsync(unassignDto);

            return response;
        }
        catch (ApiException<DeactivateDeviceLicenseResponseError> ex)
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
}