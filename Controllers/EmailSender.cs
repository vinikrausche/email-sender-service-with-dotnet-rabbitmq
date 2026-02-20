using EmailSender.Factory;
using EmailSender.Messaging.Producer;
using EmailSender.Records;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EmailSender.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailSender : ControllerBase
    {
        public readonly RabbitMqProducer _producer;

        public EmailSender(RabbitMqProducer producer)
        {
            _producer = producer;
        }
        [HttpPost]
        public async Task<IActionResult>  sendEmail([FromBody] SendEmailRequest body, CancellationToken ct)
        {
            var json = JsonConvert.SerializeObject(body);
            await _producer.PublishAsync("email-sender", json, ct);
            return Accepted();
        }
    }
}   
