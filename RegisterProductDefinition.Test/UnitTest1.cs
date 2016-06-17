using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;

namespace RegisterProductDefinition.Test
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void Product_()
        {
            string connectionString = "https://ilsequencercrossenv.blob.core.windows.net/productdefinitionblob/?sv=2015-04-05&sr=c&si=RegistrationUtility&sig=B7dyMGIPZB4GWt8hnVKPYgqCm%2BMWNfZHXGVENIIH6cU%3D";
            CloudBlobContainer cloudBlobContainer = new CloudBlobContainer(new Uri(connectionString));

            int maxBlobsToEnumerate = 10;

            Console.WriteLine("Listing " + maxBlobsToEnumerate + "{0} blobs:");

            var result = cloudBlobContainer.ListBlobs();

            foreach (var blobItem in result)
                Console.WriteLine(blobItem.Uri);

            Console.WriteLine("Listing complete...");

            Assert.AreEqual(2, result.Count());
        }
    }
}
