using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rota.Application.Interfaces;
using Rota.API.Models;
using System.Threading.Tasks;
using System;
using Rota.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _svc;
    private readonly ILogger<InvoiceController> _log;

    public InvoiceController(IInvoiceService svc, ILogger<InvoiceController> log)
    {
        _svc = svc;
        _log = log;
    }


    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] InvoiceUploadForm form)
    {
      
        if (form.File == null || form.File.Length == 0)
             return BadRequest(new { error = "O arquivo PDF é obrigatório." });

        await using var st = form.File.OpenReadStream();

       
        
        
        var normalizedValue = form.Value.Replace(",", ".");
        
       
        if (!decimal.TryParse(normalizedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
        {
            return BadRequest(new { error = "O valor informado é inválido." });
        }

       
        var dto = new InvoiceUploadDTO
        {
            Name = form.Name,
            Description = form.Description,
            IssueDate = form.IssueDate,
            Value = decimalValue, 
            AgencyId = form.AgencyId,
            IsAnnual = form.IsAnnual,
            FileName = form.File.FileName,
            ContentType = form.File.ContentType,
            Content = st,
        };

        var created = await _svc.UploadAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] InvoiceUpdateDTO dto)
    {
        try
        {
            await _svc.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDTO>> GetById(int id) =>
        Ok(await _svc.GetByIdAsync(id));

    [HttpGet]
    public async Task<IEnumerable<InvoiceDTO>> GetAll() =>
        await _svc.GetAllAsync();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    { await _svc.RemoveAsync(id); return NoContent(); }

    [HttpGet("{id}/arquivo")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var dto = await _svc.DownloadAsync(id, ct);
        return File(dto.Content, dto.ContentType, dto.FileName);
    }

}
