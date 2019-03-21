using System.Threading;
using System.Threading.Tasks;
using AdvertApi.Services;
using Microsoft.Extensions.HealthChecks;

namespace AdvertApi.HealthChecks
{
    public class StorageHealthCheck: IHealthCheck
    {
        private readonly IAdvertStorageService _storageService;

        public StorageHealthCheck(IAdvertStorageService storageService)
        {
            _storageService = storageService;
        }

        public async ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var isStorageOk = await _storageService.CheckHealthAsync();

            var checkStatus = isStorageOk ? CheckStatus.Healthy : CheckStatus.Unhealthy;

            return HealthCheckResult.FromStatus(checkStatus, "");
        }
    }
}
