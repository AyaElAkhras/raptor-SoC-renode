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
    public raptor_SPI(Machine machine, uint transmitDepth, uint receiveDepth) : base(machine)
    {
      transmitBuffer = new Queue<ushort>();  // 16 bits 
      receiveBuffer = new Queue<ushort>();

      this.transmitDepth = transmitDepth;

      var registersMap = new Dictionary<long, DoubleWordRegister>
      {
        {(long)Registers.SPIDATA, new DoubleWordRegister(this)
                    .WithReservedBits(16, 16)
                    .WithValueField(0, 16, valueProviderCallback: _ =>    // whenever the field is read, dequeue from receieve buffer 
                    {
                        if(!start.Value)
                        {
                            this.Log(LogLevel.Warning, "Trying to read value from a disabled SPI");
                            return 0;
                        }
                        
                        if(!TryDequeueFromReceiveBuffer(out var data))
                        {
                            this.Log(LogLevel.Warning, "Trying to read from an empty FIFO");
                            return 0;
                        }

                        return data; 
                    },
                    writeCallback: (_, val) =>    // whenever the register is written to, enqueue to transmit buffer 
                    {
                        if(!start.Value)
                        {
                            this.Log(LogLevel.Warning, "Cannot write to SPI buffer while disabled");
                            return;
                        }
                    
                        EnqueueToTransmitBuffer((ushort)val);
                    }, name: "DATA")
                   
        },
         
        {(long)Registers.SPICTRL, new DoubleWordRegister(this)
                  .WithFlag(0, out start, changeCallback: (_, val) =>
                    {
                        if(val == false)
                        {
                            ClearBuffers();
                        }
                    }, name: "START")
                    
//                     .WithFlag(1, , changeCallback: (_, val) =>
//                     {
//                         if(val == false)
//                         {
//                             ClearBuffers();
//                         }
//                     }, name: "SS")  
        },
        
        {(long)Registers.SPICFG, new DoubleWordRegister(this)
                    
        },
        
        {(long)Registers.SPISTATUS, new DoubleWordRegister(this)
                    
        }
          
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
    
    private IFlagRegisterField start; // start or enabler of the SPI
    
    private enum Registers
    {
      SPIDATA = 0x0,
      SPICTRL = 0x4,  // 0:Start, 1:Slave Select (SS0 bit)
      SPICFG = 0X8,
      SPISTATUS = 0x10  // 0:Done
    }
  
  }
}
