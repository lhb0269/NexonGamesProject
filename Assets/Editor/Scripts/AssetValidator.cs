using UnityEditor;
using UnityEngine;
using System.Linq;

namespace NexonGame.Editor
{
    /// <summary>
    /// 에셋 검증 도구
    /// </summary>
    public static class AssetValidator
    {
        [MenuItem("NexonGame/도구/에셋 검증")]
        public static void ValidateAssets()
        {
            int issueCount = 0;

            // 텍스처 검증
            var textures = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art/Textures" });
            foreach (var guid in textures)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null && importer.maxTextureSize > 2048)
                {
                    Debug.LogWarning($"큰 텍스처 발견: {path} (Max Size: {importer.maxTextureSize})");
                    issueCount++;
                }
            }

            // AudioClip 검증
            var audioClips = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Project/Audio" });
            foreach (var guid in audioClips)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;

                if (importer != null && !importer.loadInBackground)
                {
                    Debug.LogWarning($"백그라운드 로딩이 비활성화된 오디오: {path}");
                    issueCount++;
                }
            }

            if (issueCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "검증 완료",
                    "모든 에셋이 정상입니다.",
                    "확인"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "검증 완료",
                    $"{issueCount}개의 잠재적 문제가 발견되었습니다.\n콘솔을 확인하세요.",
                    "확인"
                );
            }
        }

        [MenuItem("NexonGame/도구/누락된 스크립트 찾기")]
        public static void FindMissingScripts()
        {
            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            int missingCount = 0;

            foreach (var go in allGameObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        Debug.LogError($"누락된 스크립트 발견: {go.name}", go);
                        missingCount++;
                    }
                }
            }

            EditorUtility.DisplayDialog(
                "검색 완료",
                missingCount == 0
                    ? "누락된 스크립트가 없습니다."
                    : $"{missingCount}개의 누락된 스크립트를 발견했습니다.",
                "확인"
            );
        }
    }
}
