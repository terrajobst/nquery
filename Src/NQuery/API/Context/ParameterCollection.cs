using System;
using System.Collections.Generic;

using NQuery.Runtime;

namespace NQuery
{
	public sealed class ParameterCollection : BindingCollection<ParameterBinding>
	{
		public ParameterCollection()
		{
		}
		
		public ParameterBinding Add(string parameterName, Type parameterType)
		{
            return Add(parameterName, parameterType, null);
        }
		
		public ParameterBinding Add(string parameterName, Type parameterType, object value)
		{
            if (parameterName == null)
                throw ExceptionBuilder.ArgumentNull("parameterName");

            if (parameterType == null)
                throw ExceptionBuilder.ArgumentNull("parameterType");

		    string extractParameterName = ParameterBinding.ExtractParameterName(parameterName);
		    ParameterBinding parameterBinding = new ParameterBinding(extractParameterName, parameterType);
            parameterBinding.Value = value;
            Add(parameterBinding);
            return parameterBinding;
		}
    
	    public ParameterBinding Add(string parameterName, Type parameterType, object value, IList<PropertyBinding> customProperties)
        {
            if (parameterName == null)
                throw ExceptionBuilder.ArgumentNull("parameterName");

            if (parameterType == null)
                throw ExceptionBuilder.ArgumentNull("parameterType");

            if (customProperties == null)
                throw ExceptionBuilder.ArgumentNull("customProperties");
	        
            string extractParameterName = ParameterBinding.ExtractParameterName(parameterName);
	        ParameterBinding parameterBinding = new ParameterBinding(extractParameterName, parameterType, customProperties);
            parameterBinding.Value = value;
            Add(parameterBinding);
            return parameterBinding;
        }

		public override ParameterBinding this[string bindingName]
        {
            get
            {
				if (bindingName == null)
					throw ExceptionBuilder.ArgumentNull("bindingName");

				bindingName = ParameterBinding.ExtractParameterName(bindingName);
                foreach (ParameterBinding parameterBinding in this)
                {
					if (parameterBinding.Name == bindingName)
                        return parameterBinding;
                }

                return null;
            }
        }
    }
}