using System;
using System.Text;
using p4dn;
using System.IO;

namespace P4API
{
    /// <summary>
    /// Actions to take when resolving a file.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public enum MergeAction
    {
        /// <summary>
        /// Quit the resolve workflow.
        /// </summary>
        Quit,           //  CMS_QUIT   user wants to quit

        /// <summary>
        /// Skip this file in the resolve workflow.
        /// </summary>
        Skip,           //  CMS_SKIP   skip the integration record

        /// <summary>
        /// Accept the merged file from Perforce.  This file must not be edited.
        /// </summary>
        AcceptMerged,	//  CMS_MERGED accepted merged theirs and yours
        
        /// <summary>
        /// Accept the result file with your edits.  The result file should be edited before returning this merge action.
        /// </summary>
        AcceptEdit,     //  CMS_EDIT   accepted edited merge
        
        /// <summary>
        /// Accept 'their' file ('your' changes will be lost). 
        /// </summary>
        AcceptTheirs,   //  CMS_THEIRS accepted theirs
        
        /// <summary>
        /// Accept 'your' file ('thier' changes will be lost). 
        /// </summary>
        AcceptYours     //  CMS_YOUR   accepted yours
    }

    /// <summary>
    /// Contains information about the files being merged.
    /// </summary>
    public class MergeData : IDisposable
    {
        private P4MergeData _mergeData;

        internal MergeData(P4MergeData mergeData)
        {
            _mergeData = mergeData;
        }

        /// <summary>
        /// Base file for the 3-way merge.
        /// </summary>
        public FileInfo BaseFile
        {
            get
            {
                return _mergeData.GetBasePath();
            }
        }

        /// <summary>
        /// Your file for the 3-way merge.
        /// </summary>
        public FileInfo YourFile
        {
            get
            {
                return _mergeData.GetYourPath();
            }
        }

        /// <summary>
        /// Perforce's recommended merge action.
        /// </summary>
        public MergeAction MergeHint
        {
            get
            {
                switch (_mergeData.GetMergeHint())
                {
                    case P4MergeStatus.CMS_QUIT:
                        return MergeAction.Quit;
                    case P4MergeStatus.CMS_SKIP:
                        return MergeAction.Skip;
                    case P4MergeStatus.CMS_EDIT:
                        return MergeAction.AcceptEdit;
                    case P4MergeStatus.CMS_MERGED:
                        return MergeAction.AcceptMerged;
                    case P4MergeStatus.CMS_YOURS:
                        return MergeAction.AcceptYours;
                    case P4MergeStatus.CMS_THEIRS:
                        return MergeAction.AcceptTheirs;
                }
                return MergeAction.Quit;
            }
        }

        /// <summary>
        /// Thier file for the 3-way merge.
        /// </summary>
        public FileInfo TheirFile
        {
            get
            {
                return _mergeData.GetTheirPath();
            }
        }

        /// <summary>
        /// File where merged result should be written.
        /// </summary>
        public FileInfo ResultFile
        {
            get
            {
                return _mergeData.GetResultPath();
            }
        }

        /// <summary>
        /// Perforce name of the base file.  Format is '//depot/path/file.ext#rev'.
        /// </summary>
        public string BaseName
        {
            get
            {
                return _mergeData.GetBaseName();
            }
        }

        /// <summary>
        /// Perforce name of your file.  Format is '//client/path/file.ext'.
        /// </summary>
        public string YourName
        {
            get
            {
                return _mergeData.GetYourName();
            }
        }

        /// <summary>
        /// Perforce name of their file.  Format is '//depot/path/file.ext#rev'.
        /// </summary>
        public string TheirName
        {
            get
            {
                return _mergeData.GetTheirName();
            }
        }

        /// <summary>
        /// Returns the number of chunks changed in your file.
        /// </summary>
        public int YourChunks
        {
            get
            {
                return _mergeData.GetYourChunks();
            }
        }

        /// <summary>
        /// Returns the MD5 checksum of WHAT?? file.
        /// </summary>
        public string MergeDigest
        {
            get
            {
                return _mergeData.GetMergeDigest();
            }
        }

        /// <summary>
        /// Returns the MD5 checksum of your file.
        /// </summary>
        public string YourDigest
        {
            get
            {
                return _mergeData.GetYourDigest();
            }
        }

        /// <summary>
        /// Returns the MD5 checksum of their file.
        /// </summary>
        public string TheirDigest
        {
            get
            {
                return _mergeData.GetTheirDigest();
            }
        }
        
        /// <summary>
        /// Returns the number of chunks with changes in their file. 
        /// </summary>
        public int TheirChunks
        {
            get
            {
                return _mergeData.GetTheirChunks();
            }
        }

        /// <summary>
        /// Returns the number of chunks that have the same change in both files.
        /// </summary>
        public int BothChunks
        {
            get
            {
                return _mergeData.GetBothChunks();
            }
        }

        /// <summary>
        /// Returns the number of chunks that have conflicts.
        /// </summary>
        public int ConflictChunks
        {
            get
            {
                return _mergeData.GetConflictChunks();
            }
        }

        /// <summary>
        /// Runs the external merge tool configured by P4MERGE.
        /// </summary>
        /// <returns>True if the merge tool exits with code 0, false otherwise.</returns>
        public bool RunMergeTool()
        {
            return _mergeData.RunMergeTool();
        }

        #region IDisposable Members

        /// <summary>
        /// Frees unmanaged memory.
        /// </summary>
        public void Dispose()
        {
            _mergeData.Dispose();
        }

        #endregion
    }
}
