using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ_Excel.Web.Models;
using RabbitMQ_Excel.Web.Services;

namespace RabbitMQ_Excel.Web.Controllers
{
    [Authorize]
    public class ProductsController(AppDbContext appDbContext, 
        UserManager<IdentityUser> userManager,
        RabbitMQ_PublisherService rabbitMQ_PublisherService) : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateExcelFile()
        {
            var user = await userManager.FindByNameAsync(User.Identity!.Name!);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1,10)}";

            UserFile userFile = new UserFile
            {
                FileName = fileName,
                FileStatus = FileStatus.Creating,
                UserId = user!.Id
            };

            await appDbContext.UserFiles.AddAsync(userFile);

            await appDbContext.SaveChangesAsync();


            rabbitMQ_PublisherService.PublishAsync(new ClassLibrary.CreateExcelMessage
            {
                FileId = userFile.Id
            });


            TempData["StartCreatingExcel"] = true;

            return RedirectToAction(nameof(DownloadExcelFile));
        }

        public async Task<IActionResult> DownloadExcelFile()
        {
            var user = await userManager.FindByNameAsync(User.Identity!.Name!);

            var file = await appDbContext.UserFiles.Where(x => x.UserId == user!.Id).OrderByDescending(x => x.Id).ToListAsync();

            return View(file);
        }
    }
}
