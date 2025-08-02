using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region singleTon
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogWarning("more than one instance");
            }
            return instance;
        }
    }
    #endregion

    // <- this hold all the panels below (quest, ranking, reweards, inbox, notice)
    [SerializeField] private GameObject main_Panel;

    // the main panel is split into two part which the top part is holding all the every buttons and the lower part is holding all the functional buttons
    // when showing a half screen panel, this part should be disable
    [SerializeField] private GameObject main_Panel_Footer_Contents;

    //[SerializeField] private GameObject toggle_Buttons;

    [SerializeField] private GameObject all_Panels_Holder;

    [Header("Panels")]
    [SerializeField] private GameObject quest_Panel;
    [SerializeField] private GameObject ranking_Panel;
    [SerializeField] private GameObject rewards_Panel;
    [SerializeField] private GameObject inbox_Panel;
    [SerializeField] private GameObject notice_Panel;
    [SerializeField] private GameObject setting_Panel;
    [SerializeField] private GameObject rewardingArea_Panel;
    [SerializeField] private GameObject hatchingArea_Panel;
    [SerializeField] private GameObject miningArea_Panel;
    [SerializeField] private GameObject trainingArea_Panel;
    [SerializeField] private GameObject battleFieldArea_Panel;
    [SerializeField] private GameObject store_Panel;
    [SerializeField] private GameObject inventory_Panel;

    [Header("Panel Buttons")]
    [SerializeField] private Button quest_Button;
    [SerializeField] private Button ranking_Button;
    [SerializeField] private Button rewards_Button;
    [SerializeField] private Button inbox_Button;
    [SerializeField] private Button notice_Button;
    [SerializeField] private Button setting_Button;
    [SerializeField] private Button area_Rewarding_Button;
    [SerializeField] private Button area_Hatching_Button;
    [SerializeField] private Button area_Mining_Button;
    [SerializeField] private Button area_Training_Button;
    [SerializeField] private Button area_BattleField_Button;
    [SerializeField] private Button store_Button;
    [SerializeField] private Button inventory_Button;
    [SerializeField] private Button hatchmonInventory_Button;

    [Header("Panel Back Buttons")]
    [SerializeField] private Button quest_Back_Button;
    [SerializeField] private Button ranking_Back_Button;
    [SerializeField] private Button rewards_Back_Button;
    [SerializeField] private Button inbox_Back_Button;
    [SerializeField] private Button notice_Back_Button;
    [SerializeField] private Button setting_Back_Button;
    [SerializeField] private Button rewardingArea_Back_Button;
    [SerializeField] private Button hatching_Back_Button;
    [SerializeField] private Button mining_Back_Button;
    [SerializeField] private Button training_Back_Button;
    [SerializeField] private Button battfield_Back_Button;
    [SerializeField] private Button store_Back_Button;
    [SerializeField] private Button inventory_Back_Button;

    //[Header("Functions Buttons (Click for Camera Transition)")]
    //[SerializeField] private Button rewardingArea_Button;
    //[SerializeField] private Button hatchingArea_Button;
    //[SerializeField] private Button miningArea_Button;
    //[SerializeField] private Button trainingArea_Button;
    //[SerializeField] private Button battleFieldArea_Button;

    [Header("Poppu Windows)")]
    [SerializeField] private GameObject area_BattleField_StarLevelDownGrade_Poppu;
    [SerializeField] private GameObject area_BattleField_ResetStage_Poppu;
    [SerializeField] private GameObject area_BattleField_HatchmonInfo_Poppu;
    [SerializeField] private GameObject area_BattleField_StarUpgradeItemInfo_Poppu;
    [SerializeField] private GameObject area_BattleField_StageLocked_Poppu;
    [SerializeField] private GameObject area_BattleField_StartNewStage_Poppu;
    [SerializeField] private GameObject area_Hatching_HatchingItemInfo_Poppu;
    [SerializeField] private GameObject area_Hatching_ReceiveHatchmon_Poppu;

    [Header("Extra Functions)")]
    [SerializeField] private GameObject message_Panel;
    // use this to show a meesage animation to the user, for example if user can not do current action just show the message to him
    [SerializeField] private TextMeshPro user_DisplayMessage;

    public static Dictionary<EPoppuWindowType, GameObject> poppuWindows = new Dictionary<EPoppuWindowType, GameObject>();

    private Animator toggleButton_Aniamtor;
    private static readonly int slideAnim = Animator.StringToHash("SlideIn");
    private static IUIPanelState currentPanel;

    // getting an extra reference for the mainPanel to determine if need to enable/disable the content when switch state (open another ui)
    // for example if just opening an a small window then we should keep the vision of the main panel
    // but if opening a full screen panel, then we should close the main panel for performance.
    private static GameObject mainPanel;

    private static readonly Dictionary<UIPanelNameType, IUIPanelState> panels = new Dictionary<UIPanelNameType, IUIPanelState>();

