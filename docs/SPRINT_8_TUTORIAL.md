# Sprint 8 Phase A — Tutorial Onboarding

> Premier livrable Sprint 8 GAMEPLAY CONTENT.
> Branche : `feat/sprint-8-gameplay-content`.
> Pivot stratégique : UI gelée post-v0.7.7, Ajwad produit ses propres assets art ; Sprint 8 = contenu gameplay sans toucher au UI.

## Objectif

Onboarder un nouveau joueur en 60 secondes lors de la **première session**. Les joueurs existants (saves antérieurs) ne voient PAS le tutorial (auto-skip via migration v9→v10).

## Architecture

```
GameManager.Tutorial : TutorialService POCO
   ├── steps : List<TutorialStep>  (loaded depuis Resources/Tutorial/)
   ├── state.tutorialDone / tutorialStepIndex  (persistés via SaveService v10)
   └── subscribe GameEvents pour les triggers (OnTapResolved, OnUpgradePurchased, ...)
                                  │
                                  ▼
GameEvents.OnTutorialStepShown / Completed / Finished
                                  │
                                  ▼
TutorialOverlayView (sur GameObject TutorialOverlay parent du Canvas)
   ├── Dim full-screen (raycastTarget=false — pas de blocage)
   ├── HighlightRing (PuffySprite.RoundedOutline tinted skyBlue, pulse Sin yoyo)
   ├── SpeechBubble (RhosGFX frameBasicGrey Sliced + Lilita 24sp)
   └── SkipButton (bottom-right au-dessus Bottom Nav)
```

## 6 steps canoniques

Fichiers : `Assets/_Project/Resources/Tutorial/Step_NN_xxx.asset` (générés par menu `Saga > Sprint 8 > Generate Tutorial Steps`).

| # | id                | trigger                              | anchor            | message FR                                                  |
|---|-------------------|--------------------------------------|-------------------|-------------------------------------------------------------|
| 1 | `tap_to_gain`     | TapsReached 5                        | CombatZone        | "Tape sur l'écran\npour gagner de la Force"                 |
| 2 | `force_intro`     | TapsReached 15 (cumulé depuis boot)  | ForcePill         | "Voici ta Force.\nPlus tu en as, plus tu progresses."       |
| 3 | `buy_strike`      | UpgradePurchased                     | UpgradeStrike     | "Achète Strike\npour taper plus fort."                      |
| 4 | `stages_intro`    | OnAdversaireDefeated ∨ OnStadeChanged| StageChip         | "Bats les adversaires\npour avancer les Stades."            |
| 5 | `vague_ready`     | OnElanFull                           | VagueButton       | "Vague est prête !\nDéclenche-la."                          |
| 6 | `settings_bravo`  | OnVagueTriggered                     | SettingsButton    | "Bravo ! Ouvre ici les paramètres.\nBonne aventure."        |

Toutes les bulles : max 2 lignes, ≤ 60 chars.

## Migration SaveService v9 → v10

```csharp
if (state.saveVersion < 10) {
    state.tutorialDone = true;          // legacy player auto-skip
    state.tutorialStepIndex = 0;
}
```

Fresh save (constructor) : `tutorialDone = false` → tutorial joué au premier boot.

## Skip flow

Bouton "Passer le tuto" bottom-right (au-dessus du Bottom Nav, offset Y = 200 px).
`onClick → GameManager.Instance.Tutorial.Skip()` :
- Unsubscribe tous les GameEvents
- `tutorialDone = true`, persist
- Raise `OnTutorialFinished`
- `TutorialOverlayView.HandleFinished` fade-out + Destroy

## Wiring

`MainSceneBootstrap.BuildMainScene` (à la fin, après tous les builders UI) :
```csharp
TutorialOverlayBuilder.Build(ctx);
```

Build no-op si `state.tutorialDone == true`. Sinon construit l'overlay puis lance `gm.Tutorial.Start()`.

## Tests EditMode (Saga.Tests.EditMode/TutorialServiceTests)

- `FirstBoot_StartsAtStep0_AndShowsCurrent`
- `Skip_MarksTutorialDone_AndRaisesFinished`
- `TapsReachedTrigger_AdvancesAfterThresholdTaps`
- `SaveService_MigrationV9toV10_SetsTutorialDoneTrueForLegacy`

Total : **138 → 142 tests verts**.

## Action Ajwad (one-time)

1. Ouvrir Unity, menu **Saga > Sprint 8 > Generate Tutorial Steps** — matérialise les 6 `.asset` dans `Resources/Tutorial/`.
2. (Optionnel) Reset son save si testé sur le device : `PlayerPrefs.DeleteAll()` ou supprimer `Application.persistentDataPath/save.json` pour rejouer le tutorial.
3. Play : observer les 6 prompts dans l'ordre. Skip à tout moment via bottom-right.

## Limites assumées Sprint 8 Phase A

- **Anchor `CombatZone`** : pas de GameObject dédié dans `SceneBuilder` → ring caché + bubble centrée écran. Acceptable car combat occupe le centre.
- **Pas de tail pointing** sur la SpeechBubble (queue qui pointe vers le target) — simple rectangle. À ajouter Sprint 8 Phase polish si demandé.
- **Pas de localisation** — FR hardcodé. Localisation = sprint dédié 10-12 (validation Q3 coordinateur).
- **Pas de re-prompt timeout** — joueur progresse à son rythme (validation Q5).

## Next : Phase B — Voie Samurai complète
