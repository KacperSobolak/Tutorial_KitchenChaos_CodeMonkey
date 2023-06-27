using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress{

	public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChanged;
	public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
	public class OnStateChangedEventArgs : EventArgs {
		public State state;
	}

	public enum State {
		Idle,
		Frying,
		Fried,
		Burned,
	}

	[SerializeField] private FryingRecipeSO[] fryingRecipesSO;
	[SerializeField] private BurningRecipeSO[] burningRecipesSO;

	private State state;
	private float fryingTimer;
	private FryingRecipeSO fryingRecipeSO;
	private float burningTimer;
	private BurningRecipeSO burningRecipeSO;

	private void Start() {
		state = State.Idle;
	}

	private void Update() {
		if (HasKitchenObject()) {
			switch (state) {
				case State.Idle:

					break;
				case State.Frying:
					fryingTimer += Time.deltaTime;

					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
						progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
					});

					if (fryingTimer > fryingRecipeSO.fryingTimerMax) {
						//Fried 
						fryingTimer = 0f;
						GetKitchenObject().DestroySelf();

						KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

						state = State.Fried;
						burningTimer = 0f;
						burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

						OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
							state = state
						});
					}
					break;
				case State.Fried:
					burningTimer += Time.deltaTime;

					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
						progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
					});

					if (burningTimer > burningRecipeSO.burningTimerMax) {
						//Burned 
						GetKitchenObject().DestroySelf();

						KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

						state = State.Burned;

						OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
							state = state
						});

						OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
							progressNormalized = 0f
						});
					}
					break;
				case State.Burned:
					break;
			}
		}
	}

	public override void Interact(Player player) {
		if (!HasKitchenObject()) {
			if (player.HasKitchenObject()) {
				if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
					player.GetKitchenObject().SetKitchenObjectParent(this);

					fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

					state = State.Frying;
					fryingTimer = 0f;

					OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
						state = state
					});

					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
						progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
					});
				}
			}
		}
		else {
			if (!player.HasKitchenObject()) {
				GetKitchenObject().SetKitchenObjectParent(player);
				state = State.Idle;

				OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
					state = state
				});

				OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
					progressNormalized = 0f
				});
			}
			else {
				if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
					if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
						GetKitchenObject().DestroySelf();

						state = State.Idle;

						OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
							state = state
						});

						OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
							progressNormalized = 0f
						});
					}
				}
			}
		}
	}

	private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
		FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
		return fryingRecipeSO != null;
	}

	private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
		FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
		if (fryingRecipeSO != null) {
			return fryingRecipeSO.output;
		}
		else {
			return null;
		}
	}

	private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach (FryingRecipeSO fryingRecipeSO in fryingRecipesSO) {
			if (fryingRecipeSO.input == inputKitchenObjectSO) {
				return fryingRecipeSO;
			}
		}
		return null;
	}

	private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach (BurningRecipeSO burningRecipeSO in burningRecipesSO) {
			if (burningRecipeSO.input == inputKitchenObjectSO) {
				return burningRecipeSO;
			}
		}
		return null;
	}

}
