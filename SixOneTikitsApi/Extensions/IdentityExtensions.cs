using System.Security.Principal;
using System.Threading.Tasks;
using SixOneTikitsApi.DataAccess.Repositories;
using SixOneTikitsApi.Models;

namespace SixOneTikitsApi.Extensions
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