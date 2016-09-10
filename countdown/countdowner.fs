#! /usr/bin/env gforth

: usage ( -- )
    ." ./countdowner.fs <terminal> <minutes>"  cr
    ." i.e.: ./countdowner.fs /dev/ttyUSB0 35" cr
    bye ;

\ read terminal name
next-arg 2constant terminal-filename

\ read minutes
0 0 next-arg dup 0= [IF] usage [THEN] >number

\ check both arguments
0<> terminal-filename nip 0= or [IF] usage [THEN]
2drop variable minutes
minutes !

\ open terminal
terminal-filename w/o open-file throw constant terminal
include serial.fs
B115200 terminal set-baud

\ writers
: $>terminal ( c-addr n -- )
    terminal write-file throw
    terminal flush-file throw ;
: >terminal ( xt -- )
    >string-execute $>terminal ;

\ runner
: countdown ( -- )
    begin
        [: ." dup " minutes @ . ." countdown-step" cr ;] >terminal
        minutes @ . cr
        60000 ms
        minutes @ 1- dup minutes ! 0=
    until ;

\ clear any previous input
[: cr cr cr ;] >terminal
100 ms
\ initial progress bar
[: minutes @ . cr ;] >terminal
countdown
[: ." drop countdown-thanks" cr ;] >terminal

\ close terminal
terminal close-file throw
bye
