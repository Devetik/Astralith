using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourGenerator
{

    ColourSettings settings;
    Texture2D texture;
    const int textureResolution = 50;
    INoiseFilter biomeNoiseFilter;

    public void UpdateSettings(ColourSettings settings)
    {
        this.settings = settings;
        if (texture == null || texture.height != settings.biomeColourSettings.biomes.Length)
        {
            texture = new Texture2D(textureResolution*2, settings.biomeColourSettings.biomes.Length, TextureFormat.RGBA32, false);
        }
        biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(settings.biomeColourSettings.noise);
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }

    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        //float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
        float heightPercent = Mathf.Abs(pointOnUnitSphere.y);
        heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - settings.biomeColourSettings.noiseOffset) * settings.biomeColourSettings.noiseStrength;
        float biomeIndex = 0;
        int numBiomes = settings.biomeColourSettings.biomes.Length;
        float blendRange = settings.biomeColourSettings.blendAmount / 2f + .001f;

        for (int i = 0; i < numBiomes; i++)
        {
            float dst = heightPercent - settings.biomeColourSettings.biomes[i].startHeight;
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }

        return biomeIndex / Mathf.Max(1, numBiomes - 1);
    }

    public void UpdateColours()
    {
        Color[] colours = new Color[texture.width * texture.height];
        int colourIndex = 0;
        
        // Générer d'abord les couleurs de base pour tous les biomes
        foreach (var biome in settings.biomeColourSettings.biomes)
        {
            for (int i = 0; i < textureResolution * 2; i++)
            {
                Color gradientCol;
                if (i < textureResolution) {
                    gradientCol = settings.oceanColour.Evaluate(i / (textureResolution - 1f));
                }
                else {
                    gradientCol = biome.gradient.Evaluate((i-textureResolution) / (textureResolution - 1f));
                }
                Color tintCol = biome.tint;
                colours[colourIndex] = gradientCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent;
                colourIndex++;
            }
        }
        
        // Appliquer les couches d'override par-dessus
        ApplyOverrideLayers(colours);
        
        texture.SetPixels(colours);
        texture.Apply();
        settings.planetMaterial.SetTexture("_texture", texture);
    }

    private void ApplyOverrideLayers(Color[] colours)
    {
        int colourIndex = 0;
        
        foreach (var biome in settings.biomeColourSettings.biomes)
        {
            // Si ce biome est une couche d'override, l'appliquer par-dessus
            if (biome.isOverrideLayer)
            {
                for (int i = 0; i < textureResolution * 2; i++)
                {
                    Color overrideCol;
                    if (i < textureResolution) {
                        // Pour les océans, utiliser le gradient du biome override
                        overrideCol = biome.gradient.Evaluate(i / (textureResolution - 1f));
                    }
                    else {
                        overrideCol = biome.gradient.Evaluate((i-textureResolution) / (textureResolution - 1f));
                    }
                    
                    // Mélanger avec la couleur existante selon l'intensité d'override
                    Color existingCol = colours[colourIndex];
                    Color tintCol = biome.tint;
                    float overrideStrength = biome.overrideOtherBiomes ? 1f : biome.tintPercent;
                    
                    colours[colourIndex] = Color.Lerp(existingCol, overrideCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent, overrideStrength);
                    colourIndex++;
                }
            }
            else
            {
                // Avancer l'index sans modifier les couleurs pour les biomes normaux
                colourIndex += textureResolution * 2;
            }
        }
    }
}
