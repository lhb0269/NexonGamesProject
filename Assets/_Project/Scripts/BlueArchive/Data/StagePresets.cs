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

            // 그리드 설정 (4x3 그리드)
            stageData.gridWidth = 4;
            stageData.gridHeight = 3;

            // 시작 위치와 전투 위치
            stageData.startPosition = new Vector2Int(0, 0); // 왼쪽 상단
            stageData.battlePosition = new Vector2Int(3, 1); // 오른쪽 중앙 (전투)

            // 플랫폼 위치
            // 레이아웃:
            //       0   1   2   3
            //   0   S              (시작)
            //   1       -   -   B  (일반-일반-전투)
            //   2   -              (일반)
            stageData.platformPositions = new List<Vector2Int>
            {
                new Vector2Int(0, 2), // 일반 플랫폼 1 (하단)
                new Vector2Int(1, 1), // 일반 플랫폼 2 (중앙)
                new Vector2Int(2, 1)  // 일반 플랫폼 3 (중앙)
                // (0, 0)은 startPosition으로 자동 생성됨
                // (3, 1)은 battlePosition으로 자동 생성됨
            };

            // 적 스폰 데이터
            stageData.enemies = new List<EnemySpawnData>
            {
                new EnemySpawnData
                {
                    enemyName = "일반병 A",
                    spawnPosition = new Vector2Int(3, 0),
                    hp = 1200,
                    attack = 50,
                    defense = 20
                },
                new EnemySpawnData
                {
                    enemyName = "일반병 B",
                    spawnPosition = new Vector2Int(3, 1),
                    hp = 1200,
                    attack = 50,
                    defense = 20
                },
                new EnemySpawnData
                {
                    enemyName = "정예병",
                    spawnPosition = new Vector2Int(3, 2),
                    hp = 2500,
                    attack = 80,
                    defense = 30
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
- 그리드: 4x3
- 시작 위치: (0, 0)
- 전투 위치: (3, 1) - 빨간색 전투 플랫폼
- 레이아웃:
        0   1   2   3
    0   [S]              (시작)
    1       -   -   [B]  (일반-일반-전투)
    2   -                (일반)

## 플랫폼 구성 (총 5개)
- 시작 플랫폼: 1개 (0,0) - 초록색
- 일반 플랫폼: 3개 (0,2), (1,1), (2,1) - 회색
- 전투 플랫폼: 1개 (3,1) - 빨간색

## 적 구성
1. 일반병 A
   - HP: 1200 | ATK: 50 | DEF: 20
   - 위치: (3, 0)

2. 일반병 B
   - HP: 1200 | ATK: 50 | DEF: 20
   - 위치: (3, 1)

3. 정예병 (보스)
   - HP: 2500 | ATK: 80 | DEF: 30
   - 위치: (3, 2)

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