#if UNITY_EDITOR
    [SerializeField] private static List<UIPanelNameType> viewer = new List<UIPanelNameType>();
#endif

    private float nextUpdateTime = 0f;
    private readonly float updatePeroid = 0.1f; // 1second / 0.1 = run 10 times in a second

    private void Awake()
    {
        instance = this;

        // must have
        if (mainPanel == null) mainPanel = GameObject.Find("Main_Panel");
        if (main_Panel_Footer_Contents == null) main_Panel_Footer_Contents = mainPanel.transform.Find("Footer").gameObject;
        //if (toggle_Buttons == null) toggle_Buttons = GameObject.Find("Toggle_Buttons");
        if (all_Panels_Holder == null) all_Panels_Holder = GameObject.Find("All_Panels_Holder");

        //toggleButton_Aniamtor = toggle_Buttons.GetComponent<Animator>();

        all_Panels_Holder.SetActive(true);

        InitPoppuWindows();

        InitPanelsTransition();

        InitButtonEvenets();

        // enter the main panel when game start
        EnterSateByNameType(UIPanelNameType.MAIN);
    }

    private void InitPoppuWindows()
    {
        // store and hide all poppu windows
        poppuWindows.Add(EPoppuWindowType.Area_BattleField_StarLevelDownGrade, area_BattleField_StarLevelDownGrade_Poppu);
        poppuWindows.Add(EPoppuWindowType.Area_BattleField_ResetStage, area_BattleField_ResetStage_Poppu);
        poppuWindows.Add(EPoppuWindowType.Area_BattleField_HatchmonInfo, area_BattleField_HatchmonInfo_Poppu);
        poppuWindows.Add(EPoppuWindowType.Area_BattleField_StarUpgradeItemInfo, area_BattleField_StarUpgradeItemInfo_Poppu);
        poppuWindows.Add(EPoppuWindowType.Area_BattleField_StageLocked, area_BattleField_StageLocked_Poppu);
        poppuWindows.Add(EPoppuWindowType.Area_BattleField_StartNewStage, area_BattleField_StartNewStage_Poppu);
        poppuWindows.Add(EPoppuWindowType.Area_Hatching_HatchingItemInfo, area_Hatching_HatchingItemInfo_Poppu);
        poppuWindows.Add(EPoppuWindowType.Area_Hatching_ReceiveHatchmon, area_Hatching_ReceiveHatchmon_Poppu);
        foreach (var window in poppuWindows)
        {
            window.Value.SetActive(false);
        }
    }

    private void InitPanelsTransition()
    {
        var mainPanel = new MainPanelUIState(main_Panel);
        var questPanel = new QuestPanelUI(quest_Panel);
        var rankingPanel = new RankingPanelUI(ranking_Panel);
        var rewardsPanel = new RewardsPanelUI(rewards_Panel);
        var inboxPanel = new InboxPanelUI(inbox_Panel);
        var noticePanel = new NoticePanelUI(notice_Panel);
        var settingPanel = new SettingPanelUI(setting_Panel);
        var rewardingAreaPanel = new RewardingAreaPanelUIState(rewardingArea_Panel);
        var hatchingAreaPanel = new HatchingAreaPanelUISate(hatchingArea_Panel);
        var miningAreaPanel = new MiningAreaPanelUIState(miningArea_Panel);
        var trainingAreaPanel = new TrainingAreaPanelUIState(trainingArea_Panel);
        var BattleFieldAreaPanel = new BattleFieldAreaPanelUIState(battleFieldArea_Panel);
        var storePanel = new StorePanelUI(store_Panel);
        var inventoryPanel = new InventoryPanelUIState(inventory_Panel, InventoryType.Item);
        var hatchmonInventoryPanel = new InventoryPanelUIState(inventory_Panel, InventoryType.Hatchmon);

        panels.Add(UIPanelNameType.MAIN, mainPanel);
        panels.Add(UIPanelNameType.QUEST, questPanel);
        panels.Add(UIPanelNameType.RANKING, rankingPanel);
        panels.Add(UIPanelNameType.REWARDS, rewardsPanel);
        panels.Add(UIPanelNameType.INBOX, inboxPanel);
        panels.Add(UIPanelNameType.NOTICE, noticePanel);
        panels.Add(UIPanelNameType.SETTING, settingPanel);
        panels.Add(UIPanelNameType.REWARDING, rewardingAreaPanel);
        panels.Add(UIPanelNameType.HATCHINGAREA, hatchingAreaPanel);
        panels.Add(UIPanelNameType.MININGAREA, miningAreaPanel);
        panels.Add(UIPanelNameType.TRAININGAREA, trainingAreaPanel);
        panels.Add(UIPanelNameType.BATTLEFIELDAREA, BattleFieldAreaPanel);
        panels.Add(UIPanelNameType.STORE, storePanel);
        panels.Add(UIPanelNameType.INVENTORY, inventoryPanel);

        quest_Button.onClick.AddListener(() => { SetState(questPanel); });
        ranking_Button.onClick.AddListener(() => { SetState(rankingPanel); });
        rewards_Button.onClick.AddListener(() => { SetState(rewardsPanel); });
        inbox_Button.onClick.AddListener(() => { SetState(inboxPanel); });
        notice_Button.onClick.AddListener(() => { SetState(noticePanel); });
        setting_Button.onClick.AddListener(() => { SetState(settingPanel); });
        //area_Rewarding_Button.onClick.AddListener(() => { SetState(rewardingAreaPanel); });
        //area_Hatching_Button.onClick.AddListener(() => { SetState(hatchingAreaPanel); });
        //area_Mining_Button.onClick.AddListener(() => { SetState(miningAreaPanel); });
        //area_Training_Button.onClick.AddListener(() => { SetState(trainingAreaPanel); });
        //area_BattleField_Button.onClick.AddListener(() => { SetState(BattleFieldAreaPanel); });
        store_Button.onClick.AddListener(() => { SetState(storePanel); });
        inventory_Button.onClick.AddListener(() => { SetState(inventoryPanel); });
        hatchmonInventory_Button.onClick.AddListener(() => { SetState(hatchmonInventoryPanel); });

        quest_Back_Button.onClick.AddListener(() => { ExitState(questPanel); });
        ranking_Back_Button.onClick.AddListener(() => { ExitState(rankingPanel); });
        rewards_Back_Button.onClick.AddListener(() => { ExitState(rewardsPanel); });
        inbox_Back_Button.onClick.AddListener(() => { ExitState(inboxPanel); });
        notice_Back_Button.onClick.AddListener(() => { ExitState(noticePanel); });
        setting_Back_Button.onClick.AddListener(() => { ExitState(settingPanel); });
        rewardingArea_Back_Button.onClick.AddListener(() => { ExitState(rewardingAreaPanel); });
        hatching_Back_Button.onClick.AddListener(() => { ExitState(hatchingAreaPanel); });
        mining_Back_Button.onClick.AddListener(() => { ExitState(miningAreaPanel); });
        training_Back_Button.onClick.AddListener(() => { ExitState(trainingAreaPanel); });
        battfield_Back_Button.onClick.AddListener(() => { ExitState(BattleFieldAreaPanel); });
        store_Back_Button.onClick.AddListener(() => { ExitState(storePanel); });
        inventory_Back_Button.onClick.AddListener(() => { ExitState(inventoryPanel); });
        inventory_Back_Button.onClick.AddListener(() => { ExitState(hatchmonInventoryPanel); });

        CloseAllPanels();
    }

    private void InitButtonEvenets()
    {
        //area_Rewarding_Button.onClick.AddListener(() => {
        //    area_Hatching_Button.gameObject.SetActive(false);
        //    area_Mining_Button.gameObject.SetActive(false);
        //    area_Training_Button.gameObject.SetActive(false);
        //    area_BattleField_Button.gameObject.SetActive(false);
        //    area_Rewarding_Button.gameObject.SetActive(true);
        //    //toggle_Buttons.SetActive(true);
        //    //toggleButton_Aniamtor.SetTrigger(slideAnim);
        //});

        //area_Hatching_Button.onClick.AddListener(() => {
        //    area_Rewarding_Button.gameObject.SetActive(false);
        //    area_Mining_Button.gameObject.SetActive(false);
        //    area_Training_Button.gameObject.SetActive(false);
        //    area_BattleField_Button.gameObject.SetActive(false);
        //    area_Hatching_Button.gameObject.SetActive(true);
        //    //toggle_Buttons.SetActive(true);
        //    //toggleButton_Aniamtor.SetTrigger(slideAnim);
        //});

        //area_Mining_Button.onClick.AddListener(() => {
        //    area_Rewarding_Button.gameObject.SetActive(false);
        //    area_Hatching_Button.gameObject.SetActive(false);
        //    area_Training_Button.gameObject.SetActive(false);
        //    area_BattleField_Button.gameObject.SetActive(false);
        //    area_Mining_Button.gameObject.SetActive(true);
        //    //toggle_Buttons.SetActive(true);
        //    //toggleButton_Aniamtor.SetTrigger(slideAnim);
        //});

        //area_Training_Button.onClick.AddListener(() => {
        //    area_Rewarding_Button.gameObject.SetActive(false);
        //    area_Hatching_Button.gameObject.SetActive(false);
        //    area_Mining_Button.gameObject.SetActive(false);
        //    area_BattleField_Button.gameObject.SetActive(false);
        //    area_Training_Button.gameObject.SetActive(true);
        //    //toggle_Buttons.SetActive(true);
        //    //toggleButton_Aniamtor.SetTrigger(slideAnim);
        //});

        //area_BattleField_Button.onClick.AddListener(() => {
        //    area_Rewarding_Button.gameObject.SetActive(false);
        //    area_Hatching_Button.gameObject.SetActive(false);
        //    area_Mining_Button.gameObject.SetActive(false);
        //    area_Training_Button.gameObject.SetActive(false);
        //    area_BattleField_Button.gameObject.SetActive(true);
        //    //toggle_Buttons.SetActive(true);
        //    //toggleButton_Aniamtor.SetTrigger(slideAnim);
        //});
    }

    //private void TurnOffAllTroggles()
    //{
    //    toggle_Buttons.SetActive(false);

    //    rewarding_Button.gameObject.SetActive(false);
    //    hatching_Button.gameObject.SetActive(false);
    //    mining_Button.gameObject.SetActive(false);
    //    training_Button.gameObject.SetActive(false);
    //    battleField_Button.gameObject.SetActive(false);
    //}

    private void Start()
    {

#if UNITY_EDITOR
        UpdateViewer();
#endif
    }

