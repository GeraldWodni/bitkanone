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

@ Code generator for M3 and M4

@ -----------------------------------------------------------------------------
movwkomma: @ Register r0: Konstante                                    Constant
           @ Register r3: Zielregister, fertig geschoben zum Verodern  Destination register, readily shifted to be ORed with opcode.
@ -----------------------------------------------------------------------------
  pushdatos    @ Platz auf dem Datenstack schaffen 
  ldr tos, =0xf2400000 @ Opcode movw r0, #0

  movs r1, #0x0000F000  @ Bit 16 - 13
  ands r2, r0, r1       @ aus der Adresse maskieren   Mask bits of constant
  lsls r2, #4           @ passend schieben            shift them accordingly
  orrs tos, r2          @ zum Opcode hinzufügen       and OR them to opcode.

  movs r1, #0x00000800  @ Bit 12
  ands r2, r0, r1       @ aus der Adresse maskieren   ...
  lsls r2, #15          @ passend schieben
  orrs tos, r2          @ zum Opcode hinzufügen

  movs r1, #0x00000700  @ Bit 11 - 9
  ands r2, r0, r1       @ aus der Adresse maskieren
  lsls r2, #4           @ passend schieben
  orrs tos, r2          @ zum Opcode hinzufügen

  movs r1, #0x000000FF  @ Bit 8 - 1
  ands r2, r0, r1       @ aus der Adresse maskieren
  @ lsrs r2, #0         @ passend schieben
  orrs tos, r2          @ zum Opcode hinzufügen

  @ Füge den gewünschten Register hinzu:  OR desired target register.
  orrs tos, r3
  
  b.n reversekomma @ Insert finished movw Opcode into Dictionary

@ -----------------------------------------------------------------------------
movtkomma: @ Register r0: Konstante                                    Constant
           @ Register r3: Zielregister, fertig geschoben zum Verodern  Destination register, readily shifted to be ORed with opcode.
@ -----------------------------------------------------------------------------
  pushdatos    @ Platz auf dem Datenstack schaffen
  ldr tos, =0xf2c00000 @ Opcode movt r0, #0

  movs r1, #0xF0000000  @ Bit 32 - 29
  ands r2, r0, r1       @ aus der Adresse maskieren
  lsrs r2, #12          @ passend schieben
  orrs tos, r2          @ zum Opcode hinzufügen

  movs r1, #0x08000000  @ Bit 28
  ands r2, r0, r1       @ aus der Adresse maskieren
  lsrs r2, #1           @ passend schieben
  orrs tos, r2          @ zum Opcode hinzufügen

  movs r1, #0x07000000  @ Bit 27 - 25
  ands r2, r0, r1       @ aus der Adresse maskieren
  lsrs r2, #12          @ passend schieben
  orrs tos, r2          @ zum Opcode hinzufügen

  movs r1, #0x00FF0000  @ Bit 24 - 17
  ands r2, r0, r1       @ aus der Adresse maskieren
  lsrs r2, #16          @ passend schieben
  orrs tos, r2          @ zum Opcode hinzufügen

  @ Füge den gewünschten Register hinzu:
  orrs tos, r3

  b.n reversekomma @ Insert finished movt Opcode into Dictionary


@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "registerliteral," @ ( x Register -- )
registerliteralkomma: @ Compile code to put a literal constant into a register.
@ -----------------------------------------------------------------------------
  push {r0, r1, r2, r3, lr}

  popda r3    @ Hole die Registermaske               Fetch register to generate constant for
  lsls r3, #8 @ Den Register um 8 Stellen schieben   Shift register accordingly for opcode generation
  popda r0    @ Hole die Konstante                   Fetch constant


  @ Generiere movs-Opcode für sehr kleine Konstanten :-)
  @ Generate short movs Opcode for small constants within 0 and 255

  cmp r0, #0xFF @ Does literal fit in 8 bits ?
  bhi 1f        @ Gewünschte Konstante passt in 8 Bits. 

    @ Generate opcode for movs target, #...
    pushdatos
    movs tos, #0x2000 @ MOVS-Opcode
    orrs tos, r3      @ OR with register
    orrs tos, r0      @ OR with constant
    bl hkomma
    pop {r0, r1, r2, r3, pc}
