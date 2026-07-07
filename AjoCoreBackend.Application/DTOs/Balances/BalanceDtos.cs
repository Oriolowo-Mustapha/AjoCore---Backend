using System;
using System.Collections.Generic;

namespace AjoCoreBackend.Application.DTOs.Balances
{
    public record SystemAdminBalanceDto
    {
        public int ActiveCycles { get; init; }
        public int TotalGroups { get; init; }
        public int TotalMembers { get; init; }
        public decimal TotalContributions { get; init; }
        public decimal TotalPayouts { get; init; }
        public decimal TotalReversals { get; init; }
        public decimal PendingContributions { get; init; }
        public int Traders { get; init; }
        public int CooperativeAdmins { get; init; }
        public int TotalUsers => Traders + CooperativeAdmins;
        public decimal SystemWalletBalance => TotalContributions - TotalPayouts - TotalReversals;
    }

    public record CooperativeAdminBalanceDto
    {
        public Guid CooperativeGroupId { get; init; }
        public int ActiveCycles { get; init; }
        public int TotalMembers { get; init; }
        public int TotalGroups { get; init; }
        public decimal TotalContributions { get; init; }
        public decimal TotalPayouts { get; init; }
        public decimal TotalReversals { get; init; }
        public decimal PendingContributions { get; init; }
        public decimal GroupWalletBalance => TotalContributions - TotalPayouts - TotalReversals;
    }

    public record SavingCycleBalanceDto
    {
        public Guid SavingCycleId { get; init; }
        public decimal TotalContributions { get; init; }
        public decimal TotalPayouts { get; init; }
        public decimal TotalReversals { get; init; }
        public decimal CycleWalletBalance => TotalContributions - TotalPayouts - TotalReversals;
    }

    public record TraderBalanceDto
    {
        public decimal OverallTotalPaid { get; set; }
        public decimal PendingContributions { get; set; }
        public decimal CurrentIntervalTarget { get; set; }
        public decimal CurrentIntervalSaved { get; set; }
        public List<TraderCycleBalanceDto> CycleBalances { get; init; } = new();
    }

    public record TraderCycleBalanceDto
    {
        public Guid CycleId { get; init; }
        public string CycleName { get; init; } = string.Empty;
        public string CycleType { get; init; } = string.Empty;
        public string CycleStatus { get; init; } = string.Empty;
        public decimal TargetAmount { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal RemainingAmount => Math.Max(0, TargetAmount - TotalPaid);
        public int CurrentInterval { get; init; }
        public decimal CurrentIntervalTarget { get; init; }
        public decimal CurrentIntervalSaved { get; init; }
    }
}
