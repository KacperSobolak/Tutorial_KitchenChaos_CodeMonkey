using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject{

	public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
	public class OnIngredientAddedEventArgs : EventArgs {
		public KitchenObjectSO kitchenObjectSO;
	}

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

			OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs {
				kitchenObjectSO = kitchenObjectSO
			});
			return true;
		}
	}

}
