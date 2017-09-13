//This code can be used for private or commercial projects but cannot be sold or redistributed without written permission.
//Copyright Nik W. Kraus / Dark Cube Entertainment LLC. 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (ParticleSystem))]

public class HologramControlShurikan_C : MonoBehaviour {

	public bool Enable = true;

	[Range (0.0f, 0.2f)]
	public float JitterAmount = 0.0f;
	
	[Range (0.1f, 8.0f)]
	public float ParticleSpeed = 0.5f;

	[Range (1, 2000)]
	public int ParticleRate = 100;
	public bool UseRandomColor = false;

	//[Space (10, order=0)]

	[Tooltip("When on particles do not die. When off, particle system life setting is used. Turn off to use Evap or Dissolve.")]
	public bool ParticleStatic = true;

	public enum ParticleEnd { Dissolve, Evap}
	public ParticleEnd ParticleEndType;

	[Tooltip ("Turn off Particle Static to use End type")]
	public Vector3 EvapDir = new Vector3(0.0f, 0.2f, 0.0f);
	
	private Mesh CreateMesh;
	public Mesh SourceMeshObject;
	public Mesh SourceMeshObjectDos;

	
	[Tooltip ("Position that particles gather in world space. If none assigned position default is this object.")]
	public Transform GatherPos;
	private Vector3 GatherT;
	private Quaternion GatherR;
	
	[Tooltip ("Used to scale the mesh if needed. 1 = default mesh scale")]
	public float GatherScale = 1.0f;
	
	private ParticleSystem PartSystem;
	private ParticleSystem.Particle[] particles;
	private int TotalPart;
	
	private float CheckTimer = 0.0f;
	private float Timer = .3f;
	
	private Vector3[] newVertices;
	private Vector3[] dosVertices;
	private int[] newTriangles;
	private Vector3 PosP;
	private Vector3 dosPosP;
	public Transform desplazar;
	private GameObject Persona;

	//Start
	void Start () {
		Persona = GameObject.Find ("Persona");
		PartSystem = gameObject.GetComponent<ParticleSystem>();
		PartSystem.startSpeed = 0;
		PartSystem.simulationSpace = ParticleSystemSimulationSpace.World;

		CheckTimer = Time.time;
		
		///this is to help fix a Unity particle render issue
//		gameObject.transform.localScale = new Vector3(3,3,3);
		gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

		if(SourceMeshObject){
			newVertices = SourceMeshObject.vertices;
		}		
		else{
			Debug.Log("No Mesh assigned, please assign one.");
		}
			
		
		///Gather position
		if(GatherPos){
			GatherT = GatherPos.position;
			GatherR = GatherPos.rotation;
		}
		else{
			GatherT = gameObject.transform.position;
			GatherR = gameObject.transform.rotation;
			Debug.Log("No Gather Position transform assigned, this game object position will be used by default.");
		}
		
		//Model Vertex density check
		if(newVertices.Length > 9000){
			Debug.Log("Number of Verts is " + newVertices.Length + " ... suggest assigning a less complex model.");
		}
		
		particles = new ParticleSystem.Particle[PartSystem.particleCount];
		PartSystem.maxParticles = newVertices.Length;	
		
	}///End Start
	
	

