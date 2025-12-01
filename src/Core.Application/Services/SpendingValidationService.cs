using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;

namespace Core.Application.Services
{
    /// <summary>
    /// Motor de Reglas: Valida si un gasto es permitido según las reglas del usuario
    /// </summary>
    public class SpendingValidationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFinancialRuleRepository _ruleRepository;
        private readonly ITransactionRepository _transactionRepository;

        public SpendingValidationService(
            IUserRepository userRepository,
            IFinancialRuleRepository ruleRepository,
            ITransactionRepository transactionRepository)
        {
            _userRepository = userRepository;
            _ruleRepository = ruleRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<SpendingValidationResponse> ValidateSpendingAsync(SpendingValidationRequest request)
        {
            var userId = Guid.Parse(request.UserId);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return new SpendingValidationResponse
                {
                    IsApproved = false,
                    Verdict = "Rechazado",
                    Reason = "Usuario no encontrado"
                };
            }

            // 1. Verificar saldo disponible
            if (user.CurrentBalance < request.Amount)
            {
                return new SpendingValidationResponse
                {
                    IsApproved = false,
                    Verdict = "Rechazado",
                    Reason = $"Saldo insuficiente. Disponible: ${user.CurrentBalance:N0}, Requerido: ${request.Amount:N0}",
                    RemainingBudget = user.CurrentBalance
                };
            }

            // 2. Verificar reglas activas
            var activeRules = await _ruleRepository.GetActiveRulesByUserIdAsync(userId);
            
            foreach (var rule in activeRules)
            {
                // Solo validar reglas que apliquen a la categoría o sean globales
                if (!string.IsNullOrEmpty(rule.Category) && rule.Category != request.Category)
                    continue;

                var (startDate, endDate) = GetPeriodDates(rule.Period);
                var transactionsInPeriod = await _transactionRepository.GetByUserAndPeriodAsync(
                    userId, startDate, endDate, rule.Category);

                var totalSpentInPeriod = transactionsInPeriod
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                var projectedTotal = totalSpentInPeriod + request.Amount;

                if (projectedTotal > rule.AmountLimit)
                {
                    var remaining = rule.AmountLimit - totalSpentInPeriod;
                    return new SpendingValidationResponse
                    {
                        IsApproved = false,
                        Verdict = "Rechazado",
                        Reason = $"Excedes el límite {GetPeriodName(rule.Period)} de {rule.Category ?? "general"}: ${rule.AmountLimit:N0}. Ya gastaste ${totalSpentInPeriod:N0}, te quedan ${remaining:N0}",
                        RemainingBudget = remaining
                    };
                }
            }

            // 3. Si pasa todas las validaciones, aprobar
            return new SpendingValidationResponse
            {
                IsApproved = true,
                Verdict = "Aprobado",
                Reason = $"Gasto permitido. Saldo después: ${user.CurrentBalance - request.Amount:N0}",
                RemainingBudget = user.CurrentBalance - request.Amount
            };
        }

        private (DateTime start, DateTime end) GetPeriodDates(RulePeriod period)
        {
            var now = DateTime.UtcNow;
            return period switch
            {
                RulePeriod.Daily => (now.Date, now.Date.AddDays(1)),
                RulePeriod.Weekly => (now.Date.AddDays(-(int)now.DayOfWeek), now.Date.AddDays(7 - (int)now.DayOfWeek)),
                RulePeriod.Monthly => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1)),
                RulePeriod.Yearly => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1)),
                _ => (now.Date, now.Date.AddDays(1))
            };
        }

        private string GetPeriodName(RulePeriod period)
        {
            return period switch
            {
                RulePeriod.Daily => "diario",
                RulePeriod.Weekly => "semanal",
                RulePeriod.Monthly => "mensual",
                RulePeriod.Yearly => "anual",
                _ => "desconocido"
            };
        }
    }
}