1:


  @ Ist die gewünschte Konstante eine kleine negative Zahl ?
  @ Is desired constant a small negative number ?

  mvns r1, r0
  cmp r1, #0xFF @ Does literal fit in 8 bits ?
  bhi.n movwmovt_internal @ Gewünschte Konstante passt in 8 Bits. 

    @ Generate opcode for movs target, #... with inverted value
    pushdatos
    movs tos, #0x2000 @ MOVS-Opcode
    orrs tos, r3      @ OR with register
    orrs tos, r1      @ OR with constant
    bl hkomma

    @ Generate opcode for mvns target, target
    pushdatos
    movw tos, #0x43C0 @ Opcode for mvns
    lsrs r3, #8 @ Den Register um 8 Stellen zurückschieben
    orrs tos, r3 @ OR with register
    lsls r3, #3 
    orrs tos, r3 @ OR with register
    bl hkomma

    pop {r0, r1, r2, r3, pc}


@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "movwmovt," @ ( x Register -- )
  @ Compile code to put a literal constant into any register.
@ -----------------------------------------------------------------------------
  push {r0, r1, r2, r3, lr}
  
  popda r3    @ Hole die Registermaske               Fetch register to generate constant for
  lsls r3, #8 @ Den Register um 8 Stellen schieben   Shift register accordingly for opcode generation
  popda r0    @ Hole die Konstante                   Fetch constant

movwmovt_internal:
  @ Long constant that cannot be encoded in a small and simple way.
  @ Generate movw and movt pairs.

  bl movwkomma

  @ ldr r1, =0xffff0000 @ High-Teil
  @ ands r0, r1 
  @ cmp r0, #0 

  movw r1, #0xFFFF          @ Wenn der High-Teil Null ist, brauche ich keinen movt-Opcode mehr zu generieren.
  ands r0, r0, r1, lsl #16  @ If High-Part is zero there is no need to generate a movt opcode.
  beq 3f

    bl movtkomma @ Bei Bedarf einfügen

3:pop {r0, r1, r2, r3, pc}


  @ Agenda:

  @ Is constant within the possibilities of the long movs/mvns Opcode ?
  @ Can this constant be generated by rotating an 8 bit value ?

  @ 8 Bit constant: 0000 00XY is encoded as | 0000 | XY |
  @                 00XY 00XY is encoded as | 0001 | XY |
  @                 XY00 XY00 is encoded as | 0010 | XY |
  @                 XYXY XYXY is encoded as | 0011 | XY |
  

@ The assembler encodes the constant in an instruction into imm12, as described below. imm12 is mapped
@ into the instruction encoding in hw1[10] and hw2[14:12,7:0], in the same order.

@ Shifted 8-bit values
@ If the constant lies in the range 0-255, then imm12 is the unmodified constant.
@ Otherwise, the 32-bit constant is rotated left until the most significant bit is bit[7]. The size of the left
@ rotation is encoded in bits[11:7], overwriting bit[7]. imm12 is bits[11:0] of the result.

@ For example, the constant 0x01100000 has its most significant bit at bit position 24. To rotate this bit to
@ bit[7], a left rotation by 15 bits is required. The result of the rotation is 0b10001000. The 12-bit encoding of
@ the constant consists of the 5-bit encoding of the rotation amount 15 followed by the bottom 7 bits of this
@ result, and so is 0b011110001000.

@ Constants of the form 0x00XY00XY
@ Bits[11:8] of imm12 are set to 0b0001, and bits[7:0] are set to 0xXY.
@ This form is UNPREDICTABLE if bits[7:0] == 0x00.

@ Constants of the form 0xXY00XY00
@ Bits[11:8] of imm12 are set to 0b0010, and bits[7:0] are set to 0xXY.
@ This form is UNPREDICTABLE if bits[7:0] == 0x00.

@ Constants of the form 0xXYXYXYXY
@ Bits[11:8] of imm12 are set to 0b0011, and bits[7:0] are set to 0xXY.
@ This form is UNPREDICTABLE if bits[7:0] == 0x00.


@ -----------------------------------------------------------------------------
callkommalang: @ ( Zieladresse -- ) Schreibt einen LANGEN Call-Befehl für does>
               @ Es ist wichtig, dass er immer die gleiche Länge hat.
               @ Writes a long call instruction with known fixed length. 
               @ This is needed for does> as you cannot predict the call target address and 
               @ the shortest instruction length possible needed for it.
@ -----------------------------------------------------------------------------
  @ Dies ist ein bisschen schwierig und muss nochmal gründlich optimiert werden.
  @ Schreibe einen ganz langen Sprung ins Dictionary !
  @ Wichtig für <builds does> wo die Lückengröße vorher festliegen muss.

  push {r0, r1, r2, r3, lr}
  adds tos, #1 @ Ungerade Adresse für Thumb-Befehlssatz   Uneven target address for Thumb instruction set !

  popda r0     @ Zieladresse holen    Destination address
  movs r3, #0  @ Register r0 wählen   Choose register r0
  bl movwkomma
  bl movtkomma

  b.n callkommakurz_intern

