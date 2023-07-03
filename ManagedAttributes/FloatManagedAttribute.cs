using System;
using Newtonsoft.Json;

namespace Godot.Community.ManagedAttributes;

public class FloatManagedAttribute : ManagedAttribute<float>
{
    [JsonProperty]
    protected float CurrentValue
    {
        get => currentValue;
        set => SetValue(ref currentValue, value);
    }
    private float currentValue;

    [JsonProperty]
    protected float MaxValue
    {
        get => maxValue;
        set => SetValue(ref maxValue, value);
    }
    private float maxValue;

    [JsonProperty]
    protected float MinValue
    {
        get => minValue;
        set => SetValue(ref minValue, value);
    }
    private float minValue;

    [JsonProperty]
    protected float RegenValue
    {
        get => regenValue;
        set => SetValue(ref regenValue, value);
    }
    private float regenValue;

    public FloatManagedAttribute()
    {
        CurrentValue = 0;
        MinValue = float.MinValue;
        MaxValue = float.MaxValue;
    }

    public FloatManagedAttribute(float val)
    {
        CurrentValue = val;
        MinValue = float.MinValue;
        MaxValue = float.MaxValue;
    }

    public FloatManagedAttribute(float min, float val, float max)
    {
        CurrentValue = val;
        MinValue = min;
        MaxValue = max;
    }

    public float ObjectToFloat(object obj)
    {
        if (obj is int intObj)
        {
            return (float)intObj;
        }
        if (obj is Int32 int32Obj)
        {
            return (float)int32Obj;
        }
        if (obj is float floatObj)
        {
            return floatObj;
        }
        if(obj is string stringObj)
        {
            if (float.TryParse(stringObj, out float retValue))
            {
                return retValue;
            }
        }

        throw new InvalidCastException($"Invalid type. Expected float, got {obj.GetType().Name}");
    }

    public override void Set(object val, AttributeValueType valType = AttributeValueType.Value)
    {
        var numVal = ObjectToFloat(val);
        float retValue = valType switch 
        {
            AttributeValueType.Min => SetMinValue(numVal),
            AttributeValueType.Value => SetValue(numVal),
            AttributeValueType.Max => SetMaxValue(numVal),
            AttributeValueType.Regen => SetRegenValue(numVal),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null),
        };
    }

    protected override float Get(AttributeValueType valType = AttributeValueType.Value)
    {
        float retValue = valType switch 
        {
            AttributeValueType.Min => Calculate(AttributeValueType.Min),
            AttributeValueType.Value => Calculate(AttributeValueType.Value),
            AttributeValueType.Max => Calculate(AttributeValueType.Max),
            AttributeValueType.Regen => Calculate(AttributeValueType.Regen),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null),
        };
        return retValue;
    }

    private float Calculate(AttributeValueType valType = AttributeValueType.Value)
    {
        var result = GetRaw(valType);
        foreach (var m in modifiers)
        {
            if (m.ModifierValues.ContainsKey(valType) && m.ModifierValues[valType] is ManagedAttributeModifierValue mamv)
            {
                result += mamv.Add;
                result = mamv.Multiplier * result;
            }
        }

        return result;
    }

    protected override float GetRaw(AttributeValueType valType = AttributeValueType.Value)
    {
        float retValue = valType switch
        {
            AttributeValueType.Min => MinValue,
            AttributeValueType.Value => CurrentValue,
            AttributeValueType.Max => MaxValue,
            AttributeValueType.Regen => RegenValue,
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null),
        };
        return retValue;
    }

    public override void Add(object val, AttributeValueType valType = AttributeValueType.Value)
    {
        var numVal = ObjectToFloat(val);
        float retValue = valType switch 
        {
            AttributeValueType.Min => SetMinValue(MinValue + numVal),
            AttributeValueType.Value => SetValue(CurrentValue + numVal),
            AttributeValueType.Max => SetMaxValue(MaxValue + numVal),
            AttributeValueType.Regen => SetRegenValue(RegenValue + numVal),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null),
        };
    }

    protected float Clamp(float val)
    {
        if(val > MaxValue)
        {
            val = MaxValue;
        }
        if(val < MinValue)
        {
            val = MinValue;
        }
        return CurrentValue;
    }

    protected float SetMinValue(float val)
    {
        var raiseChanged = MinValue != val;
        MinValue = val;
        if(CurrentValue < MinValue)
        {
            CurrentValue = MinValue;
            raiseChanged = true;
        }
        if(MaxValue < MinValue)
        {
            MaxValue = MinValue;
            raiseChanged = true;
        }
        if (raiseChanged)
        {
            RaiseHasChanged();
        }
        return MinValue;
    }

    protected float SetValue(float val)
    {
        var originalValue = CurrentValue;
        CurrentValue = Clamp(val);
        if (originalValue != CurrentValue)
        {
            RaiseHasChanged();
        }
        return CurrentValue;
    }

    protected float SetMaxValue(float val)
    {
        var raiseChanged = MaxValue != val;
        MaxValue = val;
        if (CurrentValue > MaxValue)
        {
            CurrentValue = MaxValue;
            raiseChanged = true;
        }
        if (MinValue > MaxValue)
        {
            MinValue = MaxValue;
            raiseChanged = true;
        }
        if (raiseChanged)
        {
            RaiseHasChanged();
        }
        return MaxValue;
    }

    protected float SetRegenValue(float val)
    {
        RegenValue = val;
        return RegenValue;
    }

    public override void Update(ulong tick)
    {
        base.Update(tick);

        //Regen
        var originalVal = CurrentValue;
        CurrentValue = Clamp(CurrentValue + RegenValue);
        if (originalVal != CurrentValue)
        {
            RaiseHasChanged();
        }
    }

    protected override void OnModifierAdded(ManagedAttributeModifier mod)
    {
        if (mod.ModifierValues.ContainsKey(AttributeValueType.Max) && CurrentValue > Get(AttributeValueType.Max))
        {
            CurrentValue = Get(AttributeValueType.Max);
        }
        if (mod.ModifierValues.ContainsKey(AttributeValueType.Max) && CurrentValue < Get(AttributeValueType.Min))
        {
            CurrentValue = Get(AttributeValueType.Min);
        }
    }
}