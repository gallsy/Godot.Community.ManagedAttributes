using System;
using Newtonsoft.Json;
using Godot;

namespace Godot.Community.ManagedAttributes;

public class IntManagedAttribute : ManagedAttribute<int>
{
    [JsonProperty]
    protected int CurrentValue
    {
        get => currentValue;
        set => SetValue(ref currentValue, value);
    }
    private int currentValue;

    [JsonProperty]
    protected int MaxValue
    {
        get => maxValue;
        set => SetValue(ref maxValue, value);
    }
    private int maxValue;

    [JsonProperty]
    protected int MinValue
    {
        get => minValue;
        set => SetValue(ref minValue, value);
    }
    private int minValue;

    [JsonProperty]
    protected int RegenValue
    {
        get => regenValue;
        set => SetValue(ref regenValue, value);
    }
    private int regenValue;

    public IntManagedAttribute()
    {
        CurrentValue = 0;
        MinValue = int.MinValue;
        MaxValue = int.MaxValue;
    }

    public IntManagedAttribute(int val)
    {
        CurrentValue = val;
        MinValue = int.MinValue;
        MaxValue = int.MaxValue;
    }

    public IntManagedAttribute(int min, int val, int max)
    {
        CurrentValue = val;
        MinValue = min;
        MaxValue = max;
    }

    public int ObjectToInt(object obj)
    {
        if(obj is int intObj)
        {
            return intObj;
        }
        if(obj is Int32 int32Obj)
        {
            return (int)int32Obj;
        }
        if(obj is float floatObj)
        {
            return (int)floatObj;
        }
        if(obj is string stringObj)
        {
            if (int.TryParse(stringObj, out int retValue))
            {
                return retValue;
            }
        }

        throw new InvalidCastException($"Invalid type. Expected int got {obj.GetType().Name}");
    }

    public override void Set(object val, AttributeValueType valType = AttributeValueType.Value)
    {
        var numVal = ObjectToInt(val);
        _ = valType switch
        {
            AttributeValueType.Min => SetMinValue(numVal),
            AttributeValueType.Value => SetCurrentValue(numVal),
            AttributeValueType.Max => SetMaxValue(numVal),
            AttributeValueType.Regen => SetRegenValue(numVal),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null)
        };
        GD.Print($"ValSetEnd: {Get(valType)}");
    }

    protected override int Get(AttributeValueType valType = AttributeValueType.Value)
    {
        int retValue = valType switch
        {
            AttributeValueType.Min => Calculate(AttributeValueType.Min),
            AttributeValueType.Value => Calculate(AttributeValueType.Value),
            AttributeValueType.Max => Calculate(AttributeValueType.Max),
            AttributeValueType.Regen => Calculate(AttributeValueType.Regen),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null)
        };
        return retValue;
    }

    private int Calculate(AttributeValueType valType = AttributeValueType.Value)
    {
        var result = GetRaw(valType);
        foreach (var m in modifiers)
        {
            if (m.ModifierValues.ContainsKey(valType) && m.ModifierValues[valType] is ManagedAttributeModifierValue mamv)
            {
                result += mamv.Add;
                result = (int)(mamv.Multiplier * (float)result);
            }
        }

        return result;
    }

    protected override int GetRaw(AttributeValueType valType = AttributeValueType.Value)
    {
        int retValue = valType switch
        {
            AttributeValueType.Min => MinValue,
            AttributeValueType.Value => CurrentValue,
            AttributeValueType.Max => MaxValue,
            AttributeValueType.Regen => RegenValue,
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null)
        };
        return retValue;
    }

    public override void Add(object val, AttributeValueType valType = AttributeValueType.Value)
    {
        var numVal = ObjectToInt(val);
        _ = valType switch
        {
            AttributeValueType.Min => SetMinValue(MinValue + numVal),
            AttributeValueType.Value => SetCurrentValue(CurrentValue + numVal),
            AttributeValueType.Max => SetMaxValue(MaxValue + numVal),
            AttributeValueType.Regen => SetRegenValue(RegenValue + numVal),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null)
        };
    }

    protected int Clamp(int val)
    {
        if(val > Calculate(AttributeValueType.Max))
        {
            val = Calculate(AttributeValueType.Max);
        }
        if(val < Calculate(AttributeValueType.Min))
        {
            val = Calculate(AttributeValueType.Min);
        }
        return val;
    }

    protected int SetMinValue(int val)
    {
        var originalValue = MinValue;
        MinValue = val;
        var raiseChanged = originalValue != MinValue;
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

    protected int SetCurrentValue(int val)
    {
        var originalValue = CurrentValue;
        CurrentValue = Clamp(val);
        if (originalValue != CurrentValue)
        {
            RaiseHasChanged();
        }
        return CurrentValue;
    }

    protected int SetMaxValue(int val)
    {
        var originalMaxValue = MaxValue;
        MaxValue = val;
        var raiseChanged = originalMaxValue != MaxValue;
        if(CurrentValue > MaxValue)
        {
            CurrentValue = MaxValue;
            raiseChanged = true;
        }
        if(MinValue > MaxValue)
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
    
    protected int SetRegenValue(int val)
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
        if(mod.ModifierValues.ContainsKey(AttributeValueType.Max) && CurrentValue > Get(AttributeValueType.Max))
        {
            CurrentValue = Get(AttributeValueType.Max);
        }
        if (mod.ModifierValues.ContainsKey(AttributeValueType.Max) && CurrentValue < Get(AttributeValueType.Min))
        {
            CurrentValue = Get(AttributeValueType.Min);
        }
    }
}

