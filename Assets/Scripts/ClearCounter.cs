using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : MonoBehaviour{

	[SerializeField] private KitchenObjectSO kitchenObjectSO;
	[SerializeField] private Transform counterTopPoint;

	public void Interact() {
		Debug.Log("Interact!");
		Transform kitchenObjectTranform = Instantiate(kitchenObjectSO.prefab, counterTopPoint);
		kitchenObjectTranform.localPosition = Vector3.zero;

		Debug.Log(kitchenObjectTranform.GetComponent<KitchenObject>().GetKitchenObjectSO().objectName);
	}

}
