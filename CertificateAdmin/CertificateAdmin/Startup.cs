using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using SQLite;


[assembly: OwinStartup(typeof(CertificateAdmin.Startup))]

namespace CertificateAdmin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            SqlLite sql = new SqlLite();
            sql.createNewDatabase();
            ConfigureAuth(app);
          
        }
    }
}
