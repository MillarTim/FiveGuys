using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    [ComVisible(false)]
    [Serializable]
    internal class RegexDictionary : Dictionary<string, Regex>
    {
        public RegexDictionary(string resourceName, Assembly assembly)
        {
            LoadFromResource(resourceName, assembly);
        }

        protected RegexDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private void LoadFromResource(string resourceName, Assembly assembly)
        {
            ResourceManager resourceManager = resourceManager = new ResourceManager(resourceName, assembly);
            ResourceSet resourceSet;
            try
            {
                resourceSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
            }
            catch (MissingManifestResourceException e)
            {
                e.Data["ResourceName"] = resourceName;
                throw;
            }

            using (resourceSet)
            {
                IDictionaryEnumerator resource = resourceSet.GetEnumerator();
                while (resource.MoveNext())
                {
                    Regex regex = new Regex((string)resource.Value, RegexOptions.Compiled);
                    this.Add((string)resource.Key, regex);
                }
            }
        }
    }
}