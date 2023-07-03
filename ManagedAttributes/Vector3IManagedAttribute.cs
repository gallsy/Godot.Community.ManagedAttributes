using System;
using Godot;
using Newtonsoft.Json;

namespace Godot.Community.ManagedAttributes;

public class Vector3IManagedAttribute : ManagedAttribute<Vector3I>
{
    [JsonProperty]
    protected Vector3I CurrentValue
    {
        get => currentValue;
        set => SetValue(ref currentValue, value);
    }
    private Vector3I currentValue;

    [JsonProperty]
    protected Vector3I MaxValue
    {
        get => maxValue;
        set => SetValue(ref maxValue, value);
    }
    private Vector3I maxValue;

    [JsonProperty]
    protected Vector3I MinValue
    {
        get => minValue;
        set => SetValue(ref minValue, value);
    }
    private Vector3I minValue;

    [JsonProperty]
    protected Vector3I RegenValue
    {
        get => regenValue;
        set => SetValue(ref regenValue, value);
    }
    private Vector3I regenValue;

    public Vector3IManagedAttribute()
    {
        CurrentValue = Vector3I.Zero;
        MinValue = new Vector3I(int.MinValue, int.MinValue, int.MinValue);
        MaxValue = new Vector3I(int.MaxValue, int.MaxValue, int.MaxValue);
    }

    public Vector3IManagedAttribute(Vector3I val)
    {
        CurrentValue = val;
        MinValue = new Vector3I(int.MinValue, int.MinValue, int.MinValue);
        MaxValue = new Vector3I(int.MaxValue, int.MaxValue, int.MaxValue);
    }

    public Vector3IManagedAttribute(Vector3I min, Vector3I val, Vector3I max)
    {
        CurrentValue = val;
        MinValue = min;
        MaxValue = max;
    }

    public Vector3I ObjectToVector3I(object obj)
    {
        if (obj is Vector3I vecObj)
        {
            return vecObj;
        }
        if(obj is string stringObj)
        {
            var regex = new System.Text.RegularExpressions.Regex(@"\(([-\d]+), ([-\d]+), ([-\d]+)\)");
            var match = regex.Match(stringObj);
            if (match.Success)
            {
                var x = int.Parse(match.Groups[1].Value);
                var y = int.Parse(match.Groups[2].Value);
                var z = int.Parse(match.Groups[3].Value);
                return new Vector3I(x, y, z);
            }
        }

        throw new InvalidCastException($"Invalid type. Expected int got {obj.GetType().Name}");
    }

    public override void Set(object val, AttributeValueType valType = AttributeValueType.Value)
    {
        var numVal = ObjectToVector3I(val);
        Vector3I retValue = valType switch
        {
            AttributeValueType.Min => SetMinValue(numVal),
            AttributeValueType.Value => SetValue(numVal),
            AttributeValueType.Max => SetMaxValue(numVal),
            AttributeValueType.Regen => SetRegenValue(numVal),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null),
        };
        GD.Print($"ValSetEnd: {Get(valType)}");
    }

    protected override Vector3I Get(AttributeValueType valType = AttributeValueType.Value)
    {
        Vector3I retValue = valType switch
        {
            AttributeValueType.Min => Calculate(AttributeValueType.Min),
            AttributeValueType.Value => Calculate(AttributeValueType.Value),
            AttributeValueType.Max => Calculate(AttributeValueType.Max),
            AttributeValueType.Regen => Calculate(AttributeValueType.Regen),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null),
        };
        return retValue;
    }

    private Vector3I Calculate(AttributeValueType valType = AttributeValueType.Value)
    {
        var result = GetRaw(valType);
        foreach (var m in modifiers)
        {
            if (m.ModifierValues.ContainsKey(valType) && m.ModifierValues[valType] is ManagedAttributeModifierValue mamv)
            {
                result.X += mamv.Add;
                result.Y += mamv.Add;
                result.Z += mamv.Add;

                result.X = (int)(result.X * mamv.Multiplier);
                result.Y = (int)(result.Y * mamv.Multiplier);
                result.Z = (int)(result.Z * mamv.Multiplier);
            }
        }

        return result;
    }

    protected override Vector3I GetRaw(AttributeValueType valType = AttributeValueType.Value)
    {
        Vector3I retValue = valType switch
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
        var numVal = ObjectToVector3I(val);
        Vector3I retValue = valType switch
        {
            AttributeValueType.Min => SetMinValue(MinValue + numVal),
            AttributeValueType.Value => SetValue(CurrentValue + numVal),
            AttributeValueType.Max => SetMaxValue(MaxValue + numVal),
            AttributeValueType.Regen => SetRegenValue(RegenValue + numVal),
            _ => throw new ArgumentOutOfRangeException(nameof(valType), valType, null),
        };
    }

    protected int ClampInt(int val, int min, int max)
    {
        if (val > max)
        {
            val = max;
        }
        if (val < min)
        {
            val = min;
        }
        return val;
    }

    protected Vector3I ClampVector3I(Vector3I val)
    {
        var retVal = val;
        retVal.X = ClampInt(retVal.X, MinValue.X, MaxValue.X);
        retVal.Y = ClampInt(retVal.Y, MinValue.Y, MaxValue.Y);
        retVal.Z = ClampInt(retVal.Z, MinValue.Z, MaxValue.Z);
        return retVal;
    }

    protected Vector3I SetMinValue(Vector3I val)
    {
        var originalValue = MinValue;
        MinValue = val;
        if (originalValue != MinValue)
        {
            RaiseHasChanged();
        }
        if (CurrentValue < MinValue)
        {
            CurrentValue = MinValue;
        }
        if (MaxValue < MinValue)
        {
            MaxValue = MinValue;
        }
        return MinValue;
    }

    protected Vector3I SetValue(Vector3I val)
    {
        var originalValue = CurrentValue;
        CurrentValue = ClampVector3I(val);
        if (originalValue != CurrentValue)
        {
            RaiseHasChanged();
        }
        return CurrentValue;
    }

    protected Vector3I SetMaxValue(Vector3I val)
    {
        var originalMaxValue = MaxValue;
        MaxValue = val;
        if (originalMaxValue != MaxValue)
        {
            RaiseHasChanged();
        }
        if (CurrentValue > MaxValue)
        {
            CurrentValue = MaxValue;
        }
        if (MinValue > MaxValue)
        {
            MinValue = MaxValue;
        }
        return MaxValue;
    }

    protected Vector3I SetRegenValue(Vector3I val)
    {
        RegenValue = val;
        return RegenValue;
    }

    public override void Update(ulong tick)
    {
        base.Update(tick);

        //Regen
        var originalVal = CurrentValue;
        CurrentValue = ClampVector3I(CurrentValue + RegenValue);
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

