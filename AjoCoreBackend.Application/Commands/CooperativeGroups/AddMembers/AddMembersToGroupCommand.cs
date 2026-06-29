using System;
using System.Collections.Generic;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.AddMembers
{
    public class AddMembersToGroupCommand : IRequest<List<string>>
    {
        public Guid GroupId { get; set; }
        public List<AddGroupMemberDto> Members { get; set; } = new();
    }

    public class AddGroupMemberDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }
}
