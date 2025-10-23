using IETT_APP.Application.Dtos.Line;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class LinesController : ControllerBase
{
    private readonly ILineService<Guid> _lineService;

    public LinesController(ILineService<Guid> lineService)
    {
        _lineService = lineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? active)
    {
        var lines = await _lineService.GetAllAsync();
        lines = lines.Where(l => !l.IsDeleted).ToList();

        if (active.HasValue)
            lines = lines.Where(l => l.IsActive == active.Value).ToList();

        return Ok(lines);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var line = await _lineService.GetByIdAsync(id);
        if (line == null || line.IsDeleted) return NotFound();
        return Ok(line);
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromBody] LineCreateUpdateDto<Guid> dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _lineService.CreateOrUpdateAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _lineService.SoftDeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query)
    {
        var lines = string.IsNullOrWhiteSpace(query)
            ? await _lineService.GetAllAsync()
            : await _lineService.SearchAsync(query);

        lines = lines.Where(l => !l.IsDeleted).ToList();
        return Ok(lines);
    }
}

