using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject{

	[SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

	private List<KitchenObjectSO> kitchenObjectsSOList = new(); 

	public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO) {
		if (!validKitchenObjectSOList.Contains(kitchenObjectSO)) {
			return false;
		}
		if (kitchenObjectsSOList.Contains(kitchenObjectSO)) {
			return false;
		}
		else {
			kitchenObjectsSOList.Add(kitchenObjectSO);
			return true;
		}
	}

}
