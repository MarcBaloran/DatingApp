using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;

        }

        /*One way of verifying user entered credentials START*/
        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync( x => x.Username == username);

            if(user == null)
              return null;

            /*Method to verify the hash password*/
            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
              return null;

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            /*Enter the password salt as it is the key*/
            using( var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)) {
                
                /*Creates a hash password*/
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                /*Loop to compare each by of the password*/
                for(int i = 0; i < computedHash.Length; i++ ){
                  if(computedHash[i] != passwordHash[i]) return false;
                }
            }
            /*return true if password hash is correct*/
            return true;
        }
        /*One way of verifying user entered credentials END*/


        /*One way of create hashed password START*/
        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;

            /*Youtube link for out/ref keyword use: https://www.youtube.com/watch?v=ZcouA7mu2aQ */
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            /*Using the using keyword will disposed of the object instance once it is done processing. */
            /*Youtube link for using keyword use: https://www.youtube.com/watch?v=Dxbbtx-8MKw*/
            using( var hmac = new System.Security.Cryptography.HMACSHA512()) {
                
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        /*One way of create hashed password END*/

        /*  */
        public  async Task<bool> UserExists(string username)
        {
            if(await _context.Users.AnyAsync(x => x.Username == username))
              return true;

            return false;
        }

    }
}