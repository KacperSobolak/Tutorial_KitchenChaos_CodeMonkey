using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress{

	public static event EventHandler OnCutAny;

	public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChanged;
	public event EventHandler OnCut;

	[SerializeField] CuttingRecipeSO[] cuttingRecipesSO;

	private int cuttingProgress;

	public override void Interact(Player player) {
		if (!HasKitchenObject()) {
			if (player.HasKitchenObject()) {
				if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
					player.GetKitchenObject().SetKitchenObjectParent(this);
					cuttingProgress = 0;

					CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
						progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
					});
;				}
			}
		}
		else {
			if (!player.HasKitchenObject()) {
				GetKitchenObject().SetKitchenObjectParent(player);

				OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
					progressNormalized = 0f
				});
			}
			else {
				if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
					if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
						GetKitchenObject().DestroySelf();
					}
				}
			}
		}
	}

	public override void InteractAlternate(Player player) {
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
			cuttingProgress++;

			OnCut?.Invoke(this, EventArgs.Empty);
			OnCutAny?.Invoke(this, EventArgs.Empty);

			CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

			OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
				progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
			});

			if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
				KitchenObjectSO outputKitchenObjestSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
				GetKitchenObject().DestroySelf();
			
				KitchenObject.SpawnKitchenObject(outputKitchenObjestSO, this);
			}

		}
	}

	private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
		return cuttingRecipeSO != null;
	} 

	private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
		if (cuttingRecipeSO != null) {
			return cuttingRecipeSO.output;
		}
		else {
			return null;
		}
	}

	private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipesSO) {
			if (cuttingRecipeSO.input == inputKitchenObjectSO) {
				return cuttingRecipeSO;
			}
		}
		return null;
	}
}
