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

@ Routinen, um mit Strings umzugehen, die aus einem Längenbyte und den folgenden Zeichen bestehen.
@ Routines for counted strings.


  .ifdef m0core

.macro lowercase Register @ Ein Zeichen in einem Register wird auf Lowercase umgestellt. Convert to lowercase.
  @    Hex Dec  Hex Dec
  @ A  41  65   61  97  a
  @ Z  5A  90   7A  122 z
  cmp \Register, #0x41
  blo 9f
  cmp \Register, #0x5B
  bhs 9f
  adds \Register, #0x20
9:  
.endm

  .else

.macro lowercase Register @ Ein Zeichen in einem Register wird auf Lowercase umgestellt.
  @    Hex Dec  Hex Dec
  @ A  41  65   61  97  a
  @ Z  5A  90   7A  122 z
  cmp \Register, #0x41
  blo 9f
  cmp \Register, #0x5B
  it lo
  addlo \Register, #0x20
9:  
.endm

  .endif

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "compare"
compare: @ ( str1 str2 -- f ) Vergleicht zwei Strings  Compare two strings
@ -----------------------------------------------------------------------------
   push {r0, r1, r2, r3, lr}

   popda r0

   ldrb    r1, [tos]         @ Hole das Längenbyte des ersten  Strings   Length of  first string
   ldrb    r2, [r0]          @ Hole das Längenbyte des zweiten Strings   Length of second string

   @ Längenbyte vergleichen
   cmp     r1, r2            @ Sind sie gleich lang ?                    Same length ?
   bne     2f                @ Wenn nicht, sind die Strings ungleich.
   @ Länge ist gleich. Ist sie Null ? Dann ist nichts zum Vergleichen da.
   b 4f @ Beginne mit der Probe, ob noch etwas da ist.

   @ Länge größer als Null, vergleiche also Byte für Byte.
1: adds tos, #1               @ Zeiger weiterrücken  Advance pointer
   adds r0, #1

   ldrb r2, [tos]             @ Hole ein Zeichen aus dem ersten String   Fetch character from  first string
   ldrb r3, [r0]              @ Hole ein Zeichen aus dem zweiten String  Fetch character from secons string

   @ Beide Zeichen in Kleinbuchstaben verwandeln.  Convert booth to lowercase.
   lowercase r2
   lowercase r3

   @ Zeichen vergleichen
   cmp     r2, r3            @ Sind die Zeichen gleich ?                 Compare characters
   bne     2f                @ Wenn nicht, sind die Strings ungleich.

   subs r1, #1               @ Ein Zeichen weniger                       One less left.
4: cmp r1, #0                @ Sind noch Zeichen übrig ? Any characters to compare left ?
   bne 1b

   @ Gleich ! Equal !
   movs tos, #0
   mvns tos, tos
   b 3f

2: @ Ungleich ! Unequal !
   movs tos, #0

3: pop {r0, r1, r2, r3, pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "cr" @ Zeilenumbruch
@ -----------------------------------------------------------------------------
  push {lr}
  writeln ""
  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "bl" @ Leerzeichen-code
@ -----------------------------------------------------------------------------
  pushdaconst 32
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "space" @ Leerzeichen-code
space:
@ -----------------------------------------------------------------------------
  pushdaconst 32
  b.n emit

@------------------------------------------------------------------------------
  Wortbirne Flag_immediate+Flag_foldable_0, "[char]" @ ( -- )  Get character from input stream and compile it as literal
  @ Holt ein Zeichen aus dem Eingabestrom und fügt es als Literal ein.
@------------------------------------------------------------------------------
  b.n holechar

@------------------------------------------------------------------------------
  Wortbirne Flag_visible, "char" @ Holt ein Zeichen aus dem Eingabestrom  Get character from input stream
holechar: @ ( -- Zeichen )
@------------------------------------------------------------------------------
  push {lr}
  bl token    @ Gibt Stringadresse zurück  Fetch token
  adds tos, #1 @ Pointer um eine Stelle auf das erste Zeichen im String weiterschieben. Advance pointer by one to skip string length
  ldrb tos, [tos] @ Zeichen an der Stelle holen. Read character
  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate|Flag_foldable_0, "(" @ Der Kommentar
@ -----------------------------------------------------------------------------
  pushdaconst 41 @ Die Klammer )
  b 1f

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate|Flag_foldable_0, "\\" @ Der lange Kommentar
@ -----------------------------------------------------------------------------
  pushdaconst 0 @ Gibt es nicht - immer bis zum Zeilenende. Zero never occours - always catches whole line.

