using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : Interactable, IKitchenObjectParent{

	[SerializeField] private Transform counterTopPoint;

	private KitchenObject kitchenObject;

	public Transform GetKitchenObjectFollowTranform() {
		return counterTopPoint;
	}

	public void SetKitchenObject(KitchenObject kitchenObject) {
		this.kitchenObject = kitchenObject;
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
}
