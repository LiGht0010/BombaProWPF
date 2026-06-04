namespace FourniPro.Models;

/// <summary>
/// Wraps a <see cref="ClientDto"/> and exposes display-friendly computed
/// properties for the clients table view.
/// </summary>
public class ClientCardItem(ClientDto dto)
{
    public ClientDto Dto => dto;
    public int ClientId => dto.ClientId;

    public string Nom =>
        string.IsNullOrWhiteSpace(dto.Nom) ? "—" : dto.Nom;

    public string NomSociete =>
        string.IsNullOrWhiteSpace(dto.NomSociete) ? "—" : dto.NomSociete;

    public string NumeroClient =>
        string.IsNullOrWhiteSpace(dto.NumeroClient) ? "—" : dto.NumeroClient;

    public string Contact =>
        string.IsNullOrWhiteSpace(dto.Contact) ? "—" : dto.Contact;

    public string CIN =>
        string.IsNullOrWhiteSpace(dto.CIN) ? "—" : dto.CIN;

    public string Personel =>
        string.IsNullOrWhiteSpace(dto.Personel) ? "—" : dto.Personel;

    public string Adresse =>
        string.IsNullOrWhiteSpace(dto.Adresse) ? "—" : dto.Adresse;
}
