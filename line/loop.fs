\ Animations
\ (c)copyright 2016 by Gerald Wodni <gerald.wodni@gmail.com> 

\  cold

compiletoflash
0 variable step             \ current program step (two byte values in one word-variable)
step 1+ constant substep     \ substep for intermix

: step. step c@ u.2 space substep c@ u.4 space buffer.. ;

: color-init ( d-rgb-bright d-rgb-dim -- )
    #leds 1- 2/ step c!  0 substep c! \ start at stripcenter -1
    buffer!                \ dim lower half
    #leds #leds 2/ do                 \ full brightness for upper half
        2dup i rgb-px!
    loop 2drop flush ;

: color-step ( -- )
    substep c@ 1+ dup $7F > if
        \ cr ." NEW STEP!" step c@ .
        step c@ 1- dup 0< if drop #leds 1- then step c!
        \ step c@ . cr
        \ drop 0
    then substep c!
    ;

: green-init ( -- )
    $00.FF.00 $00.7F.00 color-init ;

: green-step ( -- )
     1 step c@ led-addr c+! \ increment step led
    -1 step c@ #leds/2 led+addr c+! \ decrement end of bright half
    color-step
    ;

: red-init ( -- )
    $FF.00.00 $7F.00.00 color-init ;

: red-step ( -- )
     1 step c@ led-addr 1+ c+! \ increment step led
    -1 step c@ #leds/2 led+addr 1+ c+! \ decrement end of bright half
    color-step
    ;

: green-loop
    init-spi
    green-init
    begin
        green-step
        flush
        \ step. cr
        \ 10 ms
    key? until ;

: red-loop
    init-spi
    red-init
    begin
        red-step
        flush
        \ step. cr
        \ 10 ms
    key? until ;

green-loop
