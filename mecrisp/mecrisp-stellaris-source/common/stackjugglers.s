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

@ Stackjongleure
@ Stack jugglers

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible, "depth" @ ( -- Zahl der Elemente, die vorher auf den Datenstack waren )
                                  @ ( -- Number of elements that have been on datastack before )
@ -----------------------------------------------------------------------------
  @ Berechne den Stackfüllstand
  ldr r1, =datenstackanfang @ Anfang laden  Calculate stack fill gauge
  subs r1, psp @ und aktuellen Stackpointer abziehen
  lsrs r1, #2 @ Durch 4 teilen  Divide through 4 Bytes/element.
  pushda r1
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_1|Flag_inline, "dup" @ ( x -- x x )
@ -----------------------------------------------------------------------------
  dup
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_1|Flag_inline, "drop" @ ( x -- )
@ -----------------------------------------------------------------------------
  drop
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_1|Flag_inline, "?dup" @ ( x -- 0 | x x )
@ -----------------------------------------------------------------------------
  cmp tos, #0
  beq 1f
  pushdatos
1:bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_2|Flag_inline, "swap" @ ( x y -- y x )
@ -----------------------------------------------------------------------------
  ldr x, [psp]   @ Load X from the stack, no SP change.
  str tos, [psp] @ Replace it with TOS.
  movs tos, x     @ And vice versa.
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_2|Flag_inline, "nip" @ ( x y -- x )
@ -----------------------------------------------------------------------------
  nip
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_2|Flag_inline, "over" @ ( x y -- x y x )
@ -----------------------------------------------------------------------------
  pushdatos
  ldr tos, [psp, #4] 
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_2|Flag_inline, "tuck" @ ( x1 x2 -- x2 x1 x2 )
@ -----------------------------------------------------------------------------
tuck:
  ldm psp!, {w}
  subs psp, #8
  str tos, [psp, #4]
  str w, [psp]
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_3|Flag_inline, "rot" @ ( x w y -- w y x )
@ -----------------------------------------------------------------------------
rot:
  ldm psp!, {w, x}
  subs psp, #8
  str w, [psp, #4]
  str tos, [psp]
  movs tos, x
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_foldable_3|Flag_inline, "-rot" @ ( x w y -- y x w )
@ -----------------------------------------------------------------------------
minusrot:
  ldm psp!, {w, x}
  subs psp, #8
  str tos, [psp, #4]
  str x, [psp]
  movs tos, w
  bx lr

@ -----------------------------------------------------------------------------
  Wortbirne Flag_visible|Flag_inline, "pick" @ ( xu .. x1 x0 u -- xu ... x1 x0 xu ) 
@ -----------------------------------------------------------------------------
  .ifdef m0core
  lsls r0, tos, #2
  ldr tos, [psp, r0]
  bx lr
  .else
  ldr tos, [psp, tos, lsl #2]  @ I love ARM. :-)
  bx lr
  .endif

@ Returnstack

@------------------------------------------------------------------------------
  Wortbirne Flag_visible|Flag_inline, ">r" @ Legt das oberste Element des Datenstacks auf den Returnstack.
@------------------------------------------------------------------------------
  push {tos}
  ldm psp!, {tos}
  bx lr

@------------------------------------------------------------------------------
  Wortbirne Flag_visible|Flag_inline, "r>" @ Holt das zwischengespeicherte Element aus dem Returnstack zurück
@------------------------------------------------------------------------------
  pushdatos
  pop {tos}
  bx lr

@------------------------------------------------------------------------------
  Wortbirne Flag_visible|Flag_inline, "r@" @ Kopiert das oberste Element des Returnstacks auf den Datenstack
@------------------------------------------------------------------------------
  pushdatos
  ldr tos, [sp]
  bx lr

@------------------------------------------------------------------------------
  Wortbirne Flag_visible|Flag_inline, "rdrop" @ Entfernt das oberste Element des Returnstacks
@------------------------------------------------------------------------------
  add sp, #4
  bx lr
