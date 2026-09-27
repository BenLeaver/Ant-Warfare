using UnityEngine;

public class FrameWatchdog : MonoBehaviour
{
    float last;

    public int totalObjects;
    public int totalFood;
    public int totalUnits;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float now = Time.realtimeSinceStartup;
        float dt = now - last;

        if (dt > 0.5f)
        {
            totalObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Length;
            Debug.LogError($"Frame spike: {dt}, Time since startup: {now}, Objects: {totalObjects}");
        }
        last = now;
    }
}
