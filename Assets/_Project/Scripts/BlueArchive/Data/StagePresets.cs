using System.Collections.Generic;
using UnityEngine;

namespace NexonGame.BlueArchive.Data
{
    /// <summary>
    /// 스테이지 프리셋 데이터
    /// - Normal 1-4 스테이지 설정
    /// </summary>
    public static class StagePresets
    {
        /// <summary>
        /// Normal 1-4 스테이지 데이터 생성
        /// </summary>
        public static StageData CreateNormal1_4()
        {
            var stageData = ScriptableObject.CreateInstance<StageData>();

            // 기본 정보
            stageData.stageName = "Normal 1-4";
            stageData.stageId = 104;

            // 그리드 설정 (10x5 그리드)
            stageData.gridWidth = 10;
            stageData.gridHeight = 5;

            // 시작 위치와 전투 위치
            stageData.startPosition = new Vector2Int(0, 2); // 왼쪽 중앙
            stageData.battlePosition = new Vector2Int(7, 2); // 오른쪽에서 3칸 왼쪽

            // 플랫폼 위치 (일직선 경로)
            stageData.platformPositions = new List<Vector2Int>
            {
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2),
                new Vector2Int(5, 2),
                new Vector2Int(6, 2)
            };

            // 적 스폰 데이터
            stageData.enemies = new List<EnemySpawnData>
            {
                new EnemySpawnData
                {
                    enemyName = "일반병 A",
                    spawnPosition = new Vector2Int(8, 2),
                    enemyHP = 800,
                    enemyAttack = 45,
                    enemyDefense = 15
                },
                new EnemySpawnData
                {
                    enemyName = "일반병 B",
                    spawnPosition = new Vector2Int(9, 1),
                    enemyHP = 800,
                    enemyAttack = 45,
                    enemyDefense = 15
                },
                new EnemySpawnData
                {
                    enemyName = "정예병",
                    spawnPosition = new Vector2Int(9, 3),
                    enemyHP = 1200,
                    enemyAttack = 60,
                    enemyDefense = 20
                }
            };

            // 보상 설정
            stageData.rewards = new List<RewardItemData>();

            // 화폐 보상
            var currencyReward = ScriptableObject.CreateInstance<RewardItemData>();
            currencyReward.itemName = "크레딧";
            currencyReward.itemType = RewardItemType.Currency;
            currencyReward.quantity = 1000;
            stageData.rewards.Add(currencyReward);

            // 재료 보상
            var materialReward = ScriptableObject.CreateInstance<RewardItemData>();
            materialReward.itemName = "노트";
            materialReward.itemType = RewardItemType.Material;
            materialReward.quantity = 5;
            stageData.rewards.Add(materialReward);

            // 장비 보상
            var equipmentReward = ScriptableObject.CreateInstance<RewardItemData>();
            equipmentReward.itemName = "T1 가방";
            equipmentReward.itemType = RewardItemType.Equipment;
            equipmentReward.quantity = 1;
            stageData.rewards.Add(equipmentReward);

            // 경험치 보상
            var expReward = ScriptableObject.CreateInstance<RewardItemData>();
            expReward.itemName = "전술 EXP";
            expReward.itemType = RewardItemType.Exp;
            expReward.quantity = 150;
            stageData.rewards.Add(expReward);

            // 클리어 조건
            stageData.requiredKills = 3; // 3명의 적 모두 격파

            return stageData;
        }

        /// <summary>
        /// 생성된 스테이지 데이터 정리
        /// </summary>
        public static void DestroyStageData(StageData stageData)
        {
            if (stageData != null)
            {
                // 보상 데이터 정리
                if (stageData.rewards != null)
                {
                    foreach (var reward in stageData.rewards)
                    {
                        if (reward != null)
                        {
                            Object.DestroyImmediate(reward);
                        }
                    }
                }

                Object.DestroyImmediate(stageData);
            }
        }

        /// <summary>
        /// Normal 1-4 스테이지 정보
        /// </summary>
        public static string GetStageInfo()
        {
            return @"
=== Normal 1-4 스테이지 ===

## 맵 구성
- 그리드: 10x5
- 시작 위치: (0, 2)
- 전투 위치: (7, 2)
- 플랫폼: 일직선 경로 (6칸)

## 적 구성
1. 일반병 A
   - HP: 800 | ATK: 45 | DEF: 15
   - 위치: (8, 2)

2. 일반병 B
   - HP: 800 | ATK: 45 | DEF: 15
   - 위치: (9, 1)

3. 정예병 (보스)
   - HP: 1200 | ATK: 60 | DEF: 20
   - 위치: (9, 3)

## 클리어 조건
- 모든 적 격파 (3명)

## 보상
- 크레딧 x1000 (Currency)
- 노트 x5 (Material)
- T1 가방 x1 (Equipment)
- 전술 EXP x150 (Exp)
";
        }
    }
}
