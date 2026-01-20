using System;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    [Serializable]
    public struct UserPlacedItem
    {
        public string itemId;
        public int cellX;
        public int cellY;
        public int rotation; // 필요 없으면 0만 쓰면 됨
    }

    public class UserDataModel : SingletonBase<UserDataModel>
    {
        public event Action<string> OnSelectedItemChanged;
        public event Action OnInventoryChanged;
        public event Action OnGardenChanged;

        public bool isSelected = false;
        public string SelectedItemId { get; private set; }

        [SerializeField]SerializableWrapDictionary<string, int> inventoryCounts = new SerializableWrapDictionary<string, int>();
        [SerializeField]SerializableWrapDictionary<Vector2Int, UserPlacedItem> gardenPlaced = new SerializableWrapDictionary<Vector2Int, UserPlacedItem>();

        public void Initialize()
        {
            // Day-4에서 로드 붙일 자리
        }

        public void SelectItem(string itemId)
        {
            isSelected = true;
            SelectedItemId = itemId;
            OnSelectedItemChanged?.Invoke(itemId);
        }

        public int GetItemCount(string itemId)
        {
            return inventoryCounts.TryGetValue(itemId, out int c) ? c : 0;
        }

        public IEnumerable<UserPlacedItem> GetAllPlaced()
        {
            foreach (var kv in gardenPlaced)
                yield return kv.Value;
        }

        public void AddItem(string itemId, int amount = 1)
        {
            if (!inventoryCounts.ContainsKey(itemId))
                inventoryCounts[itemId] = 0;

            inventoryCounts[itemId] += amount;
            OnInventoryChanged?.Invoke();
        }

        public bool TryPlace(string itemId, int cellX, int cellY, int rotation = 0)
        {
            var key = new Vector2Int(cellX, cellY);
            if (gardenPlaced.ContainsKey(key))
                return false;

            gardenPlaced[key] = new UserPlacedItem
            {
                itemId = itemId,
                cellX = cellX,
                cellY = cellY,
                rotation = rotation
            };

            OnGardenChanged?.Invoke();
            isSelected = false;
            return true;
        }

        public bool TryRemove(int cellX, int cellY)
        {
            var key = new Vector2Int(cellX, cellY);
            if (!gardenPlaced.Remove(key))
                return false;

            OnGardenChanged?.Invoke();
            return true;
        }

        public bool IsOccupied(int cellX, int cellY)
        {
            return gardenPlaced.ContainsKey(new Vector2Int(cellX, cellY));
        }

        public bool TryGetPlaced(int cellX, int cellY, out UserPlacedItem placed)
        {
            return gardenPlaced.TryGetValue(new Vector2Int(cellX, cellY), out placed);
        }

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

        ////        private void SaveData<T>(Dictionary<int, T> newData) where T : RootDataDTO
        ////        {
        ////#if UNITY_EDITOR
        ////            string jsonPath = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Json/{typeof(T).Name}.json";
        ////            string csvPath = $"Assets/PROJECT TST/Anothers/Editor Saved Data/Csv/{typeof(T).Name}.csv";
        ////#else
        ////            string jsonPath = $"{Application.persistentDataPath}/{typeof(T).Name}.json";
        ////            string csvPath = $"{Application.persistentDataPath}/{typeof(T).Name}.csv";
        ////#endif
        ////            // JSON 저장
        ////            SaveLoadDataWrapper<T> wrapper = new SaveLoadDataWrapper<T>();
        ////            foreach (var dic in newData)
        ////            {
        ////                wrapper.Values.Add(newData[dic.Key]);
        ////            }

        ////            var settings = new JsonSerializerSettings
        ////            {
        ////                ContractResolver = new ParentFirstContractResolver(),
        ////                Converters = new List<JsonConverter>
        ////                {
        ////                    new Vector3Converter(),
        ////                    new QuaternionConverter()
        ////                },
        ////                Formatting = Formatting.Indented
        ////            };

        ////            if (wrapper == null || !wrapper.Values.Any())
        ////                return;

        ////            var jsonData = JsonConvert.SerializeObject(wrapper, settings);
        ////            FileManager.WriteFileFromString(jsonPath, jsonData);
        ////            Debug.Log($"Save Data to JSON Success: {jsonData}");

        ////            // CSV 저장
        ////            if (SaveToCsv(wrapper.Values, csvPath))
        ////                Debug.Log($"Save Data to CSV Success: {csvPath}");
        ////        }

        ////        private static bool SaveToCsv<T>(IEnumerable<T> dataCollection, string filePath) where T : RootDataDTO
        ////        {
        ////            if (dataCollection == null || !dataCollection.Any())
        ////            {
        ////                Debug.LogError("Data collection is null or empty.");
        ////                return false;
        ////            }

        ////            var csvBuilder = new StringBuilder();

        ////            // 부모 클래스부터 순차적으로 속성 추출
        ////            var properties = GetPropertiesInHierarchy(typeof(T));

        ////            // 헤더 생성
        ////            csvBuilder.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        ////            // 데이터 추가
        ////            foreach (var data in dataCollection)
        ////            {
        ////                var values = properties.Select(p =>
        ////                {
        ////                    var value = p.GetValue(data);

        ////                    if (value == null)
        ////                        return ""; // Null 값을 빈 문자열로 처리

        ////                    if (value is IEnumerable enumerable && !(value is string))
        ////                    {
        ////                        return $"\"{string.Join("&", enumerable.Cast<object>())}\""; // 리스트는 '&'로 구분
        ////                    }
        ////                    else if (value is Vector3 vector)
        ////                    {
        ////                        return $"\"({vector.x},{vector.y},{vector.z})\""; // Vector3 형식
        ////                    }
        ////                    else if (value is Quaternion quaternion)
        ////                    {
        ////                        return $"\"({quaternion.x},{quaternion.y},{quaternion.z},{quaternion.w})\""; // Quaternion 형식
        ////                    }
        ////                    else
        ////                    {
        ////                        return value?.ToString()?.Replace(",", " ").Replace("\"", "\"\""); // 쉼표와 큰따옴표 이스케이프
        ////                    }
        ////                });

        ////                csvBuilder.AppendLine(string.Join(",", values));
        ////            }

        ////            // 파일 저장
        ////            try
        ////            {
        ////                FileManager.WriteFileFromString(filePath, csvBuilder.ToString());
        ////            }
        ////            catch (IOException ex)
        ////            {
        ////                Debug.LogError($"File is locked or cannot be accessed: {filePath}. Error: {ex.Message}");
        ////                return false;
        ////            }

        ////            return true;
        ////        }

        ////        // 부모 클래스부터 속성 추출
        ////        private static List<FieldInfo> GetPropertiesInHierarchy(Type type)
        ////        {
        ////            var properties = new List<FieldInfo>();
        ////            var types = new List<Type>();

        ////            while (type != null && type != typeof(object))
        ////            {
        ////                types.Add(type);
        ////                type = type.BaseType; // 부모 클래스로 이동
        ////            }

        ////            // 부모부터 돌도록
        ////            types.Reverse();

        ////            for (int i = 0; i < types.Count; i++)
        ////            {
        ////                properties.AddRange(types[i].GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        ////            }

        ////            return properties;
        ////        }

        //        #endregion

        //#region Serialize & Deserialize & FindParentProperty
        //public class ParentFirstContractResolver : DefaultContractResolver
        //{
        //    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        //    {
        //        var properties = base.CreateProperties(type, memberSerialization);

        //        // 부모 클래스의 속성을 먼저 정렬
        //        return properties
        //            .OrderBy(p => GetInheritanceDepth(p.DeclaringType)) // 상속 깊이에 따라 정렬
        //            .ThenBy(p => p.Order ?? int.MaxValue) // JsonProperty(Order) 속성 적용
        //            .ToList();
        //    }

        //    private int GetInheritanceDepth(Type type)
        //    {
        //        int depth = 0;
        //        while (type.BaseType != null)
        //        {
        //            depth++;
        //            type = type.BaseType;
        //        }
        //        return depth;
        //    }
        //}

        //public class Vector3Converter : JsonConverter
        //{
        //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //    {
        //        var vector = (Vector3)value;
        //        writer.WriteStartObject();
        //        writer.WritePropertyName("x");
        //        writer.WriteValue(vector.x);
        //        writer.WritePropertyName("y");
        //        writer.WriteValue(vector.y);
        //        writer.WritePropertyName("z");
        //        writer.WriteValue(vector.z);
        //        writer.WriteEndObject();
        //    }

        //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //    {
        //        var obj = JObject.Load(reader);
        //        return new Vector3(
        //            (float)obj["x"],
        //            (float)obj["y"],
        //            (float)obj["z"]
        //        );
        //    }

        //    public override bool CanConvert(Type objectType)
        //    {
        //        return objectType == typeof(Vector3);
        //    }
        //}

        //public class QuaternionConverter : JsonConverter
        //{
        //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //    {
        //        var quaternion = (Quaternion)value;
        //        writer.WriteStartObject();
        //        writer.WritePropertyName("x");
        //        writer.WriteValue(quaternion.x);
        //        writer.WritePropertyName("y");
        //        writer.WriteValue(quaternion.y);
        //        writer.WritePropertyName("z");
        //        writer.WriteValue(quaternion.z);
        //        writer.WritePropertyName("w");
        //        writer.WriteValue(quaternion.w);
        //        writer.WriteEndObject();
        //    }

        //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //    {
        //        var obj = JObject.Load(reader);
        //        return new Quaternion(
        //            (float)obj["x"],
        //            (float)obj["y"],
        //            (float)obj["z"],
        //            (float)obj["w"]
        //        );
        //    }

        //    public override bool CanConvert(Type objectType)
        //    {
        //        return objectType == typeof(Quaternion);
        //    }
        //}
        //#endregion
    }
}
