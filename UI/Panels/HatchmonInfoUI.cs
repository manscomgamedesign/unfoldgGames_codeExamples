using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// this slot is design to have the follow elements
/// index1.background color image
/// index2.hatchmon image
/// index3.hatchmon in-game level
/// index4.creature type iamge
/// index5.Star Level
/// </summary>
public class HatchmonInfoUI : MonoBehaviour
{
    private Image backgroundColor_Image;
    private Image hatchmon_Image;
    private Image creatureType_Image;
    private TextMeshProUGUI inGameLevel_Text;
    private Transform starLevel_Holder;

    private string hatchId;

    private readonly StringBuilder str = new();

    public string HatchId { get { return hatchId; } }

    private void Awake()
    {
        GetReference();
    }

    private void GetReference()
    {
        if (backgroundColor_Image == null) backgroundColor_Image = transform.Find("BG_Color").GetComponent<Image>();
        if (hatchmon_Image == null) hatchmon_Image = transform.Find("Hatchmon_Image").GetComponent<Image>();
        if (creatureType_Image == null) creatureType_Image = transform.Find("CreatureType_Image").GetComponent<Image>();
        if (inGameLevel_Text == null) inGameLevel_Text = transform.Find("inGameLevel_Text").GetComponent<TextMeshProUGUI>();
        if (starLevel_Holder == null) starLevel_Holder = transform.Find("StarLevel");
    }

    public void SetData(string hatchmonNameId, EHatchmonGradeType grade, CreatureType creatureType, int inGameLevel, int starLevel, string hatchId)
    {
        if (backgroundColor_Image == null)
            GetReference();

        // hatchmon grade image
        Sprite backgroundColorSprite = null;
        switch (grade)
        {
            case EHatchmonGradeType.NORMAL:
                backgroundColorSprite = SpriteManager.Instance.GetSprite("normalGrade");
                break;
            case EHatchmonGradeType.RARE:
                backgroundColorSprite = SpriteManager.Instance.GetSprite("rareGrade");
                break;
            case EHatchmonGradeType.EPIC:
                backgroundColorSprite = SpriteManager.Instance.GetSprite("epicGrade");
                break;
        }
        backgroundColor_Image.sprite = backgroundColorSprite;

        // createure type image
        Sprite creatureTypeSprite = null;
        switch (creatureType)
        {
            case CreatureType.DEVIL:
                creatureTypeSprite = SpriteManager.Instance.GetSprite("devil");
                break;
            case CreatureType.NATURE:
                creatureTypeSprite = SpriteManager.Instance.GetSprite("nature");
                break;
            case CreatureType.MACHINE:
                creatureTypeSprite = SpriteManager.Instance.GetSprite("machine");
                break;
        }
        creatureType_Image.sprite = creatureTypeSprite;

        // Hatachmon image
        hatchmon_Image.sprite = SpriteManager.Instance.GetSprite(hatchmonNameId);

        // in-game level
        str.Clear();
        str.Append("Lv");
        str.Append(inGameLevel);
        inGameLevel_Text.text = str.ToString();

        // star level
        UpdateStarUI(starLevel);

        this.hatchId = hatchId;
    }

    public void UpdateStarUI(int starLevel)
    {
        for (int i = 0; i < starLevel_Holder.childCount; i++)
        {
            starLevel_Holder.GetChild(i).gameObject.SetActive(false);
        }

        if (starLevel > 0)
            starLevel_Holder.GetChild(starLevel - 1).gameObject.SetActive(true);
    }

    /// <summary>
    /// use this when need to copy the same result on other slot
    /// </summary>
    public void GetCloneValue(out Sprite bg, out Sprite hatchmon, out Sprite type, out string levelText, out int starLevel)
    {
        bg = backgroundColor_Image.sprite;
        hatchmon = hatchmon_Image.sprite;
        type = creatureType_Image.sprite;
        levelText = inGameLevel_Text.text;
        starLevel = GetStarLevel();
    }

    /// <summary>
    /// when have the target data need to clone on the current
    /// </summary>
    public void CloneFromTargetUIValue(HatchmonInfoUI target)
    {
        target.GetCloneValue(out Sprite bg, out Sprite hatchmon
            , out Sprite type, out string levelText, out int starLevel);

        backgroundColor_Image.sprite = bg;
        hatchmon_Image.sprite = hatchmon;
        creatureType_Image.sprite = type;
        inGameLevel_Text.text = levelText;
        UpdateStarUI(starLevel);
    }

    /// <summary>
    /// find the hatchmons' star level base on the showing star level
    /// </summary>
    public int GetStarLevel()
    {
        for (int i = 0; i < starLevel_Holder.childCount; i++)
        {
            if (starLevel_Holder.GetChild(i).gameObject.activeSelf)
            {
                return (i + 1);
            }
        }
        return 0;
    }
}
