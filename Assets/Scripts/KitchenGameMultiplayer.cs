using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameMultiplayer : NetworkBehaviour {
    
    public static KitchenGameMultiplayer Instance { get; private set; }

    [SerializeField] private KitchenObjectSOList kitchenObjectSOList;

    private void Awake() {
        Instance = this;
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) {
        SpawnKitchenObjectServerRpc(kitchenObjectSO, kitchenObjectParent);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, IKitchenObjectParent kitchenObjectParent) {
        KitchenObjectSO kitchenObjectSO = kitchenObjectSOList.kitchenObjectSOList[kitchenObjectSOIndex];
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);
    
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }
}
