using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());
            
            var trader = users.FirstOrDefault();

            if (trader == null)
            {
                // We shouldn't reveal if a user exists or not, but for a hackathon we might throw or just return a generic success message.
                // Let's just return a dummy token or throw NotFoundException.
                throw new NotFoundException($"Trader with email {request.Email} was not found.");
            }

            var resetToken = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // 6 character code for simplicity
            
            trader.ResetPasswordToken = resetToken;
            trader.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            _unitOfWork.Repository<Trader>().Update(trader);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // In a real application, you would send an email here.
            // For now, we will just return it so it can be seen in the API response (for testing).
            return resetToken;
        }
    }
}
