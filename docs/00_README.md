# SAGA - Pack de documentation dev

Projet: **SAGA** (working title), jeu mobile idle/incremental sur Unity 6, plateforme iOS + Android.

Studio: HiddenLab. Lead: Ajwad.

## Comment ce pack fonctionne

Ce dossier `/docs` contient toute la connaissance partagée nécessaire pour développer SAGA. Le dev Claude (sur Claude Desktop, connecté à Coplay Unity MCP) lit ces fichiers avant de coder. Le coordinateur (Claude dans le chat web d'Ajwad) garde la vision globale et arbitre les décisions.

## Ordre de lecture conseillé

Pour comprendre le projet de zéro, lire dans cet ordre:

1. **01_VISION.md** - la pitch, le ton, la cible émotionnelle
2. **02_GAME_DESIGN.md** - les mécaniques core et les principes d'addiction
3. **03_CULTURES.md** - les 8 voies de combat et le système hybride
4. **04_PROGRESSION.md** - tiers visuels, currencies, prestige
5. **05_VISUAL_STYLE.md** - direction artistique verrouillée (A: Lame & Encre)
6. **06_TECH_STACK.md** - Unity et toutes les libs/packages
7. **07_ARCHITECTURE.md** - patterns code, ScriptableObject, save system
8. **08_ROADMAP.md** - sprints, MVP scope, ordre des features
9. **09_CONVENTIONS.md** - folder structure, naming, code style
10. **10_COORDINATOR_PROTOCOL.md** - comment communiquer avec Ajwad et le coordinateur

## Fichiers vivants

- **11_CURRENT_STATUS.md** : créé et mis à jour par le dev Claude après chaque session. État du projet, ce qui est fait, ce qui est en cours, questions ouvertes.
- **DESIGN_DECISIONS_LOG.md** : à créer dès qu'une décision de design est arbitrée par le coordinateur. Évite de relitiger les mêmes points.

## Règle d'or

Si quelque chose n'est pas dans les docs et touche au game design, à l'UX ou à l'identité visuelle: **demander au coordinateur, pas trancher seul**. Pour la tech pure (refactor interne, choix de pattern technique parmi des équivalents), trancher et logger.

## Glossaire rapide

- **Voie** : style/culture de combat (Samurai, Viking, etc.)
- **Stade** : tier visuel du personnage (1 à 6, Mendiant à Mythe)
- **Force / Technique / Renom / Échos** : les 4 currencies
- **Esprit** : compagnon spirituel équipable (3 slots)
- **Disciple** : suiveur auto-générateur de Force
- **Prestige** : reset volontaire pour gagner Échos et débloquer de la profondeur
