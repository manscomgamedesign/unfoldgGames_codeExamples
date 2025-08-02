using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatchmonStarLevelUpgradeItemUI : MonoBehaviour
{
    [SerializeField] private Transform upgradeLevel_Holder;
    [SerializeField] private Transform[] allLevels;

    private void Awake()
    {
        if (upgradeLevel_Holder == null) upgradeLevel_Holder = transform.Find("UpgradeLevel");
        allLevels = new Transform[upgradeLevel_Holder.childCount];

        for (int i = 0; i < upgradeLevel_Holder.childCount; i++)
        {
            allLevels[i] = upgradeLevel_Holder.GetChild(i);
        }
    }

    public void UpdateLevelUI(int whatLevel)
    {
        for (int i = 0; i < allLevels.Length; i++)
        {
            allLevels[i].gameObject.SetActive(false);
        }

        allLevels[whatLevel - 1].gameObject.SetActive(true);
    }

    public int GetLevel()
    {
        for (int i = 0; i < allLevels.Length; i++)
        {
            if (allLevels[i].gameObject.activeSelf)
                return i;
        }

        return -1;
    }
}
