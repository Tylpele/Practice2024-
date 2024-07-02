using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using СhatBot.Models;
using СhatBot.RabbitMQ;

namespace ChatBot.Controllers
{
    public class HomeController : Controller

    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly PreHandler _preHandler;

        public HomeController()
        {
            _rabbitMqService = new RabbitMqService();
            _preHandler = new PreHandler();

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Submit(string inputText)
        {
            string queueName = "pre-queue";
            _rabbitMqService.SendMessage(inputText, queueName);
            _preHandler.PreHandl(queueName);

            ViewBag.Message = inputText.ToLower();
            return View("Index");
        }

    }
}
