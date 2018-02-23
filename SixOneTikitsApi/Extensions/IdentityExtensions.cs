using System.Security.Principal;
using System.Threading.Tasks;
using PianoBarApi.DataAccess.Repositories;
using PianoBarApi.Models;

namespace PianoBarApi.Extensions
{
    public static class IdentityExtensions
    {
        public static async Task<User> AsAppUser(this IIdentity identity)
        {
            var user = new UserRepository().Get(identity.Name);
            return await Task.FromResult(user);
        }
    }
}