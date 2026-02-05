using Microsoft.Extensions.Options;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using StreamAp.Models;

namespace StreamAp.Services;

public class ObsService : IObsService, IDisposable
{
    private readonly OBSWebsocket _obs;
    private readonly ObsSettings _settings;
    private readonly ILogger<ObsService> _logger;
    private string? _lastReplaySavedPath;

    public bool IsConnected => _obs.IsConnected;

    public ObsService(IOptions<ObsSettings> settings, ILogger<ObsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _obs = new OBSWebsocket();

        _obs.Connected += (sender, e) => _logger.LogInformation("Connected to OBS");
        _obs.Disconnected += (sender, e) => _logger.LogInformation("Disconnected from OBS");
        _obs.ReplayBufferSaved += (sender, e) =>
        {
            _lastReplaySavedPath = e.SavedReplayPath;
            _logger.LogInformation("Replay saved: {Path}", e.SavedReplayPath);
        };
    }

    public Task<bool> ConnectAsync()
    {
        if (_obs.IsConnected)
            return Task.FromResult(true);

        var tcs = new TaskCompletionSource<bool>();

        void onConnected(object? sender, EventArgs e)
        {
            _obs.Connected -= onConnected;
            tcs.TrySetResult(true);
        }

        void onDisconnected(object? sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
        {
            _obs.Disconnected -= onDisconnected;
            tcs.TrySetResult(false);
        }

        try
        {
            var url = $"ws://{_settings.Host}:{_settings.Port}";
            _logger.LogInformation("Connecting to OBS at {Url}", url);

            _obs.Connected += onConnected;
            _obs.Disconnected += onDisconnected;
            _obs.ConnectAsync(url, _settings.Password ?? "");

            Task.Delay(10000).ContinueWith(_ =>
            {
                _obs.Connected -= onConnected;
                _obs.Disconnected -= onDisconnected;
                tcs.TrySetResult(false);
            });

            return tcs.Task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to OBS");
            return Task.FromResult(false);
        }
    }

    public void Disconnect()
    {
        if (_obs.IsConnected)
        {
            _obs.Disconnect();
        }
    }

    public async Task<ObsStatusDto> GetStatusAsync()
    {
        var status = new ObsStatusDto
        {
            IsConnected = _obs.IsConnected
        };

        if (!_obs.IsConnected)
            return status;

        try
        {
            status.Scenes = await GetScenesAsync();
            status.CurrentScene = status.Scenes.FirstOrDefault(s => s.IsActive)?.Name;
            status.IsStreaming = await GetStreamingStatusAsync();
            status.IsRecording = await GetRecordingStatusAsync();
            status.AudioSources = await GetAudioSourcesAsync();
            status.Stats = await GetStatsAsync();
            status.ReplayBuffer = await GetReplayBufferStatusAsync();

            if (!string.IsNullOrEmpty(status.CurrentScene))
            {
                status.SceneItems = await GetSceneItemsAsync(status.CurrentScene);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OBS status");
        }

        return status;
    }

    public Task<List<SceneDto>> GetScenesAsync()
    {
        if (!_obs.IsConnected)
            return Task.FromResult(new List<SceneDto>());

        try
        {
            var sceneList = _obs.GetSceneList();
            var scenes = sceneList.Scenes
                .Select(s => new SceneDto
                {
                    Name = s.Name,
                    IsActive = s.Name == sceneList.CurrentProgramSceneName
                })
                .ToList();

            return Task.FromResult(scenes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scenes");
            return Task.FromResult(new List<SceneDto>());
        }
    }

    public Task SetSceneAsync(string sceneName)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.SetCurrentProgramScene(sceneName);
            _logger.LogInformation("Switched to scene: {Scene}", sceneName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching to scene {Scene}", sceneName);
        }

        return Task.CompletedTask;
    }

    public Task<bool> GetStreamingStatusAsync()
    {
        if (!_obs.IsConnected)
            return Task.FromResult(false);

        try
        {
            var status = _obs.GetStreamStatus();
            return Task.FromResult(status.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stream status");
            return Task.FromResult(false);
        }
    }

    public Task ToggleStreamAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.ToggleStream();
            _logger.LogInformation("Toggled stream");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling stream");
        }

        return Task.CompletedTask;
    }

    public Task StartStreamAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.StartStream();
            _logger.LogInformation("Started stream");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stream");
        }

