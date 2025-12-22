using UnityEngine;

namespace NexonGame.BlueArchive.Data
{
    /// <summary>
    /// 보상 아이템 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "RewardItemData", menuName = "BlueArchive/Reward Item")]
    public class RewardItemData : ScriptableObject
    {
        [Header("아이템 정보")]
        public string itemName;
        public int itemId;
        public RewardItemType itemType;

        [Header("수량")]
        public int quantity = 1;

        [Header("비주얼")]
        public Sprite icon;

        [TextArea(2, 4)]
        public string description;
    }

    public enum RewardItemType
    {
        Currency,   // 화폐 (골드, 크레딧 등)
        Material,   // 재료
        Equipment,  // 장비
        Exp         // 경험치
    }
}
