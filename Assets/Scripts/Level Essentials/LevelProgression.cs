using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgression : MonoBehaviour
{
    public int previousLevel = -1;
    public int thisLevel = -1;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        thisLevel = SceneManager.GetActiveScene().buildIndex;
    }

    private void OnLevelWasLoaded(int level)
    {
        if (thisLevel != -1)
        {
            previousLevel = thisLevel;
        }
        thisLevel = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
