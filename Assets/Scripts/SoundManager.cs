using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour{

	private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

	public static SoundManager Instance { get; private set; }

	[SerializeField] private AudioClipsRefsSO AudioClipsRefsSO;

	private float volume = 1f;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("More than one instance of Sound Manager");
		}
		Instance = this;

		volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
	}


	private void Start() {
		DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
		DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
		CuttingCounter.OnCutAny += CuttingCounter_OnCutAny;
		Player.Instance.OnPlayerPickup += Player_OnPlayerPickup;
		Player.Instance.OnPlayerDrop += Player_OnPlayerDrop;
		BaseCounter.OnAnyObjectPlaced += BaseCounter_OnAnyObjectPlaced;
		TrashCounter.OnAnyObjectTrash += TrashCounter_OnAnyObjectTrash;
	}

	private void TrashCounter_OnAnyObjectTrash(object sender, System.EventArgs e) {
		PlaySound(AudioClipsRefsSO.trash, ((TrashCounter)sender).transform.position);

	}

	private void BaseCounter_OnAnyObjectPlaced(object sender, System.EventArgs e) {
		PlaySound(AudioClipsRefsSO.objectDrop, ((BaseCounter)sender).transform.position);
	}

	private void Player_OnPlayerDrop(object sender, System.EventArgs e) {
		PlaySound(AudioClipsRefsSO.objectDrop, Player.Instance.transform.position);
	}

	private void Player_OnPlayerPickup(object sender, System.EventArgs e) {
		PlaySound(AudioClipsRefsSO.objectPick, Player.Instance.transform.position);
	}

	private void CuttingCounter_OnCutAny(object sender, System.EventArgs e) {
		CuttingCounter cuttingCounter = sender as CuttingCounter;
		PlaySound(AudioClipsRefsSO.chop, cuttingCounter.transform.position);
	}

	private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e) {
		DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
		PlaySound(AudioClipsRefsSO.deliveryFail, deliveryCounter.transform.position);
	}

	private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e) {
		DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
		PlaySound(AudioClipsRefsSO.deliverySuccess, deliveryCounter.transform.position);
	}

	private void PlaySound(AudioClip[] audioClips, Vector3 position, float volume = 1f) {
		PlaySound(audioClips[Random.Range(0, audioClips.Length)], position, volume);
	}	
	
	private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplayer = 1f) {
		AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplayer * volume);
	}

	public void PlayFootstepsSound(Vector3 position, float volume) {
		PlaySound(AudioClipsRefsSO.footstep, position, volume);
	}

	public void PlayCountdownSound() {
		PlaySound(AudioClipsRefsSO.warning, Vector3.zero);
	}

	public void PlayWarningSound(Vector3 position) {
		PlaySound(AudioClipsRefsSO.warning, position);
	}

	public void ChangeVolume() {
		volume += 0.1f;
		if (volume > 1f) {
			volume = 0f;
		}

		PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
		PlayerPrefs.Save();
	}

	public float GetVolume() {
		return volume;
	}
}
