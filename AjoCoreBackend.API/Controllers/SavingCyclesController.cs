using AjoCoreBackend.Application.Commands.CreateSavingCycle;
using AjoCoreBackend.Application.Commands.IndividualContriution.CreateIndividualSavingCycle;
using AjoCoreBackend.Application.Commands.JoinSavingCycle;
using AjoCoreBackend.Application.Commands.StartSavingCycle;
using AjoCoreBackend.Application.Queries.GetAllSavingCycles;
using AjoCoreBackend.Application.Queries.GetMemberContributions;
using AjoCoreBackend.Application.Queries.GetSavingCycleById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/saving-cycles")]
    public class SavingCyclesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SavingCyclesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new saving cycle under a cooperative group. Admin only.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSavingCycleCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPost("individual")]
        public async Task<IActionResult> CreateIndividualSavingCycle([FromBody] CreateIndividualSavingCycleCommand command)
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
        
        [HttpGet("individual")]
        public async Task<IActionResult> GetAllIndividualSavingCycles()
        {
            var cycles = await _mediator.Send(new GetAllSavingCyclesQuery());
            return Ok(cycles);
        }
        [HttpGet("personal/{id}")]
        public async Task<IActionResult> GetPersonalCycleDetails(Guid id)
        {
            var cycle = await _mediator.Send(new AjoCoreBackend.Application.Queries.GetMyCycleDetails.GetMyCycleDetailsQuery { SavingCycleId = id, ExpectedCycleType = "Personal" });
            return Ok(cycle);
        }

        [HttpGet("rosca/{id}")]
        public async Task<IActionResult> GetRoscaCycleDetails(Guid id)
        {
            var cycle = await _mediator.Send(new AjoCoreBackend.Application.Queries.GetMyCycleDetails.GetMyCycleDetailsQuery { SavingCycleId = id, ExpectedCycleType = "Rosca" });
            return Ok(cycle);
        }

        [HttpGet("asca/{id}")]
        public async Task<IActionResult> GetAscaCycleDetails(Guid id)
        {
            var cycle = await _mediator.Send(new AjoCoreBackend.Application.Queries.GetMyCycleDetails.GetMyCycleDetailsQuery { SavingCycleId = id, ExpectedCycleType = "Asca" });
            return Ok(cycle);
        }

        [Authorize(Roles = "CooperativeAdmin")]
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetCycleMembers(Guid id)
        {
            var members = await _mediator.Send(new AjoCoreBackend.Application.Queries.GetSavingCycleMembers.GetSavingCycleMembersQuery { SavingCycleId = id });
            return Ok(members);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cycle = await _mediator.Send(new GetSavingCycleByIdQuery { SavingCycleId = id });
            return Ok(cycle);
        }

        [HttpGet("my-personal")]
        public async Task<IActionResult> GetMyPersonalCycles()
        {
            var cycles = await _mediator.Send(new AjoCoreBackend.Application.Queries.GetMyPersonalCycles.GetMyPersonalCyclesQuery());
            return Ok(cycles);
        }

        [HttpPost("{id}/liquidate-early")]
        public async Task<IActionResult> LiquidateEarly(Guid id)
        {
            var command = new AjoCoreBackend.Application.Commands.IndividualContriution.LiquidateEarly.LiquidateEarlyCommand { SavingCycleId = id };
            await _mediator.Send(command);
            return Ok(new { status = "Liquidated successfully", message = "A 5% penalty has been applied and your payout is being processed." });
        }

        /// <summary>
        /// Trader requests to join a saving cycle.
        /// </summary>
        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(Guid id)
        {
            var command = new JoinSavingCycleCommand
            {
                SavingCycleId = id
            };
            var memberId = await _mediator.Send(command);
            return Ok(new { memberId });
        }

        /// <summary>
        /// Start a saving cycle. Admin only.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost("{id}/start")]
        public async Task<IActionResult> Start(Guid id)
        {
            var command = new StartSavingCycleCommand { SavingCycleId = id };
            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Get contributions for a specific member.
        /// </summary>
        [HttpGet("members/{memberId}/contributions")]
        public async Task<IActionResult> GetContributions(Guid memberId)
        {
            var result = await _mediator.Send(new GetMemberContributionsQuery { SavingCycleMemberId = memberId });
            return Ok(result);
        }

        /// <summary>
        /// Approve a member's request to join a saving cycle. Admin only.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost("{id}/members/{memberId}/approve")]
        public async Task<IActionResult> ApproveMember(Guid id, Guid memberId)
        {
            await _mediator.Send(new AjoCoreBackend.Application.Commands.SavingCycles.ApproveMember.ApproveCycleMemberCommand { SavingCycleId = id, MemberId = memberId });
            return Ok(new { status = "Approved" });
        }

        /// <summary>
        /// Reject a member's request to join a saving cycle. Admin only.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost("{id}/members/{memberId}/reject")]
        public async Task<IActionResult> RejectMember(Guid id, Guid memberId)
        {
            await _mediator.Send(new AjoCoreBackend.Application.Commands.SavingCycles.RejectMember.RejectCycleMemberCommand { SavingCycleId = id, MemberId = memberId });
            return Ok(new { status = "Rejected" });
        }

        /// <summary>
        /// Reorder payout orders for members in a saving cycle before it starts. Admin only.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost("{id}/members/reorder")]
        public async Task<IActionResult> ReorderMembers(Guid id, [FromBody] System.Collections.Generic.List<AjoCoreBackend.Application.Commands.SavingCycles.ReorderMembers.MemberOrderDto> memberOrders)
        {
            await _mediator.Send(new AjoCoreBackend.Application.Commands.SavingCycles.ReorderMembers.ReorderCycleMembersCommand 
            { 
                SavingCycleId = id, 
                MemberOrders = memberOrders 
            });
            return Ok(new { status = "Reordered successfully" });
        }
    }
}
