using UnityEngine;

public class AnimationColliderUpdaterAUB : AnimationColliderUpdater
{
    protected override void SetColliderSize()
    {
        var bottomWidth = GetBottomLineWidthInUnityUnits();

        if (bottomWidth == 0f)
        {
            base.SetColliderSize();
            return;
        }
        
        objectCollider.size = new Vector2(bottomWidth + colliderSizeAddonX, objectRenderer.bounds.size.y + colliderSizeAddonY);
    }

    protected override void SetColliderOffset()
    {
        var bottomLineOffset = GetBottomLineOffset();

        if (bottomLineOffset == 0f)
        {
            base.SetColliderOffset();
            return;
        }

        // Calculate the offset
        var offset = objectRenderer.bounds.center - transform.position;
        offset.x += bottomLineOffset + colliderOffsetAddon;
        offset.x *= transform.localScale.x;
        offset.y *= transform.localScale.y;

        objectCollider.offset = offset;
    }
    
    private float GetBottomLineOffset()
    {
        var sprite = objectRenderer.sprite;

        if (sprite == null || sprite.texture == null)
        {
            Debug.LogError("Sprite or texture is null.");
            return 0f;
        }

        Texture2D texture = sprite.texture;

        // Check if the texture is readable
        if (!texture.isReadable)
        {
            Debug.LogError($"Texture '{texture.name}' is not readable. Enable 'Read/Write Enabled' in the texture import settings.");
            return 0f;
        }

        Rect spriteRect = sprite.textureRect;

        // Get the bottom row of pixels
        int y = Mathf.FloorToInt(spriteRect.yMin);
        int xStart = Mathf.FloorToInt(spriteRect.xMin);
        int xEnd = Mathf.FloorToInt(spriteRect.xMax);

        Color[] bottomRowPixels = texture.GetPixels(xStart, y, xEnd - xStart, 1);

        // Calculate the center of the visible pixels
        float visiblePixelSum = 0f;
        int visiblePixelCount = 0;

        for (int i = 0; i < bottomRowPixels.Length; i++)
        {
            if (bottomRowPixels[i].a > 0) // Check if the pixel is not transparent
            {
                visiblePixelSum += i;
                visiblePixelCount++;
            }
        }

        if (visiblePixelCount == 0)
        {
            return 0f; // No visible pixels
        }

        float visiblePixelCenter = visiblePixelSum / visiblePixelCount;

        // Convert the pixel center to Unity units
        float pixelsPerUnit = sprite.pixelsPerUnit;
        float textureToWorldRatio = spriteRect.width / pixelsPerUnit;
        float centerOffsetInUnityUnits = ((visiblePixelCenter + xStart) - (spriteRect.xMin + spriteRect.width / 2)) / pixelsPerUnit;

        return centerOffsetInUnityUnits;
    }

    private float GetBottomLineWidthInUnityUnits()
    {
        var sprite = objectRenderer.sprite;

        if (sprite == null || sprite.texture == null)
        {
            Debug.LogError("Sprite or texture is null.");
            return 0f;
        }

        Texture2D texture = sprite.texture;

        // Check if the texture is readable
        if (!texture.isReadable)
        {
            Debug.LogError($"Texture '{texture.name}' is not readable. Enable 'Read/Write Enabled' in the texture import settings.");
            return 0f;
        }

        Rect spriteRect = sprite.textureRect;

        // Get the bottom row of pixels
        int y = Mathf.FloorToInt(spriteRect.yMin);
        int xStart = Mathf.FloorToInt(spriteRect.xMin);
        int xEnd = Mathf.FloorToInt(spriteRect.xMax);

        Color[] bottomRowPixels = texture.GetPixels(xStart, y, xEnd - xStart, 1);

        // Count non-transparent pixels
        int visiblePixelCount = 0;
        foreach (Color pixel in bottomRowPixels)
        {
            if (pixel.a > 0) // Check if the pixel is not transparent
            {
                visiblePixelCount++;
            }
        }

        // Calculate the length in Unity units
        float pixelsPerUnit = sprite.pixelsPerUnit;
        float textureToWorldRatio = spriteRect.width / (spriteRect.width / pixelsPerUnit);
        float lengthInUnityUnits = visiblePixelCount / textureToWorldRatio;

        return lengthInUnityUnits;
    }
}
