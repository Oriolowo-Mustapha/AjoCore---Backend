using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());
            var trader = users.FirstOrDefault();

            var admins = await _unitOfWork.Repository<CooperativeAdmin>()
                .FindAsync(a => a.Email.ToLower() == request.Email.ToLower());
            var admin = admins.FirstOrDefault();

            if (trader != null)
            {
                if (trader.ResetPasswordToken != request.Token || 
                    trader.ResetPasswordTokenExpiry == null || 
                    trader.ResetPasswordTokenExpiry < DateTime.UtcNow)
                {
                    throw new DomainException("Invalid or expired reset token.");
                }

                trader.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                trader.ResetPasswordToken = null;
                trader.ResetPasswordTokenExpiry = null;

                _unitOfWork.Repository<Trader>().Update(trader);
            }
            else if (admin != null)
            {
                if (admin.ResetPasswordToken != request.Token || 
                    admin.ResetPasswordTokenExpiry == null || 
                    admin.ResetPasswordTokenExpiry < DateTime.UtcNow)
                {
                    throw new DomainException("Invalid or expired reset token.");
                }

                admin.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                admin.ResetPasswordToken = null;
                admin.ResetPasswordTokenExpiry = null;

                _unitOfWork.Repository<CooperativeAdmin>().Update(admin);
            }
            else
            {
                throw new DomainException("Invalid or expired reset token.");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
