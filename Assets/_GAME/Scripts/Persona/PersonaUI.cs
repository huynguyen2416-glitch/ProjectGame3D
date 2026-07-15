using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PersonaUI : MonoBehaviour
{
    [Header("Phím mở bảng")]
    public KeyCode toggleKey = KeyCode.P;

    [Header("Panel gốc")]
    public GameObject personaPanel;

    [Header("Trang Ghi chú mở đầu")]
    public GameObject introNotePanel;

    [Header("Khung Trang Sách (Luôn BẬT khi mở sách)")]
    public GameObject shelfGroup;

    [Header("UI Trang Phải: Luân phiên hiển thị")]
    [Tooltip("Kéo object UpgradeShelf (danh sách các nút nâng cấp) vào đây. Sẽ TẮT khi xem chi tiết.")]
    public GameObject upgradeShelfPanel;

    [Tooltip("Kéo object LevelDetailPanel vào đây. Sẽ BẬT lên khi chọn 1 node.")]
    public GameObject levelDetailPanel;

    [Header("Nội dung mặc định khác (Tuỳ chọn)")]
    public GameObject rightPageDefaultContent;

    [Header("Nút chuyển nhánh")]
    public Button lyTriTabButton;
    public Button sinhTonTabButton;
    public Button closeBookButton;

    [Header("Danh sách nâng cấp trong 1 nhánh (ScrollView Content)")]
    public Transform upgradeListContent;
    [Tooltip("Prefab 1 nút nâng cấp: cần có component Button, 1 Text (tên) ở component con, và tuỳ chọn 1 object tên 'Icon' chứa Image")]
    public GameObject upgradeNodeButtonPrefab;

    [Header("Chi tiết Level")]
    public Text detailTitleText;
    public Text detailDescriptionText;
    [Tooltip("Hiển thị hiệu ứng của Level hiện đang sở hữu (effectDescription của level đã mở khoá cao nhất)")]
    public Text effectText;
    public Image iconArtImage;
    public Transform levelRowContent;
    [Tooltip("Prefab 1 dòng level. Cần có object con tên 'LevelLabel', 'RequirementText', 'UnlockButton', 'EffectText'")]
    public GameObject levelRowPrefab;

    [Header("Nút quay lại danh sách")]
    [Tooltip("Bấm để đóng chi tiết Level, bật lại UpgradeShelf")]
    public Button backToShelfButton;

    [Header("Thanh Level tượng trưng (vd 2/5)")]
    public Image progressFillImage;
    public Text levelCounterText;

    [Header("Giải phóng chuột (QUAN TRỌNG ĐỂ NHẤN NÚT)")]
    [Tooltip("Kéo Script di chuyển / xoay Camera của Player vào đây (ví dụ: FirstPersonController, PlayerMovement...). Code sẽ tự động tắt script này khi mở sách để giải phóng chuột, giúp nút bấm click được bình thường.")]
    public MonoBehaviour playerController;

    private PersonaBranch currentBranch = PersonaBranch.SinhTon;
    private PersonaUpgradeSO currentSelectedUpgrade;

    private readonly List<GameObject> spawnedNodeButtons = new List<GameObject>();
    private readonly List<GameObject> spawnedLevelRows = new List<GameObject>();

    private float refreshTimer = 0f;

    private void Start()
    {
        if (personaPanel != null) personaPanel.SetActive(false);
        if (levelDetailPanel != null) levelDetailPanel.SetActive(false);

        if (lyTriTabButton != null) lyTriTabButton.onClick.AddListener(() => ShowBranch(PersonaBranch.LyTri));
        if (sinhTonTabButton != null) sinhTonTabButton.onClick.AddListener(() => ShowBranch(PersonaBranch.SinhTon));
        if (closeBookButton != null) closeBookButton.onClick.AddListener(ClosePanel);
        if (backToShelfButton != null) backToShelfButton.onClick.AddListener(CloseLevelDetail);
    }

    private void CloseLevelDetail()
    {
        if (levelDetailPanel != null) levelDetailPanel.SetActive(false);
        if (upgradeShelfPanel != null) upgradeShelfPanel.SetActive(true);
        if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(true);
        currentSelectedUpgrade = null;
    }

    private void ClosePanel()
    {
        if (personaPanel != null) personaPanel.SetActive(false);

        // Khóa chuột lại khi đóng bảng
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Bật lại điều khiển nhân vật
        if (playerController != null) playerController.enabled = true;
    }

    void Update()
    {
        // Vẫn giữ phím P phòng khi bác muốn test song song
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePersonaPanel();
        }

        bool detailIsOpen = levelDetailPanel != null && levelDetailPanel.activeSelf;
        if (detailIsOpen && currentSelectedUpgrade != null)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= 0.5f)
            {
                RefreshLevelRows(currentSelectedUpgrade);
                RefreshLevelProgressBar(currentSelectedUpgrade);
                RefreshEffectText(currentSelectedUpgrade);
                refreshTimer = 0f;
            }
        }
    }

    // LUỒNG ĐỒNG BỘ: Hàm public để Button bên ngoài Canvas gọi tới
    public void TogglePersonaPanel()
    {
        if (personaPanel == null) return;

        bool willOpen = !personaPanel.activeSelf;
        personaPanel.SetActive(willOpen);

        // Xử lý trạng thái chuột dứt điểm
        Cursor.lockState = willOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = willOpen;

        // Đóng/Mở script nhân vật để không bị tranh chấp quyền điều khiển chuột
        if (playerController != null)
        {
            playerController.enabled = !willOpen;
        }

        // Khởi tạo trạng thái ruột trang sách khi mở
        if (willOpen)
        {
            if (shelfGroup != null) shelfGroup.SetActive(true);
            if (levelDetailPanel != null) levelDetailPanel.SetActive(false);
            if (upgradeShelfPanel != null) upgradeShelfPanel.SetActive(false);

            if (introNotePanel != null) introNotePanel.SetActive(true);
            if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(false);

            foreach (var go in spawnedNodeButtons) Destroy(go);
            spawnedNodeButtons.Clear();

            currentSelectedUpgrade = null;

        }
    }

    public void ShowBranch(PersonaBranch branch)
    {
        currentBranch = branch;

        if (shelfGroup != null) shelfGroup.SetActive(true);
        if (levelDetailPanel != null) levelDetailPanel.SetActive(false);
        if (introNotePanel != null) introNotePanel.SetActive(false);
        if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(true);

        if (upgradeShelfPanel != null) upgradeShelfPanel.SetActive(true);

        foreach (var go in spawnedNodeButtons) Destroy(go);
        spawnedNodeButtons.Clear();

        if (PersonaManager.Instance == null || upgradeListContent == null || upgradeNodeButtonPrefab == null) return;

        foreach (var upgrade in PersonaManager.Instance.allUpgrades)
        {
            if (upgrade == null || upgrade.branch != branch) continue;

            GameObject nodeGO = Instantiate(upgradeNodeButtonPrefab, upgradeListContent);
            spawnedNodeButtons.Add(nodeGO);

            Text label = nodeGO.GetComponentInChildren<Text>();
            if (label != null)
            {
                int lvl = PersonaManager.Instance.GetCurrentLevel(upgrade);
                label.text = $"{upgrade.upgradeName} (Lv {lvl}/{upgrade.levels.Count})";
            }

            Transform iconTransform = nodeGO.transform.Find("Icon");
            if (iconTransform != null && upgrade.icon != null)
            {
                Image icon = iconTransform.GetComponent<Image>();
                if (icon != null) icon.sprite = upgrade.icon;
            }

            Button btn = nodeGO.GetComponent<Button>();
            if (btn != null)
            {
                PersonaUpgradeSO capturedUpgrade = upgrade;
                btn.onClick.AddListener(() => OpenLevelDetail(capturedUpgrade));
            }
        }
    }

    private void OpenLevelDetail(PersonaUpgradeSO upgrade)
    {
        currentSelectedUpgrade = upgrade;

        if (introNotePanel != null) introNotePanel.SetActive(false);
        if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(false);

        if (upgradeShelfPanel != null) upgradeShelfPanel.SetActive(false);
        if (levelDetailPanel != null) levelDetailPanel.SetActive(true);

        if (detailTitleText != null) detailTitleText.text = upgrade.upgradeName;
        if (detailDescriptionText != null) detailDescriptionText.text = upgrade.description;
        if (iconArtImage != null && upgrade.icon != null) iconArtImage.sprite = upgrade.icon;

        foreach (var go in spawnedLevelRows) Destroy(go);
        spawnedLevelRows.Clear();

        RefreshLevelRows(upgrade);
        RefreshLevelProgressBar(upgrade);
        RefreshEffectText(upgrade);
    }

    private void RefreshEffectText(PersonaUpgradeSO upgrade)
    {
        if (effectText == null || PersonaManager.Instance == null) return;

        int currentLevel = PersonaManager.Instance.GetCurrentLevel(upgrade);

        if (currentLevel <= 0)
        {
            effectText.text = "Chưa mở khoá hiệu ứng nào.";
            return;
        }

        PersonaLevelData currentLevelData = upgrade.levels.Find(l => l.level == currentLevel);
        effectText.text = currentLevelData != null
            ? $"Hiệu ứng hiện tại: {currentLevelData.effectDescription}"
            : "";
    }

    private void RefreshLevelProgressBar(PersonaUpgradeSO upgrade)
    {
        int currentLevel = PersonaManager.Instance.GetCurrentLevel(upgrade);
        int maxLevel = upgrade.levels.Count;

        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = maxLevel > 0 ? (float)currentLevel / maxLevel : 0f;
        }

        if (levelCounterText != null)
        {
            levelCounterText.text = $"{currentLevel}/{maxLevel}";
        }
    }

    private void RefreshLevelRows(PersonaUpgradeSO upgrade)
    {
        if (levelRowContent == null || levelRowPrefab == null) return;

        if (spawnedLevelRows.Count != upgrade.levels.Count)
        {
            foreach (var go in spawnedLevelRows) Destroy(go);
            spawnedLevelRows.Clear();

            foreach (var _ in upgrade.levels)
            {
                spawnedLevelRows.Add(Instantiate(levelRowPrefab, levelRowContent));
            }
        }

        int currentLevel = PersonaManager.Instance.GetCurrentLevel(upgrade);

        for (int i = 0; i < upgrade.levels.Count; i++)
        {
            PersonaLevelData levelData = upgrade.levels[i];
            GameObject row = spawnedLevelRows[i];

            Text levelLabel = row.transform.Find("LevelLabel")?.GetComponent<Text>();
            Text reqText = row.transform.Find("RequirementText")?.GetComponent<Text>();
            Button unlockBtn = row.transform.Find("UnlockButton")?.GetComponent<Button>();
            Text rowEffectText = row.transform.Find("EffectText")?.GetComponent<Text>();

            bool isUnlocked = levelData.level <= currentLevel;
            bool isNextLevel = levelData.level == currentLevel + 1;

            if (levelLabel != null)
            {
                levelLabel.text = $"Level {levelData.level}" + (isUnlocked ? " (Đã mở)" : "");
            }

            if (rowEffectText != null)
            {
                rowEffectText.text = levelData.effectDescription;
                rowEffectText.color = isUnlocked ? Color.white : Color.grey;
            }
            else
            {
                Debug.LogWarning($"[PersonaUI] KHÔNG tìm thấy child 'EffectText' trong row Level {levelData.level}");
            }

            if (reqText != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var req in levelData.requirements)
                {
                    int have = CountItemInInventory(req.itemName);
                    string vnName = ItemNameVN.Get(req.itemName);
                    sb.AppendLine($"{vnName}: {have}/{req.amount}");
                }
                reqText.text = sb.ToString();
                reqText.color = isUnlocked
                    ? Color.grey
                    : (isNextLevel && PersonaManager.Instance.CanUnlockNextLevel(upgrade) ? Color.green : Color.red);
            }

            if (unlockBtn != null)
            {
                unlockBtn.gameObject.SetActive(isNextLevel);
                unlockBtn.interactable = isNextLevel && PersonaManager.Instance.CanUnlockNextLevel(upgrade);

                unlockBtn.onClick.RemoveAllListeners();
                PersonaUpgradeSO capturedUpgrade = upgrade;
                unlockBtn.onClick.AddListener(() =>
                {
                    if (PersonaManager.Instance.TryUnlockNextLevel(capturedUpgrade))
                    {
                        if (SoundManager.Instance != null)
                            SoundManager.Instance.PlaySound(SoundManager.Instance.personaSound);

                        RefreshLevelRows(capturedUpgrade);
                        RefreshLevelProgressBar(capturedUpgrade);
                        RefreshEffectText(capturedUpgrade);
                    }
                    else
                    {
                        Debug.LogWarning("[PersonaUI] TryUnlockNextLevel trả về false.");
                    }
                });
            }
        }
    }

    private int CountItemInInventory(string itemName)
    {
        if (InventorySystem.Instance == null) return 0;
        int count = 0;
        foreach (string item in InventorySystem.Instance.itemList)
        {
            if (item == itemName) count++;
        }
        return count;
    }
}