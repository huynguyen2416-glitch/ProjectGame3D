using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // THÊM DÒNG NÀY: Để xử lý việc tải lại màn chơi khi bấm nút quay lại

public class PlayerState : MonoBehaviour
{
	public static PlayerState Instance { get; set; } // Giữ nguyên Singleton của bác

	// ---- Player Health ---- //
	public float currentHealth;
	public float maxHealth;

	// ---- Player Calories ---- //
	public float currentCalories;
	public float maxCalories;
	float distanceTravelled = 0; // Giữ nguyên biến đo quãng đường của bác
	Vector3 lastPosition;        // Giữ nguyên của bác
	public GameObject playerBody; // Giữ nguyên của bác

	// ---- Player Hydration ---- //
	public float currentHydrationPercent; // Giữ nguyên tên biến nước của bác
	public float maxHydrationPercent;     // Giữ nguyên của bác
	public bool isHydrationActive;        // Giữ nguyên của bác

	// ==== CÁC BIẾN ĐƯỢC THÊM MỚI ĐỂ PHỤC VỤ HỆ THỐNG CHẾT & HỒI SINH ==== //
	[Header("Giao diện UI khi chết")]
	public GameObject deathPanel; // Nơi bác kéo Panel "Bạn đã chết" vào ngoài Unity

	[Header("Cấu hình sinh tồn nâng cao")]
	public float starvationDamageRate = 1f; // Lượng máu bị trừ mỗi giây nếu hết sạch Calo hoặc Nước

	private bool isDead = false; // Cờ kiểm tra xem người chơi đã chết chưa

	private void Awake()
	{
		// Giữ nguyên logic Awake khởi tạo Instance của bác
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject); //
		}
		else
			Instance = this; //
	}

	private void Start()
	{
		currentHealth = maxHealth; //
		currentCalories = maxCalories; //
		currentHydrationPercent = maxHydrationPercent; //

		// Đảm bảo ẩn bảng chết lúc mới vào game
		if (deathPanel != null) deathPanel.SetActive(false);

		StartCoroutine(decreaseHydration()); // Khởi chạy Coroutine giảm nước của bác
	}

	// Giữ nguyên Coroutine đếm thời gian trừ nước mỗi 2 giây của bác
	IEnumerator decreaseHydration()
	{
		while (true)
		{
			if (!isDead) // Thêm điều kiện: Chỉ trừ nước khi còn sống
			{
				currentHydrationPercent -= 1; //
				if (currentHydrationPercent < 0) currentHydrationPercent = 0;
			}
			yield return new WaitForSeconds(2); //
		}
	}

	void Update()
	{
		if (isDead) return; // Nếu đã chết thì dừng mọi xử lý di chuyển hay tính toán chỉ số bên dưới

		// Giữ nguyên logic tính Calo dựa trên khoảng cách di chuyển của bác
		distanceTravelled += Vector3.Distance(playerBody.transform.position, lastPosition); //
		lastPosition = playerBody.transform.position; //

		if (distanceTravelled >= 5) //
		{
			distanceTravelled = 0; //
			currentCalories -= 1; //
			if (currentCalories < 0) currentCalories = 0;
		}

		// --- THÊM MỚI LOGIC: ĐÓI KHÁT QUÁ SẼ BỊ TRỪ MÁU ---
		// Nếu Calo chạm đáy HOẶC Nước chạm đáy (bằng 0) thì người chơi mất máu dần dần theo thời gian
		if (currentCalories <= 0 || currentHydrationPercent <= 0)
		{
			float damage = starvationDamageRate * Time.deltaTime;
			setHealth(currentHealth - damage);
		}

<<<<<<< HEAD
        // Nhánh Persona (Sinh tồn) có thể làm chậm tốc độ đốt calo qua calorieBurnRateReduction (vd 0.2 = -20%)
        float calorieBurnReduction = PersonaManager.Instance != null ? PersonaManager.Instance.calorieBurnRateReduction : 0f;
        float calorieDistanceThreshold = 5f / Mathf.Max(0.1f, 1f - calorieBurnReduction);

        if (distanceTravelled >= calorieDistanceThreshold) //
        {
            distanceTravelled = 0; //
            currentCalories -= 1; //
            if (currentCalories < 0) currentCalories = 0;
        }
=======
		// Nút N thần thánh để test tụt máu (Giữ nguyên của bác)
		if (Input.GetKeyDown(KeyCode.N)) //
		{
			setHealth(currentHealth - 10); //
		}
	}
>>>>>>> parent of a57ad63 (tạm thời như v)

	// NÂNG CẤP HÀM SET HEALTH: Để tự động kiểm tra xem khi nào máu về 0 và kích hoạt chết
	public void setHealth(float amount)
	{
		if (isDead) return;

		currentHealth = amount; //

<<<<<<< HEAD
        // Nút N thần thánh để test tụt máu
        if (Input.GetKeyDown(KeyCode.N)) //
        {
            setHealth(currentHealth - 10); //
        }
    }
    // NÂNG CẤP HÀM SET HEALTH: Tự động kiểm tra chết và chặn vượt giới hạn
    public void setHealth(float amount)
    {
        if (isDead) return;

        currentHealth = amount;

        // Giới hạn dưới: Không cho máu bị âm
        if (currentHealth < 0) currentHealth = 0;

        // Giới hạn trên: Không cho máu vượt qua giới hạn maxHealth
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // Nếu máu thực sự bằng 0 thì Kích hoạt chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Tối ưu hàm set Calo & Nước để tránh lỗi chỉ số hiển thị bị âm
    public void setCalories(float amount)
    {
        currentCalories = amount; 
        if (currentCalories < 0) currentCalories = 0;
    }

    public void setHydration(float amount)
    {
        currentHydrationPercent = amount; 
        if (currentHydrationPercent < 0) currentHydrationPercent = 0;
    }
=======
		// Giới hạn máu không bị âm xuống dưới 0
		if (currentHealth < 0) currentHealth = 0;

		// Nếu máu thực sự bằng 0 -> Kích hoạt hàm Chết ngay lập tức!
		if (currentHealth <= 0)
		{
			Die();
		}
	}

	// Tối ưu hàm set Calo & Nước để tránh lỗi chỉ số hiển thị bị âm
	public void setCalories(float amount)
	{
		currentCalories = amount; //
		if (currentCalories < 0) currentCalories = 0;
	}

	public void setHydration(float amount)
	{
		currentHydrationPercent = amount; //
		if (currentHydrationPercent < 0) currentHydrationPercent = 0;
	}

	// ================= HÀM XỬ LÝ KHI NGƯỜI CHƠI CHẾT ================= //
	void Die()
	{
		isDead = true;
		Debug.Log("Người chơi đã cạn kiệt sinh lực và chết!");

		// 1. Hiện giao diện bảng Báo Chết lên màn hình
		if (deathPanel != null)
		{
			deathPanel.SetActive(true);
		}

		// 2. Mở khóa và hiện con trỏ chuột để người chơi có thể rê chuột bấm nút "Quay lại"
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
>>>>>>> parent of a57ad63 (tạm thời như v)

		// 3. Mẹo nhỏ: Bác nên tắt script di chuyển của Player ở đây để cái xác không trượt đi lung tung nhé!
		// Ví dụ: GetComponent<YourMoveScript>().enabled = false;
	}

	// ================= HÀM XỬ LÝ KHI BẤM NÚT "QUAY LẠI / HỒI SINH" ================= //
	public void OnRespawnButtonClick()
	{
		// Tải lại chính cái Cảnh (Scene) hiện tại đang chơi để reset game sạch sẽ từ đầu
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
