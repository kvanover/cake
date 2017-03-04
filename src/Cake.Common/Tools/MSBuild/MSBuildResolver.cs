﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Cake.Core;
using Cake.Core.IO;

namespace Cake.Common.Tools.MSBuild
{
    internal static class MSBuildResolver
    {
        public static FilePath GetMSBuildPath(IFileSystem fileSystem, ICakeEnvironment environment, MSBuildToolVersion version, MSBuildPlatform buildPlatform)
        {
            var binPath = version == MSBuildToolVersion.Default
                ? GetHighestAvailableMSBuildVersion(fileSystem, environment, buildPlatform)
                : GetMSBuildPath(fileSystem, environment, (MSBuildVersion)version, buildPlatform);

            if (binPath == null)
            {
                throw new CakeException("Could not resolve MSBuild.");
            }

            // Get the MSBuild path.
            return binPath.CombineWithFilePath("MSBuild.exe");
        }

        private static DirectoryPath GetHighestAvailableMSBuildVersion(IFileSystem fileSystem, ICakeEnvironment environment, MSBuildPlatform buildPlatform)
        {
            var versions = new[]
            {
                MSBuildVersion.MSBuild15,
                MSBuildVersion.MSBuild14,
                MSBuildVersion.MSBuild12,
                MSBuildVersion.MSBuild4,
                MSBuildVersion.MSBuild35,
                MSBuildVersion.MSBuild20
            };

            foreach (var version in versions)
            {
                var path = GetMSBuildPath(fileSystem, environment, version, buildPlatform);
                if (fileSystem.Exist(path))
                {
                    return path;
                }
            }
            return null;
        }

        private static DirectoryPath GetMSBuildPath(IFileSystem fileSystem, ICakeEnvironment environment, MSBuildVersion version, MSBuildPlatform buildPlatform)
        {
            switch (version)
            {
                case MSBuildVersion.MSBuild15:
                    return GetVisualStudio2017Path(fileSystem, environment, buildPlatform);
                case MSBuildVersion.MSBuild14:
                    return GetVisualStudioPath(environment, buildPlatform, "14.0");
                case MSBuildVersion.MSBuild12:
                    return GetVisualStudioPath(environment, buildPlatform, "12.0");
                case MSBuildVersion.MSBuild4:
                    return GetFrameworkPath(environment, buildPlatform, "v4.0.30319");
                case MSBuildVersion.MSBuild35:
                    return GetFrameworkPath(environment, buildPlatform, "v3.5");
                case MSBuildVersion.MSBuild20:
                    return GetFrameworkPath(environment, buildPlatform, "v2.0.50727");
                default:
                    return null;
            }
        }

        private static DirectoryPath GetVisualStudioPath(ICakeEnvironment environment, MSBuildPlatform buildPlatform, string version)
        {
            // Get the bin path.
            var programFilesPath = environment.GetSpecialPath(SpecialPath.ProgramFilesX86);
            var binPath = programFilesPath.Combine(string.Concat("MSBuild/", version, "/Bin"));
            if (buildPlatform == MSBuildPlatform.Automatic)
            {
                if (environment.Platform.Is64Bit)
                {
                    binPath = binPath.Combine("amd64");
                }
            }
            if (buildPlatform == MSBuildPlatform.x64)
            {
                binPath = binPath.Combine("amd64");
            }
            return binPath;
        }

        private static DirectoryPath GetVisualStudio2017Path(IFileSystem fileSystem, ICakeEnvironment environment,
            MSBuildPlatform buildPlatform)
        {
            var vsEditions = new[]
            {
                "Enterprise",
                "Professional",
                "Community",
                "BuildTools"
            };

            var visualStudio2017Path = environment.GetSpecialPath(SpecialPath.ProgramFilesX86);

            foreach (var edition in vsEditions)
            {
                // Get the bin path.
                var binPath = visualStudio2017Path.Combine(string.Concat("Microsoft Visual Studio/2017/", edition, "/MSBuild/15.0/Bin"));
                if (fileSystem.Exist(binPath))
                {
                    if (buildPlatform == MSBuildPlatform.Automatic)
                    {
                        if (environment.Platform.Is64Bit)
                        {
                            binPath = binPath.Combine("amd64");
                        }
                    }
                    if (buildPlatform == MSBuildPlatform.x64)
                    {
                        binPath = binPath.Combine("amd64");
                    }
                    return binPath;
                }
            }
            return visualStudio2017Path.Combine("Microsoft Visual Studio/2017/Professional/MSBuild/15.0/Bin");
        }

        private static DirectoryPath GetFrameworkPath(ICakeEnvironment environment, MSBuildPlatform buildPlatform, string version)
        {
            // Get the Microsoft .NET folder.
            var windowsFolder = environment.GetSpecialPath(SpecialPath.Windows);
            var netFolder = windowsFolder.Combine("Microsoft.NET");

            if (buildPlatform == MSBuildPlatform.Automatic)
            {
                // Get the framework folder.
                var is64Bit = environment.Platform.Is64Bit;
                var frameWorkFolder = is64Bit ? netFolder.Combine("Framework64") : netFolder.Combine("Framework");
                return frameWorkFolder.Combine(version);
            }

            if (buildPlatform == MSBuildPlatform.x86)
            {
                return netFolder.Combine("Framework").Combine(version);
            }

            if (buildPlatform == MSBuildPlatform.x64)
            {
                return netFolder.Combine("Framework64").Combine(version);
            }

            throw new NotSupportedException();
        }
    }
}