        return Task.CompletedTask;
    }

    public Task StopStreamAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.StopStream();
            _logger.LogInformation("Stopped stream");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping stream");
        }

        return Task.CompletedTask;
    }

    public Task<bool> GetRecordingStatusAsync()
    {
        if (!_obs.IsConnected)
            return Task.FromResult(false);

        try
        {
            var status = _obs.GetRecordStatus();
            return Task.FromResult(status.IsRecording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting record status");
            return Task.FromResult(false);
        }
    }

    public Task ToggleRecordAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.ToggleRecord();
            _logger.LogInformation("Toggled recording");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling recording");
        }

        return Task.CompletedTask;
    }

    public Task StartRecordAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.StartRecord();
            _logger.LogInformation("Started recording");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording");
        }

        return Task.CompletedTask;
    }

    public Task StopRecordAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.StopRecord();
            _logger.LogInformation("Stopped recording");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording");
        }

        return Task.CompletedTask;
    }

    public Task<List<AudioSourceDto>> GetAudioSourcesAsync()
    {
        if (!_obs.IsConnected)
            return Task.FromResult(new List<AudioSourceDto>());

        try
        {
            var inputs = _obs.GetInputList();
            var audioSources = new List<AudioSourceDto>();

            foreach (var input in inputs)
            {
                try
                {
                    var muted = _obs.GetInputMute(input.InputName);
                    var volume = _obs.GetInputVolume(input.InputName);
                    audioSources.Add(new AudioSourceDto
                    {
                        Name = input.InputName,
                        IsMuted = muted,
                        Volume = (float)volume.VolumeMul
                    });
                }
                catch
                {
                    // Not all inputs have mute/volume capability, skip them
                }
            }

            return Task.FromResult(audioSources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audio sources");
            return Task.FromResult(new List<AudioSourceDto>());
        }
    }

    public Task ToggleMuteAsync(string sourceName)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.ToggleInputMute(sourceName);
            _logger.LogInformation("Toggled mute for: {Source}", sourceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling mute for {Source}", sourceName);
        }

        return Task.CompletedTask;
    }

    public Task SetVolumeAsync(string sourceName, float volume)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.SetInputVolume(sourceName, volume, true);
            _logger.LogInformation("Set volume for {Source} to {Volume}", sourceName, volume);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting volume for {Source}", sourceName);
        }

        return Task.CompletedTask;
    }

    public Task<List<SceneItemDto>> GetSceneItemsAsync(string sceneName)
    {
        if (!_obs.IsConnected)
            return Task.FromResult(new List<SceneItemDto>());

        try
        {
            var items = _obs.GetSceneItemList(sceneName);
            var sceneItems = items.Select(item => new SceneItemDto
            {
                Id = item.ItemId,
                Name = item.SourceName,
                SceneName = sceneName,
                IsVisible = _obs.GetSceneItemEnabled(sceneName, item.ItemId),
                SourceType = item.SourceType.ToString()
            }).ToList();

            return Task.FromResult(sceneItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scene items for {Scene}", sceneName);
            return Task.FromResult(new List<SceneItemDto>());
        }
    }

    public Task ToggleSceneItemVisibilityAsync(string sceneName, int sceneItemId)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            var items = _obs.GetSceneItemList(sceneName);
            var item = items.FirstOrDefault(i => i.ItemId == sceneItemId);
            if (item != null)
            {
                var currentVisibility = _obs.GetSceneItemEnabled(sceneName, item.ItemId);
                _obs.SetSceneItemEnabled(sceneName, sceneItemId, !currentVisibility);
                _logger.LogInformation("Toggled visibility for item {Id} in {Scene}", sceneItemId, sceneName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling scene item visibility");
        }

        return Task.CompletedTask;
    }

    public Task SetSceneItemVisibilityAsync(string sceneName, int sceneItemId, bool visible)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.SetSceneItemEnabled(sceneName, sceneItemId, visible);
            _logger.LogInformation("Set visibility for item {Id} in {Scene} to {Visible}", sceneItemId, sceneName, visible);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting scene item visibility");
        }

        return Task.CompletedTask;
    }

    public Task<ReplayBufferStatusDto> GetReplayBufferStatusAsync()
    {
        if (!_obs.IsConnected)
            return Task.FromResult(new ReplayBufferStatusDto());

        try
        {
            var status = _obs.GetReplayBufferStatus();
            return Task.FromResult(new ReplayBufferStatusDto
            {
                IsActive = status,
                LastSavedPath = _lastReplaySavedPath
            });
        }
        catch
        {
            // Replay buffer might not be configured
            return Task.FromResult(new ReplayBufferStatusDto());
        }
    }

    public Task ToggleReplayBufferAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.ToggleReplayBuffer();
            _logger.LogInformation("Toggled replay buffer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling replay buffer");
        }

        return Task.CompletedTask;
    }

    public Task SaveReplayBufferAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.SaveReplayBuffer();
            _logger.LogInformation("Saved replay buffer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving replay buffer");
        }

        return Task.CompletedTask;
    }

    public Task StartReplayBufferAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.StartReplayBuffer();
            _logger.LogInformation("Started replay buffer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting replay buffer");
        }

        return Task.CompletedTask;
    }

    public Task StopReplayBufferAsync()
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.StopReplayBuffer();
            _logger.LogInformation("Stopped replay buffer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping replay buffer");
        }

        return Task.CompletedTask;
    }

    public Task<StreamStatsDto> GetStatsAsync()
    {
        if (!_obs.IsConnected)
            return Task.FromResult(new StreamStatsDto());

        try
        {
            var stats = _obs.GetStats();
            var result = new StreamStatsDto
            {
                CpuUsage = stats.CpuUsage,
                MemoryUsage = stats.MemoryUsage,
                AvailableDiskSpace = stats.FreeDiskSpace,
                ActiveFps = stats.FPS,
                AverageFrameRenderTime = stats.AverageFrameTime,
                RenderSkippedFrames = stats.RenderMissedFrames,
                RenderTotalFrames = stats.RenderTotalFrames,
                OutputSkippedFrames = stats.OutputSkippedFrames,
                OutputTotalFrames = stats.OutputTotalFrames
            };

            try
            {
                var streamStatus = _obs.GetStreamStatus();
                if (streamStatus.IsActive)
                {
                    result.StreamTimecode = streamStatus.TimeCode;
                    result.StreamBytes = streamStatus.BytesSent;
                    result.StreamKbitsPerSec = streamStatus.BytesSent > 0 ? (streamStatus.BytesSent * 8) / 1000 : 0;
                }
            }
            catch { }

            try
            {
                var recordStatus = _obs.GetRecordStatus();
                if (recordStatus.IsRecording)
                {
                    result.RecordTimecode = recordStatus.RecordTimecode;
                    result.RecordBytes = recordStatus.RecordingBytes;
                }
            }
            catch { }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats");
            return Task.FromResult(new StreamStatsDto());
        }
    }

    public Task<List<SourceFilterDto>> GetSourceFiltersAsync(string sourceName)
    {
        if (!_obs.IsConnected)
            return Task.FromResult(new List<SourceFilterDto>());

        try
        {
            var filters = _obs.GetSourceFilterList(sourceName);
            var result = filters.Select(f => new SourceFilterDto
            {
                FilterName = f.Name,
                SourceName = sourceName,
                FilterKind = f.Kind,
                IsEnabled = f.IsEnabled
            }).ToList();

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filters for {Source}", sourceName);
            return Task.FromResult(new List<SourceFilterDto>());
        }
    }

    public Task ToggleSourceFilterAsync(string sourceName, string filterName)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            var filters = _obs.GetSourceFilterList(sourceName);
            var filter = filters.FirstOrDefault(f => f.Name == filterName);
            if (filter != null)
            {
                _obs.SetSourceFilterEnabled(sourceName, filterName, !filter.IsEnabled);
                _logger.LogInformation("Toggled filter {Filter} on {Source}", filterName, sourceName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling filter");
        }

        return Task.CompletedTask;
    }

    public Task SetSourceFilterEnabledAsync(string sourceName, string filterName, bool enabled)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.SetSourceFilterEnabled(sourceName, filterName, enabled);
            _logger.LogInformation("Set filter {Filter} on {Source} to {Enabled}", filterName, sourceName, enabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting filter enabled");
        }

        return Task.CompletedTask;
    }

    public Task<List<HotkeyDto>> GetHotkeyListAsync()
    {
        if (!_obs.IsConnected)
            return Task.FromResult(new List<HotkeyDto>());

        try
        {
            var hotkeys = _obs.GetHotkeyList();
            return Task.FromResult(hotkeys.Select(h => new HotkeyDto { Name = h }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hotkey list");
            return Task.FromResult(new List<HotkeyDto>());
        }
    }

    public Task TriggerHotkeyByNameAsync(string hotkeyName)
    {
        if (!_obs.IsConnected)
            return Task.CompletedTask;

        try
        {
            _obs.TriggerHotkeyByName(hotkeyName);
            _logger.LogInformation("Triggered hotkey: {Hotkey}", hotkeyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering hotkey {Hotkey}", hotkeyName);
        }

        return Task.CompletedTask;
    }

    public async Task ExecuteMacroAsync(List<MacroAction> actions)
    {
        foreach (var action in actions)
        {
            try
            {
                switch (action.Type)
                {
                    case MacroActionType.SwitchScene:
                        if (!string.IsNullOrEmpty(action.Target))
                            await SetSceneAsync(action.Target);
                        break;

                    case MacroActionType.ToggleSource:
                        if (!string.IsNullOrEmpty(action.Target) && !string.IsNullOrEmpty(action.Value))
                        {
                            var sceneName = action.Target;
                            if (int.TryParse(action.Value, out var itemId))
                                await ToggleSceneItemVisibilityAsync(sceneName, itemId);
                        }
                        break;

                    case MacroActionType.ToggleMute:
                        if (!string.IsNullOrEmpty(action.Target))
                            await ToggleMuteAsync(action.Target);
                        break;

                    case MacroActionType.SetVolume:
                        if (!string.IsNullOrEmpty(action.Target) && float.TryParse(action.Value, out var volume))
                            await SetVolumeAsync(action.Target, volume);
                        break;

                    case MacroActionType.StartStream:
                        await StartStreamAsync();
                        break;

                    case MacroActionType.StopStream:
                        await StopStreamAsync();
                        break;

                    case MacroActionType.ToggleStream:
                        await ToggleStreamAsync();
                        break;

                    case MacroActionType.StartRecord:
                        await StartRecordAsync();
                        break;

                    case MacroActionType.StopRecord:
                        await StopRecordAsync();
                        break;

                    case MacroActionType.ToggleRecord:
                        await ToggleRecordAsync();
                        break;

                    case MacroActionType.SaveReplay:
                        await SaveReplayBufferAsync();
                        break;

                    case MacroActionType.ToggleFilter:
                        if (!string.IsNullOrEmpty(action.Target) && !string.IsNullOrEmpty(action.Value))
                            await ToggleSourceFilterAsync(action.Target, action.Value);
                        break;

                    case MacroActionType.TriggerHotkey:
                        if (!string.IsNullOrEmpty(action.Target))
                            await TriggerHotkeyByNameAsync(action.Target);
                        break;

                    case MacroActionType.Delay:
                        if (action.DelayMs > 0)
                            await Task.Delay(Math.Min(action.DelayMs, 10000)); // Max 10 second delay
                        break;

                    case MacroActionType.PlaySound:
                        // Sound playback would be handled client-side
                        break;
                }

                // Apply any delay after the action
                if (action.Type != MacroActionType.Delay && action.DelayMs > 0)
                {
                    await Task.Delay(Math.Min(action.DelayMs, 10000));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing macro action {Type}", action.Type);
            }
        }
    }

    public void Dispose()
    {
        Disconnect();
    }
}
