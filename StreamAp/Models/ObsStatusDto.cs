namespace StreamAp.Models;

public class ObsStatusDto
{
    public bool IsConnected { get; set; }
    public string? CurrentScene { get; set; }
    public bool IsStreaming { get; set; }
    public bool IsRecording { get; set; }
    public List<SceneDto> Scenes { get; set; } = new();
    public List<AudioSourceDto> AudioSources { get; set; } = new();
    public List<SceneItemDto> SceneItems { get; set; } = new();
    public StreamStatsDto? Stats { get; set; }
    public ReplayBufferStatusDto? ReplayBuffer { get; set; }
}

public class SceneDto
{
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
}

public class AudioSourceDto
{
    public string Name { get; set; } = "";
    public bool IsMuted { get; set; }
    public float Volume { get; set; } = 1.0f;
}

public class SceneItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string SceneName { get; set; } = "";
    public bool IsVisible { get; set; }
    public string? SourceType { get; set; }
}

public class StreamStatsDto
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double AvailableDiskSpace { get; set; }
    public double ActiveFps { get; set; }
    public double AverageFrameRenderTime { get; set; }
    public long RenderSkippedFrames { get; set; }
    public long RenderTotalFrames { get; set; }
    public long OutputSkippedFrames { get; set; }
    public long OutputTotalFrames { get; set; }
    public string? StreamTimecode { get; set; }
    public long StreamBytes { get; set; }
    public long StreamKbitsPerSec { get; set; }
    public string? RecordTimecode { get; set; }
    public long RecordBytes { get; set; }
}

public class ReplayBufferStatusDto
{
    public bool IsActive { get; set; }
    public string? LastSavedPath { get; set; }
}

public class SourceFilterDto
{
    public string FilterName { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string FilterKind { get; set; } = "";
    public bool IsEnabled { get; set; }
}

public class HotkeyDto
{
    public string Name { get; set; } = "";
}
