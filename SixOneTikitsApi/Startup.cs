using System;
using System.IO;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using SixOneTikitsApi;

[assembly: OwinStartup(typeof(Startup))]

namespace SixOneTikitsApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LoadSettings();
            //app.MapSignalR();
            ConfigureAuth(app);
        }

        private static void LoadSettings()
        {
            SetupConfig.Setting = JsonConvert.DeserializeObject<Setting>
                (File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetupConfig.json")));
        }
    }
}
