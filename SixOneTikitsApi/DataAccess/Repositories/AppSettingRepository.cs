using System.Linq;
using SixOneTikitsApi.Models;

namespace SixOneTikitsApi.DataAccess.Repositories
{
    public class AppSettingRepository : BaseRepository<AppSetting>
    {
        public AppSetting Get(string name)
        {
            return DbSet.FirstOrDefault(q => q.Name == name);
        }
    }
}