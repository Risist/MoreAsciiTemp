using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Prototype
/*
 * 
 * BehaviourHolder has a tag describing what kind of ability it is
 * 
 * Character has perceived state describing what state is going on
 * 
 */

// what ability does
public enum EBehaviourType
{
	ENoFlag			= 0,
	EAtack			= 1 << 0,
	EDash			= 1 << 1,
	EForward		= 1 << 2,
	ESide			= 1 << 3,
	EBackwards		= 1 << 4,
	EBlock			= 1 << 5,
	ETargetSearch	= 1 << 6,
	EClose			= 1 << 7,
	EFar			= 1 << 8, 
}

public enum EBehaviourState
{
	EDanger,
	ENoise,
	EClose,
	EFar,
}
#endregion

public struct WorldState
{
	/// creates a new WorldState with by default no flag set
	public WorldState(UInt64 fSet = 0, UInt64 fUnset = 0)
	{
		_flags = 0;
		_mask = 0;
		UnsetFlags(fUnset);
		SetFlags(fSet);
	}

	public void SetFlags(UInt64 f)
	{
		_flags = _flags | f;
		_mask = _mask | f;
	}
	public void UnsetFlags(UInt64 f)
	{
		_flags = _flags & (~f);
		_mask = _mask | f;
	}
	public void SetBit(int i)
	{
		UInt64 bit = (1UL << i);
		_flags = _flags | bit;
		_mask = _mask | bit;
	}
	public void UnsetBit(int i)
	{
		UInt64 bit = (1UL << i);
		_flags = _flags & (~bit);
		_mask = _mask | bit;
	}


	/// checks whether other fillfils requirements described by this world state
	public bool IsFulfiledBy(ref WorldState other)
	{
		UInt64 f1 = _flags & _mask;
		UInt64 f2 = other._flags & _mask;
		UInt64 s = f1 ^ f2;
		bool r = s == 0;
		bool eq = (_mask & other._mask) == _mask;

		return r && eq;
	}

	/// returns owerwritten state by other
	public WorldState GetAppliedChange(ref WorldState other)
	{
		WorldState state = new WorldState();
		state._flags = (_flags & (~other._mask)) |
						(other._flags & other._mask);
		state._mask = _mask | other._mask;
		return state;
	}

	/// heuristic measure of distance
	/// how many states has to change in order to fullfil given goal
	public UInt64 GetDistance(ref WorldState goal)
	{
		UInt64 d = (_flags & goal._mask) ^ (goal._flags & goal._mask);
		UInt64 sum = 0;
		while (d > 0)
		{
			sum += d & 1;
			d = d >> 1;
		}

		return sum;
	}


	// flags set in the world state; note not all flags are relevant
	UInt64 _flags;

	// which flags are relevant to this world state
	UInt64 _mask;
}
