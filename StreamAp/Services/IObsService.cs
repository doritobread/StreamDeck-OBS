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
    Task StartStreamAsync();
    Task StopStreamAsync();

    Task<bool> GetRecordingStatusAsync();
    Task ToggleRecordAsync();
    Task StartRecordAsync();
    Task StopRecordAsync();

    Task<List<AudioSourceDto>> GetAudioSourcesAsync();
    Task ToggleMuteAsync(string sourceName);
    Task SetVolumeAsync(string sourceName, float volume);

    // Scene items (sources within scenes)
    Task<List<SceneItemDto>> GetSceneItemsAsync(string sceneName);
    Task ToggleSceneItemVisibilityAsync(string sceneName, int sceneItemId);
    Task SetSceneItemVisibilityAsync(string sceneName, int sceneItemId, bool visible);

    // Replay buffer
    Task<ReplayBufferStatusDto> GetReplayBufferStatusAsync();
    Task ToggleReplayBufferAsync();
    Task SaveReplayBufferAsync();
    Task StartReplayBufferAsync();
    Task StopReplayBufferAsync();

    // Stats
    Task<StreamStatsDto> GetStatsAsync();

    // Filters
    Task<List<SourceFilterDto>> GetSourceFiltersAsync(string sourceName);
    Task ToggleSourceFilterAsync(string sourceName, string filterName);
    Task SetSourceFilterEnabledAsync(string sourceName, string filterName, bool enabled);

    // Hotkeys
    Task<List<HotkeyDto>> GetHotkeyListAsync();
    Task TriggerHotkeyByNameAsync(string hotkeyName);

    // Macro execution
    Task ExecuteMacroAsync(List<MacroAction> actions);
}
