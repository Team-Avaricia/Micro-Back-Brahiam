using System;
using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Interfaces;

namespace Core.Application.Services
{
    public class TransactionService : ITransactionService
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
            Guid userId;

            // Try to parse as GUID first, if fails, treat as email
            if (!Guid.TryParse(request.UserId, out userId))
            {
                // UserId is an email, find the user
                var userByEmail = await _userRepository.GetByEmailAsync(request.UserId);
                if (userByEmail == null)
                {
                    throw new InvalidOperationException($"User with email '{request.UserId}' not found");
                }
                userId = userByEmail.Id;
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var transaction = new Transaction(
                userId,
                request.Amount,
                request.Type,
                request.Category,
                ColombiaTimeZone.Now,
                request.Source,
                request.Description
            );

            await _transactionRepository.AddAsync(transaction);

            var balanceChange = request.Type == TransactionType.Income ? request.Amount : -request.Amount;
            user.UpdateBalance(balanceChange);
            await _userRepository.UpdateAsync(user);

            return transaction;
        }
    }
}
