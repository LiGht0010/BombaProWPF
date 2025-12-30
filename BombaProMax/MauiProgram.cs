using BombaProMax.Views;
using BombaProMax.Views.AchatViews;
using BombaProMax.Views.PompeViews;
using BombaProMax.Views.EmployeViews;
using BombaProMax.Views.ChauffeurViews;
using BombaProMax.Views.CamionViews;
using BombaProMax.Views.CiterneViews;
using BombaProMax.Views.ClientViews;
using BombaProMax.Views.DashboardViews;
using BombaProMax.Views.DepenseViews;
using BombaProMax.Views.FournisseurViews;
using BombaProMax.Views.ReservoirViews;
using BombaProMax.Views.ReservoirCalibrationViews;
using BombaProMax.Views.JaugeageViews;
using BombaProMax.Views.ProduitViews;
using BombaProMax.Views.PeriodeViews;
using BombaProMax.Views.User;
using BombaProMax.Views.VenteLubEtArticles;
using BombaProMax.Views.FactureViews;
using BombaProMax.Views.RapportViews;
using BombaProMax.Views.ServiceViews;
using BombaProMax.Views.VenteServiceViews;
using BombaProMax.Views.CaisseViews;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BombaProMax
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                // Initialize the .NET MAUI Community Toolkit by adding the below line of code
                .UseMauiCommunityToolkit()
                // After initializing the .NET MAUI Community Toolkit, optionally add additional fonts
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            //Views---------------------------------------------

            //LoginPage neummorphic
            builder.Services.AddSingleton<LoginPage>();

            //User
            builder.Services.AddSingleton<User>();
            builder.Services.AddSingleton<UserCreatePopup>();
            builder.Services.AddSingleton<UserEditPopup>();
            builder.Services.AddSingleton<UserDetailsPopup>();
            builder.Services.AddSingleton<HomePage>();

            //Main
            builder.Services.AddSingleton<MainPage>();

            //builder.Services.AddSingleton<RegisterPage>();

            //Other Pages
            builder.Services.AddSingleton<AboutPage>();
            builder.Services.AddSingleton<ContactPage>();

            //DashboardViews
            builder.Services.AddSingleton<DashboardPage>();

            //ClientViews
            builder.Services.AddSingleton<ClientPage>();
            builder.Services.AddSingleton<ClientCreatePopup>();
            builder.Services.AddSingleton<ClientEditPopup>();
            builder.Services.AddSingleton<ClientDetailsPopup>();
            builder.Services.AddTransient<ClientCreditManagement>();

            //ProduitViews
            builder.Services.AddSingleton<ProduitPage>();
            builder.Services.AddSingleton<ProduitCreatePopup>();
            builder.Services.AddSingleton<ProduitEditPopup>();

            //FournisseurViews
            builder.Services.AddSingleton<FournisseurPage>();
            builder.Services.AddSingleton<FournisseurCreatePopup>();
            builder.Services.AddSingleton<FournisseurEditPopup>();
            builder.Services.AddSingleton<FournisseurDetailsPopup>();

            //ChauffeurViews
            builder.Services.AddSingleton<ChauffeurPage>();
            builder.Services.AddSingleton<ChauffeurCreatePopup>();
            builder.Services.AddSingleton<ChauffeurEditPopup>();
            builder.Services.AddSingleton<ChauffeurDetailsPopup>();

            //CamionViews
            builder.Services.AddSingleton<CamionPage>();
            builder.Services.AddSingleton<CamionCreatePopup>();
            builder.Services.AddSingleton<CamionEditPopup>();
            builder.Services.AddSingleton<CamionDetailsPopup>();

            //CiterneViews
            builder.Services.AddSingleton<CiternePage>();
            builder.Services.AddSingleton<CiterneCreatePopup>();
            builder.Services.AddSingleton<CiterneEditPopup>();
            builder.Services.AddSingleton<CiterneDetailsPopup>();

            //EmployeViews
            builder.Services.AddSingleton<EmployePage>();
            builder.Services.AddSingleton<EmployeCreatePopup>();
            builder.Services.AddSingleton<EmployeEditPopup>();
            builder.Services.AddSingleton<EmployeDetailsPopup>();

            //ReservoirViews
            builder.Services.AddSingleton<ReservoirPage>();
            builder.Services.AddSingleton<ReservoirCreatePopup>();
            builder.Services.AddSingleton<ReservoirEditPopup>();
            builder.Services.AddSingleton<ReservoirDetailsPopup>();

            //ReservoirCalibrationViews
            builder.Services.AddSingleton<ReservoirCalibrationPage>();
            builder.Services.AddTransient<CalibrationImportPopup>();

            //JaugeageViews
            builder.Services.AddSingleton<JaugeagePage>();

            //PompeViews
            builder.Services.AddSingleton<PompePage>();
            builder.Services.AddSingleton<PompeCreatePopup>();
            builder.Services.AddSingleton<PompeEditPopup>();
            builder.Services.AddSingleton<PompeDetailsPopup>();

            //AchatViews
            builder.Services.AddSingleton<AchatPage>();
            builder.Services.AddSingleton<AchatCreatePopup>();
            builder.Services.AddSingleton<AchatEditPopup>();
            builder.Services.AddSingleton<AchatDetailsPopup>();
            builder.Services.AddSingleton<AchatAllocationPopup>();

            //PeriodeViews
            builder.Services.AddSingleton<PeriodePage>();
            builder.Services.AddSingleton<PeriodeCreatePopup>();
            builder.Services.AddSingleton<PeriodeDetailsPopup>();

            //VenteLubrifiantsEtArticlesViews
            builder.Services.AddSingleton<VenteLubrifiantsEtArticlesPage>();
            builder.Services.AddSingleton<VenteLubrifiantsEtArticlesCreatePopup>();

            //DepenseViews
            builder.Services.AddSingleton<DepensePage>();
            builder.Services.AddSingleton<DepenseCreatePopup>();

            //FactureViews
            builder.Services.AddSingleton<FacturationPage>();
            builder.Services.AddSingleton<FactureEtBL>();

            //RapportViews
            builder.Services.AddSingleton<RapportPage>();

            //ServiceViews
            builder.Services.AddSingleton<ServicePage>();
            builder.Services.AddTransient<ServiceCreatePopup>();
            builder.Services.AddTransient<ServiceCategorieCreatePopup>();

            //VenteServiceViews
            builder.Services.AddSingleton<VenteServicePage>();
            builder.Services.AddTransient<VenteServiceCreatePopup>();

            //CaisseViews
            builder.Services.AddSingleton<CaissePage>();
            builder.Services.AddTransient<DepotCaisseCreatePopup>();
            builder.Services.AddTransient<DepotCaisseDetailsPopup>();

            //ViewModels---------------------------------------------------
            builder.Services.AddSingleton<ViewModels.LoginPageViewModel>();
            builder.Services.AddSingleton<ViewModels.DashboardViewModel>();
            builder.Services.AddSingleton<ViewModels.ClientViewModel>();
            builder.Services.AddSingleton<ViewModels.AchatViewModel>();
            builder.Services.AddSingleton<ViewModels.ChauffeurViewModel>();
            builder.Services.AddSingleton<ViewModels.CamionViewModel>();
            builder.Services.AddSingleton<ViewModels.CiterneViewModel>();
            builder.Services.AddSingleton<ViewModels.FournisseurViewModel>();
            builder.Services.AddSingleton<ViewModels.ProduitViewModel>();
            builder.Services.AddSingleton<ViewModels.ReservoirViewModel>();
            builder.Services.AddSingleton<ViewModels.ReservoirCalibrationViewModel>();
            builder.Services.AddSingleton<ViewModels.JaugeageViewModel>();
            builder.Services.AddSingleton<ViewModels.PompeViewModel>();
            builder.Services.AddSingleton<ViewModels.PeriodeViewModel>();
            builder.Services.AddSingleton<ViewModels.VenteLubrifiantsEtArticlesViewModel>();
            builder.Services.AddSingleton<ViewModels.DepenseViewModel>();
            builder.Services.AddSingleton<ViewModels.FactureViewModel>();
            builder.Services.AddSingleton<ViewModels.FactureEtBLViewModel>();
            builder.Services.AddSingleton<ViewModels.UserViewModel>();
            builder.Services.AddSingleton<ViewModels.RapportViewModel>();
            builder.Services.AddSingleton<ViewModels.ServiceViewModel>();
            builder.Services.AddSingleton<ViewModels.VenteServiceViewModel>();
            builder.Services.AddSingleton<ViewModels.CaisseViewModel>();

            //Services-----------------------------------------------------
            builder.Services.AddSingleton<Services.IDialogService, Services.DialogService>();
            builder.Services.AddSingleton<Services.UserService>();
            builder.Services.AddSingleton<Services.ClientService>();
            builder.Services.AddSingleton<Services.CreditTransactionService>();
            builder.Services.AddSingleton<Services.ReglementCreditService>();
            builder.Services.AddSingleton<Services.ProduitService>();
            builder.Services.AddSingleton<Services.CategorieService>();
            builder.Services.AddSingleton<Services.FournisseurService>();
            builder.Services.AddSingleton<Services.CamionService>();
            builder.Services.AddSingleton<Services.CiterneService>();
            builder.Services.AddSingleton<Services.EmployeService>();
            builder.Services.AddSingleton<Services.ReservoirService>();
            builder.Services.AddSingleton<Services.ReservoirCalibrationService>();
            builder.Services.AddSingleton<Services.PompeService>();
            builder.Services.AddSingleton<Services.AchatService>();
            builder.Services.AddSingleton<Services.AchatAllocationService>();
            builder.Services.AddSingleton<Services.ChauffeurService>();
            builder.Services.AddSingleton<Services.PeriodeService>();
            builder.Services.AddSingleton<Services.VenteLubrifiantsEtArticlesService>();
            builder.Services.AddSingleton<Services.DepenseService>();
            builder.Services.AddSingleton<Services.BonLivraisonService>();
            builder.Services.AddSingleton<Services.FactureService>();
            builder.Services.AddSingleton<Services.JourneeNavigationService>();
            builder.Services.AddSingleton<Services.JaugeageService>();
            builder.Services.AddSingleton<Services.JaugeageDetailService>();
            builder.Services.AddSingleton<Services.DashboardService>();
            builder.Services.AddSingleton<Services.RapportService>();
            builder.Services.AddSingleton<Services.ServiceService>();
            builder.Services.AddSingleton<Services.ServiceCategorieService>();
            builder.Services.AddSingleton<Services.VenteServiceService>();
            builder.Services.AddSingleton<Services.CaisseService>();

            //Interfaces--------------------------------------------------------------------------------------------------------------------------------------
            builder.Services.AddSingleton<Services.ILoginRepository, Services.LoginServices>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
