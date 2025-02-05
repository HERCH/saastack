using System.Text.RegularExpressions;
using Common.Extensions;

namespace Infrastructure.Persistence.OnPremises.Extensions;

public static class RabbitMQValidationExtensions
{
    public static string SanitizeAndValidateTopicName(this string name)
    {
        var sanitized = SanitizeName(name);
        ValidateRabbitMQExchangeName(sanitized);
        return sanitized;
    }

    public static string SanitizeAndValidateSubscriptionName(this string name)
    {
        var sanitized = SanitizeName(name);
        ValidateRabbitMQQueueName(sanitized);
        return sanitized;
    }

    private static string SanitizeName(string name)
    {
        return Regex.Replace(name, "[^a-zA-Z0-9-_.:]", "_");
    }

    private static void ValidateRabbitMQExchangeName(string name)
    {
        if (name.Length > 255)
        {
            throw new ArgumentOutOfRangeException(
                Resources.ValidationExtensions_InvalidRabbitMQExchangeName.Format(name));
        }

        if (!name.IsMatchWith(RabbitMQConstants.ExchangeNameValidationExpression))
        {
            throw new ArgumentOutOfRangeException(
                Resources.ValidationExtensions_InvalidRabbitMQExchangeName.Format(name));
        }
    }

    private static void ValidateRabbitMQQueueName(string name)
    {
        if (name.Length > 255)
        {
            throw new ArgumentOutOfRangeException(
                Resources.ValidationExtensions_InvalidRabbitMQQueueName.Format(name));
        }

        if (!name.IsMatchWith(RabbitMQConstants.QueueNameValidationExpression))
        {
            throw new ArgumentOutOfRangeException(
                Resources.ValidationExtensions_InvalidRabbitMQQueueName.Format(name));
        }
    }
}