using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour {
    private void Start() {
        GameManager.Instance.OnLocalPlayerReadyChange += GameManager_OnOnLocalPlayerReadyChange;
        GameManager.Instance.OnStateChanged += GameManager_OnOnStateChanged;

        Hide();
    }

    private void GameManager_OnOnStateChanged(object sender, EventArgs e) {
        if (GameManager.Instance.IsCountdownToStartActive()) {
            Hide();
        }
    }

    private void GameManager_OnOnLocalPlayerReadyChange(object sender, EventArgs e) {
        if (GameManager.Instance.IsLocalPlayerReady()) {
            Show();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
