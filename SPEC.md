# KSP-ControlFromHere — Spécification

Note de référence figeant les décisions prises avec Lionel avant implémentation. Sert aussi de
point de reprise si l'implémentation est menée dans une autre conversation.

Conventions : prose en français ; **code et commentaires en anglais**. Commentaire d'en-tête =
**contrat** d'une méthode, jamais l'algorithme (le « comment » se documente en ligne).

---

## 1. Objectif

Mod KSP (scène de **vol**) affichant une fenêtre qui liste les **modules de commande** du vaisseau
courant, avec deux actions par module :

- **Afficher le PAW** de la pièce.
- **Contrôler d'ici** (« Control From Here ») : raccourci qui revient à ouvrir le PAW puis cliquer
  l'action « Control From Here » de la pièce.

Le module qui contrôle actuellement le vaisseau est marqué d'un badge.

Maquette de référence : [mockup.html](mockup.html). Icône de barre d'app validée :
[icon.png](icon.png) (joystick blanc 32×32).

---

## 2. Contenu de la liste

### Filtre d'inclusion (verrouillé)

Une ligne par pièce telle que `part.FindModuleImplementing<ModuleCommand>() != null`.

- Ce critère **exclut naturellement les ports d'amarrage** : ceux-ci offrent « Control From Here »
  via `ModuleDockingNode`, pas via `ModuleCommand`. (Pattern repris de VesselBookmark,
  `BookmarkRefreshManager.cs:51`.)
- Le critère « avoir un `vesselNaming` » serait trop restrictif (un module de commande sans nom
  custom doit apparaître). C'est bien `ModuleCommand` le bon critère.

### Tri (verrouillé)

1. `namingPriority` **décroissant**,
2. puis nom de vaisseau du module (croissant),
3. puis titre de pièce (croissant).

Les égalités sont départagées « au mieux » par ces clés successives.

### Données par ligne (verrouillé)

| Élément | Source |
|---|---|
| Icône | sprite **natif** de type de vaisseau : `VesselTypeIcons.Get(part.vesselNaming?.vesselType ?? vessel.vesselType)`. Repli sur **aucune icône** si `null`. |
| Gros texte | nom du vaisseau porté par le module : `part.vesselNaming?.vesselName ?? vessel.vesselName`. |
| Sous-ligne | `part.partInfo.title` — **déjà localisé** par KSP (tags `#autoLOC_…` résolus au chargement). |
| Tag priorité | `part.vesselNaming.namingPriority`, affiché **seulement si > 0**. Informatif uniquement. |
| Badge « Pilotage » | présent sur le module qui contrôle le vaisseau (le `referenceTransform`). |

---

## 3. Barre de titre (verrouillé)

Nom **global** du vaisseau = `vessel.vesselName`, **lu tel quel** depuis KSP.

> Ne JAMAIS recalculer ce nom à partir des priorités. KSP détermine lui-même quel module donne son
> nom au vaisseau ; on lit seulement le résultat. (Pattern VesselBookmark,
> `BookmarkRefreshManager.cs:231`.)

---

## 4. Actions par ligne (verrouillé)

- **Bouton PAW** `⚙` : icône seule + tooltip ; ouvre le Part Action Window de la pièce.
- **Bouton « Contrôler d'ici »** `⌖` : **icône seule** + tooltip « Contrôler d'ici ». Raccourci de
  l'action PAW « Control From Here » — **appel direct de l'API**, jamais de clic simulé (cf. §8.1).
  Un seul état : **désactivé** sur le module **déjà aux commandes** (tooltip « Déjà aux commandes »).
  Pas d'autre grisage : dans le jeu, l'action « Control From Here » reste **toujours** présente dans
  le PAW (même vaisseau non commandable) — confirmé par Lionel et par le décompilé (cf. §8.3).

---

## 5. Localisation

- Les **titres de pièces** sont déjà localisés par KSP via `part.partInfo.title` — rien à traduire
  de notre côté, et ça couvre les pièces des autres mods.
- Seules **nos propres chaînes d'UI** (titre de fenêtre, tooltips, message « aucun module »…) sont
  à traduire. Mécanisme de localisation : voir comment VesselBookmark le fait (`ModLocalization`)
  et le reprendre.

---

## 6. Dépendances

- **Sous-module partagé `KSP-Shared`** : utiliser le **même commit que KSP-VesselBookmark**, qui
  contient déjà `VesselTypeIcons` (`Get(VesselType)`, `TryGet(string, out Sprite)`, `Available`) —
  fichier `KSP-Shared/Src/shared/ugui/sprites/VesselTypeIcons.cs`. **Rien à réimplémenter.**
- Reprendre la structure d'un des trois mods existants (KSP-VesselBookmark, KSP-SteamInputPlugin,
  KSP-DrawLayerPlugin) : `.csproj`, `Src/`, `GameData/`, scripts de build (`build.bat`/`build.sh`).
