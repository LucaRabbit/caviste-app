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

> Le projet WPF ne référence **que** `DTOs`. Garantit le découplage client/serveur.

---

## 2. Workflow Git et conventions

### 2.1 Stratégie de branches

```
main          ← production, protégée, MERGE chef uniquement
  └── develop ← intégration, protégée, PR obligatoire
        ├── feature/auth-jwt
        ├── feature/vin-crud
        ├── feature/wpf-login
        └── fix/vente-calcul-total
```

**Règle très importante** : personne ne push directement sur `main` ou `develop`.

### 2.2 Workflow quotidien

```bash
git checkout develop
git pull origin develop
git checkout -b feature/ma-tache

# travail + commits réguliers
git commit -m "feat(vin): ajouter VinsController"

git push -u origin feature/ma-tache
# Ouvrir PR sur GitHub → review chef → merge
```

### 2.3 Convention de commits

Format :
```
<type>(<scope>): <description en français>

[corps optionnel]
```

#### Types

| Type       | Usage                                      |
|------------|--------------------------------------------|
| `feat`     | Nouvelle fonctionnalité                    |
| `fix`      | Correction de bug                          |
| `refactor` | Réécriture sans changement de comportement |
| `style`    | Formatage, indentation                     |
| `docs`     | Documentation                              |
| `test`     | Tests                                      |
| `chore`    | Maintenance, dépendances                   |
| `db`       | Migrations EF Core                         |
| `ui`       | XAML, styles WPF                           |
| `api`      | Modifications API                          |

#### Scopes

`auth`, `vin`, `client`, `vente`, `stock`, `fournisseur`, `commande`, `alerte`, `wineapi`, `pdf`, `mail`, `db`, `ui`, `setup`, `jwt`, `cors`

#### Règles

- **Une chose par commit**. Si tu écris "et" dans le message, c'est probablement deux commits.
- Description **< 72 caractères**, à l'**impératif présent**.
- **Pas de point final**.
- Le corps explique le **pourquoi**.

#### Exemple

```
feat(vin): ajouter VinsController avec CRUD complet
```
