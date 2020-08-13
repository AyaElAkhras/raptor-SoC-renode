/*
  SPI Master Controller 
*/ 


using System;
using System.Collections.Generic;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Utilities;
using Antmicro.Renode.Exceptions;

namespace Antmicro.Renode.Peripherals.SPI
{
  public class raptor_SPI: SimpleContainer<ISPIPeripheral>, IDoubleWordPeripheral, IKnownSize
  {
  
  
  
  
  
    public uint ReadDoubleWord(long offset)
    {
      return registersCollection.Read(offset);
    }

    public void WriteDoubleWord(long offset, uint value)
    {
      registersCollection.Write(offset, value);
    }
    
    private enum Registers
    {
      SPIDATA = 0x0,
      SPICTRL = 0x4,
      SPICFG = 0X8,
      SPISTATUS = 0x10
    }
  
  }
}













  
