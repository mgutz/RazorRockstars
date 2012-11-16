﻿using System.Net;
using Funq;
using ServiceStack.Configuration;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite;
using ServiceStack.Razor;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;

//The entire C# code for the stand-alone RazorRockstars demo.
namespace RazorRockstars.SelfHost
{
    public class AppHost : AppHostHttpListenerBase
    {
        public static Rockstar[] SeedData = new[] {
            new Rockstar(1, "Jimi", "Hendrix", 27, false), 
            new Rockstar(2, "Janis", "Joplin", 27, false), 
            new Rockstar(4, "Kurt", "Cobain", 27, false),              
            new Rockstar(5, "Elvis", "Presley", 42, false), 
            new Rockstar(6, "Michael", "Jackson", 50, false), 
            new Rockstar(7, "Eddie", "Vedder", 47, true), 
            new Rockstar(8, "Dave", "Grohl", 43, true), 
            new Rockstar(9, "Courtney", "Love", 48, true), 
            new Rockstar(10, "Bruce", "Springsteen", 62, true), 
        };

        public AppHost() : base("Test Razor", typeof(AppHost).Assembly) { }

        public override void Configure(Container container)
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            Plugins.Add(new RazorFormat());

            var connectionString = ConfigUtils.GetConnectionString("postgresql");
            container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory(connectionString, false,
                                                                                PostgreSqlDialect.Provider));
           //container.Register<IDbConnectionFactory>(
            //    new OrmLiteConnectionFactory(":memory:", false, SqliteDialect.Provider));

            using (var db = container.Resolve<IDbConnectionFactory>().OpenDbConnection())
            {
                db.CreateTableIfNotExists<Rockstar>();
                db.InsertAll(SeedData);
            }

            SetConfig(new EndpointHostConfig {
                CustomHttpHandlers = {
                    { HttpStatusCode.NotFound, new RazorHandler("/notfound") }
                }
            });
        }
    }

    //Poco Data Model for OrmLite + SeedData 
    [Route("/rockstars", "POST")]
    public class Rockstar
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
        public bool Alive { get; set; }

        public string Url
        {
            get { return "/stars/{0}/{1}".Fmt(Alive ? "alive" : "dead", LastName.ToLower()); }
        }

        public Rockstar() { }
        public Rockstar(int id, string firstName, string lastName, int age, bool alive)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Alive = alive;
        }
    }
}