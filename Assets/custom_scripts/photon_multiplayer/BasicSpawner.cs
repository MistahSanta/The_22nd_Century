using UnityEngine;

using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private Canvas _menuCanvas;
    private Text _hostLabel;
    private Text _joinLabel;
    [SerializeField] private Camera _menuCamera; // Assign a simple camera in Inspector
    

    
    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            
            Vector3 spawnPosition = new Vector3(-16, 4, -12);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            runner.SetPlayerObject(player, networkPlayerObject);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            
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
    }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Read WASD or Joystick
        float x = Input.GetAxis("Vertical");
        float z = -Input.GetAxis("Horizontal");

        data.Direction = new Vector3(x, 0, z);

        input.Set(data);

    }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
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
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        

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

    private void Start()
    {
        if (_menuCamera != null)
            _menuCamera.gameObject.SetActive(true);

        GameObject canvasGO = new GameObject("MenuCanvas");
        _menuCanvas = canvasGO.AddComponent<Canvas>();
        _menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // No camera needed
        canvasGO.AddComponent<GraphicRaycaster>();

        // Add CanvasScaler for consistent sizing across resolutions
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 200);

        _hostLabel = CreateLabel(canvasGO, "Press A to Host", new Vector2(0, 40));
        _joinLabel = CreateLabel(canvasGO, "Press B to Join", new Vector2(0, -40));
    }

    private void Update()
    {
        if (_runner == null)
        {
            if (_menuCanvas != null)
            {
                _menuCanvas.gameObject.SetActive(true);
                // No camera positioning needed for ScreenSpaceOverlay
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
                StartGame(GameMode.Host);
            }
            else if (x_button)
            {
                Debug.Log("Starting as CLIENT...");
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