# raptor-SoC-renode
Support for Raptor (Cortex M0 based SoC) in Renode 

## Raptor Peripherals

1. Two multi-function 32-bit PWM/timers

2. One SPI master controller (AHB bus)

3. One SPI master controller (APB bus)

4. One GPIO port with 16 assignable digital inputs or outputs

5. One I2C master controller-standard mode

6. Two UART with FIFO and baud rate generator

Please note that the current work only supports the uart, SPI and GPIO port peripherals.

## Simple Firmware
A simple firmware is provided to demonstrate and verify the usage of Renode to simulate Raptor SoC peripherlas. The currently provided application demonstrates the usage of the uart peripheral by sending characters of the phrase "Hello Raptor" over it.

### Needed Files
○ raptor-example.c - C application that configures the uart register and sends characters over it
○ cm0_startup.s - Startup code
○ sections.ld - linker script
○ Makefile

### Running the Firmware

#### Required Installations
The application requires arm-none-eabi toolchain which can be installed in Ubuntu using the following command:
    *sudo apt-get install gcc-arm-none-eabi binutils-arm-none-eabi gdb-arm-none-eabi openocd*
    
Instructions for installing Renode can be found through the following link:
    https://renode.readthedocs.io/en/latest/introduction/installing.html

  
#### Steps for Running the Firmware in Renode
Clone this repo, then start Renode and pass to it the renode script (.resc) file that contains all the required commands to create a machine on the simulator, include the peripherals and load the elf file of the application
    *git clone https://github.com/AyaElAkhras/raptor-SoC-renode.git*
    *cd raptor-SoC-renode/*
    *renode raptor-config.resc*

Now to start running the application, in the Renode terminal type
    *start*

To stop running and close the machine, in the Renode terminal type
    *quit*

To re-build the application
    *make clean*
    *make*
