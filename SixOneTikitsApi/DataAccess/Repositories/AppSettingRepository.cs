using System.Linq;
using PianoBarApi.Models;

namespace PianoBarApi.DataAccess.Repositories
{
    public class AppSettingRepository : BaseRepository<AppSetting>
    {
        public AppSetting Get(string name)
        {
            return DbSet.FirstOrDefault(q => q.Name == name);
        }
    }
}