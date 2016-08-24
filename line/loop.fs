\ Animations
\ (c)copyright 2016 by Gerald Wodni <gerald.wodni@gmail.com> 

\  cold

compiletoflash
5 variable delay          \ ms between program-steps
0 variable step             \ current program step (two byte values in one word-variable)
step 1+ constant substep     \ substep for intermix

: step. step c@ u.2 space substep c@ u.4 space buffer.. ;

: color-init ( d-rgb-bright d-rgb-dim -- )
    #leds 1- 2/ step c!  0 substep c! \ start at stripcenter -1
    buffer!                \ dim lower half
    #leds #leds 2/ do                 \ full brightness for upper half
        2dup i rgb-px!
    loop 2drop flush ;

: next-substep ( -- )
    substep c@ 1+ dup $7F > if
        \ cr ." NEW STEP!" step c@ .
        step c@ 1- dup 0< if drop #leds 1- then step c!
        \ step c@ . cr
        \ drop 0
    then substep c!  ;

: color-step ( -- )
    step c@ #leds/2 led+addr rgb+! \ decrement end of bright half
    step c@ led-addr rgb+! \ increment step led
    next-substep ;

: red-init ( -- )
    $FF.00.00 $7F.00.00 color-init ;

: red-step ( -- )
    $01.00.00 $FF.00.00 color-step ;

: yellow-init ( -- )
    $FF.FF.00 $7F.7F.00 color-init ;

: yellow-step ( -- )
    $01.01.00 $FF.FF.00 color-step ;

: green-init ( -- )
    $00.FF.00 $00.7F.00 color-init ;

: green-step ( -- )
    $00.01.00 $00.FF.00 color-step ;

: cyan-init ( -- )
    $00.FF.FF $00.7F.7F color-init ;

: cyan-step ( -- )
    $00.01.01 $00.FF.FF color-step ;

: blue-init ( -- )
    $00.00.FF $00.00.7F color-init ;

: blue-step ( -- )
    $00.00.01 $00.00.FF color-step ;

: magenta-init ( -- )
    $FF.00.FF $7F.00.7F color-init ;

: magenta-step ( -- )
    $01.00.01 $FF.00.FF color-step ;

: violet-init ( -- )
    $3F.00.FF $3F.00.7F color-init ;

: violet-step ( -- )
    $00.00.01 $00.00.FF color-step ;

\ program count and xts
7 constant #programs
create programs
    ' red-init , ' red-step ,
    ' yellow-init , ' yellow-step ,
    ' green-init , ' green-step ,
    ' cyan-init , ' cyan-step ,
    ' blue-init , ' blue-step ,
    ' magenta-init , ' magenta-step ,
    ' violet-init , ' violet-step ,

0 variable program-index
0 variable program      \ xt to current stepper

: next-program ( -- )
    program-index @ 1+  \ increment program-index
    #programs mod
    dup program-index !
    4 * programs + 2@   \ get init and program
    execute             \ execute initialization
    program ! ;         \ store program stepper

: looper ( -- )
    init-spi
    0 program-index !
    programs 2@ execute \ initialize first program
    program !
    begin
        begin
            program @ execute \ execute first program
            flush
            delay @ ms
        key? button? or until \ wait for button or keypress
        next-program
    key? until ;

looper
