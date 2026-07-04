using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.AddMembers
{
    public class AddMembersToGroupCommandHandler : IRequestHandler<AddMembersToGroupCommand, List<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;

        public AddMembersToGroupCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<string>> Handle(AddMembersToGroupCommand request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (!Guid.TryParse(userIdString, out Guid adminId))
            {
                throw new UnauthorizedAccessException("User is not authenticated properly.");
            }

            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (group == null)
            {
                throw new NotFoundException($"CooperativeGroup with ID {request.GroupId} not found.");
            }

            if (group.CooperativeAdminId != adminId)
            {
                throw new UnauthorizedAccessException("Only the creator of this group can add members directly.");
            }

            var results = new List<string>();

            foreach (var memberDto in request.Members)
            {
                // Try find by phone number or email in Traders
                var existingTraders = await _unitOfWork.Repository<Trader>().FindAsync(t => 
                    t.Email.ToLower() == memberDto.Email.ToLower());
                
                // Try find by phone number or email in Admins
                var existingAdmins = await _unitOfWork.Repository<CooperativeAdmin>().FindAsync(a => 
                    a.PhoneNumber == memberDto.PhoneNumber || 
                    a.Email.ToLower() == memberDto.Email.ToLower());

                if (existingAdmins.Any())
                {
                    results.Add($"{memberDto.Email}: Cannot add this user because they are registered as a Cooperative Admin.");
                    continue;
                }
                
                var trader = existingTraders.FirstOrDefault();
                bool isNewTrader = false;
                string generatedPassword = "";

                if (trader == null)
                {
                    // Create new trader
                    generatedPassword = GenerateRandomPassword(10);
                    trader = new Trader
                    {
                        FirstName = memberDto.FirstName ?? "User",
                        LastName = memberDto.LastName ?? "",
                        Email = memberDto.Email.ToLower(),
                        PhoneNumber = memberDto.PhoneNumber,
                        PasswordHash = _passwordHasher.HashPassword(generatedPassword),
                        Role = UserRole.Trader,
                        DateOfBirth = DateTime.SpecifyKind(memberDto.DateOfBirth, DateTimeKind.Utc),
                        IsEmailVerified = true, // Force verified since admin added them
                        IsKycCompleted = false,  // They MUST update their BVN to activate
                        PayoutAccountNumber = memberDto.PayoutAccountNumber,
                        PayoutBankName = memberDto.PayoutBankName,
                        PayoutAccountName = memberDto.PayoutAccountName
                    };

                    await _unitOfWork.Repository<Trader>().AddAsync(trader);
                    isNewTrader = true;
                }

                // Check if already in group
                var existingMemberships = await _unitOfWork.Repository<CooperativeGroupMember>().FindAsync(m => 
                    m.CooperativeGroupId == group.Id && m.TraderId == trader.Id);

                if (existingMemberships.Any())
                {
                    results.Add($"{memberDto.Email}: Already a member of this group.");
                    continue; // Skip adding again
                }

                // Add to group immediately as Approved
                var membership = new CooperativeGroupMember
                {
                    CooperativeGroupId = group.Id,
                    Trader = trader,
                    Status = ApprovalStatus.Approved,
                    ApprovedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<CooperativeGroupMember>().AddAsync(membership);

                // Save so we have an ID for the email sending if needed, but we'll defer to batch save.
                // However, EF Core handles tracking.
                
                // Send Email Notification
                if (isNewTrader)
                {
                    var subject = $"Welcome to AjoCore - You've been added to {group.Name}!";
                    var body = $@"
                        <h3>Hello {trader.FirstName},</h3>
                        <p>You have been added to the cooperative group <strong>{group.Name}</strong> on AjoCore.</p>
                        <p>An account has been automatically created for you. Your login details are:</p>
                        <ul>
                            <li><strong>Email:</strong> {trader.Email}</li>
                            <li><strong>Password:</strong> {generatedPassword}</li>
                        </ul>
                        <p><strong>IMPORTANT:</strong> For security and KYC purposes, you MUST log in to update your password and provide your BVN. You will not be able to participate in any saving cycles until your BVN is verified.</p>
                    ";
                    _ = _emailService.SendEmailAsync(trader.Email, subject, body);
                    results.Add($"{memberDto.Email}: New account created and added successfully.");
                }
                else
                {
                    var subject = $"You've been added to {group.Name} on AjoCore!";
                    var body = $@"
                        <h3>Hello {trader.FirstName},</h3>
                        <p>You have been directly added to the cooperative group <strong>{group.Name}</strong>.</p>
                        <p>Log in to your AjoCore dashboard to view your new group.</p>
                    ";
                    _ = _emailService.SendEmailAsync(trader.Email, subject, body);
                    results.Add($"{memberDto.Email}: Existing account added successfully.");
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return results;
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
