namespace Core.Domain.Enums
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public enum TransactionSource
    {
        Manual,
        Telegram,
        WhatsApp,
        Automatic // From Email/n8n
    }

    public enum RuleType
    {
        SpendingLimit,
        SavingsGoal,
        CategoryBudget
    }

    public enum RulePeriod
    {
        Daily,
        Weekly,
        Biweekly,
        Monthly,
        Yearly
    }
}
