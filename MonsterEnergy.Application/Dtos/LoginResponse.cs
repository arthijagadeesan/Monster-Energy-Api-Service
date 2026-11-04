namespace MonsterEnergy.Application.Dtos
{
    public record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, DateTime ExpiresAt);
}
