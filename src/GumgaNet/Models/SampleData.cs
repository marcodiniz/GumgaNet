using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace MovieAngular.Models
{
    public class SampleData
    {

        public static async Task CreateSampleData(IServiceProvider applicationServices)
        {
            using (var dbContext = applicationServices.GetService<MoviesAppContext>())
            {
                var sqlServerDatabase = dbContext.Database as SqlServerDatabase;
                if (sqlServerDatabase != null)
                {
                    // Create database in user root (c:\users\your name)
                    if (await sqlServerDatabase.EnsureCreatedAsync())
                    {
                        // add some movies
                        var movies = new List<Movie>
                {
                    new Movie {Title="Star Wars", Director="Lucas"},
                    new Movie {Title="King Kong", Director="Jackson"},
                    new Movie {Title="Memento", Director="Nolan"}
                };
                        movies.ForEach(m => dbContext.Movies.Add(m));

                        // add some users
                        var userManager = applicationServices.GetService<UserManager<ApplicationUser>>();

                        // add editor user
                        var usr1 = new ApplicationUser
                        {
                            UserName = "Zeca"
                        };
                        var result = await userManager.CreateAsync(usr1, "!@#Mudar");
                        await userManager.AddClaimAsync(usr1, new Claim("CanEdit", "true"));

                        // add normal user
                        var usr2 = new ApplicationUser
                        {
                            UserName = "Carioca"
                        };
                        await userManager.CreateAsync(usr2, "!@#Mudar");
                    }

                }
            }
        }


        public static async Task InitializeMoviesDatabaseAsync(IServiceProvider serviceProvider, bool createUsers = true)
        {
            using (var db = serviceProvider.GetService<MoviesAppContext>())
            {
                var sqlServerDatabase = db.Database as SqlServerDatabase;
                if (sqlServerDatabase != null)
                {   
                    if (await sqlServerDatabase.EnsureCreatedAsync())
                    {
                        await InsertTestData(serviceProvider);
                    }
                }
                else
                {
                    await InsertTestData(serviceProvider);
                }
            }
        }

        private static async Task InsertTestData(IServiceProvider serviceProvider)
        {
            await AddOrUpdateAsync(serviceProvider, g => g.Id, Movies.Select(movie => movie.Value));
        }



        // TODO [EF] This may be replaced by a first class mechanism in EF
        private static async Task AddOrUpdateAsync<TEntity>(
            IServiceProvider serviceProvider,
            Func<TEntity, object> propertyToMatch, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            // Query in a separate context so that we can attach existing entities as modified
            List<TEntity> existingData;
            using (var db = serviceProvider.GetService<MoviesAppContext>())
            {
                existingData = db.Set<TEntity>().ToList();
            }

            using (var db = serviceProvider.GetService<MoviesAppContext>())
            {
                foreach (var item in entities)
                {
                    db.Entry(item).State = existingData.Any(g => propertyToMatch(g).Equals(propertyToMatch(item)))
                        ? EntityState.Modified
                        : EntityState.Added;
                }

                await db.SaveChangesAsync();
            }
        }

        private static Dictionary<string, Movie> movies;
        public static Dictionary<string, Movie> Movies
        {
            get
            {
                if (movies == null)
                {
                    var artistsList = new Movie[]
                    {
                        new Movie { Director = "John", Title = "Jurrassic Park" , ReleaseDate = new DateTime(2015,02,2), TicketPrice = (decimal) 3.3},
                        new Movie { Director = "Peter", Title = "Lord Of", ReleaseDate = new DateTime(2015,05,2), TicketPrice = (decimal) 3.5},

                    };

                    // TODO [EF] Swap to store generated keys when available
                    int artistId = 1;
                    movies = new Dictionary<string, Movie>();
                    foreach (Movie movie in artistsList)
                    {
                        movie.Id = artistId++;
                        movies.Add(movie.Title, movie);
                    }
                }

                return movies;
            }
        }


    }
}
