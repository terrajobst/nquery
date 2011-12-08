using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Runtime;

namespace NQuery
{
	public abstract class BindingCollection<T> : Collection<T> where T : Binding
	{
		protected BindingCollection()
		{
		}

		protected virtual void OnChange(EventArgs args)
		{
			EventHandler<EventArgs> handler = Changed;
			if (handler != null)
				handler(this, args);
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			OnChange(EventArgs.Empty);
		}

		protected override void InsertItem(int index, T item)
		{
			if (item == null)
				throw ExceptionBuilder.ArgumentNull("item");

			BeforeInsert(item);
			base.InsertItem(index, item);
			OnChange(EventArgs.Empty);
		}

		protected override void RemoveItem(int index)
		{
			T elementToRemove = this[index];
			base.RemoveItem(index);
			AfterRemove(elementToRemove);
			OnChange(EventArgs.Empty);
		}

		protected override void SetItem(int index, T item)
		{
			if (item == null)
				throw ExceptionBuilder.ArgumentNull("item");

			T removedItem = this[index];

			BeforeInsert(item);
			base.SetItem(index, item);
			AfterRemove(removedItem);

			OnChange(EventArgs.Empty);
		}

		protected virtual void BeforeInsert(T binding)
		{
			T existingBinding = this[binding.Name];

			if (existingBinding != null)
				throw ExceptionBuilder.BindingWithSameNameAlreadyInCollection("binding", binding);
		}

		protected virtual void AfterRemove(T binding)
		{
		}

		public virtual T[] Find(Identifier identifier)
		{
			if (identifier == null)
				throw ExceptionBuilder.ArgumentNull("identifier");

			List<T> result = new List<T>();

			foreach (T binding in this)
			{
				if (identifier.Matches(binding.Name))
					result.Add(binding);
			}

			return result.ToArray();
		}

		public void Remove(string name)
		{
			if (name == null)
				throw ExceptionBuilder.ArgumentNull("name");

			T bindingToRemove = this[name];

			if (bindingToRemove != null)
				Remove(bindingToRemove);
		}

		public virtual T this[string bindingName]
		{
			get
			{
				if (bindingName == null)
					throw ExceptionBuilder.ArgumentNull("bindingName");

				foreach (T binding in this)
				{
					if (binding.Name == bindingName)
						return binding;
				}

				return null;
			}
		}

		public event EventHandler<EventArgs> Changed;
	}
}