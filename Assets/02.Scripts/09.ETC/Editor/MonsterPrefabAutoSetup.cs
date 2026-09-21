using System;
using UnityEditor;
using UnityEngine;

public static class MonsterPrefabAutoSetup
{
    // 몬스터의 가장 높은 부분과 HP바 사이의 간격
    // 프리팹 루트의 로컬 좌표 기준
    private const float hpBarGap = 0.25f;

    // Collider 외곽에 추가할 여유 공간
    private const float colliderPadding = 0.02f;

    // 크기 계산에서 제외할 오브젝트 이름
    // 해당 이름으로 시작하는 자식도 제외: Hit_2, Attack_bone 등
    private static readonly string[] excludedNames =
    {
        "Shadow",
        "Hit",
        "Dead",
        "Death",
        "Attack",
        "Effect",
        "VFX"
    };

    [MenuItem("Tools/Monster/선택한 프리팹 자동 설정")]
    private static void SetupSelectedPrefabs()
    {
        // 플레이 중에는 프리팹을 수정하지 않음
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("플레이를 종료한 후 실행해주세요.");
            return;
        }

        int successCount = 0;

        foreach (UnityEngine.Object selected in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selected);

            // Project 창에서 선택한 .prefab 파일만 처리
            if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                continue;

            GameObject root = null;

