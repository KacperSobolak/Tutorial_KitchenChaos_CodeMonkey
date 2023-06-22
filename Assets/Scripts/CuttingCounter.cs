using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter{

	[SerializeField] CuttingRecipeSO[] cuttingRecipesSO;

	public override void Interact(Player player) {
		if (!HasKitchenObject()) {
			if (player.HasKitchenObject()) {
				if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
					player.GetKitchenObject().SetKitchenObjectParent(this);
				}
			}
		}
		else {
			if (!player.HasKitchenObject()) {
				GetKitchenObject().SetKitchenObjectParent(player);
			}
		}
	}

	public override void InteractAlternate(Player player) {
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
			KitchenObjectSO outputKitchenObjestSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
			GetKitchenObject().DestroySelf();
			
			KitchenObject.SpawnKitchenObject(outputKitchenObjestSO, this);
		}
	}

	private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipesSO) {
			if (cuttingRecipeSO.input == inputKitchenObjectSO) {
				return true;
			}
		}
		return false;
	} 

	private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipesSO) {
			if (cuttingRecipeSO.input == inputKitchenObjectSO) {
				return cuttingRecipeSO.output;
			}
		}
		return null;
	}
}
