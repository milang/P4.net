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

//
// Copyright 2010 Jacob Gladish. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
//    1. Redistributions of source code must retain the above copyright notice, this list of
//       conditions and the following disclaimer.
// 
//    2. Redistributions in binary form must reproduce the above copyright notice, this list
//       of conditions and the following disclaimer in the documentation and/or other materials
//       provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY Jacob Gladish ``AS IS'' AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// The views and conclusions contained in the software and documentation are those of the
// authors and should not be interpreted as representing official policies, either expressed
// or implied, of <copyright holder>.
// 
#pragma once

#include "Error_m.h"
#include "ClientUser_m.h"
#include "KeepAlive_m.h"
#include "ClientUserDelegate.h"
#include "Spec_m.h"

using namespace System::Runtime::InteropServices;

namespace p4dn {

    //================================================================
    // 
    // class for callbacks into the KeepAlive interfaces
    //
	class KeepAliveDelegate : public ::KeepAlive 
		{
        private:
            
		// handle to managed KeepAlive class
        gcroot<p4dn::KeepAlive^> _KeepAlive;
        public:
            KeepAliveDelegate() 
			{
				 _KeepAlive = NULL;
			}
			
			void SetKeepAlive(gcroot<p4dn::KeepAlive^> mKeepAlive) 
			{
				_KeepAlive = mKeepAlive;
			}
            
			~KeepAliveDelegate()
			{
				// Free the GC refererence
				delete _KeepAlive;
			}
            
			int IsAlive() {
				
			if ((p4dn::KeepAlive^)_KeepAlive == nullptr){
					return 1;
				}
                bool b = _KeepAlive->IsAlive();
				return ( b ? 1 : 0 );
            }
        };

	public ref class ClientApi : public System::IDisposable 
    {

    public:

        ClientApi();
        ~ClientApi();

        void              __clrcall SetTrans( int output, int content, int fnames, int dialog );
		void              __clrcall SetProtocol( System::String^ p, System::String^ v );
        void              __clrcall SetProtocolV( System::String^ p );
		System::String^	  __clrcall GetProtocol( System::String^ v );

        void              __clrcall Init( p4dn::Error^ e );
        void              __clrcall Run(System::String^ func, p4dn::ClientUser^ ui );
        int               __clrcall  Final(p4dn::Error^ e );
        int	              __clrcall Dropped();
		p4dn::Error^      __clrcall CreateError();
		p4dn::Spec^       __clrcall CreateSpec(System::String^ specDef);

		void              __clrcall SetVar(System::String^ var, System::String^ val);
		void              __clrcall SetTag();

        void              __clrcall SetCharset( System::String^ c );
        void              __clrcall SetClient( System::String^ c );
        void              __clrcall SetCwd( System::String^ c );
        void              __clrcall SetHost( System::String^ c );
        void              __clrcall SetLanguage( System::String^ c );
        void              __clrcall SetPassword( System::String^ c );
        void              __clrcall SetPort( System::String^ c );
		void              __clrcall SetProg( System::String^ c );
		void              __clrcall SetVersion( System::String^ c );
		void              __clrcall SetTicketFile( System::String^ c );
        void              __clrcall SetUser( System::String^ c );
        void              __clrcall SetArgv( array<System::String^>^ args );
        void              __clrcall SetBreak( p4dn::KeepAlive^ keepAlive );

		void              __clrcall SetMaxResults(int maxResults);
		void              __clrcall SetMaxScanRows(int maxScanRows);
		void              __clrcall SetMaxLockTime(int maxLockTime);

        void              __clrcall DefineCharset( System::String^ c, p4dn::Error^ e );
        void              __clrcall DefineClient( System::String^ c, p4dn::Error^ e );
        void              __clrcall DefineHost( System::String^ c, p4dn::Error^ e );
        void              __clrcall DefineLanguage( System::String^ c, p4dn::Error^ e );
        void              __clrcall DefinePassword( System::String^ c, p4dn::Error^ e );
        void              __clrcall DefinePort( System::String^ c, p4dn::Error^ e );
        void              __clrcall DefineUser( System::String^ c, p4dn::Error^ e );        

		property System::String^ Charset     
		{ 
			System::String^ get() 
				{ 
					return gcnew System::String(_clientApi->GetCharset().Text()); 
				} 
		}
		property System::String^ Client      
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetClient().Text()); 
			} 
		}
		property System::String^ Cwd         
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetCwd().Text()); 
			} 
		}
		property System::String^ Host        
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetHost().Text()); 
			} 
		}
		property System::String^ Language    
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetLanguage().Text()); 
			} 
		}
		property System::String^ Os          
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetOs().Text()); 
			}
		}
		property System::String^ Password    
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetPassword().Text()); 
			}
		}
		property System::String^ Port        
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetPort().Text()); 
			} 
		}
		property System::String^ User        
		{ 
			System::String^ get() 
			{ 
				return gcnew System::String(_clientApi->GetUser().Text()); 
			} 
		}
		property System::Text::Encoding^ Encoding     
		{ 
			System::Text::Encoding^ get() 
			{ 
				return _encoding; 
			} 
		}

    private:
        ::ClientApi*   __clrcall    getClientApi();
		void           __clrcall    CleanUp();
		
		::ClientApi*				_clientApi;		
		System::Text::Encoding^		_encoding;
		bool						_Disposed;
        KeepAliveDelegate*			_keepAliveDelegate;
    };
}
