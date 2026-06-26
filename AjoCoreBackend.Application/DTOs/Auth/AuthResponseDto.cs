namespace AjoCoreBackend.Application.DTOs.Auth
{
    public record AuthResponseDto
    {
        public string Token { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public System.Guid UserId { get; init; }
        public string RefreshToken { get; init; } = string.Empty;
    }
}
