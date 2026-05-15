# Caviste App

Application de caisse pour caviste : gestion de stock, ventes, fournisseurs et alertes.

Architecture **client/serveur** : API REST ASP.NET Core 9 + client lourd WPF.

> Projet Collaboratif — CESI

---

## Documentation

Avant de commencer, lire [`.github/docs/caviste_doc.md`](.github/docs/caviste_doc.md) qui couvre l'architecture et le workflow Git.

---

## Démarrage rapide

### 1. Prérequis

- **Visual Studio 2026 Community** avec :
  - .NET desktop development
  - ASP.NET and web development
  - .NET 10 SDK
- **MySQL Server 8.x** (avec MySQL Workbench)
- **Postman** (pour tester l'API)
- **Git**
- **dotnet-ef** (outil global) :
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2. Configurer MySQL

Dans MySQL Workbench, connecté en root :

```sql
CREATE DATABASE caviste_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'caviste_user'@'localhost' IDENTIFIED BY 'motdepasse';
GRANT ALL PRIVILEGES ON caviste_db.* TO 'caviste_user'@'localhost';
FLUSH PRIVILEGES;
```

> Le mot de passe doit correspondre à celui dans `appsettings.Development.json`.

### 3. Cloner le repo

```bash
git clone https://github.com/LucaRabbit/caviste-app.git
cd caviste-app
```

### 4. Créer les fichiers de configuration locaux (gitignorés)

**`src/CavisteApp.Api/appsettings.Development.json`** :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=caviste_db;User=caviste_user;Password=motdepasse;"
  },
  "Jwt": {
    "SecretKey": "une-clé-de-minimum-32-caractères"
  },
  "Mail": {
    "Username": "identifiant-mailtrap",
    "Password": "mdp-mailtrap"
  }
}
```

**`src/CavisteApp.WPF/appsettings.Development.json`** :

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5000/"
  }
}
```

> Ces fichiers sont **gitignorés**. Ne jamais les commiter. Les credentials Mailtrap et la clé JWT seront partagés via un canal privé.

### 5. Appliquer les migrations

```bash
dotnet ef database update --project src/CavisteApp.Api
```

### 6. Lancer l'application

**Méthode Multi-startup Visual Studio 2026** :

Clic droit sur la solution → **Configure Startup Projects** → **Multiple startup projects** → définir `CavisteApp.Api` ET `CavisteApp.WPF` en `Start`. Sauvegarder. F5 lance les deux.

**Méthode manuelle (deux terminaux)** :

```bash
# Terminal 1
dotnet run --project src/CavisteApp.Api

# Terminal 2 (attendre que l'API soit démarrée)
dotnet run --project src/CavisteApp.WPF
```

### 7. Vérification

1. Ouvrir http://localhost:5000/swagger → interface Swagger visible
2. Tester `GET /api/health` → renvoie `"OK"`
3. Dans le WPF, login `admin / admin123` → MainWindow s'ouvre

---

## Comptes de test

Créés automatiquement au premier lancement de l'API par le seeder Identity.

| Login      | Mot de passe  | Rôle           |
|------------|---------------|----------------|
| `admin`    | `admin123`    | Administrateur |
| `visiteur` | `visiteur123` | Visiteur       |

---

## Structure du projet

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

> Le projet WPF ne référence **que** `DTOs`. Garantit le découplage client/serveur.

---

## Stack technique

| Composant          | Choix                                        |
|--------------------|----------------------------------------------|
| Langage            | C# 13 / .NET 10 LTS                          |
| API                | ASP.NET Core Web API 9                       |
| UI                 | WPF (MVVM)                                   |
| ORM                | Entity Framework Core 9.0.15                 |
| BDD                | MySQL 8 (Pomelo 9.0.0)                       |
| Auth               | ASP.NET Core Identity (minimal) + JWT Bearer |
| Hash mots de passe | PBKDF2 (Identity, automatique)               |
| PDF                | QuestPDF                                     |
| Mail               | MailKit + Mailtrap (dev)                     |
| Doc API            | Swagger / OpenAPI                            |
| API externe        | wineapi.io (avec fallback local)             |

---

## Équipe

| Membre         | Rôle                    |
|----------------|-------------------------|
| **Luca**       | Chef d'équipe / Backend |
| **Léo**        | Backend                 |
| **Alexandre**  | Frontend WPF            |

---

## Licence

Projet école — CESI École d'Ingénieurs.