using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedInteractableVisual : MonoBehaviour{

	[SerializeField] private Interactable interactable;
	[SerializeField] private GameObject[] visualGameObjects;

	private void Start() {
		if (Player.LocalInstance != null) {
			Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
		}
		else {
			Player.OnAnyPlayerSpawned += Player_OnOnAnyPlayerSpawned;
		}
	}
	private void Player_OnOnAnyPlayerSpawned(object sender, EventArgs e) {
		if (Player.LocalInstance != null) {
			Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
			Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
		}	
	}

	private void OnDestroy() {
		Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
	}

	private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedObjectChangedEventArgs e) {
		foreach (GameObject visualGameObject in visualGameObjects) {
			if (visualGameObject == null)
				return;
			
			visualGameObject.SetActive(e.interactable == interactable);
		}
	}
}
