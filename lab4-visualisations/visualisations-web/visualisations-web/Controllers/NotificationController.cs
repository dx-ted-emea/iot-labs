using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using visualisations_web.Hubs;

namespace visualisations_web.Controllers
{
    public class NotificationController : ApiController
    {
        public bool Post(HeaterStatus status)
        {
            GlobalHost.ConnectionManager.GetHubContext<HeaterHub>().Clients.All.notifyClient(status);

            return true;
        }
    }
}
