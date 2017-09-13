using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using System.Threading;


// =================================
// Classes.
// =================================

[RequireComponent (typeof(ParticleSystem))]
public class ParticlePlexus : MonoBehaviour
{
	public float maxDistance = 1.0f;
	public int maxConnections = 5;
	public int maxLineRenderers = 100;

	[Range (0.0f, 1.0f)]
	public float widthFromParticle = 0.125f;

	[Range (0.0f, 1.0f)]
	public float colourFromParticle = 1.0f;

	[Range (0.0f, 1.0f)]
	public float alphaFromParticle = 1.0f;

	new ParticleSystem particleSystem;

	ParticleSystem.Particle[] particles;

	Vector3[] particlePositions;

	Color[] particleColours;
	float[] particleSizes;

	ParticleSystem.MainModule particleSystemMainModule;

	public LineRenderer lineRendererTemplate;
	List<LineRenderer> lineRenderers = new List<LineRenderer> ();
	Transform _transform;

	[Header ("General Performance Settings")]

	[Range (0.0f, 1.0f)]
	public float delay = 0.0f;

	float timer;

	public bool alwaysUpdate = false;
	bool visible;

	///MIO
	[Range (0, 29)]
	private int rand;
	private int sumatiorio = 0;
	public int MaxParticleTotalAno;
	public Mesh SourceMeshObjectDos;
	private BaseDatosHandler bdHandler;
	private IEnumerator coroutine;
	private IEnumerator coroutineScroll;
	private float tiempo = 1f;
	// =================================
	// Functions.
	// =================================

	// ...

	void Start ()
	{
		GameObject plano = GameObject.Find ("Plane");
		bdHandler = plano.GetComponent<BaseDatosHandler>();

		rand = Random.Range (0, 28);
		MaxParticleTotalAno = bdHandler.bd.anos [rand].estudiantes.total;

		particleSystem = GetComponent<ParticleSystem> ();
		particleSystemMainModule = particleSystem.main;

		_transform = transform;

	}

	// ...

	void OnDisable ()
	{
		for (int i = 0; i < lineRenderers.Count; i++) {
			lineRenderers [i].enabled = false;
		}
	}

	// ...

	void OnBecameVisible ()
	{
		visible = true;
	}

	void OnBecameInvisible ()
	{
		visible = false;
	}

	// ...

