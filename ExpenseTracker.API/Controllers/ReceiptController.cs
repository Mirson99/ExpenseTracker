using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Receipts.Commands.ProcessReceipt;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("receipt")]
public class ReceiptController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorageService;

    public ReceiptController(ISender sender, IFileStorageService fileStorageService)
    {
        _sender = sender;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Test endpoint for ProcessReceiptCommandHandler.
    /// </summary>
    /// <param name="storageKey">Storage key of the uploaded receipt file.</param>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessReceipt([FromQuery] string storageKey)
    {
        var command = new ProcessReceiptCommand(storageKey);
        var expenseId = await _sender.Send(command);
        return Ok(expenseId);
    }

    /// <summary>
    /// Generates a pre-signed URL for uploading a receipt file directly to storage.
    /// </summary>
    /// <param name="fileName">File Name</param>
    /// <param name="contentType">MIME content type of the file.</param>
    [HttpGet("presigned-url")]
    public async Task<IActionResult> GetPreSignedUrl(
        [FromQuery] string fileName,
        [FromQuery] string contentType)
    {
        var expiry = TimeSpan.FromMinutes(5);

        var (preSignedUrl, objectKey) = await _fileStorageService.GetPreSignedUrlAsync(fileName, expiry, contentType);

        return Ok(new { preSignedUrl, objectKey });
    }
}

