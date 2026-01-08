//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Serialization;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Assertions;
//using static TST.UserItemDTO;

//namespace TST
//{
//    public interface ILoader<Key, Value>
//    {
//        public Dictionary<Key,Value> MakeDict();
//    }

//    [System.Serializable]
//    public class SaveLoadDataWrapper<T> : ILoader<int, T> where T : RootDataDTO
//    {
//        public List<T> Values = new List<T>();

//        public Dictionary<int, T> MakeDict()
//        {
//            Dictionary<int, T> dict = new Dictionary<int, T>();
//            foreach(T value in Values)
//                dict.Add(value.ID, value);
//            return dict;
//        }
//    }

//    public class UserDataModel : SingletonBase<UserDataModel>
//    {
//        [field: SerializeField] public Dictionary<int, IngamePlayerDataDTO> IngamePlayerData { get; private set; } = new Dictionary<int, IngamePlayerDataDTO> ();
//        [field: SerializeField] public Dictionary<int, IngameMonsterDataDTO> ingameMonsterData { get; private set; } = new Dictionary<int, IngameMonsterDataDTO> ();

//        [field: SerializeField] public UserItemDTO UserItemData { get; private set; } = new UserItemDTO();
//        [field: SerializeField] public PlayerEquipmentDTO PlayerEquipmentData { get; private set; } = new PlayerEquipmentDTO();

//        public event System.Action<UserItemDTO.UserItemData> OnUserItemChangedEvent;
//        public event System.Action<ItemEquipmentCategory, int, int> OnPlayerEquipmentChanagedEvent; // BeforeSlotId, AfterSlotId

//        public void Initialize()
//        {
//            //IngamePlayerData = LoadData<IngamePlayerDataDTO>().MakeDict();
//            //ingameMonsterData = LoadData<IngameMonsterDataDTO>().MakeDict();
//        }

//        public void ChangeData<T>(int id, T data) where T : RootDataDTO
//        {
//            var dictionary = GetDictionaryForType<T>();
//            if (dictionary != null)
//            {
//                dictionary[id] = data;
//                Debug.Log($"Saved data for ID {id} of type {typeof(T).Name}");
//            }
//            else
//            {
//                Debug.LogError($"Unsupported type: {typeof(T).Name}");
//            }
//        }
  
//        // 함수 계속 추가해야함
//        private Dictionary<int, T> GetDictionaryForType<T>() where T : RootDataDTO
//        {
//            if (typeof(T) == typeof(IngamePlayerDataDTO))
//            {
//                return IngamePlayerData as Dictionary<int, T>;
//            }
//            else if (typeof(T) == typeof(IngameMonsterDataDTO))
//            {
//                return ingameMonsterData as Dictionary<int, T>;
//            }

//            return null;
//        }

//        private void SaveAllInGameData()
//        {
//            // TODO : Dictionary 멤버 추가될때마다 늘어나야함
//            SaveData(IngamePlayerData);
//            //SaveData(ingameMonsterData);
//        }

//        private void OnDisable()
//        {
//            // 데이터 저장 용도
//            SaveAllInGameData();
//        }
        
//        public void AddItemToInventory(ItemData itemData, int count = 1)
//        {
//            // TODO : UserItemData에 먹은 아이템 추가
//            // TODO : 기존에 먹은 아이템이 있는가? 있으면 카운트만 증가, 없으면 새로 추가
//            // TODO : 기존에 먹은 아이템이 있지만, 해당 슬롯의 Count가 MaxCount 까지 넘어갔는가? 넘어갔으면 새로운 슬롯에 추가
//            for (int i = 0; i < count; i++) 
//            {
//                UserItemDTO.UserItemData changedData = null;
//                int existedItemDataIndex = UserItemData.Items.FindLastIndex(x => x.itemID.Equals(itemData.ItemID));
//                if (existedItemDataIndex >= 0)
//                {
//                    bool isExistGameData = GameDataModel.Singleton.GetItemData(itemData.ItemID, out var itemGameData);

