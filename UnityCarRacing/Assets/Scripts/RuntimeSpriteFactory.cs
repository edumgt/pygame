using UnityEngine;

public static class RuntimeSpriteFactory
{
    private static Sprite cachedSquare;

    public static Sprite GetSquareSprite()
    {
        if (cachedSquare != null)
        {
            return cachedSquare;
        }

        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        cachedSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedSquare;
    }
}
