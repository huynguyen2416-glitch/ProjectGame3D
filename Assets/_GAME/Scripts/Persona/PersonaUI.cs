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
    [Header("Nội dung mặc định của Trang Phải")]
    public GameObject rightPageDefaultContent;
    [Header("Nút chuyển nhánh")]
    public Button lyTriTabButton;
    public Button sinhTonTabButton;
    public Button closeBookButton;

    [Header("Danh sách nâng cấp trong 1 nhánh (ScrollView Content, thường ở Trang Trái)")]
    public Transform upgradeListContent;
    [Tooltip("Prefab 1 nút nâng cấp: cần có component Button, 1 Text (tên) ở component con, và tuỳ chọn 1 object tên 'Icon' chứa Image")]
    public GameObject upgradeNodeButtonPrefab;

    [Header("Shelf (Tab bar + danh sách node ở Trang Trái) - ĐÓNG lại khi đang xem chi tiết 1 Level")]
    [Tooltip("Kéo object cha gom RibbonTabBar + LeftPage (danh sách node) vào đây. Ẩn đi khi mở Level Detail, hiện lại khi bấm Back hoặc chọn nhánh mới")]
    public GameObject shelfGroup;

    [Header("Bảng chi tiết Level (ĐẶT NGAY TRONG RIGHT PAGE, cùng cấp với Right Page Default Content)")]
    [Tooltip("Panel này giờ hiển thị NGAY BÊN TRONG Right Page, thay thế Right Page Default Content khi đang chọn 1 nâng cấp - không còn là trang riêng toàn màn hình nữa")]
    public GameObject levelDetailPanel;
    public Text detailTitleText;
    public Text detailDescriptionText;
    [Tooltip("Hiển thị hiệu ứng của Level hiện đang sở hữu (effectDescription của level đã mở khoá cao nhất)")]
    public Text effectText;
    public Image iconArtImage;
    public Transform levelRowContent;
    [Tooltip("Prefab 1 dòng level - NÊN style giống hệt upgradeNodeButtonPrefab (cùng màu nền/khung/font) để đồng bộ giao diện. Cần có object con tên 'LevelLabel' (Text), 'RequirementText' (Text), 'UnlockButton' (Button)")]
    public GameObject levelRowPrefab;

    [Header("Nút quay lại Right Page mặc định")]
    [Tooltip("Bấm để đóng chi tiết Level, quay về nội dung mặc định của Right Page (không đóng cả cuốn sách)")]
    public Button backToShelfButton;

    [Header("Thanh Level tượng trưng (vd 2/5)")]
    [Tooltip("Image kiểu Filled (Fill Method = Horizontal) — đặt trên object ProgressFillImage")]
    public Image progressFillImage;
    [Tooltip("Text hiển thị dạng 'currentLevel/maxLevel', vd '2/5'")]
    public Text levelCounterText;

    private PersonaBranch currentBranch = PersonaBranch.SinhTon;
    private PersonaUpgradeSO currentSelectedUpgrade;

    private readonly List<GameObject> spawnedNodeButtons = new List<GameObject>();
    private readonly List<GameObject> spawnedLevelRows = new List<GameObject>();

    private void Start()
    {
        if (personaPanel != null) personaPanel.SetActive(false);
        if (levelDetailPanel != null) levelDetailPanel.SetActive(false);

        if (lyTriTabButton != null) lyTriTabButton.onClick.AddListener(() => ShowBranch(PersonaBranch.LyTri));
        if (sinhTonTabButton != null) sinhTonTabButton.onClick.AddListener(() => ShowBranch(PersonaBranch.SinhTon));
        if (closeBookButton != null) closeBookButton.onClick.AddListener(ClosePanel);
        if (backToShelfButton != null) backToShelfButton.onClick.AddListener(CloseLevelDetail);
    }

    // Đóng phần chi tiết Level, quay lại nội dung mặc định của Right Page, MỞ LẠI shelf (tab bar +
    // danh sách node ở Trang Trái). KHÔNG đóng cả cuốn sách.
    private void CloseLevelDetail()
    {
        if (levelDetailPanel != null) levelDetailPanel.SetActive(false);
        if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(true);
        if (shelfGroup != null) shelfGroup.SetActive(true);
        currentSelectedUpgrade = null;
    }

    private void ClosePanel()
    {
        if (personaPanel != null) personaPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            bool willOpen = personaPanel != null && !personaPanel.activeSelf;
            if (personaPanel != null) personaPanel.SetActive(willOpen);

            Cursor.lockState = willOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = willOpen;

            //Khi vừa mở sách, đưa mọi thứ về trạng thái trống trơn mặc định
            if (willOpen)
            {
                if (levelDetailPanel != null) levelDetailPanel.SetActive(false);
                if (shelfGroup != null) shelfGroup.SetActive(true);

                // Bật ghi chú lên, TẮT nội dung cũ đi
                if (introNotePanel != null) introNotePanel.SetActive(true);
                if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(false);

                foreach (var go in spawnedNodeButtons) Destroy(go);
                spawnedNodeButtons.Clear();

                currentSelectedUpgrade = null;
            }
        }

        // Chỉ chạy trình cập nhật thời gian thực khi người chơi đã chọn một nâng cấp cụ thể
        bool detailIsOpen = levelDetailPanel != null && levelDetailPanel.activeSelf;
        if (detailIsOpen && currentSelectedUpgrade != null)
        {
            RefreshLevelRows(currentSelectedUpgrade);
            RefreshLevelProgressBar(currentSelectedUpgrade);
            RefreshEffectText(currentSelectedUpgrade);
        }
    }

    public void ShowBranch(PersonaBranch branch)
    {
        currentBranch = branch;
        if (levelDetailPanel != null) levelDetailPanel.SetActive(false);
        if (introNotePanel != null) introNotePanel.SetActive(false);
        if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(true);
        if (shelfGroup != null) shelfGroup.SetActive(true);
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
                PersonaUpgradeSO capturedUpgrade = upgrade; // tránh lỗi tham chiếu vòng lặp closure
                btn.onClick.AddListener(() => OpenLevelDetail(capturedUpgrade));
            }
        }
    }

    // Mở chi tiết Level NGAY TRONG Right Page: ẩn Right Page Default Content, hiện Level Detail Panel,
    // ĐÓNG shelf (tab bar + danh sách node ở Trang Trái) lại để tập trung vào chi tiết.
    private void OpenLevelDetail(PersonaUpgradeSO upgrade)
    {
        currentSelectedUpgrade = upgrade;

        if (introNotePanel != null) introNotePanel.SetActive(false);
        if (rightPageDefaultContent != null) rightPageDefaultContent.SetActive(false);
        if (shelfGroup != null) shelfGroup.SetActive(false);
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

    // Hiển thị hiệu ứng của level ĐÃ MỞ KHOÁ cao nhất hiện tại (level 0 = chưa có hiệu ứng nào)
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

    // Cập nhật thanh level tượng trưng (fillAmount) và chữ "currentLevel/maxLevel"
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

        // Chỉ dựng lại danh sách dòng nếu số dòng chưa khớp, tránh Instantiate lại mỗi frame
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

            // Mỗi dòng tự hiển thị hiệu ứng CỦA RIÊNG level đó (không phải chỉ level hiện tại)
            if (rowEffectText != null)
            {
                rowEffectText.text = levelData.effectDescription;
                rowEffectText.color = isUnlocked ? Color.white : Color.grey;
            }
            else
            {
                // LOG TẠM THỜI ĐỂ DÒ LỖI - xoá dòng này sau khi tìm ra nguyên nhân
                Debug.LogWarning($"[PersonaUI] KHÔNG tìm thấy child 'EffectText' trong row Level {levelData.level} (prefab gốc: '{row.name}'). Kiểm tra lại tên GameObject/vị trí lồng trong LevelRowPrefab.");
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
                            SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound);

                        ShowBranch(currentBranch);
                        OpenLevelDetail(capturedUpgrade);
                    }
                    else
                    {
                        Debug.LogWarning("[PersonaUI] TryUnlockNextLevel trả về false — thiếu nguyên liệu hoặc đã max level.");
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