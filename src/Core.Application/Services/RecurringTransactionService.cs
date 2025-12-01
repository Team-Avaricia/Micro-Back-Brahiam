using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Application.Services
{
    /// <summary>
    /// Servicio para gestionar transacciones recurrentes
    /// </summary>
    public class RecurringTransactionService
    {
        private readonly IRecurringTransactionRepository _recurringRepository;
        private readonly IUserRepository _userRepository;
        private readonly TransactionService _transactionService;
        private readonly ILogger<RecurringTransactionService> _logger;

        public RecurringTransactionService(
            IRecurringTransactionRepository recurringRepository,
            IUserRepository userRepository,
            TransactionService transactionService,
            ILogger<RecurringTransactionService> logger)
        {
            _recurringRepository = recurringRepository;
            _userRepository = userRepository;
            _transactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva transacción recurrente
        /// </summary>
        public async Task<RecurringTransaction> CreateAsync(CreateRecurringTransactionRequest request)
        {
            var userId = Guid.Parse(request.UserId);

            // Verificar que el usuario existe
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Intento de crear transacción recurrente para usuario inexistente: {UserId}", userId);
                throw new InvalidOperationException("Usuario no encontrado");
            }

            var recurring = new RecurringTransaction(
                userId,
                request.Amount,
                request.Type,
                request.Category,
                request.Description,
                request.Frequency,
                request.StartDate,
                request.EndDate,
                request.DayOfMonth,
                request.DayOfWeek
            );

            await _recurringRepository.AddAsync(recurring);
            
            _logger.LogInformation("Transacción recurrente creada: {RecurringId} para usuario {UserId}, Monto: {Amount}, Frecuencia: {Frequency}",
                recurring.Id, userId, request.Amount, request.Frequency);
            
            return recurring;
        }

        /// <summary>
        /// Obtiene todas las transacciones recurrentes de un usuario
        /// </summary>
        public async Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _recurringRepository.GetByUserIdAsync(userId);
        }

        /// <summary>
        /// Obtiene solo las transacciones recurrentes activas de un usuario
        /// </summary>
        public async Task<IEnumerable<RecurringTransaction>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _recurringRepository.GetActiveByUserIdAsync(userId);
        }

        /// <summary>
        /// Obtiene el resumen de cashflow mensual basado en transacciones recurrentes
        /// </summary>
        public async Task<CashflowResponse> GetCashflowAsync(Guid userId)
        {
            var activeRecurring = await _recurringRepository.GetActiveByUserIdAsync(userId);
            var recurringList = activeRecurring.ToList();

            var incomes = recurringList.Where(r => r.Type == TransactionType.Income).ToList();
            var expenses = recurringList.Where(r => r.Type == TransactionType.Expense).ToList();

            var totalMonthlyIncome = incomes
                .Where(r => r.Frequency == RecurrenceFrequency.Monthly)
                .Sum(r => r.Amount);

            var totalMonthlyExpenses = expenses
                .Where(r => r.Frequency == RecurrenceFrequency.Monthly)
                .Sum(r => r.Amount);

            return new CashflowResponse
            {
                TotalMonthlyIncome = totalMonthlyIncome,
                TotalMonthlyExpenses = totalMonthlyExpenses,
                NetMonthlyCashflow = totalMonthlyIncome - totalMonthlyExpenses,
                IncomeBreakdown = incomes
                    .GroupBy(r => r.Category)
                    .Select(g => new CashflowItem
                    {
                        Category = g.Key,
                        Amount = g.Sum(r => r.Amount)
                    }),
                ExpenseBreakdown = expenses
                    .GroupBy(r => r.Category)
                    .Select(g => new CashflowItem
                    {
                        Category = g.Key,
                        Amount = g.Sum(r => r.Amount)
                    })
            };
        }

        /// <summary>
        /// Actualiza una transacción recurrente
        /// </summary>
        public async Task UpdateAsync(Guid id, decimal? amount, string description, DateTime? endDate)
        {
            var recurring = await _recurringRepository.GetByIdAsync(id);
            if (recurring == null)
            {
                _logger.LogWarning("Intento de actualizar transacción recurrente inexistente: {RecurringId}", id);
                throw new InvalidOperationException("Transacción recurrente no encontrada");
            }

            recurring.Update(amount, description, endDate);
            await _recurringRepository.UpdateAsync(recurring);
            
            _logger.LogInformation("Transacción recurrente actualizada: {RecurringId}", id);
        }

        /// <summary>
        /// Activa/Desactiva una transacción recurrente
        /// </summary>
        public async Task ToggleActiveAsync(Guid id, bool isActive)
        {
            var recurring = await _recurringRepository.GetByIdAsync(id);
            if (recurring == null)
            {
                _logger.LogWarning("Intento de toggle transacción recurrente inexistente: {RecurringId}", id);
                throw new InvalidOperationException("Transacción recurrente no encontrada");
            }

            if (isActive)
                recurring.Activate();
            else
                recurring.Deactivate();

            await _recurringRepository.UpdateAsync(recurring);
            
            _logger.LogInformation("Transacción recurrente {Status}: {RecurringId}", 
                isActive ? "activada" : "pausada", id);
        }

        /// <summary>
        /// Elimina una transacción recurrente
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            await _recurringRepository.DeleteAsync(id);
            _logger.LogInformation("Transacción recurrente eliminada: {RecurringId}", id);
        }

        /// <summary>
        /// Procesa todas las transacciones recurrentes que deben ejecutarse hoy
        /// (Este método es llamado por el Background Job)
        /// </summary>
        public async Task ProcessDueTransactionsAsync()
        {
            _logger.LogInformation("Iniciando procesamiento de transacciones recurrentes");
            
            var dueTransactions = await _recurringRepository.GetDueTransactionsAsync();
            var dueList = dueTransactions.ToList();
            
            _logger.LogInformation("Encontradas {Count} transacciones recurrentes para procesar", dueList.Count);

            int processed = 0;
            int failed = 0;

            foreach (var recurring in dueList)
            {
                try
                {
                    // Crear la transacción real
                    await _transactionService.CreateTransactionAsync(new CreateTransactionRequest
                    {
                        UserId = recurring.UserId.ToString(),
                        Amount = recurring.Amount,
                        Type = recurring.Type,
                        Category = recurring.Category,
                        Description = $"{recurring.Description} (Recurrente)",
                        Source = TransactionSource.Automatic
                    });

                    // Actualizar la próxima fecha de ejecución
                    recurring.UpdateNextExecutionDate();
                    await _recurringRepository.UpdateAsync(recurring);
                    
                    processed++;
                    
                    _logger.LogInformation("Transacción recurrente procesada: {RecurringId}, Usuario: {UserId}, Monto: {Amount}",
                        recurring.Id, recurring.UserId, recurring.Amount);
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogError(ex, "Error al procesar transacción recurrente: {RecurringId}", recurring.Id);
                    continue;
                }
            }
            
            _logger.LogInformation("Procesamiento completado. Exitosas: {Processed}, Fallidas: {Failed}", 
                processed, failed);
        }
    }
}
