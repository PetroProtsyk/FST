# Finite State Transducer (FST) Library for .NET Core
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) [![Travis Status](https://travis-ci.com/PetroProtsyk/FST.svg?branch=master)](https://travis-ci.com/PetroProtsyk/FST)

C# implementation of Finite State Transducer for use in full-text search tasks

# Command Line Arguments

   ```bat
    dotnet run --configuration=Release -- build -i ../../Datasets/Simple/airports.txt
   ```

   ```txt
    FST constructed time: 00:00:02.5726407, terms: 46894, cache size: 65000, Memory: 71503872, output size: 949500
    FST (memory) verification time: 00:00:00.2863610
    FST (file)   verification time: 00:00:00.2789775
   ```

   ```bat
    dotnet run --configuration=Release -- print -p "Lely*"
   ```

   ```txt
    FST header terms: 46894, max length: 95, states: 162860
    Lelygebergte Airstrip->323787
    Lelystad Airport->2522
    FST print terms: 2, time: 00:00:00.0708254
   ```

# References

* [Index 1,600,000,000 Keys with Automata and Rust](https://blog.burntsushi.net/transducers)
* [Using Finite State Transducers in Lucene](http://blog.mikemccandless.com/2010/12/using-finite-state-transducers-in.html)
* S. Mihov, D. Maurel, [Direct Construction of Minimal Acyclic Subsequential Transducers](http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.24.3698&rep=rep1&type=pdf)