//                    // TODO : 아이템 게임 데이터가 없는것에 대한 예외처리
//                    Assert.IsTrue(isExistGameData, $"ItemData {itemData.ItemID} is not exist in GameDataModel");

//                    int limitStack = itemGameData.ItemMaxStack;
//                    // 5개 있고 7개 추가 = 최대 10개
//                    if (UserItemData.Items[existedItemDataIndex].itemCount + 1 <= limitStack)
//                    {
//                        UserItemData.Items[existedItemDataIndex].itemCount += 1;
//                        changedData = UserItemData.Items[existedItemDataIndex];
//                    }
//                    else
//                    {
//                        changedData = new UserItemDTO.UserItemData()
//                        {
//                            slotID = UserItemData.Items.Count,
//                            itemID = itemData.ItemID,
//                            itemCount = 1
//                        };
//                        UserItemData.Items.Add(changedData);
//                    }
//                }
//                else
//                {
//                    changedData = new UserItemDTO.UserItemData()
//                    {
//                        slotID = UserItemData.Items.Count,
//                        itemID = itemData.ItemID,
//                        itemCount = 1
//                    };
//                    UserItemData.Items.Add(changedData);
//                }

//                // TODO : 데이터 저장
//                // TODO : UserDataModel 의 OnUserItemChangedEvent 를 호출해주자.
//                OnUserItemChangedEvent?.Invoke(changedData);
//            }
//        }

        
//        public bool UseInventoryItem(ItemData itemData, int useCount)
//        {
//            var itemDataTemp = UserItemData.Items.Find(x => x.itemID.Equals(itemData.ItemID));
//            if (itemDataTemp == null)
//                return false;

//            bool isSucceed = RecursiveSearch(itemData, useCount);

//            return true;
//        }

//        public void UnEquipItem(ItemEquipmentCategory category, int slotId)
//        {
//            if (PlayerEquipmentData.equipmentItems[category] != slotId)
//                return;

//            int beforeSlotID = slotId;
//            int afterSlotID = -1;
//            PlayerEquipmentData.equipmentItems[category] = -1;

//            // 플레이어 장비 변경 이벤트 발생
//            OnPlayerEquipmentChanagedEvent.Invoke(category, beforeSlotID, afterSlotID);
//        }

//        // 동일한 파츠의 장비를 갈아낄수도 있다.!
//        public void EquipItem(ItemEquipmentCategory category, int slotId)
//        {
//            int beforeSlotID = -1;
//            int afterSlotID = slotId;

//            if (PlayerEquipmentData.equipmentItems[category] >= 0) // 기존에 장착된 아이템이 있는 경우
//            {
//                // UnEquip Item
//                beforeSlotID = PlayerEquipmentData.equipmentItems[category];
//                PlayerEquipmentData.equipmentItems[category] = -1;
//            }

//            // 새로 전달받은 인벤토리 SlotId 값을 EquipData에 덮어 씌운다
//            PlayerEquipmentData.equipmentItems[category] = slotId;

//            // 플레이어 장비 변경 이벤트 발생
//            OnPlayerEquipmentChanagedEvent?.Invoke(category, beforeSlotID, afterSlotID);
//        }

//        private bool RecursiveSearch(ItemData itemData, int useCount)
//        {
//            int existedItemDataIndex = UserItemData.Items.FindLastIndex(x => x.itemID.Equals(itemData.ItemID));

//            UserItemDTO.UserItemData changedData = null;

//            if (existedItemDataIndex < 0)
//                return false;

//            if (existedItemDataIndex >= 0)
//            {
//                bool isExistGameData = GameDataModel.Singleton.GetItemData(itemData.ItemID, out var itemGameData);

//                changedData = UserItemData.Items[existedItemDataIndex];
//                Assert.IsTrue(isExistGameData, $"ItemData {itemData.ItemID} is not exist in GameDataModel");

//                int minimumZone = 0;
//                if (UserItemData.Items[existedItemDataIndex].itemCount - useCount > minimumZone)
//                {
//                    UserItemData.Items[existedItemDataIndex].itemCount -= useCount;

