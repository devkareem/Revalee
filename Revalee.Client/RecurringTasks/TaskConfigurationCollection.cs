﻿#region License

/*
The MIT License (MIT)

Copyright (c) 2014 Sage Analytic Technologies, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion License

using Revalee.Client.Configuration;
using System;
using System.Configuration;

namespace Revalee.Client.RecurringTasks
{
	internal class TaskConfigurationCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		public TaskConfigurationElement this[int Index]
		{
			get
			{
				return (TaskConfigurationElement)this.BaseGet(Index);
			}
			set
			{
				if (this.BaseGet(Index) != null)
				{
					this.BaseRemoveAt(Index);
				}

				this.BaseAdd(Index, value);
			}
		}

		protected override string ElementName
		{
			get { return "task"; }
		}

		protected override bool IsElementName(string elementName)
		{
			return (elementName == this.ElementName);
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new TaskConfigurationElement();
		}

		protected override object GetElementKey(System.Configuration.ConfigurationElement element)
		{
			return ((TaskConfigurationElement)element).Key;
		}

		[ConfigurationProperty("callbackBaseUri", IsKey = false, IsRequired = false)]
		[UrlValidator(AllowAbsolute = true, AllowRelative = false)]
		public Uri CallbackBaseUri
		{
			get
			{
				return (Uri)this["callbackBaseUri"];
			}
		}
	}
}