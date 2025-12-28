namespace BombaProMax.Services;

/// <summary>
/// Centralized API configuration. Change BaseUrl here to switch between environments.
/// </summary>
public static class ApiConfig
{
    // ===========================================
    // CHANGE THIS URL TO SWITCH ENVIRONMENTS
    // ===========================================

    // Production (Debian Server)
    //public const string BaseUrl = "http://62.84.189.17:5000/api";

    // Development (Local)
    public const string BaseUrl = "https://localhost:7100/api";

    // ===========================================
    // API Endpoints
    // ===========================================
    public static string Users => $"{BaseUrl}/Users";
    public static string Clients => $"{BaseUrl}/Clients";
    public static string Produits => $"{BaseUrl}/Produits";
    public static string Categories => $"{BaseUrl}/Categories";
    public static string Fournisseurs => $"{BaseUrl}/Fournisseurs";
    public static string Achats => $"{BaseUrl}/Achats";
    public static string AchatAllocations => $"{BaseUrl}/AchatAllocations";
    public static string Factures => $"{BaseUrl}/Factures";
    public static string BonLivraisons => $"{BaseUrl}/BonLivraisons";
    public static string Employes => $"{BaseUrl}/Employes";
    public static string Chauffeurs => $"{BaseUrl}/Chauffeurs";
    public static string Camions => $"{BaseUrl}/Camions";
    public static string Citernes => $"{BaseUrl}/Citernes";
    public static string Pompes => $"{BaseUrl}/Pompes";
    public static string Reservoirs => $"{BaseUrl}/Reservoirs";
    public static string ReservoirCalibrations => $"{BaseUrl}/ReservoirCalibrations";
    public static string Jaugeages => $"{BaseUrl}/Jaugeages";
    public static string JaugeageDetails => $"{BaseUrl}/JaugeageDetails";
    public static string Depenses => $"{BaseUrl}/Depenses";
    public static string DepenseCategories => $"{BaseUrl}/DepenseCategories";
    public static string MoyensPaiement => $"{BaseUrl}/MoyensPaiements";
    public static string BilanCredits => $"{BaseUrl}/BilanCredits";
    public static string CreditTransactions => $"{BaseUrl}/CreditTransactions";
    public static string ReglementCredits => $"{BaseUrl}/ReglementCredits";
    public static string Periodes => $"{BaseUrl}/Periodes";
    public static string Services => $"{BaseUrl}/Services";
    public static string ServiceCategories => $"{BaseUrl}/ServiceCategories";
    public static string VenteLubrifiantsEtArticles => $"{BaseUrl}/VenteLubrifiantsEtArticles";
    public static string VenteServices => $"{BaseUrl}/VenteServices";
    public static string Dashboard => $"{BaseUrl}/Dashboard";
    public static string Rapports => $"{BaseUrl}/Rapports";
    public static string Home => $"{BaseUrl}";
 }
