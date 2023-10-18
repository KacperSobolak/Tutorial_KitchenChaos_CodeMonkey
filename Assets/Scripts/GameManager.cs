using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour{
	public static GameManager Instance { get; private set; }

	public event EventHandler OnStateChanged;
	public event EventHandler OnGamePaused;
	public event EventHandler OnGameUnpaused;
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
	private float gamePlayingTimerMax = 10f;
	private bool isGamePaused;

	private Dictionary<ulong, bool> playerReadyDictionary;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("More than one Game Manager");
		}
		
		Instance = this;
		
		playerReadyDictionary = new Dictionary<ulong, bool>();
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
		state.OnValueChanged += ValueChanged;
	}

	private void ValueChanged(State previousValue, State newValue) {
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

	public bool IsLocalPlayerReady() {
		return isLocalPlayerReady;
	}

	public float GetPlayingTimerNormalized() {
		return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
	}

	public void TogglePauseGame() {
		isGamePaused = !isGamePaused;
		if (isGamePaused) {
			Time.timeScale = 0f;
			OnGamePaused?.Invoke(this, EventArgs.Empty);
		}
		else {
			Time.timeScale = 1f;
			OnGameUnpaused?.Invoke(this, EventArgs.Empty);
		}
	}
}