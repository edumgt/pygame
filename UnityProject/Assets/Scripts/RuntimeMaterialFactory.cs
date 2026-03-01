using UnityEngine;

public static class RuntimeMaterialFactory
{
    private static Shader cachedShader;

    public static Material Create(Color color)
    {
        Shader shader = ResolveShader();
        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
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
}
