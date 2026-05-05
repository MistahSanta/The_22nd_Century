using UnityEngine;

using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;
using Photon.Voice.Unity;
using System.Threading.Tasks;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    
    // [Header("Per-Player Tools")]
    // public NetworkPrefabRef garbagePickerPrefab;
    // public NetworkPrefabRef shovelPrefab;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private Canvas _menuCanvas;
    private Text _hostLabel;
    private Text _joinLabel;
    [SerializeField] private Camera _menuCamera; // Assign a simple camera in Inspector
    
    // Dictionary to track which tools belong to which player
    Dictionary<PlayerRef, List<NetworkObject>> _playerTools = new();

    
    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPosition = new Vector3(-16, 4, -12);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            
            
            runner.SetPlayerObject(player, networkPlayerObject);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);

            // Spawn tools for this player
            //var tools = new List<NetworkObject>();

            // var picker = runner.Spawn(garbagePickerPrefab, spawnPosition, Quaternion.identity, player);
            // var shovel = runner.Spawn(shovelPrefab, spawnPosition, Quaternion.identity, player);

            // tools.Add(picker);
            // tools.Add(shovel);
            //_playerTools[player] = tools;
                    
            if (_spawnedCharacters.Count == 1)
            {  // Start nav mesh on that player now that a player exist now
                foreach (var zombie in FindObjectsOfType<NavMeshAgent>())
                {
                    zombie.enabled = true;
                }
            }


        }


        if (player == runner.LocalPlayer)
        {
            // Disable menu camera so VR rig camera takes over
            if (_menuCamera != null)
                _menuCamera.gameObject.SetActive(false);

            // Hide menu
            if (_menuCanvas != null)
                _menuCanvas.gameObject.SetActive(false);
        }
    }
    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }

        // if (_playerTools.TryGetValue(player, out var tools))
        // {
        //     foreach (var tool in tools)
        //         if (tool != null) runner.Despawn(tool);
        //     _playerTools.Remove(player);
        // }
    }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Read WASD or Joystick
        float x = Input.GetAxis("Vertical");
        float z = -Input.GetAxis("Horizontal");


        // Keyboard fallback
        if (x == 0) x = (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f);
        if (z == 0) z = (Input.GetKey(KeyCode.S) ? -1f : 0f) + (Input.GetKey(KeyCode.W) ? 1f : 0f);


        data.Direction = new Vector3(x, 0, z);

        Transform cam = LocalPlayerHolder.GetLocalCamera();
        if (cam != null)
            data.CameraYaw = cam.eulerAngles.y;

        input.Set(data);

    }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Network shutdown: " + shutdownReason);
        if (_statusLabel != null) _statusLabel.text = "Disconnected: " + shutdownReason;
        _runner = null;
    }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError("Connection failed: " + reason);
        if (_statusLabel != null) _statusLabel.text = "Connection failed! Check WiFi.";
        _runner = null; // Allow retry
    }
    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }



    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.GetComponent<NetworkRunner>(); //gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        

        _runner.AddCallbacks(this);
        
        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

    }



    private Text CreateLabel(GameObject parent, string text, Vector2 position)
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(parent.transform, false);

        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 36;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 80);
        rt.anchoredPosition = position;

        return t;
    }

    private Text _statusLabel;

    private void Start()
    {
        if (_menuCamera != null)
            _menuCamera.gameObject.SetActive(true);

        GameObject canvasGO = new GameObject("MenuCanvas");
        _menuCanvas = canvasGO.AddComponent<Canvas>();
        _menuCanvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 350);
        rt.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        // Position in front of camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            canvasGO.transform.position = cam.transform.position + cam.transform.forward * 3f;
            canvasGO.transform.rotation = Quaternion.LookRotation(
                canvasGO.transform.position - cam.transform.position);
        }
        else
        {
            canvasGO.transform.position = new Vector3(0, 2, 3);
        }

        // Background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(canvasGO.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.8f);
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 350);

        CreateLabel(canvasGO, "The 22nd Century", new Vector2(0, 130));
        _hostLabel = CreateLabel(canvasGO, "Press [A] to Host a Game", new Vector2(0, 40));
        _joinLabel = CreateLabel(canvasGO, "Press [X] to Join a Game", new Vector2(0, -40));
        _statusLabel = CreateLabel(canvasGO, "Both players must be on the same WiFi", new Vector2(0, -120));
        _statusLabel.fontSize = 20;
        _statusLabel.color = Color.yellow;
    }

    private void Update()
    {
        if (_runner == null)
        {
            if (_menuCanvas != null)
            {
                _menuCanvas.gameObject.SetActive(true);

                // Keep menu in front of camera (WorldSpace)
                Camera cam = _menuCamera != null && _menuCamera.gameObject.activeSelf
                    ? _menuCamera : Camera.main;
                if (cam != null)
                {
                    _menuCanvas.transform.position = cam.transform.position + cam.transform.forward * 3f;
                    _menuCanvas.transform.rotation = Quaternion.LookRotation(
                        _menuCanvas.transform.position - cam.transform.position);
                }
            }

            bool a_button = ControllerMapping.Instance != null
                    ? ControllerMapping.Instance.GetInteractDown()
                    : Input.GetKeyDown(KeyCode.E);
            bool x_button = ControllerMapping.Instance != null
                    ? ControllerMapping.Instance.GetSwitchToolDown()
                    : Input.GetKeyDown(KeyCode.X);

            if (a_button)
            {
                Debug.Log("Starting as HOST...");
                if (_statusLabel != null) _statusLabel.text = "Connecting as HOST...";
                StartGame(GameMode.Host);
            }
            else if (x_button)
            {
                Debug.Log("Starting as CLIENT...");
                if (_statusLabel != null) _statusLabel.text = "Joining game...";
                StartGame(GameMode.Client);
            }
        }
        else
        {
            // Hide menu once runner exists
            if (_menuCanvas != null)
                _menuCanvas.gameObject.SetActive(false);
        }
    }

}