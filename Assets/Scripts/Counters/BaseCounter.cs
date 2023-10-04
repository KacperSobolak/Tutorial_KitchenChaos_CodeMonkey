using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : Interactable, IKitchenObjectParent{

	public static event EventHandler OnAnyObjectPlaced;

	public static void ResetStaticData() {
		OnAnyObjectPlaced = null;
	}

	[SerializeField] private Transform counterTopPoint;

	private KitchenObject kitchenObject;

	public virtual void InteractAlternate(Player player) {
		//Debug.LogError("Interactable.InteractAlternate();");
	}

	public Transform GetKitchenObjectFollowTransform() {
		return counterTopPoint;
	}

	public void SetKitchenObject(KitchenObject kitchenObject) {
		this.kitchenObject = kitchenObject;
		if (kitchenObject != null) {
			OnAnyObjectPlaced?.Invoke(this, EventArgs.Empty);
		}
	}

	public KitchenObject GetKitchenObject() {
		return kitchenObject;
	}

	public void ClearKitchenObject() {
		kitchenObject = null;
	}

	public bool HasKitchenObject() {
		return kitchenObject != null;
	}

	public NetworkObject GetNetworkObject() {
		return null;
	}
}
