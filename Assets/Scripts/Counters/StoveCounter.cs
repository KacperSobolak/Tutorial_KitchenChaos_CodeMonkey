using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

	private NetworkVariable<State> state = new();
	private NetworkVariable<float> fryingTimer = new();
	private FryingRecipeSO fryingRecipeSO;
	private NetworkVariable<float> burningTimer = new();
	private BurningRecipeSO burningRecipeSO;

	private void Start() {
		state.Value = State.Idle;
	}

	public override void OnNetworkSpawn() {
		fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
		burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
		state.OnValueChanged += State_OnValueChanged;
	}

	private void State_OnValueChanged(State previousValue, State newValue) {
		OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
			state = state.Value
		});

		if (state.Value == State.Burned || state.Value == State.Idle) {
			OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
				progressNormalized = 0f
			});
		}
	}

	private void BurningTimer_OnValueChanged(float previousValue, float newValue) {
		float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;
		
		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
			progressNormalized = burningTimer.Value / burningTimerMax
		});
	}

	private void FryingTimer_OnValueChanged(float previousValue, float newValue) {
		float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;
		
		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs {
			progressNormalized = fryingTimer.Value / fryingTimerMax
		});
	}

	private void Update() {
		if (!IsServer) 
			return;
		
		if (HasKitchenObject()) {
			switch (state.Value) {
				case State.Idle:

					break;
				case State.Frying:
					fryingTimer.Value += Time.deltaTime;
					
					if (fryingTimer.Value > fryingRecipeSO.fryingTimerMax) {
						//Fried 
						KitchenObject.DestroyKitchenObject(GetKitchenObject());

						KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

						state.Value = State.Fried;
						burningTimer.Value = 0f;
						
						SetBurningRecipeSOClientRpc(
							KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO()));
					}
					break;
				case State.Fried:
					burningTimer.Value += Time.deltaTime;
					
					if (burningTimer.Value > burningRecipeSO.burningTimerMax) {
						//Burned 
						KitchenObject.DestroyKitchenObject(GetKitchenObject());

						KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

						state.Value = State.Burned;
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
					KitchenObject kitchenObject = player.GetKitchenObject();
					
					kitchenObject.SetKitchenObjectParent(this);

					InteractLogicPlaceObjectOnCounterServerRpc(
						KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));
				}
			}
		}
		else {
			if (!player.HasKitchenObject()) {
				GetKitchenObject().SetKitchenObjectParent(player);
				state.Value = State.Idle;
			}
			else {
				if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
					if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
						KitchenObject.DestroyKitchenObject(GetKitchenObject());

						SetStateIdleServerRpc();
					}
				}
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetStateIdleServerRpc() {
		state.Value = State.Idle;
	}

	[ServerRpc(RequireOwnership = false)]
	private void InteractLogicPlaceObjectOnCounterServerRpc(int KitchenObjectSOIndex) {
		fryingTimer.Value = 0f;
		state.Value = State.Frying;

		SetFryingRecipeSOClientRpc(KitchenObjectSOIndex);
	}

	[ClientRpc]
	private void SetFryingRecipeSOClientRpc(int KitchenObjectSOIndex) {
		KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(KitchenObjectSOIndex);
		fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
	}
	
	[ClientRpc]
	private void SetBurningRecipeSOClientRpc(int KitchenObjectSOIndex) {
		KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(KitchenObjectSOIndex);
		burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
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

	public bool IsFried() {
		return state.Value == State.Fried;
	}

}
