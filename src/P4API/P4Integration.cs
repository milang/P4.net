using System;
using System.Collections.Generic;
using System.Text;

namespace P4API
{
    /// <summary>
    /// P4Integration objects hold details about the integrations that have
    /// been performed on a particular revision. Used primarily with the
    /// P4Revision class
    /// </summary>
    public class P4Integration
    {
        
        internal P4Integration(string how, string file, int srev, int erev)
        {
            _how = how;
            _file = file;
            _srev = srev;
            _erev = erev;
        }

        private string _how;
        /// <summary>
        /// Returns the type of the integration record - how that record was created. 
        /// </summary>
        public string How
        {
            get
            {
                return _how;
            }
        }

        private string _file;
        /// <summary>
        /// Returns the path to the file being integrated to/from. 
        /// </summary>
        public string File
        {
            get
            {
                return _file;
            }
        }

        private int _srev;
        /// <summary>
        /// Returns the start revision number used for this integration. 
        /// </summary>
        public int StartRev
        {
            get
            {
                return _srev;
            }
        }

        private int _erev;
        /// <summary>
        /// Returns the end revision number used for this integration. 
        /// </summary>
        public int EndRev
        {
            get
            {
                return _erev;
            }
        }

        /// <summary>
        /// Representation of the integration record.
        /// </summary>
        /// <returns>String representing the integration record.</returns>
        public override string ToString()
        {
            return String.Format("Integration (how = {0} file = {1} srev = {2} erev = {3})", _how, _file, _srev, _erev);
        }
    }
}
