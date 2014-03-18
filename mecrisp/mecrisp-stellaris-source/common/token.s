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

@ Token und Parse zum Zerlegen des Eingabepuffers
@ Token and parse to cut contents of input buffer apart

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "token" @ ( -- Addr )
token:
@ -----------------------------------------------------------------------------
  movs r0, #32 @ Leerzeichen  Space
  pushda r0
  b.n parse

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "parse" @ ( c -- Addr )
parse:
@ -----------------------------------------------------------------------------
  push {r4, r5, lr}
  @ Mal ein ganz anderer Ansatz:
  @ Der Eingabepuffer bleibt die ganze Zeit unverändert.
  @ Pufferstand gibt einfach einen Offset in den Eingabepuffer, der zeigt, wie viele Zeichen schon verbraucht worden sind.
  @ Zu Beginn ist der Pufferstand 0.
  @ Sind alle Zeichen verbraucht, ist der Pufferstand gleich der Länge des Eingabepuffers.

  @ Kopiere die Zeichen in den Tokenpuffer.

  @ The idea is to copy characters from input buffer into token buffer.
  @ Pufferstand is a variable indicating how many characters of the input buffer are already consumed. It is 0 after query.
  @ Are all characters collected, Pufferstand equals length of input.

  ldr r0, =Eingabepuffer @ Pointer auf den Eingabepuffer         Pointer to input buffer
  ldrb r1, [r0]          @ Länge des Eingabepuffers              Length of input buffer

  ldr r2, =Pufferstand
  ldrb r2, [r2]          @ Aktuellen Pufferstand                 Current input buffer gauge

  ldr r3, =Tokenpuffer   @ Pointer für den Sammelpuffer          Pointer to collection buffer
  movs r4, #0            @ Zahl der aktuell gesammelten Zeichen  Number of already collected characters

  @ TOS                  @ Gesuchtes Trennzeichen  Delimiter searched for

  @ Beginne beim Pufferstand:
  adds r0, r2 @ Aktuellen Pufferstand zum Pointer hinzuaddieren  Skip already consumed characters

  @ Speziell for Token, falls das Trennzeichen das Leerzeichen ist:
  cmp tos, #32
  bne 2f

    @ Führende Leerzeichen abtrennen.  Skip leading delimiters if delimiter is space. This special behaviour is needed for token.
4:  cmp r1, r2 @ Ist noch etwas da ?  Something left ?
    beq 3f

    @ Hole ein Zeichen.  Fetch one character.
    adds r0, #1 @ Eingabepufferzeiger um ein Zeichen weiterrücken.  Advance pointers.
    adds r2, #1 @ Pufferstand um ein Zeichen weiterschieben

    @ Hole an der Stelle ein Zeichen und entscheide, was damit zu tun ist.
    ldrb r5, [r0]
    cmp r5, tos @ Ist es das Leerzeichen ?
    beq 4b @ Führende Leerzeichen nicht Sammeln.             Skip spaces.
    b 5f   @ Ist es etwas anderes, dann beginne zu Sammeln.  Start collecting if this is not space.


2: @ Sammelschleife. Collecting loop.

  @ Erster Schritt: Ist noch etwas zum Sammeln da ?  Something left ?
  cmp r1, r2
  beq 3f

  @ Zweiter Schritt: Hole ein Zeichen.  Fetch a character.
  adds r0, #1 @ Eingabepufferzeiger um ein Zeichen weiterrücken. Advance pointers.
  adds r2, #1 @ Pufferstand um ein Zeichen weiterschieben
  @ Hole an der Stelle ein Zeichen und entscheide, was damit zu tun ist.
  ldrb r5, [r0]
  cmp r5, tos    @ Wenn das Trennzeichen erreicht ist, höre auf.  Stop if this is a delimiter.
  beq 3f

5: @ Wenn es mir gefällt, nimm es in den Tokenpuffer auf. I like the character. Collect it !
  adds r3, #1 @ Pointer weiterschieben  Advance pointers.
  adds r4, #1 @ Zahl der gesammelten Zeichen weiterschieben
  strb r5, [r3] @ Write collected character into buffer.
  b 2b @ Sammelschleife


3: @ Fertig, entweder nichts mehr da, oder Trennzeichen gefunden.  Finished. Either input is exhausted or delimiter is found.
  ldr r0, =Pufferstand
  strb r2, [r0]         @ Aktuellen Pufferstand vermerken.  Save current input buffer gauge.

  ldr tos, =Tokenpuffer @ Tokenpufferadresse zurückgeben.   Give back buffer address
  strb r4, [tos]        @ Zahl der gesammelten Zeichen vermerken  Save number of collected characters as length byte in buffer.

  pop {r4, r5, pc}
 