using AzureArtifactManager.Model;
using AzureArtifactManager.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AzureArtifactManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string organization;
        private string projectName;
        private string personalAccessToken;

        private AzureArtifactService artifactService;
        private Feed selectedFeed;
        private PackageViewModel selectedPackage;

        public MainWindowViewModel()
        {
            AddNewPackageCommand = new RelayCommand(() => AddNewPackageToFeed(), () => SelectedFeed != null);
            OpenAppConfigCommand = new RelayCommand(() => OpenAppConfig());

            Feeds = new ObservableCollection<Feed>();
            AvailablePackages = new ObservableCollection<PackageViewModel>();

            organization = ConfigurationManager.AppSettings["Organization"];
            projectName = ConfigurationManager.AppSettings["Project"];
            personalAccessToken = ConfigurationManager.AppSettings["PersonalAccessToken"];

            if (HasNoConfigurationError)
            {
                LoadFeeds();
            }
        }

        public string Organization => $"https://dev.azure.com/{organization}";

        public bool HasConfigurationError => string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(personalAccessToken);

        public bool HasNoConfigurationError => !HasConfigurationError;

        public bool ShowPackageDetails => SelectedPackage != null;

        public RelayCommand AddNewPackageCommand { get; }

        public RelayCommand OpenAppConfigCommand { get; }

        public ObservableCollection<Feed> Feeds { get; }

        public Feed SelectedFeed
        {
            get => selectedFeed;
            set
            {
                selectedFeed = value;
                AddNewPackageCommand.RaiseCanExecuteChanged();

                if (selectedFeed != null)
                {
                    LoadPackagesForFeed();
                }

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<PackageViewModel> AvailablePackages { get; }

        public PackageViewModel SelectedPackage
        {
            get => selectedPackage;
            set
            {
                selectedPackage = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ShowPackageDetails));
            }
        }

        private void AddNewPackageToFeed()
        {
            var package = new Package { Name = "new-package" };
            var packageViewModel = new PackageViewModel(SelectedFeed, package, artifactService, true);
            AvailablePackages.Add(packageViewModel);

            SelectedPackage = packageViewModel;
        }

        private async Task LoadPackagesForFeed()
        {
            AvailablePackages.Clear();

            var packages = await artifactService.GetPackagesForFeedAsync(SelectedFeed);

            foreach (var package in packages)
            {
                AvailablePackages.Add(new PackageViewModel(SelectedFeed, package, artifactService));
            }
        }

        private async Task LoadFeeds()
        {
            artifactService = new AzureArtifactService(organization, projectName, personalAccessToken);
            await artifactService.InitializeAsync();

            Feeds.Clear();
            var feeds = await artifactService.GetFeedsAsync();

            foreach (var feed in feeds)
            {
                var projectName = feed.Project?.Name;

                if ((string.IsNullOrEmpty(projectName) && string.IsNullOrEmpty(projectName)) || projectName == this.projectName)
                {
                    Feeds.Add(feed);
                }
            }

            SelectedFeed = Feeds.FirstOrDefault();
        }

        private void OpenAppConfig()
        {
            var appConfigPath = Path.Combine(AppContext.BaseDirectory, "AzureArtifactManager.dll.config");
            var processStartInfo = new ProcessStartInfo("notepad", appConfigPath);
            Process.Start(processStartInfo);
        }
    }
}
