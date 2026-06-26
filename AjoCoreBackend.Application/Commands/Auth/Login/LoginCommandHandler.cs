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
            var users = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());
            
            var trader = users.FirstOrDefault();

            if (trader == null)
            {
                throw new InvalidCredentialsException();
            }

            if (!_passwordHasher.VerifyPassword(request.Password, trader.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            var token = _jwtTokenService.GenerateToken(trader);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            trader.RefreshToken = refreshToken;
            trader.RefreshTokenExpiryTime = System.DateTime.UtcNow.AddDays(7); // 7 days expiry

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
