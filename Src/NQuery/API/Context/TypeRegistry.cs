using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NQuery 
{
	/// <summary>
	/// This class is used to store references to types. The only thing that is special about this class
	/// is that it also allows to return derived types. For exmaple if there is the class <c>D</c> derived
	/// from class <c>B</c> and <see cref="TypeRegistry{T}"/> contains <c>D</c> but not <c>B</c> a request for
	/// <c>B</c> will return <c>D</c>.
	/// </summary>
	public sealed class TypeRegistry<T> where T : class
	{
        private Dictionary<Type, T> _typeDictionary = new Dictionary<Type, T>();
		private T _defaultValue;

		private void OnChanged()
		{
			EventHandler<EventArgs> handler = Changed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		public T DefaultValue
		{
			get { return _defaultValue; }
			set
			{
				_defaultValue = value;
				OnChanged();
			}
		}

		public void Register(Type key, T value)
		{
			if (key == null)
				throw ExceptionBuilder.ArgumentNull("key");

			if (value == null)
				throw ExceptionBuilder.ArgumentNull("value");

			if (_typeDictionary.ContainsKey(key))
				throw ExceptionBuilder.TypeAlreadyRegistered("key", key);

			_typeDictionary.Add(key, value);
			OnChanged();
		}

		public void Unregister(Type key)
		{
			if (key == null)
				throw ExceptionBuilder.ArgumentNull("key");

			_typeDictionary.Remove(key);
			OnChanged();
		}

		public void Unregister(T value)
		{
			if (value == null)
				throw ExceptionBuilder.ArgumentNull("value");

			foreach (KeyValuePair<Type, T> pair in _typeDictionary)
			{
				if (Equals(value, pair.Value))
					_typeDictionary.Remove(pair.Key);
			}

			OnChanged();
		}

		public bool IsRegistered(Type key)
		{
			if (key == null)
				throw ExceptionBuilder.ArgumentNull("key");

			return _typeDictionary.ContainsKey(key);
		}

		public T GetValue(Type key)
		{
			if (key == null)
				throw ExceptionBuilder.ArgumentNull("key");

			while (key != null)
            {
                T value;
                if (_typeDictionary.TryGetValue(key, out value))
                    return value;
                
                key = key.BaseType;
            }
		    
			return _defaultValue;
		}

		public ICollection<Type> Keys
		{
			get { return _typeDictionary.Keys; }
		}

		public ICollection<T> Values
		{
			get { return _typeDictionary.Values; }
		}

		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public T this[Type key]
		{
			get
			{
				if (key == null)
					throw ExceptionBuilder.ArgumentNull("key");

				return GetValue(key);
			}

			set
			{
				if (key == null)
					throw ExceptionBuilder.ArgumentNull("key");

				if (value == null)
					throw ExceptionBuilder.ArgumentNull("value");

				_typeDictionary[key] = value;
				OnChanged();
			}
		}

		public event EventHandler<EventArgs> Changed;
	}
}