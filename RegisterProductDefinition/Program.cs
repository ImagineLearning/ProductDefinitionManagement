using System;
using System.Collections.Generic;
using System.Configuration;
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
            // Configure log4net based on the contents of the App.config
            XmlConfigurator.Configure();
            
	        Parameters parameters = new Parameters();
	        bool success = CommandLine.Parser.Default.ParseArguments(args, parameters);

	        if (!success)
	            Environment.Exit(-1);

            string blobName = parameters.Product.ToLowerInvariant() + "-" + parameters.Revision.ToLowerInvariant() + ".zip";

            string blobContainerConnectionString = ConfigurationManager.AppSettings["BlobConnectionString"];
            CloudBlobContainer cloudBlobContainer = new CloudBlobContainer(new Uri(blobContainerConnectionString));

            if (cloudBlobContainer.GetBlobReference(blobName).Exists())
            {
                Log.Info(blobName+" already exists in the blob container");
                Environment.Exit(-1);
            }
            else
            {
                CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                Log.Info("Attempting to upload " + blobName);

                using (var fileStream = System.IO.File.OpenRead(parameters.FullPath))
                {
                    blockBlob.UploadFromStream(fileStream, AccessCondition.GenerateIfNotExistsCondition());
                }

                Log.Info(blobName + " uploaded.");
            }
        }
    }

    public class Parameters
    {
		[Option("fullPath", Required = true, HelpText = @"Path to input zip file.  The path should include the file itself such as C:\Foo.zip")]
		public string FullPath { get; set; }

		[Option("product", Required = true, HelpText = "Product identifier such as ILE")]
		public string Product { get; set; }

		[Option("revision", Required = true, HelpText = "Revision, note this is a string so enclose numbers in double quotes such as \"123\"")]
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
