using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Auth;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;

        public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
            {
                throw new InvalidCredentialsException();
            }

            var userIdString = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                throw new InvalidCredentialsException();
            }

            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId);

            if (trader == null || trader.RefreshToken != request.RefreshToken || trader.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new InvalidCredentialsException();
            }

            var newAccessToken = _jwtTokenService.GenerateToken(trader);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            trader.RefreshToken = newRefreshToken;
            trader.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _unitOfWork.Repository<Trader>().Update(trader);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Email = trader.Email,
                FullName = $"{trader.FirstName} {trader.LastName}",
                Role = trader.Role.ToString(),
                UserId = trader.Id
            };
        }
    }
}
