using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour{

	public virtual void Interact(Player player) {
		Debug.LogError("Interactable.Interact();");
	}

}
