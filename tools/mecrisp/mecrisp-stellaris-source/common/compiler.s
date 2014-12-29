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

@ Die Routinen, die nötig sind, um neue Definitionen zu kompilieren.
@ The compiler - parts that are the same for Flash and for Ram.

  .ifdef m0core
  .include "../common/codegenerator-m0.s"
  .else
  .include "../common/codegenerator-m3.s"
  .endif

@------------------------------------------------------------------------------
  Wortbirne Flag_immediate|Flag_foldable_0, "[']" @ Sucht das nächste Wort im Eingabestrom  Searches the next token in input buffer and compiles its entry point as literal.
@------------------------------------------------------------------------------
  b.n tick @ So sah das mal aus: ['] ' immediate 0-foldable ;

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "'" @ Searches next token in unput buffer and gives back its code entry point.
tick: @ Nimmt das nächste Token aus dem Puffer, suche es und gibt den Einsprungpunkt zurück.
@ -----------------------------------------------------------------------------
  push {lr}
  bl token @ Hole den Namen der neuen Definition.  Fetch Name
  @ ( Tokenadresse )

  @ Überprüfe, ob der Token leer ist.
  @ Das passiert, wenn der Eingabepuffer nach create leer ist.

  popda r0       @ Check if token is empty. Maybe input buffer is exhausted after Tick...
  ldrb r1, [r0]
  cmp r1, #0
  bne 1f

    @ Token ist leer. Brauche Stacks nicht zu putzen.
    Fehler_Quit " ' needs name !"

1:@ Tokenname ist okay.
  @ Prüfe, ob es schon existiert.
  pushda r0
  bl find
  @ ( Einsprungadresse Flags )
  drop @ Benötige die Flags hier nicht. Möchte doch nur schauen, ob es das Wort schon gibt.
  @ ( Einsprungadresse )  No need for Flags here. Just check if it is found and give back its code entry address.
  cmp tos, #0
  bne 2f
nicht_gefunden:
    ldr r0, =Tokenpuffer
    pushda r0
    bl type
    Fehler_Quit " not found."

2:@ Gefunden, alles gut
  pop {pc}

@ : pif postpone if immediate ;  : ja? pif ." Ja" else ." Nein" then ;
@------------------------------------------------------------------------------
  Wortbirne Flag_immediate, "postpone" @ Sucht das nächste Wort im Eingabestrom  Search next token and fill it in Dictionary in a special way.
                                       @ und fügt es auf besondere Weise ein.
@------------------------------------------------------------------------------
  push {lr}

  bl token
  @ ( Pufferadresse )
  bl find
  @ ( Einsprungadresse Flags )
  popda r0 @ Flags holen  Fetch Flags

  @ ( Einsprungadresse )  
  cmp tos, #0  @ Not found ?
  beq.n nicht_gefunden

1:movs r1, #Flag_immediate  @ In case definition is immediate: Compile a call to its address.
  ands r1, r0
  cmp r1, #Flag_immediate
  beq 4f

2:movs r1, #Flag_inline    @ In case definition is inline: Compile entry point as literal and a call to inline, afterwards.
  ands r1, r0
  cmp r1, #Flag_inline
  bne 3f                             @ ( Einsprungadresse )
    bl literalkomma                  @ Einsprungadresse als Konstante einkompilieren
    pushdatos
    ldr tos, =inlinekomma
    b 4f                             @ zum Aufruf bereitlegen
    
3:@ Normal                     @ In case definition is normal: Compile entry point as literal and a call to call, afterwards.
    bl literalkomma
    pushdatos
    ldr tos, =callkomma
4:  bl callkomma
    pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "inline," @ ( addr -- )
inlinekomma:
@ -----------------------------------------------------------------------------
  push {lr}
  @ Übernimmt eine Routine komplett und schreibt sie ins Dictionary.
  @ TOS enthält Adresse der Routine, die eingefügt werden soll.
  @ Copy the whole code of a definition into Dictionary.

  @ Es gibt drei besondere Opcodes:     There are three special opcodes:
  @  - push {lr} wird übersprungen        will be skipped
  @  - pop {pc} ist Ende                  is end of definition
  @  - bx lr ist auch eine Endezeichen.   is end often used in core.


  .ifdef m0core
  ldr r1, =0xb500 @ push {lr}
  ldr r2, =0xbd00 @ pop {pc}
  ldr r3, =0x4770 @ bx lr
  .else
  movw r1, #0xb500 @ push {lr}
  movw r2, #0xbd00 @ pop {pc}
  movw r3, #0x4770 @ bx lr
  .endif

1:ldrh r0, [tos] @ Hole die nächsten 16 Bits aus der Routine.  Fetch next opcode...
  cmp r0, r1 @ push {lr}
  beq 2f
  cmp r0, r2 @ pop {pc}
  beq 3f
  cmp r0, r3 @ bx lr
  beq 3f

  pushda r0
  bl hkomma @ Opcode einkompilieren  After checking is done, insert opcode into Dictionary.

2:adds tos, #2 @ Pointer weiterrücken  Advance pointer
  b 1b 

3:drop
  pop {pc}

@ An der ersten Stelle wird geprüft: Ist es eine Routine mit pop {pc} oder mit bx lr am Ende ?
@ -----------------------------------------------------------------------------
suchedefinitionsende: @ Rückt den Pointer in r0 ans Ende einer Definition vor.
                      @ Advance r0 to the end of code of current definition by searching for pop {pc} or bx lr opcodes.
@ -----------------------------------------------------------------------------
        @ Suche wie in inline, nach pop {pc} oder bx lr.
        push {r1, r2, r3}

         .ifdef m0core
         ldr r2, =0xbd00 @ pop {pc}
         ldr r3, =0x4770 @ bx lr
         .else
         movw r2, #0xbd00 @ pop {pc}
         movw r3, #0x4770 @ bx lr
         .endif


1:        ldrh r1, [r0]  @ Hole die nächsten 16 Bits aus der Routine.
          adds r0, #2    @ Pointer Weiterrücken

          cmp r1, r2  @ pop {pc}
          beq 2f
          cmp r1, r3  @ bx lr
          bne 1b

2:      pop {r1, r2, r3}
        bx lr


@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "ret," @ ( -- )
retkomma: @ Write pop [pc} opcode
@ -----------------------------------------------------------------------------
  @ Mache das mit pop {pc}
  pushdaconstw 0xbd00 @ Opcode für pop {pc} schreiben
  b.n hkomma

@------------------------------------------------------------------------------
  Wortbirne Flag_immediate_compileonly, "exit" @ Kompiliert ein ret mitten in die Definition.
  @ Writes a ret opcode into current definition. Take care with inlining !
@------------------------------------------------------------------------------
  b.n retkomma

@ Some tests:
@  : fac ( n -- n! )   1 swap  1 max  1+ 2 ?do i * loop ;
@  : fac-rec ( acc n -- n! ) dup dup 1 = swap 0 = or if drop else dup 1 - rot rot * swap recurse then ; : facre ( n -- n! ) 1 swap fac-rec ;

@------------------------------------------------------------------------------
  Wortbirne Flag_immediate_compileonly, "recurse" @ Für Rekursion. Führt das gerade frische Wort aus. Execute freshly defined definition.
@------------------------------------------------------------------------------
  push {lr}
  bl fadenende_einsprungadresse
  bl callkomma
  pop {pc}

@------------------------------------------------------------------------------
fadenende_einsprungadresse: @ Kleines Helferlein spart Platz
                            @ Calculate code start address of current latest definition
@------------------------------------------------------------------------------
  push {r0, r1, r2, r3, lr} @ Vermutlich unnötig, wird nur in dodoes und recurse benutzt.
  ldr r0, =Fadenende
  ldr r0, [r0]

  @ --> Codestartadresse, analog zur Routine in words

        @ Flagfeld
        @adds r0, #2

        @ Link
        @adds r0, #4
        adds r0, #6  @ Skip Flags and Link

        ldrb r1, [r0] @ Länge des Strings holen                   Length of Name

  .ifdef emulated16bitflashwrites
  @ If 16-Bit Flash writes are emulated, the name length byte may not be set yet.
  cmp r1, #0xFF
  bne 1f
  @ It is not set. That means that the length byte is in the Flash write buffer.
  @ Scan the buffer for the needed address !
  pushda r0 @ Address to search for...
  push {r0}
  bl sammeltabellensuche
@  writeln "Fadenende-Einsprungadresse suchte im Flashpuffer"
  pop {r0}
  movs r1, #0xff @ Mask for low byte
  ands r1, tos
  drop  
1:
  .endif

        adds r1, #1  @ Plus 1 Byte für die Länge                One more for length byte
        movs r2, #1  @ Wenn es ungerade ist, noch einen mehr:  Maybe one more for aligning
        ands r2, r1
        adds r1, r2
        adds r0, r1

  @ r0 enthält jetzt die Codestartadresse der aktuellen Definition.
  pushda r0
  pop {r0, r1, r2, r3, pc}
 
@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_0, "state" @ ( -- addr )
@ -----------------------------------------------------------------------------
  pushdatos
  ldr tos, =state
  bx lr

@------------------------------------------------------------------------------
  Wortbirne Flag_visible, "]" @ In den Compile-Modus übergehen  Switch to compile mode
@ -----------------------------------------------------------------------------
  ldr r0, =state
  movs r1, #0 @ true-Flag in State legen
  mvns r1, r1 @ -1
  str r1, [r0] 
  bx lr

@------------------------------------------------------------------------------
  Wortbirne Flag_immediate, "[" @ In den Execute-Modus übergehen  Switch to execute mode
@ -----------------------------------------------------------------------------
  ldr r0, =state
  movs r1, #0 @ false-Flag in State legen.
  str r1, [r0]
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, ":" @ ( -- )
@ -----------------------------------------------------------------------------
  push {lr}

  ldr r0, =Datenstacksicherung @ Setzt den Füllstand des Datenstacks zur Probe.
  str psp, [r0]                @ Save current datastack pointer to detect structure mismatch later.

  bl create

  pushdaconstw 0xb500 @ Opcode für push {lr} schreiben  Write opcode for push {lr}
  bl hkomma

  ldr r0, =state
  movs r1, #0 @ true-Flag in State legen
  mvns r1, r1 @ -1
  str r1, [r0]

  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate_compileonly, ";" @ ( -- )
@ -----------------------------------------------------------------------------
  push {lr}

  ldr r0, =Datenstacksicherung @ Prüft den Füllstand des Datenstacks.
  ldr r1, [r0]                 @ Check fill level of datastack.
  cmp r1, psp
  beq 1f
    Fehler_Quit " Stack not balanced."
1: @ Stack balanced, ok

  pushdaconstw 0xbd00 @ Opcode für pop {pc} schreiben  Write opcode for pop {pc}
  bl hkomma

  bl smudge

  ldr r0, =state
  movs r1, #0 @ false-Flag in State legen.
  str r1, [r0]

  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "execute"
execute:
@ -----------------------------------------------------------------------------
  popda r0
  adds r0, #1 @ Ungerade Adresse für Thumb-Befehlssatz
  mov pc, r0  @ Uneven address für Thumb-Instructionset

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "immediate" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_immediate
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "inline" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_inline
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "compileonly" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_immediate_compileonly
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "0-foldable" @ ( -- )
setze_faltbarflag:
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_0
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "1-foldable" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_1
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "2-foldable" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_2
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "3-foldable" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_3
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "4-foldable" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_4
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "5-foldable" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_5
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "6-foldable" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_6
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "7-foldable" @ ( -- )
@ -----------------------------------------------------------------------------
  pushdaconst Flag_foldable_7
  b.n setflags

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "constant" @ ( n -- )
@ -----------------------------------------------------------------------------
  push {lr}
  bl create
1:bl literalkomma
  pushdaconstw 0x4770 @ Opcode for bx lr
  bl hkomma
  bl setze_faltbarflag
  bl smudge
  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "2constant" @ ( n -- )
@ -----------------------------------------------------------------------------
  push {lr}
  bl create
  swap
  bl literalkomma
  b.n 1b
