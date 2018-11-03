# FST
C# implementation of Finite State Transducers for use in full-text search tasks

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

