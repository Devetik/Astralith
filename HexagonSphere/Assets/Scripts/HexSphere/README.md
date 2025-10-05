# Système HexSphere - Sphère Hexagonale Géodésique

## Description

Le système HexSphere permet de créer des sphères géodésiques composées d'hexagones, similaires à l'asset HexaWorld Grid System Enhanced. Il est conçu pour Unity avec support URP (Universal Render Pipeline).

## Composants Principaux

### 1. HexCell.cs
- Représente une cellule hexagonale individuelle
- Contient la géométrie, les couleurs et les propriétés de chaque hexagone
- Gère les vertices, triangles et UVs

### 2. HexSphereGenerator.cs
- Générateur principal de la sphère hexagonale
- Utilise un icosaèdre comme base, puis le subdivise
- Génère le mesh combiné de tous les hexagones
- Support pour l'élévation et les gradients de couleur

### 3. HexSphereManager.cs
- Gestionnaire principal du système
- Contrôle les interactions (sélection, survol)
- Gère la rotation automatique et les contrôles
- Interface utilisateur pour les paramètres

### 4. HexSphereMaterial.cs
- Gestionnaire de matériaux pour la sphère
- Support pour différents états visuels (normal, sélectionné, survol)
- Création de matériaux dynamiques

### 5. HexSphereDemo.cs
- Script de démonstration avec exemples d'utilisation
- Effets visuels (pulsation, cycle de couleurs)
- Démonstrations automatiques

### 6. HexSphereSetup.cs
- Utilitaire pour configurer rapidement une sphère
- Paramètres prédéfinis pour différents cas d'usage

## Installation et Utilisation

### Installation Rapide

1. **Créer un GameObject vide** dans votre scène
2. **Ajouter le script HexSphereSetup**
3. **Cliquer sur "Créer Sphère Hexagonale"** dans l'inspecteur
4. **Ajuster les paramètres** selon vos besoins

### Installation Manuelle

1. **Créer un GameObject** pour votre sphère
2. **Ajouter HexSphereGenerator** comme composant
3. **Configurer les paramètres** :
   - Niveau de subdivision (1-5)
   - Rayon de la sphère
   - Taille des hexagones
4. **Ajouter HexSphereManager** pour les interactions
5. **Générer la sphère** avec le bouton "Générer Sphère Hexagonale"

## Paramètres Principaux

### HexSphereGenerator
- **Subdivision Level** : Niveau de détail (1 = simple, 5 = très détaillé)
- **Radius** : Rayon de la sphère
- **Hex Size** : Taille des hexagones individuels
- **Use Elevation** : Active l'élévation aléatoire
- **Use Gradient** : Active les gradients de couleur

### HexSphereManager
- **Auto Rotate** : Rotation automatique
- **Rotation Speed** : Vitesse de rotation
- **Show Debug Info** : Affiche les informations de debug
- **Enable Hover Effect** : Effet de survol

## Contrôles

### Souris
- **Clic + Glisser** : Rotation manuelle
- **Molette** : Zoom
- **Clic** : Sélection d'hexagone

### Clavier
- **R** : Régénérer la sphère
- **T** : Toggle rotation automatique
- **G** : Toggle gizmos
- **1-5** : Démonstrations (avec HexSphereDemo)
- **SPACE** : Toggle démonstration automatique

## Exemples d'Utilisation

### Créer une Planète Simple
```csharp
// Utiliser HexSphereSetup
HexSphereSetup setup = gameObject.AddComponent<HexSphereSetup>();
setup.subdivisionLevel = 2;
setup.radius = 2f;
setup.CreateHexSphere();
```

### Créer une Sphère Interactive
```csharp
// Configuration manuelle
HexSphereGenerator generator = gameObject.AddComponent<HexSphereGenerator>();
generator.subdivisionLevel = 3;
generator.radius = 1f;
generator.GenerateHexSphere();

HexSphereManager manager = gameObject.AddComponent<HexSphereManager>();
manager.sphereGenerator = generator;
```

### Modifier les Couleurs Dynamiquement
```csharp
// Changer la couleur d'une cellule
HexCell cell = sphereManager.GetNearestHexCell(worldPosition);
cell.color = Color.red;

// Régénérer le mesh
generator.GenerateMesh();
```

## Optimisations

### Performance
- **Subdivision Level 1-2** : Idéal pour les jeux mobiles
- **Subdivision Level 3-4** : Bon équilibre qualité/performance
- **Subdivision Level 5** : Maximum de détail (PC uniquement)

### Mémoire
- Chaque niveau de subdivision multiplie le nombre d'hexagones par ~4
- Niveau 1 : ~20 hexagones
- Niveau 2 : ~80 hexagones
- Niveau 3 : ~320 hexagones
- Niveau 4 : ~1280 hexagones
- Niveau 5 : ~5120 hexagones

## Personnalisation

### Matériaux
- Support URP et Built-in Render Pipeline
- Textures personnalisées
- Couleurs dynamiques
- Effets d'émission

### Géométrie
- Élévation personnalisée
- Distorsion des hexagones
- Taille variable par cellule

### Interactions
- Sélection de cellules
- Survol avec feedback visuel
- Rayon de sélection
- Détection de voisins

## Dépannage

### Problèmes Courants

1. **Sphère ne s'affiche pas**
   - Vérifier que le matériau est assigné
   - Vérifier que le MeshRenderer est présent

2. **Performance faible**
   - Réduire le niveau de subdivision
   - Désactiver les gizmos
   - Optimiser le matériau

3. **Interactions ne fonctionnent pas**
   - Vérifier que HexSphereManager est présent
   - Vérifier que le collider est ajouté
   - Vérifier les layers

### Debug
- Activer "Show Debug Info" dans HexSphereManager
- Utiliser les gizmos pour visualiser la structure
- Vérifier les logs de la console

## Support

Ce système est conçu pour Unity 2022.3+ avec URP. Pour des questions ou des améliorations, consultez la documentation Unity ou les forums communautaires.

## Licence

Ce système est fourni tel quel pour usage éducatif et de développement. Adaptez-le selon vos besoins spécifiques.
