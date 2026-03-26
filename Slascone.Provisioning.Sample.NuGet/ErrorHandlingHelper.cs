using Slascone.Client;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using static Slascone.Provisioning.Sample.NuGet.Services.ErrorHandlingHelper;

namespace Slascone.Provisioning.Sample.NuGet.Services
{
    /// <summary>
    /// Helper class to handle errors and standard retries while calling the SLASCONE API.
    /// </summary>
    internal static class ErrorHandlingHelper
    {
        /// <summary>
        /// Error type enum to differentiate between error categories
        /// </summary>
        public enum ErrorType
        {
            /// <summary>
            /// No error occurred
            /// </summary>
            None,

            /// <summary>
            /// Business logic or validation error (e.g., invalid input, conflict)
            /// </summary>
            Functional,

            /// <summary>
            /// Technical error in the system (e.g., internal server error)
            /// </summary>
            Technical,

            /// <summary>
            /// Network or connectivity issue
            /// </summary>
            Network
        }

        /// <summary>
        /// Wait time between retries (in seconds)
        /// </summary>
        private static readonly int RetryWaitTime = 15;

        /// <summary>
        /// Do max 1 retry
        /// </summary>
        private const int MaxRetryCount = 1;

        /// <summary>
        /// Call a SLASCONE API endpoint with standard retry logic
        /// </summary>
        /// <typeparam name="TIn">Type of input argument</typeparam>
        /// <typeparam name="TOut">Type of result</typeparam>
        /// <param name="func">SLASCONE API endpoint call</param>
        /// <param name="argument">Input argument</param>
        /// <param name="callerMemberName">Caller member name for error message if necessary</param>
        /// <returns></returns>
        public static async Task<(TOut data, ErrorType errorType, ErrorResultObjects error, string message)> Execute<TIn, TOut>(
            Func<TIn, Task<ApiResponse<TOut>>> func,
            TIn argument,
            [CallerMemberName] string callerMemberName = "")
            where TOut : class
        {
            string errorMessage = null;
            ApiResponse<TOut> response = null;

            try
            {
                int retryCountdown = MaxRetryCount;

                while (0 <= retryCountdown)
                {
                    // Call the SLASCONE API endpoint
                    response = await func.Invoke(argument).ConfigureAwait(false);

                    if ((int)HttpStatusCode.OK == response.StatusCode)
                    {
                        // Success
                        return (response.Result, ErrorType.None, null, null);
                    }

                    // Error handling based on status code
                    if ((int)HttpStatusCode.Conflict == response.StatusCode)
                    {
                        // Functional error: Return error message
                        return (null, ErrorType.Functional, response.Error,
                            $"{callerMemberName} received an error: {response.Error.Message} (Id: {response.Error.Id})");
                    }
                    else if ((int)HttpStatusCode.Unauthorized == response.StatusCode
                             || (int)HttpStatusCode.Forbidden == response.StatusCode)
                    {
                        // Unauthorized or forbidden: Return error message
                        return (null, ErrorType.Technical, null,
                            $"{callerMemberName} received an error: Not authorized");
                    }

                    var isTransientStatusCode = IsTransientError(response.StatusCode);
                    var isTransientException = IsTransientError(response.ApiException);

                    if (isTransientStatusCode || isTransientException)
                    {
                        // Transient error: Wait and try again
                        // Get the wait time from the response header or use default
                        --retryCountdown;
                        if (0 <= retryCountdown)
                        {
                            // Get retry-after period from response header or use default
                            int retryAfterSeconds = GetRetryAfterPeriod(response.ApiException);
                            await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds)).ConfigureAwait(false);
                            continue;
                        }
                    }

                    var errorType = null != response.ApiException?.InnerException && typeof(HttpRequestException) ==
                        response.ApiException?.InnerException.GetType()
                            ? ErrorType.Network
                            : ErrorType.Technical;

                    errorMessage = null != response.ApiException
                        ? $"{callerMemberName} received an error: {response.ApiException.Message}"
                        : 0 < response.StatusCode
                            ? $"{callerMemberName} received an error (status code: {response.StatusCode}; message: '{response.Message}')"
                            : $"{callerMemberName} received an error (message: '{response.Message}')";

                    return (null, errorType, null, errorMessage);
                }

                return (null, ErrorType.Technical, null, null);
            }
            catch (Exception ex)
            {
                return (response?.Result, ErrorType.Technical, response?.Error, errorMessage);
            }
        }

        private static bool IsTransientError(ApiException exception)
        {
            if (exception?.InnerException != null && typeof(HttpRequestException) == exception.InnerException.GetType())
            {
                return true;
            }
            if (exception != null)
            {
                return IsTransientError(exception.StatusCode);
            }
            return false;
        }

        private static bool IsTransientError(int httpStatusCode)
        {
            return httpStatusCode == 408 || // Request Timeout
                   httpStatusCode == 429 || // Too Many Requests
                   httpStatusCode == 500 || // Internal Server Error
                   httpStatusCode == 502 || // Bad Gateway
                   httpStatusCode == 503 || // Service Unavailable
                   httpStatusCode == 504 || // Gateway Timeout
                   httpStatusCode == 507; // Insufficient Storage
        }

        private static int GetRetryAfterPeriod(ApiException apiException)
        {
            if (apiException?.Headers?.TryGetValue("Retry-After", out var retryAfterValues) ?? false)
            {
                var retryAfterValue = retryAfterValues.FirstOrDefault();
                if (int.TryParse(retryAfterValue, out var retryAfterSeconds))
                {
                    return Math.Clamp(retryAfterSeconds, 5, 120);
                }
            }

            return RetryWaitTime;
        }
    }
}
