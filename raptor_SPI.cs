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
  /////////////////////////////////////////////////////////////////////
  
    public raptor_SPI(Machine machine, uint transmitDepth, uint receiveDepth) : base(machine)
    {
      transmitBuffer = new Queue<ushort>();  // 16 bits 
      receiveBuffer = new Queue<ushort>();

      this.transmitDepth = transmitDepth;

      var registersMap = new Dictionary<long, DoubleWordRegister>
      {
        
        {(long)Registers.DataRegister, new DoubleWordRegister(this)
                    .WithReservedBits(16, 16)
                    .WithValueField(0, 16, valueProviderCallback: _ =>    // whenever the field is read, dequeue from receieve buffer 
                    {
                        if(!TryDequeueFromReceiveBuffer(out var data))
                        {
                            this.Log(LogLevel.Warning, "Trying to read from an empty FIFO");
                            return 0;
                        }

                        return data;
                    },
                    writeCallback: (_, val) =>    // whenever the register is written to, enqueue to transmit buffer 
                    {
                        EnqueueToTransmitBuffer((ushort)val);
                    }, name: "DATA")
        },
        
        
        
        
        
        
        
        
        
      };

      registersCollection = new DoubleWordRegisterCollection(this, registersMap);
            
      }

  
 
  
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













  
