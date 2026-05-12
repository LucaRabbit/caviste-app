using CavisteApp.DTOs.Auth;

namespace CavisteApp.WPF.Services
{
    public class SessionService
    {
        public string? Token { get; private set; }
        public DateTime? Expiration { get; private set; }
        public UtilisateurDto? Utilisateur { get; private set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && Expiration > DateTime.UtcNow;
        public bool IsAdmin => Utilisateur?.Role == "admin";

        public event EventHandler? SessionChanged;

        public void Connecter(LoginResponse reponse)
        {
            Token = reponse.Token;
            Expiration = reponse.Expiration;
            Utilisateur = reponse.Utilisateur;
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Deconnecter()
        {
            Token = null;
            Expiration = null;
            Utilisateur = null;
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
