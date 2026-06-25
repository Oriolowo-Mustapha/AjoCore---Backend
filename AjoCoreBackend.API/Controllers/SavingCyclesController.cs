using System;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Commands.CreateSavingCycle;
using AjoCoreBackend.Application.Commands.JoinSavingCycle;
using AjoCoreBackend.Application.Commands.StartSavingCycle;
using AjoCoreBackend.Application.Queries.GetAllSavingCycles;
using AjoCoreBackend.Application.Queries.GetSavingCycleById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Route("api/saving-cycles")]
    public class SavingCyclesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SavingCyclesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSavingCycleCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cycles = await _mediator.Send(new GetAllSavingCyclesQuery());
            return Ok(cycles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cycle = await _mediator.Send(new GetSavingCycleByIdQuery { SavingCycleId = id });
            return Ok(cycle);
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(Guid id, [FromBody] JoinSavingCycleRequest request)
        {
            var command = new JoinSavingCycleCommand
            {
                SavingCycleId = id,
                UserId = request.UserId
            };
            var memberId = await _mediator.Send(command);
            return Ok(new { memberId });
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> Start(Guid id)
        {
            var command = new StartSavingCycleCommand { SavingCycleId = id };
            await _mediator.Send(command);
            return Ok();
        }
    }

    public class JoinSavingCycleRequest
    {
        public Guid UserId { get; set; }
    }
}
