using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;


public class ReversalProcessingService : IReversalProcessingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INombaApiClient _nombaApiClient;
    private readonly IBankCodeService _bankCodeService;
    private readonly ILogger<ReversalProcessingService> _logger;
    private readonly IEmailService _emailService;

    public ReversalProcessingService(
        IUnitOfWork unitOfWork,
        INombaApiClient nombaApiClient,
        IBankCodeService bankCodeService,
        ILogger<ReversalProcessingService> logger,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _nombaApiClient = nombaApiClient;
        _bankCodeService = bankCodeService;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task ProcessPendingReversalsAsync(Guid reversalLedgerId)
    {
        // 1. Get all pending reversals
        var reversal = await _unitOfWork.Repository<ReversalLedger>().GetByIdAsync(reversalLedgerId);

        
            try
            {
                var member = await _unitOfWork.SavingCycleMembers.GetByIdAsync(reversal.SavingCycleMemberId,x => x.Cycle);
                var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);

                if (string.IsNullOrEmpty(trader.PayoutAccountNumber) || string.IsNullOrEmpty(trader.PayoutBankName))
                {
                    reversal.Status = TransactionStatus.Failed;
                    reversal.FailureReason = "User has no registered personal bank account.";
                throw new UserBankDetailNotFoundExcepion(trader.Id, $"{trader.FirstName} {trader.LastName}");
                }


            var actualBankCode = await _bankCodeService.GetBankCodeByNameAsync(trader.PayoutBankName);

            var lookupRequest = new BankLookupRequest
            {
                AccountNumber = trader.PayoutAccountNumber,
                BankCode = actualBankCode
            };
            // 2. Perform Account Validation (Lookup)
            BankLookupResponse lookupResponse = null;
            try
            {
                lookupResponse = await _nombaApiClient.LookupBankAccountAsync(lookupRequest);
                if (string.IsNullOrWhiteSpace(lookupResponse.AccountName))
                {
                    _logger.LogWarning($"Bank lookup failed for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while looking up bank account for Account {lookupRequest.AccountNumber}, BankCode {lookupRequest.BankCode}");
                return;
            }


            // 3. Execute Transfer (Same as ASCA/ROSCA payout logic)
            var transferResponse = await _nombaApiClient.ExecuteBankTransferAsync(new BankTransferRequest
                      {
                          Amount = reversal.Amount,
                          AccountNumber = trader.PayoutAccountNumber,
                          AccountName = lookupResponse.AccountName,
                          BankCode = lookupRequest.BankCode,
                          MerchantTxRef = reversal.ReversalTxRef,
                          SenderName = "AjoCore Reversal"
                      });

                if (transferResponse != null)
                {
                    reversal.Status = TransactionStatus.Success;
                    reversal.CompletedAt = DateTime.UtcNow;
                await _emailService.SendEmailAsync(trader.Email, $"AjoCore: Unsuccessful Contribution Refund - {member.Cycle.Name}",ReversalBody(new ReversalEmailData(trader.FirstName,trader.LastName,member.Cycle.Name,member.Cycle.ContributionAmount,reversal.Amount,trader.PayoutAccountName,trader.PayoutAccountNumber)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process reversal: { reversal.ReversalTxRef}");
                    reversal.Status = TransactionStatus.Failed;
                reversal.FailureReason = ex.Message;
            }
       
        await _unitOfWork.SaveChangesAsync(default);
    }
    private string ReversalBody(ReversalEmailData model) => $"""
         <!DOCTYPE html>
        <html>
        <head>
            <meta charset = "utf-8" >
            < meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Contribution Refund Notice</title>
        </head>
        <body style = "font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f6f9fc; margin: 0;padding: 0; -webkit-font-smoothing: antialiased;">
            <table border = "0" cellpadding="0" cellspacing="0" width="100%" style="background-color: #f6f9fc; padding: 20px 0;">
                <tr>
                    <td align = "center" >
                        < table border="0" cellpadding="0" cellspacing="0" width="600" style="background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);">
                            <!-- Header -->
                            <tr>
                                <td align = "center" style="background-color: #e53e3e; padding: 30px 20px;">
                                    <h1 style = "color: #ffffff; margin: 0; font-size: 24px; font-weight: 600; letter-spacing: 0.5px;">Refund Initiated</h1>
                                </td>
                            </tr>

                            <!-- Content -->
                            <tr>
                                <td style = "padding: 40px 30px; color: #4a5568; line-height: 1.6;" >
                                    < p style="font-size: 16px; margin-top: 0;">Hello {model.FirstName} {model.LastName},</p>

                                    <p style = "font-size: 15px;" > We received a transfer into your dedicated virtual account for the cycle <strong>{model.CycleName}</strong>, but the amount did not match the required contribution amount.</p>

                                    <!-- Transaction Details Box -->
                                    <table border = "0" cellpadding= "10" cellspacing= "0" width= "100%" style= "background-color: #f7fafc; border: 1px solid #edf2f7; border-radius: 6px; margin: 25px 0; font-size: 14px;">
                                        <tr>
                                            <td width = "50%" style= "color: #718096; font-weight: 500;" > Required Contribution:</td>
                                            <td width = "50%" style= "color: #2d3748; font-weight: 600; text-align: right;">₦{model.RequiredAmount}</td>
                                        </tr>
                                        <tr>
                                            <td width = "50%" style= "color: #718096; font-weight: 500;" > Amount Transferred:</td>
                                            <td width = "50%" style= "color: #e53e3e; font-weight: 600; text-align: right;">₦{model.AmountReceived}</td>
                                        </tr>
                                        <tr>
                                            <td colspan = "2" style= "border-top: 1px solid #edf2f7; padding: 0;" ></ td >
                                        </ tr >
                                        < tr >
                                            < td width= "50%" style= "color: #718096; font-weight: 500; padding-top: 15px;">Refund Destination:</td>
                                            <td width = "50%" style= "color: #2d3748; font-weight: 600; text-align: right; padding-top: 15px;">{model.BankName} ({model.MaskedAccountNumber})</td>
                                        </tr>
                                        <tr>
                                            <td width = "50%" style="color: #718096; font-weight: 500;">Refund Status:</td>
                                            <td width = "50%" style="color: #3182ce; font-weight: 600; text-align: right;">Processing</td>
                                        </tr>
                                    </table>

                                    <p style = "font-size: 15px;" > To maintain the integrity of the savings rotation, we have initiated an automatic reversal.The funds will be credited back to your registered personal account shortly.</p>

                                    <p style = "font-size: 14px; color: #718096; font-style: italic; margin-top: 15px;">Note: Depending on your bank, it may take up to 24 hours for the reversal to reflect in your bank statement.</p>
                                </td>
                            </tr>

                            <!-- Footer -->
                            <tr>
                                <td style = "background-color: #edf2f7; padding: 20px 30px; text-align: center; font-size:12px; color: #a0aec0; border-top: 1px solid #e2e8f0;">
                                    <p style = "margin: 0 0 5px 0;" > This is an automated security notification from AjoCore.</p>
                                    <p style = "margin: 0;" > If you have any questions or did not authorize this, please contact support @ajocore.com</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    public record ReversalEmailData(
    string FirstName,
    string LastName,
    string CycleName,
    decimal RequiredAmount,
    decimal AmountReceived,
    string BankName,
    string MaskedAccountNumber);
}