	void LateUpdate ()
	{
		int lineRenderersCount = lineRenderers.Count;

		// In case max line renderers value is changed at runtime -> destroy extra.

		if (lineRenderersCount > maxLineRenderers) {
			for (int i = maxLineRenderers; i < lineRenderersCount; i++) {
				Destroy (lineRenderers [i].gameObject);
			}

			lineRenderers.RemoveRange (maxLineRenderers, lineRenderersCount - maxLineRenderers);
			lineRenderersCount -= lineRenderersCount - maxLineRenderers;
		}

		if (alwaysUpdate || visible) {
			// Prevent constant allocations so long as max particle count doesn't change.
			particleSystemMainModule.maxParticles = MaxParticleTotalAno;
			int maxParticles = particleSystemMainModule.maxParticles;

			if (particles == null || particles.Length < maxParticles) {
				particles = new ParticleSystem.Particle[maxParticles];

				particlePositions = new Vector3[maxParticles];

				particleColours = new Color[maxParticles];
				particleSizes = new float[maxParticles];
			}

			timer += Time.deltaTime;

			if (timer >= delay) {
				timer = 0.0f;

				int lrIndex = 0;

				// Only update if drawing/making connections.

				if (maxConnections > 0 && maxLineRenderers > 0) {
					particleSystem.GetParticles (particles);
					int particleCount = particleSystem.particleCount;

					float maxDistanceSqr = maxDistance * maxDistance;

					ParticleSystemSimulationSpace simulationSpace = particleSystemMainModule.simulationSpace;
					ParticleSystemScalingMode scalingMode = particleSystemMainModule.scalingMode;

					Transform customSimulationSpaceTransform = particleSystemMainModule.customSimulationSpace;

					Color lineRendererStartColour = lineRendererTemplate.startColor;
					Color lineRendererEndColour = lineRendererTemplate.endColor;

					float lineRendererStartWidth = lineRendererTemplate.startWidth * lineRendererTemplate.widthMultiplier;
					float lineRendererEndWidth = lineRendererTemplate.endWidth * lineRendererTemplate.widthMultiplier;

					// Save particle properties in a quick loop (accessing these is expensive and loops significantly more later, so it's better to save them once now).

					for (int i = 0; i < particleCount; i++) {
						if (i % 2 == 0) {
							particles [i].startColor = new Color (100, 200, 100, .5f);
						} 

						particlePositions [i] = particles [i].position;

						particleColours [i] = particles [i].GetCurrentColor (particleSystem);
						particleSizes [i] = particles [i].GetCurrentSize (particleSystem);
					}

					Vector3 p1p2_difference;

					// If in world space, there's no need to do any of the extra calculations... simplify the loop!

					if (simulationSpace == ParticleSystemSimulationSpace.World) {
						for (int i = 0; i < particleCount; i++) {
							if (lrIndex == maxLineRenderers) {
								break;
							}

							Color particleColour = particleColours [i];

							Color lineStartColour = Color.LerpUnclamped (lineRendererStartColour, particleColour, colourFromParticle);
							lineStartColour.a = Mathf.LerpUnclamped (lineRendererStartColour.a, particleColour.a, alphaFromParticle);

							float lineStartWidth = Mathf.LerpUnclamped (lineRendererStartWidth, particleSizes [i], widthFromParticle);

							int connections = 0;

							for (int j = i + 1; j < particleCount; j++) {
								p1p2_difference.x = particlePositions [i].x - particlePositions [j].x;
								p1p2_difference.y = particlePositions [i].y - particlePositions [j].y;
								p1p2_difference.z = particlePositions [i].z - particlePositions [j].z;

								//float distanceSqr = Vector3.SqrMagnitude(p1p2_difference);

								float distanceSqr =

									p1p2_difference.x * p1p2_difference.x +
									p1p2_difference.y * p1p2_difference.y +
									p1p2_difference.z * p1p2_difference.z;

								if (distanceSqr <= maxDistanceSqr) {
									LineRenderer lr;

									if (lrIndex == lineRenderersCount) {
										lr = Instantiate (lineRendererTemplate, _transform, false);

										lineRenderers.Add (lr);
										lineRenderersCount++;
									}

									lr = lineRenderers [lrIndex];
									lr.enabled = true;

									lr.SetPosition (0, particlePositions [i]);
									lr.SetPosition (1, particlePositions [j]);

									lr.startColor = lineStartColour;

									particleColour = particleColours [j];

									Color lineEndColour = Color.LerpUnclamped (lineRendererEndColour, particleColour, colourFromParticle);
									lineEndColour.a = Mathf.LerpUnclamped (lineRendererEndColour.a, particleColour.a, alphaFromParticle);

									lr.endColor = lineEndColour;
                                                
									lr.startWidth = lineStartWidth;
									lr.endWidth = Mathf.LerpUnclamped (lineRendererEndWidth, particleSizes [j], widthFromParticle);

									lrIndex++;
									connections++;

									if (connections == maxConnections || lrIndex == maxLineRenderers) {
										break;
									}
								}
							}
						}
					}
				}

				// Disable remaining line renderers from the pool that weren't used.

				for (int i = lrIndex; i < lineRenderersCount; i++) {
					if (lineRenderers [i].enabled) {
						lineRenderers [i].enabled = false;
					}
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			coroutine = EfectoInit (1f);
			StartCoroutine (coroutine);
		}

		if (Input.GetAxis ("Mouse ScrollWheel") != 0f) {
//			tiempo = 1f;
//			particleSystemMainModule.startSpeed = 1f;
//			coroutine = EfectoInit (1f);
//			StartCoroutine (coroutine);

			if(Input.GetAxis("Mouse ScrollWheel") > 0){
				sumatiorio++;
			} else {
				sumatiorio--;
			}


			Debug.Log (sumatiorio);
			int total = bdHandler.bd.anos [sumatiorio].estudiantes.total;
			var result = (int)Mathf.Lerp (442, 3000, Mathf.InverseLerp (1579, 10670, (int)total));
			MaxParticleTotalAno = result;
		}
		//particleSystem.SetParticles(particles, particles.Length);

	}


	private IEnumerator EfectoInit (float waitTime)
	{
		while (true) {
			if (particleSystemMainModule.startSpeed.constant > 0f) {
				tiempo -= 0.09f;
				if (particleSystemMainModule.startSpeed.constant <= 0.2f) {
					particleSystemMainModule.startLifetime = 4f;
				}
				particleSystemMainModule.startSpeed = tiempo;
			} else {
				//var sh = particleSystem.shape;
				//sh.shapeType = ParticleSystemShapeType.Sphere;
				particleSystemMainModule.startSpeed = 0.03f;
				Debug.Log ("stop");
				StopCoroutine (coroutine);
			}

			yield return new WaitForSeconds (waitTime);
			print ("WaitAndPrint " + Time.time);
		}
	}

	// =================================
	// End functions.
	// =================================

}

