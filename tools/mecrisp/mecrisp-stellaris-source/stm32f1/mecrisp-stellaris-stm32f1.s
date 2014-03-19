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

.syntax unified
.cpu cortex-m3
.thumb

@ Not available: .equ charkommaavailable, 1

@ -----------------------------------------------------------------------------
@ Meldungen, hier definiert, damit das Zeilenende leicht geändert werden kann
@ Messages are defined here for simple exchange of line endings.
@ -----------------------------------------------------------------------------

.macro write Meldung
  bl dotgaensefuesschen	
	.byte 9f - 8f         @ Compute length of name field.
8:	.ascii "\Meldung"
9:	.p2align 1
.endm

.macro writeln Meldung
  bl dotgaensefuesschen 
        .byte 9f - 8f         @ Compute length of name field.
8:      .ascii "\Meldung\n"
9:      .p2align 1
.endm

.macro Fehler_Quit Meldung
  bl dotgaensefuesschen 
        .byte 9f - 8f         @ Compute length of name field.
8:      .ascii "\Meldung\n"
9:      .p2align 1
  b quit
.endm

.macro Fehler_Quit_n Meldung
  bl dotgaensefuesschen 
        .byte 9f - 8f         @ Compute length of name field.
8:      .ascii "\Meldung\n"
9:      .p2align 1
  b.n quit
.endm

@ -----------------------------------------------------------------------------
@ Vorbereitung der Dictionarystruktur
@ Preparations for dictionary structure
@ -----------------------------------------------------------------------------
  .include "../common/datastackandmacros.s"

  .set CoreVariablenPointer, RamDictionaryEnde @ Im Flash definierte Variablen kommen ans RAM-Ende
  .set Latest, FlashDictionaryAnfang @ Zeiger auf das letzte definierte Wort
  .set Neu,    0xFFFFFFFF            @ Variable für aktuellen Zeiger, am Anfang ungesetzt

@ -----------------------------------------------------------------------------
@ Anfang im Flash - Interruptvektortabelle ganz zu Beginn
@ Flash start - Vector table has to be placed here
@ -----------------------------------------------------------------------------
.text    @ Hier beginnt das Vergnügen mit der Stackadresse und der Einsprungadresse
.include "vectors.s" @ You have to change vectors for Porting !

@ -----------------------------------------------------------------------------
@ Alle anderen Teile von Mecrisp-Stellaris
@ All other parts of Mecrisp-Stellaris core
@ -----------------------------------------------------------------------------
  .include "../common/double.s"
  .include "../common/stackjugglers.s" 
  .include "../common/logic.s"
  .include "../common/comparisions.s"
  .include "../common/memory.s"
  .include "flash.s"
  .include "../common/calculations.s"
  .ltorg @ Mal wieder Konstanten schreiben
  .include "terminal.s"
  .include "../common/query.s"
  .include "../common/strings.s"
  .include "../common/deepinsight.s"
  .ltorg @ Mal wieder Konstanten schreiben
  .include "../common/compiler.s"
  .ltorg @ Mal wieder Konstanten schreiben
  .include "../common/compiler-flash.s"
  .ltorg @ Mal wieder Konstanten schreiben
  .include "../common/controlstructures.s"
  .ltorg @ Mal wieder Konstanten schreiben
  .include "../common/doloop.s"
  .include "../common/case.s"
  .include "../common/token.s"
  .ltorg @ Mal wieder Konstanten schreiben
  .include "../common/numberstrings.s"
  .include "../common/interpreter.s"
  .include "interrupts.s" @ You have to change interrupt handlers for Porting !

.equ CoreDictionaryAnfang, Latest @ Dictionary-Einsprungpunkt setzen
                                  @ Set entry point for Dictionary

@ -----------------------------------------------------------------------------
Reset: @ Einsprung zu Beginn
@ -----------------------------------------------------------------------------
   @ Initialisierungen der Hardware, habe und brauche noch keinen Datenstack dafür
   @ Initialisations for Terminal hardware, without Datastack.
   bl uart_init

