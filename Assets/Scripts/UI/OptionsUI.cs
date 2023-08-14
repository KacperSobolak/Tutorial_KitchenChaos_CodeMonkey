using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
	public static OptionsUI Instance {get; private set;}

	[SerializeField] private Button soundEffectsButtons;
	[SerializeField] private Button musicButton;
	[SerializeField] private Button closeButton;
	[SerializeField] private Button moveUpButton;
	[SerializeField] private Button moveDownButton;
	[SerializeField] private Button moveLeftButton;
	[SerializeField] private Button moveRightButton;
	[SerializeField] private Button interactButton;
	[SerializeField] private Button interactAlternateButton;
	[SerializeField] private Button pauseButton;
	[SerializeField] private TextMeshProUGUI soundEffectsText;
	[SerializeField] private TextMeshProUGUI musicText;
	[SerializeField] private TextMeshProUGUI moveUpText;
	[SerializeField] private TextMeshProUGUI moveDownText;
	[SerializeField] private TextMeshProUGUI moveLeftText;
	[SerializeField] private TextMeshProUGUI moveRightText;
	[SerializeField] private TextMeshProUGUI interactText;
	[SerializeField] private TextMeshProUGUI interactAlternateText;
	[SerializeField] private TextMeshProUGUI pauseText;
	[SerializeField] private Transform pressToRebindKeyTransform;

	private Action onCloseButtonAction;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("More than one OptionsUI is scene");
		}
		Instance = this;
		soundEffectsButtons.onClick.AddListener(() => {
			SoundManager.Instance.ChangeVolume();
			UpdateVisual();
		});
		musicButton.onClick.AddListener(() => {
			MusicManager.Instance.ChangeVolume();
			UpdateVisual();
		});
		closeButton.onClick.AddListener(() => {
			onCloseButtonAction();
			Hide();
		});

		moveUpButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Up); });
		moveDownButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Down); });
		moveLeftButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Left); });
		moveRightButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Right); });
		interactButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Interact); });
		interactAlternateButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.InteractAlternate); });
		pauseButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Pause); });
	}

	private void Start() {
		GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;

		UpdateVisual();
		
		Hide();
		HidePressToRebindKey();
	}

	private void GameManager_OnGameUnpaused(object sender, System.EventArgs e) {
		Hide();
	}

	private void UpdateVisual() {
		soundEffectsText.text = "Sound Effects : " + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
		musicText.text = "Music : " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

		moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
		moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
		moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
		moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
		interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
		interactAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
		pauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
	}

	public void Show(Action onCloseButtonAction) {
		this.onCloseButtonAction = onCloseButtonAction;

		gameObject.SetActive(true);
	}

	private void Hide() {
		gameObject.SetActive(false);
	}

	private void ShowPressToRebingKey() {
		pressToRebindKeyTransform.gameObject.SetActive(true);
	}

	private void HidePressToRebindKey() {
		pressToRebindKeyTransform.gameObject.SetActive(false);
	}

	private void RebindBinding(GameInput.Binding binding) {
		ShowPressToRebingKey();
		GameInput.Instance.RebindBinding(binding, () => {
			HidePressToRebindKey();
			UpdateVisual();
		});
	}
}
