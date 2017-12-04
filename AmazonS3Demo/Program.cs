using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading.Tasks;
using System.Threading;

namespace s3.amazon.com.docsamples
{
    class Program
    {
        static string _bucketName = "stucco-yan-tests";
        static IAmazonS3 _client;

        public static async Task Main(string[] args)
        {
            //await ReadFileContent("Download/README.txt");
            //await DownloadFile("Download/README.txt");
            await UploadFile(_bucketName, "Upload/File4Upload.txt", @"..\SolutionItems\File4Upload.txt", "Uploaded.");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static async Task UploadFile(string bucketName, string keyName, string sourceFilePath, string title)
        {
            try
            {
                using (_client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
                {
                    Console.WriteLine("Uploading an object");
                    //await WritingAnObject(@"Upload/FileFromContent.txt");
                    await WritingAnObject(bucketName, keyName, sourceFilePath, "text/plan", "Upload#1");
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || 
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine("For service sign up go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("Error occurred. Message:'{0}' when writing an object", amazonS3Exception.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        static async Task<PutObjectResponse> WritingAnObject(string keyName)
        {
            PutObjectRequest putRequest1 = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = keyName,
                ContentBody = "Hello S3!"
            };

            PutObjectResponse response = await _client.PutObjectAsync(putRequest1);
            return response;
        }

        private static async Task<PutObjectResponse> WritingAnObject(string bucketName, string keyName, string sourceFilePath, string contentType = "text/plain", string title = "None.")
        {
            PutObjectRequest putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = keyName,
                FilePath = sourceFilePath,
                ContentType = contentType
            };
            putRequest.Metadata.Add("x-amz-meta-title", title);

            PutObjectResponse response = await _client.PutObjectAsync(putRequest);
            return response;
        }

        /// <summary>
        /// File name in S3 with path within the bucket
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private static async Task ReadFileContent(string keyName) 
        {
            try
            {
                Console.WriteLine("Retrieving (GET) an object");
                string data = await ReadObjectData(keyName);
            }
            catch (AmazonS3Exception s3Exception)
            {
                Console.WriteLine(s3Exception.Message, s3Exception.InnerException);
            }
        }

        /// <summary>
        /// File name in S3 with path within the bucket
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private static async Task DownloadFile(string keyName)    
        {
            try
            {
                Console.WriteLine("Downloading (GET) an object");
                using (_client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
                {
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = keyName
                    };

                    using (GetObjectResponse response = await _client.GetObjectAsync(request))
                    {
                        string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), keyName);
                        if (!File.Exists(dest))
                        {
                            CancellationToken cancellationTocken = new CancellationToken();
                            await response.WriteResponseStreamToFileAsync(dest, false, cancellationTocken);
                        }
                    }
                }
            }
            catch (AmazonS3Exception s3Exception)
            {
                Console.WriteLine(s3Exception.Message, s3Exception.InnerException);
            }
        }

        /// <summary>
        /// File name in S3 with path within the bucket
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        static async Task<string> ReadObjectData(string keyName)  
        {
            string responseBody = string.Empty;

            using (_client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = await _client.GetObjectAsync(request))
                {
                    using (Stream responseStream = response.ResponseStream)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            string title = response.Metadata["x-amz-meta-title"];
                            Console.WriteLine("The object's title is {0}", title);

                            responseBody = reader.ReadToEnd();
                        }
                    }
                }
            }
            return responseBody;
        }
    }
}
