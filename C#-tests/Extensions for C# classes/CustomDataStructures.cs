using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CustomDataStructures
{
    /// <summary>
    /// Represents a collection that maintains a set of elements, allowing access to each element by any of its properties marked as a key
    /// </summary>
    public class InterlinkedCollection<T> : IEnumerable<T>
    {
        private Dictionary<object, Guid> elementToIdMap;    // here 'object' is one element from 'Data' ('Data' of <T> type)
        private Dictionary<Guid, T> idToDataMap;

        public InterlinkedCollection(params T[] elements)
        {
            elementToIdMap = new();
            idToDataMap = new();
        }

        public T FindRelatedSet(object key)
        {
            if (!elementToIdMap.TryGetValue(key, out Guid id))
            {
                throw new KeyNotFoundException($"The key was not found in the collection: {key}");
            }
            return idToDataMap[id];
        }

        public T this[object key]
        {
            get => FindRelatedSet(key);
        }


        public void Add(T element)
        {
            var id = Guid.NewGuid();
            idToDataMap[id] = element;
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var keyProperty = property.GetCustomAttribute<CanBeKeyAttribute>();
                if (keyProperty == null || !keyProperty.CanBeKey)
                    continue;

                var key = property.GetValue(element);
                if (key == null)
                    continue;

                if (elementToIdMap.ContainsKey(key))
                {
                    throw new ArgumentException($"Duplicate key found: {key}. Key must be unique.");
                }

                elementToIdMap[key] = id;
            }


        }

        public void Update(string key, string propertyName, object newValue)
        {
            if (!elementToIdMap.TryGetValue(key, out Guid id))
                throw new KeyNotFoundException($"No entry found for key: {key}");

            T element = idToDataMap[id];
            var property = typeof(T).GetProperty(propertyName);

            if (property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type {typeof(T).Name}");

            if (!property.CanWrite)
                throw new ArgumentException($"Property '{propertyName}' is not writable.");

            // Check if the property has CanBeKey attribute and handle key updates
            var keyAttr = property.GetCustomAttribute<CanBeKeyAttribute>();
            object oldKey = null;
            if (keyAttr != null && keyAttr.CanBeKey)
            {
                oldKey = property.GetValue(element);
                if (oldKey.Equals(key)) // Only need to update if the key itself is being changed
                {
                    elementToIdMap.Remove(oldKey);
                }
            }

            // Set the new value
            property.SetValue(element, newValue);

            // Re-add the key if it was a key property
            if (oldKey != null && keyAttr != null && keyAttr.CanBeKey)
            {
                elementToIdMap[property.GetValue(element)] = id;
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return idToDataMap.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


    /// <summary>
    /// Specifies that a property of an object can be used as a key in an InterlinkedCollection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CanBeKeyAttribute : Attribute
    {
        public bool CanBeKey { get; private set; }

        public CanBeKeyAttribute(bool canBeKey)
        {
            CanBeKey = canBeKey;
        }
    }

    public class NotifyingObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<string, object> PropertyChanging;

        private volatile bool _isUpdating;

        protected bool IsUpdating
        {
            get => _isUpdating;
            set => _isUpdating = value;
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (IsUpdating)
            {
                field = value;
                return;
            }

            // for every other "outer" try to change
            OnPropertyChanging(propertyName, value);
        }

        protected void OnPropertyChanging(string propertyName, object newValue)
        {
            PropertyChanging?.Invoke(propertyName, newValue);
        }
    }

    /// <summary>
    /// Represents an element in an InterlinkedCollection with specified properties that can be used as keys.
    /// </summary>
    /*public class ExampleDataSet : NotifyingObject
    {
        [CanBeKey(true)]
        public string name { get; set; }

        [CanBeKey(true)]
        public int age { get; set; }

        [CanBeKey(false)]
        public bool isNormal { get; set; }


        public ExampleDataSet(string name, int age, bool isNormal)
        {
            this.name = name;
            this.age = age;
            this.isNormal = isNormal;
        }
    }*/
}