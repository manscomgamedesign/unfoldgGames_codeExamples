using UnityEngine;

public class MercenarySummonSlot : MonoBehaviour
{
    [System.Serializable]
    public struct MercenarySummonData
    {
        [SerializeField] private GameObject obj;
        [SerializeField] private string id;
        [SerializeField] private int level;

        public GameObject Obj { get { return obj; } set { obj = value; } }
        public string Id { get { return id; } set { id = value; } }
        public int Level { get { return level; } set { level = value; } }

        public MercenarySummonData(GameObject obj, string id, int level)
        {
            this.obj = obj;
            this.id = id;
            this.level = level;
        }
    }

    [SerializeField] private bool hasMercenary = false;
    [SerializeField] private MercenarySummonData mercenarySummonData;

    public bool HasMercenary { get { return hasMercenary; } set { hasMercenary = value; } }

    public void SetMercenarySummonData(GameObject obj, string id, int level)
    {
        mercenarySummonData.Obj = obj;
        mercenarySummonData.Id = id;
        mercenarySummonData.Level = level;
    }

    public string GetMercenaryId()
    {
        return mercenarySummonData.Id;
    }

    public int GetMercenaryStarLevel()
    {
        return mercenarySummonData.Level;
    }

    public void GetMercenaryData(out GameObject obj, out string id, out int level)
    {
        obj = mercenarySummonData.Obj;
        id = mercenarySummonData.Id;
        level = mercenarySummonData.Level;
    }

    /// <summary>
    /// when mercenary ui levelup, usually happen when two similar mercenary merge together to become a higher level
    /// </summary>
    public void LevelUp()
    {
        // check the lab to see if increase the grade-up percentage for this merge
        // for exmaple we have two lv1 mercenary, but if success then we get a level 3 mercenary, if not then just become level2 mercenary
        // During the preparation process at in-stage, when a player summons a mercenary, the basic percentage of that summoned mercenary will be Star 1 above (Star 2) is set at 0.5 %
        // So if a player unlocked level 1, then additioanl 1% will be added to the basic 0.5% so as a conclusion it becomes 1.5%. 
        float changeToUpgradeAHigherLevel = 0.5f; // <- base value follow from excel
        //changeToUpgradeAHigherLevel += StrategyLabDataManager.Instance.mercenarySynthesizingUpgradeIncrease; // <- check the Lab1 see if has the possbility of getting a higher level mercenary in this summon

        int precentage = System.Convert.ToInt32(Random.Range(changeToUpgradeAHigherLevel, 100));

        if (precentage <= changeToUpgradeAHigherLevel)
            mercenarySummonData.Level += 2; //higher level
        else
            mercenarySummonData.Level++;

        //mercenarySummonData.Obj.GetComponent<MercenaryStarLevelDisplay>().UpdateDisplayStarLevel(mercenarySummonData.Level);
    }

    public void AssignMercenaryFromSlot(GameObject obj, string id, int level)
    {
        hasMercenary = true;
        mercenarySummonData.Obj = obj;
        mercenarySummonData.Id = id;
        mercenarySummonData.Level = level;
    }

    /// <summary>
    /// switch back to an empty slot
    /// </summary>
    public void EmptyThisSLot()
    {
        hasMercenary = false;
        mercenarySummonData.Obj = null;
        mercenarySummonData.Id = null;
        mercenarySummonData.Level = 0;
    }
}
