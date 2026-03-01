using System.Collections.Generic;
using UnityEngine;

public static class RuntimeMaterialFactory
{
    private static Shader cachedShader;
    private static readonly Dictionary<int, Texture2D> cachedTextures = new Dictionary<int, Texture2D>();

    public enum MaterialPreset
    {
        Default,
        Grass,
        Dirt,
        Rock,
        Bark,
        Leaf,
        Metal,
        Boundary
    }

    public static Material Create(Color color)
    {
        return Create(color, MaterialPreset.Default);
    }

    public static Material Create(Color color, MaterialPreset preset)
    {
        Shader shader = ResolveShader();
        Material mat = new Material(shader);
        ApplyColor(mat, color);

        Texture2D tex = GetOrBuildTexture(color, preset);
        ApplyTexture(mat, tex, preset);
        ApplySurface(mat, preset);

        return mat;
    }

    private static Shader ResolveShader()
    {
        if (cachedShader != null)
        {
            return cachedShader;
        }

        string[] candidates =
        {
            "Universal Render Pipeline/Lit",
            "Standard",
            "HDRP/Lit",
            "Legacy Shaders/Diffuse"
        };

        foreach (string name in candidates)
        {
            Shader shader = Shader.Find(name);
            if (shader != null)
            {
                cachedShader = shader;
                return shader;
            }
        }

        cachedShader = Shader.Find("Unlit/Color");
        return cachedShader != null ? cachedShader : Shader.Find("Sprites/Default");
    }

    private static void ApplyColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
    }

    private static void ApplyTexture(Material mat, Texture2D tex, MaterialPreset preset)
    {
        if (tex == null)
        {
            return;
        }

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", tex);
        }
        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture("_MainTex", tex);
        }

        Vector2 tiling = GetTiling(preset);
        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTextureScale("_BaseMap", tiling);
        }
        if (mat.HasProperty("_MainTex"))
        {
            mat.SetTextureScale("_MainTex", tiling);
        }
    }

    private static void ApplySurface(Material mat, MaterialPreset preset)
    {
        float smoothness;
        float metallic;
        switch (preset)
        {
            case MaterialPreset.Grass:
                smoothness = 0.03f;
                metallic = 0f;
                break;
            case MaterialPreset.Dirt:
                smoothness = 0.06f;
                metallic = 0f;
                break;
            case MaterialPreset.Rock:
                smoothness = 0.11f;
                metallic = 0f;
                break;
            case MaterialPreset.Bark:
                smoothness = 0.05f;
                metallic = 0f;
                break;
            case MaterialPreset.Leaf:
                smoothness = 0.1f;
                metallic = 0f;
                break;
            case MaterialPreset.Metal:
                smoothness = 0.28f;
                metallic = 0.24f;
                break;
            case MaterialPreset.Boundary:
                smoothness = 0.14f;
                metallic = 0.05f;
                break;
            default:
                smoothness = 0.16f;
                metallic = 0.04f;
                break;
        }

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", smoothness);
        }
        if (mat.HasProperty("_Glossiness"))
        {
            mat.SetFloat("_Glossiness", smoothness);
        }
        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }
    }

    private static Vector2 GetTiling(MaterialPreset preset)
    {
        switch (preset)
        {
            case MaterialPreset.Grass:
                return new Vector2(10f, 10f);
            case MaterialPreset.Dirt:
                return new Vector2(7f, 7f);
            case MaterialPreset.Rock:
                return new Vector2(5f, 5f);
            case MaterialPreset.Bark:
                return new Vector2(2f, 4f);
            case MaterialPreset.Leaf:
                return new Vector2(4f, 4f);
            case MaterialPreset.Metal:
                return new Vector2(2f, 2f);
            case MaterialPreset.Boundary:
                return new Vector2(6f, 3f);
            default:
                return new Vector2(3f, 3f);
        }
    }

    private static Texture2D GetOrBuildTexture(Color color, MaterialPreset preset)
    {
        int key = ComputeKey(color, preset);
        if (cachedTextures.TryGetValue(key, out Texture2D cached))
        {
            return cached;
        }

        Texture2D tex = BuildTexture(color, preset);
        cachedTextures[key] = tex;
        return tex;
    }

    private static int ComputeKey(Color color, MaterialPreset preset)
    {
        Color32 c32 = color;
        int packed = c32.r | (c32.g << 8) | (c32.b << 16) | (c32.a << 24);
        return packed ^ ((int)preset << 5);
    }

    private static Texture2D BuildTexture(Color baseColor, MaterialPreset preset)
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = $"RuntimeTex_{preset}_{ColorUtility.ToHtmlStringRGB(baseColor)}";
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        Color dark = baseColor * 0.72f;
        dark.a = 1f;
        Color bright = Color.Lerp(baseColor, Color.white, 0.14f);
        bright.a = 1f;

        float grainScale = 4.2f;
        float macroScale = 1.6f;
        float stripeScale = preset == MaterialPreset.Bark ? 13f : 0f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1);
                float v = y / (float)(size - 1);

                float n1 = Mathf.PerlinNoise((u + 0.17f) * grainScale, (v + 0.29f) * grainScale);
                float n2 = Mathf.PerlinNoise((u + 0.53f) * macroScale, (v + 0.71f) * macroScale);
                float blend = Mathf.Lerp(n1, n2, 0.36f);

                if (preset == MaterialPreset.Rock || preset == MaterialPreset.Boundary)
                {
                    float crack = Mathf.PerlinNoise((u + 0.11f) * 9.7f, (v + 0.37f) * 9.7f);
                    blend = Mathf.Lerp(blend, crack, 0.26f);
                }

                if (stripeScale > 0f)
                {
                    float stripe = Mathf.Abs(Mathf.Sin((u * stripeScale + n2 * 2.4f) * Mathf.PI));
                    blend = Mathf.Lerp(blend, stripe, 0.34f);
                }

                if (preset == MaterialPreset.Grass || preset == MaterialPreset.Leaf)
                {
                    float blade = Mathf.PerlinNoise((u + 0.9f) * 12f, (v + 0.2f) * 2f);
                    blend = Mathf.Lerp(blend, blade, 0.22f);
                }

                Color pixel = Color.Lerp(dark, bright, Mathf.Clamp01(blend));
                tex.SetPixel(x, y, pixel);
            }
        }

        tex.Apply(false, false);
        return tex;
    }
}