            try
            {
                // 프리팹 내용을 별도의 편집 공간으로 불러옴
                root = PrefabUtility.LoadPrefabContents(assetPath);

                MonsterController controller = root.GetComponent<MonsterController>();

                if (controller == null)
                {
                    Debug.LogWarning($"MonsterController가 없어 건너뜀: {assetPath}");
                    continue;
                }

                // private 필드인 pathAnchor도 에디터에서 연결하기 위해 사용
                SerializedObject serializedController = new SerializedObject(controller);
                SerializedProperty hpProperty = serializedController.FindProperty("hpBarPose");
                SerializedProperty anchorProperty = serializedController.FindProperty("pathAnchor");

                if (hpProperty == null || anchorProperty == null)
                {
                    Debug.LogWarning($"hpBarPose 또는 pathAnchor 필드를 찾을 수 없음: {assetPath}");
                    continue;
                }

                // 전체 이미지 범위와 몸통 이미지 범위를 계산
                if (!TryGetMonsterBounds(root.transform, out Bounds totalBounds, out Bounds bodyBounds))
                {
                    Debug.LogWarning($"계산할 SpriteRenderer가 없음: {assetPath}");
                    continue;
                }

                BoxCollider2D boxCollider = root.GetComponent<BoxCollider2D>();

                // 다른 종류의 Collider가 있으면 중복 생성하지 않음
                if (boxCollider == null && root.GetComponent<Collider2D>() != null)
                {
                    Debug.LogWarning($"다른 종류의 Collider2D가 있어 건너뜀: {assetPath}");
                    continue;
                }

                if (boxCollider == null)
                    boxCollider = root.AddComponent<BoxCollider2D>();

                // 전체 이미지 외곽에 맞춰 Collider 중심과 크기 설정
                boxCollider.offset = new Vector2(totalBounds.center.x, totalBounds.center.y);
                boxCollider.size = new Vector2(totalBounds.size.x + colliderPadding * 2f, totalBounds.size.y + colliderPadding * 2f) * 0.9f;

                // 이미 존재하는 기준점은 재사용하고, 없으면 생성
                Transform hpBarPos = GetOrCreatePoint(root.transform, "hpBarPos");
                Transform pathAnchor = GetOrCreatePoint(root.transform, "PathAnchor");

                // 몸통 중앙을 기준으로 이동하도록 설정
                pathAnchor.localPosition = new Vector3(bodyBounds.center.x, bodyBounds.center.y, 0f);

                // 몸통 중심의 X좌표를 사용하고, 전체 이미지보다 위에 HP바 배치
                hpBarPos.localPosition = new Vector3(bodyBounds.center.x, totalBounds.max.y + hpBarGap, 0f);

                // MonsterController의 참조 연결
                hpProperty.objectReferenceValue = hpBarPos;
                anchorProperty.objectReferenceValue = pathAnchor;
                serializedController.ApplyModifiedPropertiesWithoutUndo();

                // 변경한 내용을 원본 프리팹에 저장
                PrefabUtility.SaveAsPrefabAsset(root, assetPath, out bool saved);

                if (saved)
                {
                    successCount++;
                    Debug.Log($"몬스터 자동 설정 완료: {assetPath}", selected);
                }
                else
                {
                    Debug.LogError($"프리팹 저장 실패: {assetPath}");
                }
            }
            catch (Exception exception)
            {
                // 한 프리팹에서 오류가 나더라도 다음 프리팹은 계속 처리
                Debug.LogError($"몬스터 설정 실패: {assetPath}\n{exception}");
            }
            finally
            {
                // 임시로 불러온 프리팹 편집 공간 정리
                if (root != null)
                    PrefabUtility.UnloadPrefabContents(root);
            }
        }

        Debug.Log($"몬스터 자동 설정: 총 {successCount}개 저장 완료");
    }

    // 활성화된 기본 외형만 모아서 전체 범위와 Body 범위를 계산
    private static bool TryGetMonsterBounds(Transform root, out Bounds totalBounds, out Bounds bodyBounds)
    {
        totalBounds = default;
        bodyBounds = default;

        bool hasTotalBounds = false;
        bool hasBodyBounds = false;

        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            if (!spriteRenderer.enabled || spriteRenderer.sprite == null)
                continue;

            if (ShouldExclude(spriteRenderer.transform, root))
                continue;

            Bounds bounds = GetSpriteBoundsInRoot(spriteRenderer, root);

            if (!hasTotalBounds)
            {
                totalBounds = bounds;
                hasTotalBounds = true;
            }
            else
            {
                totalBounds.Encapsulate(bounds);
            }

            // 현재 몬스터 프리팹의 몸통 이미지 이름은 Body
            if (spriteRenderer.name.Equals("Body", StringComparison.OrdinalIgnoreCase))
            {
                if (!hasBodyBounds)
                {
                    bodyBounds = bounds;
                    hasBodyBounds = true;
                }
                else
                {
                    bodyBounds.Encapsulate(bounds);
                }
            }
        }

        // Body를 찾지 못한 몬스터는 전체 외형 중심을 사용
        if (!hasBodyBounds)
            bodyBounds = totalBounds;

        return hasTotalBounds;
    }

    // 자신과 부모 중 비활성 오브젝트 또는 제외 대상이 있는지 확인
    private static bool ShouldExclude(Transform current, Transform root)
    {
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
                return true;

            // 프리팹 루트 이름은 제외 조건에 사용하지 않음
            if (current == root)
                break;

            foreach (string excludedName in excludedNames)
            {
                if (current.name.StartsWith(excludedName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            current = current.parent;
        }

        return false;
    }

    // Sprite의 범위를 몬스터 루트의 로컬 좌표로 변환
    // 자식의 위치, 회전, 스케일과 Sprite Flip을 반영
    private static Bounds GetSpriteBoundsInRoot(SpriteRenderer spriteRenderer, Transform root)
    {
        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        Matrix4x4 matrix = root.worldToLocalMatrix * spriteRenderer.transform.localToWorldMatrix;

        Bounds result = default;
        bool initialized = false;

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector3 point = new Vector3(x == 0 ? spriteBounds.min.x : spriteBounds.max.x, y == 0 ? spriteBounds.min.y : spriteBounds.max.y, 0f);

                if (spriteRenderer.flipX)
                    point.x = -point.x;

                if (spriteRenderer.flipY)
                    point.y = -point.y;

                point = matrix.MultiplyPoint3x4(point);

                if (!initialized)
                {
                    result = new Bounds(point, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    result.Encapsulate(point);
                }
            }
        }

        return result;
    }

    // 루트 바로 아래에 기준점을 생성하거나 기존 기준점을 반환
    private static Transform GetOrCreatePoint(Transform root, string pointName)
    {
        Transform point = root.Find(pointName);

        if (point == null)
        {
            GameObject pointObject = new GameObject(pointName);
            pointObject.layer = root.gameObject.layer;

            point = pointObject.transform;
            point.SetParent(root, false);
        }

        return point;
    }
}