	void LateUpdate () {
	
		if(SourceMeshObject){
			newVertices = SourceMeshObject.vertices;
		}		
		
		if(GatherPos){
			GatherT = GatherPos.position;
			GatherR = GatherPos.rotation;
		}
		else {
			GatherT = gameObject.transform.position;
			GatherR = gameObject.transform.rotation;
		}	
		
		
		particles = new ParticleSystem.Particle[PartSystem.particleCount];
		TotalPart = PartSystem.GetParticles(particles);
		
		//Particle settings
		PartSystem.emissionRate = ParticleRate;
		PartSystem.maxParticles = newVertices.Length;


		//Loop through particles
		for(var i = 0; i < TotalPart; i++){
			if(Enable){					
				var PartAge = (particles[i].startLifetime - particles[i].remainingLifetime)/particles[i].startLifetime;
				
				if(ParticleStatic){
					particles[i].remainingLifetime = 5;
				}
				
				if(JitterAmount > 0){
					particles[i].position += new Vector3(Random.Range(-JitterAmount,JitterAmount),Random.Range(-JitterAmount,JitterAmount),Random.Range(-JitterAmount,JitterAmount)) * Mathf.Sin(Time.time);
				}
			
				if(newVertices.Length >= TotalPart){
					PosP = (GatherR*newVertices[i]+GatherT*0);
				}
				else{
					PosP = GatherT;
				}

				if(UseRandomColor){
					if(Random.Range(0,TotalPart) == i){
						particles[i].color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),1);

					}
				}
				
				if(!ParticleStatic){
					if(ParticleEndType == ParticleEnd.Evap){
						if(PartAge > particles[i].remainingLifetime-.5){								
							particles[i].position = Vector3.Lerp(particles[i].position, particles[i].position + EvapDir, Time.smoothDeltaTime*ParticleSpeed);
						}
						else{
							particles[i].position = Vector3.Lerp(particles[i].position, new Vector3(PosP.x*GatherScale,PosP.y*GatherScale,PosP.z*GatherScale) + GatherT, Time.smoothDeltaTime*ParticleSpeed);
						}							
					}
					else if(ParticleEndType == ParticleEnd.Dissolve){
						particles[i].remainingLifetime = particles[i].remainingLifetime-Random.Range(.01f,.1f);
						particles[i].position = Vector3.Lerp(particles[i].position, new Vector3(PosP.x*GatherScale,PosP.y*GatherScale,PosP.z*GatherScale) + GatherT, Time.smoothDeltaTime*ParticleSpeed);
					}
					else{
						particles[i].position = Vector3.Lerp(particles[i].position, new Vector3(PosP.x * GatherScale, PosP.y * GatherScale, PosP.z * GatherScale) + GatherT, Time.smoothDeltaTime*ParticleSpeed);
					}
				}
				else{
					particles[i].position = Vector3.Lerp(particles[i].position, new Vector3(PosP.x*GatherScale,PosP.y*GatherScale,PosP.z*GatherScale) + GatherT, Time.smoothDeltaTime*ParticleSpeed);
				}
			}/////End Enable
			else{
				if(!ParticleStatic && ParticleEndType == ParticleEnd.Dissolve){
					particles[i].remainingLifetime = particles[i].remainingLifetime-.5f;
				}
				else if(!ParticleStatic && ParticleEndType == ParticleEnd.Evap){
					particles[i].remainingLifetime = Mathf.Lerp(particles[i].remainingLifetime,0.0f,.1f);
				}
			}
		}////end loop

		if (Input.GetKeyDown (KeyCode.Space)) {
			
			ParticleStatic = false;
			var sh = PartSystem.shape;
			sh.shapeType = ParticleSystemShapeType.Circle;
//			var main = PartSystem.main;
//			main.startSize = hSliderValue;
//			sh.enabled = true;
			//PartSystem.transform.LookAt (Persona.transform);

			SourceMeshObject = SourceMeshObjectDos;
			Persona.transform.position = desplazar.transform.position;
			//JitterAmount = 0.1f;
			for (var ii = 0; ii < SourceMeshObjectDos.vertices.Length; ii++) {
				if(ii == 300){
					ParticleStatic = true;
				}
//				if(i <=dosVertices.Length ){
//					Debug.Log ("movida");
//					dosPosP = (GatherR*dosVertices[i]+GatherT*2);
//					particles[i].position += dosVertices[i];
//				}
				if (ii % 2 == 0) {
					particles[ii].color = new Color(Random.Range(0.0f,2.0f),Random.Range(0.0f,2.0f),Random.Range(0.0f,1.0f),1);
				}
//				particles[i].remainingLifetime = particles[i].remainingLifetime-Random.Range(.01f,.1f);
				//particles[i].position += Vector3.Lerp(new Vector3(dosPosP.x*GatherScale,dosPosP.y*GatherScale,dosPosP.z*GatherScale) + GatherT, particles[i].position, Time.smoothDeltaTime*ParticleSpeed);
				//particles[i].position = Vector3.MoveTowards(particles[i].position,  new Vector3(Random.Range(-2,0.0f), Random.Range(-0f,2f), Random.Range(-0f,0f)), Time.deltaTime * 0.05f);
				//particles[i].position += new Vector3(Random.Range(-JitterAmount,JitterAmount),Random.Range(-JitterAmount,JitterAmount),Random.Range(-JitterAmount,JitterAmount)) * Mathf.Sin(Time.time);
//				particles[i].position  = Vector3.MoveTowards(particles[i].position, 
//					new Vector3(2, 3, 2), 
//					Time.deltaTime * 5f);
			}
			Debug.Log("Space key was pressed.");
		}

		PartSystem.SetParticles(particles, particles.Length);
		
	} ///////End Update

}//End Class
