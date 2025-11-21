namespace TestAPI.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string username);
    }
}
