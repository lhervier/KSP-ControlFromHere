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

Le mod embarque aussi un **disjoncteur de poussée** (§9) : il coupe et **verrouille** les moteurs
quand on accélère alors que la poussée réelle n'est pas alignée avec le sens de contrôle du vaisseau,
et propose de reprendre le contrôle depuis le bon module. Activable/désactivable, réglage global
persisté.

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
  le PAW (même vaisseau non commandable) — confirmé par Lionel (cf. §8.3).

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

## 8. Détails techniques

> Décisions tranchées : (1) **siège externe** — on s'en tient strictement à `ModuleCommand` (un
> `ExternalCommandSeat`/`KerbalSeat` ne porte pas de `ModuleCommand` et n'apparaîtra donc pas, c'est
> assumé). (2) **icônes** — on garde l'implémentation existante de `VesselTypeIcons` dans le
> sous-module, sans la retoucher.

### 8.1 Action « Control From Here » (PAS de clic simulé)

L'action PAW est portée par `ModuleCommand` :

```csharp
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
— pose `referenceTransformId = p.flightID`, `referenceTransformPart = p`, et
déclenche `GameEvents.onVesselReferenceTransformSwitch`. (Les sièges/ports de docking, hors scope,
font en plus `part.SetReferenceTransform(controlTransform)` pour viser un transform custom.)

### 8.2 Module actuellement aux commandes (badge « Pilotage »)

```csharp
public Part GetReferenceTransformPart()  // -> referenceTransformPart
```

**Critère : `vessel.GetReferenceTransformPart() == part`.** Robuste et direct. (Le champ
`vessel.referenceTransformId` = `flightID` de cette pièce sert d'équivalent persistant.)

### 8.3 Pas de grisage lié au contrôle

**Confirmé (Lionel) : l'action « Control From Here » reste TOUJOURS présente dans le PAW**,
y compris vaisseau non commandable. L'event `MakeReference` n'est jamais masqué (toujours
`guiActive = true`, aucun `Events["MakeReference"].guiActive = …` dans `ModuleCommand`).

→ **On ne grise donc PAS** le bouton selon la disponibilité du contrôle. Pas besoin de lire
`part.isControlSource` ni `Vessel.IsControllable`. Le seul état désactivé du bouton est « module
déjà aux commandes » (§4, via §8.2).

### 8.4 Rafraîchissement de la fenêtre

Events `GameEvents` à écouter :

| Event | Signature | Couvre |
|---|---|---|
| `onVesselChange` | `EventData<Vessel>` | changement de vaisseau actif |
| `onVesselWasModified` | `EventData<Vessel>` | structure modifiée (inclut docking/undocking, ajout/retrait de pièces) |
| `onVesselReferenceTransformSwitch` | `EventData<Transform, Transform>` | **changement du point de contrôle** (« Control From Here ») → badge « Pilotage » |

- **Ensemble minimal** : les trois ci-dessus. `onVesselWasModified` suffit pour les
  docking/undocking (pas besoin de `onPartCouple`/`onPartUndock` séparés) ; `onVesselPartCountChanged`
  reste une option plus fine si nécessaire.
- Plus de dépendance à l'état de contrôle (grisage supprimé) → pas besoin de
  `onVesselControlStateChange` ni de rafraîchissement périodique.
- **Pattern d'abonnement** (style VesselBookmark) : `GameEvents.x.Add(handler)` dans `Start()`/
  `OnEnable()`, `GameEvents.x.Remove(handler)` dans `OnDestroy()`/`OnDisable()`.

---

## 9. Disjoncteur de poussée

Fonction ajoutée à *ce* mod (pas un mod séparé) : la solution à un désalignement poussée/contrôle est
précisément « prendre le contrôle depuis le bon module de commande », le cœur de métier du mod. Le
disjoncteur crée le besoin « au bon moment », la liste offre l'action qui le résout. Maquette de
référence : [mockup.html](mockup.html) (bandeau toujours visible + 3 états).

### 9.1 Terminologie (verrouillé)

- **Activé** = le disjoncteur est en place et surveille. Réglage **global**, **persisté entre
  scènes/sessions**.
- **Armé** = le disjoncteur est « en haut », poussée autorisée. N'a de sens que si activé.
- **Désarmé** = disjoncteur « tombé », poussée coupée et **verrouillée à 0**. **Ne peut résulter que
  d'un déclenchement automatique** — il n'y a **pas** de désarmement manuel (pour ne plus être gêné,
  on désactive : inutile de pouvoir verrouiller sa propre poussée à la main).

### 9.2 Détection — réactive, sur la poussée réelle (verrouillé)

Principe : n'agir que **pendant** la combustion, sur la poussée **réellement appliquée**, jamais sur
une poussée prédite.

- Hook : `Vessel.OnFlyByWire` (`FlightInputCallback`) sur le vaisseau actif — reçoit le
  `FlightCtrlState` mutable.
- **Vecteur de poussée réel** = `Σ (-thrustTransform.forward) × finalThrust` sur les `ModuleEngines`
  (la force réellement appliquée : `ModuleEngines.cs` fait `AddForceAtPosition(-transform.forward *
  finalThrust ...)`).
- **Sens de contrôle** = `vessel.ReferenceTransform.up` (axe « avant » du navball :
  `FlightGlobals.cs` `LookRotation(referenceTransform.up, -referenceTransform.forward)`).
- **Déclenchement** si `mainThrottle > 0`, poussée réelle non nulle, et
  `Vector3.Angle(pousséeRéelle, ReferenceTransform.up) > seuil`.

Pourquoi réactif et pas prédictif : lire `finalThrust` évite **tous** les cas durs de la prédiction
— moteur activé/allumé/étagé, efficacité atmo (Eve), carburant épuisé : un moteur qui ne pousse pas a
`finalThrust ≈ 0` et ne compte pas, gratuitement. (Approches prédictives via `VesselDeltaV`,
`ModuleEngines.MaxThrustOutputAtm` et `IThrustProvider.OnCenterOfThrustQuery` explorées puis
**écartées** — inutiles ici, et `VesselDeltaV` dépend en plus du réglage `DELTAV_CALCULATIONS_ENABLED`.)

### 9.3 Orientations de contrôle (verrouillé)

`ReferenceTransform.up` reflète **déjà** le point de contrôle actif (OKTO2 « inversé », cabine
« devant »/« dessus »…) : choisir un control point fait `part.SetReferenceTransform(controlPoint.
transform)` (`ModuleCommand.cs`, `SetControlPoint`). Donc **rien à énumérer** — le transform de
référence courant porte la bonne orientation.

### 9.4 Coupure & verrou (latch) (verrouillé)

- Au déclenchement : throttle **persistant** mis à 0 comme la touche X —
  `FlightInputHandler.state.mainThrottle = 0f` (`FlightInputHandler.cs`, `state` est un
  `public static FlightCtrlState`) — pour que le réarmement reparte **de 0**.
- Tant que désarmé : on force aussi `s.mainThrottle = 0f` à chaque frame dans `OnFlyByWire` (couvre
  une tentative de remettre les gaz).
- L'état reste **verrouillé** jusqu'à un réarmement explicite (§9.7).

### 9.5 Snapshot au déclenchement (verrouillé — piège)

Une fois désarmé, le throttle est à 0 → **plus aucune poussée réelle à lire** (`finalThrust = 0`
partout). On **fige (snapshot) la direction de poussée fautive à l'instant du déclenchement**. Ce
vecteur gelé sert à la fois au **message** d'alerte (rappel du seuil) et au **classement des
modules** (§9.6). Il reste valable jusqu'au réarmement / changement de vaisseau.

### 9.6 Suggestion de modules & tri (verrouillé)

- Un module de commande est **« aligné »** si son `ReferenceTransform.up` correspond au vecteur de
  poussée **gelé** (à une tolérance près). **Plusieurs** possibles (une sonde pointe souvent dans le
  même sens qu'une cabine).
- Pendant un déclenchement, la liste se **réordonne** : clé de tri **primaire** `aligné` (les alignés
  en tête, puce **✓ Aligné**), puis le tri habituel (priorité desc, nom, titre).
- **Pas de boutons de suggestion dans le bandeau** (affichage encombré) : on s'appuie sur la vraie
  liste. Le bandeau ne garde que **⤒ Réarmer sans changer**.

### 9.7 Réarmement (verrouillé)

Le disjoncteur se réarme (s'il est activé) sur :

- **⤒ Réarmer sans changer** (bandeau) — repart sans toucher au point de contrôle (poussée
  volontairement hors-axe) ;
- **tout « Control From Here »** — notre bouton `⌖` **ou** l'entrée native du PAW : on capte
  `GameEvents.onVesselReferenceTransformSwitch` (pas seulement le clic de notre bouton) ;
- **changement de vaisseau** — `GameEvents.onVesselChange` (qui impose aussi de re-brancher
  `OnFlyByWire` sur le nouveau vaisseau actif).

### 9.8 Seuil (verrouillé)

- Défaut **5°**, réglable **en direct** (pas dans un menu settings), sur la même ligne que le
  disjoncteur. Persisté (réglage global).
- **Masqué pendant un déclenchement** : le seuil déclencheur est déjà rappelé dans l'alerte ; on
  réarme, puis on ajuste.
- Marge nécessaire : le gimbal fait dévier la poussée instantanée de quelques degrés → un seuil trop
  bas provoquerait des faux positifs.

### 9.9 UI & intégration (verrouillé)

- **Bandeau toujours visible** en tête de fenêtre, 3 états : Désactivé / Activé·Armé / Activé·Désarmé.
- Au déclenchement : **la fenêtre s'affiche d'elle-même** si elle était masquée, et **l'icône de la
  barre d'app clignote**.
- L'icône clignotante est **repointée** sur l'état du disjoncteur — l'ancien `ToolbarWarningAnimator`
  « contrôle hors module de commande » n'y est plus branché.
- L'ancienne ligne « hors liste » (contrôle via port d'amarrage / rien, §2) **reste** comme
  information, mais **découplée** du clignotement.

### 9.10 API KSP de référence

| Besoin | API |
|---|---|
| Override throttle par frame | `Vessel.OnFlyByWire` → `FlightCtrlState.mainThrottle` |
| Couper le throttle persistant | `FlightInputHandler.state.mainThrottle = 0f` (comme la touche X) |
| Poussée réelle | `ModuleEngines.finalThrust` × `-thrustTransform.forward` |
| Sens de contrôle | `Vessel.ReferenceTransform.up` |
| Réarmement | `GameEvents.onVesselReferenceTransformSwitch`, `GameEvents.onVesselChange` |
| Orientation du control point | `ModuleCommand.SetControlPoint` → `Part.SetReferenceTransform` |

---

## 10. Fichiers jetables (à nettoyer)

- [icon_check.png](icon_check.png) — aperçu de vérification de l'icône, supprimable.
- [icon_preview.html](icon_preview.html) — galerie des propositions d'icônes, supprimable une fois
  le choix figé (concept G retenu).
