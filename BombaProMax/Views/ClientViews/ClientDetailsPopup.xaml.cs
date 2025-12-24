using CommunityToolkit.Maui.Views;
using BombaProMax.Models;
using BombaProMax.Services;

namespace BombaProMax.Views.ClientViews;

public partial class ClientDetailsPopup : Popup
{
    private readonly ClientService _clientService;
    private readonly ClientDto _client;

    public ClientDetailsPopup(ClientService clientService, ClientDto client)
    {
        InitializeComponent();
        _clientService = clientService;
        _client = client;
        BindingContext = _client;

        LoadClientFinancialData();
    }

    private async void LoadClientFinancialData()
    {
        try
        {
            // Load credit balance from service
            var creditBalance = await _clientService.GetClientCreditBalanceAsync(_client.ID);

            if (creditBalance != null)
            {
                TotalCreditLabel.Text = $"{creditBalance.TotalCredit:N2} MAD";
                TotalPaidLabel.Text = $"{creditBalance.TotalPaye:N2} MAD";
                BalanceLabel.Text = $"{creditBalance.Balance:N2} MAD";
                CreditFactureLabel.Text = $"{creditBalance.CreditFacture:N2} MAD";
                NonFactureLabel.Text = $"{creditBalance.CreditNonFacture:N2} MAD";
            }
            else
            {
                SetDefaultFinancialValues();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading client financial data: {ex.Message}");
            SetDefaultFinancialValues();
        }
    }

    private void SetDefaultFinancialValues()
    {
        TotalCreditLabel.Text = "0,00 MAD";
        TotalPaidLabel.Text = "0,00 MAD";
        BalanceLabel.Text = "0,00 MAD";
        CreditFactureLabel.Text = "0,00 MAD";
        NonFactureLabel.Text = "0,00 MAD";
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
