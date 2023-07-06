using System;
using System.Runtime.CompilerServices;

namespace CustomLanguage
{

    internal static class ProjectSourcePath
    {
        private const  string  myRelativePath = nameof(ProjectSourcePath) + ".cs";
        private static string? lazyValue;
        public static  string  Value => lazyValue ??= calculatePath();

        public static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null) //
            => callerFilePath ?? "";

        private static string calculatePath()
        {
            string pathName = GetSourceFilePathName();
            // Assert(pathName.EndsWith(myRelativePath, StringComparison.Ordinal));
            return pathName.Substring(0, pathName.Length - myRelativePath.Length);
        }
    }
}