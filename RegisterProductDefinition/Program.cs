using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using log4net;
using log4net.Config;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace RegisterProductDefinition
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            const int failureExistCode = -1;

            try
            {
                // Configure log4net based on the contents of the App.config
                XmlConfigurator.Configure();

                Parameters parameters = new Parameters();
                bool success = CommandLine.Parser.Default.ParseArguments(args, parameters);

                if (!success)
                    Environment.Exit(failureExistCode);

                if (parameters.Product.Length < 1)
                {
                    Console.WriteLine("Product identifier's length must be >= 1 ");
                    Environment.Exit(failureExistCode);
                }

                if (parameters.Revision.Length < 1)
                {
                    Console.WriteLine("Revision's length must be >= 1");
                    Environment.Exit(failureExistCode);
                }

                string blobName = parameters.Product.ToLowerInvariant() + "-" + parameters.Revision.ToLowerInvariant() +
                                  ".zip";

                string blobContainerConnectionString = ConfigurationManager.AppSettings["BlobConnectionString"];
                CloudBlobContainer cloudBlobContainer = new CloudBlobContainer(new Uri(blobContainerConnectionString));

                if (cloudBlobContainer.GetBlobReference(blobName).Exists())
                {
                    Console.WriteLine(blobName + " already exists in the blob container");
                    Environment.Exit(failureExistCode);
                }
                else
                {
                    CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                    Log.Info("Attempting to upload " + blobName);

                    if (!File.Exists(parameters.FullPath))
                    {
                        Console.WriteLine("File not found [" + parameters.FullPath + "]");
                        Environment.Exit(failureExistCode);
                    }

                    if (!parameters.FullPath.ToLowerInvariant().EndsWith(".zip"))
                    {
                        Console.WriteLine("Product definition must be a .zip file");
                        Environment.Exit(failureExistCode);
                    }

                    using (var fileStream = System.IO.File.OpenRead(parameters.FullPath))
                    {
                        blockBlob.UploadFromStream(fileStream, AccessCondition.GenerateIfNotExistsCondition());
                    }

                    Log.Info(blobName + " uploaded.");
                }
            }
            catch (StorageException exception)
            {
                if (exception.Message.Contains("403"))
                {
                    Console.WriteLine("FAILURE: The authentication key that was used to upload the zip file is invalid (rejected).  Please talk to Grady/Matt/Devin about resolving this issue");
                    Environment.Exit(failureExistCode);
                }
            }
            catch (Exception exception)
            {
                Log.Fatal(exception.Message, exception);
                Environment.Exit(failureExistCode);
            }
        }
    }

    public class Parameters
    {
		[Option("fullPath", Required = true, HelpText = @"Path to input zip file.  The path should include the file itself such as C:\Foo.zip")]
		public string FullPath { get; set; }

		[Option("product", Required = true, HelpText = "Product identifier such as ILE")]
		public string Product { get; set; }

		[Option("revision", Required = true, HelpText = "Revision.  Must be unique within the space of revisions for this particular product identifier")]
		public string Revision { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			// this without using CommandLine.Text
			//  or using HelpText.AutoBuild
			return HelpText.AutoBuild(this);
		}
	}
}
