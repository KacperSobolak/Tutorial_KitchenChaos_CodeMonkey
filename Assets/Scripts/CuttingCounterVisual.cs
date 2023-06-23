using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour{

	private const string CUT = "Cut";

	[SerializeField] private CuttingCounter containerCounter;

	private Animator animator;

	private void Awake() {
		animator = GetComponent<Animator>();
	}

	private void Start() {
		containerCounter.OnCut += ContainerCounter_OnCut;
	}

	private void ContainerCounter_OnCut(object sender, System.EventArgs e) {
		animator.SetTrigger(CUT);
	}
}
