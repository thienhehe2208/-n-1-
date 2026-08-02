namespace bài_tập_1.Services
{
    public class LibraryMaintenanceService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LibraryMaintenanceService> _logger;

        public LibraryMaintenanceService(
            IServiceScopeFactory scopeFactory,
            ILogger<LibraryMaintenanceService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var phieuMuonService = scope.ServiceProvider
                        .GetRequiredService<PhieuMuonService>();
                    var datTruocService = scope.ServiceProvider
                        .GetRequiredService<DatTruocService>();
                    var muonOnlineService = scope.ServiceProvider
                        .GetRequiredService<MuonOnlineService>();

                    await phieuMuonService.CapNhatTrangThaiAsync();
                    await datTruocService.XuLyHetHanAsync();
                    await muonOnlineService.XuLyHetHanAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Không thể đồng bộ trạng thái nghiệp vụ thư viện.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
