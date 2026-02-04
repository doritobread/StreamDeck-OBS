namespace StreamAp.Models;

public class ObsStatusDto
{
    public bool IsConnected { get; set; }
    public string? CurrentScene { get; set; }
    public bool IsStreaming { get; set; }
    public bool IsRecording { get; set; }
    public List<SceneDto> Scenes { get; set; } = new();
    public List<AudioSourceDto> AudioSources { get; set; } = new();
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
}
