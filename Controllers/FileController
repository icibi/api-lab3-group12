using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Upload(IFormFile movie, string title, string director, string genre, string releaseTime, string uid)
        {
            if (movie != null && movie.Length > 0)
            {
                ViewBag.Message = "in if.";
                Console.WriteLine("test");
                try
                {

                    //var uid = TempData["UserId"] as string;
                    //TempData.Keep("UserId");

                    //if (string.IsNullOrEmpty(userId))
                    //{
                    //    ViewBag.Message = "User is not authenticated, please log in.";
                    //    return RedirectToAction("Login", "User");
                    //}

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

                        var r = await _amazonS3Client.PutObjectAsync(putReq);
                        if (r.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            await SaveMetadataToDynamo(movieKey, title, director, genre, releaseTime, uid);
                        }
                    }

                    ViewBag.Message = "Movie Uploaded Successfully!"
;
                }
                catch (Exception e)
                {
                    ViewBag.Message = $"Error uploading movie: {e.Message}";
                }
            }
            else
            {
                ViewBag.Message = "Please select a movie to upload.";
            }

            return RedirectToAction("MoviesPage");
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
