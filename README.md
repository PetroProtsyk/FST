# Finite State Transducer (FST) Library for .NET Core
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) [![Travis Status](https://travis-ci.com/PetroProtsyk/FST.svg?branch=master)](https://travis-ci.com/PetroProtsyk/FST)

C# implementation of Finite State Transducer for use in full-text search tasks

# Command Line Arguments

   ```bat
    dotnet run --configuration=Release -- build -i ../../Datasets/Simple/airports.txt -f Compressed
   ```

   ```txt
    Input read term: 46894, time: 00:00:00.0527490
    FST constructed time: 00:00:00.6492170
    FST verification time: 00:00:00.0529780
    FST written to the output file: output.fst, size: 1180354, time: 00:00:00.0250810
   ```

   ```bat
    dotnet run --configuration=Release -- print -p "Lely*"
   ```

   ```txt
    FST read from: output.fst, time: 00:00:00.0592460
    Lelygebergte Airstrip->323787
    Lelystad Airport->2522
    FST print terms: 2, time: 00:00:00.0475820
   ```
# References

* [Index 1,600,000,000 Keys with Automata and Rust](https://blog.burntsushi.net/transducers)
* [Using Finite State Transducers in Lucene](http://blog.mikemccandless.com/2010/12/using-finite-state-transducers-in.html)
* S. Mihov, D. Maurel, [Direct Construction of Minimal Acyclic Subsequential Transducers](http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.24.3698&rep=rep1&type=pdf)
