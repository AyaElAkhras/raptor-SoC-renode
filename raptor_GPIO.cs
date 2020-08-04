
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
    public class RaptorGPIO : BaseGPIOPort
    {
    	public RaptorGPIO(Machine machine) : base(machine, NumberOfPorts)
        {

        }


        public override void Reset()
        {
            base.Reset();
        }


	private readonly Type type;
	private const int NumberOfPorts = 1;  // to be confirmed 

	private enum Registers
        {
            GPIO_DATA = 0x80000000,  // GPIO input/output (lower 16 bits) GPIO output readback (upper 16 bits)
            GPIO_ENB = 0x80000004, // GPIO output enable (0 = output, 1 = input)
            GPIO_PUB = 0x80000008, // GPIO pullup enable (0 = pullup, 1 = none)
            GPIO_PDB = 0x8000000c // GPIO pulldown enable (0 = pulldown, 1 = none)
        }


        public enum Type
        {
            In,
            Out,
            InOut
        }

       
    }
}
