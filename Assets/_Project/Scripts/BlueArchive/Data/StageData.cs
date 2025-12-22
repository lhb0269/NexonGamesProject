using UnityEngine;
using System.Collections.Generic;

namespace NexonGame.BlueArchive.Data
{
    /// <summary>
    /// 스테이지 데이터 (Normal 1-4 등)
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "BlueArchive/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("스테이지 정보")]
        public string stageName = "Normal 1-4";
        public int stageId = 104;  // 1-4 = 104

        [Header("그리드 설정")]
        public int gridWidth = 10;
        public int gridHeight = 5;

        [Header("발판 정보")]
        public List<Vector2Int> platformPositions = new List<Vector2Int>();
        public Vector2Int startPosition = new Vector2Int(0, 2);  // 시작 위치
        public Vector2Int battlePosition = new Vector2Int(7, 2); // 전투 진입 위치

        [Header("적 정보")]
        public List<EnemySpawnData> enemies = new List<EnemySpawnData>();

        [Header("보상")]
        public List<RewardItemData> rewards = new List<RewardItemData>();

        [Header("클리어 조건")]
        public int requiredKills = 3;  // 처치해야 할 적 수
    }

    [System.Serializable]
    public class EnemySpawnData
    {
        public string enemyName;
        public int hp = 500;
        public int attack = 50;
        public int defense = 20;
        public Vector2Int spawnPosition;
    }
}
