using System;

namespace AjoCoreBackend.Application.DTOs
{
    public record CooperativeGroupDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Guid AdminTraderId { get; init; }
        public string AdminName { get; init; } = string.Empty;
        public int MemberCount { get; init; }
        public int CycleCount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
