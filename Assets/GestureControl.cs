using UnityEngine;
using System.Collections;
using TouchScript.Gestures;
using TouchScript.Hit;
using DG.Tweening;

public class GestureControl : MonoBehaviour {
	public ScreenTransformGesture transformGesture;
	public TapGesture singleTap;
	public TapGesture doubleTap;
	public FlickGesture flick;
	private Animator animator;
	private int currentChar;
	private int changeChar;
	private bool flickable;
	private GameObject[] characters;
	public Camera cam;
	public LayerMask msk;

	private float accelerometerUpdateInterval = 1.0f / 60.0f;
	// The greater the value of LowPassKernelWidthInSeconds, the slower the
	// filtered value will converge towards current input sample (and vice versa).
	private float lowPassKernelWidthInSeconds = 1.0f;
	// This next parameter is initialized to 2.0 per Apple's recommendation,
	// or at least according to Brady! ;)
	private float shakeDetectionThreshold = 2.0f;

	private float lowPassFilterFactor;
	private float actionTime;
	Vector3 lowPassValue;
	void Update()
	{
		Vector3 acceleration = Input.acceleration;
		lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
		Vector3 deltaAcceleration = acceleration - lowPassValue;
		actionTime -= Time.deltaTime;
		if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold && actionTime <0.0f)
		{
			// Perform your "shaking actions" here. If necessary, add suitable
			// guards in the if check above to avoid redundant handling during
			// the same shake (e.g. a minimum refractory period).
			actionTime = 1.2f;
			animator.SetTrigger ("Shake");
		}
	}
	// Use this for initialization
	void Start () {
		actionTime = 0.0f;
		shakeDetectionThreshold *= shakeDetectionThreshold;
		lowPassValue = Input.acceleration;
		lowPassFilterFactor = accelerometerUpdateInterval /
			lowPassKernelWidthInSeconds;

		flickable = true;
		characters = GameObject.FindGameObjectsWithTag ("Chars");
		animator = characters [0].GetComponent<Animator> ();
		for(int i = 1; i < characters.Length; i++){
			characters [i].SetActive (false);
		}
		Debug.Log (characters.Length);

		transformGesture.Transformed += (object sender, System.EventArgs e) => 
		{
			this.transform.Rotate(new Vector3(0,1,0),transformGesture.DeltaPosition[0]*1.0f);
			this.transform.localScale /= transformGesture.DeltaScale;
		};

		singleTap.Tapped += (object sender, System.EventArgs e) => 
		{
			TouchHit hit;
			singleTap.GetTargetHitResult(out hit);

			if(Physics.Raycast(new Ray(cam.transform.position,hit.Point-cam.transform.position),float.MaxValue,msk)){
				animator.SetTrigger("SingleTap");
			}

		};

		doubleTap.Tapped += (object sender, System.EventArgs e) => 
		{
			TouchHit hit;
			doubleTap.GetTargetHitResult(out hit);

			if(Physics.Raycast(new Ray(cam.transform.position,hit.Point-cam.transform.position),float.MaxValue,msk)){
				animator.SetTrigger("DoubleTap");
			}
		};

		flick.Flicked += (object sender, System.EventArgs e) => 
		{
			if(flickable){
				flickable = false;
				FlickGesture gesture = sender as FlickGesture;
				bool toLeft = (gesture.ScreenFlickVector.x < 0);
				this.transform.rotation = new Quaternion(0,0,0,0);
				if(toLeft){
					Debug.Log("to left");
					changeChar = (currentChar+1)%characters.Length;
					//animator.SetTrigger("Run");
					characters[currentChar].transform.DOLocalMoveX(-2f, 1f).SetEase(Ease.InOutSine);
					characters[changeChar].transform.localPosition = new Vector3(4f,0f,0f);
					characters[changeChar].SetActive(true);
					characters[changeChar].transform.DOLocalMoveX(0f, 1f).SetEase(Ease.InOutSine);
					Invoke("AfterFlick", 1f);
				}else{
					Debug.Log("to right");
					changeChar = (currentChar-1+characters.Length)%characters.Length;
					characters[currentChar].transform.DOLocalMoveX(2f, 1f).SetEase(Ease.InOutSine);
					characters[changeChar].transform.localPosition = new Vector3(-4f,0f,0f);
					characters[changeChar].SetActive(true);
					characters[changeChar].transform.DOLocalMoveX(0f, 1f).SetEase(Ease.InOutSine);
					Invoke("AfterFlick", 1f);
				}
			}
		};
	}
	private void AfterFlick()
	{
		characters[currentChar].SetActive(false);
		currentChar = changeChar;
		flickable = true;
		animator = characters [currentChar].GetComponent<Animator> ();
	}

}
