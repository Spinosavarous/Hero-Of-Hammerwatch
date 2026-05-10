using UnityEngine;
using UnityEngine.Playables;

public class VillageScript : MonoBehaviour
{
    [SerializeField] private PlayableDirector tutorial;
    void Start()
    {
        if (!PlayerPrefs.HasKey("Tutorial"))
        {
            PlayerPrefs.SetInt("Tutorial", 1);
            tutorial.Play();
        }
    }
    
    void Update()
    {
        
    }
}
