using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class BankCodeService : IBankCodeService
    {
        private readonly INombaApiClient _nombaApiClient;
        private readonly ILogger<BankCodeService> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        private Dictionary<string, string>? _bankCodesCache;

        public BankCodeService(INombaApiClient nombaApiClient, ILogger<BankCodeService> logger)
        {
            _nombaApiClient = nombaApiClient;
            _logger = logger;
        }

        public async Task<string> GetBankCodeByNameAsync(string bankName)
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                return string.Empty;
            }

            await EnsureCacheInitializedAsync();

            var normalizedSearchName = NormalizeBankName(bankName);

            if (_bankCodesCache != null && _bankCodesCache.TryGetValue(normalizedSearchName, out var code))
            {
                return code;
            }

            // Fallback: try finding the first cache key that contains the search name or vice versa
            if (_bankCodesCache != null)
            {
                var fuzzyMatch = _bankCodesCache.FirstOrDefault(kvp => 
                    kvp.Key.Contains(normalizedSearchName) || normalizedSearchName.Contains(kvp.Key));
                    
                if (!string.IsNullOrEmpty(fuzzyMatch.Value))
                {
                    return fuzzyMatch.Value;
                }
            }

            _logger.LogWarning($"Could not find a bank code mapping for bank name: '{bankName}'");
            return string.Empty;
        }

        private async Task EnsureCacheInitializedAsync()
        {
            if (_bankCodesCache != null)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                // Double check pattern
                if (_bankCodesCache != null)
                {
                    return;
                }

                _logger.LogInformation("Initializing bank codes cache from Nomba API.");
                var response = await _nombaApiClient.FetchBanksAsync();
                
                _bankCodesCache = new Dictionary<string, string>();
                
                foreach (var bank in response.Data)
                {
                    if (!string.IsNullOrWhiteSpace(bank.Name) && !string.IsNullOrWhiteSpace(bank.Code))
                    {
                        var normalizedName = NormalizeBankName(bank.Name);
                        // Prevent duplicates if API returns any
                        if (!_bankCodesCache.ContainsKey(normalizedName))
                        {
                            _bankCodesCache.Add(normalizedName, bank.Code);
                        }
                    }
                }
                
                _logger.LogInformation($"Successfully cached {_bankCodesCache.Count} bank codes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize bank codes cache.");
                throw; // Rethrow to let caller handle it, or we could leave cache null to retry later
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string NormalizeBankName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            
            var normalized = name.ToLowerInvariant().Trim();
            
            // Strip common corporate suffixes that might cause mismatches
            var suffixesToStrip = new[] { " plc", " ltd", " limited", " bank" };
            foreach (var suffix in suffixesToStrip)
            {
                if (normalized.EndsWith(suffix))
                {
                    normalized = normalized.Substring(0, normalized.Length - suffix.Length).Trim();
                }
            }
            
            return normalized;
        }
    }
}
