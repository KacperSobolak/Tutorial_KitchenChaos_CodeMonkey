using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour{

	[SerializeField] private KitchenObjectSO kitchenObjectSO;

	private ClearCounter clearCounter;

	public KitchenObjectSO GetKitchenObjectSO() { 
		return kitchenObjectSO; 
	}

	public void SetClearCounter(ClearCounter clearCounter) {
		if (this.clearCounter != null) {
			this.clearCounter.ClearKitchenObject();
		}

		this.clearCounter = clearCounter;
		
		if (clearCounter.HasKitchenObject()) {
			Debug.LogError("Counter already has a kitchen object");
		}

		clearCounter.SetKitchenObject(this);

		transform.parent = clearCounter.GetKitchenObjectFollowTranform();
		transform.localPosition = Vector3.zero;
	}	
	
	public ClearCounter GetClearCounter() {
		return clearCounter;
	}
}
