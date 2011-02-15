using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace P4API
{

    /// <summary>
    /// P4.NET initialization (helper) methods.
    /// </summary>
    public static class Bootstrapper
    {

        //**************************************************
        //* Public interface
        //**************************************************

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
            if (0 == Interlocked.CompareExchange(ref Bootstrapper._initialized, 1, 0))
            {
                AppDomain.CurrentDomain.AssemblyResolve += CustomResolve;
            }
        }



        //**************************************************
        //* Private
        //**************************************************

        //-------------------------------------------------
        /// <summary>
        /// Custom assembly resolve implementation that
        /// recognizes requests for 'p4dn' assemblies.
        /// </summary>
        private static Assembly CustomResolve(object sender, System.ResolveEventArgs args)
        {
            const string p4DnName = "p4dn";
            if (null != args && null != args.Name && args.Name.StartsWith(p4DnName, StringComparison.OrdinalIgnoreCase))
            {
                if (null == Bootstrapper._p4DnAssembly) // CLR guarantees proper access to volatile reference by multiple threads
                {
                    lock (Bootstrapper.Sync)
                    {
                        if (null == Bootstrapper._p4DnAssembly) // just to be sure, recheck the assembly reference after the memory barrier issued by the lock above
                        {
                            // determine target assembly name
                            var targetDirectory = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                Path.Combine(
                                    "Perforce",
                                    "P4.NET"));
                            var architecture = (IntPtr.Size == 4 ? "x86" : "x64").ToLowerInvariant();
                            var thisAssembly = typeof(Bootstrapper).Assembly;
                            var thisAssemblyFileVersion = FileVersionInfo.GetVersionInfo(thisAssembly.Location).FileVersion;
                            var targetFileName = Path.Combine(
                                targetDirectory,
                                string.Concat(
                                    ClrSpec,
                                    ".",
                                    BuildSpec,
                                    ".",
                                    thisAssemblyFileVersion,
                                    ".",
                                    architecture,
                                    ".",
                                    p4DnName,
                                    ".dll"));

                            // if needed (target file does not exist or older than this assembly),
                            // extract embedded resources to target directory now
                            var targetFileInfo = new FileInfo(targetFileName);
                            if (!targetFileInfo.Exists)
                            {
                                if (!Directory.Exists(targetDirectory))
                                {
                                    Directory.CreateDirectory(targetDirectory);
                                }

                                const int bufferSize = 0x10000; // 64k
                                var buffer = new byte[bufferSize];
                                foreach (var resourceName in thisAssembly.GetManifestResourceNames())
                                {
                                    const string bootstrapperPrefix = @"P4API.Bootstrapper.";
                                    if (resourceName.StartsWith(bootstrapperPrefix, StringComparison.OrdinalIgnoreCase))
                                    {
                                        var extractName = Path.Combine(
                                            targetDirectory,
                                            resourceName
                                                .Substring(bootstrapperPrefix.Length)
                                                .Replace(@"framework", ClrSpec)
                                                .Replace(@"build", BuildSpec)
                                                .Replace(@"version", thisAssemblyFileVersion));
                                        using (var readStream = thisAssembly.GetManifestResourceStream(resourceName))
                                        {
                                            if (null == readStream) { throw new InvalidOperationException("Unable to GetManifestResourceStream for one of Assembly.GetManifestResourceNames values"); }
                                            using (var writeStream = File.Create(extractName, bufferSize))
                                            {
                                                int read;
                                                while (0 != (read = readStream.Read(buffer, 0, bufferSize)))
                                                {
                                                    writeStream.Write(buffer, 0, read);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // load the target file as assembly
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


        private static int _initialized; // flag indicating whether we've already subscribed to this AppDomain's AssemblyResolve event (1) or not (0)
        private static volatile Assembly _p4DnAssembly; // reference to the p4dn assembly for this process' architecture (set in the first resolve attempt; all subsequent attempts will use the cached reference)
        private static readonly object Sync = new object(); // used to serialize access to critical sections of code for multiple threads trying to resolve p4dn at the same time

        private const string BuildSpec = (
            #if DEBUG
            @"debug"
            #else
            @"release"
            #endif
        );

        private const string ClrSpec = (
            #if CLR4
            @"clr4"
            #else
            @"clr2"
            #endif
        );
    }

}
