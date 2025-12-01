using System;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;

namespace Core.Application.Services
{
    /// <summary>
    /// Servicio para gestionar transacciones y actualizar saldos automáticamente
    /// </summary>
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;

        public TransactionService(ITransactionRepository transactionRepository, IUserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<Transaction> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var userId = Guid.Parse(request.UserId);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("Usuario no encontrado");
            }

            // Crear la transacción
            var transaction = new Transaction(
                userId,
                request.Amount,
                request.Type,
                request.Category,
                DateTime.UtcNow,
                request.Source,
                request.Description
            );

            await _transactionRepository.AddAsync(transaction);

            // Actualizar el saldo del usuario
            var balanceChange = request.Type == TransactionType.Income ? request.Amount : -request.Amount;
            user.UpdateBalance(balanceChange);
            await _userRepository.UpdateAsync(user);

            return transaction;
        }
    }
}
