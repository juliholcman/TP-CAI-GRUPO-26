namespace Notifications.API.Exceptions
{
    // Esta clase es para errores de lógica (ej: datos inválidos NTF-002) [cite: 376]
    public class BusinessRuleException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}