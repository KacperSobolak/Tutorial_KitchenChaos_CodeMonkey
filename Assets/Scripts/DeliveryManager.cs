using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour{

	public event EventHandler OnRecipeSpawned;
	public event EventHandler OnRecipeCompleted;
	public event EventHandler OnRecipeSuccess;
	public event EventHandler OnRecipeFailed;

	public static DeliveryManager Instance { get; private set; }

	[SerializeField] private RecipeListSO recipeListSO;

	private List<RecipeSO> waitingRecipeSOList = new();

	private float spawnRecipeTimer = 4f;
	private float spawnRecipeTimerMax = 4f;
	private int waitingRecipeMax = 4;
	private int successfulRecipesAmount;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("There is more that one delivery manager instance");
		}
		Instance = this;
	}

	private void Update() {
		if (!IsServer)
			return;
		
		spawnRecipeTimer -= Time.deltaTime;
		if (spawnRecipeTimer <= 0f) {
			spawnRecipeTimer = spawnRecipeTimerMax;

			if (GameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipeMax) {
				int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
				
				SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
			}
		}
	}

	[ClientRpc]
	private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex) {
		RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex]; 
		
		waitingRecipeSOList.Add(waitingRecipeSO);

		OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
	}

	public void DeliverRecipe(PlateKitchenObject plateKitchenObject) {
		for (int i = 0; i <  waitingRecipeSOList.Count; i++) {
			RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
			if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
				//Has the same number of ingredients
				bool plateContentsMatchesRecipe = true;
				foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList) {
					//Cycling throught all ingredients in the Recipe 
					bool ingredientFound = false;
					foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) {
						//Cycling throught all ingredients in the Plate 
						if (recipeKitchenObjectSO == plateKitchenObjectSO) {
							ingredientFound = true;
							break;
							//Ingredient matches
						}
					}
					if (!ingredientFound) {
						//This recipe ingredient was not found on the Plate
						plateContentsMatchesRecipe = false;
						break;
					}
				}

				if (plateContentsMatchesRecipe) {
					//Player delivered correct recipe
					DeliverCorrectRecipeServerRpc(i);
					return;
				}
			}
		}

		//No matches found 
		//Player did not deliver a correct recipe
		DeliverIncorrectRecipeSOServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void DeliverIncorrectRecipeSOServerRpc() {
		DeliverIncorrectRecipeSOClientRpc();
	}
	
	[ClientRpc]
	private void DeliverIncorrectRecipeSOClientRpc() {
		OnRecipeFailed?.Invoke(this, EventArgs.Empty);
	}

	[ServerRpc(RequireOwnership = false)]
	private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex) {
		DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
	}

	[ClientRpc]
	private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex) {
		successfulRecipesAmount++;

		waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
		OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
		OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
	}

	public List<RecipeSO> GetWaitingRecipeSOList() {
		return waitingRecipeSOList;
	}

	public int GetSuccesfulRecipesAmount() {
		return successfulRecipesAmount;
	}
}