#if UNITY_EDITOR
    private void UpdateViewer()
    {
        viewer = panels.Keys.ToList();
    }
#endif

    private void Update()
    {
        //***FIXING should I limit the update times?
        //if (Time.time > nextUpdateTime)
        //{
            //nextUpdateTime = Time.time + updatePeroid;

            Tick();
        //}
    }

    private void CloseAllPanels()
    {
        // cloose all the panels
        foreach (var panel in panels)
        {
            panel.Value.OnExit();
        }
    }

    private void Tick()
    {
        if (currentPanel == null) return;

        currentPanel.Tick();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="panelType"></param>
    public void EnterSateByNameType(UIPanelNameType panelType)
    {
        IUIPanelState state = null;
        panels.TryGetValue(panelType, out state);

        if (state != null)
            EnterState(state);
        else
            Debug.LogError("No such panel type, please create one.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    private void EnterState(IUIPanelState state)
    {
        currentPanel = state;

        currentPanel.OnEnter();

        /// depending of the type of UI panel/ window, different acting should be taking
        /// for example when the full screen panel show, every UI behind should be disable to gain more performance
        switch (currentPanel.panelType)
        {
            case UIPanelType.MAINPANEL:
                HandleMainPanelUI();
                break;
            case UIPanelType.POPUWINDOW:
                HandlePopupWindowUI();
                break;
            case UIPanelType.HALFSCREENPANEL:
                HandleHalfScreenPanelUI();
                break;
            case UIPanelType.FULLSCREENPANEL:
                HandleFullScreenPanelUI();
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void ExitState(IUIPanelState state)
    {
        if (currentPanel == null) return;

        currentPanel.OnExit();
        EnterSateByNameType(UIPanelNameType.MAIN);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public void SetState(IUIPanelState state) 
    {
        //if (state == currentPanel)
        //    return;

        if(currentPanel != null)
            currentPanel.OnExit();

        EnterState(state);
    }

    private void HandleMainPanelUI()
    {
        //// make sure turn this off
        //if (toggle_Buttons.activeSelf)
        //    TurnOffAllTroggles();

        //// make sure this part is on
        //if (!toggle_Buttons.activeSelf)
        //    main_Panel_Footer_Contents.SetActive(true);

        Application.targetFrameRate = ApplicationManager.gameFrame;
    }

    private void HandlePopupWindowUI()
    {
        Application.targetFrameRate = ApplicationManager.gameFrame;
    }

    /// <summary>
    /// when showing a half screen ui, means the 3D scene should rendere normally on the background, then consider turn the rendere back to 60-fps 
    /// </summary>
    private void HandleHalfScreenPanelUI()
    {
        main_Panel_Footer_Contents.SetActive(false);

        Application.targetFrameRate = ApplicationManager.gameFrame;
    }

    /// <summary>
    /// Consider lowering the Application.targetFrameRate during a fullscreen UI, since you shouldn’t need to update at 60 fps.
    /// </summary>
    private void HandleFullScreenPanelUI()
    {
        // only display the full screen panel
        mainPanel.SetActive(false);

        Application.targetFrameRate = ApplicationManager.uiFrame; // <- ***FIXING need test
    }


    private void ShowUserMessage(string textContent)
    {
        message_Panel.SetActive(true);
        user_DisplayMessage.text = textContent;
    }

    public GameObject GetPopupWindowByName(EPoppuWindowType windowName)
    {
        poppuWindows.TryGetValue(windowName, out GameObject window);

        if (window == null)
            Debug.LogError("no poppu window found, please check the key name or you must create one");

        return window;
    }
}
