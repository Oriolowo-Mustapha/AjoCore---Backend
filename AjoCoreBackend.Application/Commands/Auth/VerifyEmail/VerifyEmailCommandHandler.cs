using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public VerifyEmailCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());
            
            var trader = users.FirstOrDefault();

            if (trader == null || trader.EmailVerificationToken != request.Token)
            {
                throw new DomainException("Invalid verification token or email.");
            }

            if (trader.IsEmailVerified)
            {
                return true; // Already verified
            }

            trader.IsEmailVerified = true;
            trader.EmailVerificationToken = null;

            _unitOfWork.Repository<Trader>().Update(trader);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