//                    changedData = UserItemData.Items[existedItemDataIndex];
//                }
//                else
//                {
//                    int maxcnt = UserItemData.Items[existedItemDataIndex].itemCount;

//                    // 음수로 나올 것 
//                    int remainCount = UserItemData.Items[existedItemDataIndex].itemCount - useCount;
//                    // 일단 사용한 인덱스 만큼은 뺀다.
//                    UserItemData.Items[existedItemDataIndex].itemCount = 0;
//                    changedData = UserItemData.Items[existedItemDataIndex];

//                    // 일단 기존의 뺀 데이터에 대한 정보 보내기
//                    OnUserItemChangedEvent?.Invoke(changedData);

//                    UserItemData.Items.RemoveAt(existedItemDataIndex);
//                    RecursiveSearch(itemData, -remainCount); 
//                }
//            } 

//            if (existedItemDataIndex >= 0)
//            { 
//                OnUserItemChangedEvent?.Invoke(changedData);
//                return true;
//            }

//            return false;
//        }

//        #region SAVE / LOAD Core Method
//        private SaveLoadDataWrapper<T> LoadData<T>() where T : RootDataDTO
//        {
//#if UNITY_EDITOR
//            string path = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Json/{typeof(T).Name}.json";
//#else
//            string path = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
//#endif
//            if (FileManager.ReadFileData(path, out string loadedEditorData))
//            {
//                // JSON 역직렬화
//                var wrapper = JsonConvert.DeserializeObject<SaveLoadDataWrapper<T>>(loadedEditorData);

//                return wrapper;
//            }
            
//            Debug.Log($"Failed to Load Data {typeof(T).Name}");
//            return null;
//        }

//        private void SaveData<T>(Dictionary<int, T> newData) where T : RootDataDTO
//        {
//#if UNITY_EDITOR
//            string jsonPath = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Json/{typeof(T).Name}.json";
//            string csvPath = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Csv/{typeof(T).Name}.csv";
//#else
//            string jsonPath = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
//            string csvPath = $"{Application.persistentDataPath}/{typeof(T).Name}.csv";
//#endif
//            // JSON 저장
//            SaveLoadDataWrapper<T> wrapper = new SaveLoadDataWrapper<T>();
//            foreach (var dic in newData)
//            {
//                wrapper.Values.Add(newData[dic.Key]);
//            }

//            var settings = new JsonSerializerSettings
//            {
//                ContractResolver = new ParentFirstContractResolver(),
//                Converters = new List<JsonConverter>
//                {
//                    new Vector3Converter(),
//                    new QuaternionConverter()
//                },
//                Formatting = Formatting.Indented
//            };

//            if (wrapper == null || !wrapper.Values.Any())
//                return;

//            var jsonData = JsonConvert.SerializeObject(wrapper, settings);
//            FileManager.WriteFileFromString(jsonPath, jsonData);
//            Debug.Log($"Save Data to JSON Success: {jsonData}");

//            // CSV 저장
//            if (SaveToCsv(wrapper.Values, csvPath))
//                Debug.Log($"Save Data to CSV Success: {csvPath}");
//        }

//        private static bool SaveToCsv<T>(IEnumerable<T> dataCollection, string filePath) where T : RootDataDTO
//        {
//            if (dataCollection == null || !dataCollection.Any())
//            {
//                Debug.LogError("Data collection is null or empty.");
//                return false;
//            }

//            var csvBuilder = new StringBuilder();

//            // 부모 클래스부터 순차적으로 속성 추출
//            var properties = GetPropertiesInHierarchy(typeof(T));

//            // 헤더 생성
//            csvBuilder.AppendLine(string.Join(",", properties.Select(p => p.Name)));

//            // 데이터 추가
//            foreach (var data in dataCollection)
//            {
//                var values = properties.Select(p =>
//                {
//                    var value = p.GetValue(data);

//                    if (value == null)
//                        return ""; // Null 값을 빈 문자열로 처리

