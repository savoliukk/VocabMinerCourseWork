using System.Security.Cryptography;
using System.Text;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;
using VocabMinerCourseWork.Api.Repositories;

namespace VocabMinerCourseWork.Api.BusinessLogic;

public interface IAuthService
{
    Task<UserResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse?> GetProfileAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserResponse?> GetProfileByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = HashPassword(request.Password),
            NativeLanguage = request.NativeLanguage.Trim().ToLowerInvariant(),
            TargetLanguage = request.TargetLanguage.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive || user.PasswordHash != HashPassword(request.Password))
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse?> GetProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse?> GetProfileByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return user is null ? null : ToResponse(user);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return _userRepository.EmailExistsAsync(email, cancellationToken);
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToHexString(sha256.ComputeHash(bytes)).ToLowerInvariant();
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            NativeLanguage = user.NativeLanguage,
            TargetLanguage = user.TargetLanguage,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        };
    }
}
