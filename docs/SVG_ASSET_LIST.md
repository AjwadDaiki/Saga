# SVG ASSET LIST — Refonte UI SAGA (à valider avant génération)

> Direction validée par Ajwad (2026-05-28) : **FUN & coloré, puffy 3D sticker, chibis vectoriels, palette M3 vive + or pour la Force, 3 polices, épuré-premium, bottom nav 5 onglets.**
> Ce doc liste TOUT ce qu'il faudra générer en SVG. **Rien n'est généré tant qu'Ajwad n'a pas validé cette liste (PROMPT 2).**

## Conventions communes (tous les SVG)

- **Icônes** : `viewBox="0 0 32 32"` (sauf item glyphs `0 0 40 40`).
- **Chibis** : `viewBox="0 0 120 160"` (ratio 3:4, comme ChibiSamurai référence).
- **Outline** : charcoal `#161d1f`, strokeWidth 2-3px (chibis), 1.5-2.5px (icônes), `strokeLinejoin="round"`.
- **Fills** : tokens DesignTokens (palette M3 + or Force). Pas de `#000` pur.
- **Gloss** : highlight blanc semi-transparent top-left là où pertinent.
- **Flip** : chibis supportent `facing` via `scaleX(-1)` (déjà dans la ref).

## ⚙️ Décisions techniques à acter (impactent le volume)

### T1 — Animation des chibis : transform, PAS frame-by-frame
Les références (`animate-bounce`) animent par **transform CSS** (bounce/squash), pas par frames pixel.
**Recommandation : 1 SVG statique par perso + animation Unity par transform** (bounce idle, squash-stretch sur tap, lean sur attaque, flash sur hurt). Évite de produire ~25 frames/perso.
→ *Implication archi* : `LayeredCharacterRenderer` (conçu pour des `Sprite[]` par anim) passe à 1 frame/slot + tweens DOTween. À traiter au moment de l'intégration, pas maintenant.

### T2 — Chibi monolithique (MVP) vs modulaire Body/Armor/Weapon (Sprint 8+)
Sprint 7 = équipement **stats-only visuel** (Décision D3). Les chibis de référence sont **monolithiques** (armure + arme bakées).
**Recommandation : chibis monolithiques pour le MVP** (1 SVG = perso complet par état). Le split body/armor/weapon vectoriel viendra Sprint 8+ quand l'équipement deviendra visuel (alignement des overlays sur viewBox partagé).

### T3 — Pipeline SVG → Unity
3 options : (a) package `com.unity.vectorgraphics` (import SVG natif), (b) rasterisation SVG→PNG haute réso→Sprite, (c) hand-mesh (non).
**Recommandation : (a) Vector Graphics package** si dispo, sinon (b). À confirmer côté Unity. Les SVG que je génère sont valides dans les deux cas.

---

## CATÉGORIE 1 — CHIBIS PERSONNAGES (viewBox 120×160)

### 1A. Héros joueur
| Asset | Tier | Note |
|-------|------|------|
| **Chibi_Player_Samurai** | 🟢 MVP | Voie de départ. Base = ChibiSamurai ref (armure rouge, topknot, bandeau, katana). |
| Chibi_Player_Viking | 🔵 S8+ | hache, casque cornu, fourrure |
| Chibi_Player_Wuxia | 🔵 S8+ | jian, robe fluide |
| Chibi_Player_Spartiate | 🔵 S8+ | lance, bouclier, casque crête |
| Chibi_Player_Mongol | 🔵 S8+ | arc, fourrure steppe |
| Chibi_Player_Saladin | 🔵 S8+ | cimeterre, turban |
| Chibi_Player_Aztec | 🔵 S8+ | macuahuitl, plumes |
| Chibi_Player_Gaulois | 🔵 S8+ | épée longue, casque ailé, moustache |

> MVP = **1 héros (Samurai)**. Les 8 voies arrivent quand la sélection de voie est jouable (S8+).
> Variantes d'expression optionnelles plus tard : idle / determined / hurt face.

### 1B. Adversaires (mobs communs)
| Asset | Tier | Note |
|-------|------|------|
| **Chibi_Enemy_RoninBandit** | 🟢 MVP | = ChibiEnemy ref (gris, capuche, naginata brisée, yeux rouges). |
| **Chibi_Enemy_Footsoldier** | 🟢 MVP | soldat lambda, variante couleur |
| Chibi_Enemy_Archer | 🔵 S8+ | distance |
| Chibi_Enemy_Brute | 🔵 S8+ | gros, lent |

