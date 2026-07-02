using System;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Commands.CooperativeGroups.ApproveGroupMember;
using AjoCoreBackend.Application.Commands.CooperativeGroups.CreateGroup;
using AjoCoreBackend.Application.Commands.CooperativeGroups.RejectGroupMember;
using AjoCoreBackend.Application.Commands.CooperativeGroups.RequestJoinGroup;
using AjoCoreBackend.Application.Queries.CooperativeGroups.GetAllGroups;
using AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupMembers;
using AjoCoreBackend.Application.Queries.CooperativeGroups.GetPendingRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Route("api/groups")]
    public class CooperativeGroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CooperativeGroupsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search/list all cooperative groups. Public endpoint for traders to discover groups.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var result = await _mediator.Send(new GetAllGroupsQuery { SearchTerm = search });
            return Ok(result);
        }

        /// <summary>
        /// Get a single cooperative group by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupById.GetGroupByIdQuery { GroupId = id });
            return Ok(result);
        }

        /// <summary>
        /// Create a cooperative group. Only CooperativeAdmin role.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id }, new { id });
        }

        /// <summary>
        /// Trader requests to join a group.
        /// </summary>
        [Authorize]
        [HttpPost("{groupId}/join")]
        public async Task<IActionResult> RequestJoin(Guid groupId)
        {
            var membershipId = await _mediator.Send(new RequestJoinGroupCommand { GroupId = groupId });
            return Ok(new { membershipId, status = "Pending" });
        }

        /// <summary>
        /// Admin views pending join requests for their group.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpGet("{groupId}/requests")]
        public async Task<IActionResult> GetPendingRequests(Guid groupId)
        {
            var result = await _mediator.Send(new GetPendingRequestsQuery { GroupId = groupId });
            return Ok(result);
        }

        /// <summary>
        /// Admin approves a membership request.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost("requests/{membershipId}/approve")]
        public async Task<IActionResult> Approve(Guid membershipId)
        {
            await _mediator.Send(new ApproveGroupMemberCommand { MembershipId = membershipId });
            return Ok(new { status = "Approved" });
        }

        /// <summary>
        /// Admin rejects a membership request.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost("requests/{membershipId}/reject")]
        public async Task<IActionResult> Reject(Guid membershipId)
        {
            await _mediator.Send(new RejectGroupMemberCommand { MembershipId = membershipId });
            return Ok(new { status = "Rejected" });
        }

        /// <summary>
        /// Get all approved members of a group.
        /// </summary>
        [Authorize]
        [HttpGet("{groupId}/members")]
        public async Task<IActionResult> GetMembers(Guid groupId)
        {
            var result = await _mediator.Send(new GetGroupMembersQuery { GroupId = groupId });
            return Ok(result);
        }
        /// <summary>
        /// Admin generates an invite link for traders to join the group.
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpGet("{groupId}/invite-link")]
        public async Task<IActionResult> GenerateInviteLink(Guid groupId, [FromQuery] string baseUrl = "https://your-frontend.com")
        {
            var inviteLink = await _mediator.Send(new Application.Queries.CooperativeGroups.GenerateInviteLink.GenerateInviteLinkQuery 
            { 
                GroupId = groupId,
                BaseUrl = baseUrl
            });
            return Ok(new { inviteLink });
        }

        /// <summary>
        /// Admin adds members directly to the group (single or batch).
        /// </summary>
        [Authorize(Roles = "CooperativeAdmin")]
        [HttpPost("{groupId}/members/add")]
        public async Task<IActionResult> AddMembers(Guid groupId, [FromBody] System.Collections.Generic.List<AjoCoreBackend.Application.Commands.CooperativeGroups.AddMembers.AddGroupMemberDto> members)
        {
            var command = new AjoCoreBackend.Application.Commands.CooperativeGroups.AddMembers.AddMembersToGroupCommand
            {
                GroupId = groupId,
                Members = members
            };
            var result = await _mediator.Send(command);
            return Ok(new { status = "Processed", results = result });
        }
    }
}
