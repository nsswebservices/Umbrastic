// #tool "xunit.runner.console"
#tool "GitVersion.CommandLine"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target                  = Argument("target", "Default");
var configuration           = Argument("configuration", "Release");
var solutionPath            = MakeAbsolute(File(Argument("solutionPath", "./src/Umbrastic.sln")));
var nugetProjects            = Argument("nugetProjects", "Umbrastic");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var testAssemblies          = new [] { 
                                "./tests/**/bin/" +configuration +"/*.UnitTests.dll" 
                            };

var artifacts               = MakeAbsolute(Directory(Argument("artifactPath", "./artifacts")));
var buildOutput             = MakeAbsolute(Directory(artifacts +"/build/"));
var testResultsPath         = MakeAbsolute(Directory(artifacts + "./test-results"));
var versionAssemblyInfo     = MakeAbsolute(File(Argument("versionAssemblyInfo", "VersionAssemblyInfo.cs")));

IEnumerable<FilePath> nugetProjectPaths     = null;
SolutionParserResult solution               = null;
GitVersion versionInfo                      = null;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup(() => {
    if(!FileExists(solutionPath)) throw new Exception(string.Format("Solution file not found - {0}", solutionPath.ToString()));
    solution = ParseSolution(solutionPath.ToString());

    var projects = solution.Projects.Where(x => nugetProjects.Contains(x.Name));
    if(projects == null || !projects.Any()) throw new Exception(string.Format("Unable to find projects '{0}' in solution '{1}'", nugetProjects, solutionPath.GetFilenameWithoutExtension()));
    nugetProjectPaths = projects.Select(p => p.Path);
    
    // if(!FileExists(nugetProjectPath)) throw new Exception("project path not found");
    Information("[Setup] Using Solution '{0}'", solutionPath.ToString());
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(artifacts.ToString());
    CreateDirectory(artifacts);
    CreateDirectory(buildOutput);
    
    var binDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\bin");
    var objDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\obj");
    CleanDirectories(binDirs);
    CleanDirectories(objDirs);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionPath, new NuGetRestoreSettings());
});

Task("Update-Version-Info")
    .IsDependentOn("CreateVersionAssemblyInfo")
    .Does(() => 
{
        versionInfo = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = true,
            UpdateAssemblyInfoFilePath = versionAssemblyInfo
        });

    if(versionInfo != null) {
        Information("Version: {0}", versionInfo.FullSemVer);
    } else {
        throw new Exception("Unable to determine version");
    }
});

