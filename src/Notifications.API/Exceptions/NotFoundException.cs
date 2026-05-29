namespace Notifications.API.Exceptions
{
    // Esta clase permite lanzar errores 404 con un código específico [cite: 372]
    public class NotFoundException(string errorCode, string message) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode; // Aquí guardamos el "NTF-001", etc. [cite: 374]
    }
}