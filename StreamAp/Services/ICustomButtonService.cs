using StreamAp.Models;

namespace StreamAp.Services;

public interface ICustomButtonService
{
    Task<CustomButtonConfig> GetConfigAsync();
    Task SaveConfigAsync(CustomButtonConfig config);
    Task<ButtonPage?> GetPageAsync(string pageId);
    Task<ButtonPage> CreatePageAsync(string name);
    Task<ButtonPage?> UpdatePageAsync(string pageId, string name, int order);
    Task DeletePageAsync(string pageId);
    Task<CustomButton?> GetButtonAsync(string pageId, string buttonId);
    Task<CustomButton> CreateButtonAsync(string pageId, CustomButton button);
    Task<CustomButton?> UpdateButtonAsync(string pageId, string buttonId, CustomButton button);
    Task DeleteButtonAsync(string pageId, string buttonId);
}
