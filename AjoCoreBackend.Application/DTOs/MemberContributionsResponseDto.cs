using System.Collections.Generic;

namespace AjoCoreBackend.Application.DTOs
{
    public class MemberContributionsResponseDto
    {
        public string MemberName { get; set; } = string.Empty;
        public decimal TotalContributed { get; set; }
        public List<ContributionLedgerDto> Contributions { get; set; } = new List<ContributionLedgerDto>();
    }
}
