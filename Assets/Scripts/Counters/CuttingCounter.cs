using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress{

	public static event EventHandler OnCutAny;

	new public static void ResetStaticData() {
		OnCutAny = null;
	}

	public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChanged;
	public event EventHandler OnCut;

	[SerializeField] CuttingRecipeSO[] cuttingRecipesSO;

	private int cuttingProgress;

	public override void Interact(Player player) {
		if (!HasKitchenObject()) {
			if (player.HasKitchenObject()) {
				if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
					KitchenObject kitchenObject = player.GetKitchenObject();
					
					kitchenObject.SetKitchenObjectParent(this);

					InteractLogicPlaceObjectOnCounterServerRpc()
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
						KitchenObject.DestroyKitchenObject(GetKitchenObject());
					}
				}
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void InteractLogicPlaceObjectOnCounterServerRpc() {
		InteractLogicPlaceObjectOnCounterClientRpc();
	}
	
	[ClientRpc]
	private void InteractLogicPlaceObjectOnCounterClientRpc() {
		cuttingProgress = 0;
		
		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
			progressNormalized = 0f
		});
	}

	public override void InteractAlternate(Player player) {
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
			CutObjectServerRpc();
			TestCuttingProgressDoneServerRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void CutObjectServerRpc() {
		CutObjectClientRpc();
	}

	[ClientRpc]
	private void CutObjectClientRpc() {
		cuttingProgress++;

		OnCut?.Invoke(this, EventArgs.Empty);
		OnCutAny?.Invoke(this, EventArgs.Empty);

		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
			progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
		});
	}
	
	[ServerRpc(RequireOwnership = false)]
	private void TestCuttingProgressDoneServerRpc(){
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
		
		if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
			KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
			
			KitchenObject.DestroyKitchenObject(GetKitchenObject());
			
			KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
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
		
		return null;
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
