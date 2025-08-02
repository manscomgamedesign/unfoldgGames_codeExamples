using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class HatchmonInventoryPanel : MonoBehaviour
{

    [SerializeField] private GameObject slot_Prefab;
    [SerializeField] private GameObject itemSlot_Prefab; // <- even dragable but only can drag when on the standby setection

    [SerializeField] private SSkinUIData slotSkinData;

    [SerializeField] private Transform grid_Content_Group;

    [SerializeField] private ScrollRect m_ScrollRect;

    private List<Transform> allSlots = new();
    private int slot_MaxCount = 15; // <- use to pre create slots (at least how many should show on the screen even emtpy) on the grid and see if need extra slot when more data add in

    void Awake()
    {
        PreInitSlots();
    }

    private void OnEnable()
    {
        ClickableItem.OnPointerClickEvent += OnClickEvent;

        m_ScrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnDisable()
    {
        ClickableItem.OnPointerClickEvent -= OnClickEvent;
    }

    private void PreInitSlots()
    {
        for (int i = 0; i < slot_MaxCount; i++)
        {
            CreateSlot();
        }
        grid_Content_Group.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>();
    }

    private Transform CreateSlot()
    {
        GameObject slot = Instantiate(slot_Prefab, grid_Content_Group);
        slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // is will add the needed image component directly
        SkinUISlot skin = slot.AddComponent<SkinUISlot>();
        skin.enabled = true;
        skin.skinData = slotSkinData;

        // can be drop
        //slot.AddComponent<DropableItemUI>().enabled = false;

        // can be click
        slot.AddComponent<ClickableItem>().enabled = false;

        allSlots.Add(slot.transform);

        var uiObj = Instantiate(itemSlot_Prefab);
        uiObj.transform.SetParent(slot.transform);
        uiObj.transform.localPosition = Vector3.zero;
        uiObj.transform.localScale = Vector3.one;

        // can update display infos
        uiObj.AddComponent<HatchmonInfoUI>();

        // can be drag when need
        //uiObj.AddComponent<DragableItemUI>().enabled = false;

        slot.SetActive(false);

        return slot.transform;
    }

    /// <summary>
    /// the actual function showing all the eggs on the panel
    /// </summary>
    public void OpenPanel()
    {
        AssignSlot();

        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void AssignSlot()
    {
        // disable all first and reassign again
        foreach (var slot in allSlots)
            slot.gameObject.SetActive(false);

        var hatchmonInventoryData = HatchmonDataManager.Instance.Cache_InGame_HatchmonInventory;

        foreach (HatchmonData hatchmonData in hatchmonInventoryData.Values)
        {
            //var hatchmonData = hatchmon.hatchmonStatsData; // <- in-game data
            var hatchmonProperty = HatchmonDataManager.Instance.GetHatchmonPropertyByHatchmonId(hatchmonData.hatchmonId);

            var slot = GetSlot(); // <- assign to this emtpy slot

            var hatchmonUIObj = slot.GetChild(0);
            hatchmonUIObj.name = hatchmonData.hatchmonId; // <- override the name

            // assign the data on the ui
            hatchmonUIObj.GetComponent<HatchmonInfoUI>()
            .SetData(
                hatchmonData.hatchmonId,
                hatchmonProperty.grade,
                hatchmonProperty.creatureType,
                hatchmonData.inGame_Level,
                0, // <- means the star level will be hide
                hatchmonData.hatchId);


            // can be click
            slot.AddComponent<ClickableItem>().enabled = true;
            slot.gameObject.SetActive(true);
        }
    }

    private Transform GetSlot()
    {
        for (int i = 0; i < allSlots.Count; i++)
        {
            var slot = allSlots[i];

            if (!slot.gameObject.activeSelf)
                return slot;
        }

        // if need more space
        return CreateSlot();
    }

    private void OnClickEvent(PointerEventData eventData)
    {
        var clickedItem = eventData.pointerClick;

        // if find this component then it is a hatchmon
        if (clickedItem.transform.GetChild(0).GetComponent<HatchmonInfoUI>())
        {
            var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_BattleField_HatchmonInfo);
            window.SetActive(true);

            var hatchmonInfo = clickedItem.transform.GetChild(0).GetComponent<HatchmonInfoUI>();

            hatchmonInfo.GetCloneValue(out Sprite bg, out Sprite hatchmon
                , out Sprite type, out string levelText, out int starLevel);

            var slot = window.transform.Find("Body").GetChild(0);
            slot.GetChild(0).GetComponent<Image>().sprite = bg;
            slot.GetChild(1).GetComponent<Image>().sprite = hatchmon;
            slot.GetChild(2).GetComponent<TextMeshProUGUI>().text = levelText;
            slot.GetChild(3).GetComponent<Image>().sprite = type;
            slot.GetChild(4).gameObject.SetActive(false); // no star level to show ;
        }
    }
}
