using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lab3app.Controllers
{
    public class FileController : Controller
    {

        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IAmazonS3 _amazonS3Client;
        private readonly IConfiguration _config;

        public FileController(IAmazonS3 s3Client, IAmazonDynamoDB dynamoDbClient, IConfiguration config)
        {
            //Initialize the DynamoDB and S3 client using the Helper class
            _dynamoDbClient = Helper.CreateDynamoDBClient();
            _amazonS3Client = Helper.GetS3Client();
            _config = config;
        }


        [HttpPost]
        public async Task<IActionResult> Upload()
        {

            string t = Request.Form["Title"];
            IFormFile movie = Request.Form.Files["Movie"];
            string d = Request.Form["Director"];
            string g = Request.Form["Genre"];
            string r = Request.Form["ReleaseTime"];
            Console.WriteLine($"title: {t}");

            if (movie == null)
            {
                Console.WriteLine("Null movie!");
                ViewBag.Message = "Please select a movie to upload.";
                return RedirectToAction("UsersMovies");
            }

            if (movie.Length <= 0)
            {
                Console.WriteLine("Movie Length must be greater than zero");
                ViewBag.Message = "Please select a movie to upload.";
                return RedirectToAction("UsersMovies");
            }

                Console.WriteLine("uploading...");
                try
                {

                    var u = HttpContext.Session.GetString("UserId");
             

                    if (string.IsNullOrEmpty(u))
                    {
                        ViewBag.Message = "User is not authenticated, please log in.";
                        return RedirectToAction("Login", "User");
                    }

                    var movieKey = movie.FileName;
                    var bucketName = _config["AWS:S3BName"];
                    // var filePath = Path.GetFileName(movie.FileName);

                    var transferUtility = new TransferUtility(_amazonS3Client);


                    using (var stream = movie.OpenReadStream())
                    {
                        PutObjectRequest putReq = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            Key = movieKey,
                            InputStream = stream,
                            ContentType = movie.ContentType
                        };

                        var res = await _amazonS3Client.PutObjectAsync(putReq);
                        if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            await SaveMetadataToDynamo(movieKey, t, d, g, r, u);
                        }
                    }

              
                    Console.WriteLine("Movie uploaded successfully!");
;
                }
                catch (Exception e)
                {
                    ViewBag.Message = $"Error uploading movie: {e.Message}";
                    Console.WriteLine($"Error uploading movie: { e.Message}");
            }
            

            return RedirectToAction("UsersMovies");
        }


        private async Task SaveMetadataToDynamo(string movieKey, string title, string director, string genre, string releaseTime, string userId)
        {
            var tableName = _config["AWS:DynamoDBTableName"];

            var doc = new Document();
            doc["Id"] = Guid.NewGuid().ToString();
            doc["Title"] = title;
            doc["Director"] = director;
            doc["Genre"] = genre;
            doc["MovieKey"] = movieKey;
            doc["ReleaseTime"] = releaseTime;
            doc["UploadedBy"] = userId;
            doc["Ratings"] = new List<Document>();
            doc["Comments"] = new List<Document>();

            var table = Table.LoadTable(_dynamoDbClient, tableName);
            await table.PutItemAsync(doc);

        }



    }
}
