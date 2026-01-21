using Microsoft.AspNetCore.Mvc;
using System.Text;
namespace IngestApi.Controllers
{
    [ApiController]
    [Route("api/ingest")]
    public class IngestController : ControllerBase
    {
        [HttpPost("ccda")]
        public async Task<IActionResult> IngestCcda
        ([FromQuery] string sourceSystem)
        {
           const long MaxFileSize = 5 * 1024 * 1024;

           if(Request.ContentLength == null || Request.ContentLength == 0)
           {
                return BadRequest(Error("EMPTY_PAYLOAD", "Payload cannot be Empty"));
           }

           if(Request.ContentLength > MaxFileSize)
            {
                return BadRequest(Error("FILE_SIZE_EXCEEDED", "File size exceeds 5 MB limit"));
            }

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync();

            var allowedSources = new [] {"Epic", "Cerner"};

            if(!allowedSources.Contains(sourceSystem))
            {
                return BadRequest(Error("INVALID_SOURCE", "Source is neither Epic nor Cerner"));
            } 

            string documentType;
            if(payload.Contains("<ClinicalDocument"))
                documentType = "CCDA";
            else if(payload.Contains("\"resourceType\""))
                documentType = "FHIR";
            else
                return BadRequest(Error("UNSUPPORTED_DOCUMENT", "Unsupported Document Type"));

            var ingestId = $"ING-{DateTime.UtcNow.Ticks}";
            var correlationId =Guid.NewGuid().ToString();

            var container = sourceSystem.ToLower();
            Directory.CreateDirectory($"storage/{container}");

            await System.IO.File.WriteAllTextAsync
            ($"storage/{container}/{ingestId}.{documentType.ToLower()}", payload);

            return Created("", new
            {
                ingestId,
                correlationId,
                status = "Received",
                sourceSystem,
                documentType,
                receivedAt = DateTime.UtcNow
            });
        }

         private static Object Error(string code, string message) => 
           new
           {
               status = "Failed",
               errors = new [] {new {code, message}}
           };
    }
}