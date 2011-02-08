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
using System.Collections.Generic;
using System.Text;

namespace P4API
{
    internal class P4FormRecordSet : P4BaseRecordSet
    {
        private string _FormCommand;
        private System.Text.Encoding _encoding;
        private P4Form _Form;


        internal P4FormRecordSet(string FormCommand, System.Text.Encoding encoding)
        {
            // clone this so we don't hold a reference to another object's encoding object
            // preventing it from Garbage collecting.
            _encoding = (System.Text.Encoding) encoding.Clone();
            _FormCommand = FormCommand;
        }
        internal override void Finished()
        {
            if (base.TaggedOutputs.Count != 1 || base.SpecDef == null)
            {
                throw new Exceptions.FormFetchException(_FormCommand, "Unexpected output attemting to fetch form!");
            }
            Dictionary<string, string> ht = base.TaggedOutputs[0].AllFieldDictionary;
            _Form = new P4Form(_FormCommand, base.SpecDef, ht, _encoding);
        }
        internal P4Form Form
        {
            get
            {
                return _Form;
            }
        }
    }
}
