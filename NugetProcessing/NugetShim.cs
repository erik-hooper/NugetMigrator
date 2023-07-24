using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Common;

public class NugetShim
{
    private readonly PackageSource _source;
    private readonly SourceCacheContext _cache;
    private readonly ILogger _logger = new ConsoleLogger();
    public NugetShim(string sourceUri, PackageSourceCredential credential = null)
    {
        _source = new PackageSource(sourceUri)
        {
            Credentials = credential
        };

        _cache = new SourceCacheContext() { MaxAge = DateTimeOffset.Now.AddMinutes(-1) };
    }

    public async Task<ICollection<IPackageSearchMetadata>> ListAllPackages(int page = 0)
    {
        const int pageSize = 50;
        SourceRepository repository = Repository.Factory.GetCoreV3(_source);
        var search = repository.GetResource<PackageSearchResource>();
        SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

        IEnumerable<IPackageSearchMetadata> results = await search.SearchAsync(
            "",
            searchFilter,
            skip: pageSize * page,
            take: pageSize,
            _logger,
            CancellationToken.None);

        return results.ToList();
    }


    public async Task<ICollection<IPackageSearchMetadata>> GetAllExistingPackages(IPackageSearchMetadata packageToTransfer)
    {
        SourceRepository repository = Repository.Factory.GetCoreV3(_source);

        PackageMetadataResource resource = await repository.GetResourceAsync<PackageMetadataResource>();

        IEnumerable<IPackageSearchMetadata> packages = await resource.GetMetadataAsync(
            packageToTransfer.Identity.Id,
            includePrerelease: true,
            includeUnlisted: true,
            _cache,
            _logger,
            CancellationToken.None);

        return packages.ToList();
    }

    public async Task<FileInfo> DownloadPackage(IPackageSearchMetadata package)
    {
        SourceRepository repository = Repository.Factory.GetCoreV3(_source);
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        var file = Path.Combine(Environment.CurrentDirectory, package.Identity.Id + "." + package.Identity.Version.ToString() + ".nupkg");
        using var packageStream = File.Create(file);

        await resource.CopyNupkgToStreamAsync(
            package.Identity.Id,
            package.Identity.Version,
            packageStream,
            _cache,
            _logger,
            CancellationToken.None);

        Console.WriteLine($"Downloaded package {package.Identity.Id} {package.Identity.Version}");

        return new FileInfo(file);
    }

    public async Task UpLoadPackage(IPackageSearchMetadata package, FileInfo file)
    {
        SourceRepository repository = Repository.Factory.GetCoreV3(_source);
        PackageUpdateResource resource = await repository.GetResourceAsync<PackageUpdateResource>();
        try
        {
            await resource.Push(
                file.FullName,
                symbolSource: null,
                timeoutInSecond: 5 * 60,
                disableBuffering: false,
                getApiKey: packageSource => "Bogus",
                getSymbolApiKey: packageSource => null,
                noServiceEndpoint: false,
                skipDuplicate: false,
                symbolPackageUpdateResource: null,
                _logger);
            Console.WriteLine($"Uploaded File {file.Name}");
        }
        catch (Exception ex)
        {
            ConsoleReverter.Error("Could not upload file");
            ConsoleReverter.Error(ex);
        }


    }
}