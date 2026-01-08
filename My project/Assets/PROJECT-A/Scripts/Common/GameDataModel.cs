//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace TST
//{
//    public class GameDataModel : SingletonBase<GameDataModel>
//    {
//        [field: SerializeField] public List<ItemData> ItemDatas { get; private set; } = new List<ItemData>();
//        [field: SerializeField] public List<NPCShopDataSO> NpcShopDatas { get; private set; } = new List<NPCShopDataSO>();
//        [field: SerializeField] public List<SkillDataSO> SkillDatas { get; private set; } = new List<SkillDataSO>();
//        [field: SerializeField] public List<CraftDataSO> CraftingDatas { get; private set; } = new List<CraftDataSO>();

//        public void Initialize()
//        {

//        }

//        public bool GetItemData(string itemId, out ItemData resultData)
//        {
//            resultData = ItemDatas.Find(x => x.ItemID == itemId);
//            return resultData != null;
//        }

//        public bool GetSkillData(string skill_id, out SkillData resultData)
//        {
//            resultData = SkillDatas.Find(x => x.SkillData.skillID.Equals(skill_id))?.SkillData;
//            return resultData != null;
//        }

//        public bool GetCraftingData(string crafting_id, out CraftDataSO resultData)
//        {
//            resultData = CraftingDatas.Find(x => x.CraftingID.Equals(crafting_id));
//            return resultData != null;
//        }

//        public bool GetNpcShopData(string shopping_id, out NPCShopDataSO resultData)
//        {
//            resultData = NpcShopDatas.Find(x => x.NpcShopID.Equals(shopping_id));
//            return resultData != null;
//        }
//    }
//}
