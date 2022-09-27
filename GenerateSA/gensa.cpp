////////////////////////////////////////////////////////////////////////////////
// gensa.cpp
//   Computes the suffix array of a given file.
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
#include <fstream>

#include <cstdlib>
#include <ctime>

#include "divsufsort.h"
#include "../LZ77/common.h"
#include <vector>
#include <string>

void SingleGenerate(char* file) {
    unsigned char* text;
    int length;
    read_text(file, text, length);

    // Alocate and compute the suffix array.  
    int* sa = new int[length];
    if (!sa) {
        std::cerr << "\nError: allocating " << length << " words failed\n";
        std::exit(EXIT_FAILURE);
    }


    std::cerr << "Computing suffix array... ";
    std::clock_t timestamp = std::clock();
    divsufsort(text, sa, length);
    std::cerr << elapsed(timestamp) << " secs\n";



    // Write the output on standard output.
    std::string outfilename = std::string(file) + ".sa";
    std::cerr << "Writing the output to " << outfilename << "... ";
    std::ofstream outfile;

    outfile.open(outfilename, std::ios::binary);
    outfile.write((char*)sa, sizeof(int) * length);
    outfile.close();
    std::cerr << std::endl;

    // Clean up.
    delete[] text;
    delete[] sa;
}

extern "C" {

    __declspec(dllexport) void __stdcall SingleGenerateDLL(unsigned char* text, int length, int* sa) {



        divsufsort(text, sa, length);

        std::ofstream outfile;

    }
    __declspec(dllexport) void __stdcall Free(int* sa) {
        delete[] sa;
    }
}



void SuffixesGenerate(char* file)
{

    unsigned char* text;
    int length;

    read_text(file, text, length);

    for (int i{ 0 }; i < length; i++) {
        
        std::vector<unsigned char> bufferVector(length - i + 1);
        unsigned char* bufferPtr = &bufferVector[0];

        memcpy(bufferPtr, &text[i], length - i);
        bufferPtr[length - i] = '\0';

        int* sa = new int[length - i];
        divsufsort(bufferPtr, sa, length - i);


        std::string outfilename = std::string(file) + std::to_string(i) + ".sa";

        std::ofstream outfile;
        outfile.open(outfilename, std::ios::binary);
        outfile.write((char*)sa, sizeof(int) * (length - i ));
        outfile.close();

        bufferVector.clear();

    }

}



int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: " << argv[0] << " -arg1 infile \n\n";
        std::cerr << "[arg1] = single    for generating SA and outputting to infile.sa";
        std::cerr << "[arg1] = all       for generating SA for all suffixes and outputing to infile.sa.i, for all 'i' suffixes";

        std::exit(EXIT_FAILURE);
    }

    // Read the text.
    if (strcmp(argv[1],"-single") == 0) {
        SingleGenerate(argv[2]);
    }

    else if (strcmp(argv[1], "-all") == 0) {
        SuffixesGenerate(argv[2]);
    }
    else {
        std::cerr << "arg1 not understood";
    }


    return EXIT_SUCCESS;
}

