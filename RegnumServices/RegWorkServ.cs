using ServiceManager;

namespace RegnumServices
{
	public class RegWorkServ : BackgroundService
	{
		private readonly ILogger<RegWorkServ> _logger;

		public RegWorkServ(ILogger<RegWorkServ> logger)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					//RegnumDataTrans regWork = new RegnumDataTrans();
					//regWork.SyncData();
					//_logger.LogInformation("execution Started");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, ex.Message);
					throw;
				}
				//_logger.LogInformation("Service executed");
				await Task.Delay(1000 * 60, stoppingToken);
			}
		}
		public override Task StartAsync(CancellationToken cancellationToken)
		{
			RegnumDataTransSer regWork = new RegnumDataTransSer();
			//regWork.SchemaBackUp();
			_logger.LogInformation("Service Started");
			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Service stopped");
			return base.StopAsync(cancellationToken);
		}
		
		
	}
}
