using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace P4API
{

    /// <summary>
    /// P4.NET initialization (helper) methods.
    /// </summary>
    public static class Bootstrapper
    {

        /**************************************************
        /* Public interface
        /**************************************************/

        //-------------------------------------------------
        /// <summary>
        /// Make sure 'p4dn' assembly can be resolved successfully
        /// even when the underlying DLL is not discoverable
        /// by Fusion assembly resolver.
        /// </summary>
        /// <remarks>
        /// Initializes current AppDomain so that failed attempts
        /// to resolve assemblies will be given one last chance
        /// by calling CustomResolve private method; this method
        /// will recognize 'p4dn' assembly name and attempt to
        /// satisfy the request by extracting the embedded p4dn
        /// for the appropriate CPU architecture and loading
        /// this extracted copy.
        /// </remarks>
        public static void Initialize()
        {
            if (!Bootstrapper._initialized) // CLR guarantees proper access to volatile bool by multiple threads
            {
                lock (Bootstrapper.Sync)
                {
                    if (!Bootstrapper._initialized) // just to be sure, recheck the flag value after the memory barrier issued by the lock above
                    {
                        AppDomain.CurrentDomain.AssemblyResolve += CustomResolve;
                        _initialized = true;
                    }
                }
            }
        }




        /**************************************************
        /* Private
        /**************************************************/

        //-------------------------------------------------
        /// <summary>
        /// Custom assembly resolve implementation that
        /// recognizes requests for 'p4dn' assemblies.
        /// </summary>
        private static Assembly CustomResolve(object sender, System.ResolveEventArgs args)
        {
            const string p4DnName = "p4dn";
            const string categoryError = "P4.NET resolver error";

            if (null != args && null != args.Name && args.Name.StartsWith(p4DnName, StringComparison.OrdinalIgnoreCase))
            {
                if (null == Bootstrapper._p4DnAssembly) // CLR guarantees proper access to volatile reference by multiple threads
                {
                    lock (Bootstrapper.Sync)
                    {
                        if (null == Bootstrapper._p4DnAssembly) // just to be sure, recheck the assembly reference after the memory barrier issued by the lock above
                        {
                            // directory for embedded assembly extraction must exist
                            var targetDirectory = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                Path.Combine(
                                    "Perforce",
                                    "P4.NET"));
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }

                            // is extraction needed? (or is a fresh copy in the target directory already?)
                            var architecture = (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "x86").ToLowerInvariant();
                            var thisAssembly = typeof(Bootstrapper).Assembly;
                            var thisAssemblyName = thisAssembly.GetName();
                            var targetFileName = Path.Combine(
                                targetDirectory,
                                string.Concat(
                                    p4DnName,
                                    "_",
                                    thisAssemblyName.Version.ToString(4),
                                    "_",
                                    architecture,
                                    ".dll"));
                            bool extractionNeeded;
                            var targetFileInfo = new FileInfo(targetFileName);
                            if (targetFileInfo.Exists) // if file exists, extract if target file is older than this assembly; if no file found, extract always
                            {
                                var thisAssemblyFileInfo = new FileInfo(new Uri(thisAssemblyName.CodeBase).AbsolutePath);
                                extractionNeeded = (thisAssemblyFileInfo.LastWriteTimeUtc > targetFileInfo.LastWriteTimeUtc);
                            }
                            else
                            {
                                extractionNeeded = true;
                            }

                            // extract now (unless the target file is fresh)
                            if (extractionNeeded)
                            {
                                var resourceName = string.Concat(
                                    typeof(Bootstrapper).FullName,
                                    ".",
                                    architecture,
                                    ".",
                                    p4DnName,
                                    ".dll");
                                const int bufferSize = 0x10000; // 64k
                                var buffer = new byte[bufferSize];
                                using (var readStream = thisAssembly.GetManifestResourceStream(resourceName))
                                {
                                    if (null == readStream)
                                    {
                                        Trace.WriteLine(string.Concat("No embedded resource \"", resourceName, "\" found, your architecture is probably not supported"), categoryError);
                                        return null;
                                    }
                                    using (var writeStream = File.Create(targetFileName, bufferSize))
                                    {
                                        int read;
                                        while (0 != (read = readStream.Read(buffer, 0, bufferSize)))
                                        {
                                            writeStream.Write(buffer, 0, read);
                                        }
                                    }
                                }
                            }

                            // and load the extracted file as assembly
                            Bootstrapper._p4DnAssembly = Assembly.LoadFile(targetFileName);
                        }
                    }
                }

                // make sure we can satisfy the requested reference with the embedded assembly (now extracted)
                var reference = new AssemblyName(args.Name);
                if (AssemblyName.ReferenceMatchesDefinition(reference, Bootstrapper._p4DnAssembly.GetName()))
                {
                    return Bootstrapper._p4DnAssembly;
                }
            }

            // we don't recognize the requested reference
            return null;
        }


        private static volatile bool _initialized; // flag indicating whether we've already subscribed to this AppDomain's AssemblyResolve event
        private static volatile Assembly _p4DnAssembly; // reference to the p4dn assembly for this process' architecture (set in the first resolve attempt; all subsequent attempts will use the cached reference)
        private static readonly object Sync = new object(); // used to serialize access to critical sections of code for multiple threads trying to resolve p4dn at the same time

    }

}
