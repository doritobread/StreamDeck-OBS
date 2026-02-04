using Microsoft.Extensions.Options;
using OBSWebsocketDotNet;
using StreamAp.Models;

namespace StreamAp.Services;

public class ObsService : IObsService, IDisposable
{
    private readonly OBSWebsocket _obs;
    private readonly ObsSettings _settings;
    private readonly ILogger<ObsService> _logger;

    public bool IsConnected => _obs.IsConnected;

    public ObsService(IOptions<ObsSettings> settings, ILogger<ObsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _obs = new OBSWebsocket();

        _obs.Connected += (sender, e) => _logger.LogInformation("Connected to OBS");
        _obs.Disconnected += (sender, e) => _logger.LogInformation("Disconnected from OBS");
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

            // Timeout after 10 seconds
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
                    audioSources.Add(new AudioSourceDto
                    {
                        Name = input.InputName,
                        IsMuted = muted
                    });
                }
                catch
                {
                    // Not all inputs have mute capability, skip them
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

    public void Dispose()
    {
        Disconnect();
    }
}
