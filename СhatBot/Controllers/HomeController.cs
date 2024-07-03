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

        

    }
}
