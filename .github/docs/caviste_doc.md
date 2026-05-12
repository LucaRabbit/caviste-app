## 1. Structure du projet

```
src/
├── CavisteApp.Api/        Backend complet : entités, DbContext, services, controllers
│   ├── Entities/
│   ├── Enums/
│   ├── Constants/
│   ├── Data/              (DbContext + migrations)
│   ├── Services/          (Auth, Mail, PDF)
│   ├── Controllers/
│   └── Middleware/
├── CavisteApp.DTOs/       Contrats partagés API ↔ WPF
└── CavisteApp.WPF/        Client lourd WPF (MVVM)
    ├── Services/
    ├── ViewModels/
    └── Views/
```
## 4. Lexique des concepts utilisés dans ce projet.

### Swagger
Documentation interactive de l'API générée automatiquement par ASP.NET Core à partir des controllers et DTOs. Accessible sur `http://localhost:5000/swagger`. Permet de **tester l'API sans le WPF** et **sans Postman** pour les cas simples.

### Middleware
Composant qui intercepte chaque requête HTTP **avant** qu'elle atteigne le controller, et chaque réponse **avant** qu'elle reparte. Forme une chaîne de filtres. On l'utilise pour : CORS, authentification JWT, gestion centralisée des exceptions, logging. Évite de dupliquer ce code dans chaque controller.

### DI (Injection de dépendances)
Au lieu qu'un controller crée ses dépendances (DbContext, services), il les **reçoit** via son constructeur. Configuré une fois dans `Program.cs`. Permet le découplage et la testabilité. Trois durées de vie : `Singleton` (1 instance pour toute l'app), `Scoped` (1 instance par requête HTTP — défaut côté API), `Transient` (1 instance à chaque demande).

### CORS (Cross-Origin Resource Sharing)
Mécanisme qui autorise un site web à appeler une API hébergée sur un autre domaine/port. Concerne uniquement les **navigateurs** (le WPF s'en fiche techniquement), mais on le configure pour permettre les tests via Swagger ou outils web. En dev : politique permissive. En prod : on whitelist les origines.

### ASP.NET Core Identity (minimal)
Framework officiel de Microsoft pour gérer les utilisateurs et les rôles. On l'utilise en mode **minimaliste** (`AddIdentityCore` au lieu de `AddIdentity` complet) : Identity gère le stockage des users dans 7 tables `AspNet*`, le hash automatique des mots de passe en **PBKDF2**, et les rôles en BDD. On désactive les fonctionnalités avancées (2FA, lockout, confirmation email) qu'on n'utilise pas. La génération du JWT reste **custom** via notre `JwtService` pour avoir le contrôle sur les claims.

### JWT (JSON Web Token)
Token signé qui contient l'identité et les rôles de l'utilisateur. Émis par notre `JwtService` au login (après vérification du mot de passe par Identity), envoyé par le client à chaque requête dans le header `Authorization: Bearer {token}`. L'API vérifie la signature à chaque requête sans interroger la BDD. **Stateless** : pas de session côté serveur.

### MVVM (Model-View-ViewModel)
Pattern WPF. La **Vue** (XAML) ne contient que de l'affichage. Le **ViewModel** contient la logique de présentation et les commandes. Le **Modèle** (ici : DTOs) représente les données. Liaison via **data binding** XAML.

### DTO (Data Transfer Object)
Objet simple destiné à transiter entre l'API et le client en JSON. Différent des entités EF Core qui contiennent des relations et de la logique métier. Dans `CavisteApp.DTOs`.

---

## 3. Workflow Git et conventions

### 3.1 Stratégie de branches

```
main          ← production, protégée, MERGE chef uniquement
  └── develop ← intégration, protégée, PR obligatoire
        ├── feature/auth-jwt
        ├── feature/vin-crud
        ├── feature/wpf-login
        └── fix/vente-calcul-total
```

**Règle d'or** : personne ne push directement sur `main` ou `develop`.

### 3.2 Workflow quotidien

```bash
git checkout develop
git pull origin develop
git checkout -b feature/ma-tache

# travail + commits réguliers
git commit -m "feat(vin): ajouter VinsController"

git push -u origin feature/ma-tache
# Ouvrir PR sur GitHub → review chef → merge
```

### 3.3 Convention de commits

Format :
```
<type>(<scope>): <description en français>

[corps optionnel]
```

#### Types

| Type | Usage |
|---|---|
| `feat` | Nouvelle fonctionnalité |
| `fix` | Correction de bug |
| `refactor` | Réécriture sans changement de comportement |
| `style` | Formatage, indentation |
| `docs` | Documentation |
| `test` | Tests |
| `chore` | Maintenance, dépendances |
| `db` | Migrations EF Core |
| `ui` | XAML, styles WPF |
| `api` | Modifications API |

#### Scopes

`auth`, `vin`, `client`, `vente`, `stock`, `fournisseur`, `commande`, `alerte`, `wineapi`, `pdf`, `mail`, `db`, `ui`, `setup`, `jwt`, `cors`

#### Règles

- **Une chose par commit**. Si tu écris "et" dans le message, c'est probablement deux commits.
- Description **< 72 caractères**, à l'**impératif présent**.
- **Pas de point final**.
- Le corps explique le **pourquoi**.
- **2 à 5 commits par jour minimum** par personne.

#### Exemples

```
feat(vin): ajouter VinsController avec CRUD complet

5 endpoints : GET liste, GET par ID, POST, PUT, DELETE.
Le POST/PUT/DELETE exigent le rôle Administrateur via [Authorize(Policy)].

---