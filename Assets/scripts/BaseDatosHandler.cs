using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class BaseDatosHandler : MonoBehaviour {

	public RootObject bd;
	public Text ano;
	public Text total;

	string testMovida = "movida otra";

	// Use this for initialization
	void Start () {
        string datos = File.ReadAllText(Application.dataPath + "/jsons/datos.json");
		bd = JsonUtility.FromJson<RootObject>(datos);
		//Debug.Log (this.buscarObjetoPorId (1999).ano);
	}

	void Update(){
		if (Input.GetAxis("Mouse ScrollWheel") != 0f ) // forward
		{
			int rand = Random.Range (0, 28);
			ano.text = "Años: " + bd.anos [rand].ano;
			total.text = ": " + bd.anos [rand].estudiantes.total;
//			Debug.Log ("años + " + bd.anos [rand].ano + " Total " + bd.anos [rand].estudiantes.total);
		}
	}
//
	public Ano buscarObjetoPorId(int id)
	{
		return bd.anos.Find (objecto => objecto.ano == id);
		//return bd.anos.Where(objeto => objeto.ano == id);
	}

}
