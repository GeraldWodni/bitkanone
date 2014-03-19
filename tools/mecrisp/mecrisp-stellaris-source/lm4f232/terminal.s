@
@    Mecrisp-Stellaris - A native code Forth implementation for ARM-Cortex M microcontrollers
@    Copyright (C) 2013  Matthias Koch
@
@    This program is free software: you can redistribute it and/or modify
@    it under the terms of the GNU General Public License as published by
@    the Free Software Foundation, either version 3 of the License, or
@    (at your option) any later version.
@
@    This program is distributed in the hope that it will be useful,
@    but WITHOUT ANY WARRANTY; without even the implied warranty of
@    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@    GNU General Public License for more details.
@
@    You should have received a copy of the GNU General Public License
@    along with this program.  If not, see <http://www.gnu.org/licenses/>.
@

@ Terminalroutinen
@ Terminal code and initialisations.
@ Porting: Rewrite this !

.equ RCGCGPIO,    0x400FE608
.equ RCGCUART,    0x400FE618

@ .equ GPIOA_BASE, 0x40004000
@ .equ GPIOAFSEL,  0x40004420
@ .equ GPIODEN,    0x4000451C

@ albert 20131012 modify to uart5 for LM4F232H5QC
.equ GPIOE_BASE, 0x40024000
.equ GPIOAFSEL,  0x40024420
.equ GPIODEN,    0x4002451C

.equ GPIO_PORTE_PCTL_R, 0x4002452C  @ albert 20131014
.equ GPIO_PORTE_AMSEL_R,0x40024528  @ albert 20131014

@ .equ UART0_BASE, 0x4000C000
@ .equ UARTDR,     0x4000C000
@ .equ UARTFR,     0x4000C018
@ .equ UARTIBRD,   0x4000C024
@ .equ UARTFBRD,   0x4000C028
@ .equ UARTLCRH,   0x4000C02C
@ .equ UARTCTL,    0x4000C030
@ .equ UARTCC,     0x4000CFC8

@ albert 20131012 modify to uart5 for LM4F232H5QC
.equ UART5_BASE, 0x40011000
.equ UARTDR,     0x40011000
.equ UARTFR,     0x40011018
.equ UARTIBRD,   0x40011024
.equ UARTFBRD,   0x40011028
.equ UARTLCRH,   0x4001102C
.equ UARTCTL,    0x40011030
.equ UARTCC,     0x40011FC8

@ -----------------------------------------------------------------------------
uart_init: @ ( -- )
@ -----------------------------------------------------------------------------

  @ Allgemeine Systemeinstellungen (General system settings)

  movs r1,#0x20        @ #0x20 ---> UART5 (Albert 20131012) e#1 ---->UART0        @ UART0 aktivieren (enable)
  ldr  r0, =RCGCUART
  str  r1, [r0]

  movs r1, #0x3F      @ Alle GPIO-Ports aktivieren (Enable all GPIO ports)
  ldr  r0, =RCGCGPIO
  str  r1, [r0]

@  movs r1, #3         @ PA0 und PA1 auf UART-Sonderfunktion schalten (Special function switch)
  movs r1, #0x30         @ PE4 und PE5 auf UART-Sonderfunktion schalten (Special function switch) albert 20131013
  ldr  r0, =GPIOAFSEL
  str  r1, [r0]

@ movs r1, #3       @ PA0 und PA1 als digitale Leitungen aktivieren (activate the digital lines)
  movs r1, #0x30    @ PE4 und PE5 als digitale Leitungen aktivieren (activate the digital lines)albert 20131013
  ldr  r0, =GPIODEN
  str  r1, [r0]

@  Note that each pin must be programmed individually; 
@     configure as UART5            @ albert 20131014

    ldr r1, =GPIO_PORTE_PCTL_R      @ R1 = &GPIO_PORTE_PCTL_R
    ldr r0, [r1]                    @ R0 = [R1]
    bic r0, r0, #0x000F0000         @ R0 = R0&~0x00FF0000 (clear port control field for PE4)
    add r0, r0, #0x00010000         @ R0 = R0+0x00110000 (configure PE4 as UART)
    str r0, [r1]                    @ [R1] = R0


    ldr r1, =GPIO_PORTE_PCTL_R      @ R1 = &GPIO_PORTE_PCTL_R
    ldr r0, [r1]                    @ R0 = [R1]
    bic r0, r0, #0x00F00000         @ R0 = R0&~0x00FF0000 (clear port control field for PE5)
    add r0, r0, #0x00100000         @ R0 = R0+0x00110000 (configure PE5 as UART)
    str r0, [r1]                    @ [R1] = R0

  @ UART-Einstellungen vornehmen (make settings)

  movs r1, #0         @ UART anhalten (stop)
  ldr  r0, =UARTCTL
  str  r1, [r0]

  @ Baud rate generation:
  @ 16000000 / (16 * 115200 ) = 1000000 / 115200 = 8.6805
  @ 0.6805... * 64 = 43.5   ~ 44
  @ use 8 and 44

  movs r1, #8
  ldr  r0, =UARTIBRD
  str r1, [r0]

  movs r1, #44
  ldr  r0, =UARTFBRD
  str r1, [r0]

  movs r1, #0x60|0x10  @ 8N1, FIFOs an !
  ldr  r0, =UARTLCRH
  str r1, [r0]

  movs r1, #5        @ PIOSC wählen
  ldr  r0, =UARTCC
  str r1, [r0]

  movs    r1, #0
  ldr     r0, =UARTFR
  str r1, [r0]

  movw r1, #0x301     @ UART starten
  ldr  r0, =UARTCTL
  str  r1, [r0]

  bx lr


@ Werte für den UARTFR-Register  -- Values ​​for the registers UARTFR
.equ RXFE, 0x10 @ Receive  FIFO empty
.equ TXFF, 0x20 @ Transmit FIFO full

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "emit"
emit: @ ( c -- ) Sendet Wert in r0
@ -----------------------------------------------------------------------------
   push {r0, r1}

   ldr r0, =UARTFR
1: ldr r1, [r0]     @ Warte solange der Transmit-FIFO voll ist.
   ands r1, #TXFF
   bne 1b

   ldr r0, =UARTDR  @ Abschicken
   popda r1
   str r1, [r0]

   pop {r0, r1}
   bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "key"
key: @ ( -- c ) Empfängt Wert in r0
@ -----------------------------------------------------------------------------
   push {r0, r1}

   ldr r0, =UARTFR
1: ldr r1, [r0]     @ Warte solange der Receive-FIFO leer ist.
   ands r1, #RXFE
   bne 1b

   ldr r0, =UARTDR    @ Einkommendes Zeichen abholen
   stmdb psp!, {tos}  @ Platz auf dem Datenstack schaffen

   ldr tos, [r0]      @ Register lesen
   uxtb tos, tos      @ 8 Bits davon nehmen, Rest mit Nullen auffüllen.
  
   pop {r0, r1}
   bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "?key"
  @ ( -- ? ) Ist eine Taste gedrückt ?
@ -----------------------------------------------------------------------------
   ldr r0, =UARTFR
   ldr r1, [r0]     @ Warte solange der Receive-FIFO leer ist.
   ands r1, #RXFE
   bne 1f
     pushdaconst -1
     bx lr

1: pushdaconst 0
   bx lr


  .ltorg @ Hier werden viele spezielle Hardwarestellenkonstanten gebraucht, schreibe sie gleich !
         @ Here many special hardware locations constants are used, they write the same!
