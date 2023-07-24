// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using Newtonsoft.Json;
using NuGet;
using System.Threading;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

// pre authenticated url
const string FROM =
    "TODO";

const string TO= "TODO";

const string User = "TODO";
const string PasswordText = "TODO";

Console.WriteLine("Migrating");
Console.WriteLine("FROM:: " + FROM);
Console.WriteLine("TO::" + TO);

var destination = new NugetShim(TO,
    new PackageSourceCredential(
        source: TO,
        username: User,
        passwordText: PasswordText,
        isPasswordClearText: true,
        validAuthenticationTypesText: null));


var source = new NugetShim(FROM);

int page = 0;
IEnumerable<IPackageSearchMetadata> results = null;
do
{
    results = await source.ListAllPackages(page);
    Console.WriteLine("Processing Page " + page);
    // foreach (var package in results)
    await results.ParallelForEach(async (package) =>
    {
        if (package.Identity.Id.StartsWith("Microsoft"))
        {
            return;
        }
        var migrated = await destination.GetAllExistingPackages(package);
        var existingPackages = await source.GetAllExistingPackages(package);

        foreach (var searchMetadata in existingPackages)
        {
            if (!migrated.Any(m => m.Identity.Version.Equals(searchMetadata.Identity.Version)))
            {
                ConsoleReverter.Error($"{package.Identity.Id} has a mismatch, missing :: {searchMetadata.Identity.Version}");

                var file = await source.DownloadPackage(searchMetadata);
                await destination.UpLoadPackage(searchMetadata, file);
            }
        }
    });
    page++;

} while (results?.Any() ?? false);

ConsoleReverter.Message("Done");