Reset_Inneneinsprung:
   @ Return stack pointer already set up. Time to set data stack pointer !
   @ Normaler Stackpointer bereits gesetzt. Setze den Datenstackpointer:
   ldr psp, =datenstackanfang

   @ TOS setzen, um Pufferunterläufe gut erkennen zu können
   @ TOS magic number to see spurious stack underflows in .s
   @ ldr tos, =0xAFFEBEEF
   movs tos, #42

   @ Dictionarypointer ins RAM setzen
   @ Set dictionary pointer into RAM first
   ldr r0, =Dictionarypointer
   ldr r1, =RamDictionaryAnfang
   str r1, [r0]

   @ Fadenende fürs RAM vorbereiten
   @ Set latest for RAM
   ldr r0, =Fadenende
   ldr r1, =CoreDictionaryAnfang
   str r1, [r0]

   @ Vorbereitungen für die Flash-Pointer
   @ Catch the pointers for Flash dictionary
   .include "../common/catchflashpointers.s"

   writeln "Mecrisp-Stellaris 1.0 for STM32F100 by Matthias Koch"

   @ Genauso wie in quit. Hier nochmal, damit quit nicht nach dem Init-Einsprung nochmal tätig wird.
   @ Exactly like the initialisations in quit. Here again because quit should not be executed after running "init".
   ldr r0, =base
   movs r1, #10
   str r1, [r0]

   ldr r0, =state
   movs r1, #0
   str r1, [r0]

   ldr r0, =konstantenfaltungszeiger
   movs r1, #0
   str r1, [r0]

   @ Suche nach der init-Definition:
   @ Search for current init definition in dictionary:
   ldr r0, =init_name
   pushda r0
   bl find
   drop @ Flags brauche ich nicht No need for flags
   cmp tos, #0
   beq 1f
     @ Gefunden ! Found !
     bl execute
     b.n quit_innenschleife
1:
   drop @ Die 0-Adresse von find. Wird hier heruntergeworfen, damit der Startwert AFFEBEEF erhalten bleibt !
   b.n quit @ Drop 0-address of find to keep magic TOS value intact.

init_name: .byte 4, 105, 110, 105, 116, 0 @ "init"

.ltorg @ Ein letztes Mal Konstanten schreiben

@ -----------------------------------------------------------------------------
@ Speicherkarte für Flash und RAM
@ Memory map for Flash and RAM
@ -----------------------------------------------------------------------------

@ Konstanten für die Größe des Ram-Speichers

.equ RamAnfang, 0x20000000 @ Start of RAM          Porting: Change this !
.equ RamEnde,   0x20002000 @ End   of RAM.   8 kb. Porting: Change this !

@ Konstanten für die Größe und Aufteilung des Flash-Speichers

.equ Kernschutzadresse,     0x00004000 @ Darunter wird niemals etwas geschrieben ! Mecrisp core never writes flash below this address.
.equ FlashDictionaryAnfang, 0x00004000 @ 16 kb für den Kern reserviert...          16 kb Flash reserved for core.
.equ FlashDictionaryEnde,   0x00020000 @ 128 kb Platz für das Flash-Dictionary       1 MB Flash available. Porting: Change this !
.equ Backlinkgrenze,        RamAnfang  @ Ab dem Ram-Start.

@ Speicherstellen beginnen am Anfang des Rams
.set rampointer, RamAnfang          @ Ram-Anfang setzen  Set location for core variables.

@ Variablen des Kerns  Variables of core

ramallot Pufferstand, 4
ramallot Dictionarypointer, 4
ramallot Fadenende, 4
ramallot state, 4
ramallot base, 4
ramallot konstantenfaltungszeiger, 4
ramallot leavepointer, 4
ramallot Datenstacksicherung, 4

@ Variablen für das Flashdictionary  Variables for Flash management

ramallot ZweitDictionaryPointer, 4
ramallot ZweitFadenende, 4
ramallot FlashFlags, 4
ramallot VariablenPointer, 4

@ Jetzt kommen Puffer und Stacks:  Buffers and Stacks

@ Idee für die Speicherbelegung: 12*4 + 64 + 200 + 256 + 256 + 200 = 1024 Bytes

.equ Zahlenpufferlaenge, 63 @ Zahlenpufferlänge+1 sollte durch 4 teilbar sein !      Number buffer (Length+1 mod 4 = 0)
ramallot Zahlenpuffer, Zahlenpufferlaenge+1 @ Reserviere mal großzügig 64 Bytes RAM für den Zahlenpuffer

.equ maximaleeingabe,    199 @ Eingabepufferlänge+1 sollte durch 4 teilbar sein !    Input buffer  (Length+1 mod 4 = 0)
ramallot Eingabepuffer, maximaleeingabe+1 @ Länge des Pufferinhaltes + 1 Längenbyte !

ramallot datenstackende, 256  @ Data stack
ramallot datenstackanfang, 0

ramallot returnstackende, 256  @ Return stack
ramallot returnstackanfang, 0

ramallot Tokenpuffer, maximaleeingabe+1  @ Token buffer, same length as Input buffer

.equ RamDictionaryAnfang, rampointer @ Ende der Puffer und Variablen ist Anfang des Ram-Dictionary.  Start of RAM dictionary
.equ RamDictionaryEnde,   RamEnde    @ Das Ende vom Dictionary ist auch das Ende vom gesamten Ram.   End of RAM dictionary = End of RAM