Task("CreateVersionAssemblyInfo")
    .WithCriteria(() => !FileExists(versionAssemblyInfo))
    .Does(() =>
{
    Information("Creating version assembly info");
    CreateAssemblyInfo(versionAssemblyInfo, new AssemblyInfoSettings {
        Version = "0.0.0.0",
        FileVersion = "0.0.0.0",
        InformationalVersion = "",
    });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Update-Version-Info")
    .Does(() =>
{
    MSBuild(solutionPath, settings => settings
        .WithProperty("TreatWarningsAsErrors","true")
        .WithProperty("UseSharedCompilation", "false")
        .WithProperty("AutoParameterizationWebConfigConnectionStrings", "false")
        .SetVerbosity(Verbosity.Quiet)
        .SetConfiguration(configuration)
        .WithTarget("Rebuild")
    );
});

Task("Copy-Files-Umbrastic")
    .IsDependentOn("Build")
    .Does(() => 
{
    EnsureDirectoryExists(buildOutput +"/Umbrastic");
    CopyFile("./src/Umbrastic/bin/" +configuration +"/Umbrastic.dll", buildOutput +"/Umbrastic/Umbrastic.dll");
    CopyFile("./src/Umbrastic/bin/" +configuration +"/Umbrastic.pdb", buildOutput +"/Umbrastic/Umbrastic.pdb");
    CopyFile("./src/Umbrastic/bin/" +configuration +"/readme.txt", buildOutput +"/Umbrastic/readme.txt");
    CopyDirectory("./src/Umbrastic/content", buildOutput +"/Umbrastic/content");
});

Task("Copy-Files-Umbrastic-Core")
    .IsDependentOn("Build")
    .Does(() => 
{
    EnsureDirectoryExists(buildOutput +"/Umbrastic.Core");
    CopyFile("./src/Umbrastic.Core/bin/" +configuration +"/Umbrastic.Core.dll", buildOutput +"/Umbrastic.Core/Umbrastic.Core.dll");
    CopyFile("./src/Umbrastic.Core/bin/" +configuration +"/Umbrastic.Core.pdb", buildOutput +"/Umbrastic.Core/Umbrastic.Core.pdb");
});

Task("Package-Umbrastic-Core")
    .IsDependentOn("Build")
    .IsDependentOn("Copy-Files-Umbrastic-Core")
    .Does(() => 
{
        var settings = new NuGetPackSettings {
            BasePath = buildOutput +"/Umbrastic.Core",
            Id = "Umbrastic.Core",
            Authors = new [] { "NSS Web Services" },
            Owners = new [] { "NSS Web Services" },
            Description = "Provides integration between Umbraco content and Elasticsearch as a search platform",
            LicenseUrl = new Uri("https://raw.githubusercontent.com/nsswebservices/Umbrastic/master/LICENSE"),
            ProjectUrl = new Uri("https://github.com/nsswebservices/Umbrastic"), 
            RequireLicenseAcceptance = false,
            Properties = new Dictionary<string, string> { { "Configuration", configuration }},
            Symbols = false,
            NoPackageAnalysis = true,
            Version = versionInfo.NuGetVersionV2,
            OutputDirectory = artifacts,
            IncludeReferencedProjects = true,
            Files = new[] {
                new NuSpecContent { Source = "Umbrastic.Core.dll", Target = "lib/net452" },
                new NuSpecContent { Source = "Umbrastic.Core.pdb", Target = "lib/net452" },
            },
            Dependencies = new [] {
				new NuSpecDependency { Id = "Nest.Indexing", Version = "0.1.0" },
                new NuSpecDependency { Id = "UmbracoCms.Core", Version = "[7.6.0,8.0)" }
            }
        };
        NuGetPack("./src/Umbrastic.Core/Umbrastic.Core.nuspec", settings);                     
});

Task("Package-Umbrastic")
    .IsDependentOn("Build")
    .IsDependentOn("Copy-Files-Umbrastic")
    .Does(() => 
{
        var settings = new NuGetPackSettings {
            BasePath = buildOutput +"/Umbrastic",
            Id = "Umbrastic",
            Authors = new [] { "NSS Web Services" },
            Owners = new [] { "NSS Web Services" },
            Description = "Provides integration between Umbraco content and Elasticsearch as a search platform",
            LicenseUrl = new Uri("https://raw.githubusercontent.com/nsswebservices/Umbrastic/master/LICENSE"),
            ProjectUrl = new Uri("https://github.com/nsswebservices/Umbrastic"),            
            RequireLicenseAcceptance = false,
            Properties = new Dictionary<string, string> { { "Configuration", configuration }},
            Symbols = false,
            NoPackageAnalysis = true,
            Version = versionInfo.NuGetVersionV2,
            OutputDirectory = artifacts,
            IncludeReferencedProjects = true,
            Files = new[] {
                new NuSpecContent { Source = "Umbrastic.dll", Target = "lib/net452" },
                new NuSpecContent { Source = "Umbrastic.pdb", Target = "lib/net452" },

                new NuSpecContent { Source = "content/web.config.install.xdt", Target = "content" },
                new NuSpecContent { Source = "content/web.config.uninstall.xdt", Target = "content" },
                new NuSpecContent { Source = "content/dashboard.config.install.xdt", Target = "content/config" },
                new NuSpecContent { Source = "content/dashboard.config.uninstall.xdt", Target = "content/config" },

                new NuSpecContent { Source = "readme.txt", Target = "" },

                new NuSpecContent { Source = "content/App_Plugins/umbrastic/**/*", Target = "" }
            },
            Dependencies = new [] {                
                new NuSpecDependency { Id = "UmbracoCms.Core", Version = "[7.6.0,8.0)" },
                new NuSpecDependency { Id = "Umbrastic.Core", Version = "[" +versionInfo.NuGetVersionV2 +"]" }
            }
        };
        NuGetPack("./src/Umbrastic/Umbrastic.nuspec", settings);                     
});

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("Package-Umbrastic")
    .IsDependentOn("Package-Umbrastic-Core")
    .Does(() => { });

Task("Update-AppVeyor-Build-Number")
    .IsDependentOn("Update-Version-Info")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    AppVeyor.UpdateBuildVersion(versionInfo.FullSemVer +" | " +AppVeyor.Environment.Build.Number);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Update-Version-Info")
    .IsDependentOn("Update-AppVeyor-Build-Number")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    ;

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
