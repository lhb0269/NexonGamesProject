using UnityEngine;

namespace NexonGame.BlueArchive.Data
{
    /// <summary>
    /// 학생(캐릭터) 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "StudentData", menuName = "BlueArchive/Student Data")]
    public class StudentData : ScriptableObject
    {
        [Header("기본 정보")]
        public string studentName;
        public int studentId;

        [Header("전투 스탯")]
        public int maxHP = 1000;
        public int attack = 100;
        public int defense = 50;

        [Header("스킬")]
        public SkillData exSkill;

        [Header("비주얼")]
        public Sprite portrait;
        public Color teamColor = Color.blue;
    }
}
