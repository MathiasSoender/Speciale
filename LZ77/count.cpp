////////////////////////////////////////////////////////////////////////////////
// count.cpp
//   An example tool computing the size of LZ77 parsing of a given file.
////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013 Juha Karkkainen, Dominik Kempa and Simon J. Puglisi
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

#include <iostream>
#include <cstdlib>
#include <ctime>
#include <fstream>


#include "kkp.h"
#include "common.h"




extern "C" {

    // algorithm == 1 => kkp3
    // algorithm == 2 => kkp2

    __declspec(dllexport) int __stdcall LZ77DLL(unsigned char* text, int* sa, int length, int* phrasePositions, int* phraseLengths, int algorithm) {


        int nphrases = -1;

        if (algorithm == 1)
            nphrases = kkp3(text, sa, length, phrasePositions, phraseLengths);


        return nphrases;
    }

    __declspec(dllexport) void __stdcall Free(int* phrasePositions, int* phraseLengths) {
        free(phrasePositions);
        free(phraseLengths);

        return;

    }


}


