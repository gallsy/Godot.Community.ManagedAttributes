using System;
using Newtonsoft.Json;

namespace Godot.Community.ManagedAttributes;

public class StringManagedAttribute : ManagedAttribute<string>
{
    [JsonProperty]
    protected string CurrentValue
    {
        get => currentValue;
        set => SetValue(ref currentValue, value);
    }
    private string currentValue;

    public StringManagedAttribute()
    {
        CurrentValue = "";
    }

    public StringManagedAttribute(string val)
    {
        CurrentValue = val;
    }

    public override void Set(object val, AttributeValueType valType = AttributeValueType.Value)
    {
        if(val is string strang)
        {
            var originalValue = CurrentValue;
            CurrentValue = strang;
            if(originalValue != strang)
            {
                RaiseHasChanged();
            }
            return;
        }
        throw new InvalidCastException($"Invalid type. Expected string, got {val.GetType().Name}");
    }

    public override void Add(object val, AttributeValueType valType = AttributeValueType.Value)
    {
        if(val is string strang)
        {
            CurrentValue += strang;
            if(!string.IsNullOrWhiteSpace(strang))
            {
                RaiseHasChanged();
            }
            return;
        }
        throw new InvalidCastException($"Invalid type in Add. Expected string, got {val.GetType().Name}");
    }

    protected override string Get(AttributeValueType valType = AttributeValueType.Value)
    {
        return CurrentValue;
    }

    protected override string GetRaw(AttributeValueType valType = AttributeValueType.Value)
    {
        return CurrentValue;
    }
}