using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Examples.Messages;

namespace GuimoSoft.Bus.Examples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly IEventDispatcher _dispatcher;

        public HelloController(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet]
        [Route("/{name}/{throwException}")]
        public async Task SayHello([FromRoute] string name, bool throwException)
        {
            await _dispatcher.DispatchAsync(Guid.NewGuid().ToString(), new HelloMessage(name, throwException));
        }
    }
}
