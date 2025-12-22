using UnityEngine;
using System.Collections.Generic;

namespace NexonGame.BlueArchive.Data
{
    /// <summary>
    /// 블루 아카이브 학생 프리셋 데이터
    /// - 실제 게임에서는 ScriptableObject로 생성
    /// - 테스트 및 초기 데이터용으로 사용
    /// </summary>
    public static class StudentPresets
    {
        /// <summary>
        /// 시로코 (Shiroko) - 아비도스 소속, 라이플
        /// </summary>
        public static StudentData CreateShiroko()
        {
            var skillData = ScriptableObject.CreateInstance<SkillData>();
            skillData.skillName = "섬멸의 주사위";
            skillData.costAmount = 3;
            skillData.baseDamage = 650;
            skillData.damageMultiplier = 1.8f;
            skillData.targetType = SkillTargetType.Single;
            skillData.cooldownTime = 20f;

            var studentData = ScriptableObject.CreateInstance<StudentData>();
            studentData.studentName = "Shiroko";
            studentData.studentId = 1;
            studentData.maxHP = 2431;
            studentData.attack = 478;
            studentData.defense = 16;
            studentData.exSkill = skillData;
            studentData.teamColor = new Color(0.2f, 0.5f, 0.8f); // 블루

            return studentData;
        }

        /// <summary>
        /// 호시노 (Hoshino) - 게헨나 소속, 방패 탱커
        /// </summary>
        public static StudentData CreateHoshino()
        {
            var skillData = ScriptableObject.CreateInstance<SkillData>();
            skillData.skillName = "낙천적 방패";
            skillData.costAmount = 4;
            skillData.baseDamage = 450;
            skillData.damageMultiplier = 1.5f;
            skillData.targetType = SkillTargetType.Single;
            skillData.cooldownTime = 25f;

            var studentData = ScriptableObject.CreateInstance<StudentData>();
            studentData.studentName = "Hoshino";
            studentData.studentId = 2;
            studentData.maxHP = 3528;
            studentData.attack = 382;
            studentData.defense = 28;
            studentData.exSkill = skillData;
            studentData.teamColor = new Color(0.8f, 0.3f, 0.5f); // 핑크

            return studentData;
        }

        /// <summary>
        /// 아루 (Aru) - 게헨나 문제해결부, 권총
        /// </summary>
        public static StudentData CreateAru()
        {
            var skillData = ScriptableObject.CreateInstance<SkillData>();
            skillData.skillName = "더 퍼펙트 범죄";
            skillData.costAmount = 3;
            skillData.baseDamage = 580;
            skillData.damageMultiplier = 2.0f;
            skillData.targetType = SkillTargetType.Multiple;
            skillData.cooldownTime = 22f;

            var studentData = ScriptableObject.CreateInstance<StudentData>();
            studentData.studentName = "Aru";
            studentData.studentId = 3;
            studentData.maxHP = 2156;
            studentData.attack = 524;
            studentData.defense = 14;
            studentData.exSkill = skillData;
            studentData.teamColor = new Color(0.9f, 0.2f, 0.2f); // 레드

            return studentData;
        }

        /// <summary>
        /// 하루나 (Haruna) - 게헨나 소속, 저격총
        /// </summary>
        public static StudentData CreateHaruna()
        {
            var skillData = ScriptableObject.CreateInstance<SkillData>();
            skillData.skillName = "라스트 불릿";
            skillData.costAmount = 5;
            skillData.baseDamage = 850;
            skillData.damageMultiplier = 2.5f;
            skillData.targetType = SkillTargetType.Single;
            skillData.cooldownTime = 30f;

            var studentData = ScriptableObject.CreateInstance<StudentData>();
            studentData.studentName = "Haruna";
            studentData.studentId = 4;
            studentData.maxHP = 1989;
            studentData.attack = 612;
            studentData.defense = 12;
            studentData.exSkill = skillData;
            studentData.teamColor = new Color(0.4f, 0.2f, 0.7f); // 퍼플

            return studentData;
        }

        /// <summary>
        /// 모든 학생 프리셋 목록 반환
        /// </summary>
        public static List<StudentData> CreateAllStudents()
        {
            return new List<StudentData>
            {
                CreateShiroko(),
                CreateHoshino(),
                CreateAru(),
                CreateHaruna()
            };
        }

        /// <summary>
        /// Normal 1-4 스테이지용 적 데이터 프리셋
        /// </summary>
        public static List<Character.EnemyData> CreateNormal1_4Enemies()
        {
            return new List<Character.EnemyData>
            {
                new Character.EnemyData("일반병 A", 800, 45, 15),
                new Character.EnemyData("일반병 B", 800, 45, 15),
                new Character.EnemyData("정예병", 1200, 60, 20)
            };
        }

        /// <summary>
        /// 생성된 데이터 정리 (테스트 후 정리용)
        /// </summary>
        public static void DestroyStudentData(StudentData studentData)
        {
            if (studentData != null)
            {
                if (studentData.exSkill != null)
                {
                    Object.DestroyImmediate(studentData.exSkill);
                }
                Object.DestroyImmediate(studentData);
            }
        }

        /// <summary>
        /// 모든 학생 데이터 정리
        /// </summary>
        public static void DestroyAllStudents(List<StudentData> students)
        {
            foreach (var student in students)
            {
                DestroyStudentData(student);
            }
        }
    }

    /// <summary>
    /// 학생 프리셋 통계 정보
    /// </summary>
    public static class StudentPresetInfo
    {
        public const int TotalStudents = 4;

        public static string GetStudentInfo()
        {
            return @"
=== 블루 아카이브 학생 프리셋 ===

1. Shiroko (시로코)
   - 소속: 아비도스
   - 역할: 딜러
   - HP: 2431 | ATK: 478 | DEF: 16
   - EX 스킬: 섬멸의 주사위 (코스트 3)
   - 데미지: 650 x 1.8 = 1170 (단일 타겟)

2. Hoshino (호시노)
   - 소속: 게헨나
   - 역할: 탱커
   - HP: 3528 | ATK: 382 | DEF: 28
   - EX 스킬: 낙천적 방패 (코스트 4)
   - 데미지: 450 x 1.5 = 675 (단일 타겟)

3. Aru (아루)
   - 소속: 게헨나 문제해결부
   - 역할: 딜러
   - HP: 2156 | ATK: 524 | DEF: 14
   - EX 스킬: 더 퍼펙트 범죄 (코스트 3)
   - 데미지: 580 x 2.0 = 1160 (다중 타겟)

4. Haruna (하루나)
   - 소속: 게헨나
   - 역할: 저격수
   - HP: 1989 | ATK: 612 | DEF: 12
   - EX 스킬: 라스트 불릿 (코스트 5)
   - 데미지: 850 x 2.5 = 2125 (단일 타겟)

=== Normal 1-4 적 구성 ===
- 일반병 A: HP 800 | ATK 45 | DEF 15
- 일반병 B: HP 800 | ATK 45 | DEF 15
- 정예병: HP 1200 | ATK 60 | DEF 20
";
        }
    }
}
