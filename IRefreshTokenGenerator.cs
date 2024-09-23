namespace BusBooking.Server
{
    public interface IRefreshTokenGenerator
    {
        string GenerateToken(string Email);
    }
}


