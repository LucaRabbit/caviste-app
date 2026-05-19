// ViewModels/Editing/VinEditViewModel.cs
using System.Net.Http;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Clients;
using CavisteApp.WPF.Services.ApiClient;
using System.Globalization;

namespace CavisteApp.WPF.ViewModels.Editing;

public class ClientEditViewModel : EditViewModelBase
{
    private readonly IClientsApiClient _api;
    private readonly ClientDto? _clientExistant;   // null = création

    public ClientEditViewModel(IClientsApiClient api, ClientDto? clientExistant = null)
    {
        _api = api;
        _clientExistant = clientExistant;

        if (clientExistant is not null)
        {
            Nom = clientExistant.Nom;
            Prenom = clientExistant.Prenom;
            Email = clientExistant.Email;
            Telephone = clientExistant.Telephone;
            NumRueTexte = clientExistant.NumRue.ToString();
            NomRue = clientExistant.NomRue;
            CodePostal = clientExistant.CodePostal;
            Ville = clientExistant.Ville;
        }

        DefinirErreur(string.IsNullOrWhiteSpace(Nom) ? "* Le nom est obligatoire." : null, nameof(Nom));
    }

    public override string Titre => _clientExistant is null
        ? "Nouveau client"
        : $"Modifier : {_clientExistant.Nom}";

    public bool EstModification => _clientExistant is not null;

 

    // ─── Nom ────────────────────────────────────────────────
    private string _nom = string.Empty;
    public string Nom
    {
        get => _nom;
        set
        {
            if (SetProperty(ref _nom, value))
                DefinirErreur(string.IsNullOrWhiteSpace(value) ? "* Le nom est obligatoire" : null, nameof(Nom));
        }
    }

    // ─── Prenom ────────────────────────────────────────────────
    private string _prenom = string.Empty;
    public string Prenom
    {
        get => _prenom;
        set
        {
            if (SetProperty(ref _prenom, value))
            {
                DefinirErreur(string.IsNullOrWhiteSpace(value)
                    ? "* Le prénom est obligatoire"
                    : null);
            }
        }
    }

    // ─── Email ──────────────────────────────────────────────
    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                    DefinirErreur("* L'email est obligatoire.", nameof(Email));
                else if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9.]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                    DefinirErreur("* Format invalide. (Ex.jeandupont@example.com)", nameof(Email));
                else
                    DefinirErreur(null, nameof(Email));
            }
        }
    }

    // ─── Téléphone ──────────────────────────────────────────
    private string _telephone = string.Empty;
    public string Telephone
    {
        get => _telephone;
        set
        {
            if (SetProperty(ref _telephone, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                    DefinirErreur("* Le téléphone est obligatoire.", nameof(Telephone));
                else if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{10}$"))
                    DefinirErreur("* Le téléphone doit contenir 10 chiffres.", nameof(Telephone));
                else
                    DefinirErreur(null, nameof(Telephone));
            }
        }
    }

    // ─── Numéro de rue ──────────────────────────────────────
    private int _numRue;
    private string _numRueTexte = string.Empty;
    public string NumRueTexte
    {
        get => _numRueTexte;
        set
        {
            if (SetProperty(ref _numRueTexte, value))
                ValiderNumRue(value);
        }
    }

    private void ValiderNumRue(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            DefinirErreur("* Le numéro de rue est obligatoire.", nameof(NumRueTexte));
        else if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out var v))
            DefinirErreur("* Veuillez saisir un nombre entier.", nameof(NumRueTexte));
        else if (v <= 0)
            DefinirErreur("* Le numéro doit être supérieur à 0.", nameof(NumRueTexte));
        else
        {
            _numRue = v;
            DefinirErreur(null, nameof(NumRueTexte));
        }
    }

    // ─── Nom de rue ─────────────────────────────────────────
    private string _nomRue = string.Empty;
    public string NomRue
    {
        get => _nomRue;
        set
        {
            if (SetProperty(ref _nomRue, value))
                DefinirErreur(string.IsNullOrWhiteSpace(value) ? "* Le nom de rue est obligatoire" : null, nameof(NomRue));
        }
    }

    // ─── Code postal ────────────────────────────────────────
    private string _codePostal = string.Empty;
    public string CodePostal
    {
        get => _codePostal;
        set
        {
            if (SetProperty(ref _codePostal, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                    DefinirErreur("* Le code postal est obligatoire.", nameof(CodePostal));
                else if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{5}$"))
                    DefinirErreur("Le code postal doit contenir 5 chiffres.", nameof(CodePostal));
                else
                    DefinirErreur(null, nameof(CodePostal));
            }
        }
    }

    // ─── Ville ──────────────────────────────────────────────
    private string _ville = string.Empty;
    public string Ville
    {
        get => _ville;
        set
        {
            if (SetProperty(ref _ville, value))
                DefinirErreur(string.IsNullOrWhiteSpace(value) ? "* La ville est obligatoire" : null, nameof(Ville));
        }
    }

    /// Le client créé ou modifié (rempli après Enregistrer réussi)
    public ClientDto? Resultat { get; private set; }

    public override async Task<bool> EnregistrerAsync()
    {
        if (!EstValide)
        {
            MessageErreur = "Veuillez corriger les erreurs.";
            return false;
        }

        try
        {
            EnregistrementEnCours = true;
            MessageErreur = string.Empty;

            if (_clientExistant is null)
            {
                Resultat = await _api.CreerAsync(new CreerClientDto
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    Telephone = Telephone,
                    NumRue = _numRue,
                    NomRue = NomRue,
                    CodePostal = CodePostal,
                    Ville = Ville
                });
            }
            else
            {
                await _api.ModifierAsync(_clientExistant.Id, new UpdateClientDto
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    Telephone = Telephone,
                    NumRue = _numRue,
                    NomRue = NomRue,
                    CodePostal = CodePostal,
                    Ville = Ville
                });

                Resultat = await _api.GetParIdAsync(_clientExistant.Id);
            }
            return true;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403"))
        {
            MessageErreur = "Action réservée à l'administrateur.";
            return false;
        }
        catch (Exception ex)
        {
            MessageErreur = ex.Message;
            return false;
        }
        finally
        {
            EnregistrementEnCours = false;
        }
    }
}