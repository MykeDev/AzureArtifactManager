using AzureArtifactManager.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureArtifactManager.Services
{
    /// <summary>
    /// Fetching information is based on REST API.
    /// As publish/download is not yet supported via rest api, the artifact tool is used (same as in azure cli devops extension)
    /// Parameter etc. were borrowed from https://github.com/Azure/azure-devops-cli-extension/blob/37188cd59a383ded4d49fdc6f495dd0dd6e8cd26/azure-devops/azext_devops/dev/common/artifacttool.py#L55
    /// </summary>
    public class AzureArtifactService
    {
        private const string AzureArtifactManagerPATEnvironmentVariable = "AzureArtifactManagerPAT";

        private readonly string organization;
        private readonly string project;
        private readonly string personalAccessToken;

        private string artifactToolPath;

        public AzureArtifactService(string organization, string project, string personalAccessToken)
        {
            this.organization = organization;
            this.project = project;
            this.personalAccessToken = personalAccessToken;
        }

        public async Task InitializeAsync()
        {
            artifactToolPath = await FetchArtifactTool();
        }

        public async Task<IEnumerable<Package>> GetPackagesForFeedAsync(Feed feed)
        {
            var url = $"Packaging/Feeds/{feed.Id}/Packages";
            var client = CreateFeedHttpClient();

            var packagesTask = client.GetStreamAsync(url);
            var response = await JsonSerializer.DeserializeAsync<ApiResponse<Package>>(await packagesTask);

            return response.Value;
        }

        public async Task<IEnumerable<Feed>> GetFeedsAsync()
        {
            var client = CreateFeedHttpClient();

            var url = "Packaging/Feeds/";

            var feedsTask = client.GetStreamAsync(url);
            var response = await JsonSerializer.DeserializeAsync<ApiResponse<Feed>>(await feedsTask);

            return response.Value;
        }

        public void DownloadPackage(Feed feed, string packageName, string packageVersion, string path)
        {
            var args = new List<string> {"universal", "download", "--service", $"https://dev.azure.com/{organization}", "--patvar", AzureArtifactManagerPATEnvironmentVariable, "--feed", feed.Id, "--package-name", packageName, "--package-version", packageVersion, "--path", path};

            if (project != null)
            {
                args.Add("--project");
                args.Add(project);
            }

            var processStartInfo = new ProcessStartInfo(artifactToolPath);
            processStartInfo.EnvironmentVariables.Add(AzureArtifactManagerPATEnvironmentVariable, personalAccessToken);

            foreach (var arg in args)
            {
                processStartInfo.ArgumentList.Add(arg);
            }

            var artifactToolProcess = Process.Start(processStartInfo);

            artifactToolProcess.WaitForExit();
        }

        public void PublishPackage(Feed feed, string packageName, string packageVersion, string packageDescription, string folderPath)
        {
            var args = new List<string> { "universal", "publish", "--service", $"https://dev.azure.com/{organization}", "--patvar", AzureArtifactManagerPATEnvironmentVariable, "--feed", feed.Id, "--package-name", packageName, "--package-version", packageVersion, "--path", folderPath };

            if (project != null)
            {
                args.Add("--project");
                args.Add(project);
            }

            if (!string.IsNullOrEmpty(packageDescription))
            {
                args.Add("--description");
                args.Add(packageDescription);
            }

            var processStartInfo = new ProcessStartInfo(artifactToolPath);
            processStartInfo.EnvironmentVariables.Add(AzureArtifactManagerPATEnvironmentVariable, personalAccessToken);

            foreach (var arg in args)
            {
                processStartInfo.ArgumentList.Add(arg);
            }

            var artifactToolProcess = Process.Start(processStartInfo);

            artifactToolProcess.WaitForExit();
        }

        private HttpClient CreateFeedHttpClient()
        {
            var client = CreateHttpClient();

            if (project == null)
            {
                client.BaseAddress = new Uri($"https://feeds.dev.azure.com/{organization}/_apis/");
            }
            else
            {
                client.BaseAddress = new Uri($"https://feeds.dev.azure.com/{organization}/{project}/_apis/");
            }

            return client;
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetBase64EncodedAuthenticationString());

            return client;
        }

        private string GetBase64EncodedAuthenticationString()
        {
            var authenticationString = $"user:{personalAccessToken}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

            return base64EncodedAuthenticationString;
        }

        private async Task<string> FetchArtifactTool(bool isRetry = false)
        {
            var artifactToolParentFolder = @".Azure\azuredevops\cli\tools\artifacttool";
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var artifactToolFolder = Path.Join(userProfile, artifactToolParentFolder);

            var artifactToolExecutable = Directory.GetFiles(artifactToolFolder, "artifacttool.exe", SearchOption.AllDirectories).FirstOrDefault();

            if (artifactToolExecutable == null)
            {
                if (isRetry)
                {
                    throw new InvalidOperationException("Could not download artifact tool");
                }

                // ArtifactTool not available - we should download it...
                // Download url I got from starting az cli in debug mode =)
                var url = $"https://vsblob.dev.azure.com/{organization}/_apis/clienttools/ArtifactTool/release?osName=Windows&arch=AMD64";
                var client = CreateHttpClient();

                var artifactsToolTask = client.GetStreamAsync(url);
                var response = await JsonSerializer.DeserializeAsync<ArtifactTool>(await artifactsToolTask);

                var artifactToolName = $"ArtifactTool_{response.Os}_{response.Version}";

                var tempDirectory = Path.Combine(Path.GetTempPath(), "ArtifactTool", response.Version);

                if (!Directory.Exists(tempDirectory))
                {
                    Directory.CreateDirectory(tempDirectory);
                }

                var tempFileName = Path.Combine(tempDirectory, $"{artifactToolName}.zip");

                using (var webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential("something", GetBase64EncodedAuthenticationString());
                    webClient.DownloadFile(new Uri(response.Uri), tempFileName);
                }

                ZipFile.ExtractToDirectory(tempFileName, Path.Combine(artifactToolFolder, artifactToolName));

                return await FetchArtifactTool(true);
            }
            else
            {
                return artifactToolExecutable;
            }
        }
    }
}
