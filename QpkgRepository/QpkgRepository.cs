﻿namespace ArxOne.Qnap;

using Utility;
using System.Collections.Immutable;
using System.Xml;

public class QpkgRepository
{
    private readonly QpkgRepositoryConfiguration _configuration;

    private readonly IReadOnlyList<QpkgRepositorySource> _sources;

    public QpkgRepository(QpkgRepositoryConfiguration qpkgRepositoryConfiguration, IEnumerable<QpkgRepositorySource> sources)
    {
        _configuration = qpkgRepositoryConfiguration;
        _sources = sources.ToImmutableList();
    }

    public XmlDocument GetXml(params string[]? model)
    {
        var platforms = model is not null && model.Length > 0 ? model.Select(x => x.Replace(" ", "+")).ToList() : GetPlatforms();
        var packagesBysSource = LoadPackagesBySource();

        var repository = new XmlDocument();
        var plugins = repository.CreateElement("plugins");
        repository.AppendChild(plugins);

        var cacheCheck = repository.CreateElement("cachechk");
        cacheCheck.InnerText = DateTime.Now.ToString("yyyyMMddHHmm");
        plugins.AppendChild(cacheCheck);

        foreach (var packages in packagesBysSource.GroupBy(x => x.Name))
        {
            var package = packages.MaxBy(x => x.Version);

            if (package is null)
                continue;

            var item = repository.CreateElement("item");
            plugins.AppendChild(item);

            repository.AddElementCData(item, "name", package.DisplayName);
            repository.AddElement(item, "internalName", package.Name);
            repository.AddElementCData(item, "description", package.Summary);
            repository.AddElement(item, "version", package.Version.ToString());
            repository.AddElementCData(item, "maintainer", package.Author);
            repository.AddElementCData(item, "developer", package.Author);


            foreach (var platformId in platforms)
            {
                var platform = repository.CreateAndAddElement(item, "platform");
                repository.AddElement(platform, "platformID", platformId);
                var elementValue = package.GetUri(_configuration.WebsiteRoot);
                repository.AddElement(platform, "location", elementValue);
                if (!string.IsNullOrEmpty(package.Signature))
                    repository.AddElement(platform, "signature", package.Signature);
            }

            repository.AddElement(item, "category", package.Source.Category);
            repository.AddElement(item, "type", package.Source.Type);
            repository.AddElement(item, "changeLog", package.Source.ChangelogLink);
            repository.AddElement(item, "publishedDate", package.Source.PublishedDate.ToString("yyyy/MM/dd"));
            repository.AddElement(item, "language", package.Source.Languages);
            repository.AddElement(item, "icon80", package.Source.Icon80Uri);
            repository.AddElement(item, "icon100", package.Source.Icon100Uri);
            if (package.Source.SnapshotUri is not null)
                repository.AddElementCData(item, "snapshot", package.Source.SnapshotUri);

            repository.AddElementCData(item, "forumLink", package.Source.ForumLink);
            repository.AddElementCData(item, "bannerImg", package.Source.BannerImg);
            repository.AddElementCData(item, "tutorialLink", package.Source.TutorialLink);
            repository.AddElement(item, "fwVersion", package.Source.FirmwareMinimumVersion);

        }
        return repository;
    }

    private IEnumerable<Package> LoadPackagesBySource()
    {
        var packagesBySource = new List<Package>();
        foreach (var source in _sources)
        {
            var filePaths = Directory.GetFiles(Path.Combine(_configuration.StorageRoot, source.SourceRelativeDirectory)).Where(x => x.EndsWith(".qpkg"));
            packagesBySource.AddRange(LoadPackagesFromSource(filePaths, source));
        }
        return packagesBySource;
    }

    private IEnumerable<Package> LoadPackagesFromSource(IEnumerable<string> filePaths, QpkgRepositorySource source)
    {
        var packages = new List<Package>();
        foreach (var filePath in filePaths)
        {
            using var fileStream = File.OpenRead(filePath);
            var config = source.GetRawControl(fileStream);
            var signature = File.ReadAllText(filePath + ".codesigning");
            var storageRootLength = _configuration.StorageRoot.Length + 1;
            try
            {
                var loadPackagesFromSource = new Package(config, filePath[storageRootLength..], signature, source);
                packages.Add(loadPackagesFromSource);
            }
            catch (FormatException) { }

        }
        return packages;
    }

    private static IList<string> GetPlatforms() => new List<string>
    {
            "HS-251",
            "HS-251+",
            "HS-251D",
            "MiroKing",
            "Mustang-200",
            "QB-103",
            "QGD-1600",
            "QGD-1602",
            "QGD-3014",
            "SS-X53",
            "TNC-X51B",
            "TS-1232XU",
            "TS-1635AX",
            "TS-1685",
            "TS-231P2",
            "TS-231P3",
            "TS-251+",
            "TS-431P",
            "TS-431P2",
            "TS-431P3",
            "TS-451+",
            "TS-531P",
            "TS-673A",
            "TS-832X",
            "TS-EC1080 Pro",
            "TS-EC1280U",
            "TS-EC1280U R2",
            "TS-EC1280U-RP",
            "TS-EC1680U",
            "TS-EC1680U R2",
            "TS-EC1680U-RP",
            "TS-EC2480U",
            "TS-EC2480U R2",
            "TS-EC2480U-RP",
            "TS-EC880 Pro",
            "TS-EC880U",
            "TS-EC880U R2",
            "TS-EC880U-RP",
            "TS-i410X",
            "TS-KVM",
            "TS-NASARM",
            "TS-NASX86",
            "TS-X16",
            "TS-X28A",
            "TS-X31P2",
            "TS-X31P3",
            "TS-X31X",
            "TS-X31XU",
            "TS-X32",
            "TS-X32U",
            "TS-X33",
            "TS-X35",
            "TS-X35A",
            "TS-X35EU",
            "TS-X41",
            "TS-X51",
            "TS-X51+",
            "TS-X51A",
            "TS-X51AU",
            "TS-X51B",
            "TS-X51D",
            "TS-X51DU",
            "TS-X51U",
            "TS-X53",
            "TS-X53B",
            "TS-X53BU",
            "TS-X53D",
            "TS-X53E",
            "TS-X53II",
            "TS-X53U",
            "TS-X63",
            "TS-X63U",
            "TS-X64",
            "TS-X64U",
            "TS-X71",
            "TS-X71U",
            "TS-X72",
            "TS-X72U",
            "TS-X73",
            "TS-X73A",
            "TS-X73AU",
            "TS-X73U",
            "TS-X74",
            "TS-X75",
            "TS-X75U",
            "TS-X77",
            "TS-X77U",
            "TS-X79U",
            "TS-X80",
            "TS-X80U",
            "TS-X82",
            "TS-X82S",
            "TS-X82U",
            "TS-X83XU",
            "TS-X85",
            "TS-X85U",
            "TS-X87XU",
            "TS-X88",
            "TS-X88U",
            "TS-X89FU",
            "TS-X89U",
            "TS-X90",
            "TS-X90U",
            "TS-XA28A",
            "TS-XA51",
            "TS-XA73",
            "TS-XA82",
            "TS-XA83XU",
            "TVS-463",
            "TVS-663",
            "TVS-672X",
            "TVS-672XT",
            "TVS-863",
            "TVS-863+",
            "TVS-872X",
            "TVS-872XT",
            "TVS-882ST3"
    };
}