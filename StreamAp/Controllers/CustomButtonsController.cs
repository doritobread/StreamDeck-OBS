using Microsoft.AspNetCore.Mvc;
using StreamAp.Models;
using StreamAp.Services;

namespace StreamAp.Controllers;

[ApiController]
[Route("api/buttons")]
public class CustomButtonsController : ControllerBase
{
    private readonly ICustomButtonService _buttonService;
    private readonly IObsService _obsService;

    public CustomButtonsController(ICustomButtonService buttonService, IObsService obsService)
    {
        _buttonService = buttonService;
        _obsService = obsService;
    }

    [HttpGet]
    public async Task<ActionResult<CustomButtonConfig>> GetConfig()
    {
        var config = await _buttonService.GetConfigAsync();
        return Ok(config);
    }

    [HttpPut]
    public async Task<ActionResult> SaveConfig([FromBody] CustomButtonConfig config)
    {
        await _buttonService.SaveConfigAsync(config);
        return Ok(new { success = true });
    }

    // Pages
    [HttpGet("pages")]
    public async Task<ActionResult<List<ButtonPage>>> GetPages()
    {
        var config = await _buttonService.GetConfigAsync();
        return Ok(config.Pages.OrderBy(p => p.Order).ToList());
    }

    [HttpGet("pages/{pageId}")]
    public async Task<ActionResult<ButtonPage>> GetPage(string pageId)
    {
        var page = await _buttonService.GetPageAsync(pageId);
        if (page == null)
            return NotFound();
        return Ok(page);
    }

    [HttpPost("pages")]
    public async Task<ActionResult<ButtonPage>> CreatePage([FromBody] CreatePageRequest request)
    {
        var page = await _buttonService.CreatePageAsync(request.Name);
        return CreatedAtAction(nameof(GetPage), new { pageId = page.Id }, page);
    }

    [HttpPut("pages/{pageId}")]
    public async Task<ActionResult<ButtonPage>> UpdatePage(string pageId, [FromBody] UpdatePageRequest request)
    {
        var page = await _buttonService.UpdatePageAsync(pageId, request.Name, request.Order);
        if (page == null)
            return NotFound();
        return Ok(page);
    }

    [HttpDelete("pages/{pageId}")]
    public async Task<ActionResult> DeletePage(string pageId)
    {
        await _buttonService.DeletePageAsync(pageId);
        return NoContent();
    }

    // Buttons
    [HttpGet("pages/{pageId}/buttons")]
    public async Task<ActionResult<List<CustomButton>>> GetButtons(string pageId)
    {
        var page = await _buttonService.GetPageAsync(pageId);
        if (page == null)
            return NotFound();
        return Ok(page.Buttons.OrderBy(b => b.Order).ToList());
    }

    [HttpGet("pages/{pageId}/buttons/{buttonId}")]
    public async Task<ActionResult<CustomButton>> GetButton(string pageId, string buttonId)
    {
        var button = await _buttonService.GetButtonAsync(pageId, buttonId);
        if (button == null)
            return NotFound();
        return Ok(button);
    }

    [HttpPost("pages/{pageId}/buttons")]
    public async Task<ActionResult<CustomButton>> CreateButton(string pageId, [FromBody] CustomButton button)
    {
        try
        {
            var created = await _buttonService.CreateButtonAsync(pageId, button);
            return CreatedAtAction(nameof(GetButton), new { pageId, buttonId = created.Id }, created);
        }
        catch (ArgumentException)
        {
            return NotFound("Page not found");
        }
    }

    [HttpPut("pages/{pageId}/buttons/{buttonId}")]
    public async Task<ActionResult<CustomButton>> UpdateButton(string pageId, string buttonId, [FromBody] CustomButton button)
    {
        var updated = await _buttonService.UpdateButtonAsync(pageId, buttonId, button);
        if (updated == null)
            return NotFound();
        return Ok(updated);
    }

    [HttpDelete("pages/{pageId}/buttons/{buttonId}")]
    public async Task<ActionResult> DeleteButton(string pageId, string buttonId)
    {
        await _buttonService.DeleteButtonAsync(pageId, buttonId);
        return NoContent();
    }

    // Execute a button's macro
    [HttpPost("pages/{pageId}/buttons/{buttonId}/execute")]
    public async Task<ActionResult> ExecuteButton(string pageId, string buttonId)
    {
        var button = await _buttonService.GetButtonAsync(pageId, buttonId);
        if (button == null)
            return NotFound();

        await _obsService.ExecuteMacroAsync(button.Actions);
        return Ok(new { success = true });
    }
}

public class CreatePageRequest
{
    public string Name { get; set; } = "New Page";
}

public class UpdatePageRequest
{
    public string Name { get; set; } = "";
    public int Order { get; set; }
}
