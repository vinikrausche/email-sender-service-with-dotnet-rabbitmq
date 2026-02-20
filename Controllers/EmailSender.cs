using EmailSender.Factory;
using EmailSender.Messaging.Producer;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult>  sendEmail([FromBody] string texto, CancellationToken ct)
        {
            await _producer.PublishAsync("email-sender", texto, ct);
            return Accepted();
        }
    }
}   
