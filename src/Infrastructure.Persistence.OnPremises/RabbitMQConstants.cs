namespace Infrastructure.Persistence.OnPremises;

public static class RabbitMQConstants
{
    public const string ExchangeNameValidationExpression = "^[a-zA-Z0-9-_.:]+$";
    public const string QueueNameValidationExpression = "^[a-zA-Z0-9-_.:]+$";
}