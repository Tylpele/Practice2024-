using Microsoft.AspNetCore.Mvc;
using СhatBot.Models;

namespace СhatBot.Controllers
{
    public class MessageController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string inputText)
        {

            if (ModelState.IsValid)
            {
                ViewBag.Message = "dfasaf";
            }
            return View("Index");
        }
    }
}
