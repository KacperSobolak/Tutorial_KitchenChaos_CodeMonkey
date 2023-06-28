using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour{

	public event EventHandler OnRecipeSpawned;
	public event EventHandler OnRecipeCompleted;
	public event EventHandler OnRecipeSuccess;
	public event EventHandler OnRecipeFailed;

	public static DeliveryManager Instance { get; private set; }

	[SerializeField] private RecipeListSO recipeListSO;

	private List<RecipeSO> waitingRecipeSOList = new();

	private float spawnRecipeTimer;
	private float spawnRecipeTimerMax = 4f;
	private int waitingRecipeMax = 4;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("There is more that one delivery manager instance");
		}
		Instance = this;
	}

	private void Update() {
		spawnRecipeTimer -= Time.deltaTime;
		if (spawnRecipeTimer <= 0f) {
			spawnRecipeTimer = spawnRecipeTimerMax;

			if (waitingRecipeSOList.Count < waitingRecipeMax) {
				RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];	
				waitingRecipeSOList.Add(waitingRecipeSO);

				OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
			}
		}
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
					Debug.Log("Player delivered the correct recipe!");
					waitingRecipeSOList.RemoveAt(i);
					OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
					OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
					return;
				}
			}
		}

		//No matches found 
		//Player did not deliver a correct recipe
		OnRecipeFailed?.Invoke(this, EventArgs.Empty);
	}

	public List<RecipeSO> GetWaitingRecipeSOList() {
		return waitingRecipeSOList;
	}
}
