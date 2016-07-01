using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CloudAPIClient;
using CommandLine;
using CommandLine.Text;
using log4net;
using log4net.Config;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Environment = System.Environment;

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
			        Log.Error("Product identifier's length must be >= 1 ");
			        Environment.Exit(failureExistCode);
		        }

		        if (parameters.Revision.Length < 1)
		        {
					Log.Error("Revision's length must be >= 1");
			        Environment.Exit(failureExistCode);
		        }

		        if (!File.Exists(parameters.FullPath))
		        {
					Log.Error("File not found [" + parameters.FullPath + "]");
			        Environment.Exit(failureExistCode);
		        }

		        if (!parameters.FullPath.ToLowerInvariant().EndsWith(".zip"))
		        {
					Log.Error("Product definition must be a .zip file");
			        Environment.Exit(failureExistCode);
		        }

                // Decided we wanted to stick with all upper case for product names
	            parameters.Product = parameters.Product.ToUpper();
	            parameters.Revision = parameters.Revision.ToUpper();

		        CloudAPIClient.CloudApiClientConfiguration cloudApiClientConfiguration = new CloudApiClientConfiguration();
		        cloudApiClientConfiguration.SetEnvironment(CloudApiClientConfiguration.KnownEnvironments.TestEnvironment);
		        cloudApiClientConfiguration.SetClientSecret(clientId: "UnitSequencerTests", clientSecret: "8xG8xUBGymJ9");

		        CloudAPIClient.AuthenticationApi _authenticationApi = new AuthenticationApi();
		        string ilAdminToken = _authenticationApi.GetAuthenticationToken(username: "cloud_test@imaginelearning.com",
			        password: "imagine");

		        ProductApi productApi = new ProductApi();

		        using (var fileStream = System.IO.File.OpenRead(parameters.FullPath))
		        {
			        Log.Info("Uploading " + parameters.FullPath+" as product ["+parameters.Product+"] revision ["+parameters.Revision+"]");
			        productApi.UploadUnitSequenceProductRevision(ilAdminToken, parameters.Product,
				        parameters.Revision.ToLowerInvariant(), fileStream);
					Log.Info("Upload complete");
		        }
	        }
	        catch (CloudAPIClient.ProductApi.AuthenticationException)
	        {
		        Log.Error("FAILURE: The authentication key that the sequencer service used to attempt to upload the zip file is invalid (rejected).");
		        Environment.Exit(failureExistCode);
	        }
	        catch (CloudAPIClient.ProductApi.DuplicateProductRevisionException)
	        {
		        Log.Error("This product & revision combination have already been uploaded.  No change was made.");
				Environment.Exit(failureExistCode);
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
