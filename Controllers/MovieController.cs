using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using lab3app.Models;
using lab3app;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace lab3app.Controllers
{
    public class MovieController : Controller
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IAmazonS3 _amazonS3Client;
        private readonly IConfiguration _config;

        public MovieController(IAmazonS3 s3Client, IAmazonDynamoDB dynamoDbClient, IConfiguration config)
        {
            //Initialize the DynamoDB and S3 client using the Helper class
            _dynamoDbClient = Helper.CreateDynamoDBClient();
            _amazonS3Client = Helper.GetS3Client();
            _config = config;
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

                //scan to retrieve all items
                ScanResponse response = await _dynamoDbClient.ScanAsync(request);


                //Iterate over the response items and map them to the Movie model
                foreach (var item in response.Items)
                {

                    List<Comment> comments = new List<Comment>();
                    List<Rating> ratings = new List<Rating>();


                    ////get comments list
                    if (item.ContainsKey("Comments") && item["Comments"].L != null)
                    {
                        comments = item["Comments"].L.Select(commentItem =>
                        {
                            Comment? comment = new Comment
                            {
                                CommentId = commentItem.M["CommentId"].S,
                                CommentBy = commentItem.M["CommentBy"].S,
                                CommentedTime = commentItem.M["CommentedTime"].S,
                                CommentText = commentItem.M["CommentText"].S,
                                VideoId = commentItem.M["VideoId"].S
                            };

                            return comment;
                        }).ToList();
                    }

                    ////get ratings list
                    if (item.ContainsKey("Ratings") && item["Ratings"].L != null)
                    {
                        ratings = item["Ratings"].L.Select(ratingItem =>
                        {
                            Rating? rating = new Rating
                            {
                                RatingId = ratingItem.M["RatingId"].S,
                                RatedBy = ratingItem.M["RatedBy"].S,
                                RatedTime = ratingItem.M["RatedTime"].S,
                                RatingValue = ratingItem.M["RatingValue"].S,
                                VideoId = ratingItem.M["VideoId"].S
                            };

                            return rating;
                        }).ToList();
                    }


                    var movie = new Movie
                    {
                        Id = item["Id"].S,
                        Title = item["Title"].S,
                        Director = item["Director"].S,
                        Genre = item["Genre"].S,
                        ReleaseTime = item["ReleaseTime"].S,
                        UploadedBy = item["UploadedBy"].S,
                        MovieKey = item["MovieKey"].S,
                        Ratings = ratings,
                        Comments = comments

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
                        m.ReleaseTime.Contains(search, StringComparison.OrdinalIgnoreCase)); 
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

        //get a movie's details
        public async Task<IActionResult?> MovieDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Item ID is required.");
            }

            var m = await GetMoviesByIdAsync(id);

            if (m == null)
            {
                return NotFound("Item not found.");
            }

            return View(m);
        }

        //get a movie's details with comments
        public async Task<IActionResult?> MovieComments(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Item ID is required.");
            }

            var m = await GetMoviesByIdAsync(id);

            if (m == null)
            {
                return NotFound("Item not found.");
            }

            return View(m);


        }

        [HttpGet]
        public IActionResult AddMovie()
        {
            return View();
        }

//        



        public async Task<List<Movie>> GetMoviesWithSearch(string search)
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

                //scan to retrieve all items
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
                        UploadedBy = item["UploadedBy"].S,
                        MovieKey = item["MovieKey"].S
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
                        m.ReleaseTime.Contains(search, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch (Exception ex)
            {
                //Handle any errors that occur during the query
                ViewBag.ErrorMessage = "An error occurred while retrieving movie data: " + ex.Message;
            }

            //Store the search query in ViewData to persist it in the search input
            ViewData["SearchQuery"] = search;

            return movies;
        }


        public async Task<Movie?> GetMoviesByIdAsync(string id)
        {
            GetItemRequest? r = new GetItemRequest
            {
                TableName = "MoviesTable",
                Key = new Dictionary<string, AttributeValue>
                {
                    {"Id", new AttributeValue {S = id}}
                }
            };

            GetItemResponse? response = await _dynamoDbClient.GetItemAsync(r);

            if (response.Item == null || response.Item.Count == 0)
            {
                return null;
            }

            //default empty lists for comments and ratings
            List<Comment> comments = new List<Comment>();
            List<Rating> ratings = new List<Rating>();

            //get comments list
            if (response.Item.ContainsKey("Comments") && response.Item["Comments"].L != null)
            {
                comments = response.Item["Comments"].L.Select(commentItem =>
                {
                    Comment? comment = new Comment
                    {
                        CommentId = commentItem.M["CommentId"].S,
                        CommentBy = commentItem.M["CommentBy"].S,
                        CommentedTime = commentItem.M["CommentedTime"].S,
                        CommentText = commentItem.M["CommentText"].S,
                        VideoId = commentItem.M["VideoId"].S
                    };

                    return comment;
                }).ToList();
            }

            //get ratings list
            if (response.Item.ContainsKey("Ratings") && response.Item["Ratings"].L != null)
            {
                ratings = response.Item["Ratings"].L.Select(ratingItem =>
                {
                    Rating? rating = new Rating
                    {
                        RatingId = ratingItem.M["RatingId"].S,
                        RatedBy = ratingItem.M["RatedBy"].S,
                        RatedTime = ratingItem.M["RatedTime"].S,
                        RatingValue = ratingItem.M["RatingValue"].S,
                        VideoId = ratingItem.M["VideoId"].S
                    };

                    return rating;
                }).ToList();
            }

            var movie = new Movie
            {
                Id = response.Item["Id"].S,
                Title = response.Item["Title"].S,
                Director = response.Item["Director"].S,
                Genre = response.Item["Genre"].S,
                ReleaseTime = response.Item["ReleaseTime"].S,
                MovieKey = response.Item["MovieKey"].S,
                UploadedBy = response.Item["UploadedBy"].S,
                Comments = comments,
                Ratings = ratings
            };


            return movie;

        }

        //To enusre access only when logged in
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Apply the login requirement only to the "UsersMovies" action
            if (context.ActionDescriptor.RouteValues["action"] == "UsersMovies")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                Console.WriteLine("session userid: " + userId);
                if (userId == null)
                {
                    context.Result = RedirectToAction("Login", "Account");
                }
            }
            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> UsersMovies(string search)
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

                //scan to retrieve all items
                var response = await _dynamoDbClient.ScanAsync(request);

                var u = HttpContext.Session.GetInt32("UserId");
                Console.WriteLine("sessi user id in usersmovies" + u);

                //Iterate over the response items and map them to the Movie model
                foreach (var item in response.Items)
                {
                    
                    Console.WriteLine("userid from db " + item["UploadedBy"].S);
                    if (item != null && item["UploadedBy"].S == u.ToString())
                    {
                        var movie = new Movie
                        {
                            Id = item["Id"].S,
                            Title = item["Title"].S,
                            Director = item["Director"].S,
                            Genre = item["Genre"].S,
                            ReleaseTime = item["ReleaseTime"].S,
                            UploadedBy = item["UploadedBy"].S,
                            MovieKey = item["MovieKey"].S
                        };

                        movies.Add(movie);
                    }

                }

                //If a search query is provided, filter the movies
                if (!string.IsNullOrEmpty(search))
                {
                    movies = movies.FindAll(m =>
                        m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.Director.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.Genre.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.ReleaseTime.Contains(search, StringComparison.OrdinalIgnoreCase));
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
