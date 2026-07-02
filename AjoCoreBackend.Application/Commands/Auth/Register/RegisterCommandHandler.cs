using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Auth;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.Application.Commands.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.Logging.ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            Microsoft.Extensions.Logging.ILogger<RegisterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate email in BOTH tables
            var existingTraders = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());
            var existingAdmins = await _unitOfWork.Repository<CooperativeAdmin>()
                .FindAsync(a => a.Email.ToLower() == request.Email.ToLower());

            if (existingTraders.Any() || existingAdmins.Any())
            {
                throw new DuplicateEmailException(request.Email);
            }

            // Check for duplicate phone number in BOTH tables
            var tradersWithPhone = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.PhoneNumber == request.PhoneNumber);
            var adminsWithPhone = await _unitOfWork.Repository<CooperativeAdmin>()
                .FindAsync(a => a.PhoneNumber == request.PhoneNumber);

            if (tradersWithPhone.Any() || adminsWithPhone.Any())
            {
                _logger.LogWarning("Registration failed: Phone number {PhoneNumber} is already in use by another user.", request.PhoneNumber);
                throw new DuplicatePhoneNumberException(request.PhoneNumber);
            }

            var role = Enum.Parse<UserRole>(request.Role);
            var verificationToken = Guid.NewGuid().ToString("N");
            
            BaseEntity userEntity;
            string userEmail;
            string userFullName;
            Guid userId;

            if (role == UserRole.CooperativeAdmin)
            {
                var admin = new CooperativeAdmin
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email.ToLower(),
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    Role = role,
                    EmailVerificationToken = verificationToken,
                    IsEmailVerified = false
                };
                await _unitOfWork.Repository<CooperativeAdmin>().AddAsync(admin);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                userEntity = admin;
                userEmail = admin.Email;
                userFullName = $"{admin.FirstName} {admin.LastName}";
                userId = admin.Id;
            }
            else
            {
                var trader = new Trader
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email.ToLower(),
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    Role = role,
                    DateOfBirth = request.DateOfBirth,
                    EmailVerificationToken = verificationToken,
                    IsEmailVerified = false,
                    Bvn = request.Bvn,
                    PayoutAccountNumber = request.PayoutAccountNumber,
                    PayoutBankName = request.PayoutBankName,
                    PayoutAccountName = request.PayoutAccountName
                };
                await _unitOfWork.Repository<Trader>().AddAsync(trader);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                userEntity = trader;
                userEmail = trader.Email;
                userFullName = $"{trader.FirstName} {trader.LastName}";
                userId = trader.Id;
            }

            // Send Verification Email
            var verificationLink = $"https://your-frontend-url.com/verify-email?token={verificationToken}&email={userEmail}";
            var emailBody = $"<h1>Welcome to AjoCore!</h1><p>Please verify your email by clicking <a href='{verificationLink}'>here</a>.</p><p>Alternatively, use this token: {verificationToken}</p>";
            _ = _emailService.SendEmailAsync(userEmail, "Verify your AjoCore Account", emailBody);

            return "Registration successful. Please check your email to verify your account.";
        }
    }
}
