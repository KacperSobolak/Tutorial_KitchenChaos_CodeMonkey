using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent {

	public static Player Instance { get; private set; }

	public event EventHandler<OnSelectedObjectChangedEventArgs> OnSelectedCounterChanged;
	public class OnSelectedObjectChangedEventArgs : EventArgs {
		public ISelectedObject selectedObject;
	}

	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private float dropForce = 60f;
	[SerializeField] private GameInput gameInput;
	[SerializeField] private LayerMask interactableLayerMask;
	[SerializeField] private Transform kitchenObjectHoldPoint;

	private bool isWalking;
	private Vector3 lastInteractDir;
	private ISelectedObject selectedObject;
	private KitchenObject kitchenObject;

	private void Awake() {
		if (Instance != null) {
			Debug.LogError("There is more that one player instance");
		}
		Instance = this;
	}

	private void Start() {
		gameInput.OnInteractAction += GameInput_OnInteractAction;
	}

	private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
		if (selectedObject != null) {
			selectedObject.Interact(this);
		}
		else if (kitchenObject != null) {
			DropKitchenObject();
		}
	}

	private void Update() {
		HandleMovement();
		HandleInteractions();
	}

	public bool IsWalking() {
		return isWalking;
	}

	private void HandleInteractions() {
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();

		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

		if (moveDir != Vector3.zero) {
			lastInteractDir = moveDir;
		}

		float interactDistance = 2f;
		float offsetFromGround = 0.1f;
		if (Physics.Raycast(transform.position + Vector3.up * offsetFromGround, lastInteractDir, out RaycastHit raycastHit, interactDistance, interactableLayerMask)) {
			if (raycastHit.transform.TryGetComponent(out ISelectedObject selectedObject)) {
				if (this.selectedObject != selectedObject) {
					SetSelectedObject(selectedObject);
				}
			}
			else {
				if (selectedObject != null) {
					SetSelectedObject(null);
				}
			}
		}
		else {
			if (selectedObject != null) {
				SetSelectedObject(null);
			}
		}
	}

	private void HandleMovement() {
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();

		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

		float moveDistance = moveSpeed * Time.deltaTime;
		float playerRadius = 0.7f;
		float playerHeight = 2f;
		bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);
		if (!canMove) {
			Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0f);
			canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);
			if (canMove) {
				moveDir = moveDirX;
			}
			else {
				Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z);
				canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
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

	private void SetSelectedObject(ISelectedObject selectedObject) {
		this.selectedObject = selectedObject;

		OnSelectedCounterChanged?.Invoke(this, new OnSelectedObjectChangedEventArgs {
			selectedObject = selectedObject
		});
	}

	public Transform GetKitchenObjectFollowTranform() {
		return kitchenObjectHoldPoint;
	}

	public void SetKitchenObject(KitchenObject kitchenObject) {
		this.kitchenObject = kitchenObject;
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
		kitchenObject.ClearKitchenObjectParent();
		kitchenObject.gameObject.AddComponent<Rigidbody>().AddForce(transform.forward * dropForce, ForceMode.Force);
		ClearKitchenObject();
	}
}
