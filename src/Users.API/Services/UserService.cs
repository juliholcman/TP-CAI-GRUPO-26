using System.Security.Cryptography;
using Users.API.DTOs.Requests;
using Users.API.DTOs.Responses;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services;

public class UserService
{
    private const int MaxFailedAttempts = 3;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private readonly List<User> _users = [];
    private readonly object _syncRoot = new();

    public UserResponse Register(RegisterUserRequest request)
    {
        ValidateRegisterRequest(request);
        var normalizedEmail = NormalizeEmail(request.Email);

        lock (_syncRoot)
        {
            if (_users.Any(user => string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)))
                throw new DuplicateEmailException();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Email = normalizedEmail,
                PasswordHash = HashPassword(request.Password),
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IntentosFallidos = 0,
                BloqueadoPorFraude = false
            };

            _users.Add(user);

            return ToUserResponse(user);
        }
    }

    public LoginResponse Login(LoginUserRequest request)
    {
        ValidateLoginRequest(request);
        var normalizedEmail = NormalizeEmail(request.Email);

        lock (_syncRoot)
        {
            var user = _users.FirstOrDefault(user =>
                string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

            if (user is null)
                throw new InvalidCredentialsException();

            if (user.BloqueadoPorFraude)
                throw new FraudBlockedException();

            if (!user.Activo)
                throw new UserLockedException();

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= MaxFailedAttempts)
                {
                    user.Activo = false;
                    throw new UserLockedException();
                }

                throw new InvalidCredentialsException();
            }

            user.IntentosFallidos = 0;

            return new LoginResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email
            };
        }
    }

    private static void ValidateRegisterRequest(RegisterUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Nombre))
            errors.Add("nombre es requerido");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            errors.Add("apellido es requerido");

        ValidateEmail(request.Email, errors);

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("password es requerido");

        ThrowValidationIfNeeded(errors);
    }

    private static void ValidateLoginRequest(LoginUserRequest request)
    {
        var errors = new List<string>();

        ValidateEmail(request.Email, errors);

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("password es requerido");

        ThrowValidationIfNeeded(errors);
    }

    private static void ValidateEmail(string? email, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("email es requerido");
            return;
        }

        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            errors.Add("email debe tener un formato válido");
    }

    private static void ThrowValidationIfNeeded(List<string> errors)
    {
        if (errors.Count > 0)
            throw new UserValidationException(string.Join("; ", errors));
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static UserResponse ToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');

        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