@ -----------------------------------------------------------------------------
callkommakurz: @ ( Zieladresse -- )
               @ Schreibt einen Call-Befehl je nach Bedarf.
               @ Wird benötigt, wenn die Distanz für einen BL-Opcode zu groß ist.
               @ Writes a movw-call or a movw-movt-call if destination address is too far away.
@ ----------------------------------------------------------------------------
  @ Dies ist ein bisschen schwierig und muss nochmal gründlich optimiert werden.
  @ Gedanke: Für kurze Call-Distanzen die BL-Opcodes benutzen.

  push {r0, r1, r2, r3, lr}
  adds tos, #1 @ Ungerade Adresse für Thumb-Befehlssatz

  pushdaconst 0 @ Register r0
  bl registerliteralkomma

callkommakurz_intern:
  pushdaconstw 0x4780 @ blx r0
  bl hkomma
  pop {r0, r1, r2, r3, pc}  


@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "call," @ ( Zieladresse -- )
callkomma:  @ Versucht einen möglichst kurzen Aufruf einzukompilieren. 
            @ Write a call to destination with the shortest possible opcodes.
            @ Je nachdem: bl ...                            (4 Bytes)
            @             movw r0, ...              blx r0  (6 Bytes)
            @             movw r0, ... movt r0, ... blx r0 (10 Bytes)
@ ----------------------------------------------------------------------------

  push {r0, r1, r2, r3, lr}
  movs r3, tos @ Behalte Sprungziel auf dem Stack  Keep destination on stack
  @ ( Zieladresse )

  bl here
  popda r0 @ Adresse-der-Opcodelücke  Where the opcodes shall be inserted...
  
  subs r3, r0     @ Differenz aus Lücken-Adresse und Sprungziel bilden   Calculate relative jump offset
  subs r3, #4     @ Da der aktuelle Befehl noch läuft und es komischerweise andere Offsets beim ARM gibt.  Current instruction still running...

  @ 22 Bits für die Sprungweite mit Vorzeichen - 
  @ also habe ich 21 freie Bits, das oberste muss mit dem restlichen Vorzeichen übereinstimmen. 

  @ BL opcodes support 22 Bits jump range - one of that for sign.
  @ Check if BL range is enough to reach target:

  ldr r1, =0xFFC00001   @ 21 Bits frei
  ands r1, r3
  cmp r1, #0  @ Wenn dies Null ergibt, positive Distanz ok.
  beq 1f

  ldr r2, =0xFFC00000
  cmp r1, r2
  beq 1f      @ Wenn es gleich ist: Negative Distanz ok.
    pop {r0, r1, r2, r3, lr}
    b.n callkommakurz @ Too far away - BL cannot reach that destination. Time for long distance opcodes :-)
