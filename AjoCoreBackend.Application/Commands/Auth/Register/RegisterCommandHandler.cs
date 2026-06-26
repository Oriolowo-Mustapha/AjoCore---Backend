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
            // Check for duplicate email
            var existingUsers = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());

            if (existingUsers.Any())
            {
                throw new DuplicateEmailException(request.Email);
            }

            var role = Enum.Parse<UserRole>(request.Role);
            var verificationToken = Guid.NewGuid().ToString("N");

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

            // Send Verification Email
            var verificationLink = $"https://your-frontend-url.com/verify-email?token={verificationToken}&email={trader.Email}";
            var emailBody = $"<h1>Welcome to AjoCore!</h1><p>Please verify your email by clicking <a href='{verificationLink}'>here</a>.</p><p>Alternatively, use this token: {verificationToken}</p>";
            
            // Fire and forget email sending to not block the API response
            _ = _emailService.SendEmailAsync(trader.Email, "Verify your AjoCore Account", emailBody);

            var token = _jwtTokenService.GenerateToken(trader);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            trader.RefreshToken = refreshToken;
            trader.RefreshTokenExpiryTime = System.DateTime.UtcNow.AddDays(7);
            _unitOfWork.Repository<Trader>().Update(trader);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Email = trader.Email,
                FullName = $"{trader.FirstName} {trader.LastName}",
                Role = trader.Role.ToString(),
                UserId = trader.Id
            };
        }
    }
}
