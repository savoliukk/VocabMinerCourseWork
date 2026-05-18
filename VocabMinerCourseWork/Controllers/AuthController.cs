using Microsoft.AspNetCore.Mvc;
using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Domains.ViewModels;

namespace VocabMinerCourseWork.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        if (response is null)
        {
            return Conflict("A user with this email already exists.");
        }

        return CreatedAtAction(nameof(GetProfile), new { id = response.Id }, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpGet("profile/{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetProfile(Guid id, CancellationToken cancellationToken)
    {
        var response = await _authService.GetProfileAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("health")]
    public ActionResult<ApiMessageResponse> Health()
    {
        return Ok(new ApiMessageResponse { Message = "Auth controller is available." });
    }

    [HttpGet("options/languages")]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> LanguageOptions()
    {
        return Ok(new[]
        {
            new EnumOptionResponse { Name = "uk", Value = 1 },
            new EnumOptionResponse { Name = "en", Value = 2 },
            new EnumOptionResponse { Name = "de", Value = 3 },
            new EnumOptionResponse { Name = "fr", Value = 4 }
        });
    }

    [HttpGet("options/defaults")]
    public ActionResult<RegisterDefaultsResponse> RegisterDefaults()
    {
        return Ok(new RegisterDefaultsResponse());
    }

    [HttpPost("check-email")]
    public async Task<ActionResult<EmailAvailabilityResponse>> CheckEmail(EmailAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var exists = await _authService.EmailExistsAsync(request.Email, cancellationToken);
        return Ok(new EmailAvailabilityResponse
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            IsAvailable = !exists
        });
    }

    [HttpPost("validate-register")]
    public ActionResult<ValidationResponse> ValidateRegister(RegisterRequest request)
    {
        return Ok(new ValidationResponse { IsValid = ModelState.IsValid });
    }

    [HttpPost("validate-login")]
    public ActionResult<ValidationResponse> ValidateLogin(LoginRequest request)
    {
        return Ok(new ValidationResponse { IsValid = ModelState.IsValid });
    }

    [HttpPost("profile/lookup")]
    public async Task<ActionResult<UserResponse>> LookupProfile(ProfileLookupRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.GetProfileByEmailAsync(request.Email, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("profile/by-email")]
    public async Task<ActionResult<UserResponse>> GetProfileByEmail([FromQuery] string email, CancellationToken cancellationToken)
    {
        var response = await _authService.GetProfileByEmailAsync(email, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("profile/{id:guid}/exists")]
    public async Task<ActionResult<ExistsResponse>> ProfileExists(Guid id, CancellationToken cancellationToken)
    {
        var response = await _authService.GetProfileAsync(id, cancellationToken);
        return Ok(new ExistsResponse { Id = id, Exists = response is not null });
    }

    [HttpGet("profile/{id:guid}/status")]
    public async Task<ActionResult<UserStatusResponse>> ProfileStatus(Guid id, CancellationToken cancellationToken)
    {
        var response = await _authService.GetProfileAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(new UserStatusResponse
        {
            UserId = response.Id,
            IsActive = response.IsActive,
            LastLoginAt = response.LastLoginAt
        });
    }

    [HttpGet("profile/{id:guid}/languages")]
    public async Task<ActionResult<UserLanguageResponse>> ProfileLanguages(Guid id, CancellationToken cancellationToken)
    {
        var response = await _authService.GetProfileAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(new UserLanguageResponse
        {
            UserId = response.Id,
            NativeLanguage = response.NativeLanguage,
            TargetLanguage = response.TargetLanguage
        });
    }

    [HttpGet("profile/{id:guid}/audit")]
    public async Task<ActionResult<UserAuditResponse>> ProfileAudit(Guid id, CancellationToken cancellationToken)
    {
        var response = await _authService.GetProfileAsync(id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(new UserAuditResponse
        {
            UserId = response.Id,
            CreatedAt = response.CreatedAt,
            LastLoginAt = response.LastLoginAt,
            IsActive = response.IsActive
        });
    }

    [HttpPost("logout")]
    public ActionResult<ApiMessageResponse> Logout()
    {
        return Ok(new ApiMessageResponse { Message = "Mock logout completed." });
    }

    [HttpPost("refresh-session")]
    public ActionResult<ApiMessageResponse> RefreshSession()
    {
        return Ok(new ApiMessageResponse { Message = "Mock session refreshed." });
    }

    [HttpGet("schema")]
    public ActionResult<ApiMessageResponse> Schema()
    {
        return Ok(new ApiMessageResponse { Message = "Auth API uses RegisterRequest, LoginRequest and UserResponse ViewModels." });
    }
}
