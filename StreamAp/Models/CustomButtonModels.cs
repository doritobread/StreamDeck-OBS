namespace StreamAp.Models;

public class CustomButtonConfig
{
    public List<ButtonPage> Pages { get; set; } = new();
}

public class ButtonPage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Page";
    public int Order { get; set; }
    public List<CustomButton> Buttons { get; set; } = new();
}

public class CustomButton
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "zap";
    public string Color { get; set; } = "#21262d";
    public string TextColor { get; set; } = "#f0f6fc";
    public int Order { get; set; }
    public List<MacroAction> Actions { get; set; } = new();
}

public class MacroAction
{
    public MacroActionType Type { get; set; }
    public string? Target { get; set; }
    public string? Value { get; set; }
    public int DelayMs { get; set; }
}

public enum MacroActionType
{
    SwitchScene,
    ToggleSource,
    ToggleMute,
    SetVolume,
    StartStream,
    StopStream,
    ToggleStream,
    StartRecord,
    StopRecord,
    ToggleRecord,
    SaveReplay,
    ToggleFilter,
    TriggerHotkey,
    Delay,
    PlaySound
}

public class ExecuteMacroRequest
{
    public List<MacroAction> Actions { get; set; } = new();
}
