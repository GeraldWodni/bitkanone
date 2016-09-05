\ Animations
\ (c)copyright 2016 by Gerald Wodni <gerald.wodni@gmail.com> 

\  cold

compiletoflash
25 variable delay          \ ms between program-steps
0 variable hue-start      \ starting hue-angle for hsv-hue-step
0 variable angle           \ currently hue-angle ( if needed )
0 variable step             \ current program step (two byte values in one word-variable)
step 1+ constant substep     \ substep for intermix

: step. step c@ u.2 space substep c@ u.4 space buffer.. ;

: hue-set ( hue -- )
    $FF $FF hsv>rgb buffer! ;

: hsv-value-init ( hue -- )
    >hue dup angle !
    #leds 1- 2/ step c!  0 substep c!   \ start at stripcenter -1
    dup $FF $7F hsv>rgb buffer!         \ dim lower half
    #leds #leds 2/ do                   \ full brightness for upper half
        dup $FF $FF hsv>rgb i rgb-px!
    loop drop flush ;

: next-substep ( -- )
    substep c@ 1+ dup $7F > if          \ at $80 increment step and reset substep
        step c@ 1- dup 0< if drop #leds 1- then step c!
        drop 0
    then substep c!  ;

: hsv-value-step ( -- )
    angle @
    \ decrement end of bright half
    dup $FF                 \ hue sat
    $FF substep c@ -        \ value
    hsv>rgb 
    step c@ #leds/2 led+    \ index
    rgb-px!

    \ increment step led
    $FF                     \ hue sat
    substep c@ $7F +        \ value 
    hsv>rgb step c@ rgb-px!
    next-substep ;

: seq>hue ( n-seq -- n-hue )
    hue-sector 2* mod          \ allow offsets, but clamp them
    dup hue-sector > if       \ 2nd sequence is in reverse
        hue-sector 2* swap -
    then
    hue-start @ + hue-max mod ;           \ hue-start-offset

\ hue color init words
\ primrary colors: RGB, secondary colors: YCM,
\ terciary colors: orange, lime, teal, violet
: _red        0 hsv-value-init ;
: _orange    30 hsv-value-init ;
: _yellow    60 hsv-value-init ;
: _lime      90 hsv-value-init ;
: _green    120 hsv-value-init ;
: _teal     150 hsv-value-init ;
: _cyan     180 hsv-value-init ;
: _blue     240 hsv-value-init ;
: _violet   270 hsv-value-init ;
: _magenta  300 hsv-value-init ;

: _lava       0 >hue hue-start ! ;
: _neon     240 >hue hue-start ! ;
: _waves    180 >hue hue-start ! ;
: _sunset   330 >hue hue-start ! ;

: _white $FF.FF.FF buffer! 1 delay ! ;

: rainbow-step
    angle @ 
    hue-max #leds /         \ offset per led
    #leds 0 do
        2dup i * +      \ get led-hue
        hue-max mod         \ limit
        255 255 hsv>rgb i rgb-px!   \ convert and store
    loop drop 1+ hue-max mod angle ! ;  \ increment hue angle

: hsv-hue-step ( -- )
    angle @
    hue-sector #leds 1 max /
    #leds 0 do
        2dup i * +
        seq>hue
        255 255 hsv>rgb i rgb-px!   \ convert and store
    loop drop 1+ hue-max mod angle ! ;  \ increment hue angle

\ TODO: fix thunder-init, implement thunder-step with white flashes
: thunder-init ( -- )
    _blue
    $00.00.00 buffer!         \ turn lower half off
    #leds #leds 2/ do         \ half brightness for upper half
        $00.00.7F i rgb-px!
    loop ;

: disco-step
    \ perform only after n loops
    substep c@ 1+ dup $10 = if
        angle @ 251 >hue + hue-max mod dup
        hue-set
        angle !
        drop 0
    then substep c! ;

\ program count and xts
17 constant #programs
create programs
    ' noop ,        ' rainbow-step   ,
    \ ' thunder-init ,  ' hsv-value-step ,
    ' _lava ,       ' hsv-hue-step   ,
    ' _neon ,       ' hsv-hue-step   ,
    ' _waves ,      ' hsv-hue-step   ,
    ' _sunset ,     ' hsv-hue-step   ,
    ' _red ,        ' disco-step     ,
    ' _white ,      ' noop           ,
    ' _red ,        ' hsv-value-step ,
    ' _orange ,     ' hsv-value-step ,
    ' _yellow ,     ' hsv-value-step ,
    ' _lime ,       ' hsv-value-step ,
    ' _green ,      ' hsv-value-step ,
    ' _teal ,       ' hsv-value-step ,
    ' _cyan ,       ' hsv-value-step ,
    ' _blue ,       ' hsv-value-step ,
    ' _violet ,     ' hsv-value-step ,
    ' _magenta ,    ' hsv-value-step ,

0 variable program-index
0 variable program      \ xt to current stepper

: next-program ( -- )
    \ TODO: linear interpolation between two hue steps?
    25 delay !
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

: flush-keys begin key? while key drop repeat ;
: init init ." Servus!" flush-keys looper ;
