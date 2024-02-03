using Microsoft.Extensions.Logging;
using Polly;
using System;

namespace Converter.Api.Service.Services
{
    public class RetryPolicyExecutor
    {
        private static readonly ILogger<RetryPolicyExecutor> _logger;

        static RetryPolicyExecutor()
        {
            _logger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            }).CreateLogger<RetryPolicyExecutor>();
        }

        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, int maxRetries)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .RetryAsync(maxRetries, (exception, retryCount) =>
                {
                    _logger.LogWarning($"Retry #{retryCount} due to exception: {exception.Message}");
                });

            return await retryPolicy.ExecuteAsync(async () => await action());
        }

        public static async Task ExecuteAsync(Func<Task> action, int maxRetries)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .RetryAsync(maxRetries, (exception, retryCount) =>
                {
                    _logger.LogWarning($"Retry #{retryCount} due to exception: {exception.Message}");
                });

            await retryPolicy.ExecuteAsync(async () => await action());
        }
    }
}