1:push {lr}
  bl parse
  drop
  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, ".\"" @ Fügt eine Meldung ein  Print a message
@ -----------------------------------------------------------------------------
  ldr r0, =dotgaensefuesschen

1:push {lr}
  pushda r0
  bl callkomma

  pushdaconst 34 @ Das Gänsefüßchen "
  bl parse
  bl stringkomma
  pop {pc}

@ -----------------------------------------------------------------------------
  Wortbirne Flag_immediate, "s\"" @ Fügt einen String ein  Insert a string-literal
@ -----------------------------------------------------------------------------
  ldr r0, =dotfuesschen
  b 1b

@ -----------------------------------------------------------------------------
dotgaensefuesschen: @ Gibt den inline folgenden String aus und überspringt ihn
                    @ Print the string following inline and skip it
@ -----------------------------------------------------------------------------
  push {r1, r2, r3}

  mov r2, lr  @ In lr ist nun die Stringadresse.  LR contains string address
  subs r2, #1 @ Die Stringanfangsadresse für type - PC ist ungerade im Thumb-Modus ! One less because of Thumb-instructionset marker bit
  pushda r2   @ Put prepared string address to data stack.

        @ Skip the string.

        .ifdef m0core
        ldrb r3, [r2]   @ Länge des Strings holen  Fetch length of string
        movs r1, #1
        adds r3, #1     @ Plus 1 Byte für die Länge  One more for length byte
        ands r1, r3     @ Wenn es ungerade ist, noch einen mehr:  Maybe one more for aligning
        adds r3, r1
        mov r1, lr
        adds r1, r3
        mov lr, r1
        .else
        ldrb r3, [r2]   @ Länge des Strings holen  Fetch length of string
        adds r3, #1     @ Plus 1 Byte für die Länge  One more for length byte
        ands r1, r3, #1 @ Wenn es ungerade ist, noch einen mehr:  Maybe one more for aligning
        adds r3, r1
        adds lr, r3
        .endif

  pop {r1, r2, r3}
  b.n type   @ Print it !

@ -----------------------------------------------------------------------------
dotfuesschen: @ Legt den inline folgenden String auf den Stack und überspringt ihn
              @ Put address of the string following inline on datastack and skip string.
@ -----------------------------------------------------------------------------
  push {r1, r2, r3}

  mov r2, lr
  subs r2, #1
  pushda r2

        .ifdef m0core
        ldrb r3, [r2]   @ Länge des Strings holen  Fetch length of string
        movs r1, #1
        adds r3, #1     @ Plus 1 Byte für die Länge  One more for length byte
        ands r1, r3     @ Wenn es ungerade ist, noch einen mehr:  Maybe one more for aligning
        adds r3, r1
        mov r1, lr
        adds r1, r3
        mov lr, r1
        .else
        ldrb r3, [r2]   @ Länge des Strings holen  Fetch length of string
        adds r3, #1     @ Plus 1 Byte für die Länge  One more for length byte
        ands r1, r3, #1 @ Wenn es ungerade ist, noch einen mehr:  Maybe one more for aligning
        adds r3, r1
        adds lr, r3
        .endif

  pop {r1, r2, r3}
  bx lr  @ Leave string address on datastack.

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "type"
type: @ ( str -- ) Gibt einen String aus  Print a string
@ -----------------------------------------------------------------------------
   push {r0, r1, lr}
   popda r1      @ r1 enthält die Stringadresse.      String address
   ldrb r0, [r1] @ Hole die auszugebende Länge in r0  Fetch length to type
   
   cmp r0, #0  @ Wenn nichts da ist, bin ich fertig.  Any characters left ?
   beq 2f

   @ Es ist etwas zum Tippen da !  Something available for typing !
1: adds r1, #1   @ Adresse um eins erhöhen  Advance pointer
   pushdatos     @ Platz auf dem Stack schaffen  Prepare space on datastack
   ldrb tos, [r1] @ Zeichen holen                    Put character on datastack
   bl emit        @ Zeichen senden                   Emit character

   subs    r0, #1 @ Ein Zeichen weniger  One character less
   bne     1b

2: pop {r0, r1, pc}
