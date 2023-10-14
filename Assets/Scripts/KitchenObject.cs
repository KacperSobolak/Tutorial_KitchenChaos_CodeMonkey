using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour{

	[SerializeField] private KitchenObjectSO kitchenObjectSO;
	[SerializeField] private Collider kitchenObjectCollider;

	private IKitchenObjectParent kitchenObjectParent;
	private FollowTransform followTransform;

	private void Awake() {
		followTransform = GetComponent<FollowTransform>();
	}

	public KitchenObjectSO GetKitchenObjectSO() { 
		return kitchenObjectSO; 
	}

	public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent) {
		SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
		
		// transform.localRotation = Quaternion.identity;
		// kitchenObjectCollider.enabled = false;
		// if (TryGetComponent(out Rigidbody kitchenObjectRigidbody)) {
		// 	Destroy(kitchenObjectRigidbody);
		// }
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference) {
		SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
	}

	[ClientRpc]
	private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference) {
		kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
		IKitchenObjectParent IKitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
		
		if (kitchenObjectParent != null) {
			kitchenObjectParent.ClearKitchenObject();
		}

		kitchenObjectParent = IKitchenObjectParent;
		
		if (IKitchenObjectParent.HasKitchenObject()) {
			Debug.LogError("IKitchenObjectParent already has a kitchen object");
		}

		IKitchenObjectParent.SetKitchenObject(this);
		
		followTransform.SetTargetTransform(IKitchenObjectParent.GetKitchenObjectFollowTransform());
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
		Destroy(gameObject);
	}

	public void ClearKitchenObjectOnParent() {
		kitchenObjectParent.ClearKitchenObject();
	}

	public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) {
		KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
	}

	public static void DestroyKitchenObject(KitchenObject kitchenObject) {
		KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
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
