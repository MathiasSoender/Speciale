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


void SingleGeneration(char* file, char* algo, std::string savetofile) {

    // Read the text and its suffix array.
    unsigned char* text;
    int* sa = NULL, length;

    read_text(file, text, length);

    // Compute the size of LZ77 parsing.
    std::clock_t timestamp;
    long double wtimestamp;
    std::string alg = algo;


    std::vector<std::pair<int, int>>* output;
    output = (!savetofile.empty()) ? new std::vector<std::pair<int, int>> : NULL;


    int nphrases;
    if (alg == "kkp3") {
        wtimestamp = wclock();
        read_sa(file, sa, length);

        std::cerr << "Running algorithm kkp3...\n";
        timestamp = std::clock();
        nphrases = kkp3(text, sa, length, output);
    }
    else if (alg == "kkp2") {
        wtimestamp = wclock();
        read_sa(file, sa, length);
        std::cerr << "Running algorithm kkp2...\n";
        timestamp = std::clock();
        nphrases = kkp2(text, sa, length, output);
    }
    else if (alg == "kkp1s") {
        std::cerr << "Running algorithm kkp1s...\n";
        timestamp = std::clock();
        wtimestamp = wclock();
        nphrases = kkp1s(text, length, std::string(file) + ".sa", output);
    }
    else {
        std::cerr << "\nError: unrecognized algorithm name\n";
        std::exit(EXIT_FAILURE);
    }
    std::cerr << "CPU time: " << elapsed(timestamp) << "s\n";
    std::cerr << "Wallclock time including SA reading: "
        << welapsed(wtimestamp) << "s\n";
    std::cerr << "Number of phrases = " << nphrases << std::endl;


    if (!savetofile.empty()) {
        std::ofstream phaseFile(savetofile);
        for (const auto& ele : *output) {
            phaseFile << ele.first << " " << ele.second << "\n";
        }
        phaseFile.close();
    }


    // Clean up.
    if (sa) {
        delete[] sa;
    }
    delete[] text;

}


void SuffixesGeneration(char* file, char* algo, std::string savetofile) {
    std::string alg = algo;

    if (alg != "kkp3") {
        std::cerr << "Generation of all suffixes can only be done using kkp3 algorithm.";
        return;
    }

    unsigned char* text;
    int length;
    read_text(file, text, length);

    for (int i{ 0 }; i < length; i++) {
        std::vector<std::pair<int, int>>* output = new std::vector<std::pair<int, int>>;

        std::vector<unsigned char> bufferVector(length - i + 1);
        unsigned char* bufferPtr = &bufferVector[0];

        memcpy(bufferPtr, &text[i], length - i);
        bufferPtr[length - i] = '\0';

        int* sa = NULL;
        std::string tempfile = std::string(file) + std::to_string(i);
        read_sa(tempfile.c_str(), sa, length - i);


        int nphrases = kkp3(bufferPtr, sa, length - i, output);

        std::ofstream phaseFile(savetofile + std::to_string(i));
        for (const auto& ele : *output) {
            phaseFile << ele.first << " " << ele.second << "\n";
        }
        phaseFile.close();
        output->clear();

    }
    delete[] text;



}




int main(int argc, char** argv) {
    // Check arguments.
    if (argc != 4 && argc != 5) {
        std::cerr << "usage: " << argv[0] << " infile [algorithm] [save2file] [type]  \n\n"
            << "Computes the size of LZ77 parsing of infile using       \n"
            << "selected algorithm (Mandatory). Available algorithms are:\n"
            << "  kkp3  -- the fastest, uses 13n bytes                   \n"
            << "  kkp2  -- slower than kkp3 but uses only 9n bytes       \n"
            << "  kkp1s -- semi-external version of kkp2, uses 5n bytes \n"
            << "  Set [save2file] as path to save files at. Otherwise does not save. \n"
            << "  Set [type] to 'all' to compute LZ for all suffixes of infile, or 'single' for just the infile.  (save2file must be specified for multiple)\n";

        exit(EXIT_FAILURE);
    }
    std::string savetofile = (argc == 5) ? argv[3] : "";
    
    if (strcmp(argv[4], "single") == 0) {
        SingleGeneration(argv[1], argv[2], savetofile);
    }
    else if (strcmp(argv[4], "all") == 0) {

        SuffixesGeneration(argv[1], argv[2], savetofile);
    }
    else {
        std::cerr << "Could not understand [type]";
    }
    return EXIT_SUCCESS;
}
