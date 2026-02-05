using System.Text.Json;
using StreamAp.Models;

namespace StreamAp.Services;

public class CustomButtonService : ICustomButtonService
{
    private readonly string _configPath;
    private readonly ILogger<CustomButtonService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private CustomButtonConfig? _cachedConfig;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CustomButtonService(IWebHostEnvironment env, ILogger<CustomButtonService> logger)
    {
        _configPath = Path.Combine(env.ContentRootPath, "custombuttons.json");
        _logger = logger;
    }

    public async Task<CustomButtonConfig> GetConfigAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            if (!File.Exists(_configPath))
            {
                _cachedConfig = CreateDefaultConfig();
                await SaveConfigInternalAsync(_cachedConfig);
                return _cachedConfig;
            }

            var json = await File.ReadAllTextAsync(_configPath);
            _cachedConfig = JsonSerializer.Deserialize<CustomButtonConfig>(json, JsonOptions) ?? CreateDefaultConfig();
            return _cachedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading custom button config");
            return CreateDefaultConfig();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveConfigAsync(CustomButtonConfig config)
    {
        await _lock.WaitAsync();
        try
        {
            _cachedConfig = config;
            await SaveConfigInternalAsync(config);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveConfigInternalAsync(CustomButtonConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(_configPath, json);
        _logger.LogInformation("Saved custom button config");
    }

    public async Task<ButtonPage?> GetPageAsync(string pageId)
    {
        var config = await GetConfigAsync();
        return config.Pages.FirstOrDefault(p => p.Id == pageId);
    }

    public async Task<ButtonPage> CreatePageAsync(string name)
    {
        var config = await GetConfigAsync();
        var page = new ButtonPage
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Order = config.Pages.Count,
            Buttons = new List<CustomButton>()
        };
        config.Pages.Add(page);
        await SaveConfigAsync(config);
        return page;
    }

    public async Task<ButtonPage?> UpdatePageAsync(string pageId, string name, int order)
    {
        var config = await GetConfigAsync();
        var page = config.Pages.FirstOrDefault(p => p.Id == pageId);
        if (page == null)
            return null;

        page.Name = name;
        page.Order = order;
        await SaveConfigAsync(config);
        return page;
    }

    public async Task DeletePageAsync(string pageId)
    {
        var config = await GetConfigAsync();
        config.Pages.RemoveAll(p => p.Id == pageId);
        await SaveConfigAsync(config);
    }

    public async Task<CustomButton?> GetButtonAsync(string pageId, string buttonId)
    {
        var page = await GetPageAsync(pageId);
        return page?.Buttons.FirstOrDefault(b => b.Id == buttonId);
    }

    public async Task<CustomButton> CreateButtonAsync(string pageId, CustomButton button)
    {
        var config = await GetConfigAsync();
        var page = config.Pages.FirstOrDefault(p => p.Id == pageId);
        if (page == null)
            throw new ArgumentException("Page not found");

        button.Id = Guid.NewGuid().ToString();
        button.Order = page.Buttons.Count;
        page.Buttons.Add(button);
        await SaveConfigAsync(config);
        return button;
    }

    public async Task<CustomButton?> UpdateButtonAsync(string pageId, string buttonId, CustomButton button)
    {
        var config = await GetConfigAsync();
        var page = config.Pages.FirstOrDefault(p => p.Id == pageId);
        if (page == null)
            return null;

        var index = page.Buttons.FindIndex(b => b.Id == buttonId);
        if (index < 0)
            return null;

        button.Id = buttonId;
        page.Buttons[index] = button;
        await SaveConfigAsync(config);
        return button;
    }

    public async Task DeleteButtonAsync(string pageId, string buttonId)
    {
        var config = await GetConfigAsync();
        var page = config.Pages.FirstOrDefault(p => p.Id == pageId);
        page?.Buttons.RemoveAll(b => b.Id == buttonId);
        await SaveConfigAsync(config);
    }

    private static CustomButtonConfig CreateDefaultConfig()
    {
        return new CustomButtonConfig
        {
            Pages = new List<ButtonPage>
            {
                new()
                {
                    Id = "default",
                    Name = "Main",
                    Order = 0,
                    Buttons = new List<CustomButton>()
                }
            }
        };
    }
}