> MVP = **2 mobs** (rotation visuelle suffisante pour les 5 adversaires Sprint 4 via tint).

### 1C. Capitaines (8 boss mineurs, 1/voie)
| Asset | Tier | Note |
|-------|------|------|
| **Chibi_Capitaine_Sample** (Samurai) | 🟢 MVP | 1 capitaine pour tester le flow boss mineur. |
| Chibi_Capitaine ×7 (autres voies) | 🔵 S8+ | plus grands, armure ornée, aura |

### 1D. Maîtres (8 boss majeurs légendaires)
| Asset | Tier | Note |
|-------|------|------|
| **Chibi_Maitre_Yoshitsune** (Samurai) | 🟢 MVP | 1 maître pour tester intro cinematic + prestige. |
| Chibi_Maitre_Ragnar (Viking) | 🔵 S8+ | |
| Chibi_Maitre_Sun (Wuxia) | 🔵 S8+ | |
| Chibi_Maitre_Leonidas (Spartiate) | 🔵 S8+ | |
| Chibi_Maitre_Subutai (Mongol) | 🔵 S8+ | |
| Chibi_Maitre_Saladin (Saladin) | 🔵 S8+ | |
| Chibi_Maitre_Ahuitzotl (Aztec) | 🔵 S8+ | |
| Chibi_Maitre_Vercingetorix (Gaulois) | 🔵 S8+ | |

**Total chibis : MVP = 5** (1 héros + 2 mobs + 1 capitaine + 1 maître) · **Full = ~27**

---

## CATÉGORIE 2 — ICÔNES UI (viewBox 32×32)

### 2A. Les 18 de référence (shared.jsx) — toutes 🟢 MVP
`Coin` (Force, or + 力) · `Gem` (Échos, losange violet) · `Plus` · `Gear` (settings) · `Skull` (boss/stage) · `Scroll` (quêtes) · `Trophy` (héros/légende) · `Chest` (loot) · `Fist` (upgrade Frappe) · `Lotus` (upgrade Méditation) · `People` (upgrade Disciple) · `Sword` (combat/Dojo) · `Shop` (boutique) · `Lightning` (Vague) · `Zen` (Souffle) · `Clock` (chrono) · `Back` (retour) · `Star` (rareté/note)

### 2B. Icônes additionnelles nécessaires — 🟢 MVP
| Icône | Usage |
|-------|-------|
| **Close (X)** | fermer modals |
| **Check** | validation / équipé |
| **Lock** | contenu verrouillé (voies, items) |
| **MartialArts** | onglet nav "Hero" |
| **Sparkle/AutoAwesome** | onglet nav "Artifacts" / reliques |
| **Heart** ou **Shield** | PV joueur (Sprint 8+ combat) |
| **Crit** (étoile-éclair) | stat critique |
| **ArrowUp** | enhance / upgrade niveau |
| **Pause/Play** | contrôle combat |
| **Bell** | notifications/events |

