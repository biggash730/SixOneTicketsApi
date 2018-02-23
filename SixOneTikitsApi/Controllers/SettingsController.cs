using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PianoBarApi.AxHelpers;
using PianoBarApi.Extensions;
using PianoBarApi.Models;

namespace PianoBarApi.Controllers
{
    public class ProfileController : BaseApi<Profile>
    {
        public override ResultObj Get()
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                //var data = Repository.Get(new ProfileFilter { AccountId = user.AccountId ?? 0 });
                var data = Repository.Get();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Post(Profile record)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                Repository.Insert(SetAudit(record, true));
                results = WebHelpers.BuildResponse(record, "New Profile Saved Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }
    }
}
