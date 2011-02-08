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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

namespace P4API
{
    /// <summary>
    /// Represents a Perforce 'Form' object.
    /// </summary>
    /// <include file='CodeDocuments.xml' path='//Forms/remarks' />
    /// <include file='CodeDocuments.xml' path='//Forms/example' />
    public class P4Form : P4Record
    {
        private string _formCommand;
        private string _specdef = null;
        private p4dn.Spec _spec = null;
        private System.Text.Encoding _encoding;

        internal P4Form(string FormCommand, string specDef, Dictionary<string, string> S, System.Text.Encoding encoding)
            : base(S)
        {
            _specdef = specDef;
            // clone this so we don't hold a reference to another object's encoding object
            // preventing it from Garbage collecting.
            _encoding = (System.Text.Encoding) encoding.Clone();
            _spec = new p4dn.Spec(specDef, encoding);
            _formCommand = FormCommand;
        }

        /// <summary>
        /// Parses a Perforce form without making a server connection.
        /// </summary>
        /// <param name="formCommand">The command that would otherwise be used to fetch the form.</param>
        /// <param name="specDef">The Perforce 'specdef' for the form.</param>
        /// <param name="formContents">The raw formated form text.</param>
        /// <param name="encoding">Server encoding (either ANSI or UFT-8).</param>
        /// <returns>A Perforce form object.</returns>
        /// <remarks>
        /// LoadFromSpec can be used to parse a form without making a call to the server.  
        /// LoadFromSpec can be useful in form triggers.
        /// It does require you to know the SpecDef to call, which can change when upgrading or changing
        /// the server configuration.
        /// </remarks>
        public static P4Form LoadFromSpec(string formCommand, string specDef, string formContents, System.Text.Encoding encoding)
        {

            p4dn.Spec spec = new p4dn.Spec(specDef, encoding);
            Dictionary<string, string> ht = null;
            using (p4dn.Error err = new p4dn.Error(encoding))
            {
                ht = spec.Parse(formContents, err);
                if (err.Test())
                {
                    throw new Exceptions.FormParseException(formCommand, err.Fmt());
                }
            }
            return new P4Form(formCommand, specDef, ht, encoding);

        }

        /// <summary>
        /// Gets the form command.
        /// </summary>
        /// <value>The command that was executed when the form was run.</value>
        public string FormCommand
        {
            get
            {
                return _formCommand;
            }
        }

        /// <summary>
        /// Formats the P4Form as a formated Perforce spec.
        /// </summary>
        /// <returns>String of the formated form spec.</returns>
        public string FormatSpec()
        {
            string ret = null;
            using (p4dn.Error err = new p4dn.Error(_encoding))
            {
                ret = _spec.Format(base.AllFieldDictionary, err);
                if (err.Test())
                {
                    throw new Exceptions.FormParseException(_formCommand, err.Fmt());
                }
            }
            return ret;
        }

        /// <summary>
        /// Gets a StringCollection containing the names of all the allowable fields in the form.
        /// </summary>
        /// <remarks>
        /// The fields returned by PermittedFields are not necessarily included in Fields.Keys or
        /// ArrayFields.Keys.  Perforce forms usually do not have keys for empty values.
        /// </remarks>
        /// <value>StringCollection containing the names of all the allowable fields in the form.</value>
        public StringCollection PermittedFields
        {
            get
            {
                // From P4Ruby:
                // There's no trivial way to do this using the API (and get it right), so
                // for now, we parse the string manually. We're ignoring the type of 
                // the field, and any constraints it may be under; what we're interested
                // in is solely the field name

                int fieldPos = 0;
                int codePos = 0;
                StringCollection sc = new StringCollection();
                while (true)
                {
                    codePos = _specdef.IndexOf(";", fieldPos);
                    if (codePos == -1) break;

                    sc.Add(_specdef.Substring(fieldPos, codePos - fieldPos));
                    fieldPos = _specdef.IndexOf(";;", fieldPos);
                    if (fieldPos == -1) break;
                    fieldPos += 2;
                }
                return sc;
            }
        }

        /// <summary>
        /// The underlying C++ API 'specdef' defining the form.
        /// </summary>
        /// <value>The C++ 'specdef' that defines the format of the spec.</value>
        /// <remarks>
        /// The SpecDef is a formatted string that Perforce uses internally to parse forms.
        /// SpecDefs will vary by form, server version, and even with specific server configurations.
        /// In general, you should avoid using a SpecDef directly; however, in some cases you 
        /// can avoid a server call by using the SpecDef.
        /// </remarks>
        public string SpecDef
        {
            get
            {
                return _specdef;
            }
        }

        /// <summary>
        /// Copies the P4Form object into a new instance.
        /// </summary>
        /// <returns>A copy of the P4Form object.</returns>
        public P4Form Clone()
        {
            P4Form clone = new P4Form(_formCommand, _specdef, base.AllFieldDictionary, _encoding);
            return clone;
        }
    }
}
