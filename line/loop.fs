\  cold

compiletoflash
0 variable step             \ current program step (two byte values in one word-variable)
step 1+ constant substep     \ substep for intermix

: step. step c@ u.2 space substep c@ u.4 space buffer.. ;

: green-init
    #leds 1- 2/ step c!  0 substep c! \ start at stripcenter -1
    $00.1F.00 buffer!                \ dim lower half
    #leds #leds 2/ do                 \ full brightness for upper half
        $00.8F.00 i rgb-px!
    loop flush ;

: green-step
     1 step c@ led-addr c+! \ increment step led
    -1 step c@ #leds/2 led+addr c+! \ decrement end of bright half
    substep c@ 1+ dup $7F > if
        cr ." NEW STEP!" step c@ .
        step c@ 1- dup 0< if #leds 1- then step c!
        step c@ . cr
        drop 0
    then substep c!
    ;
    \ step c@ -1 led+buffer c+!

: green-loop
    init-spi
    green-init
    begin
        green-step
        flush
        \ step. cr
        100 ms
    key? until ;

green-loop
