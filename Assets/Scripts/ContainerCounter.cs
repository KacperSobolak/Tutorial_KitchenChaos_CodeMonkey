using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter{

	public event EventHandler OnPlayerGrabObject;

	[SerializeField] private KitchenObjectSO kitchenObjectSO;

	public override void Interact(Player player) {
		if (!player.HasKitchenObject()) {
			Transform kitchenObjectTranform = Instantiate(kitchenObjectSO.prefab);
			kitchenObjectTranform.GetComponent<KitchenObject>().SetKitchenObjectParent(player);

			OnPlayerGrabObject?.Invoke(this, EventArgs.Empty);
		}
	}
}
