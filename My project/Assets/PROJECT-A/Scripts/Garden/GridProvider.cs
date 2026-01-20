using UnityEngine;

public class GridProvider : MonoBehaviour
{
    public static GridProvider Singleton { get; private set; }
    public Grid Grid { get; private set; }

    void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        Grid = GetComponent<Grid>();
        DontDestroyOnLoad(gameObject);
    }
}
