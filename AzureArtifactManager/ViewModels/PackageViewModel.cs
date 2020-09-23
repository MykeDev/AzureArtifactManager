using AzureArtifactManager.Model;
using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using AzureArtifactManager.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using GalaSoft.MvvmLight;

namespace AzureArtifactManager.ViewModels
{
    public class PackageViewModel : ViewModelBase
    {
        private readonly Feed feed;
        private readonly AzureArtifactService azureArtifactService;

        public PackageViewModel(Feed feed, Package package, AzureArtifactService azureArtifactService, bool isNew = false)
        {
            this.feed = feed;
            this.azureArtifactService = azureArtifactService;
            IsNewPackage = isNew;
            DownloadSelectedPackageCommand = new RelayCommand(() => DownloadSelectedPackage());
            UploadNewVersionCommand = new RelayCommand(() => UploadNewPackageToFeed());

            Version = package.LatestVersion?.Version ?? string.Empty;
            Name = package.Name;
            Description = package.LatestVersion?.Description ?? string.Empty;
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public bool IsNewPackage { get; private set; }

        public RelayCommand DownloadSelectedPackageCommand { get; }

        public RelayCommand UploadNewVersionCommand { get; }

        private void UploadNewPackageToFeed()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                azureArtifactService.PublishPackage(feed, Name.ToLowerInvariant(), Version, Description, dialog.FileName);
            }
        }

        private void DownloadSelectedPackage()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                azureArtifactService.DownloadPackage(feed, Name.ToLowerInvariant(), Version, dialog.FileName);
            }
        }
    }
}
