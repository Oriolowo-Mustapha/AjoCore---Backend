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

            var admins = await _unitOfWork.Repository<CooperativeAdmin>()
                .FindAsync(a => a.Email.ToLower() == request.Email.ToLower());
            var admin = admins.FirstOrDefault();

            if (trader != null)
            {
                if (trader.EmailVerificationToken != request.Token)
                {
                    throw new DomainException("Invalid verification token or email.");
                }

                if (trader.IsEmailVerified) return true;

                trader.IsEmailVerified = true;
                trader.EmailVerificationToken = null;

                _unitOfWork.Repository<Trader>().Update(trader);
            }
            else if (admin != null)
            {
                if (admin.EmailVerificationToken != request.Token)
                {
                    throw new DomainException("Invalid verification token or email.");
                }

                if (admin.IsEmailVerified) return true;

                admin.IsEmailVerified = true;
                admin.EmailVerificationToken = null;

                _unitOfWork.Repository<CooperativeAdmin>().Update(admin);
            }
            else
            {
                throw new DomainException("Invalid verification token or email.");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
