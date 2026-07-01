using AjoCoreBackend.Application.DTOs.Auth;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AjoCoreBackend.Application.Commands.Auth.AdminLogin
{
    public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, AuthResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AdminLoginCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<AuthResponseDto> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
        {
            var adminQuery = await _unitOfWork.Repository<SystemAdmin>().FindAsync(a => 
                a.Email == request.EmailOrUsername || a.Username == request.EmailOrUsername);
            
            var admin = adminQuery.FirstOrDefault();

            if (admin == null)
            {
                throw new InvalidCredentialsException();
            }

            if (!_passwordHasher.VerifyPassword(request.Password, admin.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            var token = _jwtTokenService.GenerateToken(
                admin.Id.ToString(), 
                admin.Email, 
                admin.Role.ToString(), 
                $"{admin.FirstName} {admin.LastName}");

            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            admin.RefreshToken = refreshToken;
            admin.RefreshTokenExpiryTime = _dateTimeProvider.UtcNow.AddDays(7);

            _unitOfWork.Repository<SystemAdmin>().Update(admin);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Email = admin.Email,
                FullName = $"{admin.FirstName} {admin.LastName}",
                Role = admin.Role.ToString(),
                UserId = admin.Id
            };
        }
    }
}
