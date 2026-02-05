using Microsoft.AspNetCore.Mvc;
using StreamAp.Models;
using StreamAp.Services;

namespace StreamAp.Controllers;

[ApiController]
[Route("api/obs")]
public class ObsController : ControllerBase
{
    private readonly IObsService _obsService;

    public ObsController(IObsService obsService)
    {
        _obsService = obsService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<ObsStatusDto>> GetStatus()
    {
        var status = await _obsService.GetStatusAsync();
        return Ok(status);
    }

    [HttpPost("connect")]
    public async Task<ActionResult<object>> Connect()
    {
        var success = await _obsService.ConnectAsync();
        return Ok(new { success, isConnected = _obsService.IsConnected });
    }

    [HttpPost("disconnect")]
    public ActionResult<object> Disconnect()
    {
        _obsService.Disconnect();
        return Ok(new { success = true, isConnected = false });
    }

    [HttpGet("scenes")]
    public async Task<ActionResult<List<SceneDto>>> GetScenes()
    {
        var scenes = await _obsService.GetScenesAsync();
        return Ok(scenes);
    }

    [HttpPost("scene/{name}")]
    public async Task<ActionResult> SetScene(string name)
    {
        await _obsService.SetSceneAsync(name);
        return Ok(new { success = true });
    }

    [HttpPost("stream/toggle")]
    public async Task<ActionResult> ToggleStream()
    {
        await _obsService.ToggleStreamAsync();
        var isStreaming = await _obsService.GetStreamingStatusAsync();
        return Ok(new { success = true, isStreaming });
    }

    [HttpPost("stream/start")]
    public async Task<ActionResult> StartStream()
    {
        await _obsService.StartStreamAsync();
        return Ok(new { success = true });
    }

    [HttpPost("stream/stop")]
    public async Task<ActionResult> StopStream()
    {
        await _obsService.StopStreamAsync();
        return Ok(new { success = true });
    }

    [HttpPost("record/toggle")]
    public async Task<ActionResult> ToggleRecord()
    {
        await _obsService.ToggleRecordAsync();
        var isRecording = await _obsService.GetRecordingStatusAsync();
        return Ok(new { success = true, isRecording });
    }

    [HttpPost("record/start")]
    public async Task<ActionResult> StartRecord()
    {
        await _obsService.StartRecordAsync();
        return Ok(new { success = true });
    }

    [HttpPost("record/stop")]
    public async Task<ActionResult> StopRecord()
    {
        await _obsService.StopRecordAsync();
        return Ok(new { success = true });
    }

    [HttpGet("audio")]
    public async Task<ActionResult<List<AudioSourceDto>>> GetAudioSources()
    {
        var sources = await _obsService.GetAudioSourcesAsync();
        return Ok(sources);
    }

    [HttpPost("audio/{name}/toggle")]
    public async Task<ActionResult> ToggleMute(string name)
    {
        await _obsService.ToggleMuteAsync(name);
        return Ok(new { success = true });
    }

    [HttpPost("audio/{name}/volume")]
    public async Task<ActionResult> SetVolume(string name, [FromBody] VolumeRequest request)
    {
        await _obsService.SetVolumeAsync(name, request.Volume);
        return Ok(new { success = true });
    }

    // Scene Items (sources within scenes)
    [HttpGet("scene/{sceneName}/items")]
    public async Task<ActionResult<List<SceneItemDto>>> GetSceneItems(string sceneName)
    {
        var items = await _obsService.GetSceneItemsAsync(sceneName);
        return Ok(items);
    }

    [HttpPost("scene/{sceneName}/item/{itemId}/toggle")]
    public async Task<ActionResult> ToggleSceneItemVisibility(string sceneName, int itemId)
    {
        await _obsService.ToggleSceneItemVisibilityAsync(sceneName, itemId);
        return Ok(new { success = true });
    }

    [HttpPost("scene/{sceneName}/item/{itemId}/visibility")]
    public async Task<ActionResult> SetSceneItemVisibility(string sceneName, int itemId, [FromBody] VisibilityRequest request)
    {
        await _obsService.SetSceneItemVisibilityAsync(sceneName, itemId, request.Visible);
        return Ok(new { success = true });
    }

    // Replay Buffer
    [HttpGet("replay/status")]
    public async Task<ActionResult<ReplayBufferStatusDto>> GetReplayBufferStatus()
    {
        var status = await _obsService.GetReplayBufferStatusAsync();
        return Ok(status);
    }

    [HttpPost("replay/toggle")]
    public async Task<ActionResult> ToggleReplayBuffer()
    {
        await _obsService.ToggleReplayBufferAsync();
        return Ok(new { success = true });
    }

    [HttpPost("replay/save")]
    public async Task<ActionResult> SaveReplayBuffer()
    {
        await _obsService.SaveReplayBufferAsync();
        return Ok(new { success = true });
    }

    [HttpPost("replay/start")]
    public async Task<ActionResult> StartReplayBuffer()
    {
        await _obsService.StartReplayBufferAsync();
        return Ok(new { success = true });
    }

    [HttpPost("replay/stop")]
    public async Task<ActionResult> StopReplayBuffer()
    {
        await _obsService.StopReplayBufferAsync();
        return Ok(new { success = true });
    }

    // Stats
    [HttpGet("stats")]
    public async Task<ActionResult<StreamStatsDto>> GetStats()
    {
        var stats = await _obsService.GetStatsAsync();
        return Ok(stats);
    }

    // Filters
    [HttpGet("source/{sourceName}/filters")]
    public async Task<ActionResult<List<SourceFilterDto>>> GetSourceFilters(string sourceName)
    {
        var filters = await _obsService.GetSourceFiltersAsync(sourceName);
        return Ok(filters);
    }

    [HttpPost("source/{sourceName}/filter/{filterName}/toggle")]
    public async Task<ActionResult> ToggleSourceFilter(string sourceName, string filterName)
    {
        await _obsService.ToggleSourceFilterAsync(sourceName, filterName);
        return Ok(new { success = true });
    }

    [HttpPost("source/{sourceName}/filter/{filterName}/enabled")]
    public async Task<ActionResult> SetSourceFilterEnabled(string sourceName, string filterName, [FromBody] EnabledRequest request)
    {
        await _obsService.SetSourceFilterEnabledAsync(sourceName, filterName, request.Enabled);
        return Ok(new { success = true });
    }

    // Hotkeys
    [HttpGet("hotkeys")]
    public async Task<ActionResult<List<HotkeyDto>>> GetHotkeys()
    {
        var hotkeys = await _obsService.GetHotkeyListAsync();
        return Ok(hotkeys);
    }

    [HttpPost("hotkey/{name}/trigger")]
    public async Task<ActionResult> TriggerHotkey(string name)
    {
        await _obsService.TriggerHotkeyByNameAsync(name);
        return Ok(new { success = true });
    }

    // Macro execution
    [HttpPost("macro/execute")]
    public async Task<ActionResult> ExecuteMacro([FromBody] ExecuteMacroRequest request)
    {
        await _obsService.ExecuteMacroAsync(request.Actions);
        return Ok(new { success = true });
    }
}

public class VolumeRequest
{
    public float Volume { get; set; }
}

public class VisibilityRequest
{
    public bool Visible { get; set; }
}

public class EnabledRequest
{
    public bool Enabled { get; set; }
}
