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

        @ Bit-position equates (for setting or clearing a single bit)

        .equ  BIT0,    0x00000001
        .equ  BIT1,    0x00000002
        .equ  BIT2,    0x00000004
        .equ  BIT3,    0x00000008
        .equ  BIT4,    0x00000010
        .equ  BIT5,    0x00000020
        .equ  BIT6,    0x00000040
        .equ  BIT7,    0x00000080
        .equ  BIT8,    0x00000100
        .equ  BIT9,    0x00000200
        .equ  BIT10,   0x00000400
        .equ  BIT11,   0x00000800
        .equ  BIT12,   0x00001000
        .equ  BIT13,   0x00002000
        .equ  BIT14,   0x00004000
        .equ  BIT15,   0x00008000
        .equ  BIT16,   0x00010000
        .equ  BIT17,   0x00020000
        .equ  BIT18,   0x00040000
        .equ  BIT19,   0x00080000
        .equ  BIT20,   0x00100000
        .equ  BIT21,   0x00200000
        .equ  BIT22,   0x00400000
        .equ  BIT23,   0x00800000
        .equ  BIT24,   0x01000000
        .equ  BIT25,   0x02000000
        .equ  BIT26,   0x04000000
        .equ  BIT27,   0x08000000
        .equ  BIT28,   0x10000000
        .equ  BIT29,   0x20000000
        .equ  BIT30,   0x40000000
        .equ  BIT31,   0x80000000


@ Registerdefinitionen

@ #define PERIPH_BASE           ((uint32_t)0x40000000) /*!< SRAM base address in the bit-band region */
@ #define APB1PERIPH_BASE       PERIPH_BASE
@ #define APB2PERIPH_BASE       (PERIPH_BASE + 0x10000)
@ #define AHBPERIPH_BASE        (PERIPH_BASE + 0x20000)
@ #define RCC_BASE              (AHBPERIPH_BASE + 0x1000)
@ #define GPIOA_BASE            (APB2PERIPH_BASE + 0x0800)
@ #define USART1_BASE           (APB2PERIPH_BASE + 0x3800)

        .equ RCC_BASE        ,   0x40021000
        .equ RCC_APB2ENR     ,   RCC_BASE + 0x18

          .equ AFIOEN, 0x0001
          .equ IOPAEN, 0x0004
          .equ IOPBEN, 0x0008
          .equ IOPCEN, 0x0010
          .equ IOPDEN, 0x0020
          .equ IOPEEN, 0x0040
          .equ IOPFEN, 0x0080
          .equ IOPGEN, 0x0100
          .equ USART1EN, 0x4000

        .equ USART1_BASE     ,   0x40013800
        .equ USART1_SR       ,   USART1_BASE + 0x00
        .equ USART1_DR       ,   USART1_BASE + 0x04
        .equ USART1_BRR      ,   USART1_BASE + 0x08
        .equ USART1_CR1      ,   USART1_BASE + 0x0c
        .equ USART1_CR2      ,   USART1_BASE + 0x10
        .equ USART1_CR3      ,   USART1_BASE + 0x14
        .equ USART1_GTPR     ,   USART1_BASE + 0x18

          .equ RXNE            ,   BIT5
          .equ TC              ,   BIT6
          .equ TXE             ,   BIT7

        .equ GPIOA_BASE      ,   0x40010800
        .equ GPIOA_CRL       ,   GPIOA_BASE + 0x00
        .equ GPIOA_CRH       ,   GPIOA_BASE + 0x04
        .equ GPIOA_IDR       ,   GPIOA_BASE + 0x08
        .equ GPIOA_ODR       ,   GPIOA_BASE + 0x0C
        .equ GPIOA_BSRR      ,   GPIOA_BASE + 0x10
        .equ GPIOA_BRR       ,   GPIOA_BASE + 0x14
        .equ GPIOA_LCKR      ,   GPIOA_BASE + 0x18

@ -----------------------------------------------------------------------------
uart_init:
@ -----------------------------------------------------------------------------

  @ Most of the peripherals are connected to APB2.  Turn on the
  @ clocks for the interesting peripherals and all GPIOs.
  ldr r6, = RCC_APB2ENR
  ldr r0, = AFIOEN|IOPAEN|IOPBEN|IOPCEN|IOPDEN|USART1EN  @ |IOPEEN|IOPFEN|IOPGEN|
  str r0, [r6]

  @ Set PORTA pins in alternate function mode
  @ Put PA9  (TX) to alternate function output push-pull at 50 MHz
  @ Put PA10 (RX) to floating input
  ldr r6, = GPIOA_CRH
  ldr r0, = 0x000004B0
  str r0, [r6]

  @ Configure BRR by deviding the bus clock with the baud rate

  ldr r6, = USART1_BRR
  @ ldr r0, = 0x00000341  @  9600 bps
  @ ldr r0, = 0x000000D0  @ 38400 bps
  @ ldr r0, = 0x00000045  @ 115200 bps
  ldr r0, = 0x00000046  @ 115200 bps, ein ganz kleines bisschen langsamer...
  str r0, [r6]

  @ Enable the USART, TX, and RX circuit
  ldr r6, =USART1_CR1
  ldr r0, =BIT13+BIT3+BIT2 @ USART_CR1_UE | USART_CR1_TE | USART_CR1_RE
  str r0, [r6]

  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "emit"
emit: @ ( c -- ) Sendet Wert in r0
@ -----------------------------------------------------------------------------
   push {r0, r1, r2}
   popda r0

   ldr r2, =USART1_SR
        
1: ldr r1, [r2]           @ Load USART status register
   ands r1, #TXE          @ Transmit buffer empty?
   beq 1b                 @ loop until buffer is empty

   ldr r2, =USART1_DR
   strb r0, [r2]          @ Output the character

   pop {r0, r1, r2}
   bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "key"
key: @ ( -- c ) Empfängt Wert in r0
@ -----------------------------------------------------------------------------
   push {r0, r1, r2}

   ldr r2, =USART1_SR
        
1: ldr r1, [r2]           @ Load USART status register
   ands r1, #RXNE        @ Receive buffer not empty ?
   beq 1b                 @ Loop until a character arrives

   ldr r2, =USART1_DR
   ldrb r0, [r2]          @ Fetch the character

   pushda r0
   pop {r0, r1, r2}
   bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "?key"
  @ ( -- ? ) Ist eine Taste gedrückt ?
@ -----------------------------------------------------------------------------
   ldr r0, =USART1_SR
   ldr r1, [r0]     @ Fetch status
   ands r1, #RXNE
   beq 1f
     pushdaconst -1
     bx lr

1: pushdaconst 0
   bx lr


  .ltorg @ Hier werden viele spezielle Hardwarestellenkonstanten gebraucht, schreibe sie gleich !
