namespace Slascone.Provisioning.Sample.NuGet;

public interface IUseCaseHeartbeat
{
    /// <summary>
    /// Creates a heartbeat Response is either a LicenseInfoDto or a WarningInfoDto
    /// </summary>
    /// <returns>ProvisioningInfo where LicenseInfoDto or WarningInfoDto is set.</returns>
    Task<LicenseInfoDto> AddHeartbeatAsync(AddHeartbeatDto heartbeatDto);

    /// <summary>
    /// Creates a analytical heartbeat
    /// </summary>
    /// <param name="analyticalHeartbeat">Is the object which contains all analytical Heartbeat Information.</param>
    /// <returns>"Successfully created analytical heartbeat." or a WarningInfoDto</returns>
    Task<string> AddAnalyticalHeartbeatAsync(AnalyticalHeartbeatDto analyticalHeartbeat);

    /// <summary>
    /// Creates a usage heartbeat
    /// </summary>
    /// <param name="usageHeartbeatDto">Is the object which contains all usage Heartbeat Information.</param>
    /// <returns>"Successfully created usage heartbeat." or a WarningInfoDto</returns>
    Task<string> AddUsageHeartbeatAsync(FullUsageHeartbeatDto usageHeartbeatDto);

    /// <summary>
    /// Creates a consumption heartbeat
    /// </summary>
    /// <param name="consumptionHeartbeatDtoDto">Is the object which contains all consumption Heartbeat Information.</param>
    /// <returns>"Successfully created consumption heartbeat." or a WarningInfoDto</returns>
    Task<ICollection<ConsumptionDto>> AddConsumptionHeartbeatAsync(FullConsumptionHeartbeatDto consumptionHeartbeatDto);

    /// <summary>
    /// Get the consumption status of an limitation per assignment
    /// </summary>
    /// <returns>Remaining Consumption Value</returns>
    Task<ConsumptionDto> GetConsumptionStatusAsync(ValidateConsumptionStatusDto validateConsumptionDto);
}

public class UseCaseHeartbeat : IUseCaseHeartbeat
{
    private readonly ISlasconeClientV2 _slasconeClientV2;

    public UseCaseHeartbeat()
    {
        _slasconeClientV2 = new SlasconeClientV2(Helper.ApiBaseUrl,
            Helper.IsvId,
            Helper.ProvisioningKey,
            Helper.SignatureValidationMode,
            Helper.SymmetricEncryptionKey,
            Helper.Certificate);
    }

    /// <summary>
    /// Creates a heartbeat Response is either a LicenseInfoDto or a WarningInfoDto
    /// </summary>
    /// <returns>ProvisioningInfo where LicenseInfoDto or WarningInfoDto is set.</returns>
    public async Task<LicenseInfoDto> AddHeartbeatAsync(AddHeartbeatDto heartbeatDto)
    {
        try
        {
            var response = await _slasconeClientV2.AddHeartbeatAsync(heartbeatDto);

            return response;
        }
        catch (ApiException<HeartbeatResponseErrors> ex)
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
    /// Creates a analytical heartbeat
    /// </summary>
    /// <param name="analyticalHeartbeat">Is the object which contains all analytical Heartbeat Information.</param>
    /// <returns>"Successfully created analytical heartbeat." or a WarningInfoDto</returns>
    public async Task<string> AddAnalyticalHeartbeatAsync(AnalyticalHeartbeatDto analyticalHeartbeat)
    {
        try
        {
            var response = await _slasconeClientV2.AddAnalyticalHeartbeatAsync(analyticalHeartbeat);

            return response;
        }
        catch (ApiException<AnalyticalHeartbeatResponseErrors> ex)
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
    /// Creates a usage heartbeat
    /// </summary>
    /// <param name="usageHeartbeatDto">Is the object which contains all usage Heartbeat Information.</param>
    /// <returns>"Successfully created usage heartbeat." or a WarningInfoDto</returns>
    public async Task<string> AddUsageHeartbeatAsync(FullUsageHeartbeatDto usageHeartbeatDto)
    {
        try
        {
            var response = await _slasconeClientV2.AddUsageHeartbeatAsync(usageHeartbeatDto);

            return response;
        }
        catch (ApiException<UsageHeartbeatResponseErrors> ex)
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
    /// Creates a consumption heartbeat
    /// </summary>
    /// <param name="consumptionHeartbeatDtoDto">Is the object which contains all consumption Heartbeat Information.</param>
    /// <returns>"Successfully created consumption heartbeat." or a WarningInfoDto</returns>
    public async Task<ICollection<ConsumptionDto>> AddConsumptionHeartbeatAsync(FullConsumptionHeartbeatDto consumptionHeartbeatDto)
    {
        try
        {
            var response = await _slasconeClientV2.AddConsumptionHeartbeatAsync(consumptionHeartbeatDto);

            return response;
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"{ex.StatusCode}: ex.Response");
            throw;
        }
    }

    /// <summary>
    /// Get the consumption status of an limitation per assignment
    /// </summary>
    /// <returns>Remaining Consumption Value</returns>
    public async Task<ConsumptionDto> GetConsumptionStatusAsync(ValidateConsumptionStatusDto validateConsumptionDto)
    {
        try
        {
            var response = await _slasconeClientV2.GetConsumptionStatusAsync(validateConsumptionDto);

            return response;
        }
        catch (ApiException<UsageHeartbeatResponseErrors> ex)
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
