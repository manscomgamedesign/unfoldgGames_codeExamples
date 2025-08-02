using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;

public class EggInventoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject slot_Prefab;
    [SerializeField] private GameObject itemSlot_Prefab; // <- even dragable but only can drag when on the standby setection

    [SerializeField] private SSkinUIData slotSkinData;

    [SerializeField] private Transform grid_Content_Group;

    [SerializeField] private ScrollRect m_ScrollRect;

    private List<Transform> allSlots = new();
    private int slot_InitCount = 30; // <- use to pre create slots (at least how many should show on the screen even emtpy) on the grid and see if need extra slot when more data add in
    private int newSlotsCount = 5; // <- when run out of slos, regenrate five extra slot

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
        for (int i = 0; i < slot_InitCount; i++)
        {
            CreateSlot();
        }
        grid_Content_Group.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>();
    }

    private void CreateSlot()
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
        uiObj.AddComponent<InventoryItemInfoUI>();
        uiObj.SetActive(false); // show the slot but not the item
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
            slot.GetChild(0).gameObject.SetActive(false);

        var eggInventoryData = HatchingAreaDataManager.Instance.Cache_Ingame_EggInventory;

        // found the egg that are not used yet which start time is 0
        // then count the egg with the same id and count the total amount
        var filteredData = eggInventoryData.Where(data => string.Equals(data.hatchingStartTime, "0")).ToList();
        var groupedData = filteredData
            .GroupBy(data => data.eggId)
            .Select(group => new {
                EggId = group.Key,
                TotalAmount = group.Count()
            })
            .ToList();

        foreach (var eggData in groupedData)
        {
            var slot = GetSlot(); // <- assign to this emtpy slot

            var uiObj = slot.GetChild(0);
            uiObj.name = eggData.EggId; // <- override the name

            // assign the data on the ui
            uiObj.GetComponent<InventoryItemInfoUI>()
                .SetData(
                   eggData.EggId,
                   eggData.TotalAmount);

            uiObj.gameObject.SetActive(true); // <- show the item

            // can be click
            slot.AddComponent<ClickableItem>().enabled = true;
        }
    }

    private Transform GetSlot()
    {
        for (int i = 0; i < allSlots.Count; i++)
        {
            var uiObj = allSlots[i].GetChild(0);

            if (!uiObj.gameObject.activeSelf)
                return uiObj.parent;
        }

        // if need more space, create 5 more slots
        for (int i = 0; i < newSlotsCount; i++)
        {
            CreateSlot();
        }

        return GetSlot();
    }

    private void OnClickEvent(PointerEventData eventData)
    {
        var clickedItem = eventData.pointerClick;

        var uiObj = clickedItem.transform.GetChild(0);

        // if find this component then it is a hatchmon
        if (uiObj.GetComponent<InventoryItemInfoUI>())
        {
            var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_Hatching_HatchingItemInfo);
            window.SetActive(true);

            // update the popu window ui ***FIXING (hard code to update it)
            var hathingItemUI = window.transform.Find("Body").GetChild(0).transform;
            hathingItemUI.GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(uiObj.name);
        }
    }
}
