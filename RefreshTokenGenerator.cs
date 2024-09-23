using BusBooking.Server.Models;
using System.Security.Cryptography;

namespace BusBooking.Server
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private readonly BusBookingContext context;
        public RefreshTokenGenerator(BusBookingContext db)
        {
            context = db;
        }
        public string GenerateToken(string Email)
        {
            var randomnumber = new byte[32];
            using (var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string RefreshToken = Convert.ToBase64String(randomnumber);

                var _user = context.TblRefreshtokens.FirstOrDefault(o => o.Email == Email);
                if (_user != null)
                {
                    _user.RefreshToken = RefreshToken;
                    context.SaveChanges();
                }
                else
                {
                    TblRefreshtoken tblRefreshtoken = new TblRefreshtoken()
                    {
                        Email = Email,
                        TokenId = new Random().Next().ToString(),
                        RefreshToken = RefreshToken,
                        IsActive = true
                    };
                }

                return RefreshToken;
            }
        }
    }
}
