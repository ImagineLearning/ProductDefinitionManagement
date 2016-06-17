using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using log4net;
using log4net.Config;

namespace RegisterProductDefinition
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            // Example usage:
            //
            // RegisterProductDefinition --fullPath="C:\Dev\ProductDefinitionManagement\Samples\Sample.zip" --product="ILE" --revision="123" --comments="Automatically uploaded by helix export build based on changeset 71983"
            //
            // Potential outcomes:
            //
            //   - Success
            //   - Rejected due to duplicate product:revision combination

            // Configure log4net based on the contents of the App.config
            XmlConfigurator.Configure();
            
	        Parameters parameters = new Parameters();
	        bool success = CommandLine.Parser.Default.ParseArguments(args, parameters);
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

		[Option("comments", Required = false, HelpText = "Ideally this would mention who/what uploaded this product revision and any helpful contextual information like source control changesets, etc.")]
		public string Comments { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			// this without using CommandLine.Text
			//  or using HelpText.AutoBuild
			return HelpText.AutoBuild(this);
		}
	}
}
