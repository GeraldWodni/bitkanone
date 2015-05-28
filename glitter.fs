\ Displays a glitter animation
\ (c) copyright 2015 by Gerald Wodni <gerald.wodni@gmail.com>

$FFAF3F variable glitter-color

: decay-glitter ( -- )
    rows 0 do
        cols 0 do
            i j xy@
            dup $FF0000 and if $010000 - then   \ subtract red   ( if any )
            dup $00FF00 and if $000100 - then   \ subtract green ( if any )
            dup $0000FF and if $000001 - then   \ subtract blue  ( if any )
            i j xy!
        loop
    loop ;

: glitter-step
        \ create new point
        glitter-color @ random-xy xy!

        \ decay
        1 0 do
            decay-glitter
            10 us
            flush
        loop ;

: glitter
    buffer-off
    begin
        glitter-step
        key?
    until key drop off ;
