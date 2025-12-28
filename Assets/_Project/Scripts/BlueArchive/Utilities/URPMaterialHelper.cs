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

        /// <summary>
        /// URP/Lit 셰이더를 사용하는 기본 머티리얼 가져오기
        /// </summary>
        public static Material GetURPLitMaterial()
        {
            if (_urpLitMaterial == null)
            {
                // URP/Lit 셰이더 찾기
                Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

                if (urpLitShader == null)
                {
                    Debug.LogWarning("[URPMaterialHelper] URP/Lit 셰이더를 찾을 수 없습니다. Fallback으로 Unlit 사용");
                    urpLitShader = Shader.Find("Universal Render Pipeline/Unlit");
                }

                if (urpLitShader == null)
                {
                    Debug.LogError("[URPMaterialHelper] URP 셰이더를 찾을 수 없습니다!");
                    return null;
                }

                _urpLitMaterial = new Material(urpLitShader);
                _urpLitMaterial.name = "URP_Lit_Default";
            }

            return _urpLitMaterial;
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
