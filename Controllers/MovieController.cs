using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using lab3app.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lab3app.Controllers
{
    public class MovieController : BaseController
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public MovieController()
        {
            //Initialize the DynamoDB client using the Helper class
            _dynamoDbClient = Helper.CreateDynamoDBClient();
        }

        //Action to fetch and display the list of movies with search functionality
        public async Task<IActionResult> MoviesPage(string search)
        {
            //Initialize a list to store movie details
            var movies = new List<Movie>();

            try
            {
                //Create a ScanRequest to fetch movies from the MoviesTable
                var request = new ScanRequest
                {
                    TableName = "MoviesTable"
                };

                //Scan to retrieve all items
                var response = await _dynamoDbClient.ScanAsync(request);

                //Iterate over the response items and map them to the Movie model
                foreach (var item in response.Items)
                {
                    var movie = new Movie
                    {
                        Id = item["Id"].S,
                        Title = item["Title"].S,
                        Director = item["Director"].S,
                        Genre = item["Genre"].S,
                        ReleaseTime = item["ReleaseTime"].S,
                    };

                    movies.Add(movie);
                }

                //If a search query is provided, filter the movies
                if (!string.IsNullOrEmpty(search))
                {
                    movies = movies.FindAll(m =>
                        m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.Director.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.Genre.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.ReleaseTime.Contains(search, StringComparison.OrdinalIgnoreCase)
                    );
                }
            }
            catch (Exception ex)
            {
                //Handle any errors that occur during the query
                ViewBag.ErrorMessage = "An error occurred while retrieving movie data: " + ex.Message;
            }

            //Store the search query in ViewData to persist it in the search input
            ViewData["SearchQuery"] = search;

            return View(movies);
        }
    }
}
