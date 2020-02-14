using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using poks41.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace poks41.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(Storage s)
        {
            await s.SendMessage("pages",$"Open Home page at {DateTime.Now}");

            var w = new ViewCntClass();
            w = await s.GetRec<ViewCntClass>("pages",w);
            ViewBag.view_cnt = w.Count;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Project(ProjectClass project, Storage s)
        {
            project.PartitionKey = "new";
            project.RowKey = Guid.NewGuid().ToString();




            var client = new SendGridClient("SG.YKTQoc");
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("ourcompany@co.com", "Правительство Батайска"),
                Subject = "Новый проект",
                HtmlContent = $"<p>Здравствуйте!</p> <p>Ваш проет переда на рассмотрение: {DateTime.Now}</p>"
            };
            msg.AddTo(new EmailAddress(project.Email, project.Email));
            var response = await client.SendEmailAsync(msg);





            s.InsertIn("projects", project);
            return View();
        }

        [HttpPost]
        public IActionResult Order(String Name, String Email)
        {
            return View();
        }

        public async Task<IActionResult> Messages(Storage s)
        {
            var queue = await s.GetQueue("pages");
            CloudQueueMessage retrievedMessage = await queue.GetMessageAsync();

            if (retrievedMessage != null)
            { 
                ViewBag.msg = retrievedMessage?.AsString;
                await queue.DeleteMessageAsync(retrievedMessage);
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
