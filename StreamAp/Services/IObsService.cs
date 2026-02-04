using StreamAp.Models;

namespace StreamAp.Services;

public interface IObsService
{
    bool IsConnected { get; }

    Task<bool> ConnectAsync();
    void Disconnect();

    Task<ObsStatusDto> GetStatusAsync();
    Task<List<SceneDto>> GetScenesAsync();
    Task SetSceneAsync(string sceneName);

    Task<bool> GetStreamingStatusAsync();
    Task ToggleStreamAsync();

    Task<bool> GetRecordingStatusAsync();
    Task ToggleRecordAsync();

    Task<List<AudioSourceDto>> GetAudioSourcesAsync();
    Task ToggleMuteAsync(string sourceName);
}
