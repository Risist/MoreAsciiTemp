using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Fraction Marker
 * Decides what attitude ai agents will have to each other
 */

[CreateAssetMenu(fileName = "Fraction", menuName = "Character/Ai/Fraction")]
public class AiFraction : ScriptableObject
{
	public enum Attitude
	{
		friendly,
		neutral,
		enemy,
		none
	}
	public Color fractionColorUi;
	public AiFraction[] friendlyFractions;
	public AiFraction[] enemyFractions;

	public Attitude GetAttitude(AiFraction fraction)
	{
		if (Equals(fraction))
			return Attitude.friendly;

		foreach (var it in friendlyFractions)
			if (it.Equals(fraction))
				return Attitude.friendly;

		foreach (var it in enemyFractions)
			if (it.Equals(fraction))
				return Attitude.enemy;

		return Attitude.neutral;
	}
}
