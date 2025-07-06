using FuckPlayersRecorder_ForLOL.Service.Request;

namespace FuckPlayersRecorder_ForLOL.Service.Client;

public interface IClientService
{
    Task MinimizeUxAsync();
    Task ShowUxAsync();
    Task FlashUxAsync();
    Task KillUxAsync();
    Task KillAndRestartUxAsync();
    Task UnloadUxAsync();
    Task LaunchUxAsync();
    Task<double> GetZoomScaleAsync();
    Task SetZoomScaleAsync(double scale);
}
