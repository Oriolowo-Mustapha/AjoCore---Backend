using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Auth;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            BaseEntity userEntity = null;
            string userEmail = "";
            string userFullName = "";
            string userPasswordHash = "";
            System.Guid userId = System.Guid.Empty;
            Domain.Enum.UserRole userRole = Domain.Enum.UserRole.Trader;

            var traders = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());
            var trader = traders.FirstOrDefault();

            if (trader != null)
            {
                userEntity = trader;
                userEmail = trader.Email;
                userFullName = $"{trader.FirstName} {trader.LastName}";
                userPasswordHash = trader.PasswordHash;
                userId = trader.Id;
                userRole = trader.Role;
            }
            else
            {
                var admins = await _unitOfWork.Repository<CooperativeAdmin>()
                    .FindAsync(a => a.Email.ToLower() == request.Email.ToLower());
                var admin = admins.FirstOrDefault();

                if (admin != null)
                {
                    userEntity = admin;
                    userEmail = admin.Email;
                    userFullName = $"{admin.FirstName} {admin.LastName}";
                    userPasswordHash = admin.PasswordHash;
                    userId = admin.Id;
                    userRole = admin.Role;
                }
            }

            if (userEntity == null)
            {
                throw new InvalidCredentialsException();
            }

            if (!_passwordHasher.VerifyPassword(request.Password, userPasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            var token = _jwtTokenService.GenerateToken(userId.ToString(), userEmail, userRole.ToString(), userFullName);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            if (userEntity is Trader t)
            {
                t.RefreshToken = refreshToken;
                t.RefreshTokenExpiryTime = System.DateTime.UtcNow.AddDays(7);
                _unitOfWork.Repository<Trader>().Update(t);
            }
            else if (userEntity is CooperativeAdmin a)
            {
                // To support refresh tokens for admins, we need those fields on CooperativeAdmin.
                // For now, we will skip saving it or we can add it later if needed.
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Email = userEmail,
                FullName = userFullName,
                Role = userRole.ToString(),
                UserId = userId
            };
        }
    }
}
