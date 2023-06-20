using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour, ISelectedObject{

	[SerializeField] private KitchenObjectSO kitchenObjectSO;
	[SerializeField] private Collider kitchenObjectCollider;

	private IKitchenObjectParent kitchenObjectParent;

	public KitchenObjectSO GetKitchenObjectSO() { 
		return kitchenObjectSO; 
	}

	public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent) {
		if (this.kitchenObjectParent != null) {
			this.kitchenObjectParent.ClearKitchenObject();
		}

		this.kitchenObjectParent = kitchenObjectParent;
		
		if (kitchenObjectParent.HasKitchenObject()) {
			Debug.LogError("IKitchenObjectParent already has a kitchen object");
		}

		kitchenObjectParent.SetKitchenObject(this);

		transform.parent = kitchenObjectParent.GetKitchenObjectFollowTranform();
		transform.localPosition = Vector3.zero;
		kitchenObjectCollider.enabled = false;
		if (TryGetComponent(out Rigidbody kitchenObjectRigidbody)) {
			Destroy(kitchenObjectRigidbody);
		}
	}	

	public void Interact(Player player) {
		SetKitchenObjectParent(player);
	}
	
	public IKitchenObjectParent GetKichtenObjectParent() {
		return kitchenObjectParent;
	}

	public void ClearKitchenObjectParent() {
		kitchenObjectParent = null;
		this.transform.parent = null;
		kitchenObjectCollider.enabled = true;
	}
}