**Total icônes : ~28** (18 + 10), toutes MVP (elles structurent toute l'UI).

---

## CATÉGORIE 3 — ITEM GLYPHS (équipement, viewBox 40×40)

Pour les slots équipement + inventaire (mappe nos 9 SpriteLayerSets + 8 reliques).
| Glyph | Tier | Mappe à |
|-------|------|---------|
| **katana** | 🟢 MVP | weapon_katana_samurai + relique Yoshitsune |
| **epee_courte** | 🟢 MVP | weapon_epee_courte |
| **baton** | 🟢 MVP | weapon_baton_bois |
| hache (viking) | 🔵 S8+ | weapon_hache_viking + relique Ragnar |
| jian (wuxia) | 🔵 S8+ | weapon_jian_wuxia |
| lance / arc / cimeterre / macuahuitl / epee_longue | 🔵 S8+ | reliques restantes |
| **armor_plastron** | 🟢 MVP | armor_haubert_acier |
| **tunique** | 🟢 MVP | armor_tunique_chanvre |
| **kimono** | 🟢 MVP | armor_kimono_yamato |
| **body_base** | 🟢 MVP | body_chibi_neutral (slot corps) |

**Total item glyphs : MVP = ~7** · Full = ~17

> Note : item glyphs ≠ chibis. Ce sont les **icônes d'inventaire** (petit, dans une case rareté), pas le rendu sur le perso.

---

## CATÉGORIE 4 — BACKGROUNDS ILLUSTRÉS (SVG/composite)

| Asset | Tier | Note |
|-------|------|------|
| **BG_Dojo_DuskSamurai** | 🟢 MVP | Coucher de soleil stylisé : ciel gradient + disque soleil + montagnes silhouette + torii + ligne de sol + sakura. Réutilise le `Background` de combat.jsx. |
| BG_Dojo (par voie) ×7 | 🔵 S8+ | Variantes culturelles (steppe mongole, désert Saladin, jungle Aztec...) |
| BG_Hall_Mystique | 🟡 S7.6 | Radial bleu/violet pour l'écran Prestige (peut être un simple gradient, pas forcément SVG complexe) |

**Total BG : MVP = 1** (Dojo dusk)

---

## CATÉGORIE 5 — FX & DÉCORATIFS (viewBox variable)

| Asset | Tier | Note |
|-------|------|------|
| **Sparkle** (étincelle 4 branches) | 🟢 MVP | titres, achats, boss |
| **Sakura petal** | 🟢 MVP | ambiance Dojo (déjà en particules, version SVG plus jolie) |
| **Slash FX** (trait de coupe) | 🟡 S7.6 | feedback tap (on a déjà un slash pixel) |
| **Combo banner shape** | 🟡 S7.6 | bannière ×N combo tournée |
| **Gloss oval** (réutilisable) | 🟢 MVP | highlight boutons (peut être généré en code aussi) |
| **Star burst** (explosion étoile) | 🔵 S8+ | crit / milestone |

**Total FX : MVP = ~3**

---

## CATÉGORIE 6 — GLYPHES DE VOIE (⚠️ PAS des SVG)

Les 8 glyphes culturels (侍 ᚱ 武 Σ ᠮ ...) sont des **caractères de police**, affichés en TextMeshPro (cf CultureTab). **Aucun SVG à générer** — juste s'assurer que la police les contient ou utiliser des glyphes Unicode. *Listé ici pour mémoire, hors scope SVG.*

---

## RÉCAP VOLUME

| Catégorie | MVP (S7.5/7.6) | Full (S8+) |
|-----------|----------------|------------|
| Chibis | 5 | ~27 |
| Icônes UI | ~28 | ~28 |
| Item glyphs | ~7 | ~17 |
| Backgrounds | 1 | ~9 |
| FX/déco | ~3 | ~6 |
| **TOTAL** | **~44 SVG** | **~87 SVG** |

> **PROMPT 2 (génération) devrait cibler le lot MVP (~44 SVG)** pour débloquer la refonte Dojo + nav immédiatement. Le reste suit au fil des sprints 8+.

---

## ❓ À VALIDER PAR AJWAD avant génération (PROMPT 2)

1. **Périmètre génération** : on génère tout le lot **MVP (~44)** d'un coup, ou on commence encore plus serré (juste chibi Samurai + 28 icônes + BG Dojo = ~30) pour itérer vite sur le style avant de produire le reste ?
2. **T1 — Animation transform (pas frame-by-frame)** : OK pour des chibis statiques animés par squash/stretch/bounce Unity ? (sinon volume ×25)
3. **T2 — Chibis monolithiques MVP** : OK qu'on ne split pas body/armor/weapon en vectoriel maintenant (équipement reste stats-only visuel jusqu'à S8) ?
4. **T3 — Pipeline** : tu confirmes que je peux générer les SVG et que tu installes `com.unity.vectorgraphics` (ou que tu rasterises) côté Unity ?
5. **Chibis — niveau de détail** : on reste sur le niveau ChibiSamurai (~30 paths, mignon mais simple) ou tu veux plus détaillé/plus simple ?
6. **Adversaires** : 2 mobs génériques re-tintés suffisent pour les 5 adversaires Sprint 4, ou tu veux 5 silhouettes distinctes dès le MVP ?

Une fois ces 6 points validés → **PROMPT 2 = je génère le lot retenu**, livré en fichiers SVG (probablement `Assets/_Project/Art/Vector/` organisé par catégorie) + un fichier de preview HTML pour que tu les voies tous d'un coup.
