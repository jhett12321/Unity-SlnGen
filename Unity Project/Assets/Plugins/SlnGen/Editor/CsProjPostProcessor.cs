namespace SlnGen.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal class CsProjPostProcessor : AssetPostprocessor
    {
        private static readonly string editorRefConditional;
        private static readonly string platformDefines;

        static CsProjPostProcessor()
        {
            platformDefines = GetPlatformDefinePattern(SlnGen.Platforms);
            editorRefConditional = GetEditorConfigCondition(SlnGen.Configurations);
        }

        private static string GetPlatformDefinePattern(Platform[] pForms)
        {
            StringBuilder stringBuilder = new StringBuilder();

            HashSet<string> platformDefines = new HashSet<string>();
            foreach (Platform platform in pForms)
            {
                foreach (string define in platform.Defines)
                {
                    if (!platformDefines.Add(define))
                    {
                        continue;
                    }

                    stringBuilder.Append(@"\b");
                    stringBuilder.Append(define);
                    stringBuilder.Append(@"\b|");
                }
            }

            if (stringBuilder.Length > 0)
            {
                // Remove the last pipe character.
                stringBuilder.Length = stringBuilder.Length - 1;
            }

            return stringBuilder.ToString();
        }

        private static string GetEditorConfigCondition(Configuration[] configs)
        {
            List<string> conditions = new List<string>();

            foreach (Configuration config in configs)
            {
                if (!config.UseEditorReferences)
                {
                    continue;
                }
                foreach (Platform platform in SlnGen.Platforms)
                {
                    conditions.Add(" '$(Configuration)' == '" + config.Name + platform.Name + "' ");
                }
            }

            return string.Join(" OR ", conditions.ToArray());
        }

        private static string OnGeneratedCSProject(string path, string content)
        {
            try
            {
                XDocument doc = XDocument.Parse(content);
                XElement root = doc.Root;

                Assert.IsNotNull(root);

                XNamespace nSpace = root.Name.Namespace;

                List<XElement> propertyGroups = root.Elements(nSpace + "PropertyGroup").ToList();

                // Ensure the "Release" source PropertyGroup has Unity's defines.
                CopyDefinesToReleaseGroup(propertyGroups, nSpace);

                // Generate property groups for all configurations/platforms.
                List<XElement> newGroups = GeneratePropertyGroups(propertyGroups, nSpace);
                propertyGroups[propertyGroups.Count - 1].AddAfterSelf(newGroups);
                foreach (XElement propertyGroup in propertyGroups)
                {
                    propertyGroup.Remove();
                }

                // Add editor-config conditions to project references.
                AddConditionsToReferences(root, nSpace);

                return doc.ToString();
            }
            catch (Exception e)
            {
                // Error occurred. Return the unmodified project file.
                Debug.LogException(e);
                return content;
            }
        }

                /// <summary>
        /// Generated project files do not declare defines in the release configuration.
        /// We copy these defines from the Debug configuration. They are then filtered from the defined configurations.
        /// </summary>
        private static void CopyDefinesToReleaseGroup(List<XElement> propertyGroups, XNamespace nSpace)
        {
            XElement debugDefines = null;
            XElement releasePropGroup = null;

            // Add non-debug defines to the release mode.
            foreach (XElement propertyGroup in propertyGroups)
            {
                // The correct property group has a configuration/platform condition.
                XAttribute condition = propertyGroup.Attribute("Condition");
                if (condition == null)
                {
                    continue;
                }

                // The correct property group contains an "Optimize" value.
                XElement optimizeProp = propertyGroup.Element(nSpace + "Optimize");
                if (optimizeProp == null)
                {
                    continue;
                }

                if (condition.Value.Contains("Release"))
                {
                    releasePropGroup = propertyGroup;
                }
                else
                {
                    debugDefines = propertyGroup.Element(nSpace + "DefineConstants");
                }
            }

            Assert.IsNotNull(releasePropGroup);
            Assert.IsNotNull(debugDefines);

            releasePropGroup.Add(new XElement(nSpace + "DefineConstants", debugDefines.Value.Replace("DEBUG;", "")));
        }

        private static List<XElement> GeneratePropertyGroups(List<XElement> sources, XNamespace nSpace)
        {
            List<XElement> newElements = new List<XElement>();

            foreach (XElement propertyGroup in sources)
            {
                // Determine where this condition is saved.
                XAttribute condition = propertyGroup.Attribute("Condition");
                if (condition == null) // This is a startup configuration.
                {
                    XElement rootConfig = propertyGroup.Element(nSpace + "Configuration");

                    if (rootConfig != null)
                    {
                        rootConfig.SetValue(SlnGen.DefaultConfig.Name + '-' + SlnGen.CurrentPlatform.Name);
                    }

                    newElements.Add(propertyGroup);
                    continue;
                }

                GenerateProjectConfigurations(condition, propertyGroup, ref newElements);
            }

            return newElements;
        }

        /// <summary>
        /// Creates duplicate PropertyGroups nodes for all platforms, replacing the platform name, and defines for that platform.
        /// </summary>
        private static void GenerateProjectConfigurations(XAttribute condition, XElement sourcePropGroup, ref List<XElement> newGroups)
        {
            Assert.IsNotNull(condition);

            Configuration.BaseType sourceType = condition.Value.Contains("Debug") ? Configuration.BaseType.Debug : Configuration.BaseType.Release;

            foreach (Configuration configuration in SlnGen.Configurations)
            {
                // Does not match source property group, skip.
                if (configuration.BaseConfigType != sourceType)
                {
                    continue;
                }

                foreach (Platform platform in SlnGen.Platforms)
                {
                    XElement clone = new XElement(sourcePropGroup);
                    clone.SetAttributeValue("Condition", " '$(Configuration)|$(Platform)' == '" + configuration.Name + platform.Name + "|AnyCPU' ");

                    XElement defines = clone.Element(clone.Name.Namespace + "DefineConstants");
                    if (defines != null) // There are other platform property groups that do not contain defines.
                    {
                        string configDefines = defines.Value;
                        if (!string.IsNullOrEmpty(configuration.InvalidDefines))
                        {
                            configDefines = Regex.Replace(defines.Value, configuration.InvalidDefines, "");
                        }

                        // Don't modify platform defines if we are Unity's current platform.
                        if (platform != SlnGen.CurrentPlatform)
                        {
                            // Remove existing platform defines.
                            configDefines = Regex.Replace(configDefines, platformDefines, "");
                            // Add defines for this platform.
                            configDefines += ";" + string.Join(";", platform.Defines);
                        }

                        // Remove duplicate separators
                        configDefines = configDefines.Replace(";;", ";");
                        defines.SetValue(configDefines);
                    }

                    newGroups.Add(clone);
                }
            }
        }

        private static void AddConditionsToReferences(XElement root, XNamespace nSpace)
        {
            IEnumerable<XElement> itemGroups = root.Elements(nSpace + "ItemGroup");
            foreach (XElement itemGroup in itemGroups)
            {
                ProcessItemGroup(itemGroup);
            }
        }

        private static void ProcessItemGroup(XElement itemGroup)
        {
            IEnumerable<XElement> references = itemGroup.Elements();
            foreach (XElement reference in references)
            {
                XAttribute attribute = reference.Attribute("Include");
                if (attribute == null)
                {
                    continue;
                }

                string include = attribute.Value;
                switch (reference.Name.LocalName)
                {
                    // Script Assets
                    case "Compile":
                    case "None":
                        break;
                    // Dll references
                    case "Reference":
                        if (include.ToLower().Contains("editor"))
                        {
                            reference.SetAttributeValue("Condition", editorRefConditional);
                        }
                        break;
                    // Project refs (asmdef, etc)
                    case "ProjectReference":
                        if (include.EndsWith("editor.csproj", StringComparison.OrdinalIgnoreCase))
                        {
                            reference.SetAttributeValue("Condition", editorRefConditional);
                        }
                        break;
                }
            }
        }
    }
}