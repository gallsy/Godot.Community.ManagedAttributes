using System;
using System.Collections.Generic;
using Godot;

namespace Godot.Community.ManagedAttributes;

public class ManagedAttributeModifier
{
    public delegate void AttributeModifierElapsedHandler(ManagedAttributeModifier modifier);
    public event AttributeModifierElapsedHandler AttributeModifierElapsed;

    public Dictionary<AttributeValueType, ManagedAttributeModifierValue> ModifierValues { get; set; } = new();
    public ulong ApplyTick { get; set; }
    public ulong ExpiryTick { get; set; }
    public Guid Id = new();

    public void OnModifierElapsed()
    {
        AttributeModifierElapsed?.Invoke(this);
    }
}

public class ManagedAttributeModifierValue
{
    public int Add { get; set; } = 0;
    public float Multiplier { get; set; } = 1f;
}
