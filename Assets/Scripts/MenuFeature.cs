using LearnXR.Core.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MenuFeature : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private OVRInput.Button buttonForMenuActivation;
    [SerializeField] private GameObject levelerPrefab;

    private List<GameObject> levelersAdded = new();

    private void Awake() => menu.SetActive(false); 

    public void AddLeveler()
    {
        Debug.Log("Begin AddLeveler()");
        var newLeveler = Instantiate(levelerPrefab);
        Debug.Log("newLeveler: " + newLeveler.GetHashCode());
        CommonUtilities.Instance.PlaceObjectInFrontOfCamera(newLeveler.transform);
        levelersAdded.Add(newLeveler);
        Debug.Log("End AddLeveler(): " + levelersAdded.Count());
    }
     
    public void DeleteAll()
    {
        Debug.Log("Begin DeleteAll() " + levelersAdded.Count());
        foreach (var leveler in levelersAdded)
        {
            Debug.Log("deleting leveler: " + leveler.GetHashCode());
            Destroy(leveler);
            Debug.Log("deleted leveler: " + leveler.GetHashCode());
        }
        MeasureTapeFeature.Instance.ClearTapeLines();
        Debug.Log("End DeleteAll()");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(buttonForMenuActivation))
        {
            menu.SetActive(!menu.activeSelf);
        }
    }
}
