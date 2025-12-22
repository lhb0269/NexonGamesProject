using UnityEditor;
using UnityEditor.SceneManagement;

namespace NexonGame.Editor
{
    /// <summary>
    /// 씬 빠른 전환 단축키
    /// </summary>
    public static class SceneQuickStart
    {
        [MenuItem("NexonGame/씬/샘플 씬 열기 &1")]
        public static void OpenSampleScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            }
        }

        [MenuItem("NexonGame/씬/개발 씬 폴더 열기")]
        public static void OpenDevelopmentSceneFolder()
        {
            EditorUtility.RevealInFinder("Assets/_Project/Scenes/Development");
        }

        [MenuItem("NexonGame/씬/프로덕션 씬 폴더 열기")]
        public static void OpenProductionSceneFolder()
        {
            EditorUtility.RevealInFinder("Assets/_Project/Scenes/Production");
        }
    }
}
