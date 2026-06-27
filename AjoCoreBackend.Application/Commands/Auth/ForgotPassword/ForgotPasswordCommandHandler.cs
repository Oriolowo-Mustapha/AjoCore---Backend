using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Repository<Trader>()
                .FindAsync(t => t.Email.ToLower() == request.Email.ToLower());
            var trader = users.FirstOrDefault();

            var admins = await _unitOfWork.Repository<CooperativeAdmin>()
                .FindAsync(a => a.Email.ToLower() == request.Email.ToLower());
            var admin = admins.FirstOrDefault();

            if (trader == null && admin == null)
            {
                throw new NotFoundException($"User with email {request.Email} was not found.");
            }

            var resetToken = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // 6 character code for simplicity
            
            if (trader != null)
            {
                trader.ResetPasswordToken = resetToken;
                trader.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15);
                _unitOfWork.Repository<Trader>().Update(trader);
            }
            else if (admin != null)
            {
                admin.ResetPasswordToken = resetToken;
                admin.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(15);
                _unitOfWork.Repository<CooperativeAdmin>().Update(admin);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var emailBody = $"<h1>Password Reset Request</h1><p>Your password reset code is: <strong>{resetToken}</strong></p><p>This code will expire in 15 minutes.</p>";
            
            _ = _emailService.SendEmailAsync(request.Email, "AjoCore Password Reset", emailBody);

            return resetToken;
        }
    }
}
