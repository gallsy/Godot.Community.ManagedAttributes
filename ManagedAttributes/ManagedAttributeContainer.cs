using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Formatters;
using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding;
using Godot.Community.ControlBinding.EventArgs;

namespace Godot.Community.ManagedAttributes;

public class ManagedAttributeContainer : IObservableList
{
    public delegate void AttributeUpdatedHandler(IManagedAttribute attribute);
    public event AttributeUpdatedHandler AttributeUpdated;
    public event ObservableListChangedEventHandler ObservableListChanged;

    private List<IManagedAttribute> Attributes { get; set; } = new();

    public IManagedAttribute this[string i]
    {
        get { return Attributes.Find(a => a.GetName() == i); }
    }

    public bool Add(IManagedAttribute attr)
    {
        if (Attributes.Exists(a => a.GetName() == attr.GetName()))
        {
            return false;
        }
        attr.AttributeUpdated += OnAttributeUpdated;

        Attributes.Add(attr);
        OnObservableListChanged(new ObservableListChangedEventArgs()
        {
            ChangedEntries = new List<object> { attr },
            ChangeType = ObservableListChangeType.Add,
            Index = Attributes.IndexOf(attr)
        });
        return true;
    }

    public bool Remove(IManagedAttribute attr)
    {
        if(Attributes.Contains(attr))
        {
            Attributes.Remove(attr);
            return true;
        }
        return false;
    }

    public bool Remove(string attrName)
    {
        var attr = Attributes.Find(a => a.GetName() == attrName);
        if (attr != null)
        {
            return Remove(attr);
        }
        return false;
    }

    private void OnAttributeUpdated(IManagedAttribute attribute)
    {
        AttributeUpdated?.Invoke(attribute);
    }

    public void Update(ulong tick)
    {
        foreach (var attr in Attributes)
        {
            attr.Update(tick);
        }
    }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(Attributes, Formatting.Indented, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
    }

    public void Deserialize(string jsonString)
    {
        Attributes = JsonConvert.DeserializeObject<List<IManagedAttribute>>(jsonString, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
    }

    public IList<object> GetBackingList()
    {
        return Attributes.Cast<object>().ToList();
    }

    public void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {
        ObservableListChanged?.Invoke(eventArgs);
    }

    public void RemoveAt(int index)
    {

    }
}