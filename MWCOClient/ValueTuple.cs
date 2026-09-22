using System;
using System.Runtime.CompilerServices;

public class ValueTuple<T1, T2, T3>
{
	public T1 pickupable
	{
		[CompilerGenerated]
		get
		{
			return this.<pickupable>k__BackingField;
		}
	}

	public T2 isFrozen
	{
		[CompilerGenerated]
		get
		{
			return this.<isFrozen>k__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.<isFrozen>k__BackingField = value;
		}
	}

	public T3 parent
	{
		[CompilerGenerated]
		get
		{
			return this.<parent>k__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.<parent>k__BackingField = value;
		}
	}

	public ValueTuple(T1 i1, T2 i2, T3 i3)
	{
		this.pickupable = i1;
		this.isFrozen = i2;
		this.parent = i3;
	}

	[CompilerGenerated]
	private readonly T1 <pickupable>k__BackingField;

	[CompilerGenerated]
	private T2 <isFrozen>k__BackingField;

	[CompilerGenerated]
	private T3 <parent>k__BackingField;
}
