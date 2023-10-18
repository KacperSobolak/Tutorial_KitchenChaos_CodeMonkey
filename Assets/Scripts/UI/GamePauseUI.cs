using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
	[SerializeField] private Button resumeButton;
	[SerializeField] private Button optionsButton;
	[SerializeField] private Button mainMenuButton;

	private void Awake() {
		resumeButton.onClick.AddListener(() => {
			GameManager.Instance.TogglePauseGame();
		});
		optionsButton.onClick.AddListener(() => {
			Hide();
			OptionsUI.Instance.Show(Show);
		});
		mainMenuButton.onClick.AddListener(() => {
			Loader.Load(Loader.Scene.MainMenuScene);
		});
	}

	private void Start() {
		GameManager.Instance.OnLocalGamePaused += GameManager_OnGameLocalPaused;
		GameManager.Instance.OnLocalGameUnpaused += GameManager_OnGameLocalUnpaused;

		Hide();
	}

	private void GameManager_OnGameLocalUnpaused(object sender, System.EventArgs e) {
		Hide();
	}

	private void GameManager_OnGameLocalPaused(object sender, System.EventArgs e) {
		Show();
	}

	private void Show() {
		gameObject.SetActive(true);
	}

	private void Hide() {
		gameObject.SetActive(false);
	}
}
