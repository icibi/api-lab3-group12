using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace lab3app
{
    public class Helper
    {
        public static IAmazonDynamoDB CreateDynamoDBClient()
        {
            // Retrieve AWS credentials from environment or app configuration
            var accessKeyID = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var region = RegionEndpoint.USEast1; // Or retrieve from app configuration if needed

            if (string.IsNullOrEmpty(accessKeyID) || string.IsNullOrEmpty(secretKey))
            {
                // If environment variables aren't found, fallback to appsettings or default credentials
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                accessKeyID = config["AWS:AccessKeyId"];
                secretKey = config["AWS:SecretAccessKey"];
                region = RegionEndpoint.GetBySystemName(config["AWS:Region"]);
            }

            // Create and return DynamoDB client
            var credentials = new BasicAWSCredentials(accessKeyID, secretKey);
            return new AmazonDynamoDBClient(credentials, region);
        }

        public static IAmazonS3 GetS3Client()
        {
            // Retrieve AWS credentials from environment or app configuration
            string accessKeyID = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            RegionEndpoint region = RegionEndpoint.USEast1; // Or retrieve from app configuration if needed
        
            if (string.IsNullOrEmpty(accessKeyID) || string.IsNullOrEmpty(secretKey))
            {
                // If environment variables aren't found, fallback to appsettings or default credentials
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                accessKeyID = config["AWS:AccessKeyId"];
                secretKey = config["AWS:SecretAccessKey"];
                region = RegionEndpoint.GetBySystemName(config["AWS:Region"]);
            }
        
            // Create and return DynamoDB client
            BasicAWSCredentials credentials = new BasicAWSCredentials(accessKeyID, secretKey);
            return new AmazonS3Client(credentials, region);
        }
            
    }
}
