#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PetPrefabAutoSetup
{
    private const string SourceRoot = "Assets/Layer Lab/2D Characters-PetPack2/Sprites/ImageSequence";
    private const string OutputRoot = "Assets/04.Prefabs/02.Pet";
    private static readonly string[] Motions = { "Idle", "Attack", "Busking", "Walk" };

    [MenuItem("Tools/Pet/선택한 펫 폴더 생성")]
    private static void CreateSelected()
    {
        // 펫 폴더 또는 그 안의 동작 폴더를 선택할 수 있습니다.
        var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var selected in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(selected);
            if (path == SourceRoot)
            {
                foreach (string folder in AssetDatabase.GetSubFolders(SourceRoot)) folders.Add(folder);
            }
            else if (path.StartsWith(SourceRoot + "/", StringComparison.Ordinal))
            {
                string petName = path.Substring(SourceRoot.Length + 1).Split('/')[0];
                folders.Add(SourceRoot + "/" + petName);
            }
        }
        Generate(folders.OrderBy(p => p, StringComparer.Ordinal).ToArray());
    }

    [MenuItem("Tools/Pet/전체 펫 생성")]
    private static void CreateAll()
    {
        if (!AssetDatabase.IsValidFolder(SourceRoot))
        {
            Debug.LogError($"펫 원본 폴더가 없습니다: {SourceRoot}");
            return;
        }
        Generate(AssetDatabase.GetSubFolders(SourceRoot));
    }

    private static void Generate(string[] folders)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("플레이 모드를 종료한 후 실행하세요.");
            return;
        }
        if (folders.Length == 0)
        {
            Debug.LogWarning("Project 창에서 ImageSequence 아래의 펫 폴더를 선택하세요.");
            return;
        }
        int created = 0, skipped = 0, failed = 0;
        try
        {
            foreach (string folder in folders)
            {
                string petName = Path.GetFileName(folder);
                string destination = OutputRoot + "/" + petName;
                // 기존 작업이나 재실행 시 수동 수정한 에셋을 보호합니다.
                if (AssetDatabase.IsValidFolder(destination)) { skipped++; continue; }
                if (EditorUtility.DisplayCancelableProgressBar("펫 생성", petName,
                    (float)(created + skipped + failed) / folders.Length)) break;
                try
                {
                    // 모든 동작을 먼저 검증한 후 에셋을 생성합니다.
                    var frames = Motions.ToDictionary(m => m, m => LoadFrames(folder + "/" + m));
                    EnsureFolder(destination);
                    var clips = new Dictionary<string, AnimationClip>();
                    foreach (string motion in Motions)
                    {
                        float fps = motion == "Attack" ? 24f : 12f;
                        clips[motion] = CreateClip(destination, motion, frames[motion], fps);
                    }
                    var controller = AnimatorController.CreateAnimatorControllerAtPath(destination + "/" + petName + ".controller");
                    var machine = controller.layers[0].stateMachine;
                    for (int i = 0; i < Motions.Length; i++)
                    {
                        string motion = Motions[i];
                        var state = machine.AddState(motion, new Vector3(260, i * 70));
                        state.motion = clips[motion];
                        state.writeDefaultValues = false;
                        if (motion == "Idle") machine.defaultState = state;
                    }

                    // 공격은 한 번 재생한 후 Idle로 복귀합니다.
                    var attack = machine.states.First(s => s.state.name == "Attack").state;
                    var transition = attack.AddTransition(machine.defaultState);
                    transition.hasExitTime = true;
                    transition.exitTime = 1f;
                    transition.duration = 0f;

                    var root = new GameObject(petName);
                    try
                    {
                        root.AddComponent<SpriteRenderer>().sprite = frames["Idle"][0];
                        var animator = root.AddComponent<Animator>();
                        animator.runtimeAnimatorController = controller;
                        animator.applyRootMotion = false;
                        PrefabUtility.SaveAsPrefabAsset(root, destination + "/" + petName + ".prefab", out bool saved);
                        if (!saved) throw new InvalidOperationException("프리팹 저장 실패");
                    }
                    finally { UnityEngine.Object.DestroyImmediate(root); }
                    created++;
                }
                catch (Exception e)
                {
                    failed++;
                    Debug.LogError($"펫 생성 실패: {folder}\n{e.Message}\n부분 생성 폴더가 있다면 확인 후 제거하고 다시 실행하세요.");
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }
        Debug.Log($"펫 생성 완료: 생성 {created}, 기존 폴더 건너뜀 {skipped}, 실패 {failed}\n저장 위치: {OutputRoot}");
    }

    private static Sprite[] LoadFrames(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder)) throw new InvalidOperationException($"동작 폴더 없음: {folder}");
        var paths = AssetDatabase.FindAssets("t:Texture2D", new[] { folder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => FrameNumber(Path.GetFileNameWithoutExtension(p))).ThenBy(p => p, StringComparer.Ordinal).ToArray();
        if (paths.Length == 0) throw new InvalidOperationException($"프레임 없음: {folder}");
        var frames = new List<Sprite>();
        foreach (string path in paths)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            // 원본 Import 설정은 바꾸지 않습니다. 이미지당 스프라이트 1개를 사용합니다.
            if (sprites.Length != 1) throw new InvalidOperationException($"Sprite (Single) 설정 필요: {path}");
            frames.Add(sprites[0]);
        }
        return frames.ToArray();
    }

    private static int FrameNumber(string name)
    {
        int start = name.Length;
        while (start > 0 && char.IsDigit(name[start - 1])) start--;
        if (start == name.Length || !int.TryParse(name.Substring(start), out int number))
            throw new InvalidOperationException($"프레임 번호를 읽을 수 없습니다: {name}");
        return number;
    }

    private static AnimationClip CreateClip(string folder, string motion, Sprite[] frames, float fps)
    {
        var clip = new AnimationClip { name = motion, frameRate = fps };
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        // 마지막 프레임도 정확히 1프레임 동안 표시합니다.
        keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = motion != "Attack";
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, folder + "/" + motion + ".anim");
        return clip;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = path.Substring(0, path.LastIndexOf('/'));
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
#endif