//                    if (value is IEnumerable enumerable && !(value is string))
//                    {
//                        return $"\"{string.Join("&", enumerable.Cast<object>())}\""; // 리스트는 '&'로 구분
//                    }
//                    else if (value is Vector3 vector)
//                    {
//                        return $"\"({vector.x},{vector.y},{vector.z})\""; // Vector3 형식
//                    }
//                    else if (value is Quaternion quaternion)
//                    {
//                        return $"\"({quaternion.x},{quaternion.y},{quaternion.z},{quaternion.w})\""; // Quaternion 형식
//                    }
//                    else
//                    {
//                        return value?.ToString()?.Replace(",", " ").Replace("\"", "\"\""); // 쉼표와 큰따옴표 이스케이프
//                    }
//                });

//                csvBuilder.AppendLine(string.Join(",", values));
//            }

//            // 파일 저장
//            try
//            {
//                FileManager.WriteFileFromString(filePath, csvBuilder.ToString());
//            }
//            catch (IOException ex)
//            {
//                Debug.LogError($"File is locked or cannot be accessed: {filePath}. Error: {ex.Message}");
//                return false;
//            }

//            return true;
//        }

//        // 부모 클래스부터 속성 추출
//        private static List<FieldInfo> GetPropertiesInHierarchy(Type type)
//        {
//            var properties = new List<FieldInfo>();
//            var types = new List<Type>();

//            while (type != null && type != typeof(object))
//            {
//                types.Add(type);
//                type = type.BaseType; // 부모 클래스로 이동
//            }

//            // 부모부터 돌도록
//            types.Reverse();

//            for (int i = 0; i < types.Count; i++)
//            {
//                properties.AddRange(types[i].GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
//            }

//            return properties;
//        }
 
//        #endregion

//        #region Serialize & Deserialize & FindParentProperty
//        public class ParentFirstContractResolver : DefaultContractResolver
//        {
//            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
//            {
//                var properties = base.CreateProperties(type, memberSerialization);

//                // 부모 클래스의 속성을 먼저 정렬
//                return properties
//                    .OrderBy(p => GetInheritanceDepth(p.DeclaringType)) // 상속 깊이에 따라 정렬
//                    .ThenBy(p => p.Order ?? int.MaxValue) // JsonProperty(Order) 속성 적용
//                    .ToList();
//            }

//            private int GetInheritanceDepth(Type type)
//            {
//                int depth = 0;
//                while (type.BaseType != null)
//                {
//                    depth++;
//                    type = type.BaseType;
//                }
//                return depth;
//            }
//        }

//        public class Vector3Converter : JsonConverter
//        {
//            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//            {
//                var vector = (Vector3)value;
//                writer.WriteStartObject();
//                writer.WritePropertyName("x");
//                writer.WriteValue(vector.x);
//                writer.WritePropertyName("y");
//                writer.WriteValue(vector.y);
//                writer.WritePropertyName("z");
//                writer.WriteValue(vector.z);
//                writer.WriteEndObject();
//            }

//            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//            {
//                var obj = JObject.Load(reader);
//                return new Vector3(
//                    (float)obj["x"],
//                    (float)obj["y"],
//                    (float)obj["z"]
//                );
//            }

//            public override bool CanConvert(Type objectType)
//            {
//                return objectType == typeof(Vector3);
//            }
//        }

//        public class QuaternionConverter : JsonConverter
//        {
//            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//            {
//                var quaternion = (Quaternion)value;
//                writer.WriteStartObject();
//                writer.WritePropertyName("x");
//                writer.WriteValue(quaternion.x);
//                writer.WritePropertyName("y");
//                writer.WriteValue(quaternion.y);
//                writer.WritePropertyName("z");
//                writer.WriteValue(quaternion.z);
//                writer.WritePropertyName("w");
//                writer.WriteValue(quaternion.w);
//                writer.WriteEndObject();
//            }

//            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//            {
//                var obj = JObject.Load(reader);
//                return new Quaternion(
//                    (float)obj["x"],
//                    (float)obj["y"],
//                    (float)obj["z"],
//                    (float)obj["w"]
//                );
//            }

//            public override bool CanConvert(Type objectType)
//            {
//                return objectType == typeof(Quaternion);
//            }
//        }
//        #endregion
//    }
//}
