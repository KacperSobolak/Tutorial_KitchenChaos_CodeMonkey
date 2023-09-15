using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedInteractableVisual : MonoBehaviour{

	[SerializeField] private Interactable interactable;
	[SerializeField] private GameObject[] visualGameObjects;

	private void Start() {
		// Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
	}

	private void OnDestroy() {
		// Player.Instance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
	}

	private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedObjectChangedEventArgs e) {
		foreach (GameObject visualGameObject in visualGameObjects) {
			visualGameObject.SetActive(e.interactable == interactable);
		}
	}
}
