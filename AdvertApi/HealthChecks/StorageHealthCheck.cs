using System.Threading;
using System.Threading.Tasks;
using AdvertApi.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdvertApi.HealthChecks
{
    public class StorageHealthCheck: IHealthCheck
    {
        private readonly IAdvertStorageService _storageService;

        public StorageHealthCheck(IAdvertStorageService storageService)
        {
            _storageService = storageService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var isStorageOk = _storageService.CheckHealthAsync();

            if(isStorageOk)
                return Task.FromResult(HealthCheckResult.Healthy());

            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
}
