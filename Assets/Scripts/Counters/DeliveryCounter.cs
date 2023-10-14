using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter{

	public static DeliveryCounter Instance { get; private set; }

	private void Awake() {
		if (Instance !=		null) {
			Debug.LogError("More than one instance of Delivery Counter");
		}
		Instance = this;
	}

	public override void Interact(Player player) {
		if (player.HasKitchenObject()) {
			if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
				//Only accept plate
				DeliveryManager.Instance.DeliverRecipe(plateKitchenObject);

				KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
			}
		}
	}
}
