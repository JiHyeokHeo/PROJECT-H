//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace TST
//{
//    // GameManager를 사용하는 두개의 목적을 띄고 있다 
//    // 1. 각종 게임에서 발생하는 이벤트를 처리해주는 역할[중추적인 역할]을 수행
//    // 2. Ingame이 아닌 외부 클래스에 Event를 Notify 시켜주는 역할 

//    // => 얻을 수 있는게 무엇일까???
//    // 1. UI 처리가 용이하게 변경이 될 수 있다.
//    // 2. 통합적인 이벤트, 처리가 가능해진다.

//    // partial이란? 
//    // 1. 하나의 클래스를 여러개의 *.cs 파일로 나누어서 구현할 수 있게 도와주는 키워드, // 덩치가 커지면 나눌 수 있다!
//    // !!! 주의 할점 1000줄 넘으면 코드 다이어트 고려를 하자~! 그때 사용하는것이 partial ->
//    public partial class GameManager : MonoBehaviour // 중계기 역할
//    {
//        public static GameManager Instance { get; private set; }

//        private void Awake()
//        {
//            Instance = this;
//            AwakeVariable(); // Awake 동시 사용을 못하기에 해결하는 방식
//            //AwakeEvent();
//        }

//        private void OnDestroy()
//        {
//            Instance = null;
//        }

//        [Button()]
//        // 유저데이터와 별개로 테스트 하기 위해
//        public void UseStaticItem(int slotId, int useCount, bool forceUse = false)
//        {
//            // 강제처리
//            if (forceUse)
//            {
                
//            }
//            else
//            {
//                var targetItemData = UserDataModel.Singleton.UserItemData.Items.Find(x => x.slotID == slotId);
//                if (targetItemData == null)
//                    return;

//                if (GameDataModel.Singleton.GetItemData(targetItemData.itemID, out var itemData))
//                    UserDataModel.Singleton.UseInventoryItem(itemData, useCount);
//            }
//        }

//        [Button()]
//        public void UseGenerateItem(Vector3 position)
//        {
//            GenerateItem(position);
//        }


//        [Button()]
//        // 유저데이터와 별개로 테스트 하기 위해
//        public void AddItem(string itemId, int useCount, bool forceUse = false)
//        {
//            if (GameDataModel.Singleton.GetItemData(itemId, out var itemData))
//                UserDataModel.Singleton.AddItemToInventory(itemData, useCount);
//        }

//        [Button()]
//        // 유저데이터와 별개로 테스트 하기 위해
//        public void MakePlayerDamaged(float damage)
//        {
//            CharacterController.Instance.linkedCharacter.eventHandler.OnDamaged(damage, null);
//        }

//        public void OnPlayerDead()
//        {
//            // # Game Over UI를 띄운다 등등..
//            // # Dungeon Reset
//            // # ...
//        }
//    }
//}
