using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour{

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

		// transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
		// transform.localPosition = Vector3.zero;

		// transform.localRotation = Quaternion.identity;
		// kitchenObjectCollider.enabled = false;
		// if (TryGetComponent(out Rigidbody kitchenObjectRigidbody)) {
		// 	Destroy(kitchenObjectRigidbody);
		// }
	}	
	
	//Get dropped item
	// public override void Interact(Player player) {
	// 	if (!player.HasKitchenObject()) {
	// 		SetKitchenObjectParent(player);
	// 	}
	// }
	
	public IKitchenObjectParent GetKitchenObjectParent() {
		return kitchenObjectParent;
	}

	public void ClearKitchenObjectParent() {
		kitchenObjectParent = null;
		this.transform.parent = null;
		kitchenObjectCollider.enabled = true;
	}

	public void DestroySelf() {
		kitchenObjectParent.ClearKitchenObject();

		Destroy(gameObject);
	}

	public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) {
		KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
	}

	public bool TryGetPlate(out PlateKitchenObject plateKitchenObject) {
		if (this is PlateKitchenObject) {
			plateKitchenObject = this as PlateKitchenObject;
			return true;
		}
		else {
			plateKitchenObject = null;
			return false;
		}
	}
}
