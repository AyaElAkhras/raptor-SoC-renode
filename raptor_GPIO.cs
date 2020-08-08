
/* Raptor design specifications 

General purpose configurable digital I/O with pullup/pulldown,
input or output, and enable/disable.

The GPIO pins are sixteen assignable digital inputs or outputs. 

All writes to reg_gpio_data are registered. 

All reads from reg_gpio_data are immediate.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Utilities;

namespace Antmicro.Renode.Peripherals.GPIOPort
{
    public class RaptorGPIO : BaseGPIOPort, IDoubleWordPeripheral, IKnownSize
    {
    	public RaptorGPIO(Machine machine) : base(machine, NumberOfPins)
        {
		locker = new object();
		
// 		var registersMap = new Dictionary<long, DoubleWordRegister>
//             	{
// 			{(long)Registers.PinValue, new DoubleWordRegister(this)
// 			    .WithValueField(0, 32,
// 				valueProviderCallback: _ =>
// 				{
// 				    var readOperations = pins.Select(x => (x.pinOperation & Operation.Read) != 0);
// 				    var result = readOperations.Zip(State, (operation, state) => operation && state);
// 				    return BitHelper.GetValueFromBitsArray(result);
// 				})
// 			},

// 			{(long)Registers.PinInputEnable, new DoubleWordRegister(this)
// 			    .WithValueField(0, 32,
// 				writeCallback: (_, val) =>
// 				{
// 				    var bits = BitHelper.GetBits(val);
// 				    for(var i = 0; i < bits.Length; i++)
// 				    {
// 					Misc.FlipFlag(ref pins[i].pinOperation, Operation.Read, bits[i]);
// 				    }
// 				})
// 			},

// 			{(long)Registers.PinOutputEnable, new DoubleWordRegister(this)
// 			    .WithValueField(0, 32,
// 				    writeCallback: (_, val) =>
// 				{
// 				    var bits = BitHelper.GetBits(val);
// 				    for (var i = 0; i < bits.Length; i++)
// 				    {
// 					Misc.FlipFlag(ref pins[i].pinOperation, Operation.Write, bits[i]);
// 				    }
// 				})
// 			},

// 			{(long)Registers.OutputPortValue, new DoubleWordRegister(this)
// 			    .WithValueField(0, 32,
// 				writeCallback: (_, val) =>
// 				{
// 				    lock(locker)
// 				    {
// 					var bits = BitHelper.GetBits(val);
// 					for(var i = 0; i < bits.Length; i++)
// 					{
// 					    if((pins[i].pinOperation & Operation.Write) != 0)
// 					    {
// 						State[i] = bits[i];
// 						Connections[i].Set(bits[i]);
// 					    }
// 					}
// 				    }
// 				})
// 			},

// 			{(long)Registers.RiseInterruptPending, new DoubleWordRegister(this)
// 			    .WithValueField(0, 32, writeCallback: (_, val) =>
// 			    {
// 				lock(locker)
// 				{
// 				    var bits = BitHelper.GetBits(val);
// 				    for(var i = 0; i < bits.Length; i++)
// 				    {
// 					if(bits[i])
// 					{
// 					    Connections[i].Set(State[i]);
// 					}
// 				    }
// 				}
// 			    })
// 			}
// 		};

		registers = new DoubleWordRegisterCollection(this, registersMap);
        }
	
	
	public uint ReadDoubleWord(long offset)
        {
            return RegistersCollection.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            RegistersCollection.Write(offset, value);
        }
	
	public override void Reset()
        {
            lock(locker)
            {
                base.Reset();
                registers.Reset();
            }
        }
	
	private readonly DoubleWordRegisterCollection registers;
        private readonly object locker;   // for blocking until the process is done 
        // private readonly Pin[] pins;

	private const int NumberOfPins = 16;  // to be confirmed 

	private enum Registers : long
        {
            GPIO_DATA = 0x80000000,  // GPIO input/output (lower 16 bits) GPIO output readback (upper 16 bits)
            GPIO_ENB = 0x80000004, // GPIO output enable (0 = output, 1 = input)
            GPIO_PUB = 0x80000008, // GPIO pullup enable (0 = pullup, 1 = none)
            GPIO_PDB = 0x8000000c // GPIO pulldown enable (0 = pulldown, 1 = none)
        }


        
       
    }
}
