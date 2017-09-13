using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class Estudiantes
{
	public int total;
	public int mujeres;
	public int hombres;
}

[System.Serializable]
public class Total
{
	public string nombre;
	public int total;
	public int mujeres;
	public int hombres;
}

[System.Serializable]
public class Ano
{
	public int ano;
	public string Curso;
	public Estudiantes estudiantes;
	public List<Total> total;
}

[System.Serializable]
public class RootObject
{
	public List<Ano> anos;
}