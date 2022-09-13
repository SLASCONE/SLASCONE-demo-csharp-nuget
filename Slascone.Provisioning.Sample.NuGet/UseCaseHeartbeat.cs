using Slascone.Client;

namespace Slascone.Provisioning.Sample.NuGet;

public interface IUseCaseHeartbeat
{
    /// <summary>
    /// Creates a heartbeat Response is either a LicenseInfoDto or a WarningInfoDto
    /// </summary>
    /// <returns>ProvisioningInfo where LicenseInfoDto or WarningInfoDto is set.</returns>
    Task<ApiResponse<LicenseInfoDto>> AddHeartbeatAsync(AddHeartbeatDto heartbeatDto);

    /// <summary>
    /// Creates a analytical heartbeat
    /// </summary>
    /// <param name="analyticalHeartbeat">Is the object which contains all analytical Heartbeat Information.</param>
    /// <returns>"Successfully created analytical heartbeat." or a WarningInfoDto</returns>
    Task<ApiResponse<string>> AddAnalyticalHeartbeatAsync(AnalyticalHeartbeatDto analyticalHeartbeat);

    /// <summary>
    /// Creates a usage heartbeat
    /// </summary>
    /// <param name="usageHeartbeatDto">Is the object which contains all usage Heartbeat Information.</param>
    /// <returns>"Successfully created usage heartbeat." or a WarningInfoDto</returns>
    Task<ApiResponse<string>> AddUsageHeartbeatAsync(FullUsageHeartbeatDto usageHeartbeatDto);

    /// <summary>
    /// Creates a consumption heartbeat
    /// </summary>
    /// <param name="consumptionHeartbeatDtoDto">Is the object which contains all consumption Heartbeat Information.</param>
    /// <returns>"Successfully created consumption heartbeat." or a WarningInfoDto</returns>
    Task<ApiResponse<ICollection<ConsumptionDto>>> AddConsumptionHeartbeatAsync(FullConsumptionHeartbeatDto consumptionHeartbeatDto);

    /// <summary>
    /// Get the consumption status of an limitation per assignment
    /// </summary>
    /// <returns>Remaining Consumption Value</returns>
    Task<ApiResponse<ConsumptionDto>> GetConsumptionStatusAsync(ValidateConsumptionStatusDto validateConsumptionDto);
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
    public async Task<ApiResponse<LicenseInfoDto>> AddHeartbeatAsync(AddHeartbeatDto heartbeatDto)
    {
        var response = new ApiResponse<LicenseInfoDto>();
        try
        {
            response.Result = await _slasconeClientV2.AddHeartbeatAsync(heartbeatDto);       
        }
        catch (ApiException<HeartbeatResponseErrors> ex)
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
    /// Creates a analytical heartbeat
    /// </summary>
    /// <param name="analyticalHeartbeat">Is the object which contains all analytical Heartbeat Information.</param>
    /// <returns>"Successfully created analytical heartbeat." or a WarningInfoDto</returns>
    public async Task<ApiResponse<string>> AddAnalyticalHeartbeatAsync(AnalyticalHeartbeatDto analyticalHeartbeat)
    {
        var response = new ApiResponse<string>();
        try
        {
            response.Result = await _slasconeClientV2.AddAnalyticalHeartbeatAsync(analyticalHeartbeat);
        }
        catch (ApiException<AnalyticalHeartbeatResponseErrors> ex)
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
    /// Creates a usage heartbeat
    /// </summary>
    /// <param name="usageHeartbeatDto">Is the object which contains all usage Heartbeat Information.</param>
    /// <returns>"Successfully created usage heartbeat." or a WarningInfoDto</returns>
    public async Task<ApiResponse<string>> AddUsageHeartbeatAsync(FullUsageHeartbeatDto usageHeartbeatDto)
    {
        var response = new ApiResponse<string>();
        try
        {
            response.Result = await _slasconeClientV2.AddUsageHeartbeatAsync(usageHeartbeatDto);          
        }
        catch (ApiException<UsageHeartbeatResponseErrors> ex)
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
    /// Creates a consumption heartbeat
    /// </summary>
    /// <param name="consumptionHeartbeatDtoDto">Is the object which contains all consumption Heartbeat Information.</param>
    /// <returns>"Successfully created consumption heartbeat." or a WarningInfoDto</returns>
    public async Task<ApiResponse<ICollection<ConsumptionDto>>> AddConsumptionHeartbeatAsync(FullConsumptionHeartbeatDto consumptionHeartbeatDto)
    {
        var response = new ApiResponse<ICollection<ConsumptionDto>>();
        try
        {
            response.Result = await _slasconeClientV2.AddConsumptionHeartbeatAsync(consumptionHeartbeatDto);        
        }
        catch (ApiException ex)
        {
            response.StatusCode = ex.StatusCode.ToString();
            response.Message = ex.Message;
        }

        return response;
    }

    /// <summary>
    /// Get the consumption status of an limitation per assignment
    /// </summary>
    /// <returns>Remaining Consumption Value</returns>
    public async Task<ApiResponse<ConsumptionDto>> GetConsumptionStatusAsync(ValidateConsumptionStatusDto validateConsumptionDto)
    {
        var response = new ApiResponse<ConsumptionDto>();
        try
        {
            response.Result = await _slasconeClientV2.GetConsumptionStatusAsync(validateConsumptionDto);
        }
        catch (ApiException<UsageHeartbeatResponseErrors> ex)
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
