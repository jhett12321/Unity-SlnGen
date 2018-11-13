namespace SlnGen.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal class SlnPostProcessor : AssetPostprocessor
    {
        private const int SLN_WRITE_DELAY_MS = 500;

        private static string OnGeneratedSlnSolution(string path, string content)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                using (StringReader stringReader = new StringReader(content))
                {
                    List<Project> projects = GetProjects(stringReader, stringBuilder);

                    CopyUntilLine(stringReader, stringBuilder, "	GlobalSection(SolutionConfigurationPlatforms) = preSolution");
                    CreateConfigurationPlatforms(stringBuilder);

                    SkipTo(stringReader, "	EndGlobalSection");
                    stringBuilder.AppendLine("	EndGlobalSection");

                    CopyUntilLine(stringReader, stringBuilder, "	GlobalSection(ProjectConfigurationPlatforms) = postSolution");

                    CreateConfigurations(stringBuilder, projects);

                    SkipTo(stringReader, "	EndGlobalSection");
                    stringBuilder.AppendLine("	EndGlobalSection");

                    stringBuilder.Append(stringReader.ReadToEnd());
                }

                string newSln = stringBuilder.ToString();

                // TODO Unity Bug Workaround
                WriteSlnDelayed(path, newSln);

                return newSln;
            }
            catch (Exception e)
            {
                // Exception occurred. Return the unmodified solution.
                Debug.LogException(e);
                return content;
            }
        }

        private static void WriteSlnDelayed(string path, string newSln)
        {
            // HACK - we write the sln file after a short delay as Unity does not use the returned value.
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.Sleep(SLN_WRITE_DELAY_MS);
                File.WriteAllText(path, newSln);
            });
        }

        private static List<Project> GetProjects(StringReader stringReader, StringBuilder builder)
        {
            List<Project> projects = new List<Project>();

            for (;;)
            {
                string line = stringReader.ReadLine();
                if (line == null)
                {
                    throw new EndOfStreamException("Reached the end of the sln before finding project.");
                }

                builder.AppendLine(line);

                if (line.StartsWith("Project"))
                {
                    projects.Add(new Project(line));
                }
                else if (line == "Global")
                {
                    break;
                }
            }

            return projects;
        }

        /// <summary>
        /// Skips to the specified line, without writing.
        /// </summary>
        private static void SkipTo(StringReader stringReader, string line)
        {
            ProcessSeek(stringReader, line, false, null);
        }

        /// <summary>
        /// Skips to the specified line while writing lines to the specified string builder.
        /// </summary>
        private static void CopyUntilLine(StringReader stringReader, StringBuilder stringBuilder, string line)
        {
            ProcessSeek(stringReader, line, true, stringBuilder);
        }

        private static void ProcessSeek(StringReader stringReader, string skipToLine, bool write, StringBuilder builder)
        {
            Assert.IsTrue(!write || builder != null);

            for (;;)
            {
                string line = stringReader.ReadLine();
                if (line == null)
                {
                    throw new EndOfStreamException("Reached the end of the sln before finding " + skipToLine);
                }

                if (write)
                {
                    builder.AppendLine(line);
                }

                if (line == skipToLine)
                {
                    break;
                }
            }
        }

        private static void CreateConfigurationPlatforms(StringBuilder stringBuilder)
        {
            foreach (Configuration configuration in SlnGen.Configurations)
            {
                foreach (Platform platform in SlnGen.Platforms)
                {
                    stringBuilder.AppendFormat("		{0}|{1} = {0}|{1}", configuration.Name, platform.Name);
                    stringBuilder.AppendLine();
                }
            }
        }

        private static void CreateConfigurations(StringBuilder stringBuilder, List<Project> projects)
        {
            foreach (Project project in projects)
            {
                foreach (Configuration configuration in SlnGen.Configurations)
                {
                    foreach (Platform platform in SlnGen.Platforms)
                    {
                        stringBuilder.AppendFormat("		{0}.{1}|{2}.ActiveCfg = {1}{2}|Any CPU", project.ProjectId, configuration.Name, platform.Name);
                        stringBuilder.AppendLine();

                        if (configuration.UseEditorReferences || !project.IsEditorProject)
                        {
                            stringBuilder.AppendFormat("		{0}.{1}|{2}.Build.0 = {1}{2}|Any CPU", project.ProjectId, configuration.Name, platform.Name);
                            stringBuilder.AppendLine();
                        }
                    }
                }
            }
        }
    }
}