using System;
using System.Collections.Generic;
using System.Text;

namespace P4API
{
    /// <summary>
    /// Each P4Revision object holds details about a particular revision
    /// of a file. It may also contain the history of any integrations 
    /// to/from the file
    /// </summary>
    public class P4Revision
    {
        internal P4Revision(string depotFile, int rev, int change, string action, 
            string type, DateTime time, string user, string client, string desc, string digest, int filesize)
        {
            _depotFile = depotFile;
            _rev = rev;
            _change = change;
            _action = action;
            _type = type;
            _time = time;
            _user = user;
            _client = client;
            _desc = desc;
            _digest = digest;
            _filesize = filesize;
            _integrations = new List<P4Integration>();
        }

        internal void addIntegration(P4Integration integ)
        {
            _integrations.Add(integ);
        }

        private List<P4Integration> _integrations;
        /// <summary>
        /// Returns the list of P4Integration objects for this revision. 
        /// </summary>
        public IList<P4Integration> Integrations
        {
            get
            {
                return _integrations;
            }
        }

        private string _depotFile;
        /// <summary>
        /// Returns the name of the depot file to which this object refers.
        /// </summary>
        public string DepotFile
        {
            get
            {
                return _depotFile;
            }
        }

        private string _action;
        /// <summary>
        /// Returns the name of the action which gave rise to this revision of the file. 
        /// </summary>
        public string Action
        {
            get
            {
                return _action;
            }
        }

        private string _type;
        /// <summary>
        /// Returns this revision's Perforce filetype. 
        /// </summary>
        public string FileType
        {
            get
            {
                return _type;
            }
        }

        private string _user;
        /// <summary>
        /// Returns the name of the user who created this revision. 
        /// </summary>
        public string User
        {
            get
            {
                return _user;
            }
        }

        private string _client;
        /// <summary>
        /// Returns the name of the client from which this revision was submitted. 
        /// </summary>
        public string Client
        {
            get
            {
                return _client;
            }
        }

        private string _digest;
        /// <summary>
        /// Returns the MD5 checksum of this revision.
        /// </summary>
        public string Digest
        {
            get
            {
                return _digest;
            }
        }

        private string _desc;
        /// <summary>
        /// Returns the description of the change which created this revision. 
        /// </summary>
        /// <remarks>
        /// Note that only the first 31 characters are returned unless you use p4 filelog -L for the first 250 characters, or p4 filelog -l for the full text.
        /// </remarks>
        public string Description
        {
            get
            {
                return _desc;
            }
        }

        private int _rev;
        /// <summary>
        /// Returns the number of this revision of the file. 
        /// </summary>
        public int Rev
        {
            get
            {
                return _rev;
            }
        }

        private int _change;
        /// <summary>
        /// Returns the change number that gave rise to this revision of the file. 
        /// </summary>
        public int Change
        {
            get
            {
                return _change;
            }
        }
        private int _filesize;
        /// <summary>
        /// Returns this revision's size in bytes. 
        /// </summary>
        public int FileSize
        {
            get
            {
                return _filesize;
            }
        }

        private DateTime _time;
        /// <summary>
        /// Returns the date/time that this revision was created. 
        /// </summary>
        public DateTime Time
        {
            get
            {
                return _time;
            }
        }

        /// <summary>
        /// Representation of the P4Revision
        /// </summary>
        /// <returns>String representation of the revision record.</returns>
        public override string ToString()
        {
 	         return string.Format("Revision (depotFile = {0} rev = {1} change = {2} action = {3} type = {4} time = {5} user = {6} client = {7})",
                 _depotFile, _rev, _change, _action, _type, _time, _user, _client);
        }
    }
}


