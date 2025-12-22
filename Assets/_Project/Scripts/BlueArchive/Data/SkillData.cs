using UnityEngine;

namespace NexonGame.BlueArchive.Data
{
    /// <summary>
    /// 스킬 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "BlueArchive/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("스킬 정보")]
        public string skillName;
        public int skillId;

        [Header("코스트")]
        public int costAmount = 3;  // 스킬 사용에 필요한 코스트

        [Header("효과")]
        public int baseDamage = 500;
        public float damageMultiplier = 1.5f;
        public SkillTargetType targetType = SkillTargetType.Single;

        [Header("쿨다운")]
        public float cooldownTime = 20f;  // 초 단위

        [TextArea(3, 5)]
        public string description;
    }

    public enum SkillTargetType
    {
        Single,     // 단일 대상
        Multiple,   // 다중 대상
        Area        // 범위 공격
    }
}
