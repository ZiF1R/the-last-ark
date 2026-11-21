using System;
using UnityEngine;

[Serializable]
public class Choice
{
	public string label;
	[TextArea] public string result;
	public int survival;
	public int science;
	public int morale;
	public string setFlag;
}
