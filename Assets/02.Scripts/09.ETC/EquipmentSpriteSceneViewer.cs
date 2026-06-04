using System.Collections.Generic;
using UnityEngine;

public class EquipmentSpriteSceneViewer : MonoBehaviour
{
    [Header("장비 Sprite 목록")]
    public List<Sprite> helmetSprites = new List<Sprite>();
    public List<Sprite> armorSprites = new List<Sprite>();
    public List<Sprite> wandSprites = new List<Sprite>();
    public List<Sprite> staffSprites = new List<Sprite>();
    public List<Sprite> swordSprites = new List<Sprite>();
    public List<Sprite> bluntSprites = new List<Sprite>();
    public List<Sprite> shieldSprites = new List<Sprite>();
    public List<Sprite> subItemSprites = new List<Sprite>();

    [Header("정렬 설정")]
    public float cellWidth = 1.5f;
    public float cellHeight = 1.5f;
    public float categoryGap = 2.0f;
    public int maxColumn = 10;

    [Header("생성 설정")]
    public float spriteScale = 1.0f;
    public bool clearBeforeGenerate = true;

    private const string RootName = "[Generated Equipment Sprites]";

    [ContextMenu("Generate Sprites In Scene")]
    public void GenerateSpritesInScene()
    {
        if (clearBeforeGenerate)
        {
            ClearGeneratedObjects();
        }

        GameObject root = new GameObject(RootName);
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;

        float currentY = 0f;

        CreateCategory("투구", helmetSprites, root.transform, ref currentY);
        CreateCategory("갑옷", armorSprites, root.transform, ref currentY);
        CreateCategory("완드", wandSprites, root.transform, ref currentY);
        CreateCategory("스태프", staffSprites, root.transform, ref currentY);
        CreateCategory("쉴드", shieldSprites, root.transform, ref currentY);
        CreateCategory("서브아이템", subItemSprites, root.transform, ref currentY);
    }

    [ContextMenu("Clear Generated Objects")]
    public void ClearGeneratedObjects()
    {
        Transform oldRoot = transform.Find(RootName);

        if (oldRoot == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(oldRoot.gameObject);
        }
        else
        {
            Destroy(oldRoot.gameObject);
        }
#else
        Destroy(oldRoot.gameObject);
#endif
    }

    private void CreateCategory(
        string categoryName,
        List<Sprite> sprites,
        Transform root,
        ref float currentY
    )
    {
        if (sprites == null || sprites.Count == 0)
            return;

        GameObject categoryRoot = new GameObject(categoryName);
        categoryRoot.transform.SetParent(root);
        categoryRoot.transform.localPosition = new Vector3(0f, currentY, 0f);

        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite sprite = sprites[i];

            if (sprite == null)
                continue;

            int column = i % maxColumn;
            int row = i / maxColumn;

            GameObject obj = new GameObject(sprite.name);
            obj.transform.SetParent(categoryRoot.transform);

            obj.transform.localPosition = new Vector3(
                column * cellWidth,
                -row * cellHeight,
                0f
            );

            obj.transform.localScale = Vector3.one * spriteScale;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0;
        }

        int rowCount = Mathf.CeilToInt((float)sprites.Count / maxColumn);
        currentY -= rowCount * cellHeight + categoryGap;
    }
}