- Icône `icon.png` → ira dans `GameData/<DossierMod>/icon.png` (cf. `VesselBookmarkMod/icon.png`).

---

## 7. Contraintes techniques KSP (rappel CLAUDE.md de kspmod)

- **Blocage des clics sous la fenêtre** : en vol, l'empreinte uGUI suffit (`raycastTarget`). **NE
  PAS** poser un masque `InputLockManager` large : `STAGING`/`THROTTLE` sont des bits `[Flags]`, un
  masque large tue staging/throttle en vol.
- Le **menu principal** n'est pas concerné (mod en vol uniquement).

---

## 8. Détails techniques (investigués dans le décompilé `d:\ksp-decompiled`)

> Décisions tranchées : (1) **siège externe** — on s'en tient strictement à `ModuleCommand` (un
> `ExternalCommandSeat`/`KerbalSeat` ne porte pas de `ModuleCommand` et n'apparaîtra donc pas, c'est
> assumé). (2) **icônes** — on garde l'implémentation existante de `VesselTypeIcons` dans le
> sous-module, sans la retoucher.

### 8.1 Action « Control From Here » (PAS de clic simulé)

L'action PAW est portée par `ModuleCommand` :

```csharp
// d:\ksp-decompiled\ModuleCommand.cs:763-767
[KSPEvent(guiActive = true, guiName = "#autoLOC_6001360")]
public virtual void MakeReference()
{
    base.vessel.SetReferenceTransform(base.part);
}
```

**Décision : appeler directement `moduleCommand.MakeReference()`** sur le `ModuleCommand` de la
pièce. C'est une méthode **publique `virtual`** — l'appeler n'est PAS simuler un clic ; c'est
invoquer exactement le code de l'event (et respecter d'éventuelles surcharges, p. ex. le control
point courant). À préférer à un appel « brut » de `vessel.SetReferenceTransform(part)`.

Pour mémoire, l'appel sous-jacent : `Vessel.SetReferenceTransform(Part p, bool storeRecall = true)`
(`Vessel.cs:1419`) — pose `referenceTransformId = p.flightID`, `referenceTransformPart = p`, et
déclenche `GameEvents.onVesselReferenceTransformSwitch`. (Les sièges/ports de docking, hors scope,
font en plus `part.SetReferenceTransform(controlTransform)` pour viser un transform custom.)

### 8.2 Module actuellement aux commandes (badge « Pilotage »)

```csharp
// d:\ksp-decompiled\Vessel.cs:1414
public Part GetReferenceTransformPart()  // -> referenceTransformPart
```

**Critère : `vessel.GetReferenceTransformPart() == part`.** Robuste et direct. (Le champ
`vessel.referenceTransformId` = `flightID` de cette pièce sert d'équivalent persistant.)

### 8.3 Pas de grisage lié au contrôle

**Confirmé (Lionel + décompilé) : l'action « Control From Here » reste TOUJOURS présente dans le PAW**,
y compris vaisseau non commandable. L'event `MakeReference` n'est jamais masqué (toujours
`guiActive = true`, aucun `Events["MakeReference"].guiActive = …` dans `ModuleCommand`).

→ **On ne grise donc PAS** le bouton selon la disponibilité du contrôle. Pas besoin de lire
`part.isControlSource` ni `Vessel.IsControllable`. Le seul état désactivé du bouton est « module
déjà aux commandes » (§4, via §8.2).

### 8.4 Rafraîchissement de la fenêtre

Events `GameEvents` à écouter (vérifiés dans `d:\ksp-decompiled\GameEvents.cs`) :

| Event | Signature | Ligne | Couvre |
|---|---|---|---|
| `onVesselChange` | `EventData<Vessel>` | 339 | changement de vaisseau actif |
| `onVesselWasModified` | `EventData<Vessel>` | 369 | structure modifiée (inclut docking/undocking, ajout/retrait de pièces) |
| `onVesselReferenceTransformSwitch` | `EventData<Transform, Transform>` | 345 | **changement du point de contrôle** (« Control From Here ») → badge « Pilotage » |

- **Ensemble minimal** : les trois ci-dessus. `onVesselWasModified` suffit pour les
  docking/undocking (pas besoin de `onPartCouple`/`onPartUndock` séparés) ; `onVesselPartCountChanged`
  (ligne 371) reste une option plus fine si nécessaire.
- Plus de dépendance à l'état de contrôle (grisage supprimé) → pas besoin de
  `onVesselControlStateChange` ni de rafraîchissement périodique.
- **Pattern d'abonnement** (style VesselBookmark) : `GameEvents.x.Add(handler)` dans `Start()`/
  `OnEnable()`, `GameEvents.x.Remove(handler)` dans `OnDestroy()`/`OnDisable()`.

---

## 9. Fichiers jetables (à nettoyer)

- [icon_check.png](icon_check.png) — aperçu de vérification de l'icône, supprimable.
- [icon_preview.html](icon_preview.html) — galerie des propositions d'icônes, supprimable une fois
  le choix figé (concept G retenu).
