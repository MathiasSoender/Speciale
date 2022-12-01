////////////////////////////////////////////////////////////////////////////////
// kkp.cpp
//   Implementation of main parsing functions.
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

#include <stdint.h>
#include <cstdio>
#include <ctime>
#include <cstdlib>

#include <vector>
#include <algorithm>

#include "kkp.h"
#include "SA_streamer.h"
#include <iostream>

#define STACK_BITS 16
#define STACK_SIZE (1 << STACK_BITS)
#define STACK_HALF (1 << (STACK_BITS - 1))
#define STACK_MASK ((STACK_SIZE) - 1)

// An auxiliary routine used during parsing.
int parse_phrase(unsigned char* X, int n, int i, int psv, int nsv, int* phrasePositions, int* phraseLengths, int nfactors);


// TODO: current version overwrites SA, this can
// be avoided, similarly as in KKP2.
int kkp3(unsigned char* X, int*& SA, int n, int* phrasePositions, int* phraseLengths) {
    if (n == 0) return 0;
    int* CPSS = new int[2 * n + 5];




    // This can be avoided too.
    for (int i = n; i; --i) {
        SA[i] = SA[i - 1];
        
    }
    SA[0] = SA[n + 1] = -1;


    
    // Compute PSV_text and NSV_text for SA.
    int top = 0;
    for (int i = 1; i <= n + 1; ++i) {
        while (SA[top] > SA[i]) {
            int addr = (SA[top] << 1);
            CPSS[addr] = SA[top - 1];
            CPSS[addr + 1] = SA[i];

            --top;

        }
        ++top;
        SA[top] = SA[i];
    }


    // Compute the phrases.
    phrasePositions[0] = X[0];
    phraseLengths[0] = 0;
    int i = 1, nfactors = 1;
    while (i < n) {
        int addr = (i << 1);
        int psv = CPSS[addr];
        int nsv = CPSS[addr + 1];


        i = parse_phrase(X, n, i, psv, nsv, phrasePositions, phraseLengths, nfactors);
        ++nfactors;
    }

    // Clean up.
    delete[] CPSS;
    return nfactors;
}


int parse_phrase(unsigned char* X, int n, int i, int psv, int nsv, int* phrasePositions, int* phraseLengths, int nfactors) {
    int pos, len = 0;
    if (nsv == -1) {
        while (X[psv + len] == X[i + len]) ++len;
        pos = psv;
    }
    else if (psv == -1) {
        while (i + len < n && X[nsv + len] == X[i + len]) ++len;
        pos = nsv;
    }
    else {
        while (X[psv + len] == X[nsv + len] && (psv != nsv)) ++len;
        if (X[i + len] == X[psv + len]) {
            ++len;
            while (X[i + len] == X[psv + len]) ++len;
            pos = psv;
        }
        else {
            while (i + len < n && X[i + len] == X[nsv + len]) ++len;
            pos = nsv;
        }
    }
    if (len == 0) pos = X[i];
    phraseLengths[nfactors] = len;
    phrasePositions[nfactors] = pos;

    return i + std::max(1, len);
}
