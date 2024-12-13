using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ_Excel.Web.Hubs;
using RabbitMQ_Excel.Web.Models;

namespace RabbitMQ_Excel.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController(AppDbContext appDbContext, IHubContext<MyHub> hubContext) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int fileId)
        {
            if (file is not { Length: > 0 }) return BadRequest("File is empty");
            
            var userFile = await appDbContext.UserFiles.FirstAsync(x => x.Id == fileId);

            var filePath = userFile.FileName + Path.GetExtension(file.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            await using var stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);

            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await appDbContext.SaveChangesAsync();

            await hubContext.Clients.User(userFile.UserId!).SendAsync("CompletedFile");
            return Ok();
        }
    }
}
