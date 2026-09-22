using System;
using System.Runtime.CompilerServices;

public class ValueDuple<T1, T2>
{
	public T1 first
	{
		[CompilerGenerated]
		get
		{
			return this.<first>k__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.<first>k__BackingField = value;
		}
	}

	public T2 second
	{
		[CompilerGenerated]
		get
		{
			return this.<second>k__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.<second>k__BackingField = value;
		}
	}

	public ValueDuple(T1 i1, T2 i2)
	{
		this.first = i1;
		this.second = i2;
	}

	public static ValueDuple<T1, T2> Create(T1 i1, T2 i2)
	{
		return new ValueDuple<T1, T2>(i1, i2);
	}

	[CompilerGenerated]
	private T1 <first>k__BackingField;

	[CompilerGenerated]
	private T2 <second>k__BackingField;
}
