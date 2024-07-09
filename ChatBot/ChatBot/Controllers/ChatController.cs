using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using ChatBot.Models;
using Microsoft.AspNetCore.Connections;

namespace ChatBotMVC.Controllers
{
    public class ChatController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new MessageModel());
        }

        [HttpPost]
        public IActionResult SendMessage(MessageModel model)
        {
            if (ModelState.IsValid)
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "chat_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(model.Text);

                channel.BasicPublish(exchange: "",
                                     routingKey: "chat_queue",
                                     basicProperties: null,
                                     body: body);

                ViewBag.Message = "Message sent successfully!";
            }
            return View("Index", model);
        }
    }
}
