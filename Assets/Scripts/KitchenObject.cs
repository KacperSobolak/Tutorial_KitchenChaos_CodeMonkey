using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : Interactable{

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
		transform.localRotation = Quaternion.identity;
		kitchenObjectCollider.enabled = false;
		if (TryGetComponent(out Rigidbody kitchenObjectRigidbody)) {
			Destroy(kitchenObjectRigidbody);
		}
	}	

	public override void Interact(Player player) {
		if (!player.HasKitchenObject()) {
			SetKitchenObjectParent(player);
		}
	}
	
	public IKitchenObjectParent GetKichtenObjectParent() {
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

	public static KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) {
		Transform kitchenObjectTranform = Instantiate(kitchenObjectSO.prefab);

		KitchenObject kitchenObject = kitchenObjectTranform.GetComponent<KitchenObject>();
		kitchenObject.SetKitchenObjectParent(kitchenObjectParent);

		return kitchenObject;
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
