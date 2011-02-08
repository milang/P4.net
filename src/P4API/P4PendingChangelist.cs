/*
 * P4.Net *
Copyright (c) 2007-2010 Shawn Hladky

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, 
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or 
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */


using System;
using System.Text;

namespace P4API
{
    /// <summary>
    /// Represents a pending changelist from Perforce.
    /// </summary>
    /// <include file='CodeDocuments.xml' path='//Changelist/remarks' />
    /// <include file='CodeDocuments.xml' path='//Changelist/example' />
    public class P4PendingChangelist
    {
        private P4Connection _p4 = null;
        internal P4Form baseForm;
        internal P4PendingChangelist(string Description, P4Connection p4)
        {
            _p4 = p4;
            P4ExceptionLevels oldEx = _p4.ExceptionLevel;
            try
            {
                // we want to throw an exception if there are any errors.
                _p4.ExceptionLevel = P4ExceptionLevels.NoExceptionOnWarnings;
                baseForm = _p4.Fetch_Form("change");

                // clear the Jobs list
                baseForm.ArrayFields["Jobs"] = new string[0];

                // clear the Files list
                baseForm.ArrayFields["Files"] = new string[0];

                // save the description
                baseForm.Fields["Description"] = Description;

                P4UnParsedRecordSet r = _p4.Save_Form(baseForm);
            
                // convert to int to verify we're parsing correctly
                int changeNumber = int.Parse(r.Messages[0].Split(' ')[1]);
                baseForm.Fields["Change"] = changeNumber.ToString();
                
            }
            // no catch... we want the exception bubled up.
            finally
            {
                p4.ExceptionLevel = oldEx;
            }

        }

        /// <summary>
        /// Gets the pending changelist's number.
        /// </summary>
        /// <value>The changelist number.</value>
        public int Number
        {
            get
            {
                return int.Parse(baseForm.Fields["Change"]);
            }
        }

        /// <summary>
        /// Gets the pending changelist's description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                return baseForm.Fields["Description"];
            }
        }

        /// <summary>
        /// Submits the pending changelist to Perforce.
        /// </summary>
        /// <returns>P4UnParsedRecordSet with the results of the submit.</returns>
        public P4UnParsedRecordSet Submit()
        {
            return _p4.RunUnParsed("submit", "-c", Number.ToString());
        }
    }
}
