/*  Raptor UART Design Specifications:

1) Each UART uses two FIFOs as buffers for both sending and receiving data.

2) Reading from the FIFO while being empty would block the execution 
till the UART receives data. Same behavior is observed when trying to 
write to the transmission FIFO while being full. 

3) The UART generates an interrupt, which is driven HIGH whenever 
there is data to be read on the receiving FIFO. The interrupt is cleared
when the data is read. 
UART0 uses IRQ0 (INT16) and UART1 uses IRQ1 (INT17).

4) Data is 8 bits and there is 1 stop bit 

*/

using System;
using System.Collections.Generic;

namespace Antmicro.Renode.Peripherals.UART
{
	public class Raptor_UART : UARTBase
	{
		public Raptor_UART(Machine machine, uint frequency = 50000000) : base(machine)
		{
			this.frequency = frequency;
			//DefineRegisters();
		}

		public override void WriteChar(byte value)
        {
        	if(receiveFifo.Count == 10)  // fifo is full, so block until empty
        	{
				// todo: block execution until a place is emptied
        	}

        	receiveFifo.Enqueue(value);
        	// todo: raise an interrupt

        }

        public byte ReadChar()
        {
        	if(sendFifo.Count == 0)
        	{
        		// todo: block execution until a it becomes not empty
        	}
        	return sendFifo.Dequeue();
        }

        public override void Reset()   
        {
            base.Reset();
            receiveFifo.Clear();
            sendFifo.Clear();
        }

		// todo
		// Process read and write with interrupt

		private enum Registers   // Need to check the addresses 
		{
		    BaudRate = 0x10,
		    Status = 0x14,
		    Control = 0x18,
		    Data = 0x1c,
		    Fifo = 0x28
		}
		

		public override Parity ParityBit
		{
			get
			{	// No parity
				return Parity.None;
			}
		}

		public override Bits StopBits
    	{
	        get 
	        {
	        	// 1 stop bit 
	        	return Bits.One;
	        }
	    }	

		public override uint BaudRate
		{
			get
			{
				// baud rate = sysclk/163*1
				return frequency / 163*16;
			}
		}

		private readonly uint frequency;  // supposing this is the sysclk
		
		// 2 fifos for sending and receiving data
		private readonly Queue<byte> receiveFifo = new Queue<byte>();
		private Queue<byte> sendFifo = new Queue<byte>();

	}

}
