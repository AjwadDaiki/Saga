# 11 - CURRENT STATUS

> Ce fichier est mis à jour par le dev Claude après chaque session.

## État actuel du projet

**Phase**: Sprint 2 closed (validé en play test by Ajwad, mergé `dev` + `main`, tagué `v0.2.0`). **En attente décision artistique avant Sprint 3.**

**Dernière session**: 2026-05-27, dev Claude (Opus 4.7) sur Claude Code

## Sprints terminés

| Sprint | Tag | Description | Status |
|--------|-----|-------------|--------|
| 0 | — | Setup Unity 6 + URP 2D + asmdefs + git + plugins (BreakInfinity, DOTween) | ✅ closed |
| 1 | `v0.1.0` | Core tap loop : compteur ticker, combo 4 tiers, "+X" floating, dust particles, save/reload | ✅ closed |
| 2 | `v0.2.0` | Upgrades de base : Frappe (per-tap), Disciple (passive Force/sec), Méditation (combo amp). UI cards bottom dock, near-miss glow. | ✅ closed |

## Métriques de projet

- **Sprint actuel** : 2/11 closed → Sprint 3 en attente
- **Branches** : `main` + `dev` synced à `0de172c`, pas de feat branch active
- **Tags** : `v0.1.0`, `v0.2.0`
- **Lignes de code C# runtime (hors lib tierce)** : ~1900
- **Tests EditMode** : 47 cases (24 Sprint 1 + 23 Sprint 2)
- **ScriptableObjects** : 3 (Frappe, Disciple, Méditation)
- **Voies implémentées** : 0/8 (Sprint 4+)

## 🚧 En attente : décision artistique pour Sprint 3

Ajwad a demandé au coordinateur de trancher entre :
- **Option A** : pivot complet vers chibi cute pixel art (refs erisesra, Eatventure)
- **Option B** : garder sumi-e dark "Lame & Encre" pour le perso officiel, chibi seulement pour les templates communauté

Cette décision impacte :
- Le rig perso Sprint 3 (Stade 1 Mendiant, Stade 2 Apprenti)
- Le choix Spine 2D vs Unity 2D Animation
- L'identité visuelle des dojos et FX
- La palette par voie

**Tant que cette décision n'est pas tranchée**, Sprint 3 ne démarre pas. Le dev Claude reste disponible pour des polish tasks (cf section suivante).

## Polish items deferred (optionnels, désormais hors-Sprint)

À traiter au choix avant ou pendant Sprint 3 :

1. **Refactor `MainSceneBootstrap` → scene-authored UI** : le runtime-builder est OK fonctionnellement mais le scene-authored est plus idiomatique Unity. Requires `manage_scene` MCP tool (currently flaky). À traiter quand MCP tools sont stables.
2. **Fix 43 warnings `IsPointerOverGameObject`** : Unity 6 InputSystem deprecation. Le call est dans `TapHandler.cs`, fix = passer `Pointer.current.deviceId` en argument. Mérite play test pour vérifier le comportement du UI filter avant de livrer.
3. **Import fonts Inter Variable + JetBrains Mono Variable** : pour vrai ticker tabular sur le compteur Force. Requires TMP Font Asset Creator (Editor UI), pas faisable en filesystem.

Aucun de ces items n'est bloquant pour Sprint 3.

## Questions ouvertes pour le coordinateur

1. **Direction artistique** : chibi cute vs sumi-e dark (cf section au-dessus). **Bloquant Sprint 3**.
2. **Spine 2D vs Unity 2D Animation** : à trancher au Sprint 3 (le doc 06_TECH_STACK.md le mentionne). Probablement influencée par la décision artistique #1.

## Prochaine session (Sprint 3, en attente)

**Objectif** (cf 08_ROADMAP.md) : Stade visuel et milestone — le perso change visuellement aux paliers.

À faire :
- `StadeManager` qui surveille les seuils Force
- Setup rig perso Stade 1 (Mendiant) + Stade 2 (Apprenti avec katana)
- Animations idle + tap-react (Unity 2D Animation OU Spine, selon décision)
- Background dojo Stade 1 + Stade 2 (style selon décision)
- Mini-cinématique 1-2s au passage de stade (ink wash transition OU style chibi, drum hit)
- NumberJuice complet (taille de chiffre selon magnitude, couleur selon palier)
- Sound design : ambient loop différent par stade

**Pré-requis avant Sprint 3** : décision artistique tranchée + (idéalement) MCP tools stables pour l'authoring scene.

## Notes libres

- Sprint 2 a tourné sans accroc majeur après les 3 fixes compile post-refresh (Sign() using, HandleComboChanged sig, TMP API). Pattern asmdef + filesystem-mode commence à être bien rodé.
- Bridge Coplay MCP tools toujours flaky (resource `mcpforunity://instances` OK, tools timeout). On a passé 2 sprints entiers en filesystem-only sans drame, c'est notre nouvelle baseline jusqu'à une release Coplay stable.
- L'Editor utility pattern (`Saga > Sprint N > Generate Y Assets`) sera très utile Sprint 4+ pour générer les voies, esprits, régions. Bonne base reproductible.
- 47 tests EditMode total, tous verts. Coverage data + services solid. UI views non testées (du wiring + visual, pas de métier critique).
- Méditation multiplicatif global a bien marché en play test ("combo amplifié sensible"). Décision validée empiriquement.
- À noter pour Sprint 3+ : si le pivot art est vers chibi cute, le doc 05_VISUAL_STYLE.md devra être updaté en parallèle (la palette "Lame & Encre" reste valide pour UI, mais le character art sera différent).
