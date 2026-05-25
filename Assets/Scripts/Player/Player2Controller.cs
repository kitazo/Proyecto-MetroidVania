using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class Player2Controller : NetworkBehaviour
{
    [Header("Ajustes de Velocidad P2")]
    public float velocidadNormal = 15f; 
    public float velocidadRapida = 40f; 

    [Header("Trampas disponibles")]
    public GameObject[] trapPrefabs;
    public LayerMask placementLayer;
    public int trapBudget = 4;
    public float trapSpawnYOffset = 0f; 

    [Header("Posesión de enemigos")]
    public float possessionRange = 3f;
    public LayerMask enemyLayer;

    [Header("Cooldowns P2 (Saboteador)")]
    public float trapCooldownDuration = 10f;
    public float possessionCooldownDuration = 10f;
    private float currentTrapCooldown = 0f;
    private float currentPossessionCooldown = 0f;

    [Header("UI Propia")]
    public TextMeshProUGUI budgetText;
    public GameObject placementGhost;

    [Header("UI del Player 1 (Lectura)")]
    public Image p1HealthBar;         
    public Image p1DashCooldownBar;   

    public bool isP2Active = false;

    public NetworkVariable<int> networkTrapsPlaced = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private int selectedTrap = 0;
    private EnemyBase possessedEnemy = null;
    private Camera cam;
    private PlayerControllerComplete player1Target; 

    public override void OnNetworkSpawn()
    {
        networkTrapsPlaced.OnValueChanged += OnTrapsChanged;
        
        if (IsOwner)
        {
            isP2Active = true;
            VincularCamara(); 
            ApagarMinimapa();

            if (budgetText == null)
            {
                GameObject obj = GameObject.Find("BudgetText");
                
                if (obj != null) 
                {
                    budgetText = obj.GetComponent<TextMeshProUGUI>();
                }
            }

            if (placementGhost != null) 
            {
                placementGhost.SetActive(false);
            }
        }
        
        UpdateUI();
    }

    public override void OnNetworkDespawn() 
    {
        //Desvincula la cámara antes de que este objeto sea destruido.
        //Sin esto, Camera.main se destruye junto con el God Player al recargar
        //la escena, causando un crash de motor sin mensaje de error.
        DesvinculaCamara();

        //Limpia referencias a objetos de escena para que en Ronda 2
        //se busquen de nuevo en lugar de usar punteros muertos.
        possessedEnemy  = null;
        player1Target   = null;
        p1HealthBar     = null;
        p1DashCooldownBar = null;
        budgetText      = null;

        networkTrapsPlaced.OnValueChanged -= OnTrapsChanged; 
    }

    public override void OnDestroy()
    {
        DesvinculaCamara();
    }

    //Separa Camera.main de este transform para que no se destruya con él.
    //También nulleamos cam para que VincularCamara() la rebusque correctamente
    //en Ronda 2 cuando se spawnee un nuevo God prefab.
    private void DesvinculaCamara()
    {
        if (cam != null && cam.transform.parent == this.transform)
        {
            cam.transform.SetParent(null);
        }
        cam = null;
    }

    private void OnTrapsChanged(int oldValue, int newValue) 
    { 
        UpdateUI(); 
    }

    private void ApagarMinimapa()
    {
        MinimapController minimap = FindAnyObjectByType<MinimapController>();
        
        if (minimap != null)
        {
            minimap.gameObject.SetActive(false);
        }
    }

    private void VincularCamara()
    {
        if (cam == null) 
        {
            cam = Camera.main;
        }

        if (cam != null)
        {
            MonoBehaviour camFollow = (MonoBehaviour)cam.GetComponent("CameraFollow");
            
            if (camFollow != null) 
            {
                camFollow.enabled = false;
            }

            cam.transform.SetParent(this.transform);
            cam.transform.localPosition = new Vector3(0, 0, -10f); 
        }
    }

    void Update()
    {
        if (!IsSpawned || !isP2Active || !IsOwner || Time.timeScale == 0f) 
        {
            return;
        }
        
        if (currentTrapCooldown > 0f) 
        {
            currentTrapCooldown -= Time.deltaTime;
        }

        if (currentPossessionCooldown > 0f) 
        {
            currentPossessionCooldown -= Time.deltaTime;
        }

        HandleMovementAndCamera(); 
        
        HandleEnemyPossession();
        HandleTrapSelection();
        UpdateP1UI(); 
        
        if (currentTrapCooldown > 0f || currentPossessionCooldown > 0f) 
        {
            UpdateUI();
        }
    }

    void HandleMovementAndCamera()
    {
        if (possessedEnemy != null)
        {
            Vector3 targetPos = possessedEnemy.transform.position;
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        }
        else
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical"); 
            float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadRapida : velocidadNormal;

            Vector3 moveDir = new Vector3(moveX, moveY, 0).normalized;
            transform.position += moveDir * velocidadActual * Time.deltaTime;
        }
    }

    void HandleTrapPlacement()
    {
        //Función desactivada temporalmente.
    }

    [ServerRpc]
    public void PlaceTrapServerRpc(int trapIndex, Vector3 spawnPosition)
    {
        if (networkTrapsPlaced.Value >= trapBudget) 
        {
            return;
        }

        if (trapIndex < 0 || trapIndex >= trapPrefabs.Length) 
        {
            return;
        }

        GameObject trapInstance = Instantiate(trapPrefabs[trapIndex], spawnPosition, Quaternion.identity);
        NetworkObject netObj = trapInstance.GetComponent<NetworkObject>();

        if (netObj != null) 
        { 
            netObj.Spawn(); 
            networkTrapsPlaced.Value++; 
        }
    }

    void HandleEnemyPossession()
    {
        if (possessedEnemy != null)
        {
            if (possessedEnemy.isDead) 
            { 
                possessedEnemy = null; 
                currentPossessionCooldown = possessionCooldownDuration; 
                return; 
            }

            float h = Input.GetAxisRaw("Horizontal");
            
            if (IsServer)
            {
                possessedEnemy.MoveAsPossessed(h);
            }
            else
            {
                MovePossessedEnemyServerRpc(h);
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)) 
            {
                AttackPossessedEnemyServerRpc();
            }

            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                SpecialActionPossessedEnemyServerRpc();
            }

            if (Input.GetMouseButtonDown(1)) 
            { 
                UnpossessEnemyServerRpc();
                possessedEnemy = null; 
                currentPossessionCooldown = possessionCooldownDuration; 
            }
            
            return; 
        }

        if (Input.GetMouseButtonDown(1) && currentPossessionCooldown <= 0f)
        {
            if (cam == null) 
            {
                return;
            }

            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Collider2D hit = Physics2D.OverlapCircle(mouseWorld, possessionRange, enemyLayer);
            
            if (hit != null)
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                
                if (enemy != null && !enemy.isDead) 
                { 
                    possessedEnemy = enemy; 
                    PossessEnemyServerRpc(enemy.NetworkObjectId);
                }
            }
        }
    }

    [ServerRpc]
    public void MovePossessedEnemyServerRpc(float horizontal)
    {
        if (possessedEnemy != null && possessedEnemy.networkIsPossessed.Value)
        {
            possessedEnemy.MoveAsPossessed(horizontal);
        }
    }

    [ServerRpc]
    public void AttackPossessedEnemyServerRpc()
    {
        if (possessedEnemy != null && possessedEnemy.networkIsPossessed.Value)
        {
            possessedEnemy.AttackAsPossessed();
        }
    }

    [ServerRpc]
    public void SpecialActionPossessedEnemyServerRpc()
    {
        if (possessedEnemy != null && possessedEnemy.networkIsPossessed.Value)
        {
            possessedEnemy.SpecialActionAsPossessed();
        }
    }

    [ServerRpc]
    public void UnpossessEnemyServerRpc()
    {
        if (possessedEnemy != null)
        {
            possessedEnemy.SetPossessed(false);
        }
    }

    [ServerRpc]
    public void PossessEnemyServerRpc(ulong enemyNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetworkObjectId, out NetworkObject netObj))
        {
            EnemyBase enemy = netObj.GetComponent<EnemyBase>();
            
            if (enemy != null && !enemy.isDead)
            {
                enemy.SetPossessed(true);
            }
        }
    }

    void HandleTrapSelection()
    {
        if (trapPrefabs.Length <= 1) 
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0) 
        {
            selectedTrap = (selectedTrap + 1) % trapPrefabs.Length;
        }

        if (scroll < 0) 
        {
            selectedTrap = (selectedTrap - 1 + trapPrefabs.Length) % trapPrefabs.Length;
        }
    }

    void UpdateUI()
    {
        if (!IsOwner || budgetText == null) 
        {
            return;
        }

        string trapText = $"Trampas: {trapBudget - networkTrapsPlaced.Value} / {trapBudget}";
        
        if (currentTrapCooldown > 0f) 
        {
            trapText += $" (Espera: {currentTrapCooldown:F1}s)";
        }

        string posText = currentPossessionCooldown > 0f ? $"Posesión: {currentPossessionCooldown:F1}s" : "Posesión: Lista";
        budgetText.text = trapText + "\n" + posText;
    }

    void UpdateP1UI()
    {
        if (p1HealthBar == null) 
        { 
            GameObject objVida = GameObject.Find("Health"); 
            
            if (objVida != null) 
            {
                p1HealthBar = objVida.GetComponent<Image>(); 
            }
        }

        if (p1DashCooldownBar == null) 
        { 
            GameObject objDash = GameObject.Find("DashCoolDown"); 
            
            if (objDash != null) 
            {
                p1DashCooldownBar = objDash.GetComponent<Image>(); 
            }
        }

        if (p1HealthBar == null || p1DashCooldownBar == null) 
        {
            return;
        }

        if (player1Target == null) 
        { 
            player1Target = FindAnyObjectByType<PlayerControllerComplete>(); 
            
            if (player1Target == null) 
            {
                return; 
            }
        }

        p1HealthBar.fillAmount = Mathf.Clamp01((float)player1Target.networkHealth.Value / player1Target.maxHealth);
        p1DashCooldownBar.fillAmount = Mathf.Clamp01(player1Target.networkDashCooldown.Value);
    }

    public void ResetForNewRound() 
    { 
        if (IsServer) 
        {
            networkTrapsPlaced.Value = 0; 
        }

        possessedEnemy = null; 
        isP2Active = false; 
        currentTrapCooldown = 0f; 
        currentPossessionCooldown = 0f; 
        UpdateUI(); 
    }
}