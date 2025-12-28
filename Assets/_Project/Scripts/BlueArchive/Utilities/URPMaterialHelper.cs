using UnityEngine;

namespace NexonGame.BlueArchive.Utilities
{
    /// <summary>
    /// URP 머티리얼 헬퍼
    /// - 빌드에서 마젠타색 문제 방지
    /// - GameObject.CreatePrimitive()로 생성한 오브젝트에 URP 머티리얼 적용
    /// </summary>
    public static class URPMaterialHelper
    {
        private static Material _urpLitMaterial;
        private static Material _urpUnlitMaterial;

        /// <summary>
        /// URP/Lit 셰이더를 사용하는 기본 머티리얼 가져오기
        /// </summary>
        public static Material GetURPLitMaterial()
        {
            if (_urpLitMaterial == null)
            {
                // 1. Resources 폴더에서 미리 만든 URP 머티리얼 로드 시도
                Material resourceMaterial = Resources.Load<Material>("URPDefaultMaterial");
                if (resourceMaterial != null)
                {
                    Debug.Log("[URPMaterialHelper] Resources에서 URP 머티리얼 로드 성공");
                    _urpLitMaterial = new Material(resourceMaterial);
                    _urpLitMaterial.name = "URP_Lit_Instance";
                    return _urpLitMaterial;
                }

                // 2. 런타임에서 셰이더 찾기 시도
                Debug.LogWarning("[URPMaterialHelper] Resources에서 머티리얼을 찾을 수 없습니다. 런타임 생성 시도");

                // 여러 가능한 URP 셰이더 이름 시도
                string[] possibleShaderNames = new string[]
                {
                    "Universal Render Pipeline/Lit",
                    "Shader Graphs/URP_Lit",
                    "URP/Lit",
                    "Universal Render Pipeline/Simple Lit",
                    "URP/Simple Lit"
                };

                Shader urpShader = null;
                foreach (var shaderName in possibleShaderNames)
                {
                    urpShader = Shader.Find(shaderName);
                    if (urpShader != null)
                    {
                        Debug.Log($"[URPMaterialHelper] 셰이더 찾음: {shaderName}");
                        break;
                    }
                }

                // Lit를 못 찾으면 Unlit 시도
                if (urpShader == null)
                {
                    Debug.LogWarning("[URPMaterialHelper] URP Lit 셰이더를 찾을 수 없습니다. Unlit으로 대체");
                    urpShader = GetURPUnlitShader();
                }

                if (urpShader == null)
                {
                    Debug.LogError("[URPMaterialHelper] 어떤 URP 셰이더도 찾을 수 없습니다! Built-in으로 대체");
                    // 최후의 수단: Built-in Unlit/Color
                    urpShader = Shader.Find("Unlit/Color");
                }

                if (urpShader == null)
                {
                    Debug.LogError("[URPMaterialHelper] 셰이더를 전혀 찾을 수 없습니다!");
                    return null;
                }

                _urpLitMaterial = new Material(urpShader);
                _urpLitMaterial.name = "URP_Lit_Default";
                _urpLitMaterial.enableInstancing = true; // GPU Instancing 활성화
            }

            return _urpLitMaterial;
        }

        /// <summary>
        /// URP Unlit 셰이더 찾기
        /// </summary>
        private static Shader GetURPUnlitShader()
        {
            string[] possibleUnlitShaders = new string[]
            {
                "Universal Render Pipeline/Unlit",
                "URP/Unlit",
                "Unlit/Color"
            };

            foreach (var shaderName in possibleUnlitShaders)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    Debug.Log($"[URPMaterialHelper] Unlit 셰이더 찾음: {shaderName}");
                    return shader;
                }
            }

            return null;
        }

        /// <summary>
        /// GameObject에 URP 머티리얼 적용
        /// </summary>
        public static void ApplyURPMaterial(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;

            Material urpMaterial = GetURPLitMaterial();
            if (urpMaterial == null) return;

            // 새 머티리얼 인스턴스 생성 (공유 방지)
            renderer.material = new Material(urpMaterial);
            renderer.material.color = color;
        }

        /// <summary>
        /// Primitive GameObject 생성 및 URP 머티리얼 자동 적용
        /// </summary>
        public static GameObject CreatePrimitiveWithURPMaterial(PrimitiveType primitiveType, Color color)
        {
            GameObject obj = GameObject.CreatePrimitive(primitiveType);
            ApplyURPMaterial(obj, color);
            return obj;
        }
    }
}
