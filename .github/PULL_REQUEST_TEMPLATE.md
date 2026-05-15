\## Description

<!-- Décrire en 2-3 phrases ce que fait cette PR et POURQUOI.

&#x20;    Exemple : "Ajoute le CRUD complet sur les vins. Permet à l'admin

&#x20;    d'ajouter, modifier, supprimer et ajuster le stock depuis le WPF." -->



\## Type de changement



<!-- Cocher les cases qui s'appliquent (mettre un x dans \[ ]) -->



\- \[ ] Nouvelle fonctionnalité (`feat`)

\- \[ ] Correction de bug (`fix`)

\- \[ ] Refactoring (`refactor`)

\- \[ ] Documentation (`docs`)

\- \[ ] Style / formatage (`style`)

\- \[ ] Base de données / migration (`db`)

\- \[ ] Configuration / dépendances (`chore`)



\## Côtés modifiés



<!-- Cocher tous les projets impactés -->



\- \[ ] \*\*DTOs\*\*

\- \[ ] \*\*Api\*\*

\- \[ ] \*\*WPF\*\*

\- \[ ] \*\*Documentation / Configuration\*\*



\## Tests effectués



<!-- Exemple :

&#x20;    - "Testé via Swagger : POST /api/vins avec admin → 201 Created, le vin apparaît dans GET /api/vins"

&#x20;    - "Testé en WPF : login admin → vue Vins → bouton Ajouter → formulaire → vin créé visible dans la liste"

&#x20;    - "Testé avec visiteur : bouton Supprimer non visible (rôle correctement filtré)" -->



\## Checklist obligatoire



\- \[ ] Le code compile sans erreur ni warning critique

\- \[ ] Test(s) manuel(s) la fonctionnalité (voir section ci-dessus)

\- \[ ] Les commits suivent la convention du projet : `<type>(<scope>): <description>`

\- \[ ] Aucun fichier sensible commité (pas de `appsettings.Development.json`, pas de mots de passe, pas de clé JWT)

\- \[ ] Si modifié `Shared/DTOs` : prévenir l'équipe sur le canal de discussion

\- \[ ] Si modifié une migration EF Core : testé `dotnet ef database update` sur une BDD vide



\## Liens utiles (optionnel)



<!-- Issues GitHub liées, captures d'écran, etc. -->



\## Notes pour le reviewer (optionnel)



<!-- Points d'attention spécifiques, questions, choix d'implémentation à valider, etc. -->





```

