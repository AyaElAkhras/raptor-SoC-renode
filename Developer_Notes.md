#### This file contains some stuff I explored in the process of developing peripherals for raptor-SoC.. They could be helpful for any developer willing to add support for their SoC in Renode.. 


1. To allow the CPU to access the developed peripherals, Renode provides various interfaces that define methods allowing for CPU access. These fall under the family of `IBusPeripheral` interfaces which can be found in the following directory:
      renode/src/Infrastructure/src/Emulator/Main/Peripherals/Bus
      
There are 3 types of these interfaces IBytePeripheral (for 8 bit access), IWordPeripheral (for 16 bit access) and IDoubleWordPeripheral (for 32 bit access) varying in access width but all providing the following pair of methods:
      uint ReadDoubleWord(long offset);
      void WriteDoubleWord(long offset, uint value);


2. Renode provides a base class for each peripheral to inherit from. Each peripheral has its base class file in a separate directory under the following path:
    renode/src/Infrastructure/src/Emulator/Main/Peripherals/
If the desired peripheral isn't found among those under that path, then BasicDoubleWordPeripheral can be used as it will provide basic methods.   


3. For defining and mapping registers, an enum called `Registers` (a convention understood by the logging system) is created lsiting all the registers in a peripheral with their addresses. 
Inside the constructor of your peripheral class, create a dictionary of registers that maps the Register's fields to DoubleWordRegister which is a class defined in the following file: 
      renode/src/Infrastructure/src/Emulator/Main/Core/Structure/Registers/PeripheralRegister.cs
      
This dictionary will then be used as a constructor parameter for a DoubleWordRegisterCollection object. This class can be found in the following file:
      renode/src/Infrastructure/src/Emulator/Main/Core/Structure/Registers/RegisterCollection.cs
      
The DoubleWordRegisterCollection object provides Reset, Read, Write methods that are used to implement WriteDoubleWord/ ReadDoubleWord methods.   


4. Following up on 3), for convenience the parameters passed to methods called over the DoubleWordRegister during the mapping of registers are defined below (these could be found in: renode/src/Infrastructure/src/Emulator/Main/Core/Structure/Registers/PeripheralRegister.cs
```
<param name="position">Offset in the register.</param>

<param name="width">Maximum width of the value, in terms of binary representation.</param>

<param name="mode">Access modifiers of this field.</param>

<param name="readCallback">Method to be called whenever the containing register is read. The first parameter is the value of this field before read,
the second parameter is the value after read. Note that it will also be called for unreadable fields.</param>

<param name="writeCallback">Method to be called whenever the containing register is written to. The first parameter is the value of this field before write,
the second parameter is the value written (without any modification). Note that it will also be called for unwrittable fields.</param>

<param name="changeCallback">Method to be called whenever this field's value is changed, either due to read or write. The first parameter is the value of this field before change,the second parameter is the value after change. Note that it will also be called for unwrittable fields.</param>

<param name="valueProviderCallback">Method to be called whenever this field is read. The value passed is the current field's value, that will be overwritten by the value returned from it. This returned value is eventually passed as the first parameter of <paramref name="readCallback"/>.</param>

<param name="name">Ignored parameter, for convenience. Treat it as a comment.</param>
```
        
5. Base classes provide reset methods that could be overridden inside your peripheral class to reset the registers or update the interrupts. 

6. All registers addresses used within a single .cs peripheral design file are relative to the registration point in the .repl file.

7. For details about how to describe the platform in the .repl file, refer to the documentation provided by Renode that can be accessible through this link: 
      https://renode.readthedocs.io/en/latest/basic/describing_platforms.html
