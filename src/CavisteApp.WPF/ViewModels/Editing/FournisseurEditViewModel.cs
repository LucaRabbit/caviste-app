// ViewModels/Editing/FournisseurEditViewModel.cs
using System.Net.Http;
using CavisteApp.DTOs.Fournisseurs;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels.Editing;

public class FournisseurEditViewModel : EditViewModelBase
{
    private readonly IFournisseursApiClient _api;
    private readonly FournisseurDto? _fournisseurExistant;   // null = création

    public FournisseurEditViewModel(IFournisseursApiClient api, FournisseurDto? fournisseurExistant = null)
    {
        _api = api;
        _fournisseurExistant = fournisseurExistant;

        if (fournisseurExistant is not null)
        {
            Nom = fournisseurExistant.Nom;
            Email = fournisseurExistant.Email;
            Telephone = fournisseurExistant.Telephone;
            NumRue = fournisseurExistant.NumRue;
            NomRue = fournisseurExistant.NomRue;
            CodePostal = fournisseurExistant.CodePostal;
            Ville = fournisseurExistant.Ville;
        }

        DefinirErreur(string.IsNullOrWhiteSpace(Nom) ? "* Le nom est obligatoire." : null, nameof(Nom));
    }

    public override string Titre => _fournisseurExistant is null
        ? "Nouveau fournisseur"
        : $"Modifier : {_fournisseurExistant.Nom}";

    public bool EstModification => _fournisseurExistant is not null;

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

    // ─── Email ──────────────────────────────────────────────
    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
                DefinirErreur(string.IsNullOrWhiteSpace(value) ? "* L'email est obligatoire" : null, nameof(Email));
        }
    }

    // ─── Téléphone ──────────────────────────────────────────
    private string _telephone = string.Empty;
    public string Telephone
    {
        get => _telephone;
        set => SetProperty(ref _telephone, value);
    }

    // ─── Numéro de rue ──────────────────────────────────────
    private int _numRue = 0;
    public int NumRue
    {
        get => _numRue;
        set => SetProperty(ref _numRue, value);
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
                DefinirErreur(string.IsNullOrWhiteSpace(value) ? "* Le code postal est obligatoire" : null, nameof(CodePostal));
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

    /// Le fournisseur créé ou modifié (rempli après Enregistrer réussi)
    public FournisseurDto? Resultat { get; private set; }

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

            if (_fournisseurExistant is null)
            {
                Resultat = await _api.CreerAsync(new CreerFournisseurDto
                {
                    Nom = Nom,
                    Email = Email,
                    Telephone = Telephone,
                    NumRue = NumRue,
                    NomRue = NomRue,
                    CodePostal = CodePostal,
                    Ville = Ville
                });
            }
            else
            {
                await _api.ModifierAsync(_fournisseurExistant.Id, new UpdateFournisseurDto
                {
                    Nom = Nom,
                    Email = Email,
                    Telephone = Telephone,
                    NumRue = NumRue,
                    NomRue = NomRue,
                    CodePostal = CodePostal,
                    Ville = Ville
                });

                Resultat = await _api.GetParIdAsync(_fournisseurExistant.Id);
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