1:

  @ Within reach of BL. Generate the opcode !

  @ ( Zieladresse )
  drop
  @ ( -- )
  @ BL: S | imm10 || imm11
  @ Also 22 Bits, wovon das oberste das Vorzeichen sein soll.

  @ r3 enthält die Distanz:

  lsrs r3, #1            @ Bottom bit ignored
    ldr r0, =0xF000F800  @ Opcode-Template

    movw r1, #0x7FF       @ Bottom 11 bits of immediate
    ands r1, r3
    orrs r0, r1

  lsrs r3, #11

    movw r1, #0x3FF       @ 10 more bits shifted to second half
    ands r1, r3
    lsls r1, #16
    orrs r0, r1

  lsrs r3, #10         

    ands r1, r3, #1      @ Next bit, treated as sign, shifted into bit 26.
    lsls r1, #26
    orrs r0, r1

  @ Opcode fertig in r0
  pushda r0
  bl reversekomma  @ Write finished opcode into Dictionary.

  pop {r0, r1, r2, r3, pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "literal," @ ( x -- )
literalkomma: @ Nur r3 muss erhalten bleiben  Save r3 !
@ -----------------------------------------------------------------------------
  push {r3, lr}

  pushdaconstw 0xf847  @ str tos, [psp, #-4]!
  bl hkomma
  pushdaconstw 0x6d04
  bl hkomma

  pushdaconst 6 @ Gleich in r6=tos legen
  bl registerliteralkomma

  pop {r3, pc}


/* Some tests:
schuhu: push {lr} 
        writeln "Es sitzt der Uhu auf einem Baum und macht Schuhuuuu, Schuhuuuu !"
        pop {pc}

: c <builds $12345678 , does> . ." does>-Teil " ;  c uhu ' uhu dump
: con <builds h, does> h@ ;  42 con antwort    antwort .
*/
  

@------------------------------------------------------------------------------
  Wortbirne Flag_inline, "does>"
does: @ Gives freshly defined word a special action.
      @ Has to be used together with <builds !
@------------------------------------------------------------------------------
    @ At the place where does> is used, a jump to dodoes is inserted and
    @ after that a pushda lr to put the address of the definition entering the does>-part
    @ on datastack. This is a very special implementation !

  @ Universeller Sprung zu dodoes:  Universal jump to dodoes. There has already been a push {lr} before in the definition that calls does>.
  @ Davor ist in dem Wort, wo does> eingefügt wird schon ein push {lr} gewesen.
  movw r0, #:lower16:dodoes+1
  @  movt r0, #:upper16:dodoes+1   Dieser Teil ist Null, da dodoes weit am Anfang des Flashs sitzt.  Not needed as dodoes in core is in the lowest 64 kb.
  blx r0 @ Den Aufruf mit absoluter Adresse einkompilieren. Perform this call with absolute addressing.


    @ Die Adresse ist hier nicht auf dem Stack, sondern in LR. LR ist sowas wie "TOS" des Returnstacks.
    @ Address is in LR which is something like "TOS in register" of return stack.

  pushdatos
  subs tos, lr, #1 @ Denn es ist normalerweise eine ungerade Adresse wegen des Thumb-Befehlssatzes.  Align address. It is uneven because of Thumb-instructionset bit set.

  @ Am Ende des Wortes wird ein pop {pc} stehen, und das kommt prima hin.
  @ At the end of the definition there will be a pop {pc}, that is fine.
  bx lr @ Very important as delimiter as does> itself is inline.

dodoes:
  @ Hier komme ich an. Die Adresse des Teils, der als Zieladresse für den Call-Befehl genutzt werden soll, befindet sich in LR.

  @ The call to dodoes never returns.
  @ Instead, it compiles a call to the part after its invocation into the dictionary
  @ and exits through two call layers.  

  @ Momentaner Zustand von Stacks und LR:  Current stack:
  @    ( -- )
  @ R: ( Rücksprung-des-Wortes-das-does>-enthält )    R:  ( Return-address-of-the-definition-that-contains-does> )
  @ LR Adresse von dem, was auf den does>-Teil folgt  LR: Address of the code following does>

  @ Muss einen Call-Befehl an die Stelle, die in LR steht einbauen.
  @ Generate a long call to the destination in LR that is inserted into the hole alloted by <builds.

  @ Präpariere die Einsprungadresse, die via callkomma eingefügt werden muss.
  @ Prepare the destination address

  pushdatos
  subs tos, lr, #1
               @ Brauche den Link danach nicht mehr, weil ich über die in dem Wort das does> enthält gesicherte Adresse rückspringe
               @ We don't need this Link later because we return with the address saved by the definition that contains does>.
               @ Einen abziehen. Diese Adresse ist schon ungerade für Thumb-2, aber callkomma fügt nochmal eine 1 dazu. 
               @ Subtract one. Adress is already uneven for Thumb-instructionset, but callkomma will add one anyway.

  bl fadenende_einsprungadresse @ Get the address the long call has to be inserted.

    @ Dictionary-Pointer verbiegen:
      @ Dictionarypointer sichern
      ldr r2, =Dictionarypointer
      ldr r3, [r2] @ Alten Dictionarypointer auf jeden Fall bewahren  Save old Dictionarypointer.

  popda r1     @ r1 enthält jetzt die Codestartadresse der aktuellen Definition.  
  adds r1, #2  @ Am Anfang sollte das neudefinierte Wort ein push {lr} enthalten, richtig ?
               @ Skip the push {lr} opcode in that definition.

  @ Change the Dictionarypointer to insert the long call with the normal comma mechanism.
      str r1, [r2] @ Dictionarypointer umbiegen
  bl callkommalang @ Aufruf einfügen
      str r3, [r2] @ Dictionarypointer wieder zurücksetzen.

  bl smudge
  pop {pc}


@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "<builds"
        @ Beginnt ein Defining-Wort.  Start a defining definition.
        @ Dazu lege ich ein neues Wort an, lasse eine Lücke für den Call-Befehl. Create a new definition and leave space for inserting the does>-Call later.
        @ Keine Strukturkennung  No structure pattern matching here !
@ -----------------------------------------------------------------------------
  push {lr}
  bl create       @ Neues Wort wird erzeugt

  pushdaconstw 0xb500 @ Opcode für push {lr} schreiben  Write opcode for push {lr}
  bl hkomma

  pushdaconst 10  @ Hier kommt ein Call-Befehl hinein, aber ich weiß die Adresse noch nicht.
  bl allot        @ Lasse also eine passende Lücke frei !  Leave space for a long call opcode sequence.
  pop {pc}
