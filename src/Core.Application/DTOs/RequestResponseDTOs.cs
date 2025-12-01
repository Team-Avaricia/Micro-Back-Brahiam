using Core.Domain.Enums;

namespace Core.Application.DTOs
{
    /// <summary>
    /// Request del MS AI Worker para validar un gasto
    /// </summary>
    public class SpendingValidationRequest
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Respuesta del MS Core al MS AI Worker con el veredicto
    /// </summary>
    public class SpendingValidationResponse
    {
        public bool IsApproved { get; set; }
        public string Verdict { get; set; } // "Aprobado" o "Rechazado"
        public string Reason { get; set; } // Explicación del veredicto
        public decimal RemainingBudget { get; set; }
    }

    /// <summary>
    /// Request para registrar una transacción
    /// </summary>
    public class CreateTransactionRequest
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public TransactionSource Source { get; set; }
    }

    /// <summary>
    /// Request para crear una regla financiera
    /// </summary>
    public class CreateFinancialRuleRequest
    {
        public string UserId { get; set; }
        public RuleType Type { get; set; }
        public string Category { get; set; }
        public decimal AmountLimit { get; set; }
        public RulePeriod Period { get; set; }
    }

    /// <summary>
    /// Response para consultas de transacciones con totales
    /// </summary>
    public class TransactionQueryResponse
    {
        public IEnumerable<object> Data { get; set; }
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Response para balance del usuario
    /// </summary>
    public class BalanceResponse
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }

    /// <summary>
    /// Response para resumen por categoría
    /// </summary>
    public class CategorySummaryResponse
    {
        public IEnumerable<CategorySummaryItem> Data { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class CategorySummaryItem
    {
        public string Category { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Request para crear una transacción recurrente
    /// </summary>
    public class CreateRecurringTransactionRequest
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public RecurrenceFrequency Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DayOfMonth { get; set; }
        public int? DayOfWeek { get; set; }
    }

    /// <summary>
    /// Response para cashflow mensual
    /// </summary>
    public class CashflowResponse
    {
        public decimal TotalMonthlyIncome { get; set; }
        public decimal TotalMonthlyExpenses { get; set; }
        public decimal NetMonthlyCashflow { get; set; }
        public IEnumerable<CashflowItem> IncomeBreakdown { get; set; }
        public IEnumerable<CashflowItem> ExpenseBreakdown { get; set; }
    }

    public class CashflowItem
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
    }
}
