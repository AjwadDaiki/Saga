# DESIGN DECISIONS LOG

> Chaque décision de design (game design, visuel, ou architecture importante) prise en cours de dev est loggée ici.
> Format: date, contexte, décision, raison.
> Cela évite de relitiger les mêmes points et garde une traçabilité.

## 2026-05-27 — Direction visuelle verrouillée

**Décision**: Direction A "Lame & Encre" (sumi-e moderne dark) comme base, avec accents C "Voie Brutale" (high contrast) aux moments épiques (prestige, boss kill, unlock voie, milestone Mythe).

**Raison**: Direction A gagne sur la longueur (sessions du soir, OLED-friendly, ton prestigieux qui matche l'univers mythique-humain). Direction C ajoute le punch nécessaire aux pivots narratifs. Direction B (Parchemin) écartée pour éviter la confusion identitaire.

**Conséquence**: tous les assets et UI suivent la palette définie dans 05_VISUAL_STYLE.md.

---

## 2026-05-27 — Suppression du tier "Divin" / "Ascension céleste"

**Décision**: Le palier final visuel est **Mythe** (humain devenu légende), pas un tier divin. 6 stades au total (Mendiant → Mythe), pas 7.

**Raison**: Le ton du jeu est mythique-humain (cf 01_VISION.md). Une ascension divine cassait la narration "underdog devenu légende par la mémoire". L'arc final est "reconstituer la Voie Originelle" via la connaissance des cultures, pas devenir un dieu.

**Conséquence**: pas de currency "Essence Divine". On reste sur 4 currencies (Force, Technique, Renom, Échos). Régions mythiques (Olympe, etc.) restent dans le ton "terres légendaires", pas dimensions divines.

---

## 2026-05-27 — Stack technique de base

**Décision**: Unity 6 LTS + URP 2D + BreakInfinity.cs + DOTween (free puis Pro à 15$) + TextMeshPro + Addressables + 2D Animation + Cinemachine + Newtonsoft.Json + Coplay Unity MCP.

**Raison**: Setup éprouvé pour idle game mobile, libs gratuites ou peu chères, MCP déjà maitrisé par Ajwad.

**Conséquence**: voir 06_TECH_STACK.md pour la liste complète. Spine 2D et FMOD restent des décisions ouvertes à trancher en cours de dev.

---

## 2026-05-27 — Sprint 0: DOTween free imported manually via Asset Store

**Décision**: DOTween free n'est pas auto-installé via le script Sprint 0. Importé manuellement par Ajwad via Asset Store au focus Unity. Location effective: `Assets/Plugins/Demigiant/DOTween/` (DOTween crée son propre subfolder `Demigiant/` sous `Plugins/`).

**Raison**: DOTween free n'est pas dispo sur OpenUPM ni en UPM officiel. Télécharger un .zip du site demigiant.com sans validation = risque. L'install via Asset Store est triviale (Window → Package Manager → My Assets → DOTween → Import) et standard chez tous les devs Unity.

**Conséquence**: 
- `Assets/Plugins/DOTween/` placeholder supprimé (DOTween a créé son propre `Assets/Plugins/Demigiant/DOTween/`)
- 06_TECH_STACK.md ligne 129 mentionne `Plugins/DOTween/` — légèrement off, la realité est `Plugins/Demigiant/DOTween/`. Pas critique, à amender au prochain pass doc.
- `DOTween.dll` et `DOTweenEditor.dll` seront stockés via Git LFS (pattern `*.dll` dans `.gitattributes`).
- Sprint 0 DOTween: ✅ FAIT.

---

## 2026-05-27 — Sprint 0: Localization tables setup manuel par Ajwad

**Décision**: Le package `com.unity.localization` est installé et résolu (✅), mais la création des tables `UI_Common` (FR + EN) sera faite manuellement par Ajwad via l'Editor UI quand il rouvrira Unity. Étapes:
1. Window > Asset Management > Localization Tables > Create > String Table Collection
2. Nom: `UI_Common`, locales: FR + EN, location: `Assets/_Project/Data/Localization/`
3. Window > Asset Management > Localization Settings > Available Locales: ajouter Locale French (fr) + Locale English (en)

**Raison**: Écrire à la main les assets YAML du Localization package (LocalizationSettings.asset, Locale_fr.asset, Locale_en.asset, StringTableCollection, StringTable) est risqué — format propriétaire, fileIDs spécifiques. L'Editor UI fait ça en 30 secondes. Pas bloquant pour le critère de succès Sprint 0 (app boot + GameManager OK).

**Conséquence**: 
- Sprint 0 marqué "Localization-pending" jusqu'à l'étape manuelle.
- Ajout du folder `Assets/_Project/Data/Localization/` à créer au moment du setup.

---

## 2026-05-27 — Sprint 0: Switch Platform Android manuel par Ajwad

**Décision**: Le switch de la plateforme cible vers Android ne peut pas se faire en filesystem direct (Unity stocke l'état dans `Library/EditorUserBuildSettings.asset` qui est gitignored et créé lazily par le Build Settings window). Ajwad fait le switch via File > Build Settings > Android > Switch Platform au prochain focus Unity (30 sec, Android Build Support déjà installé).

**Raison**: Le bridge MCP était down pendant Sprint 0, et même via MCP `manage_editor` n'expose pas de "switch active build target". Le switch déclenche aussi une recompile pour le platform Android — c'est une action Unity native qu'on ne peut pas reproduire en filesystem proprement.

**Conséquence**: ProjectSettings est correctement configuré pour Android (bundle id `com.hiddenlab.saga`, ARM64 + ARMv7, IL2CPP backend), il manque juste le switch effectif.

---

## 2026-05-27 — Sprint 0: package versions Unity 6 pinées dans manifest.json

**Décision**: Versions installées pour Unity 6.3 (6000.3.9f1):
- Addressables 2.6.0
- Cinemachine 3.1.4 (CM3 pour Unity 6)
- Localization 1.5.5
- Mobile Notifications 2.4.1
- Newtonsoft Json 3.2.1

**Raison**: Fallback filesystem direct sur `Packages/manifest.json` car le bridge MCP a perdu la connexion pendant la session. Versions choisies = stable et connues compatibles Unity 6.

**Conséquence**: Unity re-resolve auto au refocus. Si une version pose problème, ajustement trivial dans le manifest. À vérifier au checkpoint compile.

---

## Template pour nouvelles entrées

```
## YYYY-MM-DD — [Titre court]

**Décision**: ...

**Raison**: ...

**Conséquence**: ...
```
