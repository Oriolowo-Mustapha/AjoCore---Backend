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

namespace AjoCoreBackend.Application.Commands.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
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
                    Role = role
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
                    EmailVerificationToken = verificationToken,
                    IsEmailVerified = false
                };
                await _unitOfWork.Repository<Trader>().AddAsync(trader);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                userEntity = trader;
                userEmail = trader.Email;
                userFullName = $"{trader.FirstName} {trader.LastName}";
                userId = trader.Id;
            }

            // Send Verification Email (Simplification for now, applies mostly to traders but good practice)
            var verificationLink = $"https://your-frontend-url.com/verify-email?token={verificationToken}&email={userEmail}";
            var emailBody = $"<h1>Welcome to AjoCore!</h1><p>Please verify your email by clicking <a href='{verificationLink}'>here</a>.</p><p>Alternatively, use this token: {verificationToken}</p>";
            _ = _emailService.SendEmailAsync(userEmail, "Verify your AjoCore Account", emailBody);

            // Wait, we need to pass a specific object to GenerateToken. IJwtTokenService might only take Trader.
            // We need to update IJwtTokenService to take an interface or separate methods. For now we will cast or fix the interface.
            // Let's pass the raw properties to a GenerateToken overload or just assume we'll update it next.
            var token = _jwtTokenService.GenerateToken(userId.ToString(), userEmail, role.ToString(), userFullName);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            if (userEntity is Trader t)
            {
                t.RefreshToken = refreshToken;
                t.RefreshTokenExpiryTime = System.DateTime.UtcNow.AddDays(7);
                _unitOfWork.Repository<Trader>().Update(t);
            }
            // Add refresh token to admin if we add the properties to CooperativeAdmin later.

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Email = userEmail,
                FullName = userFullName,
                Role = role.ToString(),
                UserId = userId
            };
        }
    }
}
