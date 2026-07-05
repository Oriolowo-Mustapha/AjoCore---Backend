using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.DTOs.IndividualSavingCycle;
using AjoCoreBackend.Domain.Entities;
using AutoMapper;
using System.Linq;

namespace AjoCoreBackend.Application.Mappings
{
    public class SavingCycleProfile : Profile
    {
        public SavingCycleProfile()
        {
            // SavingCycle → SavingCycleDto
            CreateMap<SavingCycle, SavingCycleDto>()
                .ForMember(dest => dest.CycleType, opt => opt.MapFrom(src => src.CycleType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TotalSaved, opt => opt.MapFrom(src => 
                    src.Members != null ? src.Members.SelectMany(m => m.Contributions ?? new System.Collections.Generic.List<ContributionLedger>()).Sum(c => c.Amount) : 0));

            // SavingCycleMember → SavingCycleMemberDto
            // Flatten the nested VirtualAccount properties directly onto the DTO
            CreateMap<SavingCycleMember, SavingCycleMemberDto>()
                .ForMember(dest => dest.VirtualAccountNumber, opt => opt.MapFrom(src => src.VirtualAccount != null ? src.VirtualAccount.AccountNumber : null))
                .ForMember(dest => dest.VirtualAccountBank, opt => opt.MapFrom(src => src.VirtualAccount != null ? src.VirtualAccount.BankName : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus.ToString()))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.TraderName, opt => opt.MapFrom(src => src.Trader != null ? $"{src.Trader.FirstName} {src.Trader.LastName}" : string.Empty))
                .ForMember(dest => dest.TraderEmail, opt => opt.MapFrom(src => src.Trader != null ? src.Trader.Email : string.Empty))
                .ForMember(dest => dest.TotalContributed, opt => opt.MapFrom(src => src.Contributions != null ? src.Contributions.Sum(c => c.Amount) : 0));
            // Flatten the nested VirtualAccount properties directly onto the DTO
            CreateMap<SavingCycleMember, IndividualSavingCycleDto>()
                .ForMember(dest => dest.VirtualAccountNumber, opt => opt.MapFrom(src => src.VirtualAccount != null ? src.VirtualAccount.AccountNumber : null))
                .ForMember(dest => dest.VirtualAccountBank, opt => opt.MapFrom(src => src.VirtualAccount != null ? src.VirtualAccount.BankName : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.CreatedAt));
            
            // Ledger → DTO (direct 1:1 maps)
            CreateMap<ContributionLedger, ContributionLedgerDto>();
            CreateMap<PayoutLedger, PayoutLedgerDto>();
        }
    }
}
