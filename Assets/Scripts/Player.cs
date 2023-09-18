using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour, IKitchenObjectParent {

	// public static Player Instance { get; private set; }

	public event EventHandler OnPlayerPickup;
	public event EventHandler OnPlayerDrop;
	public event EventHandler<OnSelectedObjectChangedEventArgs> OnSelectedCounterChanged;
	public class OnSelectedObjectChangedEventArgs : EventArgs {
		public Interactable interactable;
	}

	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private float dropForce = 60f;
	[SerializeField] private LayerMask interactableLayerMask;
	[SerializeField] private Transform kitchenObjectHoldPoint;

	private bool isWalking;
	private Vector3 lastInteractDir;
	private Interactable interactable;
	private KitchenObject kitchenObject;

	private void Awake() {
		// Instance = this;
	}

	private void Start() {
		GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
		GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
	}

	private void GameInput_OnInteractAlternateAction(object sender, EventArgs e) {
		if (!GameManager.Instance.IsGamePlaying()) return;

		if (interactable != null && interactable.GetType().IsSubclassOf(typeof(BaseCounter))) {
			((BaseCounter)interactable).InteractAlternate(this);
		}
	}

	private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
		if (!GameManager.Instance.IsGamePlaying()) return;

		if (interactable != null) {
			interactable.Interact(this);
		}
		else if (kitchenObject != null) {
			DropKitchenObject();
		}
	}

	private void Update() {
		if (!IsOwner) 
			return;
		
		HandleMovement();
		HandleInteractions();
	}

	public bool IsWalking() {
		return isWalking;
	}
	
	private void HandleInteractions() {
		Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
		if (moveDir != Vector3.zero) {
			lastInteractDir = moveDir;
		}

		float interactDistance = 1.75f;
		float offsetFromGround = 0.1f;
		if (Physics.Raycast(transform.position + Vector3.up * offsetFromGround, lastInteractDir, out RaycastHit raycastHit, interactDistance, interactableLayerMask)) {
			if (raycastHit.transform.TryGetComponent(out Interactable interactable)) {
				if (this.interactable != interactable) {
					SetSelectedObject(interactable);
				}
			}
			else {
				if (interactable != null) {
					SetSelectedObject(null);
				}
			}
		}
		else {
			if (interactable != null) {
				SetSelectedObject(null);
			}
		}
	}

	private void HandleMovement() {
		Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

		float moveDistance = moveSpeed * Time.deltaTime;
		float playerRadius = 0.7f;
		float playerHeight = 2f;
		bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);
		if (!canMove) {
			Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0f);
			canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
			if (canMove) {
				moveDir = moveDirX;
			}
			else {
				Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z);
				canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
				if (canMove) {
					moveDir = moveDirZ;
				}
			}
		}
		if (canMove) {
			transform.position += moveDir * moveDistance;
		}

		isWalking = moveDir != Vector3.zero && canMove;

		float rotateSpeed = 10f;
		transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
	}

	private void SetSelectedObject(Interactable selectedObject) {
		this.interactable = selectedObject;

		OnSelectedCounterChanged?.Invoke(this, new OnSelectedObjectChangedEventArgs {
			interactable = selectedObject
		});
	}

	public Transform GetKitchenObjectFollowTranform() {
		return kitchenObjectHoldPoint;
	}

	public void SetKitchenObject(KitchenObject kitchenObject) {
		this.kitchenObject = kitchenObject;

		if (kitchenObject != null) {
			OnPlayerPickup?.Invoke(this, EventArgs.Empty);
		}
	}

	public KitchenObject GetKitchenObject() {
		return kitchenObject;
	}

	public void ClearKitchenObject() {
		kitchenObject = null;
	}

	public bool HasKitchenObject() {
		return kitchenObject != null;
	}

	public void DropKitchenObject() {
		OnPlayerDrop?.Invoke(this, EventArgs.Empty);
		kitchenObject.ClearKitchenObjectParent();
		kitchenObject.gameObject.AddComponent<Rigidbody>().AddForce((transform.forward) * dropForce, ForceMode.Force);
		ClearKitchenObject();
	}
}
