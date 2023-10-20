using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour{
	public static GameManager Instance { get; private set; }

	public event EventHandler OnStateChanged;
	public event EventHandler OnLocalGamePaused;
	public event EventHandler OnLocalGameUnpaused;
	public event EventHandler OnMultiplayerGamePaused;
	public event EventHandler OnMultiplayerGameUnpaused;
	public event EventHandler OnLocalPlayerReadyChange;

	private enum State {
		WaitingToStart,
		CountdownToStart,
		GamePlaying,
		GameOver
	}

	private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
	private bool isLocalPlayerReady;
	private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
	private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
	private float gamePlayingTimerMax = 90f;

	private bool isLocalGamePaused;
	private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>();
	private Dictionary<ulong, bool> playerReadyDictionary;
	private Dictionary<ulong, bool> playerPauseDictionary;
	private bool autoTestGamePausedState;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("More than one Game Manager");
		}
		
		Instance = this;
		
		playerReadyDictionary = new Dictionary<ulong, bool>();
		playerPauseDictionary = new Dictionary<ulong, bool>();
	}

	private void Start() {
		GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
		GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
		
// #if DEVELOPMENT_BUILD || UNITY_EDITOR
// 		gamePlayingTimerMax = 300f;
// 		countdownToStartTimer = 1f;
// 		
// 		state = State.CountdownToStart;
// 		OnStateChanged?.Invoke(this, EventArgs.Empty);
// #endif
	}

	public override void OnNetworkSpawn() {
		state.OnValueChanged += State_ValueChanged;
		isGamePaused.OnValueChanged += IsGamePaused_ValueChanged;

		if (IsServer) {
			NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
		}
	}

	private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
		autoTestGamePausedState = true;
	}

	private void IsGamePaused_ValueChanged(bool previousValue, bool newValue) {
		if (isGamePaused.Value) {
			Time.timeScale = 0f;
			
			OnMultiplayerGamePaused?.Invoke(this,EventArgs.Empty);
		}
		else {
			Time.timeScale = 1f;
			
			OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
		}
	}

	private void State_ValueChanged(State previousValue, State newValue) {
		OnStateChanged?.Invoke(this, EventArgs.Empty);
	}

	private void GameInput_OnInteractAction(object sender, EventArgs e) {
		if (state.Value == State.WaitingToStart) {
			isLocalPlayerReady = true;
			OnLocalPlayerReadyChange?.Invoke(this, EventArgs.Empty);
			
			SetPlayerReadyServerRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default) {
		playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

		bool allClientsReady = true;
		foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
			if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
				// This player in not ready
				allClientsReady = false;
				break;
			}
		}
		
		if (allClientsReady) {
			state.Value = State.CountdownToStart;
		}
	}

	private void GameInput_OnPauseAction(object sender, EventArgs e) {
		TogglePauseGame();
	}

	private void Update() {
		if (!IsServer)
			return;
		
		switch (state.Value) {
			case State.WaitingToStart:
				break;
			case State.CountdownToStart:
				countdownToStartTimer.Value -= Time.deltaTime;
				if (countdownToStartTimer.Value < 0f) {
					state.Value = State.GamePlaying;
					gamePlayingTimer.Value = gamePlayingTimerMax;
				}
				break;
			case State.GamePlaying:
				gamePlayingTimer.Value -= Time.deltaTime;
				if (gamePlayingTimer.Value < 0f) {
					state.Value = State.GameOver;
				}
				break;
			case State.GameOver:
				break;
		}
	}

	private void LateUpdate() {
		if (autoTestGamePausedState) {
			autoTestGamePausedState = false;
			TestGamePausedState();
		}
	}

	public bool IsGamePlaying() {
		return state.Value == State.GamePlaying;
	}

	public bool IsCountdownToStartActive() {
		return state.Value == State.CountdownToStart;
	}

	public float GetCountdownToStartTimer() {
		return countdownToStartTimer.Value;
	}

	public bool IsGameOver() {
		return state.Value == State.GameOver;
	}

	public bool IsWaitingToStart() {
		return state.Value == State.WaitingToStart;
	}

	public bool IsLocalPlayerReady() {
		return isLocalPlayerReady;
	}

	public float GetPlayingTimerNormalized() {
		return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
	}

	public void TogglePauseGame() {
		isLocalGamePaused = !isLocalGamePaused;
		if (isLocalGamePaused) {
			PauseGameServerRpc();

			OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
		}
		else {
			UnpauseGameServerRpc();

			OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default) {
		playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;
		
		TestGamePausedState();
	}
	
	[ServerRpc(RequireOwnership = false)]
	private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default) {
		playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;

		TestGamePausedState();
	}

	private void TestGamePausedState() {
		foreach (ulong clientsId in NetworkManager.Singleton.ConnectedClientsIds) {
			if (playerPauseDictionary.ContainsKey(clientsId) && playerPauseDictionary[clientsId]) {
				// This player is paused
				isGamePaused.Value = true;
				return;
			}
		}
		
		// All players are unpaused
		isGamePaused.Value = false;
	}
}