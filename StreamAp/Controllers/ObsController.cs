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

    [HttpPost("record/toggle")]
    public async Task<ActionResult> ToggleRecord()
    {
        await _obsService.ToggleRecordAsync();
        var isRecording = await _obsService.GetRecordingStatusAsync();
        return Ok(new { success = true, isRecording });
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
}
