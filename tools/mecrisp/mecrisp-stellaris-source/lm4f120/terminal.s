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

.equ RCGCPIO,    0x400FE608
.equ RCGCUART,   0x400FE618

.equ GPIOA_BASE, 0x40004000
.equ GPIOAFSEL,  0x40004420
.equ GPIODEN,    0x4000451C

.equ UART0_BASE, 0x4000C000
.equ UARTDR,     0x4000C000
.equ UARTFR,     0x4000C018
.equ UARTIBRD,   0x4000C024
.equ UARTFBRD,   0x4000C028
.equ UARTLCRH,   0x4000C02C
.equ UARTCTL,    0x4000C030
.equ UARTCC,     0x4000CFC8

@ -----------------------------------------------------------------------------
uart_init: @ ( -- )
@ -----------------------------------------------------------------------------

  @ Allgemeine Systemeinstellungen

  movs r1, #1         @ UART0 aktivieren
  ldr  r0, =RCGCUART
  str  r1, [r0]

  movs r1, #0x3F      @ Alle GPIO-Ports aktivieren
  ldr  r0, =RCGCPIO
  str  r1, [r0]

  movs r1, #3         @ PA0 und PA1 auf UART-Sonderfunktion schalten
  ldr  r0, =GPIOAFSEL
  str  r1, [r0]

  @ movs r1, #3       @ PA0 und PA1 als digitale Leitungen aktivieren
  ldr  r0, =GPIODEN
  str  r1, [r0]


  @ UART-Einstellungen vornehmen

  movs r1, #0         @ UART anhalten
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


@ Werte für den UARTFR-Register
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
