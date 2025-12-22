using UnityEditor;
using UnityEngine;

namespace NexonGame.Editor
{
    /// <summary>
    /// 프로젝트 설정을 위한 커스텀 에디터 윈도우
    /// </summary>
    public class ProjectSetupWindow : EditorWindow
    {
        [MenuItem("NexonGame/프로젝트 설정")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectSetupWindow>("프로젝트 설정");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("프로젝트 설정", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "이 윈도우는 프로젝트 설정을 빠르게 구성하는 도구입니다.\n" +
                "DI 패턴을 사용하여 서비스를 관리합니다.",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("GameBootstrapper 씬에 추가", GUILayout.Height(30)))
            {
                AddGameBootstrapperToScene();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("입력 시스템 C# 클래스 재생성", GUILayout.Height(30)))
            {
                RegenerateInputSystemClass();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("프로젝트 폴더 열기", GUILayout.Height(30)))
            {
                EditorUtility.RevealInFinder("Assets/_Project");
            }
        }

        private void AddGameBootstrapperToScene()
        {
            var existingBootstrapper = FindObjectOfType<Core.GameBootstrapper>();

            if (existingBootstrapper != null)
            {
                EditorUtility.DisplayDialog(
                    "이미 존재함",
                    "씬에 이미 GameBootstrapper가 존재합니다.",
                    "확인"
                );
                return;
            }

            var go = new GameObject("GameBootstrapper");
            go.AddComponent<Core.GameBootstrapper>();
            Selection.activeGameObject = go;

            EditorUtility.DisplayDialog(
                "성공",
                "GameBootstrapper가 씬에 추가되었습니다.",
                "확인"
            );
        }

        private void RegenerateInputSystemClass()
        {
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "완료",
                "입력 시스템 C# 클래스를 재생성하려면\n" +
                "InputSystem_Actions.inputactions 파일을 선택하고\n" +
                "Inspector에서 'Generate C# Class' 버튼을 클릭하세요.",
                "확인"
            );
        }
    }
}
